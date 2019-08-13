// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using SBConfigStor;


namespace SBLocalRM
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class SBLocalRM
	{
		public		static	string					g_sConsoleArg = "--console";
		public		static	string					g_sDesktopArg = "--desktop";
		public		static	ISMessaging.MsgQueue	m_aqMsgIn;
		private		static	Thread					m_tListener;
		private		static	Thread					m_tWorker;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			bool				bRes = true;
			ILegacyLogger		logger = null;
			string				sDisableKeyboard = "";
			string				sPrompt = "LRM> ", sCmd = "";
			string				sMonoBinPath = "", sBinPath = "", sArgs = "";
			bool				bDesktopRuntime = false;
			Process				oProc = null;

			try
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + ": In main.");
				if( (RunningSystem.RunningPlatform == CLRPlatform.Mono) && (args != null) && (args.Length > 0) )
				{
					foreach(string sArg in args)
					{
						if(sArg.IndexOf(g_sDesktopArg) >= 0)
						{
							bDesktopRuntime = true;
						}
					}
				}

				if( (args != null) && (args.Length > 0) && (args[0].IndexOf(g_sConsoleArg) >= 0) )
				{
					// This is the forked copy, carry on...
					try
					{
						Console.Error.WriteLine(DateTime.Now.ToString() + ": (F) Setting up event handlers...");
						// Set up abnormal shutdown handlers
						AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
						AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
						AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

						Console.Error.WriteLine(DateTime.Now.ToString() + ": (F) Setting up logging...");
						// Name the main thread
						Thread.CurrentThread.Name = "LocalRM-ForkedT";

						// Start logging
						bRes = StartLoggers(out logger);

						logger.Log(Level.Info, "LRM logger started.");

						// Check if IP address should be auto-saved to DB and config files.
						bRes = CheckIPAutoSave(logger);

						m_aqMsgIn = new MsgQueue(0);

						// Start threads
						m_tListener = StartListenerThread(logger, m_aqMsgIn);
						m_tWorker = StartWorkerThread(logger, m_aqMsgIn, bDesktopRuntime);

						// Start CLI
						sDisableKeyboard = ConfigurationManager.AppSettings["DisableKeyboard"];
						if(sDisableKeyboard == "false")
						{
							Console.Write(sPrompt);
							sCmd = Console.ReadLine();
							while(bRes)
							{
								//					bRes = ProcessCmdString(sCmd);
								if(bRes)
								{
									Console.Write(sPrompt);
									sCmd = Console.ReadLine();
								}
							}
						}

						if(m_tListener == null)
						{
							logger.Log(Level.Exception, "LRM(F): Listener thread didn't start!");
						}
						else if(m_tWorker == null)
						{
							logger.Log(Level.Exception, "LRM(F): Worker thread didn't start!");
						}
						else
						{
							// Wait for threads to complete
							m_tListener.Join();
							m_tWorker.Join();
						}
					}
					catch(Exception exc)
					{
						if(logger != null)
						{
							logger.Log(exc);
						}
						else
						{
							Console.Error.WriteLine(DateTime.Now.ToString() + ": (F) " + exc.ToString());
						}
					}

					if(logger != null)
					{
						logger.Log(Level.Info, "(F) LRM shutdown.");
						logger.Close();
					}
				}
				else
				{
					// This is the parent, so "fork".  // Is MainWindowTitle the only way that works to get the current program file???
					Console.Error.WriteLine(DateTime.Now.ToString() + ": (P) Setting up logging...");
					// Name the main thread
					Thread.CurrentThread.Name = "LocalRMT-Parent";

					// Start logging
					bRes = StartLoggers(out logger);

					logger.Log(Level.Info, "LRM (P) logger started.");

					if(RunningSystem.RunningPlatform == CLRPlatform.DotNet)
					{
						logger.Log(Level.Info, "LRM (P) Windows start.");

						sBinPath = ConfigurationManager.AppSettings["DefaultBinPath"];
						if( !(sBinPath.EndsWith("/")) && !(sBinPath.EndsWith("\\")) )
						{
							sBinPath += "/";
						}
						sBinPath += "SBLocalRM.exe";
						//Process.Start(sBinPath, g_sConsoleArg);
						sArgs = g_sConsoleArg;
					}
					else if(RunningSystem.RunningPlatform == CLRPlatform.Mono)
					{
						logger.Log(Level.Info, "LRM (P) Mono start.");

						sMonoBinPath = ConfigurationManager.AppSettings["MonoBinPath"];
						if( !(sMonoBinPath.EndsWith("/")) && !(sMonoBinPath.EndsWith("\\")) )
						{
							sMonoBinPath += "/";
						}
						sMonoBinPath += "mono";

						if(bDesktopRuntime)
						{
							sBinPath = g_sDesktopArg + " ";
						}
						else
						{
							sBinPath = "";
						}
						sBinPath += ConfigurationManager.AppSettings["DefaultBinPath"];
						if( !(sBinPath.EndsWith("/")) && !(sBinPath.EndsWith("\\")) )
						{
							sBinPath += "/";
						}
						sBinPath += "SBLocalRM.exe ";

						if(bDesktopRuntime)
						{
							logger.Log(Level.Info, "LRM (P) Desktop run.");

							//Process.Start(sMonoBinPath, sBinPath + g_sConsoleArg + " " + g_sDesktopArg);
							sArgs = sBinPath + g_sConsoleArg + " " + g_sDesktopArg;
							sBinPath = sMonoBinPath;
						}
						else
						{
							logger.Log(Level.Info, "LRM (P) Daemon run.");

							//Process.Start(sMonoBinPath, sBinPath + g_sConsoleArg);
							sArgs = sBinPath + g_sConsoleArg;
							sBinPath = sMonoBinPath;
						}
					}
					else
					{
						sBinPath = "";
						sArgs = "";
						logger.Log(Level.Exception, "(P) Main Start: Unknown platform '" + RunningSystem.RunningPlatform + "'.");
					}

					if(sBinPath.Length > 0)
					{
						logger.Log(Level.Info, "(P) Main Start-ing('" + sBinPath + "', '" + sArgs + "')...");
						oProc = Process.Start(sBinPath, sArgs);
						if(oProc == null)
						{
							logger.Log(Level.Exception, "(P) Main Failed to Start('" + sBinPath + "', '" + sArgs + "')!");
						}
						else
						{
							logger.Log(Level.Info, "(P) Main Start-ed('" + sBinPath + "', '" + sArgs + "').");
						}
					}
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + ": " + exc.ToString());
				Console.ReadLine();
			}

			return(0);
		} // Main

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_logConsole"></param>
		/// <param name="i_logFile"></param>
		/// <param name="i_logger"></param>
		/// <returns></returns>
		protected static bool StartLoggers(out ILegacyLogger o_logger)
		{
			bool				bRet = true;
			string				sPath = "";

			// Set up the logger(s)
			sPath = ConfigurationManager.AppSettings["DefaultLogFilePath"];
			o_logger = new LegacyLogger();
			o_logger.Init("", "", Thread.CurrentThread.Name, "", "", sPath);
			bRet = o_logger.Open();

			return(bRet);
		} // Start Loggers

		protected static Thread StartListenerThread(ILegacyLogger i_Logger, MsgQueue i_aqMsgIn)
		{
			Thread					tRet = null;
			UdpMsgListenerThread	listener = null;

			try
			{
				listener = new UdpMsgListenerThread(i_Logger, i_aqMsgIn);
				tRet = new Thread(new ThreadStart(listener.ThreadProc));
				tRet.Start();
			}
			catch(Exception exc)
			{
				i_Logger.Log(Level.Exception, "SBLocalRM.StartListenerThread caught exception: " + exc.ToString());
			}

			return(tRet);
		} // StartListenerThread

		protected static Thread StartWorkerThread(ILegacyLogger i_Logger, MsgQueue i_aqMsgIn, bool i_bDesktopRuntime)
		{
			Thread				tRet = null;
			LRMWorkerThread		worker = null;

			try
			{
				worker = new LRMWorkerThread(i_Logger, i_aqMsgIn, i_bDesktopRuntime);
				tRet = new Thread(new ThreadStart(worker.ThreadProc));
				tRet.Start();
			}
			catch(Exception exc)
			{
				i_Logger.Log(Level.Exception, "SBLocalRM.StartWorkerThread caught exception: " + exc.ToString());
			}

			return(tRet);
		} // StartWorkerThread

		protected static bool CheckIPAutoSave(ILegacyLogger i_Logger)
		{
			// FIX - As with the constants in GetPbxValue(), these constants should be pulled out.
			const string csProxyServer =				"SpeechBridge SIP Proxy";
			const string csAudiomgrAddr =				"AudioMgr IP address";

			bool						bRet = true, bRes = true;
			string						sCheck = "", sPath = "", sCurrAddr = "";

			System.Net.IPHostEntry		tmpIpHost = null;
			System.Net.IPAddress[]		tmpIpAddrs = null;
			string[]					asAddrs = null;
			int							ii = 0, iNumAddrs = 0;

			ConfigParams				cfgs = null;
			SBConfigStor.SIP			sipTmp = null;
			int							iNumElems = 0;
			string						sProxyAddr = "", sAMAddr = "";

			try
			{
				sCheck = ConfigurationManager.AppSettings["CheckIpOnStartup"];
				if( (sCheck == null) || (sCheck.Length == 0) || (sCheck.ToLower() == false.ToString().ToLower()) )
				{
					// Default or 'false' is to do nothing.
					i_Logger.Log(Level.Info, "CheckIPAutoSave - Not checking/saving IP address.");
				}
				else
				{
					// Get current IP address and compare it against what is stored in the DB.
					tmpIpHost = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
					tmpIpAddrs = tmpIpHost.AddressList;
					iNumAddrs = tmpIpAddrs.Length;

					asAddrs = new string[iNumAddrs];
					for(ii = 0; ii < iNumAddrs; ii++)
					{
						asAddrs[ii] = tmpIpAddrs[ii].ToString();
					}
					sCurrAddr = asAddrs[0];		// FIX - Assumes active NIC is 1'st one

					cfgs = new ConfigParams();
					cfgs.LoadFromTableByComponent(ConfigParams.e_Components.SIP.ToString());
					iNumElems = cfgs.Count;
					if(iNumElems <= 0)
					{
						i_Logger.Log(Level.Exception, "SBLocalRM.CheckIPAutoSave Couldn't load settings from DB.");
					}
					else
					{
						sProxyAddr = cfgs[csProxyServer].Value;
						sAMAddr = cfgs[csAudiomgrAddr].Value;

						// If address is different, save it to SIP Proxy and AudioMgr values, and save config files.
						if( (sCurrAddr != sProxyAddr) || (sCurrAddr != sAMAddr) )
						{
							sPath = ConfigurationManager.AppSettings["DefaultConfigPath"];
							cfgs[csProxyServer].Value = sCurrAddr;
							cfgs[csAudiomgrAddr].Value = sCurrAddr;
							bRes = cfgs.SaveToTableByComponent(ConfigParams.e_Components.SIP.ToString());
							if(!bRes)
							{
								i_Logger.Log(Level.Exception, "SBLocalRM.CheckIPAutoSave Error saving settings to DB.");
							}
							else
							{
								i_Logger.Log(Level.Info, "CheckIPAutoSave Saved settings to DB.");
							}


							// Get the PBX Type information and append it to the other configuration data before
							// regenerating the configuration files for the Proxy Server and the Audio Routers.
							//
							// NOTE: This code is very similar to what exists in ServerSettings.aspx.cs (m_buttonUpdate_Click())
							//       so it should be refactored to a common location.
							//       Also, as per the comment in ServerSettings.aspx.cs, if more elements are added to SIP2 then
							//       those will also have to be added to the configuration data passed to SIP.Persist().

							ConfigParams cfgs2 = new ConfigParams();
							cfgs2.LoadFromTableByComponent(ConfigParams.e_Components.SIP2.ToString());
							cfgs.Add(GetPbxValue(cfgs2));

							// Get the load-balancing/failover information and append it to the other configuration data before
							// regenerating the configuration files for the Proxy Server and the Audio Routers.

							ConfigParams cfgs3 = new ConfigParams();
							cfgs3.LoadFromTableByComponent(ConfigParams.e_Components.SIP3.ToString());

							foreach (ConfigParams.ConfigParam cfg in cfgs3)
							{
								cfgs.Add(cfg);
							}

							sipTmp = new SIP();
							bRes = sipTmp.Persist(cfgs, sPath);
							if(!bRes)
							{
								i_Logger.Log(Level.Exception, "SBLocalRM.CheckIPAutoSave Error saving settings to config files.");
							}
							else
							{
								i_Logger.Log(Level.Info, "CheckIPAutoSave Saved settings to config files.");
							}
							
							// Write addr to /etc/hosts, hostname to /etc/sysconfig/network
						}
						else
						{
							i_Logger.Log(Level.Info, "CheckIPAutoSave - IP addresses matched.");
						}
					}
				}
			}
			catch(Exception exc)
			{
				i_Logger.Log(Level.Exception, "SBLocalRM.CheckIPAutoSave caught exception: " + exc.ToString());
				bRet = false;
			}

			return(bRet);
		} // CheckIPAutoSave

		private static ConfigParams.ConfigParam GetPbxValue(ConfigParams i_cfgs)
		{
			// NOTE: The following code is almost identical to what exists in ServerSettings.aspx.cs (m_buttonUpdate_Click())
			//  so it should be refactored to a common location.
			//  Also see CheckIPAutoSave()

			const string csLableIppbxType =				"IP-PBX Type";
			const string csIppbxTypeAdtran =			"Adtran";
			const string csIppbxTypeAlcatelOxe =		"Alcatel/Lucent OXE";
			const string csIppbxTypeAlcatelOxo =		"Alcatel/Lucent OXO";
			const string csIppbxTypeAllworx =			"Allworx";
			const string csIppbxTypeAsterisk =			"Asterisk";
			const string csIppbxTypeFonalityTrixbox =	"Fonality/Trixbox";
			const string csIppbxTypeNecSphericall =		"NEC Sphericall";
			const string csIppbxTypeNortelCs =			"Nortel CS";
			const string csIppbxTypeNortelBcm =			"Nortel BCM";
			const string csIppbxTypeNortelScs =			"Nortel SCS";
			const string csIppbxTypeSipxecs =			"sipXecs";
			const string csIppbxTypeShoretel =			"ShoreTel";
			const string csIppbxTypeShoretelLegacy140 =	"ShoreTel legacy (14.0 and earlier)";
			const string csIppbxTypeSwitchvox =			"SwitchVox";
			const string csIppbxTypeZultys =			"Zultys";

			ConfigParams.ConfigParam cfg = i_cfgs[csLableIppbxType];

			if (null != cfg)
			{
				switch (cfg.Value)
				{
					case csIppbxTypeAsterisk:
					case csIppbxTypeFonalityTrixbox:
					case csIppbxTypeNortelScs:
					case csIppbxTypeSipxecs:
					case csIppbxTypeShoretel:
					case csIppbxTypeSwitchvox:
					case csIppbxTypeZultys:
						cfg.Value = "0";
						break;
					case csIppbxTypeAdtran:
					case csIppbxTypeAlcatelOxe:
					case csIppbxTypeAlcatelOxo:
					case csIppbxTypeAllworx:
					case csIppbxTypeNecSphericall:
					case csIppbxTypeNortelCs:
					case csIppbxTypeNortelBcm:
					case csIppbxTypeShoretelLegacy140:
					default:
						cfg.Value = "1";
						break;
				}
			}

			return cfg;
		} // GetPbxValue

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.Error.WriteLine(DateTime.Now.ToString() + ": " + "CurrentDomain_UnhandledException: " + e.ExceptionObject.ToString());
		}

		private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			Console.Error.WriteLine(DateTime.Now.ToString() + ": " + "CurrentDomain_DomainUnload");
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			Console.Error.WriteLine(DateTime.Now.ToString() + ": " + "CurrentDomain_DomainUnload");
		}
	}
}
