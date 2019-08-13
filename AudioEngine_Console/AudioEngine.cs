// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;

namespace AudioMgr
{
	/// <summary>
	/// Summary description for AudioEngine.
	/// </summary>
	class AudioEngine
	{
		public static string		cs_LogToFileEnabled =					"LogToFileEnabled";
		public static string		cs_LogToDatabaseEnabled =				"LogToDatabaseEnabled";
		public static string		cs_LogFilePath =						"LogFilePath";
		public static string		cs_DisableKeyboard =					"DisableKeyboard";
		public static string		cs_RemotingServerPort =					"RemotingServerPort";
		public static string		cs_RemotingDialogMgrUrl =				"RemotingDialogMgrUrl";
		public static string		cs_FirstAudioRtrPort =					"FirstAudioRtrPort";
		public static string		cs_AsrToLoad =							"AsrToLoad";
		public static string		cs_GrammarFormat =						"GrammarFormat";
		public static string		cs_SaveAudioStream =					"SaveAudioStream";
		public static string		cs_SaveUttWavs =						"SaveUttWavs";
		public static string		cs_DumpGram =							"DumpGram";
		public static string		cs_DtmfTimeout =						"DtmfTimeout";
		public static string		cs_DefaultInactivityTimeout =			"DefaultInactivityTimeout";
		public static string		cs_DefaultInactivityHangupTimeout =		"DefaultInactivityHangupTimeout";
		public static string		cs_DynamicPromptsPath =					"DynamicPromptsPath";
		public static string		cs_DynamicPromptsEnabled =				"DynamicPromptsEnabled";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool						bRes = true;
			string						sPrompt = "> ", sCmd = "", sLogPath = "";
			AudioEngine_srv				amSrv = null;
			ILegacyLogger				logger = null;
			string						sDisableKeyboard = "", sVer = "";
			AssemblyName				asmName = null;

			//Name the main thread
			Thread.CurrentThread.Name = "AudioMgrMainT";

			// Set up the logger(s)
			sLogPath = ConfigurationManager.AppSettings[cs_LogFilePath];
			if (!(sLogPath.EndsWith("/") || sLogPath.EndsWith("\\")))
			{
				sLogPath = sLogPath + "/";
			}
			logger = new LegacyLogger();
			logger.Init("", "", Thread.CurrentThread.Name, "", "", sLogPath);
			bRes = logger.Open();
			if (!bRes)
			{
				Console.Error.WriteLine("AudioMgr failed to open the logger!");
			}
			else
			{
				asmName = Assembly.GetAssembly(typeof(AudioMgr.AudioEngine)).GetName();
				sVer = asmName.Version.ToString();
				logger.Log(Level.Info, "AudioMgr v" + sVer);

				bRes = RemotingConfig(logger);

				amSrv = new AudioEngine_srv(logger);
				bRes = RegisterEventHandlers(amSrv);

				if (!bRes)
				{
					logger.Log(Level.Exception, " AudioMgr failed registering event handlers!");
				}
				else
				{
					bRes = amSrv.LoadASR(logger);
					if (!bRes)
					{
						logger.Log(Level.Exception, " AudioMgr failed to load ASR(s)!");
					}
					else
					{
						bRes = amSrv.CreateWorkerThreads(sLogPath);
						if (!bRes)
						{
							logger.Log(Level.Exception, " AudioMgr failed to create worker threads!");
						}
						else
						{
							//Console.WriteLine("AudioMgr startup successful.");
							logger.Log(Level.Info, "AudioMgr startup successful.");

							sDisableKeyboard = ConfigurationManager.AppSettings[cs_DisableKeyboard];
							sDisableKeyboard = (sDisableKeyboard == null) ? "" : sDisableKeyboard;
							if (sDisableKeyboard == "false")
							{
								Console.Write(sPrompt);
								sCmd = Console.ReadLine();
								while (bRes)
								{
									bRes = amSrv.ProcessCmdString(sCmd);
									if (bRes)
									{
										Console.Write(sPrompt);
										sCmd = Console.ReadLine();
									}
								}
							}

							// Join worker threads before exiting
							amSrv.JoinWorkerThreads();

							UnregisterEventHandlers(amSrv);
						}
					}
				}

				logger.Log(Level.Info, "AudioMgr shutdown.");
				logger.Close();
			}
		}

		private static bool RemotingConfig(ILegacyLogger i_Logger)
		{
			//string				sHostCfgFile, sDMCfgFile;
			bool		bRet = true;
			string		sTmp;

			try
			{
				/*
				// Load the server configuration file
				sHostCfgFile = "AudioEngine_host.Config";
				RemotingConfiguration.Configure(sHostCfgFile);

				// Load the client configuration files
				sDMCfgFile = "DialogMgr_client.Config";		// The DialogMgr config file
				RemotingConfiguration.Configure(sDMCfgFile);
				*/

				// Mono compatible remoting configuration
				// Server config
				sTmp = ConfigurationManager.AppSettings[cs_RemotingServerPort];
				sTmp = (sTmp == null) ? "" : sTmp;
				ChannelServices.RegisterChannel(new TcpChannel(int.Parse(sTmp)), false);
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(AudioMgr.AEMessaging), "AudioEngine.rem", WellKnownObjectMode.SingleCall);

				// Client config
				sTmp = ConfigurationManager.AppSettings[cs_RemotingDialogMgrUrl];
				sTmp = (sTmp == null) ? "" : sTmp;
				RemotingConfiguration.RegisterWellKnownClientType(typeof(ISMessaging.Delivery.ISMReceiverImpl), sTmp);
			}
			catch(Exception exc)
			{
				i_Logger.Log(exc);
				bRet = false;
			}

			return(bRet);
		}

		private static bool RegisterEventHandlers(AudioEngine_srv i_amSrv)
		{
			bool	bRet = true;

			AMEvHook.MsgDistr = new ISMessaging.Delivery.ISMDistributer();
			AMEvHook.MsgDistr.ISMMsg += new ISMessaging.Delivery.ISMDistributer.ISMDistributerEventHandler(i_amSrv.NewMsg);

			return(bRet);
		}

		private static bool UnregisterEventHandlers(AudioEngine_srv i_amSrv)
		{
			bool	bRet = true;

			ISMessaging.Delivery.ISMDistributer.ISMDistributerEventHandler deh = new ISMessaging.Delivery.ISMDistributer.ISMDistributerEventHandler(i_amSrv.NewMsg);
			AMEvHook.MsgDistr.ISMMsg -= deh;

			return(bRet);
		}
	} // class AudioEngine
}
