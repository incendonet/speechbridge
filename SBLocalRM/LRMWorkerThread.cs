// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using SBConfigStor;


namespace SBLocalRM
{
	/// <summary>
	/// Summary description for LRMWorkerThread.
	/// </summary>
	public class LRMWorkerThread
	{
		private	ILegacyLogger	m_Logger = null;
		private	MsgQueue		m_aqMsgIn = null;
		private	SBProcesses		m_aProcs = null;
		private bool			m_bDesktopRuntime = false;

		protected const string m_csAppNameProxysrv =			"ProxySrv";
		protected const string m_csAppNameAudiortr =			"AudioRtr";
		protected const string m_csAppNameAudiomgr =			"AudioMgr";
		protected const string m_csAppNameDialogmgr =			"DialogMgr";
		protected const string m_csAppNameSbsched =				"SBSched";
		protected const string m_csAppNameSblocalrm =			"SBLocalRM";
		protected const string m_csAppNameSblauncher =			"SBLauncher";

		public	SBProcesses		Procs
		{
			get { return(m_aProcs); }
		}

		public LRMWorkerThread(ILegacyLogger i_Logger, MsgQueue i_aqMsgIn, bool i_bDesktopRuntime)
		{
			m_Logger = i_Logger;
			m_aqMsgIn = i_aqMsgIn;
			m_aProcs = new SBProcesses();
			m_bDesktopRuntime = i_bDesktopRuntime;
		} // LRMWorkerThread constructor

		public void ThreadProc()
		{
			string	sTmp = "";
			bool	bRes = true;

			Thread.CurrentThread.Name = "LRMWorkerT";
			m_Logger.Init("", "", Thread.CurrentThread.Name, "", "", "");
			m_Logger.Log(Level.Verbose, "LRMWorkerThread started.");

			// Check cfg file if we are to auto-start
			sTmp = ConfigurationManager.AppSettings["AutoStart"];
			if(sTmp.ToLower() == true.ToString().ToLower())
			{
				bRes = StartSBComponents();
			}

			MsgLoop();

			m_Logger.Log(Level.Verbose, "LRMWorkerThread exiting.");
		} // ThreadProc

		public void MsgLoop()
		{
			bool		bDone = false, bRes = true;
			RMMsg		rmMsg = null;
			XmlMsg		xMsg = null;

			while(!bDone)
			{
				try
				{
					rmMsg = (RMMsg)(m_aqMsgIn.Pop());
					xMsg = XmlMsg.Parse(rmMsg.m_sMsgDecoded);

					switch(xMsg.MsgType)
					{
						case XmlMsg.eMsgTypes.Ack :
						case XmlMsg.eMsgTypes.NOP :
						case XmlMsg.eMsgTypes.Ping :
						{	// All messages we can ignore
						}
						break;
						case XmlMsg.eMsgTypes.SBResetComp :
						{
							bRes = ResetSBComponents(xMsg);
						}
						break;
						default :
						{
							m_Logger.Log(Level.Warning, string.Format("LRMWorkerThread.MsgLoop: Unsupported msg type: {0}.{1}", xMsg.MsgType.ToString(), xMsg.MsgType));
						}
						break;
					}

					xMsg = null;
					rmMsg = null;
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
			}
		} // MsgLoop

		public bool StartSBComponents()
		{
			bool				bRet = true, bRes = true;
			string				sStarts = "";
			StringCollection	asBins = null, asMonoBins = null;
			StringBuilder		sbLauncherApps = null;
			int					ii, iMaxSessions = 0;
			string				sMonoBinPath = "", sBinPath = "", sCfgPath = "", sLogPath = "";
			string				sCmd = "";
			StringBuilder		sbArgs = null;
			char				cSlash;
			string[]			asLauncherSupportedApps = { m_csAppNameAudiortr };		// FIX:  This should instead by initialized by calling `SBLauncher --supported` and reading in the results from stdin.
			bool				bLauncherStartedAudiortr = false;

			try
			{
				if(RunningSystem.RunningPlatform == CLRPlatform.Mono)
				{
					cSlash = '/';
				}
				else
				{
					//cSlash = '\\';	// We can use '/' on Windows too.
					cSlash = '/';
				}

				sMonoBinPath = ConfigurationManager.AppSettings["MonoBinPath"];
				if( !(sMonoBinPath.EndsWith("/")) && !(sMonoBinPath.EndsWith("\\")) )
				{
					sMonoBinPath += cSlash;
				}
				sBinPath = ConfigurationManager.AppSettings["DefaultBinPath"];
				if( !(sBinPath.EndsWith("/")) && !(sBinPath.EndsWith("\\")) )
				{
					sBinPath += cSlash;
				}

				if(m_bDesktopRuntime == true)
				{
					sMonoBinPath += "mono " + SBLocalRM.g_sDesktopArg + " " + sBinPath;
				}
				else
				{
					sMonoBinPath += "mono " + sBinPath;
				}

				sLogPath = ConfigurationManager.AppSettings["DefaultLogFilePath"];
				if( !(sLogPath.EndsWith("/")) && !(sLogPath.EndsWith("\\")) )
				{
					sLogPath += cSlash;
				}

				sCfgPath = ConfigurationManager.AppSettings["DefaultConfigPath"];
				if( !(sCfgPath.EndsWith("/")) && !(sCfgPath.EndsWith("\\")) )
				{
					sCfgPath += cSlash;
				}

				iMaxSessions = ConfigParams.GetNumExt();

				// Binary executables to start
				asBins = new StringCollection();
				sStarts = ConfigurationManager.AppSettings["BinsToStart"];
				bRes = Utilities.GetItemsFromString(sStarts, ';', asBins);

				// Mono binaries to start
				asMonoBins = new StringCollection();
				sStarts = ConfigurationManager.AppSettings["MonoBinsToStart"];
				bRes = Utilities.GetItemsFromString(sStarts, ';', asMonoBins);

				sbArgs = new StringBuilder();

				// Launch Mono executables
				try
				{
					if(asMonoBins.Contains(m_csAppNameAudiomgr))
					{
						sbArgs.Length = 0;
//						sCmd = sMonoBinPath + "AudioMgr.exe";
						sCmd = sBinPath + "AudioMgr.sh";			// Launch script to get around bug introduced in Mono 2.6.3 where Mono EXEs fail to launch directly.  FIX - Change this back once they fix it, we don't want the extra 'sh' overhead.

						/* Redirection like this doesn't work on mono/linux
						sCmd += " >/dev/null";	// FIX - won't work on Mono for Windows

						if(sLogPath.Length > 0)
						{
							sbArgs.AppendFormat(" 2>>{0}AudioMgr.stderr.log", sLogPath);
						}
						else
						{
							sbArgs.AppendFormat("2>>/home/speechbridge/logs/AudioMgr.stderr.log");	// FIX - Won't work on Mono for Windows
						}
						*/

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameAudiomgr, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
				try
				{
					if(asMonoBins.Contains(m_csAppNameDialogmgr))
					{
						sbArgs.Length = 0;
//						sCmd = sMonoBinPath + "DialogMgr.exe";
						sCmd = sBinPath + "DialogMgr.sh";			// Launch script to get around bug introduced in Mono 2.6.3 where Mono EXEs fail to launch directly.  FIX - Change this back once they fix it, we don't want the extra 'sh' overhead.

						/*
						sCmd += " >/dev/null";	// FIX - won't work on Mono for Windows

						if(sLogPath.Length > 0)
						{
							sbArgs.AppendFormat("2>>{0}DialogMgr.stderr.log", sLogPath);
						}
						else
						{
							sbArgs.AppendFormat("2>>/home/speechbridge/logs/DialogMgr.stderr.log");
						}
						*/

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameDialogmgr, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
				try
				{
					if(asMonoBins.Contains(m_csAppNameSbsched))
					{
						sbArgs.Length = 0;
//						sCmd = sMonoBinPath + "SBSched.exe";
						sCmd = sBinPath + "SBSched.sh";			// Launch script to get around bug introduced in Mono 2.6.3 where Mono EXEs fail to launch directly.  FIX - Change this back once they fix it, we don't want the extra 'sh' overhead.

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameSbsched, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}

				// Launch executables	// FIX - Check that Mono bins aren't here?  (I.e., config file error)
				try
				{
					if(asBins.Contains(m_csAppNameAudiomgr))
					{
						sbArgs.Length = 0;
						sCmd = sBinPath + "AudioMgr.exe";

						/* Redirection like this doesn't work on mono/linux
						if(sLogPath.Length > 0)
						{
							sbArgs.AppendFormat("2>>{0}AudioMgr.stderr.log", sLogPath);
						}
						*/

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameAudiomgr, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
				try
				{
					if(asBins.Contains(m_csAppNameDialogmgr))
					{
						sbArgs.Length = 0;
						sCmd = sBinPath + "DialogMgr.exe";

						/* Redirection like this doesn't work on mono/linux
						if(sLogPath.Length > 0)
						{
							sbArgs.AppendFormat("2>>{0}DialogMgr.stderr.log", sLogPath);
						}
						*/

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameDialogmgr, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
				try
				{
					if(asBins.Contains(m_csAppNameSbsched))
					{
						sbArgs.Length = 0;
						sCmd = sBinPath + "SBSched.exe";

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameSbsched, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}

				// Launch SIP Proxy server
				try
				{
					if(asBins.Contains(m_csAppNameProxysrv))
					{
						sbArgs.Length = 0;
						sCmd = sBinPath + m_csAppNameProxysrv;
						sbArgs.AppendFormat("-x -i {0}ProxySrv.config -l {1}ProxySrv.log", sCfgPath, sLogPath);

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameProxysrv, sCmd, sbArgs.ToString());
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}

				// Start SBLauncher.  For now, this should be second to last, since it is only starting AudioRtr.
				// FIX:  If it takes on more launching duties, it should be moved accordingly.
				try
				{
					if (asBins.Contains(m_csAppNameSblauncher))
					{
						sbLauncherApps = new StringBuilder();
						for (ii = 0; ii < asLauncherSupportedApps.Length; ii++)
						{
							if (asBins.Contains(asLauncherSupportedApps[ii]))
							{
								sbLauncherApps.Append(asLauncherSupportedApps[ii]);
								sbLauncherApps.Append(' ');

								if (asLauncherSupportedApps[ii] == m_csAppNameAudiortr)
								{
									bLauncherStartedAudiortr = true;
								}
							}
							else
							{
								m_Logger.Log(Level.Warning, "LWT.StartSBComponents: Unsupported app '" + asLauncherSupportedApps[ii] + "'.");
							}
						}
						sbArgs.Length = 0;
						sCmd = sBinPath + m_csAppNameSblauncher;
						sbArgs.AppendFormat("{0} {1} {2} {3} {4}", iMaxSessions, sBinPath, sCfgPath, sLogPath, sbLauncherApps.ToString());

						m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
						bRes = StartSBProc(m_csAppNameSblauncher, sCmd, sbArgs.ToString());
					}
				}
				catch (Exception exc)
				{
					m_Logger.Log(exc);
				}

				// Launch AudioRtrs last
				try
				{
					if(!bLauncherStartedAudiortr && asBins.Contains(m_csAppNameAudiortr) )
					{
						for(ii = 0; ii < iMaxSessions; ii++)
						{
							sbArgs.Length = 0;

							//sCmd = sBinPath + "gua";
							sCmd = sBinPath + m_csAppNameAudiortr;

							//sbArgs.AppendFormat("-f {0}{1}.cfg >>{2}AudioRtr_p{3}.log 2>>{4}AudioRtr_p{5}.stderr.log", sCfgPath, ii, sLogPath, ii, sLogPath, ii);
							sbArgs.AppendFormat("-f {0}{1}.cfg", sCfgPath, ii);

							m_Logger.Log(Level.Debug, "LRMWorkerThread.StartSBComponents: Starting '" + sCmd + " " + sbArgs.ToString() + "'.");
							bRes = StartSBProc(m_csAppNameAudiortr, sCmd, sbArgs.ToString());
						}
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(exc);
			}

			return(bRet);
		} // StartSBComponents

		public bool StartSBProc(string i_sName, string i_sCmd, string i_sArgs)
		{
			bool		bRet = true;
			SBProcess	procChild = null;

			try
			{
				procChild = new SBProcess();
				m_aProcs.Add(procChild);		// Always add, even if it doesn't start because we may want to try again.

				procChild.Name = i_sName;
				procChild.Command = i_sCmd + " " + i_sArgs;

				procChild.Proc = new Process();
				procChild.Proc.Exited += new EventHandler(proc_Exited);
				procChild.Proc.EnableRaisingEvents = true;
				//proc.SynchronizingObject = this;		// Do we need to do this?  Requires ISynchronizeInvoke

				//procChild.Proc.StartInfo.CreateNoWindow = true;
				//procChild.Proc.StartInfo.UseShellExecute = false;
				//procChild.Proc.StartInfo.WorkingDirectory = i_sWorkingDir;
				procChild.Proc.StartInfo.FileName = i_sCmd;
				procChild.Proc.StartInfo.Arguments = i_sArgs;

				bRet = procChild.Proc.Start();
				if(!bRet)
				{
					m_Logger.Log(Level.Exception, "StartSBProc Failed to Start('" + i_sCmd + "', '" + i_sArgs + "')!");
				}
				else
				{
					m_Logger.Log(Level.Info, "StartSBProc Start-ed('" + i_sCmd + "', '" + i_sArgs + "').");
				}

				procChild.Running = bRet;
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "StartSBProc tried to Start('" + i_sCmd + "', '" + i_sArgs + "'), caught exception: " + exc.ToString());
			}

			return(bRet);
		} // StartSBProc

		/// <summary>
		/// Note: The behavior of the code below is really only appropriate for AudioRtr,
		/// since it kills off all of the specified components, then restarts them, which
		/// is not appropriate for AudioMgr & DialogMgr.
		/// </summary>
		/// <param name="i_XmlMsg"></param>
		/// <returns></returns>
		public bool ResetSBComponents(XmlMsg i_XmlMsg)
		{
#if(false)
			bool				bRet = true, bRes = true;
			int					iRes = -1;
			string				sComp = "", sName = "", sFile = "", sArgs = "";
			int					ii = 0, iNumComps = 0;
			StringCollection	asNames = null, asFiles = null, asArgs = null;
			bool				bFound = false;

			try
			{
				sComp = i_XmlMsg.Data;
				m_Logger.Log(Level.Info, "LRMWorkerThread.ResetSBComponents: Restarting '" + sComp + "'.");
				asNames = new StringCollection();
				asFiles = new StringCollection();
				asArgs = new StringCollection();

				// Kill off components
				do
				{
					iRes = FindComponentInProcessList(sComp, out sFile, out sArgs);
					if(iRes == -1)
					{
					}
					else
					{
						bFound = true;
						sName = m_aProcs[iRes].Name;
						if(m_aProcs[iRes].Running)
						{
							m_aProcs[iRes].Proc.EnableRaisingEvents = false;
							m_aProcs[iRes].Proc.Exited -= new EventHandler(proc_Exited);
							m_aProcs[iRes].Proc.Kill();
						}
						m_aProcs.RemoveAt(iRes);

						// Add to list to restart (even if it didn't start before.)
						asNames.Add(sName);
						asFiles.Add(sFile);
						asArgs.Add(sArgs);
						iNumComps++;
					}
				}
				while(iRes != -1);

				if(!bFound)
				{
					m_Logger.Log(Level.Warning, "LRMWorkerThread.ResetSBComponents: Couldn't find '" + sComp + "' in the list.");
				}

				// Restart components
				for(ii = 0; ii < iNumComps; ii++)
				{
					bRes = StartSBProc(asNames[ii], asFiles[ii], asArgs[ii]);
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(exc);
			}
#else
			bool			bRet = true;
			char			cSlash = '/';
			string			sBinPath = "", sCmd = "", sArgs = "";
			System.Diagnostics.Process		procCmd;

			try
			{
				// Disable raising 'exited' event
				foreach(SBProcess procChild in m_aProcs)
				{
					procChild.Proc.EnableRaisingEvents = false;
				}

				// Format command and args
				if(RunningSystem.RunningPlatform == CLRPlatform.Mono)
				{
					cSlash = '/';
				}
				else
				{
					//cSlash = '\\';	// We can use '/' on Windows too.
					cSlash = '/';
				}

				sBinPath = ConfigurationManager.AppSettings["DefaultBinPath"];
				if( !(sBinPath.EndsWith("/")) && !(sBinPath.EndsWith("\\")) )
				{
					sBinPath += cSlash;
				}
				sCmd = sBinPath + "speechbridged";
				sArgs = "restart";

				procCmd = new Process();
				procCmd.StartInfo.FileName = sCmd;
				procCmd.StartInfo.Arguments = sArgs;

				m_Logger.Log(Level.Debug, "LRMWorkerThread.ResetSBComponents: Starting '" + sCmd + "', '" + sArgs + "'.");

				// Run the 'reset' command
				bRet = procCmd.Start();

				Environment.Exit(0);
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "LRMWorkerThread.ResetSBComponents caught exception: " + exc.ToString());
			}

#endif

			return(bRet);
		} // ResetSBComponents

		private void proc_Exited(object sender, System.EventArgs e)
		{
			int			iPid = -1, ii = 0;
			SBProcess	proc = null;
			bool		bRes = true;
			string		sFile = "", sArgs = "", sAMFile = "", sAMArgs = "", sDMFile = "", sDMArgs = "";

			try
			{
				Monitor.Enter(m_aProcs);

				iPid = ((Process)sender).Id;

				for(ii = 0; ((ii < m_aProcs.Count) && (proc == null) ); ii++)
				{
					if(m_aProcs[ii].Running)
					{
						if(m_aProcs[ii].Proc.Id == iPid)
						{
							proc = m_aProcs[ii];
							m_aProcs.RemoveAt(ii);
						}
					}
				}

				if(proc == null)
				{
					m_Logger.Log(Level.Warning, "LRMWorkerThread.proc_Exited: PID '" + iPid.ToString() + "' not found in list!  Reason: " + e.ToString());
				}
				else
				{
					// AudioMgr and DialogMgr need to be restarted together, others are independent.
					if(proc.Proc.StartInfo.FileName.ToLower().IndexOf(m_csAppNameAudiomgr) > 0)
					{
						m_Logger.Log(Level.Warning, "LRMWorkerThread.proc_Exited: AudioMgr exited!  Reason: " + e.ToString());

						sAMFile = proc.Proc.StartInfo.FileName;
						sAMArgs = proc.Proc.StartInfo.Arguments;

						// Find DialogMgr and kill it (turning off events first.)
						// (Traverse entire list in case there are ligering copies running.)
						do
						{
							ii = FindComponentInProcessList(m_csAppNameDialogmgr, out sFile, out sArgs);
							if(ii != -1)
							{
								sDMFile = sFile;
								sDMArgs = sArgs;
								if(m_aProcs[ii].Running)	// Only attempt to kill if it had been started
								{
									m_aProcs[ii].Proc.EnableRaisingEvents = false;
									m_aProcs[ii].Proc.Exited -= new EventHandler(proc_Exited);
									m_aProcs[ii].Proc.Kill();
								}
								m_aProcs.RemoveAt(ii);
							}
						}
						while(ii != -1);

						// Start AudioMgr and DialogMgr back up.
						m_Logger.Log(Level.Info, "LRMWorkerThread.proc_Exited: Restarting AudioMgr & DialogMgr.");
						bRes = StartSBProc(m_csAppNameAudiomgr, sAMFile, sAMArgs);
						bRes = StartSBProc(m_csAppNameDialogmgr, sDMFile, sDMArgs);
					}
					else if (proc.Proc.StartInfo.FileName.ToLower().IndexOf(m_csAppNameDialogmgr) > 0)
					{
						m_Logger.Log(Level.Warning, "LRMWorkerThread.proc_Exited: DialogMgr exited!  Reason: " + e.ToString());

						sDMFile = proc.Proc.StartInfo.FileName;
						sDMArgs = proc.Proc.StartInfo.Arguments;

						// Find AudioMgr and kill it (turning off events first.)
						// (Traverse entire list in case there are ligering copies running.)
						do
						{
							ii = FindComponentInProcessList(m_csAppNameAudiomgr, out sFile, out sArgs);
							if(ii != -1)
							{
								sAMFile = sFile;
								sAMArgs = sArgs;
								if(m_aProcs[ii].Running)	// Only attempt to kill if it had been started
								{
									m_aProcs[ii].Proc.EnableRaisingEvents = false;
									m_aProcs[ii].Proc.Exited -= new EventHandler(proc_Exited);
									m_aProcs[ii].Proc.Kill();
								}
								m_aProcs.RemoveAt(ii);
							}
						}
						while(ii != -1);

						// Start AudioMgr and DialogMgr back up.
						m_Logger.Log(Level.Info, "LRMWorkerThread.proc_Exited: Restarting AudioMgr & DialogMgr.");
						bRes = StartSBProc(m_csAppNameAudiomgr, sAMFile, sAMArgs);
						bRes = StartSBProc(m_csAppNameDialogmgr, sDMFile, sDMArgs);
					}
					else
					{
						m_Logger.Log(Level.Warning, "LRMWorkerThread.proc_Exited: " + proc.Proc.StartInfo.FileName + " exited!  Restarting it.  (Args: '" + proc.Proc.StartInfo.Arguments + "')  Reason: " + e.ToString());

						bRes = StartSBProc(proc.Name, proc.Proc.StartInfo.FileName, proc.Proc.StartInfo.Arguments);
					}

					proc = null;
				}
			}
			catch(Exception exc)
			{
				m_Logger.Log(exc);
			}
			finally
			{
				Monitor.Exit(m_aProcs);
			}

		} // proc_Exited

		public int FindComponentInProcessList(string i_sComponentName, out string o_sFile, out string o_sArgs)
		{
			int		iRet = -1;
			int		ii = 0;

			o_sFile = "";
			o_sArgs = "";

			try
			{
				for(ii = 0; ( (ii < m_aProcs.Count) && (iRet == -1) ); ii++)
				{
					if(m_aProcs[ii].Proc.StartInfo.FileName.ToLower().IndexOf(i_sComponentName.ToLower()) > 0)
					{
						iRet = ii;
						o_sFile = m_aProcs[ii].Proc.StartInfo.FileName;
						o_sArgs = m_aProcs[ii].Proc.StartInfo.Arguments;
					}
				}
			}
			catch(Exception exc)
			{
				iRet = -1;
				m_Logger.Log(exc);
			}

			return(iRet);
		} // FindComponentInProcessList
	} // LRMLocalWorkerThread
}
