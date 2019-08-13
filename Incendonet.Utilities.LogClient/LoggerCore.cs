// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

namespace Incendonet.Utilities.LogClient
{
	using ParamCollection =			List<LoggerCore.Param>;

	/// <summary>
	/// LoggerCore is an abstract class that provides some basic implementation that should be used by classes
	/// implementinng ILogger and ILegacyLogger.  In particular, it will be very helpful for deriviations to
	/// call base.Init() to fill the default member variables.
	/// 
	/// The format of the columns should be:  DateTime, Severity, IpAddress, SessionId, ComponentName, VmcId, ThreadId, Msg
	/// 
    /// enum	i_eSeverity -->		One of Attributes.eSeverity
    /// string	i_sIpAddr -->		The IP address of the caller (it's a string, so no preference of IPv4 or IPv6)
	/// string	i_sSessionId -->	The unique identifier of the session.  Most likely a SIP Call-ID
	/// string	i_sComponent -->	The calling software component.  If a reference to the Logger is passed as a reference, the caller should use the form of Log() that includes i_sComponent.
	/// string	i_sVmcId -->		The VMC ID (AKA - VMC.m_iKey) of the session
	/// string	i_sThreadId -->		The name/index of the thread (if known)
	/// string	i_sMsg -->			The message to be logged
	/// 
	/// Note:	This implementation assumes that each client thread will be uniquely named via setting the Thread.CurrentThread.Name property.
	///			If the client thread is not uniquely named (this includes the empty string) and uses the Log() method with the least
	///			parameters, it may have misleading values in the resulting log statement.
	/// Note:	The timestamp will be determined and saved when Log() is called.
	/// Note:	i_sIpAddr wouldn't be necessary if: 1. we used a reference to a VMC object rather than the ID/key and 2. we knew it would always be set properly, but then this interface wouldn't be scriptable.
	/// Note:	Derivations of Logger must override Init() and call base.Init()
	/// Note:	The default level of log filtering is Debug
	/// </summary>
	public abstract class LoggerCore : ILogger
	{
		private const string		m_csUnnamed =	"{UNNAMED}";

		/// <summary>
		/// Note:  The ordering of the enum is the reverse of what you'd expect for a priority value so that the code using it is easier to read.  (I.e.: if(yourprio >= m_FilterLevel) {} )
		/// </summary>
		public enum eSeverity
		{
			Debug,
			Info,
			Warning,
			Error,
			Critical,
			Crash
		}

		/// <summary>
		/// 
		/// </summary>
		public class Param
		{
			private string			m_sName =		"";
			private string			m_sValue =		"";

			public Param(string i_sName, string i_sValue)
			{
				m_sName = i_sName;
				m_sValue = i_sValue;
			} // Param.ctor

			public string GetName()
			{
				return (m_sName);
			} // GetName

			public string GetValue()
			{
				return (m_sValue);
			} // GetValue
		} // class Param

		/// <summary>
		/// 
		/// </summary>
		public enum eRequiredParams
		{
			IpAddress,
			SessionId,
			ComponentName,
			ThreadId,
			VmcId
		}

		/// <summary>
		/// 
		/// </summary>
		protected class LoggerThreadInfo
		{
			public string m_sIpAddress =		"";
			public string m_sSessionId =		"";
			public string m_sComponentName =	"";
			public string m_sThreadId =			"";
			public string m_sVmcId =			"";

			public void Init(string i_sIpAddress, string i_sSessionId, string i_sComponentName, string i_sThreadId, string i_sVmcId)
			{
				m_sIpAddress =		i_sIpAddress;
				m_sSessionId =		i_sSessionId;
				m_sComponentName =	i_sComponentName;
				m_sThreadId =		i_sThreadId;
				m_sVmcId =			i_sVmcId;
			} // Init
		} // class LoggerThreadInfo

		/// <summary>
		/// Thread-safe singleton implentation follows #5 from http://www.csharpindepth.com/articles/general/singleton.aspx
		/// </summary>
		protected sealed class ThreadInfoDictSingleton
		{
			private class Nested
			{
				static Nested()										{}
				internal static readonly ThreadInfoDictSingleton	m_instance = new ThreadInfoDictSingleton();
			} // class Nested

			private ThreadInfoDictSingleton()
			{
				m_LoggerThreadInfoDict = new SortedDictionary<string, LoggerThreadInfo>();
			} // ThreadInfoDictSingleton

			private SortedDictionary<string, LoggerThreadInfo>		m_LoggerThreadInfoDict = null;

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public static ThreadInfoDictSingleton GetInstance()
			{
				return (Nested.m_instance);
			} // ThreadInfoDictSingleton

			/// <summary>
			/// 
			/// </summary>
			/// <param name="i_sKey"></param>
			/// <param name="o_Info"></param>
			/// <returns></returns>
			public bool TryGetValue(string i_sKey, out LoggerThreadInfo o_Info)
			{
				bool			bRet = true;

				lock (m_LoggerThreadInfoDict)
				{
					if( (i_sKey == null) || (i_sKey == "") )			// Check to make sure there is a valid value in the key
					{
						i_sKey = m_csUnnamed;
					}
					bRet = m_LoggerThreadInfoDict.TryGetValue(i_sKey, out o_Info);
					if (!bRet)
					{
StringBuilder		sbKeys = null;

sbKeys = new StringBuilder();
sbKeys.AppendFormat("{0} TryGetValues('{1}') not among: ", DateTime.Now.ToString(), i_sKey);
foreach (string sTmp in ThreadInfoDictSingleton.GetInstance().m_LoggerThreadInfoDict.Keys)
{
	sbKeys.AppendFormat("{0}, ", sTmp);
}
Console.WriteLine(sbKeys.ToString());
					}
				}

				return (bRet);
			} // TryGetValue

			/// <summary>
			/// 
			/// </summary>
			/// <param name="i_sKey"></param>
			/// <param name="o_sIpAddress"></param>
			/// <param name="o_sSessionId"></param>
			/// <param name="o_sComponentName"></param>
			/// <param name="o_sThreadId"></param>
			/// <param name="o_sVmcId"></param>
			/// <returns></returns>
			public bool TryGetValues(string i_sKey, out string o_sIpAddress, out string o_sSessionId, out string o_sComponentName, out string o_sThreadId, out string o_sVmcId)
			{
				bool				bRet = true;
				LoggerThreadInfo	oInfo = null;

				o_sIpAddress = o_sSessionId = o_sComponentName = o_sThreadId = o_sVmcId = "";

				if( (i_sKey == null) || (i_sKey == "") )			// Check to make sure there is a valid value in the key
				{
					i_sKey = m_csUnnamed;
				}
				bRet = ThreadInfoDictSingleton.GetInstance().TryGetValue(i_sKey, out oInfo);
				if (bRet)
				{
					o_sIpAddress =		oInfo.m_sIpAddress;
					o_sSessionId =		oInfo.m_sSessionId;
					o_sComponentName =	oInfo.m_sComponentName;
					o_sThreadId =		oInfo.m_sThreadId;
					o_sVmcId =			oInfo.m_sVmcId;
				}
				else
				{
StringBuilder		sbKeys = null;

sbKeys = new StringBuilder();
sbKeys.AppendFormat("{0} TryGetValues('{1}') not among: ", DateTime.Now.ToString(), i_sKey);
foreach (string sTmp in ThreadInfoDictSingleton.GetInstance().m_LoggerThreadInfoDict.Keys)
{
	sbKeys.AppendFormat("{0}, ", sTmp);
}
Console.WriteLine(sbKeys.ToString());
				}

				return (bRet);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="i_sKey"></param>
			/// <param name="i_Info"></param>
			public void Add(string i_sKey, LoggerThreadInfo i_Info)
			{
				lock (m_LoggerThreadInfoDict)
				{
					if( (i_sKey == null) || (i_sKey == "") )			// Check to make sure there is a valid value in the key
					{
						i_sKey = m_csUnnamed;
					}
					m_LoggerThreadInfoDict.Add(i_sKey, i_Info);
				}
			} // Add
		} // class ThreadInfoDictSingleton

		// Local variables
		protected ParamCollection		m_ParamCollection;
		protected eSeverity				m_FilterLevel =			eSeverity.Debug;		// By default we include all logging

		// Abstract methods
		public abstract bool		Open();
		public abstract bool		Close();
		// The Log() methods return bools for success, requiring the caller to check the error code or string with the
		// appropriate followup method call.  This was intential to make it easier for scripting languages to use
		// Logger, despite making it slightly clumsier for staticly typed languages.
		public abstract bool		Log(eSeverity i_eSeverity, string i_sMsg);
		public abstract bool		Log(eSeverity i_eSeverity, int i_iVmcId, string i_sComponentName, string i_sMsg);
		public abstract string		GetErrorStr();		// Returns a description of the error from the last time Log() returned 'false'.
		public abstract int			GetErrorCode();		// Returns the ID of the error from the last time Log() returned 'false'.

		// Base class implementations

		/// <summary>
		/// To increase or decrease the amount of logging, set the filter level accordingly.
		/// </summary>
		/// <param name="i_FilterLevel"></param>
		public void SetFilterLevel(eSeverity i_FilterLevel)
		{
			m_FilterLevel = i_FilterLevel;
		}

		/// <summary>
		/// Returns the current logging filter level.
		/// </summary>
		/// <returns></returns>
		public eSeverity GetFilterLevel()
		{
			return (m_FilterLevel);
		}

		/// <summary>
		/// Initializes the parameters, but does not open the logger.  Init can be called multiple times.
		/// Remember to call base.Init() in your overriding member.
		/// </summary>
		/// <param name="i_Params"></param>
		/// <returns></returns>
		public virtual bool Init(ref ParamCollection i_Params)
		{
			bool						bRet = true, bRes = true;
			string						sParam = "", sThreadId = "";
			LoggerThreadInfo			ltiTmp = null;
			ThreadInfoDictSingleton		oDict = null;

			try
			{
				sParam = eRequiredParams.ThreadId.ToString();
				sThreadId = i_Params.Find(param => param.GetName() == sParam).GetValue();
				oDict = ThreadInfoDictSingleton.GetInstance();
				bRes = oDict.TryGetValue(sThreadId, out ltiTmp);
				if (!bRes)
				{
					ltiTmp = new LoggerThreadInfo();
					oDict.Add(sThreadId, ltiTmp);
					ltiTmp.m_sThreadId = sThreadId;
				}

				lock (ltiTmp)		// Technically it shouldn't be necessary to lock, but it can't hurt
				{
					// Pull the required params from the collection to the member variables, for easier access later.
					sParam = eRequiredParams.IpAddress.ToString();
					ltiTmp.m_sIpAddress = i_Params.Find(param => param.GetName() == sParam).GetValue();
					sParam = eRequiredParams.SessionId.ToString();
					ltiTmp.m_sSessionId = i_Params.Find(param => param.GetName() == sParam).GetValue();
					sParam = eRequiredParams.ComponentName.ToString();
					ltiTmp.m_sComponentName = i_Params.Find(param => param.GetName() == sParam).GetValue();
					sParam = eRequiredParams.VmcId.ToString();
					ltiTmp.m_sVmcId = i_Params.Find(param => param.GetName() == sParam).GetValue();
				}
			}
			catch
			{
				bRet = false;		// Unnecessary if throwing an exception
				throw(new ArgumentException("Logger.Init: A required parameter was not included in i_Params: " + sParam));
			}

			return (bRet);
		} // Init

		/// <summary>
		/// Updates the value of the named logger param.  Note: this will only update the values that LoggerCore
		/// knows about, and not any of a derived class.
		/// </summary>
		/// <param name="i_sThreadId"></param>
		/// <param name="i_sName"></param>
		/// <param name="i_sValue"></param>
		/// <returns></returns>
		public virtual bool UpdateValue(string i_sThreadId, string i_sName, string i_sValue)
		{
			bool						bRet = true, bRes = true;
			LoggerThreadInfo			ltiTmp = null;
			ThreadInfoDictSingleton		oDict = null;

			try
			{
				oDict = ThreadInfoDictSingleton.GetInstance();
				bRes = oDict.TryGetValue(i_sThreadId, out ltiTmp);
				if (!bRes)
				{
					// We shouldn't get here, unless the caller forgot to call Init(), or has changed the thread-id
					bRet = false;
				}

				// Update the params that were passed in
				lock (ltiTmp)		// Technically it shouldn't be necessary to lock, but it can't hurt
				{
					if (i_sName == eRequiredParams.IpAddress.ToString())
					{
						ltiTmp.m_sIpAddress = i_sValue;
					}
					else if (i_sName == eRequiredParams.SessionId.ToString())
					{
						ltiTmp.m_sSessionId = i_sValue;
					}
					else if (i_sName == eRequiredParams.ComponentName.ToString())
					{
						ltiTmp.m_sComponentName = i_sValue;
					}
					else if (i_sName == eRequiredParams.VmcId.ToString())
					{
						ltiTmp.m_sVmcId = i_sValue;
					}
					else
					{
						bRet = false;		// Return false, even when some params have been correctly updated
					}
				} // lock
			}
			catch
			{
				bRet = false;
			}

			return (bRet);
		} // UpdateValue

	} // class Logger
}
