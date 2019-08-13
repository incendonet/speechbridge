// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using NSpring.Logging;


namespace Incendonet.Utilities.LogClient
{
	using ParamCollection = List<LoggerCore.Param>;

	/// <summary>
	/// The format of the columns will be:  DateTime, IpAddress, SessionId, ComponentName, VmcId, ThreadId, Msg
	/// </summary>
	public class TsvAndStdoutLogger : LoggerCore, ILogger
	{
		private NSpring.Logging.Loggers.ConsoleLogger		m_LogConsole = null;
		private NSpring.Logging.Loggers.FileLogger			m_LogFile = null;
		private NSpring.Logging.Loggers.CompositeLogger		m_Logger = null;

		private string										m_sLastError = "";
		private int											m_iLastError = 0;
//		private StackTrace									m_StackTrace = null;

		public enum eMoreParams
		{
			Path
		}

		protected string									m_sPath =			"";

		/// <summary>
		/// 
		/// </summary>
		~TsvAndStdoutLogger()
		{
			Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iLastError"></param>
		/// <param name="i_sLastError"></param>
		protected virtual void SetLastError(int i_iLastError, string i_sLastError)
		{
			m_iLastError = i_iLastError;
			if ((i_sLastError == null) || (i_sLastError.Length <= 0))
			{
				m_sLastError = "";
			}
			else
			{
				m_sLastError = i_sLastError;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Params"></param>
		/// <returns></returns>
		public override bool Init(ref ParamCollection i_Params)
		{
			bool					bRet = true;

			base.Init(ref i_Params);

			SetLastError(0, "");
			m_ParamCollection = i_Params;

			try
			{
				// Pull the additional params from the collection to the member variables, for easier access later.
				m_sPath = i_Params.Find(param => param.GetName() == eMoreParams.Path.ToString()).GetValue();
			}
			catch
			{
				bRet = false;		// Unnecessary if throwing an exception
				SetLastError(-1, "TsvAndStdoutLogger.Init: The parameter 'Path' was not specified in i_Params.");
				throw (new ArgumentException(m_sLastError));
			}

			return (bRet);
		} // Init

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sName"></param>
		/// <returns></returns>
		private string StripExtension(string i_sName)
		{
			string			sRet = "";

			if ( (i_sName != null) && (i_sName.Length > 0) )
			{
				// Strip trailing ".exe" or ".dll"
				if ((i_sName.EndsWith(".exe")) || (i_sName.EndsWith(".dll")))
				{
					sRet = i_sName.Remove(i_sName.Length - 4);
				}
				else
				{
					sRet = i_sName;
				}
			}

			return (sRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Open()
		{
			bool bRet = true;

			try
			{
                string sComponentName = "";
                LoggerThreadInfo oInfo = null;

                ThreadInfoDictSingleton oDict = ThreadInfoDictSingleton.GetInstance();
				bool bRes = oDict.TryGetValue(Thread.CurrentThread.Name, out oInfo);
				if (bRes)
				{
					sComponentName = oInfo.m_sComponentName;
				}

				if (sComponentName == "")
				{
                    StackTrace oST = new StackTrace();
                    sComponentName = oST.GetFrame(2).GetMethod().Module.Name;
				}
				sComponentName = StripExtension(sComponentName);

                string sLogFileTemplate = ConfigurationManager.AppSettings["LogFileTemplate"];

                if (String.IsNullOrEmpty(sLogFileTemplate))
                {
                    sLogFileTemplate = "{yyyy}{mm}{dd}";
                }

                string sFullyQualifiedLogFileName = Path.Combine(m_sPath, String.Format("{0}_{1}.log.txt", sComponentName, sLogFileTemplate));

				m_LogConsole = NSpring.Logging.Logger.CreateConsoleLogger("{timeStamp} {msg}");

                m_LogFile = NSpring.Logging.Logger.CreateFileLogger(sFullyQualifiedLogFileName, "{timeStamp}\t{msg}");
                m_LogFile.Encoding = new UTF8Encoding(false);                // Don't emit UTF-8 BOM to log file.

				m_Logger = NSpring.Logging.Logger.CreateCompositeLogger(m_LogConsole, m_LogFile);
				m_Logger.IsBufferingEnabled = true;
				m_Logger.BufferSize = 10000;			// How much to buffer up before writing out
				m_Logger.AutoFlushInterval = 2000;		// FIX - read from a config file.
				m_Logger.Open();
			}
			catch (Exception exc)
			{
				bRet = false;
				SetLastError(-1, "Caught exception in TsvAndStdoutLogger.Open:  " + exc.ToString());
			}

			return bRet;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Close()
		{
			bool bRet = true;

			if (m_Logger != null)
			{
				m_Logger.Close();
				m_Logger = null;
			}

			return (bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Level"></param>
		/// <returns></returns>
		protected eSeverity ConvertNSLevelToSeverity(NSpring.Logging.Level i_Level)
		{
			eSeverity				eRet = m_FilterLevel;

			SetLastError(0, "");

			if (i_Level == NSpring.Logging.Level.All)
			{
				eRet = eSeverity.Debug;
			}
			else if (i_Level == NSpring.Logging.Level.Config)
			{
				eRet = eSeverity.Debug;
			}
			else if (i_Level == NSpring.Logging.Level.Debug)
			{
				eRet = eSeverity.Debug;
			}
			else if (i_Level == NSpring.Logging.Level.Exception)
			{
				eRet = eSeverity.Error;
			}
			else if (i_Level == NSpring.Logging.Level.Info)
			{
				eRet = eSeverity.Info;
			}
			else if (i_Level == NSpring.Logging.Level.None)
			{
				eRet = eSeverity.Crash;
			}
			else if (i_Level == NSpring.Logging.Level.Verbose)
			{
				eRet = eSeverity.Debug;
			}
			else if (i_Level == NSpring.Logging.Level.Warning)
			{
				eRet = eSeverity.Warning;
			}
			// `else` not needed, default assigned above.

			return (eRet);
		} // ConvertNSLevelToSeverity

		protected NSpring.Logging.Level ConvertSeverityToNSLevel(eSeverity i_Severity)
		{
			NSpring.Logging.Level	lRet = NSpring.Logging.Level.Debug;

			SetLastError(0, "");

			switch (i_Severity)
			{
				case eSeverity.Crash :
					lRet = NSpring.Logging.Level.Exception;
					break;
				case eSeverity.Critical :
					lRet = NSpring.Logging.Level.Exception;
					break;
				case eSeverity.Debug :
					lRet = NSpring.Logging.Level.Debug;
					break;
				case eSeverity.Error :
					lRet = NSpring.Logging.Level.Exception;
					break;
				case eSeverity.Info :
					lRet = NSpring.Logging.Level.Info;
					break;
				case eSeverity.Warning :
					lRet = NSpring.Logging.Level.Warning;
					break;
				default :
					// Default assigned above
					break;
			}

			return (lRet);
		} // ConvertSeverityToLevel

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sMsg"></param>
		/// <param name="o_sVmc"></param>
		/// <param name="o_sThread"></param>
		/// <param name="o_sRemainder"></param>
		/// <returns></returns>
		private bool ExtractIndexesFromMsg(string i_sMsg, out string o_sVmc, out string o_sThread, out string o_sRemainder)
		{
			bool					bRet = true;
			int						iTStart, iTEnd, iVStart, iVEnd, iRemStart;

			o_sVmc = o_sThread = "";
			o_sRemainder = i_sMsg;
			iTStart = iTEnd = iVStart = iVEnd = iRemStart = -1;

			try
			{
				if ((i_sMsg == null) || (i_sMsg.Length == 0))
				{
					bRet = false;
					SetLastError(-1, string.Format("ExtractIndexesFromMsg input message string was null or empty."));
				}
				else
				{
					if (i_sMsg.StartsWith("[TI:"))
					{
						// We should have both the thread and VMC, start with the thread
						iTStart = 4;
						iTEnd = i_sMsg.IndexOf(']', iTStart);
						iRemStart = iTEnd + 1;
						o_sThread = i_sMsg.Substring(iTStart, (iTEnd - iTStart));

						// VMC should come next
						iVStart = i_sMsg.IndexOf("[VMC:", iTEnd);
						if (iVStart == -1)
						{
							// Not necessarily an error condition, just not found
						}
						else
						{
							iVStart += 5;
							iVEnd = i_sMsg.IndexOf(']', iVStart);
							if (iVEnd == -1)
							{
								// Not necessarily an error condition, just not found
							}
							else
							{
								iRemStart = iVEnd + 1;
								o_sVmc = i_sMsg.Substring(iVStart, (iVEnd - iVStart));
							}
						}

						o_sRemainder = i_sMsg.Substring(iRemStart).Trim();
					}
					else if (i_sMsg.StartsWith("["))
					{
						// We may have the VMC, but for now we'll use it even if it isn't the VMC (there really is no way to tell)
						iVStart = 1;
						iVEnd = i_sMsg.IndexOf(']', iVStart);
						iRemStart = iVEnd + 1;
						if (iVEnd == -1)
						{
							// Not necessarily an error condition, just not found
						}
						else
						{
							o_sVmc = i_sMsg.Substring(iVStart, (iVEnd - iVStart));
							o_sRemainder = i_sMsg.Substring(iRemStart).Trim();
						}
					}
					else
					{
						// Nothing to do here
					}
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				SetLastError(-1, string.Format("ExtractIndexesFromMsg caught exception: '{0}'", exc.ToString()));
				Console.Error.WriteLine(DateTime.Now.ToString() + " " + m_sLastError);
			}

			return (bRet);
		} // ExtractIndexesFromMsg

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sMsg"></param>
		/// <param name="io_sVmc"></param>
		/// <param name="io_sThread"></param>
		/// <param name="sRemainder"></param>
		/// <returns></returns>
		private bool ReplaceIndexesIfEmbedded(string i_sMsg, ref string io_sVmc, ref string io_sThread, out string sRemainder)
		{
			bool					bRet = true;
			string					sVmcParsed = "", sThreadParsed = "";

			sRemainder = i_sMsg;

			if ((io_sVmc.Length == 0) || (io_sThread.Length == 0))
			{
				bRet = ExtractIndexesFromMsg(i_sMsg, out sVmcParsed, out sThreadParsed, out sRemainder);
				if ((io_sVmc.Length == 0) && (sVmcParsed.Length > 0))
				{
					io_sVmc = sVmcParsed;
				}
				if ((io_sThread.Length == 0) && (sThreadParsed.Length > 0))
				{
					io_sThread = sThreadParsed;
				}
			}

			return (bRet);
		} // ReplaceIndexesIfEmbedded

		/// <summary>
		/// 
		/// The format of the columns will be:  DateTime, Severity, IpAddress, SessionId, ComponentName, MethodName, VmcId, ThreadId, Msg
		/// </summary>
		/// <param name="i_eSeverity"></param>
		/// <param name="i_sMsg"></param>
		/// <returns></returns>
		public override bool Log(eSeverity i_eSeverity, string i_sMsg)
		{
			bool					bRet = true, bRes = true;
			string					sComponentName = "", sMethodName = "", sVmc = "", sThread = "", sRem = "";
			LoggerThreadInfo		oInfo = null;
			StackTrace				oST = null;

			try
			{
				SetLastError(0, "");
				oInfo = new LoggerThreadInfo();
				oST = new StackTrace(2);				// Skip two frames to avoid the overhead of getting a full stack trace

				// First check to see if we need to log this message or not
				if (i_eSeverity >= m_FilterLevel)
				{
					bRes = ThreadInfoDictSingleton.GetInstance().TryGetValue(Thread.CurrentThread.Name, out oInfo);
					if (!bRes)
					{
						// This isn't always an error, some threads (like pooled or event-handlers) won't be named.
//Console.WriteLine("!!!!!{0} In Log(2) TryGetValue failed, calling method '{1}'", DateTime.Now.ToString(), oST.ToString());
						if (oInfo == null)
						{
							oInfo = new LoggerThreadInfo();
						}
					}

					sVmc = oInfo.m_sVmcId;
					sThread = oInfo.m_sThreadId;
					bRes = ReplaceIndexesIfEmbedded(i_sMsg, ref sVmc, ref sThread, out sRem);

					if (oInfo.m_sComponentName.Length == 0)
					{
						sComponentName = oST.GetFrame(0).GetMethod().Module.Name;
					}
					else
					{
						sComponentName = oInfo.m_sComponentName;
					}
					sComponentName = StripExtension(sComponentName);

					sMethodName = oST.GetFrame(0).GetMethod().DeclaringType.FullName + "." + oST.GetFrame(0).GetMethod().Name;
					oST = null;

					m_Logger.Log(ConvertSeverityToNSLevel(i_eSeverity), string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", i_eSeverity.ToString(), oInfo.m_sIpAddress, oInfo.m_sSessionId, sComponentName, sMethodName, sVmc, sThread, sRem));
				}
				else
				{
					// Below the filter level
					//Console.WriteLine("Log(2).Log() else - i_eSeverity = '{0}', m_FilterLevel = '{1}'.", i_eSeverity.ToString(), m_FilterLevel.ToString());
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				SetLastError(-1, string.Format("Caught exception in Log(2), calling method '{0}': '{1}'", oST.ToString(), exc.ToString()));
				Console.Error.WriteLine(DateTime.Now.ToString() + " " + m_sLastError);
			}

			return (bRet);
		} // Log

		/// <summary>
		/// 
		/// The format of the columns will be:  DateTime, Severity, IpAddress, SessionId, ComponentName, MethodName, VmcId, ThreadId, Msg
		/// </summary>
		/// <param name="i_eSeverity"></param>
        /// <param name="i_iVmcId"></param>
		/// <param name="i_sComponent"></param>
		/// <param name="i_sMsg"></param>
		/// <returns></returns>
		public override bool Log(eSeverity i_eSeverity, int i_iVmcId, string i_sComponentName, string i_sMsg)
		{
			bool					bRet = true, bRes = true;
			string					sComponentName = i_sComponentName, sMethodName = "", sVmc = "", sThread = "", sRem = "";
			LoggerThreadInfo		oInfo = null;
			StackTrace				oST = null;

			try
			{
				SetLastError(0, "");
				oInfo = new LoggerThreadInfo();
				oST = new StackTrace(2);				// Skip two frames to avoid the overhead of getting a full stack trace

				// First check to see if we need to log this message or not
				if (i_eSeverity >= m_FilterLevel)
				{
					bRes = ThreadInfoDictSingleton.GetInstance().TryGetValue(Thread.CurrentThread.Name, out oInfo);		// Don't need to check return value, we'll just use the empty strings assigned in the constructor.
					if (!bRes)
					{
						// This isn't always an error, some threads (like pooled or event-handlers) won't be named.
						if (oInfo == null)
						{
							oInfo = new LoggerThreadInfo();
						}
					}

					sVmc = oInfo.m_sVmcId;
					sThread = oInfo.m_sThreadId;
					bRes = ReplaceIndexesIfEmbedded(i_sMsg, ref sVmc, ref sThread, out sRem);

					if (sComponentName.Length == 0)
					{
						sComponentName = oST.GetFrame(0).GetMethod().ReflectedType.FullName;
					}
					else
					{
						sComponentName = oInfo.m_sComponentName;
					}
					sComponentName = StripExtension(sComponentName);

					sMethodName = oST.GetFrame(0).GetMethod().ReflectedType.FullName + "." + oST.GetFrame(0).GetMethod().Name;
					oST = null;

					m_Logger.Log(ConvertSeverityToNSLevel(i_eSeverity), string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", i_eSeverity.ToString(), oInfo.m_sIpAddress, oInfo.m_sSessionId, sComponentName, sMethodName, sVmc, sThread, sRem));
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				SetLastError(-1, "Caught exception in Log(3): " + exc.ToString());
				Console.Error.WriteLine(m_sLastError);
			}

			return (bRet);
		} // Log

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string GetErrorStr()
		{
			return(m_sLastError);
		} // GetErrorStr

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetErrorCode()
		{
			return(m_iLastError);
		} // GetErrorCode

	} // class TsvAndStdoutLogger
}
