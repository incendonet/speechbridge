// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using ISMessaging.SpeechRec;
using ISMessaging.Audio;
using SBConfigStor;
using SBResourceMgr;


namespace AudioMgr
{
	public class AudioInThread
	{
		public enum AudioState
		{
			WaitingForCommand,	// Toss out all audio data.
			Listening,			// Allow stream data through.
			Paused,				// Queue up data, but don't process it.  // This may require a separate message queue to handle properly.
			ReceivingDTMF,		// Currently receiving DTMF.  (Usually this means ignore utterances.)
		}

		private ArrayList								m_aASRs = null;
		private int										m_iThreadIndex = -1;
		private	ISMessaging.MsgQueue					m_qMsg = null;
		private IResourceMgr							m_RM = null;
		private string									m_sSessionId = "";
		private bool									m_bInSession = false;
		private bool									m_bCurrentlyStreaming = false;
		private bool									m_bSaveAudioStream = false;
		private bool									m_bSaveUttWavs = false;
		private EnergyDetector							m_EDet = null;
        private int                                     m_iEndpointInMilliseconds = 1000;       // 1 second endpoint by default.
		private GrammarBuilder.eGramFormat				m_GramFormat = GrammarBuilder.eGramFormat.SRGS_ABNF;
		private bool									m_bDumpGram = false;
		private GrammarBuilder							m_gbGram = null;
		private IASR									m_asr = null;
		private AudioOutMsgQueue[]						m_aqAudioOut = null;
		private int m_iAudioTimeout = -1;
		private ISMessaging.Delivery.ISMReceiverImpl	m_mRcv = null;
		private int										m_UttIndex = 1;
		private FileStream								m_fsUtt = null;	// The 'null' assignment to 'fsUtt' is to shut up the compier from erroneously flagging it as unassigned in the "Utterance DATA" block below.
		private int										m_iRawIndex = 1;
		private FileStream								m_fsRaw = null;	// The 'null' assignment to 'fsUtt' is to shut up the compier from erroneously flagging it as unassigned in the "Utterance DATA" block below.
		private StringBuilder							m_sbDtmfReceived = null;
		private System.Timers.Timer						m_tDtmf = null;
        private string                                  m_sDtmfTerminationCharacter = "";       // The VoiceXML standard specifies that the default should be '#' but by setting it to nothing (i.e. empty string) we maintain backwards compatibility with exiting SpeechBridge behavior.
		private const long								m_iMaxUttSize = 121440;	// 15 sec = 15 * 8096 = 121440  This cutoff is used because of a critical error associated to LumenVox on mono.
		private long									m_iCurrUttSize = 0;
		private string									sTmp = "";

		protected ILegacyLogger							m_Logger = null;
		protected string								m_sLogPath = "";

		public AudioState								m_AudioState = AudioState.WaitingForCommand;

		public AudioInThread(ILegacyLogger i_Logger, IResourceMgr i_RM, int i_iThreadIndex, ISMessaging.MsgQueue i_qMsg, int i_iAudioTimeout, string i_sLogPath, ArrayList i_aASRs, AudioOutMsgQueue[] i_aqAudioOut)
		{
			m_Logger = i_Logger;
			m_qMsg = i_qMsg;
			m_RM = i_RM;
			m_iThreadIndex = i_iThreadIndex;
			m_iAudioTimeout = i_iAudioTimeout;
			m_sLogPath = i_sLogPath;
			m_aASRs = i_aASRs;
			m_aqAudioOut = i_aqAudioOut;

			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_GrammarFormat];
			sTmp = (sTmp == null) ? "" : sTmp;
			if(sTmp.Length != 0)	// Defaults to "ABNF"
			{
				if(sTmp.ToUpper() == GrammarBuilder.eGramFormat.SRGS_GRXML.ToString())
				{
					m_GramFormat = GrammarBuilder.eGramFormat.SRGS_GRXML;
				}
				else if(sTmp.ToUpper() == GrammarBuilder.eGramFormat.SRGS_ABNF.ToString())
				{
					m_GramFormat = GrammarBuilder.eGramFormat.SRGS_ABNF;
				}
				else if(sTmp.ToUpper() == GrammarBuilder.eGramFormat.Phrases.ToString())
				{
					m_GramFormat = GrammarBuilder.eGramFormat.Phrases;
				}
			}

			// Audio recording.  NOTE - m_bSaveAudioStream is overridden in ProcessSessionBegin().
			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_SaveAudioStream];
			sTmp = (sTmp == null) ? "" : sTmp;
			if( (sTmp.Length != 0) && (sTmp.ToLower() == "true") )
			{
				m_bSaveAudioStream = true;
			}

			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_SaveUttWavs];
			if( (sTmp != null) && (sTmp.ToLower() == "true") )
			{
				m_bSaveUttWavs = true;
			}

			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_DumpGram];
			sTmp = (sTmp == null) ? "" : sTmp;
			if( (sTmp.Length != 0) && (sTmp.ToLower() == "true") )
			{
				m_bDumpGram = true;
			}

			m_sbDtmfReceived = new StringBuilder();
			m_tDtmf = new System.Timers.Timer();
			m_tDtmf.Elapsed += new ElapsedEventHandler(OnDtmfTimerElapsed);
			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_DtmfTimeout];
			sTmp = (sTmp == null) ? "" : sTmp;
			m_tDtmf.Interval = double.Parse(sTmp);
			m_tDtmf.Enabled = false;
		}

		public bool RemotingInit()
		{
			bool		bRet = true;
			Type		tRcv;
			string sTmp = "";

			try
			{
				//tRcv = Type.GetType("AudioMgr.AMPinger");	// Kind of a kludge to get a valid type to use in GetObject().
				tRcv = typeof(AudioMgr.AMPinger);
				if(tRcv == null)
				{
					bRet = false;
					//Console.Error.WriteLine("ERROR AudioInThread.RemotingInit:  Couldn't get type.");
					m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.RemotingInit:  Couldn't get type.");
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
						//Console.Error.WriteLine("ERROR 
						m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.RemotingInit:  Couldn't create DialogMgr remote object.");
					}
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioInThread.RemotingInit:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.RemotingInit: " + exc.ToString());
			}

			return(bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ThreadProc()
		{
			bool					bCont = true;
			bool					bRes = true;
			ISMessaging.ISMsg		mMsg;
			RecognitionResult[]		aResults = null;
			int						ii = 0;
			StringBuilder			sbTmp;

			m_Logger.Init("", "", Thread.CurrentThread.Name, "", "", "");

			m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread started.");

			sbTmp = new StringBuilder();

			// Do Remoting related init.
			bRes = false;
			while(!bRes)
			{
				bRes = RemotingInit();
				if(!bRes)
				{
					Thread.Sleep(5000);
				}
			}
			m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread connected to remote assemblies.");

			// Do ASR related init.
            m_EDet = new EnergyDetector(EnergyDetector.DEFAULT_NOISE_LEVEL, m_iEndpointInMilliseconds);

			bRes = InitASR();
			if(!bRes)
			{
				m_Logger.Log(Level.Warning, string.Format("[{0}] AudioInThread.ThreadProc failed to init ASR(s)!", m_iThreadIndex.ToString()));
			}
			else
			{
				m_Logger.Log(Level.Info, string.Format("[{0}] AudioInThread.ThreadProc ASR(s) initialized.", m_iThreadIndex.ToString()));
			}

			m_AudioState = AudioState.Listening;	// FIX - Waiting for "ready" command, or listening?

			while(bCont)
			{
				try
				{
					// Pull data from audio queue
					// Data messages should always be either raw data or shutdown requests.
					// FIX - perhaps this queue name is a misnomer and the line above is incorrect - the queue should also handle other requests (such as ASR commands), or we need another queue.
#if(false)
					mMsg = m_qMsg.Pop(m_iAudioTimeout);
					if(mMsg == null)
					{
						if(m_bCurrentlyStreaming)
						{
							m_bCurrentlyStreaming = false;
							//Console.WriteLine("Utterance timeout STOP.");
							m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "Utterance timeout STOP.");
							m_asr.UtteranceStop();

							if(m_bSaveAudioStream)
							{
								m_fsUtt.Close();
							}

							bRes = m_asr.Results(out aResults);
							if(!bRes)
							{
								//Console.Error.WriteLine("AudioInThread.ThreadProc() warning - Didn't get any results.");
								m_Logger.Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ThreadProc() warning - Didn't get any results.");
							}
						}
						else
						{
							// Don't really need to do anything here.
						}
					}
#else
					mMsg = m_qMsg.Pop();
					if(mMsg == null)
					{
						m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ThreadProc() - Pop returned null message!");
					}
#endif
					else
					{
						// Work on msg
						switch(mMsg.GetType().ToString())
						{
							case "ISMessaging.Audio.ISMRawData" :
							{
								bRes = ProcessRawAudio((ISMessaging.Audio.ISMRawData)mMsg, out aResults);
							}
							break;
							case "ISMessaging.Audio.ISMDtmf" :
							{
								bRes = ProcessDtmf(((ISMessaging.Audio.ISMDtmf)mMsg).m_sDtmf);
							}
							break;
							case "ISMessaging.Audio.ISMDtmfComplete" :
							{
								bRes = ProcessDtmf((ISMessaging.Audio.ISMDtmfComplete)mMsg);
							}
							break;
							case "ISMessaging.Audio.ISMBargeinEnable" :
							{
								bRes = BargeinEnable();
							}
							break;
							case "ISMessaging.Audio.ISMBargeinDisable" :
							{
								bRes = BargeinDisable();
							}
							break;
                            case "ISMessaging.Audio.ISMSetProperty":
                            {
                                SetProperty((ISMessaging.Audio.ISMSetProperty)mMsg);
                            }
                            break;
							case "ISMessaging.SpeechRec.ISMLoadGrammar" :
							{
								//Console.Error.WriteLine("Error:  AudioEngine_srv.AudioInThread.ThreadProc() Got ISMessaging.SpeechRec.ISMLoadGrammar msg.");
								m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioInThread.ThreadProc() Got ISMessaging.SpeechRec.ISMLoadGrammar msg.");
							}
							break;
							case "ISMessaging.SpeechRec.ISMLoadPhrases" :
							{
								//Console.WriteLine("{0} <<<AudioEngine_srv.AudioInThread -- ISMessaging.SpeechRec.ISMLoadPhrases '{1}'.>>>", m_iThreadIndex, ((ISMessaging.SpeechRec.ISMLoadPhrases)(mMsg)).m_asPhrases);
								m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "ISMessaging.SpeechRec.ISMLoadPhrases: " + ((ISMessaging.SpeechRec.ISMLoadPhrases)(mMsg)).m_asPhrases.Length);
								bRes = ProcessLoadPhrases((ISMessaging.SpeechRec.ISMLoadPhrases)mMsg);
							}
							break;
							case "ISMessaging.SpeechRec.ISMLoadGrammarFile" :
							{
								m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "ISMessaging.SpeechRec.ISMLoadGrammarFile: " + ((ISMessaging.SpeechRec.ISMLoadGrammarFile)(mMsg)).m_sUri);
								bRes = ProcessLoadGrammarFile((ISMessaging.SpeechRec.ISMLoadGrammarFile)mMsg);
							}
							break;
							case "ISMessaging.Session.ISMSessionBegin" :
							{
								// Set IpAddress, SessionId in Logger so all subsequent calls to Log() will be identified.  (Need-to/should do this in every thread once the ISMSessionBegin comes in.)
								m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.IpAddress.ToString(), Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(oAddr => oAddr.AddressFamily == AddressFamily.InterNetwork).ToString() ?? "");
								m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.SessionId.ToString(), mMsg.m_sSessionId);

								bRes = m_asr.Open();
								bRes = ProcessSessionBegin((ISMessaging.Session.ISMSessionBegin)mMsg);
							}
							break;
							case "ISMessaging.Session.ISMSessionEnd" :
							{
								//Console.WriteLine("{0} <<<AudioEngine_srv.AudioInThread -- ISMessaging.Session.ISMSessionEnd>>>", m_iThreadIndex);
								m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "<<<AIT ISMessaging.Session.ISMSessionEnd>>>");
								bRes = ProcessSessionEnd((ISMessaging.Session.ISMSessionEnd)mMsg);
								bRes = m_asr.Close();
							}
							break;
							default :
							{
								//Console.Error.WriteLine("Error:  AudioEngine_srv.AudioInThread.ThreadProc() Got unknown msg type {0}.", mMsg.GetType().ToString());
								m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioInThread.ThreadProc() Got unknown msg type: " + mMsg.GetType().ToString());
							}
							break;
						} // end switch(type)

						// Release object
						mMsg = null;
					}

					// Process results, if any.
					if(aResults != null)
					{
						if(m_bInSession)
						{
							for(ii = 0; ii < aResults.Length; ii++)
							{
								//Console.WriteLine("  RESULT ({0} of {1}) = '{2}', probability {3}%.", (ii + 1), aResults.Length, aResults[ii].Result, aResults[ii].Probability);
								sbTmp.Length = 0;
								sbTmp.AppendFormat("RESULT ({0} of {1}) = '{2}', probability {3}%.", (ii + 1), aResults.Length, aResults[ii].Result, aResults[ii].Probability);
								m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + sbTmp.ToString());
							}

							/////////////////////////////////////////
							// Forward on results.
							/////////////////////////////////////////
							ISMResults					mResults;
							ISAppEndpoint				mSrc, mDest;
							ISMVMC						mVMC, vmcTmp;

							mSrc = new ISAppEndpoint();
							mDest = new ISAppEndpoint();
							mVMC = new ISMVMC();

							vmcTmp = m_RM.GetVMCByKey(m_iThreadIndex);
							if(vmcTmp.m_iKey == -1)		// If this session has already been released
							{
								m_Logger.Log(Level.Warning, string.Format("[{0}]AudioInThread.ThreadProc: VMC not found, ignoring results.", m_iThreadIndex.ToString()));
							}
							else
							{
								mVMC.Init(vmcTmp.m_iKey, vmcTmp.m_sDescription, vmcTmp.m_sSessionId);
								mSrc.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Recognition results");
								mDest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Recognition results");

								mResults = new ISMResults(mSrc);
								mResults.m_Dest = mDest;
								mResults.m_Results = aResults;

								m_mRcv.NewMsg(mResults);		// Send the message
								m_AudioState = AudioState.WaitingForCommand;	// Set queue state.
							}
						}
						else
						{
							m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ThreadProc: Got results when not InSession.");
						}

						aResults = null;
					}
				}
				catch(Exception exc)
				{
					m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ThreadProc: " + exc.ToString());
				}
			}	// end while

			m_asr.Release();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////////////////////
		private bool InitASR()
		{
			bool					bRet = true;
			Assembly				oAsrAssembly = null;
			Type[]					atTypes = null;
			Type					tFound = null;
			int						ii = 0, iTypes = 0;;
			Type[]					oInheritedTypes = null;
			object					oAsr = null;

			try
			{
				//m_asr = new LumenvoxFacade(m_Logger);	// Fix - Decide which ASR to load from config file.
				if(m_aASRs.Count < 1)
				{
					bRet = false;
					m_Logger.Log(Level.Exception, string.Format("[{0}] AudioInThread.InitASR No ASRs loaded!", m_iThreadIndex.ToString()));
				}
				else
				{
					if(m_aASRs.Count > 1)
					{
						m_Logger.Log(Level.Warning, string.Format("[{0}] AudioInThread.InitASR More than one ASR loaded, only using the first one.", m_iThreadIndex.ToString()));
					}
					
					oAsrAssembly = ((Assembly)(m_aASRs[0]));
					atTypes = oAsrAssembly.GetTypes();
					iTypes = atTypes.Length;
					for(ii = 0; ( (ii < iTypes) && (tFound == null) ); ii++)
					{
						oInheritedTypes = atTypes[ii].GetInterfaces();
						foreach(Type tInh in oInheritedTypes)
						{
							if(tInh.FullName == (typeof(ISMessaging.SpeechRec.IASR)).ToString())
							{
								tFound = atTypes[ii];
								m_Logger.Log(Level.Info, string.Format("[{0}] AudioInThread.InitASR found IASR in '{1}'.", m_iThreadIndex.ToString(), tFound.Name));
							}
						}
					}					

					if(tFound == null)
					{
					}
					else
					{
						oAsr = oAsrAssembly.CreateInstance(tFound.FullName);
						if(oAsr == null)
						{
							bRet = false;
							m_Logger.Log(Level.Exception, string.Format("[{0}] AudioInThread.InitASR ASR failed to create '{1}'!", m_iThreadIndex.ToString(), tFound.FullName));
						}
						else
						{
							m_asr = (ISMessaging.SpeechRec.IASR)oAsr;
							if(m_asr == null)
							{
								bRet = false;
								m_Logger.Log(Level.Exception, string.Format("[{0}] AudioInThread.InitASR Failed to cast to IASR!", m_iThreadIndex.ToString()));
							}
							else
							{
								m_Logger.Log(Level.Info, string.Format("[{0}] AudioInThread.InitASR ASR created.", m_iThreadIndex.ToString()));
								bRet = m_asr.Init(m_Logger, m_GramFormat);
								if(!bRet)
								{
									m_Logger.Log(Level.Exception, string.Format("[{0}] AudioInThread.InitASR ASR failed to init!", m_iThreadIndex.ToString()));
								}
								else
								{
									m_Logger.Log(Level.Info, string.Format("[{0}] AudioInThread.InitASR ASR initialized.", m_iThreadIndex.ToString()));
								}
							}
						}
					}
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, string.Format("[{0}] AudioInThread.InitASR Exception: {1}", m_iThreadIndex.ToString(), exc.ToString()));
			}

			return(bRet);
		} // InitASR

		private bool BargeinEnable()
		{
			bool	bRet = true;

			try
			{
				m_AudioState = AudioState.Listening;
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception,"[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.BargeinEnable: " + exc.ToString());
			}

			return(bRet);
		} // BargeinEnable

		private bool BargeinDisable()
		{
			bool	bRet = true;

			try
			{
				m_AudioState = AudioState.WaitingForCommand;
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception,"[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.BargeinDisable: " + exc.ToString());
			}

			return(bRet);
		} // BargeinDisable

		public bool ProcessRawAudio(ISMessaging.Audio.ISMRawData i_mRawData, out RecognitionResult[] o_aResults)
		{
			bool								bRet = true;
			bool								bRes;
			EnergyDetector.eResult				edRes;
			string								sFName;
			ISMessaging.Audio.ISMRawData		mRaw = null;
			ISMessaging.Audio.ISMSpeechStart	mSStart = null;
			ISMessaging.Audio.ISMSpeechStop		mSStop = null;
			bool								bFound = false;
			int									ii = 0;

			o_aResults = null;	// This is just to shut the compiler up - the assignment has to be outside the try{} block.

			try
			{
				edRes = EnergyDetector.eResult.UNDETERMINED;

				// Save raw audio
				mRaw = (ISMessaging.Audio.ISMRawData)i_mRawData;
				if(m_bSaveAudioStream)
				{
					m_fsRaw.Write(mRaw.m_abData, 0, mRaw.m_abData.Length);
				}

				// Do audio-state dependent work
				if(m_AudioState == AudioState.WaitingForCommand)
				{
					i_mRawData = null;	// Toss the audio, because we're waiting for a command.

					// If we're currently streaming, we may have the stream in an invalid state, tell the ASR to discard all data.
					if(m_bCurrentlyStreaming)
					{
						// FIX - tell the ASR to discard all data.
						m_Logger.Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioInThread.ProcessRawAudio() - WaitingForCommand and CurrentlyStreaming.");

						m_bCurrentlyStreaming = false;
					}
				}
				else if(m_AudioState == AudioState.Listening)
				{
					// Perform energy detection
					edRes = m_EDet.Detect(mRaw.m_abData);
					switch(edRes)
					{
						case EnergyDetector.eResult.ENERGY :
						{
							if(!m_bCurrentlyStreaming)
							{
								m_iCurrUttSize = 0;

								//Console.WriteLine("({0}) Utterance START.", m_iThreadIndex);
								m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "Utterance START.");

								// Send a "speech start" msg to the AudioRtr (in case it's needed.)
								mSStart = new ISMessaging.Audio.ISMSpeechStart();
								mSStart.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		// Note - We're assuming that a null VMC is never returned
								m_aqAudioOut[mRaw.m_Source.m_VMC.m_iKey].Push(mSStart);

								if(m_bSaveAudioStream && m_bSaveUttWavs)
								{
									// Create and write first block to utterance file
									sFName = m_sLogPath + "UttAudio_" + ISMessaging.Utilities.GetNumericDateTime(DateTime.Now) + "_" + m_iThreadIndex + "_" + m_UttIndex.ToString() + ".bin";
									if(File.Exists(sFName))
									{
										File.Delete(sFName);
									}
									m_fsUtt = File.Create(sFName);	// Without an Exists() check, we'll always overwrite a file with the same name.
									m_fsUtt.Write(m_EDet.PreData, 0, m_EDet.PreData.Length);
									m_fsUtt.Write(mRaw.m_abData, 0, mRaw.m_abData.Length);
									m_UttIndex++;
								}

								// Tell the corresponding ASR to start and send the first block(s) of data.
								m_bCurrentlyStreaming = true;

								// Send data to ASR
								m_asr.UtteranceStart();
								if(m_EDet.PreData != null)
								{
									m_asr.UtteranceData(m_EDet.PreData);	// Add the previous block, since the beginning of the utt may have been quiet.
								}
								m_asr.UtteranceData(mRaw.m_abData);		// Add the detected block.
							}
							else
							{
								// Send the data block to the corresponding ASR.
								//m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "Utterance DATA.");

								if(m_iCurrUttSize < m_iMaxUttSize)
								{
									m_asr.UtteranceData(mRaw.m_abData);
								}
								m_iCurrUttSize += mRaw.m_abData.Length;

								if(m_bSaveAudioStream && m_bSaveUttWavs)
								{
									m_fsUtt.Write(mRaw.m_abData, 0, mRaw.m_abData.Length);
								}
							}
						}
						break;
						case EnergyDetector.eResult.SILENCE :
						{
							if(m_bCurrentlyStreaming)
							{
								// FIX - May need to keep track of multiple silence blocks?
								m_bCurrentlyStreaming = false;
								//Console.WriteLine("({0}) Utterance STOP.", m_iThreadIndex);
								m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "Utterance STOP.");

								// Send a "speech stop" msg to the AudioRtr (in case it's needed.)
								mSStop = new ISMessaging.Audio.ISMSpeechStop();
								mSStop.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		// Note - We're assuming that a null VMC is never returned
								m_aqAudioOut[mRaw.m_Source.m_VMC.m_iKey].Push(mSStop);

								// Save to file
								if(m_bSaveAudioStream && m_bSaveUttWavs)
								{
									m_fsUtt.Write(mRaw.m_abData, 0, mRaw.m_abData.Length);
									m_fsUtt.Close();
								}

								// Do ASR work
								if(m_iCurrUttSize < m_iMaxUttSize)
								{
									m_asr.UtteranceData(mRaw.m_abData);	// Trailing silence block, just in case.
								}
								m_asr.UtteranceStop();

								bRes = m_asr.Results(out o_aResults);
								if(!bRes)
								{
									//Console.Error.WriteLine("AudioInThread.ProcessRawAudio() warning - Didn't get any results.");
									m_Logger.Log(Level.Warning, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio() warning - Didn't get any results.");

									o_aResults = new RecognitionResult[1];
									o_aResults[0] = new RecognitionResult();
									o_aResults[0].Probability = 0;
									o_aResults[0].Result = "";
									o_aResults[0].Text = "";
								}
								else
								{
									// Match original text to results
									// FIX!!!  This only grabs the first phrase, which will not always be correct.  See comments
									// for ProcessLoadPhrases().
									foreach(RecognitionResult rrTmp in o_aResults)
									{
										if( (m_GramFormat == GrammarBuilder.eGramFormat.SRGS_GRXML) || (m_GramFormat == GrammarBuilder.eGramFormat.SRGS_ABNF) )
										{
											// Find result tag in gram
											bFound = false;

											if(m_gbGram == null)
											{
												m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio() - Gram was null.");
											}
											else if(m_gbGram.PPMaps == null)
											{
												m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio() - PPMaps was null.");
											}
											else if(m_gbGram.PPMaps.Count <= 0)
											{
												m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio() - PPMaps was empty.");
											}
											else
											{
												for(ii = 0; (ii < m_gbGram.PPMaps.Count && !bFound); ii++)
												{
													if(rrTmp.Result == m_gbGram.PPMaps[ii].ResultTag)
													{
														if(m_gbGram.PPMaps[ii].Phrases.Count <= 0)
														{
															m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio() - PPMaps.Phrases was empty.");
														}
														else
														{
															rrTmp.Text = m_gbGram.PPMaps[ii].Phrases[0];
															bFound = true;
														}
													}
												}
											}
										}

										if( (!bFound) || (rrTmp.Text.Length <= 0) )
										{
											rrTmp.Text = rrTmp.Result;
										}
									}
								}
							}
							else
							{
								// Anything to do here?
							}

							m_iCurrUttSize = 0;
						}
						break;
						default :
						{
							m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio() - Invalid state returned by energy detector - '{0}'." + edRes.ToString());
						}
						break;
					}	// end switch(detect)
				}
				else if(m_AudioState == AudioState.Paused)
				{
					m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioInThread.ProcessRawAudio() should never be in the 'Paused' state.");
				}
				else if(m_AudioState == AudioState.ReceivingDTMF)
				{
					// Need to do anything?
				}
				else
				{
					m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioEngine_srv.AudioInThread.ProcessRawAudio() unknown state.");
				}	// end if
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessRawAudio: " + exc.ToString() + Environment.NewLine);
			}
				
			return(bRet);
		} // ProcessRawAudio

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sDtmf"></param>
		/// <returns></returns>
		public bool ProcessDtmf(string i_sDtmf)
		{
			bool bRet = true;

			try
			{
				m_AudioState = AudioState.ReceivingDTMF;
				if (m_bCurrentlyStreaming)
				{
                    RecognitionResult[] aResults = null;

                    m_Logger.Log(Level.Debug, String.Format("[{0}]AudioInThread.ProcessDtmf while streaming.", m_iThreadIndex));


					// Stop the ASR and clean up

					m_bCurrentlyStreaming = false;
					m_asr.UtteranceStop();
					m_asr.Results(out aResults);
					aResults = null;


					// Send a "speech stop" msg to the AudioRtr (in case it's needed.)

                    ISMessaging.Audio.ISMSpeechStop mSStop = new ISMessaging.Audio.ISMSpeechStop();
					mSStop.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		// Note - We're assuming that a null VMC is never returned
					m_aqAudioOut[m_iThreadIndex].Push(mSStop);

                    m_EDet.Reset();                                                             // Reset the ED to clear any DTMF audio from the buffer.
				}

                if (m_sbDtmfReceived.Length == 0)
                {
                    // Send a "DTMF start" message to the AudioOutput queue to stop the inactivity timer.

                    ISMessaging.Audio.ISMDtmfStart mDtmfStart = new ISMessaging.Audio.ISMDtmfStart();
                    mDtmfStart.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		// Note - We're assuming that a null VMC is never returned
                    m_aqAudioOut[m_iThreadIndex].Push(mDtmfStart);
                }

                if (i_sDtmf == m_sDtmfTerminationCharacter)
                {
                    // Stop the timer

                    m_tDtmf.Enabled = false;


                    // If all we've received so far is the DTMF termination character then send it to the DialogManager.  This ensures that any DTMF menus
                    // designed with the DialogDesigner will keep working as expected.  In other words, if the DTMF termination character is '#' but a
                    // menu has an action tied to '#' then if that is all the caller presses then that menu action will be executed.
                    // If we've received other DTMF characters prior to receiving the DTMF termination character then do not append the DTMF termination
                    // character so we don't have to worry about stripping it off the caller's input in the VoiceXML code.

                    if (m_sbDtmfReceived.Length == 0)
                    {
                        m_sbDtmfReceived.Append(i_sDtmf);
                    }

                    SendDtmfToDialogManager();
                }
                else
                {
                    m_sbDtmfReceived.AppendFormat(i_sDtmf);


                    // Reset the timer

                    m_tDtmf.Enabled = false;
                    m_tDtmf.Enabled = true;
                }
			}
			catch (Exception exc)
			{
				bRet = false;
                m_Logger.Log(Level.Exception, String.Format("[{0}]AudioInTnread.ProcessDtmf: {1}", m_iThreadIndex, exc.ToString()));
			}

			return bRet;
		} // ProcessDtmf

		public bool ProcessDtmf(ISMessaging.Audio.ISMDtmfComplete i_mDtmf)
		{
			bool bRet = true;

            try
			{
                m_Logger.Log(Level.Debug, String.Format("[{0}]Final DTMF: '{1}'.", m_iThreadIndex, i_mDtmf.m_sDtmf));


                // Send a "DTMF stop" message to the AudioOutput queue to restart the inactivity hangup timer.

                ISMessaging.Audio.ISMDtmfStop mDtmfStop = new ISMessaging.Audio.ISMDtmfStop();
                mDtmfStop.m_sSessionId = m_RM.GetVMCByKey(m_iThreadIndex).m_sSessionId;		    // Note - We're assuming that a null VMC is never returned
                m_aqAudioOut[m_iThreadIndex].Push(mDtmfStop);


				// Forward to DialogMgr for processing.

                ISAppEndpoint mSrc = new ISAppEndpoint();
                ISAppEndpoint mDest = new ISAppEndpoint();
                ISMVMC mVMC = new ISMVMC();

                ISMVMC vmcTmp = m_RM.GetVMCByKey(m_iThreadIndex);
				if(vmcTmp.m_iKey == -1)                             // If this session has already been released
				{
					m_Logger.Log(Level.Warning, String.Format("[{0}]AudioInThread.ProcessDtmf: VMC not found, ignoring DTMF.", m_iThreadIndex));
				}
				else
				{
					mVMC.Init(vmcTmp.m_iKey, vmcTmp.m_sDescription, vmcTmp.m_sSessionId);
					mSrc.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "DTMF Complete");
					mDest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "DTMF Complete");
					i_mDtmf.Init(mSrc, mDest, i_mDtmf.m_sDtmf);

					m_mRcv.NewMsg(i_mDtmf);                         // Send the message

					m_AudioState = AudioState.WaitingForCommand;    // Set queue state.
				}
			}
			catch (Exception exc)
			{
				bRet = false;
                m_Logger.Log(Level.Exception, String.Format("[{0}]AudioInThread.ProcessDtmf: {1}", m_iThreadIndex, exc.ToString()));
			}

			return bRet;
		} // ProcessDtmf


		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_oSource"></param>
		/// <param name="i_eArgs"></param>
		private void OnDtmfTimerElapsed(object i_oSource, ElapsedEventArgs i_eArgs)
		{
			// First, stop the timer.
			m_tDtmf.Enabled = false;

            SendDtmfToDialogManager();
		}

        private void SendDtmfToDialogManager()
        {
            ISMessaging.Audio.ISMDtmfComplete mComp = new ISMDtmfComplete();
            mComp.m_sDtmf = m_sbDtmfReceived.ToString();
            m_qMsg.Push(mComp);

            m_sbDtmfReceived.Length = 0;
        }


		/// <summary>
		/// Add the supplied phrases to the ASR as a grammar.
		/// FIX!!!  As of 8/23/2007, this code does not take advantave of the Tag capability (matching
		/// multiple utterances to one symantic result.)  This was done intentionally, but will not always
		/// be correct.
		/// </summary>
		/// <param name="i_mPhrases"></param>
		/// <returns></returns>
		public bool ProcessLoadPhrases(ISMessaging.SpeechRec.ISMLoadPhrases i_mPhrases)
		{
			bool						bRet = true, bRes = true;
			int							ii = 0, iNumPhrases = 0;
			string						sGram = "";
			PhrasePairMap				ppmTmp = null;
			GrammarBuilder.eGramErr		eRes = GrammarBuilder.eGramErr.Success;
			string						sGramLocalPath = m_sLogPath;

			try
			{
				iNumPhrases = i_mPhrases.m_asPhrases.Length;
				if(iNumPhrases > 0)
				{
					// Reset the grammar and load the new phrases.
					bRes = m_asr.ResetGrammar();

					if(m_GramFormat == GrammarBuilder.eGramFormat.Phrases)
					{
						for(ii = 0; ii < iNumPhrases; ii++)
						{
							bRes = m_asr.AddPhrase(i_mPhrases.m_asPhrases[ii], i_mPhrases.m_asPhrases[ii]);	// Old way
						}
					}
					else
					{
						m_gbGram = new GrammarBuilder();		// Resets the grammar
						m_gbGram.LangCode = i_mPhrases.m_sLangCode;

						for(ii = 0; ii < iNumPhrases; ii++)
						{
							ppmTmp = new PhrasePairMap();

							// The trailing number is added to ResultTag to make each tag unique, as it nearly impossible
							// to prevent duplicates, which causes some GRXML interpreters (you know who) to completely fail.
							ppmTmp.ResultTag = GrammarBuilder.RemoveSpaces(i_mPhrases.m_asPhrases[ii]) + ii.ToString() + "I";
							ppmTmp.Phrases.Add(i_mPhrases.m_asPhrases[ii]);
							m_gbGram.Add(ppmTmp);
						}

						switch(m_GramFormat)
						{
							case GrammarBuilder.eGramFormat.SRGS_GRXML :
							{
								eRes = m_gbGram.Encode(GrammarBuilder.eGramFormat.SRGS_GRXML, out sGram);
							}
							break;
							case GrammarBuilder.eGramFormat.SRGS_ABNF :
							default :
							{
								eRes = m_gbGram.Encode(GrammarBuilder.eGramFormat.SRGS_ABNF, out sGram);
							}
							break;
						}

						if(m_bDumpGram)
						{
							m_Logger.Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "Grammar: '" + sGram + "'.");
						}

						if(eRes != GrammarBuilder.eGramErr.Success)
						{
							m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessLoadPhrases failed to encode grammar, error: " + eRes.ToString());
						}
						else
						{
#if(true)
							// Load grammar as a string.
							bRes = m_asr.LoadGrammar(false, "main", sGram);
#else
							// Load grammar as a file.
							StreamWriter				swGram = null;

							sGramLocalPath += "MainGram_" + m_iThreadIndex.ToString() + ".srgs.xml";
							m_Logger.Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "] ProcessLoadPhrases Writing grammar to: " + sGramLocalPath);
							swGram = new StreamWriter(sGramLocalPath, false, Encoding.UTF8);
							swGram.Write(sGram);
							swGram.Close();

							bRes = m_asr.LoadGrammarFromFile(false, "main", sGramLocalPath);
#endif
						}
					}
				}
				else
				{
					// Nothing new to load, keep using current.
					m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessLoadPhrases: Keep using current grammar.");
				}

				// Start listening again.
				m_AudioState = AudioState.Listening;
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioInThread.ProcessLoadPhrases:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessLoadPhrases: " + exc.ToString());
			}

			return(bRet);
		}

		private bool ProcessLoadGrammarFile(ISMessaging.SpeechRec.ISMLoadGrammarFile i_mLoad)
		{
			bool		bRet = true;

			try
			{
				bRet = m_asr.LoadGrammarFromFile(false, "main", i_mLoad.m_sUri);

				// Start listening again.
				m_AudioState = AudioState.Listening;
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessLoadGrammarFile: " + exc.ToString());
			}

			return(bRet);
		}

		public bool ProcessSessionBegin(ISMessaging.Session.ISMSessionBegin i_mSBegin)
		{
			bool				bRet = true, bRes = true, bAudioRec = false;
			ISAppEndpoint		mSrc = null, mDest = null;
			ISMVMC				mVMC = null, vmcTmp = null;
			string				sFName = "";
			ConfigParams		cfgs = null;
			int					ii = 0, iNumElems = 0;

			try
			{
				m_sSessionId = i_mSBegin.m_sSessionId;
				//Console.WriteLine("{0} <<<AudioEngine_srv.AudioInThread -- ISMessaging.Session.ISMSessionBegin>>>", m_iThreadIndex);
				m_Logger.Log(Level.Info, string.Format( "[{0}]<<<AIT ISMessaging.Session.ISMSessionBegin>>>{1}", m_iThreadIndex.ToString(), m_sSessionId.ToString()));

                m_EDet.Reset();                                             // Reset energy detector for new call.


				// Load the audio recording flag from DB
				cfgs = new ConfigParams();
				bRes = cfgs.LoadFromTableByComponent(ConfigParams.e_Components.Apps.ToString() + ConfigParams.c_Separator + ConfigParams.e_Components.global.ToString());
				if(!bRes)
				{
					m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionBegin: Couldn't retrieve DB settings!");

					bAudioRec = m_bSaveAudioStream;							// Use the config file setting.
					m_EDet.SetNoiseLevel(EnergyDetector.DEFAULT_NOISE_LEVEL);
				}
				else
				{
					iNumElems = cfgs.Count;
					for(ii = 0; ii < iNumElems; ii++)
					{
						if(cfgs[ii].Name == ConfigParams.e_SpeechAppSettings.AudioRecording.ToString())
						{
							bAudioRec = (cfgs[ii].Value == "1") ? true : false;
							m_bSaveAudioStream = bAudioRec;
						}
						else if(cfgs[ii].Name == ConfigParams.e_SpeechAppSettings.EDNoiseLevel.ToString())
						{
                            Int16 wEDNoiseLevel = EnergyDetector.DEFAULT_NOISE_LEVEL;

							try
							{
								wEDNoiseLevel = Int16.Parse(cfgs[ii].Value);
							}
							catch(Exception exc)
							{
								m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionBegin: Exception parsing EDNoiseLevel from DB: " + exc.ToString());
								wEDNoiseLevel = EnergyDetector.DEFAULT_NOISE_LEVEL;
							}

							m_EDet.SetNoiseLevel(wEDNoiseLevel);
						}
					}
				}

				if(bAudioRec)
				{
					//sFName = m_sLogPath + "RawAudio_" + ISMessaging.Utilities.GetNumericDateTime(DateTime.Now) + "_" + m_iThreadIndex + "_" + m_iRawIndex.ToString() + ".bin";
					sFName = m_sLogPath + "RawAudio_" + i_mSBegin.m_sSessionId.Replace(':', '_') + ".bin";
					//m_fsRaw = File.Open(sFName, FileMode.OpenOrCreate);	// Without an Exists() check, we'll always overwrite a file with the same name.
					if(File.Exists(sFName))
					{
						File.Delete(sFName);
					}
					m_fsRaw = File.Create(sFName);

					m_Logger.Log(Level.Info, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionBegin: Saving user audio to '" + sFName + "'.");
				}

				// Forward msg to DialogMgr
				mSrc = new ISAppEndpoint();
				mDest = new ISAppEndpoint();
				mVMC = new ISMVMC();

				vmcTmp = m_RM.GetVMCByKey(m_iThreadIndex);
				if(vmcTmp.m_iKey == -1)		// If this session has already been released
				{
					m_Logger.Log(Level.Warning, string.Format("[{0}]AudioInThread.ProcessSessionBegin: VMC not found, ignoring Begin.", m_iThreadIndex.ToString()));
				}
				else
				{
					mVMC.Init(vmcTmp.m_iKey, vmcTmp.m_sDescription, vmcTmp.m_sSessionId);
					mSrc.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Session begin");
					mDest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Session begin");

					i_mSBegin.m_Source = mSrc;
					i_mSBegin.m_Dest = mDest;

if(m_mRcv == null)
{
m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionBegin: m_mRcv was null!");
}
if(i_mSBegin == null)
{
m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionBegin: i_mSBegin was null!");
}

					m_mRcv.NewMsg(i_mSBegin);

					m_AudioState = AudioState.Listening;
					m_bInSession = true;
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioInThread.ProcessSessionBegin:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionBegin: " + exc.ToString());
			}

			return(bRet);
		}

		public bool ProcessSessionEnd(ISMessaging.Session.ISMSessionEnd i_mSEnd)
		{
			bool					bRet = true, bRes = true;
			ISAppEndpoint			mSrc, mDest;
			ISMVMC					mVMC, vmcTmp;
			RecognitionResult[]		aResults = null;

			try
			{
				// Note:  If the Begin message wasn't received, the m_sSessionId will be logged either as empty (in the first session) or invalid
				if(m_bCurrentlyStreaming)
				{
					m_bCurrentlyStreaming = false;
					m_Logger.Log(Level.Verbose, "[" + m_iThreadIndex.ToString() + "]" + "ProcessSessionEnd: Utterance timeout STOP.");
					m_asr.UtteranceStop();

					if(m_bSaveAudioStream && m_bSaveUttWavs)
					{
						m_fsUtt.Close();
					}

					bRes = m_asr.Results(out aResults);
					if(!bRes)
					{
						m_Logger.Log(Level.Debug, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionEnd() - Didn't get any results.");
					}
					aResults = null;
				}

				if(m_bSaveAudioStream)
				{
					m_fsRaw.Close();
					m_iRawIndex++;
				}

				// Forward msg to DialogMgr
				mSrc = new ISAppEndpoint();
				mDest = new ISAppEndpoint();
				mVMC = new ISMVMC();

				vmcTmp = m_RM.GetVMCByKey(m_iThreadIndex);
				if(vmcTmp.m_iKey == -1)		// If this session has either already been released or was not successfully set up in the first place
				{
					m_Logger.Log(Level.Warning, string.Format("[{0}]AudioInThread.ProcessSessionEnd: VMC not found, ignoring End.", m_iThreadIndex.ToString()));
				}
				else
				{
					mVMC.Init(vmcTmp.m_iKey, vmcTmp.m_sDescription, vmcTmp.m_sSessionId);
					mSrc.Init(mVMC, Environment.MachineName, EApplication.eAudioMgr, "Session end");
					mDest.Init(mVMC, "unknown machine destination", EApplication.eDialogMgr, "Session end");

					i_mSEnd.m_Source = mSrc;
					i_mSEnd.m_Dest = mDest;

					m_mRcv.NewMsg(i_mSEnd);

					// Remove session from local RM.
					// Note - This was placed here to be as close as possible to DialogMgr execution.
					// FIX - Should the following two lined be outside the `if(vmcTmp.m_iKey == -1)` check?  Do we always want to set m_bInSession to false?
					m_RM.ReleaseSession(m_iThreadIndex);
					m_bInSession = false;
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR .ToString());
				m_Logger.Log(Level.Exception, "[" + m_iThreadIndex.ToString() + "]" + "AudioInThread.ProcessSessionEnd: " + exc.ToString());
			}

			return(bRet);
		}

        private void SetProperty(ISMessaging.Audio.ISMSetProperty i_mSetProperty)
        {
            try
            {
                switch (i_mSetProperty.m_sName)
                {
                    case "completetimeout":
                        SetCompleteTimeoutProperty(i_mSetProperty.m_sValue);
                        break;

                    case "interdigittimeout":
                        SetInterDigitTimeout(i_mSetProperty.m_sValue);
                        break;

                    case "termchar":
                        SetTermChar(i_mSetProperty.m_sValue);
                        break;

                    default:
                        m_Logger.Log(Level.Exception, String.Format("Setting of property '{0}' not implemented.", i_mSetProperty.m_sName));
                        break;
                }
            }
            catch (Exception exc)
            {
                m_Logger.Log(Level.Exception, String.Format("[{0}]AudioInThread.SetProperty: {1}", m_iThreadIndex, exc.ToString()));
            }

            return;
        }

        private void SetCompleteTimeoutProperty(string i_sEndpointInMilliseconds)
        {
            int iEndpointInMilliseconds = Int32.Parse(i_sEndpointInMilliseconds);

            if (iEndpointInMilliseconds != m_iEndpointInMilliseconds)
            {
                m_iEndpointInMilliseconds = iEndpointInMilliseconds;
                m_EDet.SetEndpoint(m_iEndpointInMilliseconds);

                m_Logger.Log(Level.Info, String.Format("Endpoint set to {0}ms.", m_iEndpointInMilliseconds));           //$$$ LP - This should really be in EnergyDetector.
            }
        }

        private void SetInterDigitTimeout(string i_sInterDigitTimeoutInMilliseconds)
        {
            double dInterDigitTimeoutInMilliseconds = Double.Parse(i_sInterDigitTimeoutInMilliseconds);

            if (dInterDigitTimeoutInMilliseconds != m_tDtmf.Interval)
            {
                m_tDtmf.Interval = dInterDigitTimeoutInMilliseconds;

                m_Logger.Log(Level.Info, String.Format("DTMF timeout set to {0}ms.", i_sInterDigitTimeoutInMilliseconds));
            }
        }

        private void SetTermChar(string i_sDtmfTerminationCharacter)
        {
            if (i_sDtmfTerminationCharacter != m_sDtmfTerminationCharacter)
            {
                m_sDtmfTerminationCharacter = i_sDtmfTerminationCharacter;

                m_Logger.Log(Level.Info, String.Format("DTMF termination character set to '{0}'.", m_sDtmfTerminationCharacter));
            }
        }

    } // class AudioInThread
}
