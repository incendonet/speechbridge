// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using ISMessaging.Audio;
using SBConfigStor;
using SBResourceMgr;
using SBTTS;

namespace AudioMgr
{
	/// <summary>
	/// 
	/// </summary>
	public enum eSockResult
	{
		OK = 0,
		Warning,
		Fatal,
	};

	public enum eSockError
	{
		OK =				0,
		Unknown =			1,
		Retry =				2,
		CONNRESET =		10054,
		SHUTDOWN =		10058,
	};

	// FIX - Move to separate file.
	public class AudioOutThread
	{
		private AudioOutMsgQueue						m_qMsg = null;
		private IResourceMgr							m_RM = null;
		private AMSockConns								m_SockConns = null;
		private AMSockConns.AMSockConn					m_SockConn = null;
		private	MsgQueue[]								m_aqAudioIn = null;
		private	AudioOutMsgQueue[]						m_aqAudioOut = null;
		private int										m_iThreadIndex;
		private int										m_iAudioTimeout;
		private ISMessaging.Delivery.ISMReceiverImpl	m_mRcv = null;
		private StringCollection						m_asAlertMsgs = null;

        private AudioOutTimer m_BargeinDisabledTimer = null;
        private AudioOutTimer m_CallHangupTimer = null;
        private AudioOutTimer m_InactivityTimer = null;
        private AudioOutTimer m_InactivityHangupTimer = null;
        private AudioOutTimer m_PromptsPlayTimer = null;

        private TimeSpan m_DefaultInactivityTimeout;
        private TimeSpan m_InactivityHangupTimeout;
        private TimeSpan m_CallHangupTimeout;
		private string									m_sLastLog = "";
		private bool									m_bInSession = false;
		private TtsLanguageCodeMapping					m_TtsLanguageCodeMapping = null;

		private const double DEFAULT_CALL_TIMEOUT_IN_MINUTES = 20.0;                 // Terminate call after 20 minutes, regardless of activity.
        private const double DEFAULT_INACTIVITY_HANGUP_TIMEOUT_IN_MINUTES = 1.0;     // Hangup after 1 minute of inactivity.
        private const int DEFAULT_INACTIVITY_TIMEOUT_IN_MILLISECONDS = 15000;        // Repeat prompts after 15 seconds.
		private const int MINUTES_TO_MILLISECONDS = 60000;

		protected ILegacyLogger							m_Logger = null;

        private bool m_bDialogManagerIsBusy = false;
        private string m_sSessionEndReason = "Unknown";


		public AudioOutThread(ILegacyLogger i_Logger, IResourceMgr i_RM, int i_iThreadIndex, AudioOutMsgQueue i_qMsg, int i_iAudioTimeout, AMSockConns i_SockConns, MsgQueue[] i_aqAudioIn, AudioOutMsgQueue[] i_aqAudioOut)
		{
			string		sTmp = "";

			m_Logger = i_Logger;
			m_qMsg = i_qMsg;
			m_RM = i_RM;
			m_iThreadIndex = i_iThreadIndex;
			m_iAudioTimeout = i_iAudioTimeout;
			m_SockConns = i_SockConns;
			m_aqAudioIn = i_aqAudioIn;
			m_aqAudioOut = i_aqAudioOut;

			m_asAlertMsgs = new StringCollection();
			m_asAlertMsgs.Add("ISMessaging.Audio.ISMSpeechStart");
			m_asAlertMsgs.Add("ISMessaging.Audio.ISMSpeechStop");                   //$$$ LP - Why do we need to look for this?  The ISMSpeechStart message should have cleared out all the prompts.
            m_asAlertMsgs.Add("ISMessaging.Audio.ISMDtmfStart");
            m_asAlertMsgs.Add("ISMessaging.Audio.ISMDtmfStop");                     //$$$ LP - Why do we need to look for this?  The ISMDtmfStart message should have cleared out all the prompts.
            m_asAlertMsgs.Add("ISMessaging.Session.ISMSessionBegin");
			m_asAlertMsgs.Add("ISMessaging.Session.ISMSessionEnd");


			// Get DefaultInactivityTimeout (for repeating prompts)

            double dDefaultInactivityTimeoutInMilliseconds = DEFAULT_INACTIVITY_TIMEOUT_IN_MILLISECONDS;

			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_DefaultInactivityTimeout];

            if (!String.IsNullOrEmpty(sTmp))
            {
                try
                {
                    dDefaultInactivityTimeoutInMilliseconds = double.Parse(sTmp);
                }
                catch (Exception exc)
                {
                    m_Logger.Log(Level.Exception, String.Format("[{0}]AudioOutThread: Exception parsing DefaultInactivityTimeout from config file: ", m_iThreadIndex, exc.ToString()));
                }
            }
            else
            {
                m_Logger.Log(Level.Config, String.Format("[{0}]AudioOutThread: DefaultInactivityTimeout entry missing or empty in config file.", m_iThreadIndex));
            }

            m_DefaultInactivityTimeout = new TimeSpan(0, 0, 0, 0, (int)dDefaultInactivityTimeoutInMilliseconds);
            m_Logger.Log(Level.Info, String.Format("[{0}]AudioOutThread: DefaultInactivityTimeout set to {1}.", m_iThreadIndex, m_DefaultInactivityTimeout));


			// Get DefaultInactivityHangupTimeout (to hang up on inactive calls)

            double dInactivityHangupTimeoutInMilliseconds = DEFAULT_INACTIVITY_HANGUP_TIMEOUT_IN_MINUTES * MINUTES_TO_MILLISECONDS;

            sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_DefaultInactivityHangupTimeout];

            if (!String.IsNullOrEmpty(sTmp))
            {
                try
                {
                    dInactivityHangupTimeoutInMilliseconds = double.Parse(sTmp);
                }
                catch (Exception exc)
                {
                    m_Logger.Log(Level.Exception, String.Format("[{0}]AudioOutThread: Exception parsing DefaultInactivityHangupTimeout from config file: ", m_iThreadIndex, exc.ToString()));
                }
            }
            else
            {
                m_Logger.Log(Level.Config, String.Format("[{0}]AudioOutThread: DefaultInactivityHangupTimeout entry missing or empty in config file.", m_iThreadIndex));
            }

            m_InactivityHangupTimeout = new TimeSpan(0, 0, 0, 0, (int)dInactivityHangupTimeoutInMilliseconds);
            m_Logger.Log(Level.Info, String.Format("[{0}]AudioOutThread: InactivityHangupTimeout set to {1}.", m_iThreadIndex, m_InactivityHangupTimeout));


            // Get CallTimeout from DB

            double dCallHangupTimeoutInMilliseconds = DEFAULT_CALL_TIMEOUT_IN_MINUTES * MINUTES_TO_MILLISECONDS;

			ConfigParams cfgs = new ConfigParams();
			bool bRes = cfgs.LoadFromTableByComponent(ConfigParams.e_Components.Apps.ToString() + ConfigParams.c_Separator + ConfigParams.e_Components.global.ToString());
			if (!bRes)
			{
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread: Couldn't retrieve DB settings!");
			}
			else
			{
				for (int ii = 0; ii < cfgs.Count; ++ii)
				{
					if (cfgs[ii].Name == ConfigParams.e_SpeechAppSettings.CallTimeout.ToString())
					{
						try
						{
                            dCallHangupTimeoutInMilliseconds = double.Parse(cfgs[ii].Value) * MINUTES_TO_MILLISECONDS;
						}
						catch (Exception exc)
						{
							m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread: Exception parsing CallTimeout from DB: " + exc.ToString());
						}
					}
				}
			}

            m_CallHangupTimeout = new TimeSpan(0, 0, 0, 0, (int)dCallHangupTimeoutInMilliseconds);
            m_Logger.Log(Level.Info, String.Format("[{0}]AudioOutThread: CallTimeout set to {1}.", m_iThreadIndex, m_CallHangupTimeout));

            m_BargeinDisabledTimer = new AudioOutTimer("BargeinDisabled", OnBargeinDisabledHandler, m_iThreadIndex, m_Logger);
            m_CallHangupTimer = new AudioOutTimer("CallHangup", OnCallHangupHandler, m_iThreadIndex, m_Logger);
            m_InactivityTimer = new AudioOutTimer("Inactivity", OnInactivityHandler, TimerArmMode.Manual, m_iThreadIndex, m_Logger);
            m_InactivityHangupTimer = new AudioOutTimer("InactivityHangup", OnInactivityHangupHandler, m_iThreadIndex, m_Logger);
            m_PromptsPlayTimer = new AudioOutTimer("PromptsPlay", OnPromptsPlayHandler, m_iThreadIndex, m_Logger);

			m_TtsLanguageCodeMapping = new TtsLanguageCodeMapping(new TtsLanguageCodeMappingDAL());
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Level"></param>
		/// <param name="i_sLogStr"></param>
		/// <returns></returns>
		public bool Log(Level i_Level, string i_sLogStr)
		{
			bool		bRet = true;

			try
			{
				if(i_sLogStr != m_sLastLog)
				{
					// Been having an issue with AudioMgr logs stopping, so write non-LV exceptions to stderr, so we don't miss them.
					// Skipping LV errors, because when testing with more ports than licenses will flood us with known errors.
					if( ((i_Level == Level.Exception) || (i_Level == Level.Warning)) && (i_sLogStr.IndexOf("LV_SRE") == -1) && (i_sLogStr.IndexOf("Didn't get any results") == -1) )
					{
						Console.Error.WriteLine(DateTime.Now.ToString() + " " + i_sLogStr);
					}

					// Log to logger(s)
					m_Logger.Log(i_Level, i_sLogStr);
					m_sLastLog = i_sLogStr;
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + " AudioOutThread.Log exception: " + exc.ToString());
			}

			return(bRet);
		}

		public bool RemotingInit()
		{
			bool		bRet = true;
			Type		tRcv;
			string		sTmp = "";

			try
			{
				tRcv = typeof(AudioMgr.AMPinger);
				if(tRcv == null)
				{
					bRet = false;
					Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.RemotingInit:  Couldn't get type.");
				}
				else
				{
					sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_RemotingDialogMgrUrl];
					sTmp = (sTmp == null) ? "" : sTmp;
					if (sTmp.Length == 0)
					{
						m_mRcv = (ISMessaging.Delivery.ISMReceiverImpl)(Activator.GetObject(tRcv, "tcp://localhost:1778/DialogMgr.rem"));
					}
					else
					{
						m_mRcv = (ISMessaging.Delivery.ISMReceiverImpl)(Activator.GetObject(tRcv, sTmp));
					}

					if(m_mRcv == null)
					{
						bRet = false;
						Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.RemotingInit:  Couldn't create DialogMgr remote object.");
					}
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.RemotingInit exception: " + exc.ToString());
			}

			return(bRet);
		}

        private void OnInactivityHandler()
        {
            bool bRes = true;
            ISMessaging.Session.ISMTimerExpired mExp = null;
            ISMessaging.ISAppEndpoint src = null, dest = null;
            ISMVMC mVMC = null;

            try
            {
                src = new ISAppEndpoint();
                dest = new ISAppEndpoint();
                mVMC = new ISMVMC();
                mVMC.Init(m_iThreadIndex, "AudioMgr", m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId);		// Note - We're assuming that a null VMC is never returned
                src.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Inactivity timer elapsed");
                dest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Inactivity timer elapsed");
                mExp = new ISMessaging.Session.ISMTimerExpired(src);
                mExp.m_Dest = dest;

                bRes = m_mRcv.NewMsg(mExp);
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("[{0}]AudioOutThread.OnInactivityHandler exception: {1}", m_iThreadIndex, exc.ToString()));
            }
        }

        private void OnInactivityHangupHandler()
        {
            ISMessaging.ISAppEndpoint src = null, dest = null;
            ISMVMC mVMC = null;
            ISMessaging.Session.ISMTerminateSession mHangup = null;

            try
            {
                src = new ISAppEndpoint();
                dest = new ISAppEndpoint();
                mVMC = new ISMVMC();
                mVMC.Init(m_iThreadIndex, "AudioMgr", m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId);		// Note - We're assuming that a null VMC is never returned
                src.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Inactivity hangup timer elapsed");
                dest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Inactivity hangup timer elapsed");

                // Send hangup message to DialogMgr ??

                // Send hangup message to AudioRtr

                m_sSessionEndReason = "Inactivity";

                mHangup = new ISMessaging.Session.ISMTerminateSession(src);
                mHangup.m_Dest = dest;
                m_aqAudioOut[m_iThreadIndex].Push(mHangup);
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("[{0}]AudioOutThread.OnInactivityHangupHandler exception: {1}", m_iThreadIndex, exc.ToString()));
            }
        }

        private void OnBargeinDisabledHandler()
        {
            try
            {
                SendAudioinBargeinEnable();
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("[{0}]AudioOutThread.OnBargeinDisabledHandler exception: {1}", m_iThreadIndex, exc.ToString()));
            }

        }

        private void OnCallHangupHandler()
        {
            ISMessaging.ISAppEndpoint src = null, dest = null;
            ISMVMC mVMC = null;
            ISMessaging.Session.ISMTerminateSession mHangup = null;

            try
            {
                src = new ISAppEndpoint();
                dest = new ISAppEndpoint();
                mVMC = new ISMVMC();
                mVMC.Init(m_iThreadIndex, "AudioMgr", m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId);		// Note - We're assuming that a null VMC is never returned
                src.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Call hangup timer elapsed");
                dest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Call hangup timer elapsed");

                // Send hangup message to DialogMgr ??

                // Send hangup message to AudioRtr

                m_sSessionEndReason = "Duration";

                mHangup = new ISMessaging.Session.ISMTerminateSession(src);
                mHangup.m_Dest = dest;
                m_aqAudioOut[m_iThreadIndex].Push(mHangup);
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("[{0}]AudioOutThread.OnCallHangupElapsed exception: {1}", m_iThreadIndex, exc.ToString()));
            }
        }

        private void OnPromptsPlayHandler()
        {
            try
            {
                m_InactivityTimer.Start(m_DefaultInactivityTimeout);

                ISMessaging.Audio.ISMPlayPromptsComplete mPlayPromptsComplete = new ISMPlayPromptsComplete();
                mPlayPromptsComplete.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		// Note - We're assuming that a null VMC is never returned

                m_aqAudioOut[m_iThreadIndex].Push(mPlayPromptsComplete);
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("[{0}]AudioOutThread.OnPromptsPlayHandler exception: {1}", m_iThreadIndex, exc.ToString()));
            }
        }

		private bool SendAudioinBargeinEnable()
		{
			bool									bRet = true;
			ISMessaging.ISAppEndpoint				src = null, dest = null;
			ISMVMC									mVMC = null;
			ISMBargeinEnable						mBIEnable = null;

			try
			{
				src = new ISAppEndpoint();
				dest = new ISAppEndpoint();
				mVMC = new ISMVMC();
				mVMC.Init(m_iThreadIndex, "AudioMgr", m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId);		// Note - We're assuming that a null VMC is never returned
				src.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Bargein-disabled timer elapsed");
				dest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Bargein-disabled timer elapsed");

				// Send BargeinEnabled msg to AudioInThread
				mBIEnable = new ISMBargeinEnable();
				mBIEnable.m_Source = src;
				mBIEnable.m_Dest = dest;
				m_aqAudioIn[m_iThreadIndex].Push(mBIEnable);
			}
			catch(Exception exc)
			{
				bRet = false;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendAudioinBargeinEnable exception: " + exc.ToString());
			}

			return(bRet);
		} // SendAudioinBargeinEnable

		private bool SendAudioinBargeinDisable()
		{
			bool									bRet = true;
			ISMessaging.ISAppEndpoint				src = null, dest = null;
			ISMVMC									mVMC = null;
			ISMBargeinDisable						mBIDisable = null;

			try
			{
				src = new ISAppEndpoint();
				dest = new ISAppEndpoint();
				mVMC = new ISMVMC();
				mVMC.Init(m_iThreadIndex, "AudioMgr", m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId);		// Note - We're assuming that a null VMC is never returned
				src.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Bargein-disabled timer elapsed");
				dest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Bargein-disabled timer elapsed");

				// Send BargeinEnabled msg to AudioInThread
				mBIDisable = new ISMBargeinDisable();
				mBIDisable.m_Source = src;
				mBIDisable.m_Dest = dest;
				m_aqAudioIn[m_iThreadIndex].Push(mBIDisable);
			}
			catch(Exception exc)
			{
				bRet = false;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendAudioinBargeinDisable exception: " + exc.ToString());
			}

			return(bRet);
		} // SendAudioinBargeinDisable

        private void RequestMOHToPlay()
        {
            // Tell AudioRtr to play MOH.

            ISMessaging.Audio.ISMSpeechStop mSStop = new ISMessaging.Audio.ISMSpeechStop();
            mSStop.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		// Note - We're assuming that a null VMC is never returned

            SendMsgToAudioRouter(mSStop);
        }

		public void ThreadProc()
		{
			bool					bCont = true;
			bool					bRes = false;
			ISMessaging.ISMsg		mMsg = null;
			ISMVMC					vmcCurr = null;

			m_Logger.Init("", "", Thread.CurrentThread.Name, "", "", "");

			Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread started.");

			// Do Remoting related init.
			while(!bRes)
			{
				bRes = RemotingInit();
				if(!bRes)
				{
					Thread.Sleep(5000);
				}
			}

			while(bCont)
			{
				try
				{
					// Pull data from audio queue.  Msgs can be either data or commands.
					// We seem to be to be on rare occasions getting messages stuck in the queue, which may be due to the way the autoresetevent
					// is being used, or a limitation in its implementation.  The timeout may allow us to recover from this.
					mMsg = m_qMsg.Pop(m_iAudioTimeout);
					if(mMsg == null)
					{
						// Probably timed-out on Pop.  Check conditions.
					}
					else
					{
						// Check session-id against RM to make sure it belongs to the current session.  If the session-id is empty, it likely originated from DialogMgr, and should be allowed
						vmcCurr = m_RM.GetVMCByKey(m_iThreadIndex);
						if( ((vmcCurr.m_iKey == -1) || (mMsg.m_sSessionId != vmcCurr.m_sSessionId)) && (mMsg.m_sSessionId.Length > 0) )
						{
							if(mMsg.GetType().ToString() != "ISMessaging.Audio.ISMPlayPrompts")		// Ignore common situations
							{
								m_Logger.Log(Level.Warning, string.Format("[{0}]AudioOutThread.ThreadProc {1}'s session-id '{2}' doesn't match current '{3}'.", m_iThreadIndex.ToString(), mMsg.GetType().ToString(), mMsg.m_sSessionId, ((vmcCurr.m_iKey == -1) ? ISMVMC.m_csUnallocatedVmcTag : vmcCurr.m_sSessionId)));
							}
						}
						else
						{
							// Need to check for ISMSessionBegin and update sSessionId before first log statement, rather than in ISMSessionBegin handler as usual.
							if(mMsg.GetType().ToString() == "ISMessaging.Session.ISMSessionBegin")
							{
								// Set IpAddress, SessionId in Logger so all subsequent calls to Log() will be identified.  (Need-to/should do this in every thread once the ISMSessionBegin comes in.)
								m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.IpAddress.ToString(), Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(oAddr => oAddr.AddressFamily == AddressFamily.InterNetwork).ToString() ?? "");
								m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.SessionId.ToString(), mMsg.m_sSessionId);		// FIX - Should this be moved to after the socket is connected?
							}

                            Log(Level.Debug, String.Format("[{0}]AudioEngine_srv.AudioOutThread msg:  '{1}', curr.key:'{2}', curr.sid:'{3}', msg.sid:'{4}'.", m_iThreadIndex, mMsg.GetType().ToString(), vmcCurr.m_iKey, vmcCurr.m_sSessionId, mMsg.m_sSessionId));

							// Work on msg
							switch(mMsg.GetType().ToString())
							{
								case "ISMessaging.Audio.ISMPlayPrompts" :
								{
                                    if (!m_bInSession)	// Reject prompt if not in a call.  NOTE: Timing can occasionally have PlayPrompts from DialogMgr show up before SessionBegin.
									{
										Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
									}
									else
									{
                                        m_InactivityTimer.Stop();


                                        // Check to see if someone has barged in (or a session has ended/begin) before playing prompt
										if (HavePromptsBeenInvalidated())
										{
                                            m_PromptsPlayTimer.Stop();


                                            // Toss all the prompts at the top of the stack.  This will advance us to
											// the next non-prompt, which may not be the AudioStart/ISMSessionEnd/ISMSessionBegin
											// however.  Will this behavior ever cause an issue?  // FIX?

											m_qMsg.ClearTopPrompts();
										}
										else
										{
											bRes = ProcessPlayPrompts((ISMPlayPrompts)mMsg);
                                        }
									}
								}
								break;
								case "ISMessaging.Audio.ISMSpeechStart":
								{
									if (!m_bInSession)
									{
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
									else
									{
										// There are certain conditions that could cause a start to be received and we would not want to stop the
										// hangup timer.  For example if the AudioRtr were to crash mid-utterance (or potentially on some IP-PBXes
										// when the call goes on hold and they simply stop the RTP stream.)  In this case we want to still "hang
										// up the line" relatively quickly to recover the port.  To do this, we'll just restart the timer.

                                        m_InactivityHangupTimer.Start(m_InactivityHangupTimeout);
                                        m_PromptsPlayTimer.Stop();
                                        m_InactivityTimer.Stop();

										SendMsgToAudioRouter(mMsg);
									}
								}
								break;
								case "ISMessaging.Audio.ISMSpeechStop":
								{
									if (!m_bInSession)
									{
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
									else
									{
                                        m_bDialogManagerIsBusy = true;

                                        m_InactivityHangupTimer.Start(m_InactivityHangupTimeout);

                                        RequestMOHToPlay();
									}
								}
								break;
                                case "ISMessaging.Audio.ISMDtmfStart":
                                {
                                    if (!m_bInSession)
                                    {
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
                                    else
                                    {
                                        // There are certain conditions that could cause a start to be received and we would not want to stop the
                                        // hangup timer.  For example if the AudioRtr were to crash mid-utterance (or potentially on some IP-PBXes
                                        // when the call goes on hold and they simply stop the RTP stream.)  In this case we want to still "hang
                                        // up the line" relatively quickly to recover the port.  To do this, we'll just restart the timer.

                                        m_InactivityHangupTimer.Start(m_InactivityHangupTimeout);
                                        m_PromptsPlayTimer.Stop();
                                        m_InactivityTimer.Stop();


                                        // DTMF input will stop prompts being played even if Barge-In is disabled.

                                        m_BargeinDisabledTimer.Stop();
                                    }
                                }
                                break;
                                case "ISMessaging.Audio.ISMDtmfStop":
                                {
                                    if (!m_bInSession)
                                    {
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
                                    else
                                    {
                                        m_bDialogManagerIsBusy = true;

                                        m_InactivityHangupTimer.Start(m_InactivityHangupTimeout);
                                    }
                                }
                                break;
                                case"ISMessaging.Audio.ISMPlayPromptsComplete":
                                {
                                    if (!m_bInSession)
                                    {
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
                                    else
                                    {
                                        if (m_bDialogManagerIsBusy == true)
                                        {
                                            RequestMOHToPlay();
                                        }
                                    }
                                }
                                break;
                                case "ISMessaging.Session.ISMTransferSession":
								{
									if (!m_bInSession)
									{
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
									else
									{
                                        m_PromptsPlayTimer.Stop();
                                        m_InactivityTimer.Stop();
                                        m_InactivityHangupTimer.Stop();
                                        m_CallHangupTimer.Stop();

                                        m_sSessionEndReason = "Transfer";

										SendMsgToAudioRouter(mMsg);
									}
								}
								break;
								case "ISMessaging.Session.ISMSessionBegin" :
								{
									m_bInSession = true;
                                    m_bDialogManagerIsBusy = false;


                                    // Assume call ends because caller hangs up unless we terminate the call, in which case the appropriate reason will be set.

                                    m_sSessionEndReason = "Hangup";
int iStep = 0;

									m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "<<<AOT ISMessaging.Session.ISMSessionBegin>>>");


									// Set up the socket to the AudioRouter
									try
									{
										m_SockConn = m_SockConns.GetSockConn(m_iThreadIndex);
iStep++;
										if(m_SockConn == null)
										{
											// Socket was not found, so chances are that it blew up before we got here.  Log error and cleanup.
											// Note:  This should closely mirror ISMSessionEnd below!
											m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AOT ISMSessionBegin but no socket.");

											// This probably means that the AudioRtr has already received a BYE and has shut down the socket, therefore we don't need to
											// shut down the socket (in fact, we can't), and the ISMSessionEnd is on the way which will handle the cleanup.
											m_bInSession = false;
										}
										else
										{
											m_SockConn.m_sockARWrite = m_SockConn.m_listenARWrite.AcceptSocket();
											if(m_SockConn.m_sockARWrite == null)
											{
												m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AOT ISMSessionBegin socket accept returned null.");
											}
											else
											{
iStep++;
												// Start session timers
                                                m_InactivityHangupTimer.Start(m_InactivityHangupTimeout);
                                                m_CallHangupTimer.Start(m_CallHangupTimeout);
iStep++;
											}
										}

										// When done with Begin operations, push the message over to the AudioIn thread.
iStep++;
										m_aqAudioIn[m_iThreadIndex].Push((ISMessaging.ISMsg)((ISMessaging.Session.ISMSessionBegin)(mMsg)).Clone());	// Clone, just in case Mono is not properly ref-counting.  If it isn't, will this cause a leak?
iStep++;
									}
									catch(SocketException exc2)
									{
										Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc(ISMSessionBegin) exc2 step #" + iStep.ToString() + " socket error: " + exc2.ErrorCode.ToString());
									}
									catch(Exception exc2)
									{
										Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc(ISMSessionBegin) exc2 step #" + iStep.ToString() + ": " + exc2.ToString());
									}
								}
								break;
								case "ISMessaging.Session.ISMSessionEnd" :
								{
									// Note:  If the behavior here changes, make appropriate changes above in the (m_SockConn == null) section of ISMSessionBegin.
									bRes = ShutdownSession(true);
									m_bInSession = false;

									// When done with End operations, push the message over to the AudioIn thread.
                                    ISMessaging.Session.ISMSessionEnd seMsg = (ISMessaging.Session.ISMSessionEnd)(((ISMessaging.Session.ISMSessionEnd)mMsg).Clone());       // Clone, just in case Mono is not properly ref-counting.  If it isn't, will this cause a leak?
                                    seMsg.m_sReason = m_sSessionEndReason;

									m_aqAudioIn[m_iThreadIndex].Push(seMsg);

                                    m_sSessionEndReason = "Unknown";
								}
								break;
								case "ISMessaging.Session.ISMTerminateSession" :
								{
									if (!m_bInSession)
									{
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
									else
									{
										SendMsgToAudioRouter(mMsg);
										Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioOutThread.ThreadProc() Sending ISMTerminateSession.");

										m_bInSession = false;		// Should this be here?
										// Anything else to do?
									}
								}
								break;
								case "ISMessaging.Session.ISMTerminateSessionAfterPrompts" :
								{
									if (!m_bInSession)
									{
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
									else
									{
										SendMsgToAudioRouter(mMsg);
										Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioOutThread.ThreadProc() Sending ISMTerminateSessionAfterPrompts.");

										// Anything else to do?
									}
								}
								break;
                                case "ISMessaging.Session.ISMDialogManagerIdle":
                                {
                                    if (!m_bInSession)
                                    {
                                        Log(Level.Debug, String.Format("[{0}]AudioOutThread.ThreadProc() Received {1} when not in a call.", m_iThreadIndex, mMsg.GetType().ToString()));
                                    }
                                    else
                                    {
                                        m_bDialogManagerIsBusy = false;

                                        m_InactivityTimer.Arm();
                                    }
                                }
                                break;
								default :
								{
									Log(Level.Exception, String.Format("[{0}]AudioOutThread.ThreadProc() Got unknown msg type: {1}", m_iThreadIndex, mMsg.GetType()));
								}
								break;
							} // end switch(type)

						} // end else session-id

						// Release object
						mMsg = null;
					} // end else Pop
				} // end try
				catch(SocketException exc)
				{
					Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc caught socket error: " + exc.ErrorCode.ToString());
				}
				catch(Exception exc)
				{
					Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc: " + exc.ToString());
				}
			}	// end while

			// Cleanup
			try
			{
				if(m_SockConn != null)
				{
					if(m_SockConn.m_sockARWrite != null)
					{
						try
						{
							if(!m_SockConn.m_sockARWrite.Connected)
							{
								Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc 2 Socket was not connected.");
							}
							else
							{
								m_SockConn.m_sockARWrite.Shutdown(SocketShutdown.Both);
							}
						}
						catch(SocketException exc2)
						{
							Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc 2 caught socket error on Shutdown: " + exc2.ErrorCode.ToString());
						}
						try
						{
							m_SockConn.m_sockARWrite.Close();
						}
						catch(SocketException exc2)
						{
							Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc 2 caught socket error on Close: " + exc2.ErrorCode.ToString());
						}
						m_SockConn.m_sockARWrite = null;
					}
					if(m_SockConn.m_listenARWrite != null)
					{
						m_SockConn.m_listenARWrite.Stop();
					}
				}
			}
			catch(Exception exc)
			{
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ThreadProc: " + exc.ToString());
			}
		}	// ThreadProc

		private bool ShutdownSession(bool i_bNormalShutdown)
		{
			bool bRet = true;

			try
			{
				if ((!i_bNormalShutdown) && (m_bInSession))
				{
					Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ShutdownSession Shutting down (abnormal, in session).");
                    Log(Level.Warning, String.Format("[{0}]AudioOutThread.ShutdownSession Shutting down (abnormal, in session, {1}).", m_iThreadIndex, m_sSessionEndReason));
				}
				else
				{
					// In this case, ARMLT has already sent an ISMSessionEnd
					Log(Level.Info, String.Format("[{0}]AudioOutThread.ShutdownSession Shutting down ({1}, {2}, {3}).", m_iThreadIndex, (i_bNormalShutdown ? "normal" : "abnormal"), (m_bInSession ? "in session" : "no session"), m_sSessionEndReason));
                }

				// If this is a "normal" shutdown, we want to kill off the timers so they can't fire after the session disconnects.
				// If it isn't, we want to leave the hangup timers running so they at least attempt to disconnect the call later.
				// FIX - If this is an abnormal shutdown because the socket to AR is messed up, how can the hangup message going to get delivered after we shut down the socket?
				if (i_bNormalShutdown)
				{
					// FIX - One final timer event may be raised after the stop.  See Timer.Stop() description in MSDN.  They
					// recommend checking SignalTime to see if the event was raised after Stop was called.

                    m_PromptsPlayTimer.Stop();
                    m_BargeinDisabledTimer.Stop();
                    m_InactivityTimer.Stop();
                    m_InactivityHangupTimer.Stop();
                    m_CallHangupTimer.Stop();
				}

				bRet = CloseSockconn(eSockError.Unknown);
				m_SockConn = null;

				// If it's not a normal shutdown, release the session.  (If it is a normal shutdown, AIT should do the release.)
				if (!i_bNormalShutdown)
				{
					m_RM.ReleaseSession(m_iThreadIndex);
				}
			}
			catch (Exception exc)
			{
				bRet = false;
                Log(Level.Exception, String.Format("[{0}]AudioOutThread.ShutdownSession: {1}", m_iThreadIndex, exc.ToString()));
			}

			return bRet;
		} // ShutdownSession

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool CloseSockconn(eSockError i_eLastError)
		{
			bool			bRet = true;

			try
			{
				if(m_SockConn == null)
				{
					bRet = false;
					Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.CloseSockconn SockConn was already nulled.");
				}
				else
				{
					if(m_SockConn.m_sockARWrite == null)
					{
						bRet = false;
						Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.CloseSockconn Socket was already nulled.");
					}
					else
					{
						// Close the socket.
						if (!m_SockConn.m_sockARWrite.Connected)
						{
							bRet = false;
							Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.CloseSockconn Socket was not connected.");
						}

						if(i_eLastError == eSockError.SHUTDOWN)
						{
							try
							{
								m_SockConn.m_sockARWrite.Close();
							}
							catch (SocketException exc2)
							{
								bRet = false;
								Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.CloseSockconn caught socket error on Close: " + exc2.ErrorCode.ToString());
							}

							m_SockConn.m_sockARWrite = null;
						}
						else
						{
							try
							{
								m_SockConn.m_sockARWrite.Shutdown(SocketShutdown.Both);
							}
							catch (SocketException exc2)
							{
								bRet = false;
								Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.CloseSockconn caught socket error on Shutdown: " + exc2.ErrorCode.ToString());
							}
							try
							{
								m_SockConn.m_sockARWrite.Close();
							}
							catch (SocketException exc2)
							{
								bRet = false;
								Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.CloseSockconn caught socket error on Close: " + exc2.ErrorCode.ToString());
							}

							m_SockConn.m_sockARWrite = null;
						}

						m_SockConn.m_sockARWrite = null;
					} // else m_sockARWrite
				} // else m_SockConn
			}
			catch(Exception exc)
			{
				bRet = false;
				Log(Level.Exception, string.Format("[{0}]AudioOutThread.CloseSockconn: '{1}'.", m_iThreadIndex.ToString(), exc.ToString()));
			}

			return(bRet);
		} // CloseSockconn

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool ReacceptSockconn()
		{
			bool					bRet = true;

			try
			{
				if(m_SockConn == null)
				{
					bRet = false;
					Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ReacceptSockconn SockConn was already nulled.");
				}
				else
				{
					m_SockConn.m_sockARWrite = m_SockConn.m_listenARWrite.AcceptSocket();
					if(m_SockConn.m_sockARWrite == null)
					{
						bRet = false;
						m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AOT ReacceptSockconn socket accept returned null.");
					}
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ReacceptSockconn: " + exc.ToString());
			}

			return(bRet);
		} // ReacceptSockconn

		// Have we received an 'alert' message that invalides the current prompts?
		private bool HavePromptsBeenInvalidated()
		{
			return (m_qMsg.Find(m_asAlertMsgs) != null);
		}

		// If we've received an 'alert' message, or the socket is closed, stop issuing prompts.
		private bool IsSendingOfPromptsToBeStopped()
		{
			return (HavePromptsBeenInvalidated() || (m_SockConn == null) || (m_SockConn.m_sockARWrite == null));
		}

		/// <summary>
		/// Hand the prompts off to the AudioRouter.
		/// </summary>
		/// <param name="i_mPlayPrompts"></param>
		/// <returns></returns>
		private bool ProcessPlayPrompts(ISMPlayPrompts i_mPlayPrompts)
		{
			bool bRet = true;

			try
			{
				if (i_mPlayPrompts.m_Prompts == null)
				{
					Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ProcessPlayPrompts got a null prompt body.");

					// Restart timer using default setting.
                    m_InactivityTimer.Start(m_DefaultInactivityTimeout);
				}
				else
				{
					bool bRes = true;
					bool bQuit = false;
					int iTotalBytes = 0;
					bool bBargeinEnabled = i_mPlayPrompts.m_bBargeinEnabled;
					int iNumPrompts = i_mPlayPrompts.m_Prompts.Length;

					Log(Level.Info, String.Format("[{0}]AudioOutThread.ProcessPlayPrompts() -- Barge-In Enabled: {1}", m_iThreadIndex, bBargeinEnabled));

					for (int ii = 0; ((ii < iNumPrompts) && (!bQuit)); ii++)
					{
						if (IsSendingOfPromptsToBeStopped())
						{
							bQuit = true;
						}
						else
						{
							Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "<<<AudioEngine_srv.AudioOutThread -- ISMPlayPrompts(" + ii.ToString() + "): " + i_mPlayPrompts.m_Prompts[ii].m_sPath);

							switch(i_mPlayPrompts.m_Prompts[ii].m_Type)
							{
								case ISMPlayPrompts.PromptType.eWavFilePath:
									{
										bool bNeedToUseTts = false;

										if (i_mPlayPrompts.m_Prompts[ii].m_sPath.Length > 0)
										{
											byte[] abAudio = null;

											bRes = GetWavFile(m_Logger, i_mPlayPrompts.m_Prompts[ii].m_sPath, out abAudio);
											if (bRes)
											{
												SendPromptToAudioRouter(abAudio, ref bQuit, ref iTotalBytes);
											}
											else
											{
												bNeedToUseTts = true;
												Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ProcessPlayPrompts() - Couldn't get webfile '" + i_mPlayPrompts.m_Prompts[ii].m_sPath + "'.");
											}
										}
										else
										{
											bNeedToUseTts = true;
											Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ProcessPlayPrompts() - There was no URL.");
										}

										if (bNeedToUseTts)
										{
											// TTS the text if we couldn't get the file.

											TtsPrompt(i_mPlayPrompts.m_Prompts[ii], ref bQuit, ref iTotalBytes);
										}
									}
									break;

								case ISMPlayPrompts.PromptType.eTTS_Text:
									// FIX - Don't create TTS every time, get one from RM.
									TtsPrompt(i_mPlayPrompts.m_Prompts[ii], ref bQuit, ref iTotalBytes);
									break;

								default:
									Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ProcessPlayPrompts() - Don't know how to process type '" + i_mPlayPrompts.m_Prompts[ii].m_Type.ToString() + "'.");
									break;
							} // switch
						} // else
					} // for


                    TimeSpan timeRequiredToPlayPrompts = new TimeSpan(0, 0, 0, 0, iTotalBytes / 8);

                    m_PromptsPlayTimer.Add(timeRequiredToPlayPrompts);

					if (bBargeinEnabled == false)
					{
						bRes = SendAudioinBargeinDisable();
                        m_BargeinDisabledTimer.Add(timeRequiredToPlayPrompts);
					}
				} // else
			}
			catch (Exception exc)
			{
				bRet = false;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.ProcessPlayPrompts: " + exc.ToString());
			}

			return bRet;
		}

		private void SendPromptToAudioRouter(byte[] i_abAudio, ref bool o_bQuit, ref int io_iTotalBytes)
		{
			// NOTE: Only the bare essentials of the msg are getting set here since we know what fields are going across the socket.  This "technique" should not be done elsewhere!

			ISMRawData mRaw = new ISMRawData();
			mRaw.m_abData = i_abAudio;

			if (IsSendingOfPromptsToBeStopped())
			{
				o_bQuit = true;
			}
			else
			{
				eSockResult eRes = eSockResult.OK;

				eRes = SendMsgToAudioRouter(mRaw);
				if (eRes == eSockResult.Fatal)
				{
					o_bQuit = true;
				}
				else
				{
					io_iTotalBytes += mRaw.m_abData.Length;
				}
			}

			return;
		}

		private void TtsPrompt(ISMPlayPrompts.Prompt i_Prompt, ref bool o_bQuit, ref int io_iTotalBytes)
		{
			if (i_Prompt.m_sText.Length <= 0)
			{
				Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.TtsPrompt() - There was no text to TTS.");
			}
			else
			{
				byte[] abAudio = null;

				bool bRes = TtsString(m_Logger, m_iThreadIndex, i_Prompt.m_sText, i_Prompt.m_sLang, i_Prompt.m_Gender, i_Prompt.m_sVoiceName, i_Prompt.m_sTTSVendorProduct, out abAudio);
				if (bRes)
				{
					Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.TtsPrompt() - TTSed '" + i_Prompt.m_sText + "'.");

					SendPromptToAudioRouter(abAudio, ref o_bQuit, ref io_iTotalBytes);
				}
				else
				{
					Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.TtsPrompt() - Couldn't TTS '" + i_Prompt.m_sText + "'.");
				}
			}
		}

		private bool TtsString(ILegacyLogger i_Logger, int i_iThreadIndex, string i_sText, string i_sLang, Gender i_Gender, string i_sVoiceName, string i_sTTSVendorProduct, out byte[] o_abAudio)
		{
			bool		bRet = true, bRes = true;
			string		sTmp = "";
			ITTS		iTts = null;
			FileInfo	fiAudio = null;
			FileStream	fsAudio = null;
			int			iRead = 0, iLength = 0;
			StringBuilder	sbTmp = null;
			DynamicPrompt	dynamicPrompt = null;

			o_abAudio = null;

			try
			{
				sbTmp = new StringBuilder();
				sbTmp.AppendFormat("[{0}] AudioOutThread.TtsString('{1}', '{2}', '{3}', '{4}', '{5}')", i_iThreadIndex.ToString(), i_sText, i_sLang, i_Gender.ToString(), i_sVoiceName, i_sTTSVendorProduct);
				i_Logger.Log(Level.Info, sbTmp.ToString());

				if(i_sText.Length <= 0)
				{
					bRet = false;
					i_Logger.Log(Level.Debug, "AudioOutThread.TtsString() - i_sText was empty.");
				}
				else
				{
					dynamicPrompt = new DynamicPrompt(i_sText, i_sLang, i_Gender, i_sVoiceName);
					string sAudioFilePath = dynamicPrompt.AudioFileName;


					// Since we don't want to have to check the file system again for the existence of this file if it already
					// exist we assume that the TTS was successful (which clearly is the case if the file already exists, else how 
					// would it exist).  This will only change if we actually have to TTS the file and encounter a problem doing so.

					bool bWasTtsSuccessful = true;

					if (dynamicPrompt.IsTtsNeeded())
					{
						iTts = new ExeTTS(m_TtsLanguageCodeMapping);
						if(!iTts.Init())
						{
							bWasTtsSuccessful = false;
							i_Logger.Log(Level.Exception, "AudioOutThread.TtsString() - Init failed.");
						}
						else
						{
							dynamicPrompt.EnsureTargetDirectoryExists();


							// Replace unreadable chars.
							sTmp = i_sText.Replace('_', ' ');
							sTmp = sTmp.Replace('-', ' ');
							sTmp = sTmp.Replace('=', ' ');
							sTmp = sTmp.Replace('*', ' ');
							sTmp = sTmp.Replace('~', ' ');
							sTmp = sTmp.Replace(Environment.NewLine, ".  ");	// FIX - Will cause pauses in emails with newlines at 80 char wrap.

							// TTS the string
							bRes = iTts.TextToWav(sTmp, sAudioFilePath, i_sLang, i_Gender.ToString(), i_sVoiceName);
							if(!bRes)
							{
								bWasTtsSuccessful = false;
								i_Logger.Log(Level.Exception, "AudioOutThread.TtsString() - TextToWav failed.");
							}
							else
							{
								if (!File.Exists(sAudioFilePath))
								{
									bWasTtsSuccessful = false;
									i_Logger.Log(Level.Exception, String.Format("AudioOutThread.TtsString() - TextToWav didn't create file '{0}'.", sAudioFilePath));
								}
								else
								{
									i_Logger.Log(Level.Info, String.Format("Generated Dynamic Prompt: {0}", sAudioFilePath));


									// Wait until we know that the TTS has succeeded befor writing out the text file 
									// since there is no point in having it without the corresponding audio file.
									// NOTE: By leaving it to this point we also ensure that the contents of this file 
									//       contains exactly what the TTS received (i.e. unreadable character substitution
									//       has already been done).  Otherwise it might be confusing to have the text show
									//       one thing but the audio clearly speaking something else.

									dynamicPrompt.SaveText(sTmp);
								}
							}
						}
					}
					else
					{
						i_Logger.Log(Level.Info, String.Format("Using Dynamic Prompt: {0}", sAudioFilePath));
					}

					if (bWasTtsSuccessful)
					{
						// Read file into buffer.
						fiAudio = new FileInfo(sAudioFilePath);
						fsAudio = fiAudio.OpenRead();

						if (fiAudio.Length < 1)
						{
							i_Logger.Log(Level.Exception, String.Format("AudioOutThread.TtsString() - TextToWav didn't create audio in file '{0}' from string '{1}'.", sAudioFilePath, i_sText));
						}
						else
						{
							iLength = (int)fiAudio.Length - WavHeader.WAVHEADERSIZE;
							o_abAudio = new byte[iLength];

							// Skip over WAV header
							iRead = fsAudio.Read(o_abAudio, 0, WavHeader.WAVHEADERSIZE);
							if (iRead != WavHeader.WAVHEADERSIZE)
							{
							}
							else
							{

								// Read audio
								iRead = fsAudio.Read(o_abAudio, 0, iLength);
								while (iRead < iLength)
								{
									iRead += fsAudio.Read(o_abAudio, iRead, (iLength - iRead));
								}
							}
						}
					}
					else
					{
						bRet = false;
					}
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				i_Logger.Log(Level.Exception, "AudioOutThread.TtsString: " + exc.ToString());
			}
			finally
			{
				if(fsAudio != null)
				{
					fsAudio.Close();
				}
				if(dynamicPrompt != null)
				{
					dynamicPrompt.Cleanup();
				}
				if(iTts != null)
				{
					iTts.Release();
					iTts = null;
				}
			}

			return(bRet);
		}

		/// <summary>
		/// Get the file from the URI as an array of bytes.
		/// </summary>
		/// <param name="i_sUri"></param>
		/// <param name="o_abFile"></param>
		/// <returns></returns>
		private bool GetWavFile(ILegacyLogger i_Logger, string i_sUri, out byte[] o_abFile)
		{
			bool bGotAudioBytes = false;

			o_abFile = null;

			try
			{
				if (!String.IsNullOrEmpty(i_sUri))
				{
					byte[] abFile = null;
					int iTotalRead = 0;

					Uri uriPage = new Uri(i_sUri);

					if (uriPage.IsFile)
					{
                        string sFileName = Uri.UnescapeDataString(uriPage.AbsolutePath);            // Need to do this so that filenames can contain spaces (Uri.AbsolutePath returns a space as %20).

                        if (File.Exists(sFileName))
						{
                            FileInfo fiAudio = new FileInfo(sFileName);

							using (FileStream fsAudio = fiAudio.OpenRead())
							{
								iTotalRead = ReadBytesFromStream(fsAudio, fiAudio.Length, out abFile);

								fsAudio.Close();
							}
						}
						else
						{
                            i_Logger.Log(Level.Warning, String.Format("AudioOutThread.GetWavFile:  Error retrieving file: '{0}'.", sFileName));
						}
					}
					else
					{
						WebRequest wrReq = WebRequest.Create(uriPage);

						using (WebResponse wrResp = wrReq.GetResponse())
						using (Stream sResp = wrResp.GetResponseStream())
						{
							iTotalRead = ReadBytesFromStream(sResp, wrResp.ContentLength, out abFile);

							sResp.Close();
						}
					}

					if(iTotalRead < WavHeader.WAVHEADERSIZE)
					{
						i_Logger.Log(Level.Exception, String.Format("AudioOutThread.GetWavFile() - Couldn't read WAV header ({0}).", i_sUri));
					}
					else
					{
						WavHeader whFile = new WavHeader();
						bool bRes;

						// Extract the info from the WAV header
						bRes = whFile.Extract(abFile);
						if(!bRes)
						{
							// Didn't have a WAV header, just reassign the reference.
							o_abFile = abFile;
							bGotAudioBytes = true;
						}
						else
						{
							bRes = whFile.Valid();
							if(!bRes || (whFile.m_dwDataChunkSize <= 0) )
							{
								i_Logger.Log(Level.Exception, String.Format("AudioOutThread.GetWavFile() - The WAV (header) wasn't valid ({0}).", i_sUri));
							}
							else
							{
								//Copy the data minus the header.
								o_abFile = new byte[abFile.Length - WavHeader.WAVHEADERSIZE + 1];
								Array.Copy(abFile, WavHeader.WAVHEADERSIZE, o_abFile, 0, (abFile.Length - WavHeader.WAVHEADERSIZE));
								bGotAudioBytes = true;
							}
						}
					}
				}
			}
			catch (WebException exc)
			{
				bGotAudioBytes = false;
				if(exc.Status == WebExceptionStatus.ProtocolError)
				{
					i_Logger.Log(Level.Warning, String.Format("AudioOutThread.GetWavFile:  Error retrieving file: '{0}'.", i_sUri));
				}
				else
				{
					i_Logger.Log(exc);
				}
			}
			catch (Exception exc)
			{
				bGotAudioBytes = false;
				i_Logger.Log(Level.Exception, String.Format("AudioOutThread.GetWavFile: {0}", exc.ToString()));
			}

			return bGotAudioBytes;
		} // GetWavFile()


		// NOTE: Need to pass the number of bytes to read from stream since not all streams allow their length to be determined up-front (i.e. a stream obtained from WebResponse.GetResponseStream()).

		private int ReadBytesFromStream(Stream i_stream, long i_NumberOfBytesToRead, out byte[] o_bytes)
		{
			int iTotalBytesRead = 0;

			o_bytes = new byte[i_NumberOfBytesToRead];

			long lBytesLeft = i_NumberOfBytesToRead;

			while (lBytesLeft > 0)
			{
				int iJustRead = i_stream.Read(o_bytes, iTotalBytesRead, (int)lBytesLeft);

				if (iJustRead == 0)
				{
					break;
				}

				iTotalBytesRead += iJustRead;
				lBytesLeft -= iJustRead;
			}

			return iTotalBytesRead;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_abBuff"></param>
		/// <param name="i_iIndex"></param>
		/// <param name="i_iSize"></param>
		/// <returns></returns>
		private eSockError SendBlockToAudioRouter(byte[] i_abBuff, int i_iIndex, int i_iSize)
		{
			eSockError		eRet = eSockError.OK;
			int				iSent = 0;

			try
			{
				if (!m_SockConn.m_sockARWrite.Connected)
				{
					Log(Level.Exception, string.Format("[{0}]" + "AudioOutThread.SendBlockToAudioRouter: Socket was not connected, {1} iters.", m_iThreadIndex.ToString()));
				}
				else if (!(m_SockConn.m_sockARWrite.Poll(1, SelectMode.SelectWrite)))
				{
					eRet = eSockError.Retry;
				}
				else
				{
					iSent = m_SockConn.m_sockARWrite.Send(i_abBuff, i_iIndex, i_iSize, SocketFlags.None);
					if(iSent != i_iSize)
					{
						eRet = eSockError.Unknown;
						Log(Level.Warning, string.Format("[{0}]AudioOutThread.SendBlockToAudioRouter: Sent {1} not {2}.", m_iThreadIndex.ToString(), iSent.ToString(), i_iSize.ToString()));
					}
				}
			}
			catch (SocketException exc)
			{
				if (exc.ErrorCode == (int)eSockError.SHUTDOWN)			// The socket has been shut down
				{
					eRet = eSockError.SHUTDOWN;
					Log(Level.Exception, string.Format("[{0}]AudioOutThread.SendBlockToAudioRouter: Socket has been shut down, error code: {1}.", m_iThreadIndex.ToString(), exc.ErrorCode.ToString()));
				}
				else if (exc.ErrorCode == (int)eSockError.CONNRESET)		// Connection reset by peer
				{
					eRet = eSockError.CONNRESET;
					Log(Level.Exception, string.Format("[{0}]AudioOutThread.SendBlockToAudioRouter: Connection reset by peer, error code: {1}.", m_iThreadIndex.ToString(), exc.ErrorCode.ToString()));
				}
				else
				{
					eRet = eSockError.Unknown;
					Log(Level.Exception, string.Format("[{0}]AudioOutThread.SendBlockToAudioRouter: Error code: {1}, exception: '{2}'.", m_iThreadIndex.ToString(), exc.ErrorCode.ToString(), exc.ToString()));
				}
			} // catch

			return(eRet);
		} // SendBlockToAudioRouter

		/// <summary>
		/// 
		/// </summary>
		/// <param name="o_abBuff"></param>
		/// <returns></returns>
		private eSockResult SendBufferToAudioRouter(byte[] i_abBuff)
		{
			eSockResult		eRet = eSockResult.OK;
			eSockError		eRes = eSockError.OK;
			int				iBytesLeft = 0, iPacketSize = 0, iIndex = 0;
			bool			bRes = true;
int	iStep = 0;

			try
			{
iStep = 1;
				if(m_SockConn == null)
				{
iStep = 2;
					Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendBufferToAudioRouter attempted to write to nulled m_SockConn.");
				}
				else if(m_SockConn.m_sockARWrite == null)
				{
iStep = 3;
					Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendBufferToAudioRouter attempted to write to nulled m_sockARWrite.");
				}
				else if(!m_SockConn.m_sockARWrite.Connected)
				{
iStep = 4;
					Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendBufferToAudioRouter attempted to write to unconnected m_sockARWrite.");
				}
				else
				{
iStep = 5;
					iBytesLeft = i_abBuff.Length;
					iIndex = 0;
iStep = 6;
					while(iBytesLeft > 0)
					{
iStep = 7;
						if(iBytesLeft < AMSockData.SIZEOPPACKET)
						{
iStep = 8;
							iPacketSize = iBytesLeft;
						}
						else
						{
iStep = 9;
							iPacketSize = AMSockData.SIZEOPPACKET;
						}
iStep = 10;

						if( (m_SockConn == null) || (m_SockConn.m_sockARWrite == null) )
						{
iStep = 11;
							iBytesLeft = 0;
						}
						else
						{
iStep = 12;
							eRes = SendBlockToAudioRouter(i_abBuff, iIndex, iPacketSize);
iStep = 13;
							switch(eRes)
							{
								case eSockError.OK:				// Successful
									iIndex += iPacketSize;
									iBytesLeft -= iPacketSize;
									break;

								case eSockError.Retry:			// Couldn't send, retry
									break;

								case eSockError.SHUTDOWN:		// Fail (shutdown), but re-init and retry once
iStep = 14;
									bRes = CloseSockconn(eRes);
iStep = 15;
									bRes = ReacceptSockconn();
									if(!bRes)
									{
                                        Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendBufferToAudioRouter Resend reaccept failed.");
iStep = 16;
										ShutdownSession(false);
									}
									else
									{
										eRes = SendBlockToAudioRouter(i_abBuff, iIndex, iPacketSize);
										if(eRes == eSockError.OK)
										{
											iIndex += iPacketSize;
											iBytesLeft -= iPacketSize;
										}
										else
										{
											Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendBufferToAudioRouter Resend failed.");
										}
									}
									break;

								case eSockError.CONNRESET:		// Fail (reset by peer)
									ShutdownSession(false);
									break;
								case eSockError.Unknown:		// Fail
									ShutdownSession(false);
									break;
								default:		// Fail
									ShutdownSession(false);
									break;
							} // switch
						}
iStep = 17;
					} // while
iStep = 18;
				}
iStep = 19;
			}
			catch(Exception exc)
			{
				eRet = eSockResult.Warning;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendBufferToAudioRouter step #" + iStep.ToString() + ": " + exc.ToString());
			}

			return(eRet);
		} // SendBufferToAudioRouter()

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Msg"></param>
		/// <returns></returns>
		public eSockResult SendMsgToAudioRouter(ISMessaging.ISMsg i_Msg)
		{
			eSockResult		eRet = eSockResult.OK, eRes = eSockResult.OK;
			AMSockData		amsData = null;
			byte[]			abMsgHdr = null, abDataSize = null, abMsgBody = null;
			int				ii, iBlocks, iStrLen, jj;

			try
			{
				amsData = new AMSockData(ref m_Logger);
				amsData.Set(i_Msg);
				amsData.GetMsgHdr(out abMsgHdr);

				if(amsData.m_Type == AMSockData.AMDMsgType.eAudioData)
				{
					// FIX - Double check this code to ensure data size is correct.
					abDataSize = BitConverter.GetBytes(amsData.m_lDataSize);
					Array.Copy(abDataSize, 0, abMsgHdr, AMSockData.m_iIndexes[3], 4);

					abMsgBody = ((ISMRawData)i_Msg).m_abData;
					iBlocks = abMsgBody.Length / AMSockData.SIZEAUDIODATA;
					for(ii = 0; ( (ii < iBlocks) && (eRes != eSockResult.Fatal) ); ii++)
					{
						Array.Copy(abMsgBody, (ii * AMSockData.SIZEAUDIODATA), abMsgHdr, AMSockData.m_iIndexes[4], AMSockData.SIZEAUDIODATA);

						// FIX - Set sequence number in here as well.

						eRes = SendBufferToAudioRouter(abMsgHdr);
					}
				}
				else if(amsData.m_Type == AMSockData.AMDMsgType.eTransferSession)
				{
					ISMessaging.Session.ISMTransferSession	mTrans;

					mTrans = (ISMessaging.Session.ISMTransferSession)i_Msg;
					iStrLen = mTrans.m_sTransferToAddr.Length;
					for(ii = 0, jj = AMSockData.m_iIndexes[3]; ( (ii < iStrLen) && (jj < AMSockData.SIZETRANSFERADDR) ); ii++, jj++)
					{
						abMsgHdr[jj] = (byte)mTrans.m_sTransferToAddr[ii];
					}
					if(jj < AMSockData.SIZETRANSFERADDR)
					{
						abMsgHdr[jj] = 0;
					}
					else
					{
						abMsgHdr[AMSockData.SIZETRANSFERADDR - 1] = 0;
					}

					eRes = SendBufferToAudioRouter(abMsgHdr);
				}
				else
				{
					eRes = SendBufferToAudioRouter(abMsgHdr);
				}

				eRet = eRes;
			}
			catch(Exception exc)
			{
				eRet = eSockResult.Fatal;
				Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioOutThread.SendMsgToAudioRouter: " + exc.ToString());
			}

			return(eRet);
		} // SendMsgToAudioRouter()


		private sealed class DynamicPrompt
		{
			private const string m_csAudioFileExtension = ".wav";
			private const string m_csTextFileExtension = ".txt";

			private bool m_bSaveAudio;
			private string m_sBase64EncodedHash;
			private string m_sPathName;
			private string m_sAudioFileName;

			public DynamicPrompt(string i_sText, string i_sLanguage, Gender i_Gender, string i_sVoiceName)
			{
				if (IsAudioToBeSaved())
				{
					m_bSaveAudio = true;
					HashAlgorithm hashAlgorithm = SHA1.Create();
					byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(i_sText));
					m_sBase64EncodedHash = Base64UrlEncode(hash);


					// Base path information is in Configuration file, the rest comes from the voice information provided.
					// Make sure gender is always in lower case since UNIX is case sensitive and we don't to depend on 
					// how enum is actually defined.

					m_sPathName = GeneratePathName(i_sLanguage, i_Gender.ToString().ToLower(), i_sVoiceName);


					// Generate audio file name since we know it will definitly be used (to check if file already exists)
					// so there is no saving to be had by postponing this.

					m_sAudioFileName = GenerateFileName(m_csAudioFileExtension);
				}
				else
				{
					m_bSaveAudio = false;
					m_sAudioFileName = Path.GetTempFileName();
				}
			}

			public string AudioFileName
			{
				get { return m_sAudioFileName; }
			}

			public string TextFileName
			{
				// Since we only need this if a file needs to be create put this off until the last moment (since for most
				// instances of DynamicPrompt this will never be needed.

				get { return GenerateFileName(m_csTextFileExtension); }
			}

			public bool IsTtsNeeded()
			{
				// We need to TTS if the audio file is not to be saved (we need to check for
				// this condition since Path.GetTempFileName() creates a file) or if the audio 
				// file doesn't already exist.

				return !m_bSaveAudio || !File.Exists(AudioFileName);
			}

			public void SaveText(string i_sText)
			{
				// Don't bother saving the text if the audio file is only temporary.

				if (m_bSaveAudio)
				{
					using (StreamWriter sw = File.CreateText(TextFileName))
					{
						sw.Write(i_sText);
					}
				}
			}

			public void EnsureTargetDirectoryExists()
			{
				if (!System.IO.Directory.Exists(Path.GetDirectoryName(AudioFileName)))
				{
					System.IO.Directory.CreateDirectory(Path.GetDirectoryName(AudioFileName));
				}

			}

			public void Cleanup()
			{
				if (!m_bSaveAudio)
				{
					File.Delete(AudioFileName);
				}
			}

			private string GenerateFileName(string i_sExtension)
			{
				return Path.Combine(m_sPathName, String.Format("{0}{1}", m_sBase64EncodedHash, i_sExtension));
			}

			private string GeneratePathName(string i_sLanguage, string i_sGender, string i_sVoiceName)
			{
				StringBuilder sbSubPath = new StringBuilder();

				if (i_sLanguage.Length > 0)
				{
					sbSubPath.AppendFormat("{0}{1}", i_sLanguage, Path.DirectorySeparatorChar);
				}

				if (i_sGender.Length > 0)
				{
					sbSubPath.AppendFormat("{0}{1}", i_sGender, Path.DirectorySeparatorChar);
				}

				if (i_sVoiceName.Length > 0)
				{
					sbSubPath.AppendFormat("{0}{1}", i_sVoiceName, Path.DirectorySeparatorChar);
				}

				return Path.Combine(ConfigurationManager.AppSettings[AudioEngine.cs_DynamicPromptsPath], sbSubPath.ToString());
			}

			private bool IsAudioToBeSaved()
			{
				bool bIsAudioToBeSaved = false;

				string sDynamicPromptsEnabled = ConfigurationManager.AppSettings[AudioEngine.cs_DynamicPromptsEnabled];

				if ((null != sDynamicPromptsEnabled) && ("true" == sDynamicPromptsEnabled.ToLower()))
				{
					bIsAudioToBeSaved = true;
				}

				return bIsAudioToBeSaved;
			}

			// Using Convert.ToBase64String() results in a string that can contain characters that are not valid in
			// file names so we need to replace those with allowable characters.  A quick look around shows that what
			// we need is handled by HttpServerUtility.UrlTokenEncode but that only exists in .NET 2.0 or later.
			//  
			// The following is an implementation of a URL and file name safe base64 encoding as per RFC 4648
			// (see http://tools.ietf.org/html/rfc4648#page-7).
			//
			// NOTE: We care about URLs since the file might be accessed via HTTP (either from a remote machine
			//       or by using the http:// specifier in a VXML file).

			private string Base64UrlEncode(byte[] i_bytes)
			{
				if ((null == i_bytes) || (0 == i_bytes.Length))
					return String.Empty;

				string sBase64 = Convert.ToBase64String(i_bytes);


				// Remove the "=" padding since we don't need it.
				// NOTE: This is only true since we know that we will never have to decode the Base64 representation.
				//       However, since we know the length of the original string encoded we can compute how many
				//       padding characters would be required and add them back in if decoding is required.

				sBase64 = sBase64.TrimEnd(new char[] { '=' });


				// Replace "+" and "/" which are not safe in URLs or file names.

				sBase64 = sBase64.Replace("+", "-").Replace("/", "_");

				return sBase64;
			}
		} // DynamicPrompt
	} // AudioOutThread
}
