// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

using DialogModel;
using Incendonet.Utilities.LogClient;
using XmlDocParser;

namespace DialogEngine
{
	/// <summary>
	/// Summary description for VoiceXmlParser.
	/// </summary>
	public class VoiceXmlParser : IInteractionDocParser
	{
		protected			ILegacyLogger		m_Logger = null;

		public		static	string		s_sVariableTag =		"^";			// FIX - This convention will cause problems with other JS interpreters
		public		static	char[]		s_acTerminator =		{';'};
		//public		static	char[]		s_acBreak =				{' ', '\t', '\n', '\r', '\'', '\"', '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', '{', ']', '}', ';', ':', ',', '<', '>', '/', '?', '$', '.'};
		//public		static	char[]		s_acBreakNonVar =		{' ', '\t', '\n', '\r', '\'', '\"', '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', '{', ']', '}', ';', ':', ',', '<', '>', '/', '?'};
		//public		static	char[]		s_acBreakNonVarQuote =	{' ', '\t', '\n', '\r', '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', '{', ']', '}', ';', ':', ',', '<', '>', '/', '?'};
		public		static	char[]		s_acBreak =				{' ', '\t', '\n', '\r', '\'', '\"', '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '=', '+', '[', '{', ']', '}', ';', ':', ',', '<', '>', '/', '?', '$', '.'};
		public		static	char[]		s_acBreakNonVar =		{' ', '\t', '\n', '\r', '\'', '\"', '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '=', '+', '[', '{', ']', '}', ';', ':', ',', '<', '>', '/', '?'};
		public		static	char[]		s_acBreakNonVarQuote =	{' ', '\t', '\n', '\r', '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '=', '+', '[', '{', ']', '}', ';', ':', ',', '<', '>', '/', '?'};

        private class PropertyDefinition
        {
            public enum Type
            {
                eRealNumberDesignation,
                eIntegerDesignation,
                eTimeDesignation,
                eString,
                eNotImplemented
            };

            public Type m_type;
            public string[] m_AllowedValues;

            public PropertyDefinition(Type i_type, params string[] i_allowedValues)
            {
                m_type = i_type;
                m_AllowedValues = i_allowedValues;
            }
        }

        private static Dictionary<string, PropertyDefinition> m_KnownProperties = new Dictionary<string, PropertyDefinition>
        {
            // Generic Speech Recognizer Properties (see VoiceXML 2.0 Specification - Section 6.3.2)

            { "confidencelevel", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "sensitivity", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "speedvsaccuracy", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "completetimeout", new PropertyDefinition(PropertyDefinition.Type.eTimeDesignation, null) },
            { "incompletetimeout", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "maxspeechtimeout", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },


            // Generic DTMF Recognizer Properties (see VoiceXML 2.0 Specification - Section 6.3.3)

            { "interdigittimeout", new PropertyDefinition(PropertyDefinition.Type.eTimeDesignation, null) },
            { "termtimeout", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "termchar", new PropertyDefinition(PropertyDefinition.Type.eString, new string[] {"", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "#"}) },


            // Prompt and Collect Properties (see VoiceXML 2.0 Specification - Section 6.3.4)

            { "bargein", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"true", "false"}) },
            { "bargeintype", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"speech", "hotword"}) },
            { "timeout", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },


            // Fetching Properties (see VoiceXML 2.0 Specification - Section 6.3.5)

            { "audiofetchhint", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"safe", "prefetch"}) },
            { "audiomaxage", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "audiomaxstale", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "documentfetchhint", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"safe", "prefetch"}) },
            { "documentmaxage", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "documentmaxstale", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "grammarfetchhint", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"safe", "prefetch"}) },
            { "grammarmaxage", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "grammarmaxstale", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "objectfetchhint", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"safe", "prefetch"}) },
            { "objectmaxage", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "objectmaxstale", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "scriptfetchhint", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"safe", "prefetch"}) },
            { "scriptmaxage", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "scriptmaxstale", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "fetchaudio", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },                   //$$$ - If / when implemented this would be of type eString which would require changes to how we validate string properties since it can be any value (it is a URI) - probably should allow any string value if no list of allowed values is specified.
            { "fetchaudiodelay", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "fetchaudiominimum", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },
            { "fetchtimeout", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) },


            // Miscellaneous Properties (see VoiceXML 2.0 Specification - Section 6.3.6)

            { "inputmodes", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"dtmf voice", "dtmf", "voice"}) },            //$$$ - If / when implemented this would be of type eString which would require changes to how we validate string properties since it can be any combination of allowed values - probably should define a new type (eCombination).
            { "universals", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, new string[] {"none", "all", "exit", "help", "cancel"}) },  //$$$ - If / when implemented this would be of type eString which would require changes to how we validate string properties since it can be any combination of allowed values - probably should define a new type (eCombination).
            { "maxnbest", new PropertyDefinition(PropertyDefinition.Type.eNotImplemented, null) }
        };

		public VoiceXmlParser(ILegacyLogger i_Logger)
		{
			m_Logger = i_Logger;
		}

		/// <summary>
		/// FIX - For the most accurate determination, this should do a scoping lookup to determine
		/// if the variable has been declared.  The assumption now is that this function will only be
		/// used when a variable is expected to have the VoiceXML "_" or "$." identifiers.  This means
		/// that the current tree should be passed in.
		/// </summary>
		/// <param name="i_sVarName"></param>
		/// <returns></returns>
		public static bool IsVariableName(string i_sVarName)
		{
			bool	bRet = false;

			// FIX - This should do a traversal of all variable names, or come up with a better scheme...
			//if( (i_sVarName.IndexOf("$.") >= 0) || (i_sVarName.IndexOf('_') >= 0) || (i_sVarName.IndexOf("session.") >= 0) || (i_sVarName.IndexOf("application.") >= 0) || (i_sVarName.IndexOf("document.") >= 0) || (i_sVarName.IndexOf("dialog.") >= 0) )
			if( (i_sVarName.IndexOf("$.") >= 0) || (i_sVarName.IndexOf("session.") >= 0) || (i_sVarName.IndexOf("application.") >= 0) || (i_sVarName.IndexOf("document.") >= 0) || (i_sVarName.IndexOf("dialog.") >= 0) )
			{
				bRet = true;
			}

			return(bRet);
		}

		/// <summary>
		/// Translate variable names from '$' notation of javascript in VoiceXML to our internal
		/// representation for easier parsing.  Final output will be in the form of "^object.variable",
		/// with "object." being recursive and optional.
		/// FIX - Scope should be parsed here as well to generate a fully qualified name.
		/// </summary>
		/// <param name="i_sVarName"></param>
		/// <returns></returns>
		public static string TranslateVariableName(ILegacyLogger i_Logger, string i_sVarName)
		{
			string			sRet;
			StringBuilder	sbTmp = null;
			int				iIdStart = 0;

			try
			{
				iIdStart = i_sVarName.IndexOf("$.");		// FIX - Should we be replacing "$" also?
				if(iIdStart > 0)
				{
					sbTmp = new StringBuilder();
					sbTmp.Append(s_sVariableTag);						// FIX - This convention will cause problems with other JS interpreters
					sbTmp.Append(i_sVarName.Substring(0, iIdStart));
					iIdStart++;
					sbTmp.Append(i_sVarName.Substring(iIdStart, i_sVarName.Length - iIdStart));

					sRet = sbTmp.ToString();
				}
				else
				{
					/*
					iIdStart = i_sVarName.IndexOf('_');
					if(iIdStart >= 0)
					{
						sbTmp = new StringBuilder();
						sbTmp.Append(s_sVariableTag);
						if(iIdStart == 0)
						{
							sbTmp.Append(i_sVarName.Substring(1, i_sVarName.Length - 1));
						}
						else
						{
							sbTmp.Append(i_sVarName.Substring(0, iIdStart));
							iIdStart++;
							sbTmp.Append(i_sVarName.Substring(iIdStart, i_sVarName.Length - iIdStart));
						}

						sRet = sbTmp.ToString();
					}
					else
					*/
					{
						sRet = s_sVariableTag + i_sVarName;
					}
				}
			}
			catch(Exception exc)
			{
				sRet = "";
				i_Logger.Log(Level.Exception, "VoiceXmlParser.TranslateVariableName caught exception: " + exc.ToString());
			}

			return(sRet);
		}

		/// <summary>
		/// Translates the variable name, in place, in the script line passed in.
		/// </summary>
		/// <param name="i_sScriptLine"></param>
		/// <returns></returns>
		public static string TranslateVariableNameInPlace(ILegacyLogger i_Logger, string i_sScriptLine)
		{
			string		sRet = "", sSrc = "", sPreamble = "", sPostamble, sVarName = "";
			int			ii = 0, jj = 0, iIndexStart = 0, iIndexStop = 0, iIndexDollar = 0;
			bool		bFound = false;

			try
			{
				sSrc = i_sScriptLine;

				// Isolate variable name(s) from rest of string
				iIndexDollar = sSrc.IndexOf('$');
				if(iIndexDollar >= 0)
				{
					// Search backwards for beginning of variable name
					bFound = false;
					for(ii = iIndexDollar - 1; ( (ii >= 0) && (!bFound) ); ii--)
					{
						for(jj = 0; ( (jj < s_acBreakNonVar.Length) && (!bFound) ); jj++)
						{
							if(sSrc[ii] == s_acBreakNonVar[jj])
							{
								bFound = true;
							}
						}
					}
					iIndexStart = (iIndexStart < 0) ? 0 : (ii + 2);

					// Search forwards for end of variable name
					bFound = false;
					if(iIndexDollar < (sSrc.Length - 1) )
					{
						for(ii = iIndexDollar + 1; ( (ii < sSrc.Length) && (!bFound) ); ii++)
						{
							for(jj = 0; ( (jj < s_acBreakNonVar.Length) && (!bFound) ); jj++)
							{
								if(sSrc[ii] == s_acBreakNonVar[jj])
								{
									bFound = true;
								}
							}
						}
						iIndexStop = ii - 1;
					}
					else
					{
						iIndexStop = iIndexDollar;
					}
					iIndexStop = (iIndexStop >= sSrc.Length) ? (sSrc.Length - 1) : iIndexStop;

					sVarName = sSrc.Substring(iIndexStart, (iIndexStop - iIndexStart));

					// Pass variable names to TranslateVariableName
					sVarName = TranslateVariableName(i_Logger, sVarName);

					// Reassemble script line
					sPreamble = sSrc.Substring(0, iIndexStart);
					sPostamble = sSrc.Substring(iIndexStop, (sSrc.Length - iIndexStop));
					sRet = sPreamble + sVarName + sPostamble;

					// If there is another var to be translated in string, call recursively.
					iIndexDollar = sRet.IndexOf('$');
					if(iIndexDollar > 0)
					{
						sRet = TranslateVariableNameInPlace(i_Logger, sRet);
					}
				}
				else
				{	// FIX - This won't always be right.
					sRet = TranslateVariableName(i_Logger, i_sScriptLine);
				}
			}
			catch(Exception exc)
			{
				sRet = "";
				i_Logger.Log(Level.Exception, "VoiceXmlParser.TranslateVariableNameInPlace caught exception: " + exc.ToString());
			}

			return(sRet);
		}

		/// <summary>
		/// Parses VXML root document.  Currently only extracts and assigns application variables.
		/// </summary>
		/// <param name="io_ddDoc"></param>
		/// <returns></returns>
		public eParseError ParseRootDoc(string i_sRootDoc, ref DDocument io_ddDoc)
		{
			eParseError		eRet = eParseError.eSuccess;
			bool			bRes = true;
			string			sXmlCurrent = "";
			XElement		xeRoot = null;
			DSession		dsSession = null;
			DDocument		ddDoc = null;
			int				ii = 0, iNumVars = 0, iIndex = -1;
			string			sUrl = "", sStartUrl = "";
			DVariable		dvTmp = null;

			try
			{
				// Get and parse root doc's XML
				if(i_sRootDoc.IndexOf('/') >= 0)
				{
					sUrl = i_sRootDoc;
				}
				else
				{
					sStartUrl = ConfigurationManager.AppSettings["UrlStart"];
					iIndex = sStartUrl.LastIndexOf('/');
					if(iIndex >= 0)
					{
						sUrl = sStartUrl.Substring(0, iIndex + 1) + i_sRootDoc;
					}
					else
					{
						sUrl = i_sRootDoc;
					}
				}
				bRes = XmlParser.GetXmlString(m_Logger, sUrl, out sXmlCurrent);
				if(!bRes)
				{
					eRet = eParseError.eUnknown;		// FIX add better error to enum
				}
				else
				{
					xeRoot = new XElement();
					bRes = XmlParser.ParseXml(m_Logger, sUrl, sXmlCurrent, ref xeRoot);
					if(!bRes)
					{
						eRet = eParseError.eSyntaxError;
					}
					else
					{
						// Parse VoiceXML
						dsSession = new DSession(-1);			// FIX - SAFE????
						ddDoc = new DDocument(dsSession);		// FIX - SAFE????

						eRet = ParseDoc(xeRoot, dsSession, ref ddDoc);
						if(eRet != eParseError.eSuccess)
						{
							eRet = eParseError.eSyntaxError;
						}
						else
						{
							// Copy out application variables
							iNumVars = ddDoc.m_DVariables.Count;
							for(ii = 0; ii < iNumVars; ii++)
							{
								// NOTE - We're treating VoiceXML session and application variables the same.  Problem?

								dvTmp = ddDoc.m_DVariables[ii];
								// If the variable doesn't exist, add it, otherwise keep the original value.
								if(io_ddDoc.m_dsParentSession.m_DVariables[dvTmp.Name] == null)
								{
									io_ddDoc.m_dsParentSession.m_DVariables.Add(dvTmp);
								}
								dvTmp = null;
							}


                            // Copy out properties.

                            foreach (DProperty property in ddDoc.m_DProperties)
                            {
                                // If the property doesn't exist add it, otherwise overwrite the exiting value.

                                if (io_ddDoc.m_dsParentSession.m_DProperties[property.Name] == null)
                                {
                                    io_ddDoc.m_dsParentSession.m_DProperties.Add(property);
                                }
                                else
                                {
                                    io_ddDoc.m_dsParentSession.m_DProperties[property.Name] = property;
                                }
                            }
						}
					}
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				//Console.Error.WriteLine("VoiceXmlParser.ParseDoc() exception '{0}'.", e);
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseRootDoc caught exception: " + exc.ToString());
			}

			return(eRet);
		}

		#region IInteractionDocParser Members
		/// <summary>
		/// Parses XML document (or section of doc when called recursively)
		/// </summary>
		/// <param name="i_xeElem"></param>
		/// <param name="i_dsSession"></param>
		/// <param name="io_ddDoc"></param>
		/// <returns></returns>
		public eParseError ParseDoc(XElement i_xeElem, DSession i_dsSession, ref DDocument io_ddDoc)
		{
			eParseError		eRet = eParseError.eSuccess;
			XElement		xeCurr = null;
			XAttribute		xaCurr = null;
			int				ii, iElems, iNumAttrs;

			try
			{
				if(io_ddDoc == null)
				{
					io_ddDoc = new DDocument(i_dsSession);
				}

				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "application" :
						{
							io_ddDoc.m_sApplication = xaCurr.Value;

							// Parse root document
							ParseRootDoc(io_ddDoc.m_sApplication, ref io_ddDoc);
						}
						break;
						case "version" :
						{
						}
						break;
						case "lang" :
						{
							io_ddDoc.m_sLang = xaCurr.Value;
							m_Logger.Log(Level.Info, "VoiceXmlParser.ParseDoc using language: '" + io_ddDoc.m_sLang + "'.");
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <vxml> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				// Parse elements
				iElems = i_xeElem.m_alElements.Count;
				for(ii = 0; ii < iElems; ii++)
				{
					xeCurr = i_xeElem.m_alElements[ii];
					switch(xeCurr.Name)
					{
						case "vxml" :
						{
							eRet = ParseDoc(xeCurr, i_dsSession, ref io_ddDoc);	// FIX - Check attributes, instead of just continuing on?
						}
						break;
						case "var" :
						{
							DVariable dvTmp = new DVariable();
							io_ddDoc.m_DVariables.Add(dvTmp);
							ParseVar(xeCurr, ref dvTmp);
						}
						break;
						case "form" :
						{
							DForm	tmpForm = new DForm(io_ddDoc);

							io_ddDoc.m_DForms.Add(tmpForm);
							eRet = ParseForm(xeCurr, ref tmpForm);
						}
						break;
						case "block":
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - <block> element not implemented in <vxml> element (Near line {0} in file '{1}').", xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
						}
						break;
                        case "property":
                        {
                            DProperty property = new DProperty();
                            eRet = ParseProperty(xeCurr, ref property);

                            if (eRet == eParseError.eSuccess)
                            {
                                io_ddDoc.m_DProperties.Add(property);
                            }
                        }
                        break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <vxml> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
						}
						break;
					}
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				//Console.Error.WriteLine("VoiceXmlParser.ParseDoc() exception '{0}'.", e);
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseDoc caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseDoc
		#endregion

		public eParseError ParseForm(XElement i_xeElem, ref DForm io_Form)
		{
			eParseError		eRet = eParseError.eSuccess;
			XElement		xeCurr = null;
			XAttribute		xaCurr = null;
			int				ii, iElems;

			// Parse attributes
			for(ii = 0; ii < i_xeElem.m_alAttributes.Count; ii++)
			{
				xaCurr = i_xeElem.m_alAttributes[ii];
				switch(xaCurr.Name)
				{
					case "id" :
					{
						io_Form.ID = xaCurr.Value;		// FIX - ID should really be fully qualified name, ie. 'http://blah1/blah2.vxml.xml#blah3'.
						io_Form.m_sName = xaCurr.Value;
					}
					break;
					default :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <form> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
					}
					break;
				}
			}

			// Parse elements
			iElems = i_xeElem.m_alElements.Count;
			for(ii = 0; ii < iElems; ii++)
			{
				xeCurr = i_xeElem.m_alElements[ii];
				switch(xeCurr.Name)
				{
					case "field" :
					{
						DField	tmpField = new DField(io_Form);

						io_Form.m_DFields.Add(tmpField);
						eRet = ParseField(xeCurr, ref tmpField);
					}
					break;
					case "block" :
					{
						eRet = ParseBlock(xeCurr, ref io_Form);
					}
					break;
                    case "property":
                    {
                        DProperty property = new DProperty();
                        eRet = ParseProperty(xeCurr, ref property);

                        if (eRet == eParseError.eSuccess)
                        {
                            io_Form.Properties.Add(property);
                        }
                    }
                    break;
                    case "initial":
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - <initial> element not implemented in <form> element (Near line {0} in file '{1}').", xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
					}
					break;
					default :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <form> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
					}
					break;
				}
			}

			return(eRet);
		}

		public eParseError ParseField(XElement i_xeElem, ref DField io_Field)
		{
			eParseError		eRet = eParseError.eSuccess, eRes = eParseError.eSuccess;
			XElement		xeCurr = null;
			//XAttribute		xaCurr = null;
			int				ii, iElems;
			DActions		adaTmp = null;

			eRes = ParseFieldAttrs(i_xeElem.m_alAttributes, ref io_Field);
			// If the parse result is not successful for the attributes, return that error.
			if(eRes != eParseError.eSuccess)
			{
				eRet = eRes;
			}

			// Parse elements
			iElems = i_xeElem.m_alElements.Count;
			for(ii = 0; ii < iElems; ii++)
			{
				xeCurr = i_xeElem.m_alElements[ii];
				switch(xeCurr.Name)
				{
					case "prompt" :
					{
						adaTmp = io_Field.ActionsPre;
						eRes = ParsePrompt(xeCurr, ref adaTmp);
						io_Field.ActionsPre = adaTmp;
					}
					break;
					case "option" :
					{
						DResponse	tmpResponse = new DResponse();
						//DAction		tmpAction = new DAction();

						io_Field.m_Options.Add(tmpResponse);
						tmpResponse.m_sValue = xeCurr.Value;
					}
					break;
					case "grammar" :
					{
						io_Field.m_DGrammar = new DGrammar();

						eRes = ParseGrammar(xeCurr, ref io_Field.m_DGrammar);
					}
					break;
					case "filled" :
					{
						eRes = ParseFieldFilled(xeCurr, ref io_Field);
					}
					break;
                    case "property":
                    {
                        DProperty property = new DProperty();
                        eRes = ParseProperty(xeCurr, ref property);

                        if (eRes == eParseError.eSuccess)
                        {
                            io_Field.Properties.Add(property);
                        }
                    }
                    break;
                    default:
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <field> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
					}
					break;
				} // switch

				// If the parse result is not successful for any of the elements, return that error.
				if(eRes != eParseError.eSuccess)
				{
					eRet = eRes;
				}
			} // for

			return(eRet);
		}

		public eParseError ParseFieldAttrs(List<XAttribute> i_Attrs, ref DField io_Field)
		{
			eParseError		eRet = eParseError.eSuccess;
			XAttribute		xaCurr = null;
			int				ii, iSize;

			iSize = i_Attrs.Count;
			for(ii = 0; ii < iSize; ii++)
			{
				xaCurr = i_Attrs[ii];
				switch(xaCurr.Name)
				{
					case "name" :
					{
						io_Field.ID = xaCurr.Value;
					}
					break;
					default :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <field> element (Near line {1} in file '{2}').", xaCurr.Name, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
					}
					break;
				} // switch
			} // for

			return(eRet);
		}

		public eParseError ParseFieldFilled(XElement i_xeElem, ref DField io_Field)
		{
			eParseError		eRet = eParseError.eSuccess;
			//XAttribute		xaCurr = null;
			XElement		xeCurr = null;
			int				ii, iSize;
			DAction			daTmp = null;
			DConditions		adcConds = null;
			DVariables		advTmp = null;

			// FIX - Check if there would ever be any attributes

			iSize = i_xeElem.m_alElements.Count;
			for(ii = 0; ii < iSize; ii++)
			{
				xeCurr = i_xeElem.m_alElements[ii];
				switch(xeCurr.Name)
				{
					case "if" :
					{
						adcConds = new DConditions();
						daTmp = new DAction();
						io_Field.m_DActionsPost.Add(daTmp);
						daTmp.m_Type = DAction.DActionType.eCondition;
						daTmp.m_oValue = adcConds;

						advTmp = io_Field.Variables;
						ParseCondition(xeCurr, ref adcConds, ref advTmp);
						io_Field.Variables = advTmp;
					}
					break;
					default :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <filled> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
					}
					break;
				}
			}

			return(eRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sFieldName"></param>
		/// <param name="i_sCond"></param>
		/// <param name="o_sFieldVariable">If a field variable was referenced in the cond, it will be returned here.</param>
		/// <returns></returns>
		public string GetFieldCond(string i_sFieldName, string i_sCond, out string o_sFieldVariable)
		{
			string			sRet = null;
			int				iStart, iEnd, iLen;
			StringBuilder	sbTmp = null;
			char[]			caEOV = {' ', '=', '&'};

			o_sFieldVariable = "";

			if(!i_sCond.StartsWith(i_sFieldName))
			{
				sRet = "";
				//Console.Error.WriteLine("VoiceXmlParser.GetVal_cond - Didn't find field name '{0}' in cond attribute '{1}'.", i_sFieldName, i_sCond);
				sbTmp = new StringBuilder();
				sbTmp.AppendFormat("VoiceXmlParser.GetVal_cond - Didn't find field name '{0}' in cond attribute '{1}'.", i_sFieldName, i_sCond);
				m_Logger.Log(Level.Warning, sbTmp.ToString());
			}
			else
			{
				iStart = i_sCond.IndexOf("'") + 1;
				iEnd = i_sCond.LastIndexOf("'");
				iLen = iEnd - iStart;
				sRet = i_sCond.Substring(iStart, iLen);

				// Find variable, if used
				iStart = i_sCond.IndexOf("$.");
				if(iStart > 0)
				{
					iStart += 2;
					iEnd = i_sCond.IndexOfAny(caEOV, iStart);
					iLen = iEnd - iStart;
					o_sFieldVariable = i_sCond.Substring(iStart, iLen);
				}
			}

			return(sRet);
		}

		public eParseError ParseBlock(XElement i_xeElem, ref DForm io_dfForm)
		{
			eParseError		eRet = eParseError.eSuccess, eRes = eParseError.eSuccess;
			XElement		xeCurr = null;
			int				ii, iElems;
			DAction			daTmp = null;
			DPrompt			dpTmp = null;
			DScript			dsTmp = null;
			DConditions		adcConds = null;
			DVariables		advTmp = null;
			DActions		adaTmp = null;

			eRes = ParseBlockAttrs(i_xeElem.m_alAttributes, ref io_dfForm);
			// If the parse result is not successful for the attributes, return that error.
			if(eRes != eParseError.eSuccess)
			{
				eRet = eRes;
			}

			// Parse elements
			iElems = i_xeElem.m_alElements.Count;
			for(ii = 0; ii < iElems; ii++)
			{
				xeCurr = i_xeElem.m_alElements[ii];
				switch(xeCurr.Name)
				{
					case "prompt" :
					{
						adaTmp = io_dfForm.ActionsPre;
						eRes = ParsePrompt(xeCurr, ref adaTmp);		// FIX - shouldn't assume Pre!  (If block elements are to be used anywhere besides the beginning of a form)
						io_dfForm.ActionsPre = adaTmp;
					}
					break;
					case "script" :
					{
						dsTmp = new DScript();
						eRes = ParseScript(xeCurr, ref dsTmp);

						daTmp = new DAction();
						daTmp.m_Type = DAction.DActionType.eScript;
						daTmp.m_oValue = dsTmp;

						io_dfForm.ActionsPre.Add(daTmp);								// FIX - shouldn't assume Pre!  (If block elements are to be used anywhere besides the beginning of a form)
					}
					break;
					case "log" :	// FIX - the log element is used in multiple places, so this case should really be using a const string (but then I'd have to refactor all the case statements to keep things consistent)
					{
						daTmp = new DAction();
						ParseLog(xeCurr, ref daTmp);
						io_dfForm.ActionsPre.Add(daTmp);								// FIX - shouldn't assume Pre!  (If block elements are to be used anywhere besides the beginning of a form)
					}
					break;
					case "exit" :
					{
						daTmp = new DAction();
						ParseExit(xeCurr, ref daTmp);
						io_dfForm.ActionsPre.Add(daTmp);								// FIX - shouldn't assume Pre!  (If block elements are to be used anywhere besides the beginning of a form)
					}
					break;
					case "if" :
					{
						adcConds = new DConditions();
						daTmp = new DAction();
						io_dfForm.ActionsPre.Add(daTmp);								// FIX - shouldn't assume Pre!  (If block elements are to be used anywhere besides the beginning of a form)
						daTmp.m_Type = DAction.DActionType.eCondition;
						daTmp.m_oValue = adcConds;

						advTmp = io_dfForm.Variables;
						ParseCondition(xeCurr, ref adcConds, ref advTmp);
						io_dfForm.Variables = advTmp;
					}
					break;
					case "goto":
					{
						daTmp = new DAction();
						ParseGoto(xeCurr, ref daTmp);
						io_dfForm.ActionsPre.Add(daTmp);								// FIX - shouldn't assume Pre!  (If block elements are to be used anywhere besides the beginning of a form)
					}
					break;
					default :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <block> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
					}
					break;
				} // switch

				// If the parse result is not successful for any of the elements, return that error.
				if(eRes != eParseError.eSuccess)
				{
					eRet = eRes;
				}
			} // for

			// Check if there is a value, if so TTS it.
			if(i_xeElem.Value != null)
			{
				if(i_xeElem.Value.Length > 0)
				{
					daTmp = new DAction();
					dpTmp = new DPrompt();
					dpTmp.m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eTTS_Text;	// Default to TTS, if 'src' below, set to WAV.
					dpTmp.m_sText = i_xeElem.Value;
					daTmp.m_oValue = dpTmp;
					daTmp.m_Type = DAction.DActionType.ePlayPrompt;

					io_dfForm.ActionsPre.Add(daTmp);
				}
			}

			return(eRet);
		}

		public eParseError ParseBlockAttrs(List<XAttribute> i_Attrs, ref DForm io_dfForm)
		{
			eParseError		eRet = eParseError.eSuccess;
			XAttribute		xaCurr = null;
			int				ii, iSize;

			iSize = i_Attrs.Count;
			for(ii = 0; ii < iSize; ii++)
			{
				xaCurr = i_Attrs[ii];
				switch(xaCurr.Name)
				{
					case "name" :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <block> element (Value of '{0}' = '{1}',  Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
					}
					break;
					case "expr" :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <block> element (Value of '{0}' = '{1}',  Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
					}
					break;
					case "cond" :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <block> element (Value of '{0}' = '{1}',  Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
					}
					break;
					default :
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <block> element (Value of '{0}' = '{1}',  Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
					}
					break;
				} // switch
			} // for

			return(eRet);
		}

		public DResponse FindOptionByName(string i_sOptionName, DField i_Field)
		{
			int			ii, iNumOptions;
			bool		bFound = false;
			DResponse	drRet = null, drTmp = null;

			// Find an option/DResponse matching the condition string
			iNumOptions = i_Field.m_Options.Count;
			for(ii = 0, bFound = false; ( (ii < iNumOptions) && (!bFound) ); ii++)
			{
				drTmp = (DResponse)i_Field.m_Options[ii];
				if(drTmp.m_sValue == i_sOptionName)
				{
					bFound = true;
					drRet = drTmp;
				}
			}

			if(!bFound)
			{
				drRet = null;
			}

			return(drRet);
		}

		public eParseError ParsePrompt(XElement i_xeElem, ref DActions io_DActions)
		{
			eParseError		eRet = eParseError.eSuccess;
			XElement		xeCurr = null;
			XAttribute		xaCurr = null;
			int				ii = 0, iNumElems = 0, iNumAttrs = 0;
			DAction			daTmp = null;
			bool			bBargein = true;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "bargein" :
						{
							if(xaCurr.Value.ToLower() == "false")
							{
								bBargein = false;
							}
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <prompt> element (Value of '{0}' = '{1}',  Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				// Parse elements
				iNumElems = i_xeElem.m_alElements.Count;
				if(iNumElems < 1)
				{
					m_Logger.Log(Level.Info, String.Format("VXML WARNING - <prompt> element is empty - this is probably an error in the VXML file. (Near line {0} in file '{1}').", i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
					eRet = eParseError.eSyntaxError;
				}
				else
				{
					for(ii = 0; ii < iNumElems; ii++)
					{
						xeCurr = i_xeElem.m_alElements[ii];

						if(xeCurr.Name == "audio")
						{
							daTmp = new DAction();
							io_DActions.Add(daTmp);

							ParseAudio(xeCurr, ref daTmp, bBargein);
						}
						else
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <prompt> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
							eRet = eParseError.eSyntaxError;
						}
					} // for
				}
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParsePrompt caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParsePrompt

		public eParseError ParseTransfer(XElement i_xeElem, ref DActions io_DActions)
		{
			eParseError		eRet = eParseError.eSuccess;
			XElement		xeCurr = null;
			XAttribute		xaCurr = null;
			int				ii = 0, iNumElems = 0, iNumAttrs = 0;
			DAction			daTmpTrans = null;
			string			sTmp;
			int				iStart;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "dest" :
						{
							daTmpTrans = new DAction();
							daTmpTrans.m_Type = DAction.DActionType.eCC_TransferSession;
							daTmpTrans.m_oValue = xaCurr.Value;

							// Check for variable
							sTmp = daTmpTrans.m_oValue.ToString();
							iStart = sTmp.IndexOf("sip:");
							if(iStart >= 0)
							{
								iStart += 4;
								sTmp = sTmp.Substring(iStart, sTmp.Length - iStart);
								if(IsVariableName(sTmp))
								{
									daTmpTrans.m_oValue = "sip:" + (Object)TranslateVariableName(m_Logger, sTmp);
								}
							}
							// Hold on to transfer info, push it on last.  (We want prompts to go out first, may need to fix this implementation later.)
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <transfer> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				// Parse elements
				iNumElems = i_xeElem.m_alElements.Count;
				if(iNumElems < 1)
				{
					m_Logger.Log(Level.Info, String.Format("VXML WARNING - <transfer> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
					eRet = eParseError.eSyntaxError;
				}
				else
				{
					for(ii = 0; ii < iNumElems; ii++)
					{
						xeCurr = i_xeElem.m_alElements[ii];

						if(xeCurr.Name == "prompt")
						{
							ParsePrompt(xeCurr, ref io_DActions);
						}
						else
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <transfer> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber,xeCurr.SourceFileName));
							eRet = eParseError.eSyntaxError;
						}
					} // for
				}

				// Push transfer info on last.
				io_DActions.Add(daTmpTrans);
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseTransfer caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseTransfer

		public eParseError ParseVar(XElement i_xeElem, ref DVariable io_DVariable)
		{
			eParseError		eRet = eParseError.eSuccess;
			int				ii, iNumAttrs;
			XAttribute		xaCurr;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "name" :
						{
							io_DVariable.Name = xaCurr.Value;
						}
						break;
						case "expr" :
						{
							io_DVariable.SValue = xaCurr.Value;
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <var> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseVar caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseVar

		public eParseError ParseValue(XElement i_xeElem, ref string io_sValue)
		{
			eParseError		eRet = eParseError.eSuccess;
			int				ii, iNumAttrs;
			XAttribute		xaCurr;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "expr" :
						{
							io_sValue = s_sVariableTag + xaCurr.Value;		// FIX - This convention will cause problems with other JS interpreters

							if (String.Empty == xaCurr.Value.Trim())
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'expr' attribute of <value> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
							}
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <value> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				// Ever any elements to parse?
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseValue caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseValue

		public eParseError ParseGoto(XElement i_xeElem, ref DAction io_DAction)
		{
			eParseError		eRet = eParseError.eSuccess;
			int				ii, iNumAttrs;
			XAttribute		xaCurr;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "next" :
						{
							io_DAction.m_Type = DAction.DActionType.eLink;
							io_DAction.m_oValue = xaCurr.Value;

                            if (String.Empty == xaCurr.Value.Trim())
                            {
                                m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'next' attribute of <goto> element is empty - this is probably an error in the VXML file (Near line {0} of file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
                            }
                        }
						break;
						case "expr" :
						{
							io_DAction.m_Type = DAction.DActionType.eLinkExpr;
							io_DAction.m_oValue = xaCurr.Value;

							if (String.Empty == xaCurr.Value.Trim())
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'expr' attribute of <goto> element is empty - this is probably an error in the VXML file (Near line {0} of file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
							}
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <goto> element (Value of '{0}' = '{1}', Near line {2} of file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				// Ever any elements to parse?
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseGoto caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseGoto

		public eParseError ParseExit(XElement i_xeElem, ref DAction io_DAction)
		{
			eParseError		eRet = eParseError.eSuccess;

			try
			{
				// FIX - Parse for "expr" and "namelist" attributes.
				io_DAction.m_Type = DAction.DActionType.eCC_HangupAfterPrompts;
				io_DAction.m_oValue = new object();		// Will an "object" be enough?
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseExit caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseExit

		public eParseError ParseAudio(XElement i_xeElem, ref DAction io_DAction, bool i_bBargeinEnabled)
		{
			eParseError		eRet = eParseError.eSuccess;
			XElement		xeSub = null;
			XAttribute		xaCurr = null;
			int				ii = 0, iNumSubElems = 0, jj = 0, iNumAttrs = 0;
			DPrompt			dpTmp = null;

			try
			{
				dpTmp = new DPrompt();
				dpTmp.m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eTTS_Text;	// Default to TTS, override below depending on which attributes are present.
				dpTmp.m_bBargeinEnabled = i_bBargeinEnabled;


				io_DAction.m_oValue = dpTmp;
				io_DAction.m_Type = DAction.DActionType.ePlayPrompt;

				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "src" :
						{
							dpTmp.m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eWavFilePath;
							dpTmp.m_oValue = xaCurr.Value;

							if (String.Empty == xaCurr.Value.Trim())
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'scr' attribute of <audio> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
							}
						}
						break;
						case "expr" :
						{
							dpTmp.m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eEval;
							dpTmp.m_oValue = xaCurr.Value;

							if (String.Empty == xaCurr.Value.Trim())
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'expr' attribute of <audio> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
							}
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <audio> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				if ((null != i_xeElem.Value) && (i_xeElem.m_alElements.Count > 0))
				{
					m_Logger.Log(Level.Info, String.Format("VXML WARNING - <audio> element contains both text AND <value> elements.  This is currently not supported and will result in the <value> element overriding any text present (Text: '{0}', Near line {1} in file '{2}').", i_xeElem.Value, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
				}

				// Assign the text, if any.
				if(i_xeElem.Value != null)
				{
					dpTmp.m_sText = i_xeElem.Value;
				}

				// If there are subelements, it probably includes a value expression
				iNumSubElems = i_xeElem.m_alElements.Count;
				for (jj = 0; jj < iNumSubElems; jj++)
				{
					xeSub = i_xeElem.m_alElements[jj];
					switch(xeSub.Name)
					{
						case "value" :
						{
							string		sVal = "";
							ParseValue(xeSub, ref sVal);
							if(IsVariableName(sVal))
							{
								sVal = TranslateVariableNameInPlace(m_Logger, sVal);							//$$$ LP - // FIX - THIS CODE SEEMS TO GO OFF INTO INFINITE RECURSION
								if(dpTmp.m_sText.Length == 0)
								{
									dpTmp.m_sText = sVal;
								}
								else
								{
									m_Logger.Log(Level.Warning, "VoiceXmlParser.ParseAudio() - string wasn't empty before finding Value. ");
									dpTmp.m_sText += sVal;
								}
							}
							else
							{
								dpTmp.m_sText = sVal;
							}
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <audio> element (Near line {1} in file '{2}').", xeSub.Name, xeSub.ApproximateLineNumber, xeSub.SourceFileName));
						}
						break;
					}
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseAudio caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseAudio

		/// <summary>
		/// FIX - This code should be extended and used in ParseFieldFilledIf.
		/// </summary>
		/// <param name="i_xeElem"></param>
		/// <param name="io_DCondition"></param>
        /// <param name="io_Vars"></param>
		/// <returns></returns>
		public eParseError ParseCondition(XElement i_xeElem, ref DConditions io_DConditions, ref DVariables io_Vars)
		{
			eParseError		eRet = eParseError.eSuccess;
			eParseError		eRes = eParseError.eSuccess;
			int				ii, iNumAttrs, iNumElems;
			XAttribute		xaCurr;
			XElement		xeCurr;
			DCondition		dcTmp;
			DAction			daTmp;
			DVariable		dvTmp;
			DScript			dsTmp;
			DConditions		adcTmp;

			try
			{
				// Create initial condition
				dcTmp = new DCondition();
				io_DConditions.Add(dcTmp);

				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "cond" :
						{
							dcTmp.m_sStatement = xaCurr.Value;
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <if> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				if(dcTmp.m_sStatement.Length <= 0)
				{
					eRet = eParseError.eSyntaxError;
					m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseCondition() - ERROR: IF tag missing 'cond' attribute.");
				}
				else
				{
					// Parse elements
					iNumElems = i_xeElem.m_alElements.Count;
					if(iNumElems < 1)
					{
						m_Logger.Log(Level.Info, String.Format("VXML WARNING - <if> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
						eRet = eParseError.eSyntaxError;
					}
					else
					{
						for(ii = 0; ii < iNumElems; ii++)
						{
							xeCurr = i_xeElem.m_alElements[ii];

							if(xeCurr.Name == "audio")
							{
								daTmp = new DAction();
								dcTmp.m_Actions.Add(daTmp);
								ParseAudio(xeCurr, ref daTmp, true);		// VoiceXML does not allow control of bargein at individual prompt level, only prompt group.
							}
							else if(xeCurr.Name == "prompt")
							{
								ParsePrompt(xeCurr, ref dcTmp.m_Actions);
							}
							else if(xeCurr.Name == "goto")
							{
								daTmp = new DAction();
								dcTmp.m_Actions.Add(daTmp);
								ParseGoto(xeCurr, ref daTmp);
							}
							else if(xeCurr.Name == "elseif")
							{
								dcTmp = new DCondition();
								io_DConditions.Add(dcTmp);
								dcTmp.m_sStatement = xeCurr.GetAttributeVal("cond");
							}
							else if(xeCurr.Name == "else")
							{
								// Default's "statement" string is left as the empty string.
								dcTmp = new DCondition();
								io_DConditions.Add(dcTmp);
							}
							else if(xeCurr.Name == "transfer")
							{
								ParseTransfer(xeCurr, ref dcTmp.m_Actions);
							}
							else if(xeCurr.Name == "var")
							{
								dvTmp = new DVariable();
								io_Vars.Add(dvTmp);
								ParseVar(xeCurr, ref dvTmp);
							}
							else if(xeCurr.Name == "script")
							{
								dsTmp = new DScript();
								eRes = ParseScript(xeCurr, ref dsTmp);
								if(eRes == eParseError.eSuccess)
								{
									daTmp = new DAction();
									dcTmp.m_Actions.Add(daTmp);
									daTmp.m_Type = DAction.DActionType.eScript;
									daTmp.m_oValue = dsTmp;
								}
								else
								{
									// ParseScript should have logged an error already.
								}
							}
							else if(xeCurr.Name == "exit")
							{
								daTmp = new DAction();
								dcTmp.m_Actions.Add(daTmp);
								ParseExit(xeCurr, ref daTmp);
							}
							else if(xeCurr.Name == "if")
							{
								adcTmp = new DConditions();		// FIX - Assign this to local element, this is just for testing now.
								eRes = ParseCondition(xeCurr, ref adcTmp, ref io_Vars);
								if(eRes == eParseError.eSuccess)
								{
									daTmp = new DAction();
									dcTmp.m_Actions.Add(daTmp);
									daTmp.m_Type = DAction.DActionType.eCondition;
									daTmp.m_oValue = adcTmp;
								}
								else
								{
									// ParseCondition should have logged an error already
								}
							}
							else if(xeCurr.Name == "log")	// FIX - the log element is used in multiple places, so this case should really be using a const string (but then I'd have to refactor all the case statements to keep things consistent)
							{
								// FIX - Test this carefully, it may not work as intended due to the way conditions are built
								daTmp = new DAction();
								ParseLog(xeCurr, ref daTmp);
								dcTmp.m_Actions.Add(daTmp);
							}
							else
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported <{0}> element encountered in <if> element (Near line {1} in file '{2}').", xeCurr.Name, xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
								eRet = eParseError.eSyntaxError;	// Always necessary?
							}
						} // for
					} // else
				} // else
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseCondition caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseCondition

		public eParseError ParseScript(XElement i_xeElem, ref DScript io_DScript)
		{
			eParseError		eRet = eParseError.eSuccess;
			int				ii, iNumAttrs;
			XAttribute		xaCurr;
			string			sCode, sLine;
			int				iEnd;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						case "src" :
						{
							io_DScript.Src = xaCurr.Value;

							if (String.Empty == xaCurr.Value.Trim())
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'scr' attribute of <script> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
							}
						}
						break;
						case "language" :		// Not specified in VoiceXML 2.0
						{
							io_DScript.Language = xaCurr.Value;
						}
						break;
						case "type" :			// Not specified in VoiceXML 2.0
						{
							io_DScript.Type = xaCurr.Value;
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <script> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}

				// Ever any elements to parse?

				// Parse code lines from the 'script' element's value.
				// Lazy parsing for now, just look for ';'.  // FIX - This obviously won't work for more complex script, but I'm hoping the mono javascript engine will be usable soon.

				if (null == i_xeElem.Value)
				{
					m_Logger.Log(Level.Info, String.Format("VXML WARNING - <script> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
				}
				else
				{
					sCode = i_xeElem.Value.Trim();
					while (sCode.Length > 0)
					{
						iEnd = sCode.IndexOf(';');
						if (iEnd >= 0)
						{
							sLine = sCode.Substring(0, iEnd + 1);
							if (!sLine.StartsWith("//"))
							{
								//sLine = TranslateVariableNameInPlace(sLine);		// Leave variable lookup/translation for execution.  // FIX?
								io_DScript.Code.Add(sLine);
							}

							sCode = sCode.Substring(iEnd + 1).Trim();
						}
						else
						{
							sLine = sCode;
							io_DScript.Code.Add(sLine);
							sCode = "";
						}
					}
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseScript caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseScript

		public eParseError ParseGrammar(XElement i_xeElem, ref DGrammar io_DGram)
		{
			eParseError		eRet = eParseError.eSuccess;
			int				ii, iNumAttrs;
			XAttribute		xaCurr;

			try
			{
				// Parse attributes
				iNumAttrs = i_xeElem.m_alAttributes.Count;
				for(ii = 0; ii < iNumAttrs; ii++)
				{
					xaCurr = i_xeElem.m_alAttributes[ii];
					switch(xaCurr.Name)
					{
						// SRGS defined attributes
						case "version" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "xml:lang" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "mode" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "root" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "tag-format" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "xml:base" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						// VoiceXML 2.0 defined attributes
						case "src" :
						{
							io_DGram.Grammar = xaCurr.Value;

							if (String.Empty == xaCurr.Value.Trim())
							{
								m_Logger.Log(Level.Info, String.Format("VXML WARNING - 'scr' attribute of <grammar> element is empty - this is probably an error in the VXML file (Near line {0} in file '{1}').", xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
							}
						}
						break;
						case "scope" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "type" :
						{
							if(xaCurr.Value == "application/srgs")
							{
								io_DGram.Type = DGrammar.eType.ABNF;
							}
							else if(xaCurr.Value == "application/srgs+xml")
							{
								io_DGram.Type = DGrammar.eType.GrXML;
							}
							else
							{
								io_DGram.Type = DGrammar.eType.Unknown;
							}
						}
						break;
						case "weight" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "fetchhint" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "fetchtimeout" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "maxage" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						case "maxstale" :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - '{0}' attribute not implemented for <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
						default :
						{
							m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <grammar> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
						}
						break;
					}
				}
			}
			catch(Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseGrammar caught exception: " + exc.ToString());
			}

			return(eRet);
		} // ParseGrammar

		/// <summary>
		/// Parses the element and returns a DAction containing a string to be logged.
		/// The "expr" and "label" attributes are optional according to the spec, and not parsed (as of 2013-12-4)
		/// </summary>
		/// <param name="i_xeElem"></param>
		/// <param name="io_DAction"></param>
		/// <returns></returns>
		public eParseError ParseLog(XElement i_xeElem, ref DAction io_DAction)
		{
			eParseError				eRet = eParseError.eSuccess;
			StringBuilder			sbTmp = null;
			int						ii = 0, iNumElems = -1, iNumAttrs = -1;
			XElement				xeCurr = null;

			try
			{
				io_DAction.m_Type = DAction.DActionType.eLog;

				sbTmp = new StringBuilder();
				sbTmp.Append(i_xeElem.Value);

				// Parse elements
				iNumElems = i_xeElem.m_alElements.Count;
				if(iNumElems < 1)
				{
					// Not an error, just doesn't have any sub-elements (i.e., just contains a single string.)
				}
				else
				{
					// The sub-elements will contain the <value>s to lookup
					for(ii = 0; ii < iNumElems; ii++)
					{
						xeCurr = i_xeElem.m_alElements[ii];

						// Add the xml attribute before the value of the xml element
						// There is only one allowable xml element name: "value"
						if(xeCurr.Name == "value")
						{
							iNumAttrs = xeCurr.m_alAttributes.Count;
							// It should only have one attribute, named "expr"
							if(iNumAttrs < 1)
							{
								m_Logger.Log(Level.Exception, String.Format("VXML ERROR - A <log>'s <value> element must have one attribute.  Near line {0} in file '{1}').", xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
							}
							else
							{
								if(iNumAttrs > 1)
								{
									m_Logger.Log(Level.Warning, String.Format("VXML WARNING - A <log>'s <value> element can only have one attribute.  Near line {0} in file '{1}').", xeCurr.ApproximateLineNumber, xeCurr.SourceFileName));
								}
								sbTmp.AppendFormat("{0}{1}", s_sVariableTag, xeCurr.m_alAttributes[0].Value);		// "Variable" marker so it is properly evaluated later.  FIX - This convention will cause problems with other JS interpreters
							}
						}

						// Add the value of the xml element
						if( (xeCurr.Value != null) && (xeCurr.Value.Length > 0) )
						{
							sbTmp.Append(xeCurr.Value);
						}
					}
				}

				io_DAction.m_oValue = sbTmp.ToString();
			}
			catch (Exception exc)
			{
				eRet = eParseError.eException;
				m_Logger.Log(Level.Exception, "VoiceXmlParser.ParseLog caught exception: " + exc.ToString());
			}

			return (eRet);
		} // ParseLog


        /// <summary>
        /// Parses the element and returns a DProperty containing name and value of property.
        /// </summary>
        /// <param name="i_xeElem"></param>
        /// <param name="io_DProperty"></param>
        /// <returns></returns>
        public eParseError ParseProperty(XElement i_xeElem, ref DProperty io_DProperty)
        {
            eParseError eRet = eParseError.eUnknown;

            try
            {
                string sName = "";
                string sValue = "";

                // Parse attributes
                foreach (XAttribute xaCurr in i_xeElem.m_alAttributes)
                {
                    switch (xaCurr.Name)
                    {
                        case "name":
                            sName = xaCurr.Value.Trim();
                            break;

                        case "value":
                            sValue = xaCurr.Value.Trim();
                            break;

                        default:
                            m_Logger.Log(Level.Info, String.Format("VXML WARNING - Ignoring unsupported '{0}' attribute of <property> element (Value of '{0}' = '{1}', Near line {2} in file '{3}').", xaCurr.Name, xaCurr.Value, xaCurr.ApproximateLineNumber, xaCurr.SourceFileName));
                            break;
                    }
                }

                if (sName == "")
                {
                    m_Logger.Log(Level.Exception, String.Format("VXML ERROR - A <property> element must have a non-empty name attribute (Near line {0} in file '{1}').", i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                }
                else
                {
                    PropertyDefinition propertyDefinition;

                    if (m_KnownProperties.TryGetValue(sName, out propertyDefinition))
                    {
                        bool bValueIsValid = false;

                        switch (propertyDefinition.m_type)
                        {
                            case PropertyDefinition.Type.eIntegerDesignation:
                                bValueIsValid = IsValidIntegerNumber(sValue);

                                if (!bValueIsValid)
                                {
                                    m_Logger.Log(Level.Exception, String.Format("VXML ERROR - The property '{0}' requires an integer number value. (Value = '{1}', Near line {2} in file '{3}').", sName, sValue, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                                }
                                break;

                            case PropertyDefinition.Type.eNotImplemented:
                                m_Logger.Log(Level.Info, String.Format("VXML WARNING - The property '{0}' is not implemented. (Near line {1} in file '{2}').", sName, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                                break;

                            case PropertyDefinition.Type.eRealNumberDesignation:
                                bValueIsValid = IsValidRealNumber(sValue);

                                if (!bValueIsValid)
                                {
                                    m_Logger.Log(Level.Exception, String.Format("VXML ERROR - The property '{0}' requires a real number value. (Value = '{1}', Near line {2} in file '{3}').", sName, sValue, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                                }
                                break;

                            case PropertyDefinition.Type.eString:
                                bValueIsValid = IsValidString(sValue, propertyDefinition.m_AllowedValues);

                                if (!bValueIsValid)
                                {
                                    m_Logger.Log(Level.Exception, String.Format("VXML ERROR - The property '{0}' is set to an invalid value. (Value = '{1}', Near line {2} in file '{3}').", sName, sValue, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                                }
                                break;

                            case PropertyDefinition.Type.eTimeDesignation:
                                bValueIsValid = ConvertTimeDesignationToMilliseconds(ref sValue);

                                if (!bValueIsValid)
                                {
                                    m_Logger.Log(Level.Exception, String.Format("VXML ERROR - The property '{0}' requires a valid time designation. (Value = '{1}', Near line {2} in file '{3}').", sName, sValue, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                                }
                                break;

                            default:
                                m_Logger.Log(Level.Exception, String.Format("VoiceXmlParser.ParseProperty encountered unknown Type ({0}).", propertyDefinition.m_type));
                                break;
                        }

                        if (bValueIsValid)
                        {
                            io_DProperty.Name = sName;
                            io_DProperty.Value = sValue;

                            eRet = eParseError.eSuccess;
                        }
                    }
                    else
                    {
                        m_Logger.Log(Level.Info, String.Format("VXML WARNING - The property '{0}' is not supported. (Near line {1} in file '{2}').", sName, i_xeElem.ApproximateLineNumber, i_xeElem.SourceFileName));
                    }
                }
            }
            catch (Exception exc)
            {
                eRet = eParseError.eException;
                m_Logger.Log(Level.Exception, String.Format("VoiceXmlParser.ParseProperty caught exception: {0}", exc.ToString()));
            }

            return eRet;
        } // ParseProperty


        private bool IsValidIntegerNumber(string i_sValue)
        {
            int iDummy;

            return Int32.TryParse(i_sValue, out iDummy);
        }


        private bool IsValidRealNumber(string i_sValue)
        {
            // Use Decimal since it allows us to specify exactly what is allowed as per the VoiceXML 2.0 specification for a real number.

            Decimal dDummy;

            return Decimal.TryParse(i_sValue, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, null, out dDummy);
        }


        private bool IsValidString(string i_sValue, string[] i_AllowedValues)
        {
            foreach (string allowedValue in i_AllowedValues)
            {
                if (i_sValue == allowedValue)
                {
                    return true;
                }
            }

            return false;
        }


        private bool ConvertTimeDesignationToMilliseconds(ref string io_sValue)
        {
            bool bValidTime = false;

            Regex timeDesignation = new Regex(@"^\+?(?<value>\d*\.?\d*)(?<units>s|ms)$");

            Match match = timeDesignation.Match(io_sValue);

            if (match.Success)
            {
                if (match.Groups["units"].Value == "s")
                {
                    // Don't have to use TryParse() since at this point we know that have what is a valid number so we know that a Parse() is safe.

                    double dValue = Double.Parse(match.Groups["value"].Value);

                    dValue *= 1000;                                 // Convert from seconds to milliseconds.
                    io_sValue = dValue.ToString();
                }
                else
                {
                    io_sValue = match.Groups["value"].Value;
                }


                // Ensure that time in milliseconds has no fractional component.

                io_sValue = io_sValue.Split(new char[] { '.' })[0];

                bValidTime = true;
            }

            return bValidTime;
        }
	} // VoiceXmlParser
}
