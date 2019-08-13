// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;


namespace Incendonet.Utilities.LogClient
{
	using ParamCollection = List<LoggerCore.Param>;

	/// <summary>
	/// Doppelgänger of NSpring's Logger.Level
	/// </summary>
	public enum Level
	{
		Debug,
		Verbose,
		Config,
		Info,
		Warning,
		Exception,
		All,
		None
	}

	public class LegacyLogger : LoggerCore, ILegacyLogger
	{
		private TsvAndStdoutLogger m_Logger = null;

		~LegacyLogger()
		{
			Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sIpAddr"></param>
		/// <param name="i_sSessionId"></param>
		/// <param name="i_sThreadId"></param>
		/// <param name="i_sVmcId"></param>
		/// <param name="i_sComponentName"></param>
		/// <param name="i_sPath"></param>
		/// <returns></returns>
		public bool Init(string i_sIpAddr, string i_sSessionId, string i_sThreadId, string i_sVmcId, string i_sComponentName, string i_sPath)
		{
			bool							bRet = true;
			ParamCollection					aInitParams = null;

			try
			{
				aInitParams = new ParamCollection();
				aInitParams.Add(new Param(eRequiredParams.IpAddress.ToString(), i_sIpAddr));
				aInitParams.Add(new Param(eRequiredParams.SessionId.ToString(), i_sSessionId));
				aInitParams.Add(new Param(eRequiredParams.ThreadId.ToString(), i_sThreadId));
				aInitParams.Add(new Param(eRequiredParams.VmcId.ToString(), i_sVmcId));
				aInitParams.Add(new Param(eRequiredParams.ComponentName.ToString(), i_sComponentName));
				aInitParams.Add(new Param(TsvAndStdoutLogger.eMoreParams.Path.ToString(), i_sPath));

				if (m_Logger == null)
				{
					m_Logger = new TsvAndStdoutLogger();
				}
				bRet = m_Logger.Init(ref aInitParams);
				
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + " " + "Caught exception in LegacyLogger.Init:  " + exc.ToString());
			}

			return (bRet);
		} // Init

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Open()
		{
			bool							bRet = true;

			try
			{
				if (m_Logger == null)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " " + "LegacyLogger.Open: logger was null!");
				}
				else
				{
					bRet = m_Logger.Open();
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + " " + "Caught exception in LegacyLogger.Open:  " + exc.ToString());
			}

			return (bRet);
		} // Open

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Close()
		{
			bool bRet = true;

			if (m_Logger == null)
			{	// No need to do anything
			}
			else
			{
				m_Logger.Close();
				m_Logger = null;
			}

			return (bRet);
		} // Close

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Level"></param>
		/// <returns></returns>
		protected eSeverity ConvertLevelToSeverity(Level i_Level)
		{
			eSeverity	eRet = eSeverity.Debug;

			switch (i_Level)
			{
				case Level.All:
					eRet = eSeverity.Debug;
					break;
				case Level.Config :
					eRet = eSeverity.Debug;
					break;
				case Level.Debug :
					eRet = eSeverity.Debug;
					break;
				case Level.Exception :
					eRet = eSeverity.Error;
					break;
				case Level.Info :
					eRet = eSeverity.Info;
					break;
				case Level.None :
					eRet = eSeverity.Debug;
					break;
				case Level.Verbose :
					eRet = eSeverity.Debug;
					break;
				case Level.Warning :
					eRet = eSeverity.Warning;
					break;
				default:
					eRet = eSeverity.Debug;
					break;
			}

			return (eRet);
		} // ConvertLevelToSeverity

		/// <summary>
		/// Mimics the NSpring log method most often used in SB
		/// </summary>
		/// <param name="i_Level"></param>
		/// <param name="i_sMessage"></param>
		public void Log(Level i_Level, string i_sMessage)
		{
			m_Logger.Log(ConvertLevelToSeverity(i_Level), i_sMessage);
		} // Log

		/// <summary>
		/// Mimics another NSpring log method sometimes used in SB
		/// </summary>
		/// <param name="i_Exc"></param>
		public void Log(Exception i_Exc)
		{
			m_Logger.Log(eSeverity.Error, i_Exc.ToString());
		} // Log

		#region ILogger methods
		// Methods inherited from ILogger

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string GetErrorStr()
		{
			return (m_Logger.GetErrorStr());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetErrorCode()
		{
			return (m_Logger.GetErrorCode());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_eSeverity"></param>
		/// <param name="i_sMsg"></param>
		/// <returns></returns>
		public override bool Log(eSeverity i_eSeverity, string i_sMsg)
		{
			return(m_Logger.Log(i_eSeverity, i_sMsg));
		} // Log

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_eSeverity"></param>
		/// <param name="i_iVmcId"></param>
		/// <param name="i_sComponentName"></param>
		/// <param name="i_sMsg"></param>
		/// <returns></returns>
		public override bool Log(eSeverity i_eSeverity, int i_iVmcId, string i_sComponentName, string i_sMsg)
		{
			return (m_Logger.Log(i_eSeverity, i_iVmcId, i_sComponentName, i_sMsg));
		} // Log
		#endregion

	} // class LegacyLogger
}
