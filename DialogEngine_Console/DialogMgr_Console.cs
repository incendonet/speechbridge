// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;

using ISMessaging;
using SBResourceMgr;
using DialogEngine;

using Incendonet.Utilities.LogClient;

namespace DialogMgr_Console
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class DialogMgr
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool bRes;
			DialogMgr_srv.eDMResult eRes = DialogMgr_srv.eDMResult.eUnknown;
			DialogMgr_srv dmSrv = null;
			string sPath;
			string sDisableKeyboard = "", sVer = "";
			AssemblyName asmName = null;
			ILegacyLogger logger = null;

			// Name the main thread
			Thread.CurrentThread.Name = "DialogMgrMainT";

			// Set up the logger(s)
			sPath = ConfigurationManager.AppSettings["LogFilePath"];
			logger = new LegacyLogger();
			logger.Init("", "", Thread.CurrentThread.Name, "", "", sPath);
			bRes = logger.Open();
			if (!bRes)
			{
				Console.Error.WriteLine("DialogMgr failed to open the logger!");
			}
			else
			{

				asmName = Assembly.GetAssembly(typeof(DialogMgr_Console.DialogMgr)).GetName();
				sVer = asmName.Version.ToString();
				logger.Log(Level.Info, "DialogMgr v" + sVer);

				bRes = LoadCustomAssemblies(logger);

				bRes = RemotingConfig(logger);

				dmSrv = new DialogMgr_srv(logger);
				bRes = RegisterEventHandlers(dmSrv);

				if (bRes)
				{
					bRes = dmSrv.CreateWorkerThreads("");

					//Console.WriteLine("DialogMgr_Console startup successful.");
					logger.Log(Level.Info, "DialogMgr_Console startup successful.");

					sDisableKeyboard = ConfigurationManager.AppSettings["DisableKeyboard"];
					sDisableKeyboard = (sDisableKeyboard == null) ? "" : sDisableKeyboard;
					if (sDisableKeyboard == "false")
					{
						// Start processing messages
						eRes = dmSrv.ProcessKbMsgLoop();
					}

					// Join worker threads before exiting
					dmSrv.JoinWorkerThreads();

					// Clean up
					UnregisterEventHandlers(dmSrv);
				}
				else
				{
					//Console.WriteLine("DialogMgr_Console startup unsuccessful!!!");
					logger.Log(Level.Exception, "DialogMgr_Console startup unsuccessful!!!");
				}

				logger.Log(Level.Info, "DialogMgr_Console shutdown.");
				logger.Close();
			}
		}

		private static bool RemotingConfig(ILegacyLogger i_Logger)
		{
			bool		bRet = true;
			//string		sHostCfgFile, sAECfgFile;
			string		sTmp;

			try
			{
				/* Mono doesn't yet support reading remoting config files (as of 1.1.10).  Change this back when they do.
				// Load the server configuration file
				sHostCfgFile = "DialogMgr_host.Config";
				RemotingConfiguration.Configure(sHostCfgFile);

				// Load the client configuration files
				sAECfgFile = "AudioEngine_client.Config";			// The AudioEngine config file
				RemotingConfiguration.Configure(sAECfgFile);
				*/

				// Mono compatible remoting configuration
				// Server config
				sTmp = ConfigurationManager.AppSettings["RemotingServerPort"];
				sTmp = (sTmp == null) ? "" : sTmp;
				if(sTmp.Length <= 0)
				{
					bRet = false;
					i_Logger.Log(Level.Exception, "DialogMgr.RemotingConfig: Invalid 'RemotingServerPort'.");
				}
				else
				{
					ChannelServices.RegisterChannel(new TcpChannel(int.Parse(sTmp)), false);
					RemotingConfiguration.RegisterWellKnownServiceType(typeof(DialogMgr_Console.DMMessaging), "DialogMgr.rem", WellKnownObjectMode.SingleCall);

					// Client config
					sTmp = ConfigurationManager.AppSettings["RemotingAudioMgrUrl"];
					RemotingConfiguration.RegisterWellKnownClientType(typeof(ISMessaging.Delivery.ISMReceiverImpl), sTmp);
				}
			}
			catch(Exception exc)
			{
				i_Logger.Log(Level.Exception, "DialogMgr.RemotingConfig: Exception:" + exc.ToString());
				bRet = false;
			}

			return(bRet);
		}

		private static bool LoadCustomAssemblies(ILegacyLogger i_Logger)
		{
			bool				bRet = true, bRes = true;
			string				sConfigLine = "";
			StringCollection	asCustAssys = null;
			Assembly			aAssy = null;

			try
			{
				sConfigLine = ConfigurationManager.AppSettings["CustomAssemblies"];
				sConfigLine = (sConfigLine == null) ? "" : sConfigLine;
				if(sConfigLine.Length > 0)
				{
					asCustAssys = new StringCollection();
					bRes = Utilities.GetItemsFromString(sConfigLine, ';', asCustAssys);
					if(bRes)
					{
						foreach(string sAssyName in asCustAssys)
						{
							try
							{
								aAssy = Assembly.Load(sAssyName);	// Note: there is no corresponding 'Unload' in the FCL.  See http://www.google.com/search?as_q=assembly+unload
								if(aAssy == null)
								{
									i_Logger.Log(Level.Warning, "LoadCustomAssemblies: Load of '" + sAssyName + "' failed!");
								}
								else
								{
									i_Logger.Log(Level.Info, "LoadCustomAssemblies: Loaded '" + aAssy.FullName + "' from '" + aAssy.Location + "'.");
								}
							}
							catch(Exception exc)
							{
								i_Logger.Log(Level.Warning, "LoadCustomAssemblies: Load of '" + sAssyName + "' failed!  Exception: " + exc.ToString());
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR DialogEngine.RemotingInit:  Caught exception '{0}'.", e.ToString());
				i_Logger.Log(e);
			}

			return(bRet);
		} // LoadCustomAssemblies

		private static bool RegisterEventHandlers(DialogMgr_srv i_dmSrv)
		{
			bool	bRet = true;

			DMEvHook.MsgDistr = new ISMessaging.Delivery.ISMDistributer();
			DMEvHook.MsgDistr.ISMMsg += new ISMessaging.Delivery.ISMDistributer.ISMDistributerEventHandler(i_dmSrv.NewMsg);

			return(bRet);
		}

		private static bool UnregisterEventHandlers(DialogMgr_srv i_dmSrv)
		{
			bool	bRet = true;

			ISMessaging.Delivery.ISMDistributer.ISMDistributerEventHandler dmh = new ISMessaging.Delivery.ISMDistributer.ISMDistributerEventHandler(i_dmSrv.NewMsg);
			DMEvHook.MsgDistr.ISMMsg -= dmh;

			return(bRet);
		}
	}

	public class DialogMgr_srv
	{
		public		static	ISMessaging.MsgQueue[]	m_aqDMMsgs;
		private		static	int						m_NumDialogs = 1;			// Used to allocate threads and queues.
		protected			IResourceMgr			m_RM;
		private		static	Thread[]				m_atDialogs;
		protected ILegacyLogger m_Logger = null;

		public enum eDMResult
		{
			eUnknown,
			eSuccess,
			eError,
			eAbort,
		}

		public DialogMgr_srv(ILegacyLogger i_Logger)
		{
			m_Logger = i_Logger;
			m_RM = new ResourceMgr_local(m_Logger);
			m_NumDialogs = m_RM.GetMaxSessions();
		}

		public void NewMsg(Object i_sender, ISMessaging.Delivery.ISMDistributer.ISMDistributerEventArgs i_args)
		{
			int					iQIndex = -1;
			ISMessaging.ISMsg	mMsg = null;
			bool				bMsgBegin = false, bMsgDtmf = false;

			// Copy and release the original message (so that we don't have to call back to the remote client.)
			switch(i_args.m_Msg.GetType().ToString())		// FIX - There has to be a better way to do this...
			{
				case "ISMessaging.SpeechRec.ISMResults" :
				{
//					ISMessaging.SpeechRec.ISMResults	mTmp, mClone;
//
//					mTmp = (ISMessaging.SpeechRec.ISMResults)i_args.m_Msg;
//					mClone = (ISMessaging.SpeechRec.ISMResults)mTmp.Clone();
//					mMsg = (ISMessaging.ISMsg)mClone;
					mMsg = (ISMessaging.ISMsg)(((ISMessaging.SpeechRec.ISMResults)i_args.m_Msg).Clone());
				}
				break;
				case "ISMessaging.ISMsg" :
				{
					mMsg = (ISMessaging.ISMsg)i_args.m_Msg.Clone();
				}
				break;
				case "ISMessaging.Session.ISMSessionBegin" :
				{
					bMsgBegin = true;
					mMsg = (ISMessaging.ISMsg)(((ISMessaging.Session.ISMSessionBegin)i_args.m_Msg).Clone());
				}
				break;
				case "ISMessaging.Session.ISMSessionEnd" :
				{
					mMsg = (ISMessaging.ISMsg)(((ISMessaging.Session.ISMSessionEnd)i_args.m_Msg).Clone());
				}
				break;
				case "ISMessaging.Session.ISMTimerExpired" :
				{
					mMsg = (ISMessaging.ISMsg)(((ISMessaging.Session.ISMTimerExpired)i_args.m_Msg).Clone());
				}
				break;
				case "ISMessaging.Audio.ISMDtmfComplete" :
				{
					bMsgDtmf = true;
					mMsg = (ISMessaging.ISMsg)(((ISMessaging.Audio.ISMDtmfComplete)i_args.m_Msg).Clone());
				}
				break;
				default :
				{
					mMsg = (ISMessaging.ISMsg)i_args.m_Msg.Clone();
					//Console.Error.WriteLine("ERROR:  DialogMgr_Srv.NewMsg() - Message type was not recognized: '{0}'.", i_args.m_Msg.GetType().ToString());
					m_Logger.Log(Level.Warning, "DialogMgr_Srv.NewMsg() - Message type was not recognized: " + i_args.m_Msg.GetType());
				}
				break;
			}

			// Release remote object
			i_args = null;

			// Push message onto appropriate queue
			if(mMsg != null)
			{
				iQIndex = mMsg.m_Dest.m_VMC.m_iKey;
				m_aqDMMsgs[iQIndex].Push(mMsg);

				//Console.WriteLine("DialogMgr_srv.NewMsg '{0}' received on TID {1}.", i_args.m_Msg.GetType().ToString(), AppDomain.GetCurrentThreadId());
				if(bMsgBegin)
				{
					// Name the main thread if it hasn't been already (this is an even handler, so it may not have been)
					if( (Thread.CurrentThread.Name == null) || (Thread.CurrentThread.Name == "") )
					{
						Thread.CurrentThread.Name = "DialogMgr_srvT";
					}

					// Set IpAddress, SessionId in Logger so all subsequent calls to Log() will be identified.  (Need-to/should do this in every thread once the ISMSessionBegin comes in.)
					m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.IpAddress.ToString(), Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(oAddr => oAddr.AddressFamily == AddressFamily.InterNetwork).ToString() ?? "");
					m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.SessionId.ToString(), mMsg.m_sSessionId);

					m_Logger.Log(Level.Info, string.Format("[{0}]<<<DMNM ISMSessionBegin(F:'{1}', T:'{2}')>>>", iQIndex, ((ISMessaging.Session.ISMSessionBegin)mMsg).m_sFromAddr, ((ISMessaging.Session.ISMSessionBegin)mMsg).m_sToAddr));
				}
				else if(bMsgDtmf)
				{
					m_Logger.Log(Level.Info, string.Format("[{0}]DialogMgr_srv.NewMsg DTMF: '{1}'.", iQIndex.ToString(), ((ISMessaging.Audio.ISMDtmfComplete)mMsg).m_sDtmf));
				}
				else
				{
					m_Logger.Log(Level.Debug, string.Format("[{0}]DialogMgr_srv.NewMsg: {1}", iQIndex.ToString(), mMsg.GetType()));
				}
			}
			else
			{
				m_Logger.Log(Level.Exception, "DialogMgr_srv.NewMsg error: No message.");
			}
		}

		public eDMResult ProcessKbMsgLoop()
		{
			bool				bRes = true;
			string				sPrompt = "> ";
			string				sCmd;
			eDMResult			eRet = eDMResult.eSuccess;

			Console.Write(sPrompt);
			sCmd = Console.ReadLine();
			while(bRes)
			{
				bRes = ProcessCmdString(sCmd);
				if(bRes)
				{
					Console.Write(sPrompt);
					sCmd = Console.ReadLine();
				}
			}

			return(eRet);
		}

		protected bool ProcessMsg(string i_sXmlMsg)
		{
			bool	bRet = true;

			Console.WriteLine("DES - In ProcessMsg()");

			return(bRet);
		}

		public bool ProcessCmdString(string i_sCmd)
		{
			bool	bRet = true;
			bool	bRes;
			string	sScriptUrl = "";

			switch(i_sCmd)
			{
				case "q" :
				case "Q" :
					bRet = false;
					break;
				case "g" :
				case "G" :
					Console.WriteLine("Current TID is {0}.", Thread.CurrentThread.ManagedThreadId);
					break;
				case "t" :
				case "T" :
					bRes = CreateWorkerThreads(sScriptUrl);
					break;
				case "":
					break;
				case "h" :
				case "H" :
				case "?":
					Console.WriteLine("Your options are:");
					Console.WriteLine("  'h' - Help.");
					Console.WriteLine("  'g' - Get current TID.");
					Console.WriteLine("  't' - Create worker threads.");
					Console.WriteLine("  'q' - Quit.");
					break;
				default :
					Console.WriteLine("Invalid command entered, try 'h' or '?' for help.");
					break;
			}

			return(bRet);
		}

		public bool CreateWorkerThreads(string i_sScriptUrl)
		{
			bool			bRet = true;
			int				ii;
			DialogThread[]	atDialogs;

			try
			{
				// Create message queues, and create and start threads.
				m_aqDMMsgs = new ISMessaging.MsgQueue[m_NumDialogs];
				atDialogs = new DialogThread[m_NumDialogs];
				m_atDialogs = new Thread[m_NumDialogs];

				for(ii = 0; ii < m_NumDialogs; ii++)
				{
					// Create message queues
					m_aqDMMsgs[ii] = new ISMessaging.MsgQueue(ii);

					// Create & start threads
					atDialogs[ii] = new DialogThread(m_Logger, ii, i_sScriptUrl, m_aqDMMsgs[ii]);
					m_atDialogs[ii] = new Thread(new ThreadStart(atDialogs[ii].ThreadProc));
					m_atDialogs[ii].Name = ii.ToString() + "_DialogWorkerT";
					m_atDialogs[ii].Start();
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("DialogMgr_Console.DialogMgr_srv.CreateWorkerThreads() exception '{0}'.", e);
				m_Logger.Log(e);
			}

			return(bRet);
		}

		public bool JoinWorkerThreads()
		{
			bool		bRet = true;
			int			ii;
			string		sTmp;

			try
			{
				for(ii = 0; ii < m_NumDialogs; ii++)
				{
					sTmp = "Waiting for thread #" + ii.ToString() + " of " + m_NumDialogs.ToString() + " to complete...";
					m_Logger.Log(Level.Debug, sTmp);

					m_atDialogs[ii].Join();

					sTmp = "Thread #" + ii.ToString() + " completed.";
					m_Logger.Log(Level.Debug, sTmp);
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioEngine_Console.AudioEngine_srv.CreateWorkerThreads:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(e);
			}

			return(bRet);
		}

		public class DialogThread
		{
			private		int						m_iThreadIndex;
			private		string					m_sScriptUrl;
			private		ISMessaging.MsgQueue	m_qMsg;
			protected	ILegacyLogger m_Logger;

			protected	DialogEngine.DialogEngine	m_DEngine;

			public DialogThread(ILegacyLogger i_Logger, int i_iThreadIndex, string i_sScriptUrl, ISMessaging.MsgQueue i_qMsg)
			{
				bool	bRes;

				m_Logger = i_Logger;
				m_iThreadIndex = i_iThreadIndex;
				m_sScriptUrl = i_sScriptUrl;
				m_qMsg = i_qMsg;

				m_DEngine = new DialogEngine.DialogEngine(m_Logger, i_iThreadIndex);		// FIX - Get VMC from RM, threadindex isn't valid in distributed/redundant installations.
				bRes = m_DEngine.Init();
				if(!bRes)
				{
					// FIX - Continue on?  Try to load from a 'safe' URL?
				}
			}

			public void ThreadProc()
			{
				bool					bDone = false;
				ISMessaging.ISMsg		mMsg;

				m_Logger.Init("", "", Thread.CurrentThread.Name, "", "", "");

				while(!bDone)
				{
					try
					{
						mMsg = m_qMsg.Pop();
						if(mMsg == null)
						{
							// Shouldn't ever get here without a timeout in the Pop() above.
						}
						else
						{
							// Work on new message.
							switch(mMsg.GetType().ToString())		// FIX - There has to be a better way to determine the message type...
							{
								case "ISMessaging.SpeechRec.ISMResults" :
									m_DEngine.ProcessSpeechResults((ISMessaging.SpeechRec.ISMResults)mMsg);
									break;
								case "ISMessaging.Session.ISMSessionBegin" :
									// Set IpAddress, SessionId in Logger so all subsequent calls to Log() will be identified.  (Need-to/should do this in every thread once the ISMSessionBegin comes in.)
									m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.IpAddress.ToString(), Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(oAddr => oAddr.AddressFamily == AddressFamily.InterNetwork).ToString() ?? "");
									m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.SessionId.ToString(), mMsg.m_sSessionId);

									m_DEngine.ProcessSessionBegin((ISMessaging.Session.ISMSessionBegin)mMsg);
									break;
								case "ISMessaging.Session.ISMSessionEnd" :
									m_DEngine.ProcessSessionEnd((ISMessaging.Session.ISMSessionEnd)mMsg);
									break;
								case "ISMessaging.Audio.ISMDtmfComplete" :
									m_DEngine.ProcessDtmfComplete((ISMessaging.Audio.ISMDtmfComplete)mMsg);
									break;
								case "ISMessaging.Session.ISMTimerExpired" :
									m_DEngine.ProcessTimerExpired((ISMessaging.Session.ISMTimerExpired)mMsg);
									break;
								default :
									//Console.Error.WriteLine("Error:  DialogMgr_srv.DialogThread Got unknown msg type {0}.", mMsg.GetType().ToString());
									m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "DialogMgr_srv.DialogThread Got unknown msg type {0}." + mMsg.GetType());
									break;
							}	// end switch(type)

							mMsg = null;
						}
					}
					catch(Exception exc)
					{
						//Console.Error.WriteLine("DialogMgr_srv.DialogThread.ThreadProc() caught exception: '{0}'.", e.ToString());
						m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "DialogThread.ThreadProc: " + exc.ToString());
					}
				}	// while
			}	// ThreadProc

		}	// class DialogThread
	}
}
