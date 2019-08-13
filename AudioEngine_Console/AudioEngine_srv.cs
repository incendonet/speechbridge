// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using SBResourceMgr;

namespace AudioMgr
{
	public class AudioEngine_srv
	{
		private		const	string					m_csDefaultAsr = "AsrFacadeLumenvox";
		public				AMSockConns				m_AMSocks = null;
		private				MsgQueue[]				m_aqAudioIn = null;
		private				AudioOutMsgQueue[]		m_aqAudioOut = null;
		private				int						m_NumAudioStreams = 1;			// Used to allocate threads and queues.
		private				Thread[]				m_atARMsgListenerThreads = null, m_atAudioIns = null, m_atAudioOuts = null;
		protected			IResourceMgr			m_RM = null;
		private				AMSockData[]			m_SockMsgs = null;
		protected			ILegacyLogger			m_Logger = null;
		protected			ArrayList				m_aASRs = null;

		public AudioEngine_srv(ILegacyLogger i_Logger)
		{
			m_Logger = i_Logger;
			m_SockMsgs = new AMSockData[0];
			m_RM = new ResourceMgr_local(m_Logger);
			m_aASRs = new ArrayList();

			m_NumAudioStreams = m_RM.GetMaxSessions();

			CreateAudioRouterSockets();
		}

		public void NewMsg(Object i_sender, ISMessaging.Delivery.ISMDistributer.ISMDistributerEventArgs i_args)
		{
			ISMessaging.ISMsg	mMsg = null;

			lock(this)
			{
				try
				{
					//Console.WriteLine("AudioMgr_srv.NewMsg received on TID {0}.", AppDomain.GetCurrentThreadId());

					// Copy and release the original message (so that we don't have to call back to the remote client.)
					switch(i_args.m_Msg.GetType().ToString())		// FIX - There has to be a better way to do this...
					{
						/////////////////////////////////////
						// Audio in messages
						case "ISMessaging.SpeechRec.ISMResults" :
						{
							ISMessaging.SpeechRec.ISMResults	mTmp, mClone;

							mTmp = (ISMessaging.SpeechRec.ISMResults)i_args.m_Msg;
							mClone = (ISMessaging.SpeechRec.ISMResults)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioIn[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;
						case "ISMessaging.SpeechRec.ISMLoadPhrases" :
						{
							ISMessaging.SpeechRec.ISMLoadPhrases	mTmp, mClone;

							mTmp = (ISMessaging.SpeechRec.ISMLoadPhrases)i_args.m_Msg;
							mClone = (ISMessaging.SpeechRec.ISMLoadPhrases)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioIn[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;
						case "ISMessaging.SpeechRec.ISMLoadGrammarFile" :
						{
							ISMessaging.SpeechRec.ISMLoadGrammarFile	mTmp, mClone;

							mTmp = (ISMessaging.SpeechRec.ISMLoadGrammarFile)i_args.m_Msg;
							mClone = (ISMessaging.SpeechRec.ISMLoadGrammarFile)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioIn[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;
                        case "ISMessaging.Audio.ISMSetProperty":
                        {
                            ISMessaging.Audio.ISMSetProperty mTmp = (ISMessaging.Audio.ISMSetProperty)i_args.m_Msg;
                            ISMessaging.Audio.ISMSetProperty mClone = (ISMessaging.Audio.ISMSetProperty)mTmp.Clone();
                            mMsg = mClone;

                            m_aqAudioIn[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
                        }
                        break;

						/////////////////////////////////////
						// Audio out messages
						case "ISMessaging.Audio.ISMPlayPrompts" :
						{
							ISMessaging.Audio.ISMPlayPrompts	mTmp, mClone;

							mTmp = (ISMessaging.Audio.ISMPlayPrompts)i_args.m_Msg;
							mClone = (ISMessaging.Audio.ISMPlayPrompts)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioOut[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;

						/////////////////////////////////////
						// Audio out messages
						case "ISMessaging.Session.ISMTransferSession" :
						{
							ISMessaging.Session.ISMTransferSession	mTmp, mClone;

							mTmp = (ISMessaging.Session.ISMTransferSession)i_args.m_Msg;
							mClone = (ISMessaging.Session.ISMTransferSession)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioOut[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;
						// Other session messages (always Audio Out messages?)
						case "ISMessaging.Session.ISMTerminateSession" :
						{
							ISMessaging.Session.ISMTerminateSession	mTmp, mClone;

							mTmp = (ISMessaging.Session.ISMTerminateSession)i_args.m_Msg;
							mClone = (ISMessaging.Session.ISMTerminateSession)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioOut[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;
						case "ISMessaging.Session.ISMTerminateSessionAfterPrompts" :
						{
							ISMessaging.Session.ISMTerminateSessionAfterPrompts	mTmp, mClone;

							mTmp = (ISMessaging.Session.ISMTerminateSessionAfterPrompts)i_args.m_Msg;
							mClone = (ISMessaging.Session.ISMTerminateSessionAfterPrompts)mTmp.Clone();
							mMsg = (ISMessaging.ISMsg)mClone;

							m_aqAudioOut[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
						}
						break;
                        case "ISMessaging.Session.ISMDialogManagerIdle":
                        {
                            ISMessaging.Session.ISMDialogManagerIdle mTmp, mClone;

                            mTmp = (ISMessaging.Session.ISMDialogManagerIdle)i_args.m_Msg;
                            mClone = (ISMessaging.Session.ISMDialogManagerIdle)mTmp.Clone();
                            mMsg = (ISMessaging.ISMsg)mClone;

                            m_aqAudioOut[mMsg.m_Dest.m_VMC.m_iKey].Push(mMsg);
                        }
                        break;

						/////////////////////////////////////
						// Unknown destination messages
						case "ISMessaging.ISMsg" :
						{
							//Console.Error.WriteLine("WARNING:  AudioMgr_Srv.NewMsg() - Message type was ISMsg.");
							m_Logger.Log(Level.Warning, "AudioMgr_Srv.NewMsg() - Message type was ISMsg.");
							mMsg = (ISMessaging.ISMsg)i_args.m_Msg.Clone();

							// Where would this message go?
						}
						break;
						default :
						{
							//Console.Error.WriteLine("ERROR:  AudioMgr_Srv.NewMsg() - Message type was not recognized: '{0}'.", i_args.m_Msg.GetType().ToString());
							m_Logger.Log(Level.Exception, "AudioMgr_Srv.NewMsg() - Message type was not recognized: " + i_args.m_Msg.GetType().ToString());
						}
						break;
					} // switch

					// Release remote object
					i_args = null;
				}
				catch(Exception exc)
				{
					m_Logger.Log(Level.Exception, "AudioMgr_Srv.NewMsg() caught exception: " + exc.ToString());
				}
			} // lock
		}

		public bool ProcessMsg(string i_sXmlMsg)
		{
			bool	bRet = true;

			//Console.WriteLine("AES - In ProcessMsg()");
			m_Logger.Log(Level.Debug, "AES - In ProcessMsg()");

			return(bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sCmd"></param>
		/// <returns></returns>
		public bool ProcessCmdString(string i_sCmd)
		{
			bool	bRet = true;

			switch(i_sCmd)
			{
				case "q":
				case "Q":
					bRet = false;
					break;
				case "n":
				case "N":
					SimNewSession();
					break;
				case "c":
				case "C":
					CaptureFromSocket();
					break;
				case "s":
				case "S":
					SaveCapturedAudio();
					break;
				case "l":
				case "L":
					LoadCapturedAudio();
					break;
				case "t":
				case "T":
					//CreateWorkerThreads();
					break;
				case "v":
				case "V":
					ConvertFileMuToPCM16();
					break;
				case "wr":
				case "WR":
					ReadWAVFileHeader();
					break;
				case "ww":
				case "WW":
					WriteWAVFileHeader();
					break;
				case "":
					break;
				case "h":
				case "H":
				case "?":
					Console.WriteLine("Your options are:");
					Console.WriteLine("  'h' - Help.");
					Console.WriteLine("  'q' - Quit.");
					Console.WriteLine("  'n' - New session on a VMC.");
					Console.WriteLine("  'c' - Capture audio from AudioRouter socket.");
					Console.WriteLine("  's' - Save captured audio to a file.");
					Console.WriteLine("  'l' - Load captured audio from a file.");
					Console.WriteLine("  't' - Create the worker threads.");
					Console.WriteLine("  'wr' - Read WAV file header.");
					Console.WriteLine("  'ww' - Write WAV file header to a raw file.");
					break;
				default:
					Console.WriteLine("Invalid command entered, try 'h' or '?' for help.");
					break;
			}

			return(bRet);
		}

		/// <summary>
		/// Creates array of sockets, but doesn't connect them.
		/// </summary>
		/// <returns></returns>
		private bool CreateAudioRouterSockets()
		{
			bool	bRet = true;
			int		ii, iSessions;
			int		iFirstPortNum = 1780;
			string	sTmp = "";

			try
			{
				iSessions = m_RM.GetMaxSessions();
				m_AMSocks = new AMSockConns(m_Logger);

				sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_FirstAudioRtrPort];
				sTmp = (sTmp == null) ? "" : sTmp;
				if(sTmp.Length > 0)
				{
					iFirstPortNum = int.Parse(sTmp);
				}
				for(ii = 0; ii < iSessions; ii++)
				{
					m_AMSocks.NewSockConn( (iFirstPortNum + (ii * 2)), (iFirstPortNum + ((ii * 2) + 1)));
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioMgr.AudioEngine_srv.CreateAudioRouterSockets:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(e);
			}

			return(bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Logger"></param>
		/// <returns></returns>
		public bool LoadASR(ILegacyLogger i_Logger)
		{
			bool				bRet = true, bRes = true;
			string				sConfigLine = "";
			StringCollection	asASRs = null;
			Assembly			aASR = null;

			try
			{
				asASRs = new StringCollection();

				sConfigLine = ConfigurationManager.AppSettings[AudioEngine.cs_AsrToLoad];
				sConfigLine = (sConfigLine == null) ? "" : sConfigLine;

				if(sConfigLine.Length == 0)
				{
					// No ASR was specified, load the default by adding it to the string collection.  (Roundabout, but didn't want to duplicate the Assembly.Load() code.)
					i_Logger.Log(Level.Warning, string.Format(" AudioInThread.LoadASR No ASR was specified in config file, loading default '{0}'.", m_csDefaultAsr));
					asASRs.Add(m_csDefaultAsr);
					bRes = true;
				}
				else
				{
					// Parse out the entries from the semicolon separated string
					bRes = Utilities.GetItemsFromString(sConfigLine, ';', asASRs);
				}

				if(bRes)
				{
					// FIX - This loads multiple ASR facades if specified, but will need more work to know how to handle more than one.
					if(asASRs.Count > 1)
					{
						i_Logger.Log(Level.Warning, "AudioInThread.LoadASR More than one ASR in config file.");
					}

					foreach(string sAsrName in asASRs)
					{
						try
						{
							aASR = Assembly.Load(sAsrName);	// Note: there is no corresponding 'Unload' in the FCL, you have to unload all appdomains that contain it.  See http://www.google.com/search?as_q=assembly+unload, http://msdn.microsoft.com/en-us/library/ms173101%28v=vs.80%29.aspx
							if(aASR == null)
							{
								i_Logger.Log(Level.Exception, string.Format(" AudioInThread.LoadASR Load of '{0}' failed!", sAsrName));
							}
							else
							{
								i_Logger.Log(Level.Info, string.Format(" AudioInThread.LoadASR Loaded '{0}' from '{1}'.", aASR.FullName, aASR.Location));

								// Save identifier to a list to be used to create instance of ASR
								m_aASRs.Add(aASR);
							}
						}
						catch(Exception exc)
						{
							i_Logger.Log(Level.Exception, string.Format(" AudioInThread.LoadASR Load of '{0}' failed!  Exception: {1}", sAsrName, exc.ToString()));
						}
					}
				}
				else
				{
					i_Logger.Log(Level.Warning, " AudioInThread.LoadASR No ASRs in config file to load.");
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioInThread.LoadASR:  Caught exception '{0}'.", e.ToString());
				i_Logger.Log(Level.Warning, string.Format(" AudioInThread.LoadASR Exception: {0}", exc.ToString()));
			}

			return(bRet);
		} // LoadASR

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool CreateWorkerThreads(string i_sLogPath)
		{
			bool					bRet = true;
			int						ii;
			ARMsgListenerThread		tARMsgListener;
			AudioInThread			tAudioIn;
			AudioOutThread			tAudioOut;

			try
			{
				// Create audio listener thread array.
				m_atARMsgListenerThreads = new Thread[m_NumAudioStreams];

				// Create audio queues and ASR (audio in) wrapper thread array.
				// AudioIn thread receives audio data in messages popped from the
				// corresponding AudioEngine_srv.m_aqAudioIn queue.
				m_aqAudioIn = new ISMessaging.MsgQueue[m_NumAudioStreams];
				m_atAudioIns = new Thread[m_NumAudioStreams];

				// Create audio out queues and TTS/WAV (audio out) wrapper thread array.
				m_aqAudioOut = new AudioOutMsgQueue[m_NumAudioStreams];
				m_atAudioOuts = new Thread[m_NumAudioStreams];

				for(ii = 0; ii < m_NumAudioStreams; ii++)
				{
					// Create message queues
					m_aqAudioIn[ii] = new ISMessaging.MsgQueue(ii);
					m_aqAudioOut[ii] = new AudioOutMsgQueue(ii);

					// Create and start audio in & out threads
					tAudioIn = new AudioInThread(m_Logger, m_RM, ii, m_aqAudioIn[ii], 2000, i_sLogPath, m_aASRs, m_aqAudioOut);	// FIX - Read the default value from a config file?
					m_atAudioIns[ii] = new Thread(new ThreadStart(tAudioIn.ThreadProc));
					m_atAudioIns[ii].Name = ii.ToString() + "_AudioInT";
					m_atAudioIns[ii].Start();

					tAudioOut = new AudioOutThread(m_Logger, m_RM, ii, m_aqAudioOut[ii], 2000, m_AMSocks, m_aqAudioIn, m_aqAudioOut);	// FIX - Read the default value from a config file?
					m_atAudioOuts[ii] = new Thread(new ThreadStart(tAudioOut.ThreadProc));
					m_atAudioOuts[ii].Name = ii.ToString() + "_AudioOutT";
					m_atAudioOuts[ii].Start();

					// Create AudioRouter Msg thread (but start it later.)
					tARMsgListener = new ARMsgListenerThread(m_Logger, m_RM, ii, ((AMSockConns.AMSockConn)(m_AMSocks.m_Socks[ii])), m_aqAudioIn, m_aqAudioOut);	// FIX - Read the default value from a config file?
					m_atARMsgListenerThreads[ii] = new Thread(new ThreadStart(tARMsgListener.ThreadProc));
					m_atARMsgListenerThreads[ii].Name = ii.ToString() + "_ARMsgListenerT";
				}

				// Start AudioRouter msg threads after all other queues & threads have been initialized and started
				// (because we don't want to start receiving messages from AR until we're ready.)
				// (Note - In the future there may be further initialization necessary before they can start.)
				for(ii = 0; ii < m_NumAudioStreams; ii++)
				{
					m_atARMsgListenerThreads[ii].Start();
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioMgr.AudioEngine_srv.CreateWorkerThreads:  Caught exception '{0}'.", e.ToString());
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
				for(ii = 0; ii < m_NumAudioStreams; ii++)
				{
					sTmp = "Waiting for thread #" + ii.ToString() + " of " + m_NumAudioStreams.ToString() + " to complete...";
					m_Logger.Log(Level.Debug, sTmp);

					m_atARMsgListenerThreads[ii].Join();

					sTmp = "Thread #" + ii.ToString() + " completed.";
					m_Logger.Log(Level.Debug, sTmp);
				}
			}
			catch(Exception e)
			{
				bRet = false;
				//Console.Error.WriteLine("ERROR AudioMgr.AudioEngine_srv.CreateWorkerThreads:  Caught exception '{0}'.", e.ToString());
				m_Logger.Log(e);
			}

			return(bRet);
		}

		#region CLIFUNCS_OUTOFDATE
		/// <summary>
		/// Called from commandline option.  Do not use anywhere else.
		/// </summary>
		/// <returns></returns>
		private bool CaptureFromSocket()
		{
			bool		bRet = true;
			string		sAddr;
			int			iPort;
			int			iBytesReceived;
			byte[]		baMsg = null;
			int			iMsgsToReceive, ii;
			string		sNumMsgs;

			Socket		sockCaptureAR = null;	// FIX - Only sockets declared as public static are thread safe.
			IPEndPoint	epLocal = null;
			IPEndPoint	epRemote = null;
			EndPoint	epRemoteCasted = null;

			sAddr = "192.168.1.106";		// FIX - Get local addr?  (What if multiple interfaces?)
			iPort = 1780;

			// Clear the array if there are any messages in it already.
			if(m_SockMsgs.Length > 0)
			{
				Array.Clear(m_SockMsgs, 0, m_SockMsgs.Length);
			}

			Console.Write("How many msgs should we receive?  ");
			sNumMsgs = Console.ReadLine();
			iMsgsToReceive = Int32.Parse(sNumMsgs);
			if(iMsgsToReceive <= 0)
			{
				bRet = false;
				Console.WriteLine("Not a valid number of msgs.");
			}
			else
			{
				sockCaptureAR = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				epLocal  = new IPEndPoint(IPAddress.Parse(sAddr), iPort);
				epRemote = new IPEndPoint(IPAddress.Any, 0);
				epRemoteCasted = (EndPoint)epRemote;
				sockCaptureAR.Bind(epLocal);

				baMsg = new Byte[2048];
				m_SockMsgs = new AMSockData[iMsgsToReceive];

				for(ii = 0; ii < iMsgsToReceive; ii++)
				{
					m_SockMsgs[ii] = new AMSockData(ref m_Logger);
				}

				Console.WriteLine("Waiting for data...");

				for(ii = 0; ii < iMsgsToReceive; ii++)
				{
					iBytesReceived = sockCaptureAR.ReceiveFrom(baMsg, baMsg.Length, SocketFlags.None, ref epRemoteCasted);
					m_SockMsgs[ii].Extract(baMsg);
					Console.Write("*");
				}
				Console.WriteLine("");

				sockCaptureAR.Shutdown(SocketShutdown.Both);
				sockCaptureAR.Close();
			}
			return(bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool SaveCapturedAudio()
		{
			bool		bRet = true;
			int			ii;
			string		sDataFName = "AudioEngine_Data.wav";
			FileStream	fsData;

			Console.Write("What filename should we save the audio data to?  ");
			sDataFName = Console.ReadLine();

			fsData = File.Create(sDataFName);	// Without an Exists() check, we'll always overwrite a file with the same name.
			for(ii = 0; ii < m_SockMsgs.Length; ii++)
			{
				fsData.Write(m_SockMsgs[ii].m_abData, 0, m_SockMsgs[ii].m_abData.Length);
			}
			fsData.Close();

			return(bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool LoadCapturedAudio()
		{
			bool		bRet = true;
			string		sDataFName;
			FileInfo	fiData;
			FileStream	fsData;
			long		lBytes;

			Console.Write("What filename should we read the audio data from?  ");
			sDataFName = Console.ReadLine();

			if(!File.Exists(sDataFName))
			{
				Console.WriteLine("Invalid filename entered.");
			}
			else
			{
				fiData = new FileInfo(sDataFName);
				fsData = fiData.OpenRead();
				lBytes = fiData.Length;

				// Clear the array if there are any messages in it already.
				if(m_SockMsgs.Length > 0)
				{
					Array.Clear(m_SockMsgs, 0, m_SockMsgs.Length);
				}

				// Read data from the file.
				m_SockMsgs = new AMSockData[1];
				m_SockMsgs[0] = new AMSockData(ref m_Logger);
				m_SockMsgs[0].m_abData = new byte[lBytes];
				fsData.Read(m_SockMsgs[0].m_abData, 0, (int)lBytes);

				// Fill in the rest of the members
				m_SockMsgs[0].m_iSeqNum = 0;
				m_SockMsgs[0].m_sMsgSrc = "Data file:  '" + sDataFName + "'.";
				m_SockMsgs[0].m_Type = AMSockData.AMDMsgType.eAudioData;
			}

			return(bRet);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool SimNewSession()
		{
			bool		bRet = true;
			string		sTmp;
			int			iVmcKey = -1;
			Type		tRcv;
			ISMessaging.Delivery.ISMReceiverImpl	mRcv;
			ISMessaging.Session.ISMSessionBegin		mSB;
			ISMessaging.ISAppEndpoint				mEPSrc;

			Console.Write("Create a new session on which VMC? > ");
			sTmp = Console.ReadLine();
			iVmcKey = Int32.Parse(sTmp);

			//tRcv = Type.GetType("AudioMgr.AMPinger");	// Kind of a kludge to get a valid type to use in GetObject().
			tRcv = typeof(AudioMgr.AMPinger);
			sTmp = ConfigurationManager.AppSettings[AudioEngine.cs_RemotingDialogMgrUrl];
			sTmp = (sTmp == null) ? "" : sTmp;
			if (sTmp.Length == 0)
			{
				mRcv = (ISMessaging.Delivery.ISMReceiverImpl)(Activator.GetObject(tRcv, "tcp://localhost:1778/DialogMgr.rem"));
			}
			else
			{
				mRcv = (ISMessaging.Delivery.ISMReceiverImpl)(Activator.GetObject(tRcv, sTmp));
			}

			//mRcv.Ping();
			mEPSrc = new ISAppEndpoint();
			mEPSrc.Init(new ISMessaging.ISMVMC(), Environment.MachineName, EApplication.eAudioMgr, "Audio Manager Server");	// FIX - properly set VMC.
			mSB = new ISMessaging.Session.ISMSessionBegin("", "", mEPSrc);

			// FIX: call the InteractionManager|RM to fill in the destination here.

			mRcv.NewMsg(mSB);

			return(bRet);
		}
		#endregion

		private bool ConvertFileMuToPCM16()
		{
			bool		bRet = true;
			string		sSrc, sDest;
			FileInfo	fiSrc;
			FileStream	fsSrc, fsDest;
			int			iSamples;
			byte[]		abSrc, abDest;
			Int16[]		aiDest;
			int			ii;

			Console.WriteLine("Convert a mu-law file to PCM16 file.");
			Console.Write("Source filename:  ");
			sSrc = Console.ReadLine();
			Console.Write("Destination filename:  ");
			sDest = Console.ReadLine();

			if(!File.Exists(sSrc))
			{
				bRet = false;
				Console.Error.WriteLine("Invalid source filename '{0}'.", sSrc);
			}
			else
			{
				fiSrc = new FileInfo(sSrc);
				iSamples = (int)(fiSrc.Length);
				fsSrc = fiSrc.OpenRead();
				fsDest = File.OpenWrite(sDest);

				abSrc = new byte[iSamples];
				aiDest = new Int16[iSamples];
				abDest = new byte[iSamples * 2];

				fsSrc.Read(abSrc, 0, iSamples);
				fsSrc.Close();

				EnergyDetector.MuLawToPCM16(abSrc, iSamples, aiDest);
				abSrc = null;		// Flag for GC cleanup.		// Will the GC ever grab this before the function completes?

				for(ii = 0; ii < iSamples; ii++)
				{
					abDest[2 * ii] = (byte)(aiDest[ii] & 0x00FF);
					abDest[(2 * ii) + 1] = (byte)((aiDest[ii] >> 8) & 0x00FF);
				}

				fsDest.Write(abDest, 0, abDest.Length);

				fsDest.Close();
			}

			return(bRet);
		}

		private bool ReadWAVFileHeader()
		{
			bool		bRet = true, bRes;
			string		sFile;
			FileInfo	fiFile;
			FileStream	fsFile;
			int			iLen;
			byte[]		abFile;
			WavHeader	whFile = null;

			Console.WriteLine("Read a WAV file header.");
			Console.Write("Filename:  ");
			sFile = Console.ReadLine();

			if(!File.Exists(sFile))
			{
				bRet = false;
				Console.Error.WriteLine("Invalid filename '{0}'.", sFile);
			}
			else
			{
				fiFile = new FileInfo(sFile);
				fsFile = fiFile.OpenRead();
				iLen = (int)(fiFile.Length);
				abFile = new byte[iLen];
				fsFile.Read(abFile, 0, iLen);
				fsFile.Close();

				whFile = new WavHeader();
				bRes = whFile.Extract(abFile);
				bRes = whFile.Valid();

				Console.WriteLine("{0}", whFile);	// NOTE - until WavHeader.ToString() is overridden, this will display the type name, not the contents of the header.

				abFile = null;		// Flag for GC cleanup.
			}

			return(bRet);
		}
		
		/// <summary>
		/// FIX - This has a couple of bugs, making the resultant WAV file incorrect without hand-tuning it:
		/// 1.) The serialization includes the text class information,
		/// 2.) The file size comes out incorrect.
		/// </summary>
		/// <returns></returns>
		private bool WriteWAVFileHeader()
		{
			bool				bRet = true, bRes;
			string				sFile;
			FileInfo			fiFile;
			FileStream			fsFile;
			int					iLen;
			byte[]				abFile;
			WavHeader			whFile = null;
			BinaryFormatter		formatter;

			Console.WriteLine("Write a WAV file header.");
			Console.Write("Filename:  ");
			sFile = Console.ReadLine();

			if(!File.Exists(sFile))
			{
				bRet = false;
				Console.Error.WriteLine("Invalid filename '{0}'.", sFile);
			}
			else
			{
				// Read in file
				fiFile = new FileInfo(sFile);
				fsFile = fiFile.OpenRead();
				iLen = (int)(fiFile.Length);
				abFile = new byte[iLen];
				fsFile.Read(abFile, 0, iLen);
				fsFile.Close();

				// Create header
				whFile = new WavHeader();
				bRes = whFile.Set(iLen);
				fsFile = fiFile.OpenWrite();

				// Write out header and binary data
				formatter = new BinaryFormatter();
				formatter.Serialize(fsFile, whFile);
				fsFile.Write(abFile, 0, iLen);

				// Clean up
				fsFile.Close();
				abFile = null;		// Flag for GC cleanup.
			}

			return(bRet);
		} // WriteWAVFileHeader
	} // class AudioEngine_srv
}
