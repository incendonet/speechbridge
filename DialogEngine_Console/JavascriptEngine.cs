// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

using DialogModel;
using Incendonet.Utilities.LogClient;
using ISMessaging;


namespace DialogMgr
{
	public static class JavascriptEngine
	{
		public static char[]		s_acOperators =						{'+', '-'};
		public static string		m_csBoolEqual =						"==";
		public static string		m_csBoolGreater =					">";
		public static string		m_csBoolLess =						"<";
		public static string		m_csBoolGreaterEqual =				">=";
		public static string		m_csBoolLessEqual =					"<=";
		public static string		m_csBoolNotEqual =					"!=";

		/// <summary>
		/// Note:  Until javascript in Mono is complete and stable, this will interpret the script code.
		/// For simplicity's sake, the only supported format is:
		///		variable = FullQualifiedObject(param0, param1, ...);
		/// The variable is expected to be in field scope, and is optional.  Parameters can only be of
		/// type string, and are always passed by value.
		/// </summary>
		/// <param name="i_dfActive"></param>
		/// <param name="i_iOptionIndex"></param>
		/// <returns></returns>
//		public static bool ScriptExecute(ILegacyLogger i_Logger, string i_sVmcId, DField i_dfField, DAction i_daAction)
		public static bool ScriptExecute(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, DAction i_daAction)
		{
			bool bRet = true, bRes = true;
			int ii = 0, iIndexStart = 0;
			System.Int32 iTmp = 0;		// I know, same as 'int', but...
			DScript dsCode;
			string sStatement, sVarName, sVarNameShort, sRes = "";
			DVariable dvTmp = null;

			try
			{
				// FIX - The parsing code is currently a hack to get a demo working.
				// FIX - The variables we look for in the javascript may be VoiceXML variables.
				// Right now we only handle two types of statements: variable declarations (with or
				// without assignment) and function calls (w/ or w/out assignment.)  All parameters and
				// return values from function calls are of type string.  Any text results to be TTSed
				// need to be done through the reserved statement 'document.writeln(svar);'.
				// 

				dsCode = (DScript)i_daAction.m_oValue;
				for (ii = 0; ii < dsCode.Code.Count; ii++)
				{
					dvTmp = null;
					sStatement = dsCode.Code[ii];

					// Check if there is a variable declared
					if (sStatement.IndexOf("var ") >= 0)
					{
						sVarName = ScriptExtractVariableName(sStatement.Substring(4));
						iTmp = sVarName.IndexOf('.');
						if (iTmp < 0)
						{
							sVarNameShort = sVarName;
						}
						else
						{
							sVarNameShort = sVarName.Substring(iTmp + 1);
						}

						dvTmp = DialogEngine.DialogEngine.FindVariableByName(i_SubdocContext, sVarName);	// Reuse variable if it exists, even though script has (erroneously) declared a new one.
						if (dvTmp == null)
						{
							// Allocate variable and put it in proper scope.
							dvTmp = new DVariable();
							dvTmp.Name = sVarNameShort;

							iIndexStart = sStatement.IndexOf("var ") + 4;

							if (i_SubdocContext.GetType() == typeof(DField))
							{
								DField		dfField = (DField)i_SubdocContext;

								if (sVarName.IndexOf("session.") >= 0)
								{
									dfField.m_dfParentForm.m_ddParentDocument.m_dsParentSession.m_DVariables.Add(dvTmp);
								}
								else if (sVarName.IndexOf("application.") >= 0)
								{
									// FIX - Keep it in session since we don't currently have an "application" in USCs?
									dfField.m_dfParentForm.m_ddParentDocument.m_dsParentSession.m_DVariables.Add(dvTmp);
								}
								else if (sVarName.IndexOf("document.") >= 0)
								{
									dfField.m_dfParentForm.m_ddParentDocument.m_DVariables.Add(dvTmp);
								}
								else if (sVarName.IndexOf("dialog.") >= 0)
								{
									dfField.m_dfParentForm.Variables.Add(dvTmp);
								}
								else
								{
									i_SubdocContext.Variables.Add(dvTmp);
								}
							}
							else if(i_SubdocContext.GetType() == typeof(DForm))
							{
								DForm		dfForm = (DForm)i_SubdocContext;

								if (sVarName.IndexOf("session.") >= 0)
								{
									dfForm.m_ddParentDocument.m_dsParentSession.m_DVariables.Add(dvTmp);
								}
								else if (sVarName.IndexOf("application.") >= 0)
								{
									// FIX - Keep it in session since we don't currently have an "application" in USCs?
									dfForm.m_ddParentDocument.m_dsParentSession.m_DVariables.Add(dvTmp);
								}
								else if (sVarName.IndexOf("document.") >= 0)
								{
									dfForm.m_ddParentDocument.m_DVariables.Add(dvTmp);
								}
								else if (sVarName.IndexOf("dialog.") >= 0)
								{
									dfForm.Variables.Add(dvTmp);
								}
								else
								{
									i_SubdocContext.Variables.Add(dvTmp);
								}
							}
							else
							{
								i_Logger.Log(Level.Exception, string.Format("[{0}]DialogEngine.ScriptExecute - invalid type '{1}'.", i_sVmcId, i_SubdocContext.GetType().ToString()));
							}
						}
					}
					else if (sStatement.IndexOf(m_csBoolEqual) >= 0)
					{
						i_Logger.Log(Level.Warning, "[" + i_sVmcId + "]" + "DialogEngine.ScriptExecute - Boolean conditions not yet supported.");
					}
					else if (sStatement.IndexOf('=') >= 0)
					{
						sVarName = sStatement.Substring(0, sStatement.IndexOfAny(DialogEngine.VoiceXmlParser.s_acBreakNonVar));
						dvTmp = DialogEngine.DialogEngine.FindVariableByName(i_SubdocContext, sVarName);
						if (dvTmp == null)
						{
							i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptExecute - Variable named '" + sVarName + "' was not found, line " + ii + " '" + sStatement + "'.");
							// Create a new temporary variable so we can keep going
							// FIX - probably ought to return a failure if we get here...
							dvTmp = new DVariable();
							dvTmp.Name = sVarName;
						}
					}
					else
					{
						// No variable assigned to the return value, so create a temporary variable
						dvTmp = new DVariable("TEMPORARY", "");
					} // if 'var'

					// Check if there is an assignment
					iIndexStart = sStatement.IndexOf('=');
					if (iIndexStart >= 0)
					{
						iIndexStart++;
						bRes = ScriptAssignVariable(i_Logger, i_sVmcId, i_SubdocContext, dvTmp, sStatement.Substring(iIndexStart).Trim());
					}
					else
					{
						// Check for a function call that doesn't assign a return value to a variable
						if (sStatement.IndexOf('(') >= 0)
						{
							sRes = ScriptCallFunction(i_Logger, i_sVmcId, i_SubdocContext, sStatement.TrimEnd(DialogEngine.VoiceXmlParser.s_acTerminator));
						}
						else
						{
							i_Logger.Log(Level.Info, "[" + i_sVmcId + "]" + "DialogEngine.ExecuteScript - Seem to have found a no-op on line " + ii + ": '" + sStatement + "'.");
						}
					}
				} // for
			}
			catch (Exception e)
			{
				bRet = false;
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptExecute: " + e.ToString());
			}

			return (bRet);
		}

		/// <summary>
		/// Extracts a substring by looking for a terminating char.
		/// Note - Return string is not necessarily a variable name.
		/// </summary>
		/// <param name="i_sStatement"></param>
		/// <returns></returns>
		private static string ScriptExtractVariableName(string i_sStatement)
		{
			string sRet, sTmp;
			int iIndexStop = 0;

			sTmp = i_sStatement.Trim();
			iIndexStop = sTmp.IndexOfAny(DialogEngine.VoiceXmlParser.s_acBreakNonVar);
			if (iIndexStop < 0)
			{
				sRet = sTmp;
			}
			else
			{
				sRet = sTmp.Substring(0, iIndexStop);
			}

			return (sRet);
		}

		public static string ScriptExtractElement(ILegacyLogger i_Logger, string i_sVmcId, string i_sStatement)
		{
			string sRet = "", sTmp = "";
			int ii = 0, iIndexStop = 0;
			bool bDone = false;
			char cQuote;

			try
			{
				sTmp = i_sStatement.Trim();
				if ((sTmp[0] == '\'') || (sTmp[0] == '\"'))
				{
					cQuote = sTmp[0];

					for (ii = 1, bDone = false; ((ii < sTmp.Length) && (!bDone)); ii++)
					{
						if (sTmp[ii] == cQuote)
						{
							iIndexStop = ii;
							bDone = true;
						}
					}

					if (!bDone)
					{
						i_Logger.Log(Level.Warning, "[" + i_sVmcId + "]" + "DialogEngine.ScriptExtractElement - Malformed quoted string '" + i_sStatement + "'.");
						sRet = sTmp;
					}
					else
					{
						//sRet = sTmp.Substring(1, iIndexStop - 1);		// Return string without quotes
						sRet = sTmp.Substring(0, iIndexStop + 1);			// Return string with quotes
					}
				}
				else
				{
					iIndexStop = sTmp.IndexOfAny(DialogEngine.VoiceXmlParser.s_acBreakNonVar);
					if (iIndexStop < 0)
					{
						sRet = sTmp;
					}
					else
					{
						sRet = sTmp.Substring(0, iIndexStop);
					}
				}
			}
			catch (Exception exc)
			{
				sRet = "";
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptExtractElement: " + exc.ToString());
			}

			return (sRet);
		}

//		private static bool ScriptAssignVariable(ILegacyLogger i_Logger, string i_sVmcId, DField i_dfField, DVariable io_dvVar, string i_sStatement)
		private static bool ScriptAssignVariable(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, DVariable io_dvVar, string i_sStatement)
		{
			bool bRet = true, bRes = true;
			int iTmp = 0, iIndexNew = 0, iIndexOpen = 0, iIndexClose = 0;
			string sTmp;

			try
			{
				// Assign variable's value (string, integer or new object)
				iIndexNew = i_sStatement.IndexOf("new ");	// Indicates an object instantiation
				if (iIndexNew >= 0)
				{
					// Object instantiation
					sTmp = i_sStatement.Substring(iIndexNew + 4).Trim();
					sTmp = sTmp.TrimEnd(DialogEngine.VoiceXmlParser.s_acTerminator);
					io_dvVar.OValue = ScriptCreateObject(i_Logger, i_sVmcId, i_SubdocContext, sTmp);
				}
				else
				{
					iIndexOpen = i_sStatement.IndexOf('(');
					if (iIndexOpen >= 0)
					{
						// Function call
						io_dvVar.SValue = ScriptCallFunction(i_Logger, i_sVmcId, i_SubdocContext, i_sStatement.TrimEnd(DialogEngine.VoiceXmlParser.s_acTerminator));
					}
					else
					{	// Variable, string, or integer assignment
						iIndexOpen = i_sStatement.IndexOf('\"');
						if (iIndexOpen >= 0)
						{
							// String literal
							iIndexOpen++;
							iIndexClose = i_sStatement.IndexOf('\"', iIndexOpen);
							io_dvVar.SValue = i_sStatement.Substring(iIndexOpen, iIndexClose - iIndexOpen);
						}
						else
						{
							// Variable or integer
							sTmp = i_sStatement.TrimStart(null).TrimEnd(DialogEngine.VoiceXmlParser.s_acTerminator);
							try
							{
								iTmp = System.Int32.Parse(sTmp);
								bRes = true;
							}
							catch (FormatException)
							{
								bRes = false;
							}
							if (bRes)
							{
								// Integer literal
								io_dvVar.IValue = iTmp;
							}
							else
							{
								// variable to variable assignment
								sTmp = ScriptEvaluateExpression(i_Logger, i_sVmcId, i_SubdocContext, i_sStatement);
								try
								{
									iTmp = System.Int32.Parse(sTmp);
									io_dvVar.IValue = iTmp;
								}
								catch (Exception)
								{
									io_dvVar.SValue = sTmp;
								}
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptAssignVariable: " + exc.ToString());
			}

			return (bRet);
		}

//		private static object ScriptCreateObject(ILegacyLogger i_Logger, string i_sVmcId, DField i_dfField, string i_sStatement)
		private static object ScriptCreateObject(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, string i_sStatement)
		{
			object oRet = null;
			string sObjName = "";
			StringCollection scParams = null;
			int ii, iIndex = 0;
			string sParams = "";
			object[] aoParams = null;
			Type tObj;

			try
			{
				sObjName = ScriptGetFuncName(i_sStatement);

				// Format .NET assembly qualified name
                // The convention required for plugins is that the namespace is the same as the assembly name.
                // Thus, if the namespace is xxx.yyy and the class is zzz then this method is called for a line that reads
                //
                //         variable = new xxx.yyy.zzz();
                //
                // where the class definition is contained in an assembly called xxx.yyy (which means there exists a file called xxx.yyy.dll).
                // In this case ScriptGetFuncName() will return "xxx.yyy.zzz" and thus the namespace (and assembly name) is everything up to the last period.

                iIndex = sObjName.LastIndexOf('.');
				if (iIndex >= 0)
				{
					sObjName = sObjName + "," + sObjName.Substring(0, iIndex);
				}

				iIndex = i_sStatement.IndexOf('(');
				sParams = i_sStatement.Substring((iIndex + 1), (i_sStatement.Length - iIndex - 2));
				scParams = ScriptGetParams(i_Logger, i_sVmcId, i_SubdocContext, sParams);

				if (scParams.Count > 0)
				{
					aoParams = new object[scParams.Count];
				}

				tObj = Type.GetType(sObjName);
				if (tObj == null)
				{
					i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "The type '" + sObjName + "' could not be found.");
				}
				else
				{
					for (ii = 0; ii < scParams.Count; ii++)
					{
						aoParams[ii] = scParams[ii];
					}

					oRet = tObj.InvokeMember("",
						BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance,
						null, null, aoParams);
				}
			}
			catch (Exception exc)
			{
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCreateObject: " + exc.ToString());
			}

			return (oRet);
		} // ScriptCreateObject

//		private static string ScriptEvaluateExpression(ILegacyLogger i_Logger, string i_sVmcId, DField i_dfField, string i_sStatement)
		private static string ScriptEvaluateExpression(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, string i_sStatement)
		{
			string sRet = "", sVar = "";
			DVariable dvTmp = null;
			int iIndex, iTmp;

			try
			{
				// For now, just look for integer increments.
				iIndex = i_sStatement.IndexOfAny(s_acOperators);
				if (iIndex < 0)
				{
					i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Unsupported syntax: '" + i_sStatement + "'.");
				}
				else
				{
					// FIX - Here begins the hack
					iIndex = i_sStatement.IndexOf(" + 1");
					if (iIndex < 0)
					{
						iIndex = i_sStatement.IndexOf(" - 1");
						if (iIndex < 0)
						{
							i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Unsupported syntax: '" + i_sStatement + "'.");
						}
						else
						{
							sVar = ScriptExtractVariableName(i_sStatement);
							dvTmp = DialogEngine.DialogEngine.FindVariableByName(i_SubdocContext, sVar);
							if (dvTmp == null)
							{
								i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Couldn't find variable named '" + sVar + "' in field '" + i_SubdocContext.ID + "'.");
							}
							else
							{
								if (dvTmp.Type == DVariable.eType.USCInt)
								{
									iTmp = dvTmp.IValue;
									iTmp--;
									sRet = iTmp.ToString();
								}
								else if (dvTmp.Type == DVariable.eType.USCString)
								{
									try
									{
										iTmp = System.Int32.Parse(dvTmp.SValue);
										iTmp--;
										sRet = iTmp.ToString();
									}
									catch (Exception)
									{
										i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Unsupported syntax: '" + i_sStatement + "'.");
									}
								}
								else
								{
									i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Unsupported syntax: '" + i_sStatement + "'.");
								}
							}
						}
					}
					else
					{
						sVar = ScriptExtractVariableName(i_sStatement);
						dvTmp = DialogEngine.DialogEngine.FindVariableByName(i_SubdocContext, sVar);
						if (dvTmp == null)
						{
							i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Couldn't find variable named '" + sVar + "' in field '" + i_SubdocContext.ID + "'.");
						}
						else
						{
							if (dvTmp.Type == DVariable.eType.USCInt)
							{
								iTmp = dvTmp.IValue;
								iTmp++;
								sRet = iTmp.ToString();
							}
							else if (dvTmp.Type == DVariable.eType.USCString)
							{
								try
								{
									iTmp = System.Int32.Parse(dvTmp.SValue);
									iTmp++;
									sRet = iTmp.ToString();
								}
								catch (Exception)
								{
									i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Unsupported syntax: '" + i_sStatement + "'.");
								}
							}
							else
							{
								i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression - Unsupported syntax: '" + i_sStatement + "'.");
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptEvaluateExpression: " + exc.ToString());
			}

			return (sRet);
		} // ScriptEvaluateExpression

//		private static string ScriptCallFunction(ILegacyLogger i_Logger, string i_sVmcId, DField i_dfField, string i_sStatement)
		private static string ScriptCallFunction(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, string i_sStatement)
		{
			string sRet = "";
			string sFuncNameFull = "", sInstanceName = "", sFuncName = "", sParams = "";
			StringCollection scParams = null;
			object[] aoParams = null;
			object oRes = null;
			int ii, iIndex = 0;
			DVariable dvObject = null;

			try
			{
				sFuncNameFull = ScriptGetFuncName(i_sStatement);
				iIndex = sFuncNameFull.IndexOf('.');
				if (iIndex < 0)
				{
					i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCallFunction - Can't currently call a non-object based function.  From: '" + i_sStatement + "'.");
				}
				else
				{
					sInstanceName = sFuncNameFull.Substring(0, iIndex);
					sFuncName = sFuncNameFull.Substring(iIndex + 1);

					dvObject = DialogEngine.DialogEngine.FindVariableByName(i_SubdocContext, sInstanceName);
					if (dvObject == null)
					{
						i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCallFunction - Can't find instance variable '" + sInstanceName + "'.");
					}
					else
					{

						iIndex = i_sStatement.IndexOf('(');
						sParams = i_sStatement.Substring((iIndex + 1), (i_sStatement.Length - iIndex - 2));
						scParams = ScriptGetParams(i_Logger, i_sVmcId, i_SubdocContext, sParams);

						if (scParams.Count > 0)
						{
							aoParams = new object[scParams.Count];
							for (ii = 0; ii < scParams.Count; ii++)
							{
								aoParams[ii] = scParams[ii];
							}
						}

						try
						{
							oRes = dvObject.OValue.GetType().InvokeMember(sFuncName,
								BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod,
								null, dvObject.OValue, aoParams);
							if (oRes == null)
							{
								i_Logger.Log(Level.Warning, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCallFunction - There was no return value in the call to '" + i_sStatement + "'.");
							}
							else
							{
								sRet = oRes.ToString();
							}
						}
						catch (TargetInvocationException exc)
						{
							i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCallFunction: " + exc.ToString());
						}
						catch (Exception exc)
						{
							i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCallFunction: " + exc.ToString());
						}
					}
				}
			}
			catch (Exception exc)
			{
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptCallFunction: " + exc.ToString());
			}

			return (sRet);
		}

		private static string ScriptGetFuncName(string i_sStatement)
		{
			string sRet = "";
			int iIndex = 0;

			iIndex = i_sStatement.IndexOf('(');
			sRet = i_sStatement.Substring(0, iIndex);

			return (sRet);
		}

//		private static StringCollection ScriptGetParams(ILegacyLogger i_Logger, string i_sVmcId, DField i_dfField, string i_sParams)
		private static StringCollection ScriptGetParams(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, string i_sParams)
		{
			StringCollection scRet = null;
			int iIndex = 0;
			string sParams = "", sTmp = ""; ;

			try
			{
				sParams = i_sParams;
				scRet = new StringCollection();

				while (sParams.Length > 0)
				{
					iIndex = sParams.IndexOf(',');
					if (iIndex < 0)
					{
						sTmp = sParams;
						sParams = "";
					}
					else
					{
						sTmp = sParams.Substring(0, iIndex);
						sParams = sParams.Substring(iIndex + 1).Trim();
					}

					sTmp.Trim();
					sTmp = ScriptExtractValue(i_Logger, i_sVmcId, i_SubdocContext, sTmp);

					scRet.Add(sTmp);
				}
			}
			catch (Exception exc)
			{
				i_Logger.Log(Level.Exception, "[" + i_sVmcId + "]" + "DialogEngine.ScriptGetParams: " + exc.ToString());
			}

			return (scRet);
		}

		public static string ScriptExtractValue(ILegacyLogger i_Logger, string i_sVmcId, ISubdocContext i_SubdocContext, string i_sString)
		{
			string sRet = "";

			// FIX - Should also check for integers
			if ((i_sString[0] == '\"') || (i_sString[0] == '\''))
			{
				sRet = ScriptExtractStringValue(i_sString);
			}
			else
			{
                DVariable dvTmp = DialogEngine.DialogEngine.FindVariableByName(i_SubdocContext, i_sString);

                if (dvTmp == null)
                {
                    i_Logger.Log(Level.Exception, String.Format("[{0}]DialogEngine.ScriptExtractValue - Couldn't find variable named '{1}' in {2} '{3}'.", i_sVmcId, i_sString, ((i_SubdocContext.GetType() == typeof(DForm)) ? "form" : "field"), i_SubdocContext.ID));
                }
                else
                {
                    sRet = dvTmp.SValue;
                }
			}

			return sRet;
		}

		/// <summary>
		/// Assumes that i_sString begins and ends with double quotes.
		/// </summary>
		/// <param name="i_sString"></param>
		/// <returns></returns>
		private static string ScriptExtractStringValue(string i_sString)
		{
			return (i_sString.Substring(1, i_sString.Length - 2));
		}

	} // class JavascriptEngine
}
