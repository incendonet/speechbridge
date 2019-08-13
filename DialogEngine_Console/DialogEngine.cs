// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using DialogModel;
using Incendonet.Utilities.LogClient;
using ISMessaging;
using ISMessaging.SpeechRec;
using SBConfigStor;
using XmlDocParser;

namespace DialogEngine
{
	public enum ScriptLangType
	{
		eUnknown = 0,
		eVoiceXml,
		eXPlusV,
		eSALT,
		eAngelXML,
		eCustom,
	}

	public enum eParseError
	{
		eSuccess = 0,
		eUnknown,
		eInvalidScriptType,
		eUnsupportedScriptType,
		eSyntaxError,
		eException,
	}

	public enum ExperienceLevel
	{
		eBeginner,
		eIntermediate,
		eAdvanced,
	}

	public enum BoolOperator
	{
		eNone = 0,
		eEqual,
		eGreater,
		eLess,
		eGreaterEqual,
		eLessEqual,
		eNotEqual,
	}

	/// <summary>
	/// 
	/// </summary>
	public class DialogEngine
	{
		private ISMessaging.Delivery.ISMReceiverImpl	m_mRcv;

		private const string				m_csCustomComponentName = "VoiceXML";		// FIX - not the best name
		private const string				m_csInvalidLogString = "ERROR:  An invalid object was passed to the logger.";

		private string						m_sSessionId = "";		// Not to be confused with the DSession, this is the session-id passed over from AudioMgr in the ISMSessionBegin, and should be set in all messages passed back to it.

		private string						m_sUrlStart = "";		// FIX - Do these two URLs belong here or in DSession?
		private string						m_sUrlCurrent = "";
		private string						m_sXmlCurrent = "";
		private string						m_sFromUser = "", m_sFromAddress = "", m_sFromPort = "", m_sFromDisplayName = "";
		private string						m_sToUser = "", m_sToAddress = "", m_sToPort = "";
		private ScriptLangType				m_ScriptType;
		private IInteractionDocParser		m_ScriptParser;
		private	VoiceXmlParser				m_VXMLParser;
		private	XElement					m_xeDialog;
		protected ILegacyLogger				m_Logger = null;
		private bool						m_bPendingDisconnect = false;
		private int							m_iRecognitionCutoffPercentage = 50;

		public	DSession					m_Session;
		public	DDocument					m_Doc;

		ISMessaging.ISMVMC					m_VMC;
		ISMessaging.ISAppEndpoint			m_EPSrc;

		public DialogEngine(ILegacyLogger i_Logger, int i_iVMC)
		{
			m_Logger = i_Logger;
			m_Session = new DSession(i_iVMC);
			m_ScriptParser = m_VXMLParser = null;
			m_xeDialog = new XElement();
			m_Doc = new DDocument(m_Session);

			m_sUrlStart = ConfigurationManager.AppSettings["UrlStart"];
			m_sUrlStart = (m_sUrlStart == null) ? "" : m_sUrlStart;
			if(m_sUrlStart.Length <= 0)
			{
				m_sUrlStart = "http://localhost/VoiceDocStore/AAMain.vxml.xml";
			}

			m_VMC = new ISMVMC();
			m_VMC.Init(i_iVMC, "DialogEngine", "");	// FIX - Properly set VMC info.
			m_EPSrc = new ISAppEndpoint();
			m_EPSrc.Init(m_VMC, Environment.MachineName, EApplication.eDialogMgr, "Dialog Engine");
		}

		public bool Init()
		{
			bool		bRet = true, bRes = false;

			try
			{
				// Do Remoting related init.
				while(!bRes)
				{
					bRes = RemotingInit();
					if(!bRes)
					{
						Thread.Sleep(5000);
					}
				}
				////LoadPage(m_sUrlStart, "", false);	// Don't want to send prompts yet, audio session probably isn't connected.
				//LoadPage(m_sUrlStart, "");	// No need to LoadPage here, it is done in ProcessSessionBegin.
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("DialogEngine.DialogEngine.Init() exception '{0}'.", e);
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.Init exception: " + e.ToString());
			}

			return(bRet);
		}

		public bool RemotingInit()
		{
			bool		bRet = true;
			Type		tRcv;

			try
			{
				//tRcv = Type.GetType("DialogMgr_Console.DMPinger");	// Kind of a kludge to get a valid type to use in GetObject().
				tRcv = typeof(DialogMgr_Console.DMPinger);
				if(tRcv == null)
				{
					bRet = false;
					//Console.Error.WriteLine("ERROR DialogEngine.RemotingInit:  Couldn't get type.");
					m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.RemotingInit:  Couldn't get type.");
				}
				else
				{
					m_mRcv = (ISMessaging.Delivery.ISMReceiverImpl)(Activator.GetObject(tRcv, "tcp://localhost:1779/AudioEngine.rem"));
					if(m_mRcv == null)
					{
						bRet = false;
						//Console.Error.WriteLine("ERROR DialogEngine.RemotingInit:  Couldn't create DialogMgr remote object.");
						m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.RemotingInit:  Couldn't create DialogMgr remote object.");
					}
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR DialogEngine.RemotingInit:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.RemotingInit exception: " + e.ToString());
			}

			return(bRet);
		} // RemotingInit

		/// <summary>
		/// Saves the input log statement to the default log, using the scope of the active-field to evaluate variables for replacement.
		/// </summary>
		/// <param name="i_SubdocContext"></param>
		/// <param name="i_daStatement"></param>
//		private void Log(DField i_dfActiveField, DAction i_daStatement)
		private void Log(ISubdocContext i_SubdocContext, DAction i_daStatement)
		{
			string				sTmp = "";

			if ((i_daStatement.m_oValue != null) && (i_daStatement.m_oValue is string))
			{
				sTmp = (string)i_daStatement.m_oValue;
				sTmp = ReplaceAllVarsInPlace(i_SubdocContext, sTmp);		// Replaces the variable names with their values
			}
			else
			{
				sTmp = m_csInvalidLogString;
			}
			m_Logger.Log(LoggerCore.eSeverity.Info, m_VMC.m_iKey, m_csCustomComponentName, sTmp.ToString());
		} // Log

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_SubdocContext"></param>
		/// <param name="i_sName"></param>
		/// <returns></returns>
		public static DVariable FindVariableByName(ISubdocContext i_SubdocContext, string i_sName)
		{
			DVariable		dvTmp = null;

			if(i_SubdocContext.GetType() == typeof(DField))
			{
				dvTmp = FindVariableByName((DField)i_SubdocContext, i_sName);
			}
			else if (i_SubdocContext.GetType() == typeof(DForm))
			{
				dvTmp = FindVariableByName((DForm)i_SubdocContext, i_sName);
			}
			else
			{
				dvTmp = null;
			}

			return (dvTmp);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sName"></param>
		/// <returns></returns>
		public static DVariable FindVariableByName(DField i_dfField, string i_sName)
		{
			DVariable		dvTmp = null;
			string			sTmp;

			// Lookup variable name
			// FIX - Use sVarFullName to lookup fully qualified names!!!
			if(i_sName.IndexOf("session.") >= 0)
			{
				sTmp = i_sName.Substring(8);
				dvTmp = i_dfField.m_dfParentForm.m_ddParentDocument.m_dsParentSession.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf("application.") >= 0)
			{
				// FIX - Keep it in session since we don't currently have an "application" in USCs?
				sTmp = i_sName.Substring(12);
				dvTmp = i_dfField.m_dfParentForm.m_ddParentDocument.m_dsParentSession.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf("document.") >= 0)
			{
				sTmp = i_sName.Substring(9);
				dvTmp = i_dfField.m_dfParentForm.m_ddParentDocument.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf("dialog.") >= 0)
			{
				sTmp = i_sName.Substring(7);
				dvTmp = i_dfField.m_dfParentForm.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf(i_dfField.ID + "$.") >= 0)
			{
				sTmp = i_sName.Substring(i_dfField.ID.Length + 2);
				dvTmp = i_dfField.FindDVariable(sTmp);
			}
			else
			{	// Not fully qualified name, so go up scope tree.
				dvTmp = i_dfField.FindDVariable(i_sName);
				if(dvTmp == null)
				{
					dvTmp = i_dfField.m_dfParentForm.FindDVariable(i_sName);
					if(dvTmp == null)
					{
						dvTmp = i_dfField.m_dfParentForm.m_ddParentDocument.FindDVariable(i_sName);
						if(dvTmp == null)
						{
							dvTmp = i_dfField.m_dfParentForm.m_ddParentDocument.m_dsParentSession.FindDVariable(i_sName);
						}
					}
				}
			}

			return(dvTmp);
		} // FindVariableByName

		public static DVariable FindVariableByName(DForm i_dfForm, string i_sName)
		{
			DVariable		dvTmp = null;
			string			sTmp;

			// Lookup variable name
			// FIX - Use sVarFullName to lookup fully qualified names!!!
			if(i_sName.IndexOf("session.") >= 0)
			{
				sTmp = i_sName.Substring(8);
				dvTmp = i_dfForm.m_ddParentDocument.m_dsParentSession.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf("application.") >= 0)
			{
				// FIX - Keep it in session since we don't currently have an "application" in USCs?
				sTmp = i_sName.Substring(12);
				dvTmp = i_dfForm.m_ddParentDocument.m_dsParentSession.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf("document.") >= 0)
			{
				sTmp = i_sName.Substring(9);
				dvTmp = i_dfForm.m_ddParentDocument.FindDVariable(sTmp);
			}
			else if(i_sName.IndexOf("dialog.") >= 0)
			{
				sTmp = i_sName.Substring(7);
				dvTmp = i_dfForm.FindDVariable(sTmp);
			}
			else
			{	// Not fully qualified name, so go up scope tree.
				dvTmp = i_dfForm.FindDVariable(i_sName);
				if(dvTmp == null)
				{
					dvTmp = i_dfForm.m_ddParentDocument.FindDVariable(i_sName);
					if(dvTmp == null)
					{
						dvTmp = i_dfForm.m_ddParentDocument.m_dsParentSession.FindDVariable(i_sName);
					}
				}
			}

			return(dvTmp);
		} // FindVariableByName

		/// <summary>
		/// Returns the index in the input string of the rightmost variable in the string, passing its name out in o_sVarname.
		/// If the input string contains no variable then an empty string is returned for the variable name and an index of -1.
		/// NOTE: The index indicates the start of the scoped variable name while the variable name returned does not include the scope.
		/// </summary>
		/// <param name="i_sInput"></param>
		/// <param name="o_sVarname">Name of rightmost variable found.  Empty string if no variable found.</param>
		/// <returns>Index of start of variable name in input string.  -1 if no variable name is found.</returns>
		public int FindLastVariablenameInString(string i_sInput, out string o_sVarname)
		{
			int				iRet = -1;
			string			sVarFullName = "";
			int				iVarStart = 0, iVarEnd = -1, ii = 0, jj = 0;

			o_sVarname = "";

			try
			{
				iVarStart = i_sInput.LastIndexOf(VoiceXmlParser.s_sVariableTag);
				if(iVarStart < 0)
				{
					o_sVarname = "";
					iRet = -1;
				}
				else
				{
					iVarStart++;
					iRet = iVarStart;
					for(ii = iVarStart; ( (ii < i_sInput.Length) && (iVarEnd == -1) ); ii++)
					{
						for(jj = 0; ( (jj < VoiceXmlParser.s_acBreakNonVar.Length) && (iVarEnd == -1) ); jj++)
						{
							if(i_sInput[ii] == VoiceXmlParser.s_acBreakNonVar[jj])
							{
								iVarEnd = ii;
							}
						}
					}
					if(iVarEnd == -1)
					{
						if(ii == i_sInput.Length)			// We've reached the end
						{
							if( (i_sInput[ii - 1] == '.') || (i_sInput[ii - 1] == '$') )	// FIX - Won't catch strings ending in "$." or ".$"
							{
								iVarEnd = ii - 2;
							}
							else
							{
								iVarEnd = ii - 1;
							}
						}
						else
						{
							iVarEnd = ii;
						}
					}
					else
					{
						iVarEnd--;
					}

					sVarFullName = i_sInput.Substring(iVarStart, (iVarEnd - iVarStart + 1));
					iVarStart = sVarFullName.LastIndexOf('.');		// Only want the varname, not the full name (?)
					if(iVarStart < 0)
					{
						o_sVarname = sVarFullName;
					}
					else
					{
						o_sVarname = sVarFullName.Substring(iVarStart + 1);
					}
				}
			}
			catch(Exception exc)
			{
				iRet = -1;
				m_Logger.Log(Level.Exception, string.Format("[{0}]DialogEngine.FindLastVariablenameInString exception: {1}.", m_VMC.m_iKey.ToString(), exc.ToString()));
			}

			return(iRet);
		} // FindLastVariablenameInString

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_SubdocContext"></param>
		/// <param name="i_sVariablename"></param>
		/// <returns></returns>
		public string ReplaceVariablenameWithValue(ISubdocContext i_SubdocContext, string i_sVariablename)
		{
			string sRet = "";

			if (i_SubdocContext.GetType() == typeof(DField))
			{
				sRet = ReplaceVariablenameWithValue((DField)i_SubdocContext, i_sVariablename);
			}
			else if (i_SubdocContext.GetType() == typeof(DForm))
			{
				sRet = ReplaceVariablenameWithValue((DForm)i_SubdocContext, i_sVariablename);
			}
			else
			{
				sRet = "";
                m_Logger.Log(Level.Exception, string.Format("[{0}]DialogEngine.ReplaceVariablenameWithValue unknown type '{1}' ('{2}').", m_VMC.m_iKey, i_SubdocContext.GetType(), i_sVariablename));
			}

			return sRet;
		}

		/// <summary>
		/// Returns the string contained within the variable named by i_sVariablename if found.  If not
		/// found, return the original string.
		/// </summary>
		/// <param name="i_dfField"></param>
		/// <param name="i_sVariablename"></param>
		/// <returns></returns>
		private string ReplaceVariablenameWithValue(DField i_dfField, string i_sVariablename)
		{
			string			sRet = "", sVarName = "";
			int				iVarStart = -1;
			DVariable		dvTmp = null;

			try
			{
				iVarStart = FindLastVariablenameInString(i_sVariablename, out sVarName);
				if(sVarName == "")
				{
					// If no variable found, just return the input string.
					sRet = i_sVariablename;
				}
				else
				{
					dvTmp = FindVariableByName(i_dfField, sVarName);
					if(dvTmp != null)
					{
						sRet = dvTmp.SValue;
					}
					else
					{
						m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ReplaceVariablenameWithValue-field - No variable named '" + sVarName + "' could be found in scope.");
						sRet = i_sVariablename;	// Is this correct?
					}
				}
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ReplaceVariablenameWithValue-field exception: " + exc.ToString());
			}

			return(sRet);
		} // ReplaceVariableWithValue

		/// <summary>
		/// Returns the string contained within the variable named by i_sVariablename if found.  If not
		/// found, return the original string.
		/// </summary>
		/// <param name="i_dfForm"></param>
		/// <param name="i_sVariablename"></param>
		/// <returns></returns>
		private string ReplaceVariablenameWithValue(DForm i_dfForm, string i_sVariablename)
		{
			string			sRet = "", sVarName = "";
			int				iVarStart = 0;
			DVariable		dvTmp = null;

			try
			{
				iVarStart = FindLastVariablenameInString(i_sVariablename, out sVarName);
				if(sVarName == "")
				{
					// If no variable found, just return the input string.
					sRet = i_sVariablename;
				}
				else
				{
					dvTmp = FindVariableByName(i_dfForm, sVarName);
					if(dvTmp != null)
					{
						sRet = dvTmp.SValue;
					}
					else
					{
						m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ReplaceVariablenameWithValue-form - No variable named '" + sVarName + "' could be found in scope.");
						sRet = i_sVariablename;	// Is this correct?
					}
				}
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ReplaceVariablenameWithValue-form exception: " + exc.ToString());
			}

			return(sRet);
		} // ReplaceVariableWithValue

		/// <summary>
		/// Returns a string where all of the variables from the input string have been replaced with their values.
		/// </summary>
		/// <param name="i_SubdocContext"></param>
		/// <param name="i_sExpr"></param>
		/// <returns></returns>
//		public string ReplaceAllVarsInPlace(DField i_dfField, string i_sExpr)
		public string ReplaceAllVarsInPlace(ISubdocContext i_SubdocContext, string i_sExpr)
		{
			string			sRet = "", sRes = "", sVarname = "", sTmp = "";
			int				ii = 0, iRes = -1, iNumVars = 0, iLenOrig = 0, iLenSubbed = 0;

			try
			{
				sRet = i_sExpr;

				// Get number of variables in the string
				iLenOrig = i_sExpr.Length;
				iLenSubbed = i_sExpr.Replace(VoiceXmlParser.s_sVariableTag, "").Length;
				iNumVars = (iLenOrig - iLenSubbed) / VoiceXmlParser.s_sVariableTag.Length;

				// Extract all the strings
				for(ii = 0; ii < iNumVars; ii++)
				{
					iRes = FindLastVariablenameInString(sRet, out sVarname);
					if (iRes == -1)
					{
						m_Logger.Log(Level.Exception, string.Format("DialogEngine.ReplaceAllVarsInPlace: Invalid variable name passed in slot {0}.", (iNumVars - ii - 1)));
					}
					else
					{
						// Try the form variables first
						sTmp = VoiceXmlParser.s_sVariableTag + sVarname;

						if (i_SubdocContext.GetType() == typeof(DField))
						{
							sRes = ReplaceVariablenameWithValue(((DField)i_SubdocContext).m_dfParentForm, sTmp);

							// If the result of the form-replace is the same as its input, check the field
							if (sRes == sTmp)
							{
								sRes = ReplaceVariablenameWithValue((DField)i_SubdocContext, sTmp);
								if (sRes == sTmp)
								{
									m_Logger.Log(Level.Warning, string.Format("DialogEngine.ReplaceAllVarsInPlace: Var '{0}' not found. (1)", sTmp));
								}
							}
						}
						else if(i_SubdocContext.GetType() == typeof(DForm))
						{
							sRes = ReplaceVariablenameWithValue((DForm)i_SubdocContext, sTmp);
							if (sRes == sTmp)
							{
								m_Logger.Log(Level.Warning, string.Format("DialogEngine.ReplaceAllVarsInPlace: Var '{0}' not found. (2)", sTmp));
							}
						}
						else
						{
							m_Logger.Log(Level.Warning, string.Format("DialogEngine.ReplaceAllVarsInPlace: Invalid type passed in: '{0}'.", i_SubdocContext.GetType().ToString()));
						}

						// Replace the variable name with the result in the return string
						if(sRes != sTmp)
						{
							sRet = sRet.Replace(sTmp, sRes);
						}
					}
				}
			}
			catch (Exception exc)
			{
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ReplaceAllVarsInPlace exception: " + exc.ToString());
			}

			return(sRet);
		}

		//public bool LoadPage(string i_sPageName, string i_sFormName, bool i_bIssuePrompts)
		public bool LoadPage(string i_sPageName, string i_sFormName)
		{
			bool		bRet = true;
			bool		bFound = false;
			int			ii;
			eParseError	peErr = eParseError.eSuccess;

			try
			{
				// Only load and parse if URL is different that the current
				if(i_sPageName != m_sUrlCurrent)
				{
					// Load the voice script document
					LoadFromUrl(i_sPageName);

					// Parse it
					peErr = ParseTree();
				}

				if(peErr != eParseError.eSuccess)
				{
					bRet = false;
				}
				else
				{
					// Set default form and field
					if(i_sFormName.Length > 0)
					{
						for(ii = 0; ( (ii < m_Doc.m_DForms.Count) && (!bFound) ); ii++)
						{
							if(m_Doc.m_DForms[ii].m_sName == i_sFormName)
							{
								bRet = bFound = true;
								m_Doc.m_iActiveForm = ii;
								m_Doc.m_DForms[ii].m_iActiveField = 0;		// Reset active field, since we are (most likely) reloading a form.
							}
						}
					}

					// Interpret results from parsing.
                    SetProperties();
					bRet = ASRLoad();

					/*
					// Issue prompts
					if(i_bIssuePrompts)
					{
						IssueFieldPrompts();
					}
					*/
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR DialogEngine.LoadPage():  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.LoadPage exception: " + e.ToString());
			}

			return(bRet);
		} // LoadPage

		/// <summary>
		/// Using the current voice script doc info, tell the ASR what to do (grammar/list to load, etc.)
		/// </summary>
		/// <returns></returns>
		public bool ASRLoad()
		{
			bool			bRet = true, bRes = false;
			DForm			formActive;
			DField			fieldActive;
			int				ii, iNumOptions = 0, iNumOptionsSpeakable = 0, iSleep = 10000;
			StringBuilder	sbTmp = new StringBuilder();
			string			sTmp = "";

			ISMessaging.SpeechRec.ISMLoadGrammarFile	mLoadGrammarFile = null;
			ISMessaging.SpeechRec.ISMLoadPhrases		mLoadPhrases = null;

			try
			{
				// Gather grammar/list info and format message for ASR.
				formActive = m_Doc.m_DForms[m_Doc.m_iActiveForm];
				fieldActive = formActive.m_DFields[formActive.m_iActiveField];

				if(fieldActive.m_DGrammar != null)
				{
					// Load the grammar
					mLoadGrammarFile = new ISMLoadGrammarFile(m_EPSrc, fieldActive.m_DGrammar.Grammar);
					mLoadGrammarFile.m_Dest = new ISMessaging.ISAppEndpoint();
					mLoadGrammarFile.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");
				}
				else
				{
					iNumOptions = fieldActive.m_Options.Count;
					if(iNumOptions <= 0)
					{
						bRet = false;
						//Console.Error.WriteLine("ERROR:  No grammar or options to send to ASR!");
						m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "No grammar or options to send to ASR!");
					}
					else
					{
						// If DTMF or other un-speakable options are in the options, don't send them to the ASR.
						iNumOptionsSpeakable = iNumOptions;
						for(ii = 0; ii < iNumOptions; ii++)
						{
							sTmp = ((DResponse)(fieldActive.m_Options[ii])).m_sValue;
							if(sTmp == DField.eInputModes.dtmf.ToString())
							{
								iNumOptionsSpeakable--;
							}
						}

						mLoadPhrases = new ISMLoadPhrases(m_EPSrc);
						mLoadPhrases.m_sLangCode = m_Doc.m_sLang;
						mLoadPhrases.m_asPhrases = new string[iNumOptionsSpeakable];
						mLoadPhrases.m_Dest = new ISMessaging.ISAppEndpoint();
						mLoadPhrases.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

						for(ii = 0; ii < iNumOptions; ii++)
						{
							sTmp = ((DResponse)(fieldActive.m_Options[ii])).m_sValue;
							if(sTmp != DField.eInputModes.dtmf.ToString())
							{
								mLoadPhrases.m_asPhrases[ii] = sTmp;
							}
						}
					}
				}

				// Send message to ASR.
				if(mLoadGrammarFile != null)
				{
					// Send the grammar
					// FIX - Code cut and pasted from below!!!
					bRes = false;

					while(bRes == false)
					{
						try
						{
							mLoadGrammarFile.m_sSessionId = m_sSessionId;
							bRes = m_mRcv.NewMsg(mLoadGrammarFile);
						}
						catch(Exception exc)
						{
							m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ASRLoad exception: " + exc.ToString());
							bRes = false;
						}

						if(bRes == false)
						{
							sbTmp.Length = 0;
							sbTmp.AppendFormat("Error sending load grammar file message, retrying in {0} milliseconds...", iSleep);
							m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + sbTmp.ToString());
							Thread.Sleep(iSleep);
						}
					}

					mLoadGrammarFile = null;
				}
				else if(mLoadPhrases != null)
				{
					// Send the phrases
					bRes = false;

					while(bRes == false)
					{
						try
						{
							mLoadPhrases.m_sSessionId = m_sSessionId;
							bRes = m_mRcv.NewMsg(mLoadPhrases);
						}
						catch(Exception exc)
						{
							m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ASRLoad exception: " + exc.ToString());
							bRes = false;
						}

						if(bRes == false)
						{
							sbTmp.Length = 0;
							sbTmp.AppendFormat("Error sending load phrases message, retrying in {0} milliseconds...", iSleep);
							m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + sbTmp.ToString());
							Thread.Sleep(iSleep);
						}
					}

					mLoadPhrases = null;
				}
				else
				{
					//Console.Error.WriteLine("ERROR:  No grammar or options message to send to ASR!");
					m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "No grammar or options message to send to ASR!");
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR DialogEngine.ASRLoad():  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.AsrLoad exception: " + e.ToString());
			}

			return(bRet);
		} // ASRLoad()

        /// <summary>
        /// Using the currently active VoiceXML, set any property values defined.
        /// </summary>
        /// <returns></returns>
        public void SetProperties()
        {
            try
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();

                DForm formActive = m_Doc.m_DForms[m_Doc.m_iActiveForm];
                DField fieldActive = formActive.m_DFields[formActive.m_iActiveField];


                // Get the most "local" value for each property defined in the currently active VoiceXML.

                foreach (DProperty property in m_Doc.m_dsParentSession.m_DProperties)
                {
                    properties[property.Name] = property.Value;                 // Use Item property instead of Add() method so that if Name already exists in collection then its value is overwritten.
                }

                foreach (DProperty property in m_Doc.m_DProperties)
                {
                    properties[property.Name] = property.Value;                 // Use Item property instead of Add() method so that if Name already exists in collection then its value is overwritten.
                }

                foreach (DProperty property in formActive.Properties)
                {
                    properties[property.Name] = property.Value;                 // Use Item property instead of Add() method so that if Name already exists in collection then its value is overwritten.
                }

                foreach (DProperty property in fieldActive.Properties)
                {
                    properties[property.Name] = property.Value;                 // Use Item property instead of Add() method so that if Name already exists in collection then its value is overwritten.
                }


                // Send properties to AudioMgr.

                foreach (DictionaryEntry de in (IDictionary)properties)
                {
                    ISMessaging.Audio.ISMSetProperty messageSetProperty = new ISMessaging.Audio.ISMSetProperty(m_EPSrc);
                    messageSetProperty.m_Dest = new ISMessaging.ISAppEndpoint();
                    messageSetProperty.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");
                    messageSetProperty.m_sSessionId = m_sSessionId;

                    messageSetProperty.m_sName = (string)de.Key;
                    messageSetProperty.m_sValue = (string)de.Value;

                    m_mRcv.NewMsg(messageSetProperty);
                }

            }
            catch (Exception e)
            {
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.SetProperties exception: {1}", m_VMC.m_iKey, e.ToString()));
            }

            return;
        }

		public void IssueFieldPrompts()
		{
			try
			{
                // FIX - Some of this code is duplicated in IssueFormPrompts() below.

                bool bBargeinEnabled = true;
                bool bHangupAfterPrompts = false;
                bool bStopProcessingActions = false;

                List<ISMessaging.Audio.ISMPlayPrompts.Prompt> promptsList = new List<ISMessaging.Audio.ISMPlayPrompts.Prompt>();
                
                DForm formActive = m_Doc.m_DForms[m_Doc.m_iActiveForm];
				DField fieldActive = formActive.m_DFields[formActive.m_iActiveField];

				//daCurrent = fieldActive.m_DActions[fieldActive.m_iIterationCount];		// FIX - Need a mechanism to update m_iIterationCount.

				// FIX - This plays out all prompts, when we should also be able to rotate prompts.  This would require a change to the object definition.
                foreach (DAction daTmp in fieldActive.ActionsPre)
                {
                    switch (daTmp.m_Type)
                    {
                        case DAction.DActionType.ePlayPrompt:
                            {
                                DPrompt dpTmp = (DPrompt)(daTmp.m_oValue);

                                bBargeinEnabled &= dpTmp.m_bBargeinEnabled;		// If one prompt is bargein-disabled, the batch is.

                                ISMessaging.Audio.ISMPlayPrompts.Prompt prompt = new ISMessaging.Audio.ISMPlayPrompts.Prompt();
                                prompt.m_Type = dpTmp.m_Type;

                                prompt.m_sText = ReplaceVariablenameWithValue(fieldActive, dpTmp.m_sText);
                                prompt.m_sLang = m_Doc.m_sLang;
                                m_Logger.Log(Level.Info, String.Format("[{0}]DialogEngine.IssueFieldPrompts Using language: '{1}.", m_VMC.m_iKey, m_Doc.m_sLang));

                                if (dpTmp.m_oValue != null)
                                {
                                    switch (dpTmp.m_Type)
                                    {
                                        case ISMessaging.Audio.ISMPlayPrompts.PromptType.eEval:
                                            // FIX - assuming this is the path
                                            prompt.m_sPath = ReplaceVariablenameWithValue(fieldActive, VoiceXmlParser.s_sVariableTag + dpTmp.m_oValue.ToString());	// Prepend with s_sVariableTag so that ReplaceVariableWithValue performs the lookup.
                                            prompt.m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eWavFilePath;
                                            break;

                                        // FIX - Handle the other cases!
                                        default:
                                            prompt.m_sPath = dpTmp.m_oValue.ToString();		// FIX - assuming this is the path
                                            break;
                                    }
                                }

                                promptsList.Add(prompt);
                            }
                            break;

                        case DAction.DActionType.eScript:
                            DialogMgr.JavascriptEngine.ScriptExecute(m_Logger, m_VMC.m_iKey.ToString(), fieldActive, daTmp);
                            break;

                        case DAction.DActionType.eCC_HangupAfterPrompts:
                            bHangupAfterPrompts = true;
                            bStopProcessingActions = true;                              // No action listed after this can ever be reached so don't bother even looking at it.
                            break;

                        case DAction.DActionType.eLog:
                            Log(fieldActive, daTmp);
                            break;

                        default:
                            m_Logger.Log(Level.Warning, String.Format("[{0}]DialogEngine.IssueFieldPrompts unsupported type: '{1}'.", m_VMC.m_iKey, daTmp.m_Type.ToString()));
                            break;
                    }

                    if (bStopProcessingActions)
                    {
                        break;
                    }
                }

                if (promptsList.Count != 0)
                {
                    ISMessaging.Audio.ISMPlayPrompts mPrompt = new ISMessaging.Audio.ISMPlayPrompts(m_EPSrc);
                    mPrompt.m_Dest = new ISMessaging.ISAppEndpoint();
                    mPrompt.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

                    mPrompt.m_sSessionId = m_sSessionId;
                    mPrompt.m_bBargeinEnabled = bBargeinEnabled;
                    mPrompt.m_Prompts = promptsList.ToArray();

                    m_mRcv.NewMsg(mPrompt);
                }

				if (bHangupAfterPrompts)
				{
                    ISMessaging.Session.ISMTerminateSessionAfterPrompts mTerm = new ISMessaging.Session.ISMTerminateSessionAfterPrompts(m_EPSrc);
                    mTerm.m_Dest = new ISMessaging.ISAppEndpoint();
                    mTerm.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

                    mTerm.m_sSessionId = m_sSessionId;

					m_mRcv.NewMsg(mTerm);
				}
            }
			catch (Exception e)
			{
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.IssueFieldPrompts exception: {1}", m_VMC.m_iKey, e.ToString()));
			}

			return;
		}

		public bool IssueFormPrompts()
		{
            bool bContinueProcessingOfActions = true;

            try
			{
                // FIX - Some of this code is duplicated in IssueFieldPrompts() above.

                bool bBargeinEnabled = true;
                bool bHangupAfterPrompts = false;
                bool bStopProcessingActions = false;
                string sNextPageName = "";
                string sNextFormName = "";
                bool bProcessGoto = false;

                List<ISMessaging.Audio.ISMPlayPrompts.Prompt> promptsList = new List<ISMessaging.Audio.ISMPlayPrompts.Prompt>();

                DForm formActive = m_Doc.m_DForms[m_Doc.m_iActiveForm];
				DField fieldActive = formActive.m_DFields[formActive.m_iActiveField];


				// FIX - This plays out all prompts, when we should also be able to rotate prompts.  This would require a change to the object definition.
                foreach (DAction daTmp in formActive.ActionsPre)
				{
                    switch (daTmp.m_Type)
                    {
                        case DAction.DActionType.ePlayPrompt:
                            {
                                DPrompt dpTmp = (DPrompt)(daTmp.m_oValue);

                                bBargeinEnabled &= dpTmp.m_bBargeinEnabled;		// If one prompt is bargein-disabled, the batch is.

                                ISMessaging.Audio.ISMPlayPrompts.Prompt prompt = new ISMessaging.Audio.ISMPlayPrompts.Prompt();
                                prompt.m_Type = dpTmp.m_Type;

                                prompt.m_sText = ReplaceVariablenameWithValue(formActive, dpTmp.m_sText);
                                prompt.m_sLang = m_Doc.m_sLang;
                                m_Logger.Log(Level.Info, String.Format("[{0}]DialogEngine.IssueFormPrompts Using language: '{1}'.", m_VMC.m_iKey, m_Doc.m_sLang));

                                if (dpTmp.m_oValue != null)
                                {
                                    switch (dpTmp.m_Type)
                                    {
                                        case ISMessaging.Audio.ISMPlayPrompts.PromptType.eEval:
                                            // FIX - assuming this is the path
                                            prompt.m_sPath = ReplaceVariablenameWithValue(formActive, VoiceXmlParser.s_sVariableTag + dpTmp.m_oValue.ToString());	// Prepend with s_sVariableTag so that ReplaceVariableWithValue performs the lookup.
                                            prompt.m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eWavFilePath;
                                            break;

                                        // FIX - Handle the other cases!
                                        default:
                                            prompt.m_sPath = dpTmp.m_oValue.ToString();		// FIX - assuming this is the path
                                            break;
                                    }
                                }

                                promptsList.Add(prompt);
                            }
                            break;

                        case DAction.DActionType.eScript:
    						DialogMgr.JavascriptEngine.ScriptExecute(m_Logger, m_VMC.m_iKey.ToString(), fieldActive, daTmp);        //$$$ LP - Shouldn't this be formActive instead of fieldActive?
                            break;

                        case DAction.DActionType.eCC_HangupAfterPrompts:
                            bHangupAfterPrompts = true;
                            bStopProcessingActions = true;                              // No action listed after this can ever be reached so don't bother even looking at it.
                            break;

                        case DAction.DActionType.eLog:
                            Log(fieldActive, daTmp);
                            break;

                        case DAction.DActionType.eCondition:
                            ProcessCondition((ISubdocContext)fieldActive, (DConditions)daTmp.m_oValue, ref formActive.m_ddParentDocument.m_sApplication, ref formActive.m_sName);        //$$$ LP - Shouldn't this be formActive instead of fieldActive?
                            break;

                        case DAction.DActionType.eLink:
                        case DAction.DActionType.eLinkExpr:
                            {
                                string sNextUrl = "";

                                if (daTmp.m_Type == DAction.DActionType.eLink)
                                {
                                    sNextUrl = daTmp.m_oValue.ToString();
                                }
                                else
                                {
                                    sNextUrl = ReplaceVariablenameWithValue(formActive, VoiceXmlParser.s_sVariableTag + daTmp.m_oValue.ToString());	// Prepend with s_sVariableTag so that ReplaceVariableWithValue performs the lookup.
                                }

                                GetTargetPageAndForm(sNextUrl, ref sNextPageName, ref sNextFormName);
                                bProcessGoto = true;
                                bStopProcessingActions = true;                              // No action listed after this can ever be reached so don't bother even looking at it.
                            }

                            break;

                        default:
                            m_Logger.Log(Level.Warning, String.Format("[{0}]DialogEngine.IssueFormPrompts unsupported type: '{1}'.", m_VMC.m_iKey, daTmp.m_Type.ToString()));
                            break;
                    }

                    if (bStopProcessingActions)
                    {
                        bContinueProcessingOfActions = false;
                        break;
                    }
                }

                if (promptsList.Count != 0)
                {
                    ISMessaging.Audio.ISMPlayPrompts mPrompt = new ISMessaging.Audio.ISMPlayPrompts(m_EPSrc);
                    mPrompt.m_Dest = new ISMessaging.ISAppEndpoint();
                    mPrompt.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

                    mPrompt.m_sSessionId = m_sSessionId;
                    mPrompt.m_bBargeinEnabled = bBargeinEnabled;
                    mPrompt.m_Prompts = promptsList.ToArray();

                    m_mRcv.NewMsg(mPrompt);
                }

				if (bHangupAfterPrompts)
				{
                    ISMessaging.Session.ISMTerminateSessionAfterPrompts mTerm = new ISMessaging.Session.ISMTerminateSessionAfterPrompts(m_EPSrc);
                    mTerm.m_Dest = new ISMessaging.ISAppEndpoint();
                    mTerm.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

                    mTerm.m_sSessionId = m_sSessionId;

					m_mRcv.NewMsg(mTerm);
				}

                if (bProcessGoto)
                {
                    if (String.IsNullOrEmpty(sNextPageName))
                    {
                        sNextPageName = m_sUrlCurrent;
                    }

                    LoadPage(sNextPageName, sNextFormName);
                    IssueFormAndFieldPrompts();
                }
            }
			catch (Exception e)
			{
                bContinueProcessingOfActions = false;                       // Ran into a problem with this Form so it seems pointless to continue processing the child Field.
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.IssueFormPrompts: {1}", m_VMC.m_iKey, e.ToString()));
			}

            return bContinueProcessingOfActions;
		}

		public void IssueFormAndFieldPrompts()
		{
            bool bContinueProcessingOfActions = IssueFormPrompts();

            if (bContinueProcessingOfActions)
            {
                IssueFieldPrompts();
            }

            return;
		}


		public bool IssueResponsePrompts(ISubdocContext i_SubdocContext, List<DPrompt> i_prompts)
		{
			bool bRet = true;

			try
			{
				ISMessaging.Audio.ISMPlayPrompts mPrompt;

				mPrompt = new ISMessaging.Audio.ISMPlayPrompts(m_EPSrc);
				mPrompt.m_Dest = new ISMessaging.ISAppEndpoint();
				mPrompt.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");
				mPrompt.m_Prompts = new ISMessaging.Audio.ISMPlayPrompts.Prompt[i_prompts.Count];

				int jj = 0;

				foreach (DPrompt prompt in i_prompts)
				{
					mPrompt.m_bBargeinEnabled &= prompt.m_bBargeinEnabled;			// If one prompt is bargein-disabled, the batch is.

					mPrompt.m_Prompts[jj] = new ISMessaging.Audio.ISMPlayPrompts.Prompt();
					mPrompt.m_Prompts[jj].m_Type = prompt.m_Type;

                    mPrompt.m_Prompts[jj].m_sText = ReplaceVariablenameWithValue(i_SubdocContext, prompt.m_sText);
					mPrompt.m_Prompts[jj].m_sLang = m_Doc.m_sLang;
                    m_Logger.Log(Level.Info, String.Format("[{0}]DialogEngine.IssueResponsePrompts Using language: '{1}'.", m_VMC.m_iKey, m_Doc.m_sLang));

					if (prompt.m_oValue != null)
					{
						switch (prompt.m_Type)
						{
							case ISMessaging.Audio.ISMPlayPrompts.PromptType.eEval:
                                mPrompt.m_Prompts[jj].m_sPath = ReplaceVariablenameWithValue(i_SubdocContext, VoiceXmlParser.s_sVariableTag + prompt.m_oValue.ToString());	// Prepend with s_sVariableTag so that ReplaceVariableWithValue performs the lookup.
								mPrompt.m_Prompts[jj].m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eWavFilePath;
								break;

							default:		// FIX - Handle the other cases!
								mPrompt.m_Prompts[jj].m_sPath = prompt.m_oValue.ToString();		// FIX - assuming this is the path
								break;
						}
					}

					++jj;
				}

				mPrompt.m_sSessionId = m_sSessionId;
				m_mRcv.NewMsg(mPrompt);
			}
			catch (Exception exc)
			{
				bRet = false;
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.IssueResponsePrompts: {1}", m_VMC.m_iKey, exc.ToString()));
			}

			return bRet;
		}

		private bool IssueTerminateSessionAfterPrompts()
		{
			bool		bRet = true;
			ISMessaging.Session.ISMTerminateSessionAfterPrompts		mTerm = null;

			try
			{
				mTerm = new ISMessaging.Session.ISMTerminateSessionAfterPrompts(m_EPSrc);
				mTerm.m_Dest = new ISMessaging.ISAppEndpoint();
				mTerm.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

				mTerm.m_sSessionId = m_sSessionId;
				m_mRcv.NewMsg(mTerm);

				m_bPendingDisconnect = true;		// We're expecting the session to disconnect in the near future.
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.IssueTerminateSessionAfterPrompts: " + exc.ToString());
			}

			return(bRet);
		} // IssueTerminateSessionAfterPrompts

		public bool IssueTransferSession(ISubdocContext i_SubdocContext, DAction i_daTransfer)
		{
			bool bRet = true;

			try
			{
				// Notify AudioMgr
                ISMessaging.Session.ISMTransferSession mTrans = new ISMessaging.Session.ISMTransferSession(m_EPSrc);
				mTrans.m_Dest = new ISMessaging.ISAppEndpoint();
				mTrans.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

                mTrans.m_sTransferToAddr = ReplaceVariablenameWithValue(i_SubdocContext, i_daTransfer.m_oValue.ToString());
				m_Logger.Log(Level.Info, String.Format("[{0}]DialogEngine.IssueTransferSession - Call transfer to {1}.", m_VMC.m_iKey, mTrans.m_sTransferToAddr));

				// FIX - Add prompts (if any.)  NOTE - this may have already been taken care of in ProcessSpeechResults() if
				// it already went through the prompts.

				mTrans.m_sSessionId = m_sSessionId;
				m_mRcv.NewMsg(mTrans);

				m_bPendingDisconnect = true;		// We're expecting the session to disconnect in the near future.
			}
			catch (Exception e)
			{
				bRet = false;
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.IssueTransferSession: {1}", m_VMC.m_iKey, e.ToString()));
			}

			return bRet;
		} // IssueTransferSession

        public void IssueIdleMessage()
        {
            try 
            {
                ISMessaging.Session.ISMDialogManagerIdle mIdle = new ISMessaging.Session.ISMDialogManagerIdle(m_EPSrc);
                mIdle.m_Dest = new ISMessaging.ISAppEndpoint();
                mIdle.m_Dest.Init(m_VMC, Environment.MachineName, EApplication.eAudioMgr, "Audio Mgr");

                mIdle.m_sSessionId = m_sSessionId;
                m_mRcv.NewMsg(mIdle);
            }
            catch (Exception e)
            {
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.IssueIdleMessage: {1}", m_VMC.m_iKey, e.ToString()));
            }

            return;
        }
		/// <summary>
		/// Retrieves the XML document from the URL and parses it into XElement representation.
		/// </summary>
		/// <param name="i_sUrl"></param>
		/// <returns></returns>
		public bool LoadFromUrl(string i_sUrl)
		{
			bool	bRet = true, bRes = true;

			try
			{
				m_sUrlCurrent = i_sUrl;

				bRes = XmlParser.GetXmlString(m_Logger, m_sUrlCurrent, out m_sXmlCurrent);
				if(bRes)
				{
					m_xeDialog = new XElement();	// Make sure we're starting with a clean slate.
					bRes = XmlParser.ParseXml(m_Logger, m_sUrlCurrent, m_sXmlCurrent, ref m_xeDialog);
				}

				/**
				Console.WriteLine("\n");
				XmlParser.DisplayNodeTree(m_xeDialog);
				Console.WriteLine("\n");
				**/
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("DialogEngine.DResponse.LoadFromUrl() - Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.LoadFromUrl: " + exc.ToString());
			}

			return(bRet);
		} // LoadFromUrl

		/// <summary>
		/// Takes the XElement representation, determines the scripting language, and converts it
		/// to DElement representation for processing.
		/// </summary>
		/// <returns></returns>
		public eParseError ParseTree()
		{
			eParseError		eRet = eParseError.eSuccess;

			try
			{
				m_ScriptType = DetermineScriptingLang(m_xeDialog);
				switch(m_ScriptType)
				{
					case ScriptLangType.eVoiceXml :
					{
						if(m_VXMLParser == null)
						{
							m_VXMLParser = new VoiceXmlParser(m_Logger);
						}

						m_ScriptParser = m_VXMLParser;
						m_Doc = new DDocument(m_Session);		// Make sure we're working with a clean slate.
						m_ScriptParser.ParseDoc(m_xeDialog, m_Session, ref m_Doc);
					}
					break;
					/*case ScriptLangType.eXPlusV :
						break;
					case ScriptLangType.eSALT :
						break;
					case ScriptLangType.eAngelXML :
						break;
					case ScriptLangType.eCustom :
						break;*/
					default :
					{
						eRet = eParseError.eUnsupportedScriptType;
					}
					break;
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				//Console.Error.WriteLine("ERROR DialogEngine.ParseTree caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ParseTree: " + exc.ToString());
			}

			return(eRet);
		}

		/// <summary>
		/// Determines the script language by examining the namespace attributes in the element.
		/// </summary>
		/// <param name="i_xeRootElem"></param>
		/// <returns></returns>
		public static ScriptLangType DetermineScriptingLang(XElement i_xeElem)
		{
			bool				bElemFound = false, bAttrFound = false;
			int					ii = 0, jj = 0;
			string				sTmp;
			XElement			xeTmp;
			XAttribute			xaTmp;
			ScriptLangType		slType = ScriptLangType.eUnknown;

			// First find root node in the tree, should be "vxml" for VoiceXml, or "html" for X+V or SALT.
			ii = 0;
			while(!bElemFound)
			{
				xeTmp = (XElement)(i_xeElem.m_alElements[ii]);
				sTmp = xeTmp.Name.ToLower();

				if(sTmp == "vxml")
				{
					bElemFound = true;
					slType = ScriptLangType.eVoiceXml;
				}
				else if(sTmp == "html")
				{
					bElemFound = true;
					bAttrFound = false;
					jj = 0;

					while(!bAttrFound)
					{
						xaTmp = (XAttribute)(xeTmp.m_alAttributes[jj]);
						sTmp = xaTmp.Name.ToLower();

						if(sTmp.EndsWith("/xhtml+voice"))
						{
							bAttrFound = true;
							slType = ScriptLangType.eXPlusV;
						}
						else if(sTmp.StartsWith("xmlns:salt"))
						{
							bAttrFound = true;
							slType = ScriptLangType.eSALT;
						}
						else
						{
							jj++;
						}
					}
				}
				else
				{
					ii++;
				}
			}

			return(slType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_adaActions"></param>
		/// <returns></returns>
		public bool ProcessResults(DField i_dfActiveField, DActions i_adaActions)
		{
			bool		bRet = true;
			string		sPageName = "", sFormName = "", sTmp = "";

			try
			{
				bRet = ProcessResultsNoLoad(i_dfActiveField, i_adaActions, ref sPageName, ref sFormName);
				if(!bRet)
				{
					// BDA 9/13/2010 - Added to address issue where the full VoiceXML doc isn't read in.  If the VoiceXML is malformed, this retry will
					// have the same result as the first try.

					// There was an error processing the results, so reload the page to make sure the caller doesn't get stuck, and fresh prompts are played.
					sTmp = m_sUrlCurrent;
					m_sUrlCurrent = "";		// This will force the page to reload

					LoadPage(sTmp, "");

					// Issue prompts.  FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
					IssueFormAndFieldPrompts();
				}

				if(sPageName.Length > 0)
				{
					if(sPageName != m_sUrlCurrent)
					{
						//LoadPage(sPageName, sFormName, true);
						LoadPage(sPageName, sFormName);

						// Issue prompts.  FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
						IssueFormAndFieldPrompts();
					}
					else
					{
						//LoadPage(sPageName, sFormName, false);
						LoadPage(sPageName, sFormName);

						if(sFormName != m_Doc.m_DForms[m_Doc.m_iActiveForm].ID)
						{
							// Issue prompts.  FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
							IssueFormAndFieldPrompts();
						}
						else if(sFormName.Length > 0)
						{
							// Issue prompts.  FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
							IssueFormAndFieldPrompts();
						}
						else
						{
							IssueFieldPrompts();
						}
					}
				}
				else
				{
					// Advance field if there is more than one.
					sPageName = m_sUrlCurrent;
					if(sFormName.Length > 0)
					{
						LoadPage(sPageName, sFormName);

						// Issue prompts.  FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
						IssueFormAndFieldPrompts();
					}
					else if(m_Doc.m_DForms[m_Doc.m_iActiveForm].m_iActiveField < (m_Doc.m_DForms[m_Doc.m_iActiveForm].m_DFields.Count - 1) )
					{
						m_Doc.m_DForms[m_Doc.m_iActiveForm].m_iActiveField++;
						//LoadPage(sPageName, "", true);		// Ever need to use sFormName here?  Any reason not to?
						LoadPage(sPageName, "");

						IssueFieldPrompts();
					}
					else
					{
						////LoadPage(sPageName, sFormName, false);
						//LoadPage(sPageName, "", false);		// Ever need to use sFormName here?  Any reason not to?
						LoadPage(sPageName, "");
					}
				}

				m_Logger.Log(Level.Debug, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ProcessResults: Loaded Page '" + sPageName + "', Form '" + sFormName + "', Field '" + m_Doc.m_DForms[m_Doc.m_iActiveForm].m_DFields[m_Doc.m_DForms[m_Doc.m_iActiveForm].m_iActiveField].ID + "'.");

                IssueIdleMessage();
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ProcessResults: " + exc.ToString());
			}

			return(bRet);
		}

		public bool ProcessResultsNoLoad(ISubdocContext i_SubdocContext, DActions i_adaActions, ref string io_sPageName, ref string io_sFormName)
		{
			bool bRet = true;

			try
			{
				if (i_adaActions.Count <= 0)
				{
					// This indicates that the script was malformed.
					bRet = false;
                    m_Logger.Log(Level.Warning, String.Format("[{0}]DialogEngine.ProcessResultsNoLoad - There were no Actions to process.", m_VMC.m_iKey));
				}
				else
				{
                    bool bStopProcessingActions = false;

					for (int ii = 0; ii < i_adaActions.Count; ii++)
					{
						DAction daTmp = i_adaActions[ii];
						switch (daTmp.m_Type)
						{
							case DAction.DActionType.ePlayPrompt :
							{
								List<DPrompt> prompts = new List<DPrompt>();


								// Pass all contiguous <audio> elements as one play request.

								while ((ii < i_adaActions.Count) && (DAction.DActionType.ePlayPrompt == i_adaActions[ii].m_Type))
								{
									prompts.Add((DPrompt)i_adaActions[ii].m_oValue);
									++ii;
								}

								if ((ii < i_adaActions.Count) && (DAction.DActionType.ePlayPrompt != i_adaActions[ii].m_Type))
								{
									// Gone one too far, so back up so it gets processed the next time through the main loop.

									--ii;
								}

								// Send "prompt" command message back to AudioMgr
								IssueResponsePrompts(i_SubdocContext, prompts);
							}
							break;
							case DAction.DActionType.eCC_TransferSession :
							{
								IssueTransferSession(i_SubdocContext, daTmp);
                                bStopProcessingActions = true;                              // No action listed after this can ever be reached so don't bother even looking at it.
							}
							break;
							case DAction.DActionType.eScript :
							{
								DialogMgr.JavascriptEngine.ScriptExecute(m_Logger, m_VMC.m_iKey.ToString(), i_SubdocContext, daTmp);
							}
							break;
							case DAction.DActionType.eLink :
							case DAction.DActionType.eLinkExpr :
							{
                                string sNextUrl = "";

                                if (daTmp.m_Type == DAction.DActionType.eLink)
                                {
                                    sNextUrl = daTmp.m_oValue.ToString();
                                }
                                else
                                {
                                    sNextUrl = ReplaceVariablenameWithValue(i_SubdocContext, VoiceXmlParser.s_sVariableTag + daTmp.m_oValue.ToString());
                                }

                                GetTargetPageAndForm(sNextUrl, ref io_sPageName, ref io_sFormName);
                                bStopProcessingActions = true;                              // No action listed after this can ever be reached so don't bother even looking at it.
                            }
							break;
							case DAction.DActionType.eCondition :
							{
								ProcessCondition(i_SubdocContext, (DConditions)daTmp.m_oValue, ref io_sPageName, ref io_sFormName);
							}
							break;
							case DAction.DActionType.eCC_HangupAfterPrompts :
							{
								IssueTerminateSessionAfterPrompts();
                                bStopProcessingActions = true;                              // No action listed after this can ever be reached so don't bother even looking at it.
                            }
							break;
							case DAction.DActionType.eLog:
							{
								Log(i_SubdocContext, daTmp);
							}
							break;
							default :
							{
                                m_Logger.Log(Level.Exception, String.Format("[{0}]Unsupported type '{1}' in DialogEngine.ProcessResultsNoLoad().", m_VMC.m_iKey, daTmp.m_Type.ToString()));
							}
							break;
						} // switch

                        if (bStopProcessingActions)
                        {
                            break;
                        }
					} // for
				} // else
			}
			catch (Exception exc)
			{
				bRet = false;
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ProcessResultsNoLoad: {1}", m_VMC.m_iKey, exc.ToString()));
			}

			return bRet;
		} // ProcessResultsNoLoad


        public void GetTargetPageAndForm(string i_sTargetUrl, ref string io_sPageName, ref string io_sFormName)
        {
            int iIndex = i_sTargetUrl.IndexOf('#');
            if (iIndex >= 0)
            {
                io_sPageName = i_sTargetUrl.Substring(0, iIndex);

                if (iIndex < (i_sTargetUrl.Length - 1))
                {
                    io_sFormName = i_sTargetUrl.Substring(iIndex + 1);
                }
            }
            else
            {
                io_sPageName = i_sTargetUrl;
            }
        }


		/// <summary>
		/// Determine the boolean operator, and extract the values of the elements before and after the operator.
		/// </summary>
        /// <param name="i_SubdocContext"></param>
		/// <param name="i_sStatement"></param>
		/// <param name="o_sElem1"></param>
		/// <param name="o_sElem2"></param>
		/// <returns></returns>
		protected BoolOperator ExtractConditionValues(ISubdocContext i_SubdocContext, string i_sStatement, out string o_sElem1, out string o_sElem2)
		{
			BoolOperator eRet = BoolOperator.eNone;

			o_sElem1 = o_sElem2 = "";

			try
			{
				o_sElem1 = DialogMgr.JavascriptEngine.ScriptExtractElement(m_Logger, m_VMC.m_iKey.ToString(), i_sStatement);
				if (String.IsNullOrEmpty(o_sElem1))
				{
					eRet = BoolOperator.eNone;
                    m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ExtractConditionElements - Invalid boolean expression '{1}' (missing first argument).", m_VMC.m_iKey, i_sStatement));
                }
				else
				{
                    int iIndexStop = 0;

					iIndexStop = i_sStatement.IndexOf(DialogMgr.JavascriptEngine.m_csBoolEqual);
					if (iIndexStop > 0)
					{
						eRet = BoolOperator.eEqual;
						iIndexStop += DialogMgr.JavascriptEngine.m_csBoolEqual.Length;
					}
					else
					{
						iIndexStop = i_sStatement.IndexOf(DialogMgr.JavascriptEngine.m_csBoolNotEqual);
						if (iIndexStop > 0)
						{
							eRet = BoolOperator.eNotEqual;
							iIndexStop += DialogMgr.JavascriptEngine.m_csBoolNotEqual.Length;
						}
						else
						{
							iIndexStop = i_sStatement.IndexOf(DialogMgr.JavascriptEngine.m_csBoolGreaterEqual);
							if (iIndexStop > 0)
							{
								eRet = BoolOperator.eGreaterEqual;
                                iIndexStop += DialogMgr.JavascriptEngine.m_csBoolGreaterEqual.Length;
							}
							else
							{
								iIndexStop = i_sStatement.IndexOf(DialogMgr.JavascriptEngine.m_csBoolLessEqual);
								if (iIndexStop > 0)
								{
									eRet = BoolOperator.eLessEqual;
                                    iIndexStop += DialogMgr.JavascriptEngine.m_csBoolLessEqual.Length;
								}
								else
								{
									iIndexStop = i_sStatement.IndexOf(DialogMgr.JavascriptEngine.m_csBoolGreater);
									if (iIndexStop > 0)
									{
										eRet = BoolOperator.eGreater;
                                        iIndexStop += DialogMgr.JavascriptEngine.m_csBoolGreater.Length;
									}
									else
									{
										iIndexStop = i_sStatement.IndexOf(DialogMgr.JavascriptEngine.m_csBoolLess);
										if (iIndexStop > 0)
										{
											eRet = BoolOperator.eLess;
                                            iIndexStop += DialogMgr.JavascriptEngine.m_csBoolLess.Length;
										}
										else
										{
											eRet = BoolOperator.eNone;
										}
									}
								}
							}
						}
					}

					if ((eRet == BoolOperator.eNone) || (iIndexStop <= 0))
					{
						eRet = BoolOperator.eNone;
                        m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ExtractConditionElements - Invalid boolean expression '{1}'.", m_VMC.m_iKey, i_sStatement));
                    }
					else
					{
						o_sElem2 = i_sStatement.Substring(iIndexStop).Trim();
						o_sElem2 = DialogMgr.JavascriptEngine.ScriptExtractElement(m_Logger, m_VMC.m_iKey.ToString(), o_sElem2);

						o_sElem1 = DialogMgr.JavascriptEngine.ScriptExtractValue(m_Logger, m_VMC.m_iKey.ToString(), i_SubdocContext, o_sElem1).ToLower();
						o_sElem2 = DialogMgr.JavascriptEngine.ScriptExtractValue(m_Logger, m_VMC.m_iKey.ToString(), i_SubdocContext, o_sElem2).ToLower();
					}
				}
			}
			catch (Exception exc)
			{
				eRet = BoolOperator.eNone;
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ExtractConditionElements: {1}", m_VMC.m_iKey, exc.ToString()));
            }

			return eRet;
		} // ExtractConditionValues

		/// <summary>
		/// Processes the conditions passed in, looking up the values of the operands.
		/// Note: The equality operation (==, !=) is a string compare, the others (>, >=, <, <=) are numeric.
		/// </summary>
        /// <param name="i_SubdocContext"></param>
		/// <param name="i_adcConds"></param>
		/// <param name="io_sPageName"></param>
		/// <param name="io_sFormName"></param>
		/// <returns></returns>
		public void ProcessCondition(ISubdocContext i_SubdocContext, DConditions i_adcConds, ref string io_sPageName, ref string io_sFormName)
		{
			try
			{
                bool bDone = false;

                for (int ii = 0; ((ii < i_adcConds.Count) && (!bDone)); ++ii)
				{
					DCondition daCond = i_adcConds[ii];
					string sStatement = daCond.m_sStatement.Trim();

					if (sStatement.Length > 0)
					{
                        string sArgument1;
                        string sArgument2;

						BoolOperator boolOperator = ExtractConditionValues(i_SubdocContext, sStatement, out sArgument1, out sArgument2);
						if ((boolOperator == BoolOperator.eNone) || (sArgument1 == null) || (sArgument2 == null))
						{
                            m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ProcessCondition('{1}') - Error in ExtractConditionValues.", m_VMC.m_iKey, sStatement));
                        }
						else
						{
                            bool bComparisonIsTrue = false;

                            switch (boolOperator)
                            { 
                                case BoolOperator.eEqual:
                                    bComparisonIsTrue = (sArgument1 == sArgument2);
                                    break;

                                case BoolOperator.eNotEqual:
                                    bComparisonIsTrue = (sArgument1 != sArgument2);
                                    break;

                                default:
                                    // All following boolean operators require their arguments to be integers.

									try
									{
										int iArgument1 = int.Parse(sArgument1);
										int iArgument2 = int.Parse(sArgument2);

                                        switch (boolOperator)
                                        {
                                            case BoolOperator.eGreater:
                                                bComparisonIsTrue = (iArgument1 > iArgument2);
                                                break;

                                            case BoolOperator.eLess:
                                                bComparisonIsTrue = (iArgument1 < iArgument2);
                                                break;

                                            case BoolOperator.eGreaterEqual:
                                                bComparisonIsTrue = (iArgument1 >= iArgument2);
                                                break;

                                            case BoolOperator.eLessEqual:
                                                bComparisonIsTrue = (iArgument1 <= iArgument2);
                                                break;

                                            default:
                                                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ProcessCondition - Undefined boolean operator encountered ({1}).", m_VMC.m_iKey, boolOperator));
                                                break;
                                        }
									}
									catch
									{
                                        m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ProcessCondition - Arguments to comparison operators must be integers ('{1}' {2} '{3}').", m_VMC.m_iKey, sArgument1, boolOperator, sArgument2));
                                    }

                                    break;
							}

							if (bComparisonIsTrue)
							{
								ProcessResultsNoLoad(i_SubdocContext, daCond.m_Actions, ref io_sPageName, ref io_sFormName);
								bDone = true;
							}
							else
							{
								// Keep going
							}
						}
					}
					else
					{	
                        // This is the default condition (i.e. "else", statement is empty), do its actions if we got here.

						ProcessResultsNoLoad(i_SubdocContext, daCond.m_Actions, ref io_sPageName, ref io_sFormName);
						bDone = true;
					}
				}
			}
			catch (Exception exc)
			{
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ProcessCondition exception: {1}", m_VMC.m_iKey, exc.ToString()));
            }

			return;
		} // ProcessCondition

		/// <summary>
		/// Takes the results returned from the ASR
		/// </summary>
		/// <param name="i_mRes"></param>
		/// <returns></returns>
		public bool ProcessSpeechResults(ISMessaging.SpeechRec.ISMResults i_mRes)
		{
			bool			bRet = true, bRes = true;
			DForm			dfActiveForm = null;
			DField			dfActiveField = null;

			try
			{
				// Goto active field
				dfActiveForm = m_Doc.m_DForms[m_Doc.m_iActiveForm];
				dfActiveField = dfActiveForm.m_DFields[dfActiveForm.m_iActiveField];

				FillDefaultVariables_Utterance(i_mRes, dfActiveField);
				bRes = ProcessResults(dfActiveField, dfActiveField.m_DActionsPost);
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ProcessSpeechResults: " + exc.ToString());
			}

			return(bRet);
		} // ProcessSpeechResults()

		/// <summary>
		/// Process the DTMF returned
		/// </summary>
		/// <param name="i_mDtmf"></param>
		/// <returns></returns>
		public bool ProcessDtmfComplete(ISMessaging.Audio.ISMDtmfComplete i_mDtmf)
		{
			bool								bRet = true, bRes = false;
			DForm								dfActiveForm = null;
			DField								dfActiveField = null;

			try
			{
				// Goto active field
				dfActiveForm = m_Doc.m_DForms[m_Doc.m_iActiveForm];
				dfActiveField = dfActiveForm.m_DFields[dfActiveForm.m_iActiveField];

				// Fill in default variables
				FillDefaultVariables_Utterance(i_mDtmf, dfActiveField);
				bRes = ProcessResults(dfActiveField, dfActiveField.m_DActionsPost);
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ProcessDtmfComplete: " + exc.ToString());
			}

			return(bRet);
		} // ProcessDtmfComplete()

		/// <summary>
		/// Fill in default variables.
		/// NOTE - there can be multiple utterances returned, and this is only using highest confidence
		/// scored value.
		/// </summary>
		/// <param name="i_MResults"></param>
		/// <param name="i_dfField"></param>
		private void FillDefaultVariables_Utterance(ISMessaging.SpeechRec.ISMResults i_MResults, DField i_dfField)
		{
			try
			{
				int iProb = m_iRecognitionCutoffPercentage;			// Probability Cutoff - If probability is less than this, we don't have a match.
				string sResult = "";
				string sProb = "0";

				int iNum = i_MResults.m_Results.Length;
				for (int ii = 0; ii < iNum; ++ii)
				{
					ISMessaging.SpeechRec.RecognitionResult rrTmp = i_MResults.m_Results[ii];

					m_Logger.Log(Level.Verbose, String.Format("[{0}]  RESULT ({1} of {2}) = tag:'{3}' text:'{4}', probability {5}% (Cutoff: {6}%).", m_VMC.m_iKey, (ii + 1), iNum, rrTmp.Result, rrTmp.Text, rrTmp.Probability, m_iRecognitionCutoffPercentage));

					if(rrTmp.Probability > iProb)
					{
						iProb = rrTmp.Probability;
						sResult = rrTmp.Text;
						sProb = rrTmp.Probability.ToString();
					}
				}
				
				// Save highest ranking result to "utterance" and "confidence" variables.
				i_dfField.FindDVariable(DField.eVars.inputmode.ToString()).SValue = DField.eInputModes.speech.ToString();
				i_dfField.FindDVariable(DField.eVars.utterance.ToString()).SValue = sResult;
				i_dfField.FindDVariable(DField.eVars.confidence.ToString()).SValue = sProb;

				FillFieldNameVariable(i_dfField, sResult);

				// FIX - Fill VoiceXML standard app vars from spec 5.1.5
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.FillDefaultVariables_Utterance: " + exc.ToString());
			}
		}

		/// <summary>
		/// Fill in default variables.
		/// </summary>
		/// <param name="i_mDtmf"></param>
		/// <param name="i_dfField"></param>
		private void FillDefaultVariables_Utterance(ISMessaging.Audio.ISMDtmfComplete i_mDtmf, DField i_dfField)
		{
			try
			{
				i_dfField.FindDVariable(DField.eVars.inputmode.ToString()).SValue = DField.eInputModes.dtmf.ToString();
				i_dfField.FindDVariable(DField.eVars.utterance.ToString()).SValue = i_mDtmf.m_sDtmf;
				i_dfField.FindDVariable(DField.eVars.confidence.ToString()).SValue = "100";

				FillFieldNameVariable(i_dfField, i_mDtmf.m_sDtmf);

				// FIX - Fill VoiceXML standard app vars from spec 5.1.5
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.FillDefaultVariables_Utterance: " + exc.ToString());
			}
		}

		private bool FillFieldNameVariable(DField i_dfField, string i_sVal)
		{
			bool		bRet = true;
			DVariable	dvTmp = null;

			try
			{
				// Assign variable with name of field
				dvTmp = i_dfField.m_dfParentForm.FindDVariable(i_dfField.ID);
				if(dvTmp == null)
				{
					dvTmp = new DVariable(i_dfField.ID, i_sVal);
					i_dfField.m_dfParentForm.Variables.Add(dvTmp);
				}
				else
				{
					dvTmp.SValue = i_sVal;
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.FillFieldNameVariable: " + exc.ToString());
			}

			return(bRet);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_MDtmf"></param>
		/// <param name="i_dfField"></param>
		/// <returns></returns>
		public int FindMatchingDtmfResult(ISMessaging.Audio.ISMDtmfComplete i_MDtmf, DField i_dfField)
		{
			int			iRet = -1;
			int			ii = 0, iDResults = 0;
			DResponse	drTmp = null;

			try
			{
				iDResults = i_dfField.m_Options.Count;
				for(ii = 0; ii < iDResults; ii++)
				{
					drTmp = (DResponse)i_dfField.m_Options[ii];

					// FIX - Extract proper result
					// For now, just find option that is DTMF.
					if(drTmp.m_sValue == DField.eInputModes.dtmf.ToString())
					{
						iRet = ii;
					}
				}
			}
			catch(Exception exc)
			{
				iRet = -1;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.FindMatchingDtmfResult: " + exc.ToString());
			}

			return(iRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_mBegin"></param>
		/// <returns></returns>
		public bool ProcessSessionBegin(ISMessaging.Session.ISMSessionBegin i_mBegin)
		{
			bool bRet = true;

			try
			{
				// Set IpAddress, SessionId in Logger so all subsequent calls to Log() will be identified.  (Need-to/should do this in every thread once the ISMSessionBegin comes in.)
				m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.IpAddress.ToString(), Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(oAddr => oAddr.AddressFamily == AddressFamily.InterNetwork).ToString() ?? "");
				m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.SessionId.ToString(), i_mBegin.m_sSessionId);

				if (!CapabilitiesManager.IsLicenseValid || (ConfigParams.GetNumExt() > CapabilitiesManager.MaximumNumberOfPorts))
				{
					m_Logger.Log(Level.Warning, "IMPORTANT: SpeechBridge license missing or invalid.");
					NotifyCallerThatLicenseIsMissingOrInvalid();
				}
				else
				{
                    // Get a new DSession instance so we ensure that no session variables and properties from the previous call are
                    // available in this call.
                    // NOTE: It doesn't appear that the VMC passed to DSession is used for anything. However, to minimize the change 
                    // it seems safer to just grab the VMC from the previous instance.

                    m_Session = new DSession(m_Session.VMC);


					// If we had been expecting the session to disconnect (due to a prompt+terminate or transfer), but
					// for some reason never got the ISMSessionEnd, we need to reset some session variables here.
					if (m_bPendingDisconnect)
					{
						ClearSessionVariables();
					}

					LoadConfigurationSettings();

					// Fill the session variables
					FillDefaultVariables_Session(i_mBegin);

					// Reset the active form & field, in case this page is being reloaded.
					m_Doc.m_iActiveForm = 0;
					for (int ii = 0; ii < m_Doc.m_DForms.Count; ++ii)
					{
						m_Doc.m_DForms[ii].m_iActiveField = 0;
					}

					// Load starting URL from DB (default has already been set from config file).
					string sUrlStart = GetUrlStart(m_sToUser);
					if (sUrlStart.Length > 0)
					{
						m_sUrlStart = sUrlStart;
					}

                    m_Logger.Log(Level.Debug, String.Format("[{0}]DialogEngine.ProcessSessionBegin loading: {1}", m_VMC.m_iKey, m_sUrlStart));
					//LoadPage(m_sUrlStart, "", true);
					LoadPage(m_sUrlStart, "");
				}


				// Issue prompts.  FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
				IssueFormAndFieldPrompts();

				// Initialize active form and fields
				// IssueFieldPrompts();		// FIX - Reset on begin or end?

                IssueIdleMessage();
			}
			catch (Exception exc)
			{
				bRet = false;
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ProcessSessionBegin: {1}", m_VMC.m_iKey, exc.ToString()));
			}

			return bRet;
		} // ProcessSessionBegin()

		private string GetUrlStart(string i_sDID)
		{
			string sRet = "";

			try
			{
                m_Logger.Log(Level.Info, String.Format("[{0}]DialogEngine.GetUrlStart Looking up '{1}'.", m_VMC.m_iKey, i_sDID));
                
                DIDMap dmMap = new DIDMap();
				dmMap.Load();

				sRet = dmMap[i_sDID];

                if (String.IsNullOrEmpty(sRet))
				{
                    m_Logger.Log(Level.Verbose, String.Format("[{0}]DialogEngine.GetUrlStart - no mapping found for '{1}', using DEFAULT.", m_VMC.m_iKey, i_sDID));

					sRet = dmMap[DIDMap.DEFAULT_DID];

					if (String.IsNullOrEmpty(sRet))
					{
                        m_Logger.Log(Level.Warning, String.Format("[{0}]DialogEngine.GetUrlStart - no mapping found for DEFAULT, use UrlStart from DialogMgr.exe.config.", m_VMC.m_iKey));

                        sRet = ConfigurationManager.AppSettings["UrlStart"];

						if (String.IsNullOrEmpty(sRet))
						{
                            m_Logger.Log(Level.Exception, "UrlStart is missing or empty in DialogMgr.exe.config.");


                            //$$$ LP - The following default probably does NOT do the correct thing since starting with SB 5.1 the AAMain filename contains a language code.
                            //         We should probably just play a message informing caller of a problem with the system and then hang up (c.f. NotifyCallerThatLicenseIsMissingOrInvalid()).
                            //         Use the default US version here and just hope that it does something useful.
                            //
                            // NOTE: If you ever get here then things will probably not work right anyway since VoiceXmlParser.ParseRootDoc() relies on "UrlStart" having
                            //       a valid value for resolving where to find VXML files that aren't fully qualified (i.e. how we use "application" in the <vxml> element).

                            sRet = "http://localhost/VoiceDocStore/AAMain_en-US.vxml.xml";        
						}
					}
				}
			}
			catch (Exception exc)
			{
				sRet = "";
                m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.GetUrlStart(): '{1}'", m_VMC.m_iKey, exc.ToString()));
			}

			return sRet;
		} // GetUrlStart

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_mBegin"></param>
		private void FillDefaultVariables_Session(ISMessaging.Session.ISMSessionBegin i_mBegin)
		{
			bool		bRes = true;
			string		sProtocol = "";
			string		sFromUser = "", sFromAddress = "", sFromPort = "", sFromDisplayName = "", sFromFull = "";
			string		sToUser = "", sToAddress = "", sToPort = "", sToDisplayName = "";

			m_sSessionId = i_mBegin.m_sSessionId;		// Save session-id, to be used in all messages sent to AudioMgr

			// FIX - Outbound calls will have the local/remote reversed!
			bRes = SBSipAddr.QuickParse(i_mBegin.m_sFromAddr, out sFromUser, out sFromAddress, out sFromPort, out sFromDisplayName);
			if(bRes)
			{
				sProtocol = "SIP";
				sFromFull = sFromUser + "@" + sFromAddress + ":" + sFromPort;

				bRes = SBSipAddr.QuickParse(i_mBegin.m_sToAddr, out sToUser, out sToAddress, out sToPort, out sToDisplayName);

				m_Session.FindDVariable(DSession.eVars.connection_local_uri.ToString()).SValue = sToUser + "@" + sToAddress + ":" + sToPort;
				m_Session.FindDVariable(DSession.eVars.connection_remote_uri.ToString()).SValue =  sFromFull;
				m_Session.FindDVariable(DSession.eVars.connection_remote_displayname.ToString()).SValue = sFromDisplayName;
				m_Session.FindDVariable(DSession.eVars.connection_originator.ToString()).SValue = sFromFull;

				// Also store a local copy of session variables for easier/faster local access.
				m_sFromUser = sFromUser;
				m_sFromAddress = sFromAddress;
				m_sFromPort = sFromPort;
				m_sFromDisplayName = sFromDisplayName;
				m_sToUser = sToUser;
				m_sToAddress = sToAddress;
				m_sToPort = sToPort;

				m_Logger.Log(Level.Verbose, String.Format("CallerID: '{0}', Destination DID: '{1}'", sFromUser, sToUser));
			}
			else
			{
				sProtocol = "UNKNOWN";
			}

			m_Session.FindDVariable(DSession.eVars.connection_protocol_name.ToString()).SValue = sProtocol;
            m_Session.FindDVariable(DSession.eVars.speechbridge_session_id.ToString()).SValue = i_mBegin.m_sSessionId;
		} // FillDefaultVariables_Session

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private void ClearSessionVariables()
		{
			m_sSessionId = "";
			m_sUrlCurrent = "";
			m_bPendingDisconnect = false;
		} // ClearSessionVariables

		/// <summary>
		/// 
		/// </summary>
		private void LoadConfigurationSettings()
		{
			ConfigParams cfgs = new ConfigParams();
			bool bRes = cfgs.LoadFromTableByComponent(ConfigParams.e_Components.Apps.ToString() + ConfigParams.c_Separator + ConfigParams.e_Components.global.ToString());

			if (!bRes)
			{
				m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.LoadConfigurationSettings: Couldn't retrieve DB settings!", m_VMC.m_iKey));
			}
			else
			{
				foreach (ConfigParams.ConfigParam param in cfgs)
				{
					if (param.Name == ConfigParams.e_SpeechAppSettings.RecognitionCutoff.ToString())
					{
						int iRecognitionCutoffPercentage;

						bool bCanParse = Int32.TryParse(param.Value, out iRecognitionCutoffPercentage);

						if (bCanParse)
						{
							m_iRecognitionCutoffPercentage = iRecognitionCutoffPercentage;
						}
						else
						{
							m_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.LoadConfigurationSettings: Unable to parse RecognitionCutoff ('{1}') - using default.", m_VMC.m_iKey, param.Value));
						}

						m_Logger.Log(Level.Verbose, String.Format("[{0}]DialogEngine.LoadConfigurationSettings: Recognition Cutoff = {1}%", m_VMC.m_iKey, m_iRecognitionCutoffPercentage));
					}
				}
			}
		} // LoadConfigurationSettings

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_mEnd"></param>
		/// <returns></returns>
		public bool ProcessSessionEnd(ISMessaging.Session.ISMSessionEnd i_mEnd)
		{
			bool		bRet = true;

			try
			{
                m_Logger.Log(Level.Info, String.Format("[{0}]DialogEngine.ProcessSessionEnd - Reason: '{1}'", m_VMC.m_iKey, i_mEnd.m_sReason));
				//LoadPage(m_sUrlStart, false);		// FIX - Reset on begin or end?
				ClearSessionVariables();
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("DialogMgr_Console.DialogMgr_srv.ProcessSessionEnd() exception '{0}'.", e);
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ProcessSessionEnd: " + exc.ToString());
			}

			return(bRet);
		} // ProcessSessionEnd()

		public bool ProcessTimerExpired(ISMessaging.Session.ISMTimerExpired i_mTimer)
		{
			bool		bRet = true;

			try
			{
				// FIX - this default behavior should be overridden by voicexml.
                // FIX - It may not always be correct to issue form and field prompts.  See comments in DForm.
                IssueFormAndFieldPrompts();

                IssueIdleMessage();
            }
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_VMC.m_iKey.ToString() + "]" + "DialogEngine.ProcessTimerExpired exception :" + exc.ToString());
			}

			return(bRet);
		} // ProcessTimerExpired

		private void NotifyCallerThatLicenseIsMissingOrInvalid()
		{
			DForm form = new DForm(m_Doc);
			form.m_DFields.Add(new DField(form));				// Not used for anything but needed to prevent ArgumentOutOfRangeException when processing form in IssueFormAndFieldPrompts().

			form.ActionsPre.Add(new DAction()
									{
										m_Type = DAction.DActionType.ePlayPrompt,
										m_oValue = new DPrompt()
										{
											m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eWavFilePath,
											m_oValue = "file:///opt/speechbridge/VoiceDocStore/Prompts/LicenseMissingOrInvalid.wav",
											m_sText = "Your license is missing or invalid.  Please contact your authorized SpeechBridge reseller.",					//$$$ LP - What if system language is NOT English?  I guess they had better make sure they wave file exists and is in the correct language.
											m_bBargeinEnabled = false
										}
									});
			form.ActionsPre.Add(new DAction()
									{
										m_Type = DAction.DActionType.eCC_HangupAfterPrompts,
									});

			m_Doc.m_DForms.Clear();
			m_Doc.m_DForms.Add(form);
			m_Doc.m_iActiveForm = 0;

			return;
		} // NotifyCallerThatLicenseIsMissingOrInvalid
	}	//class DialogEngine
}
