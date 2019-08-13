// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using SBResourceMgr;

namespace AudioMgr
{
	/// <summary>
	/// Creates the thread that captures audio from the AudioRouter and forwards it on.
	/// </summary>
	class ARMsgListenerThread
	{
			private int						m_iThreadIndex = -1;
			private AMSockConns.AMSockConn	m_SockConn = null;
			private	MsgQueue[]				m_aqAudioIn = null;
			private	AudioOutMsgQueue[]		m_aqAudioOut = null;
			private IResourceMgr			m_RM = null;
			private string					m_sLastLog = "";
			private StringCollection		m_asSessionIds = null;			// FIX - This should be handled by the resource manager, not here.

			protected ILegacyLogger m_Logger = null;

			public ARMsgListenerThread(ILegacyLogger i_Logger, IResourceMgr i_RM, int i_iThreadIndex, AMSockConns.AMSockConn i_SockConn, MsgQueue[] i_aqAudioIn, AudioOutMsgQueue[] i_aqAudioOut)
			{
				m_Logger = i_Logger;
				m_iThreadIndex = i_iThreadIndex;
				m_SockConn = i_SockConn;
				m_RM = i_RM;
				m_aqAudioIn = i_aqAudioIn;
				m_aqAudioOut = i_aqAudioOut;
				m_asSessionIds = new StringCollection();
			}

			public bool Log(Level i_Level, string i_sLogStr)
			{
				bool		bRet = true;

				try
				{
					if(i_sLogStr != m_sLastLog)	// This reference compare doesn't seem to work on Mono, there must be a problem with their hashtable.
					//if(i_sLogStr.CompareTo(m_sLastLog) != 0)	// This is a more expensive operation...
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
					Console.Error.WriteLine(DateTime.Now.ToString() + " ARMsgListenerThread.Log exception: " + exc.ToString());
				}

				return(bRet);
			}

			/// <summary>
			/// 
			/// </summary>
			public void ThreadProc()
			{
				bool							bCont = true;
				int								iSockBytes = 0;
				int								iMsgBytes = 0;
				int								iArrayIndex = 0;
				int								iVmcIndex = -1;
				byte[]							baMsg;				// Buffer received from socket.
				AMSockData						amsData;			// Message extraced from socket buffer.
				byte[]							baAudio;			// Audio extracted from socket msg.
				ISMessaging.ISAppEndpoint		mSrc;				// Indicates where msg came from.
				ISMessaging.ISMVMC				mVMC;				// VMC
				string							sSessionId = "";
				string							sMsgType = "";

				m_Logger.Init("", "", Thread.CurrentThread.Name, "", "", "");

				Log(Level.Verbose, "[TI:" + m_iThreadIndex.ToString() + "]" + "ARMsgListenerThread started.");

				// Initialize session-id collection - Should be the same size as the number of VMCs.  // FIX - This may be more than this particular system handles.
				for(int ii = 0; ii < m_aqAudioIn.Length; ii++)
				{
					m_asSessionIds.Add("");
				}

				m_SockConn.m_sockARRead = m_SockConn.m_listenARRead.AcceptSocket();
				if(m_SockConn.m_sockARRead != null)
				{

					baMsg = new Byte[AMSockData.SIZEOPPACKET];


                    // Make sure that baAudio size is such as to hold an integer number of audio data packets that collectively are at least 250 ms worth of audio.

                    baAudio = new Byte[AMSockData.SIZE025SECAUDIO + (AMSockData.SIZE025SECAUDIO % AMSockData.SIZEAUDIODATA)];
					amsData = new AMSockData(ref m_Logger);
					iArrayIndex = 0;

					ISMessaging.Audio.ISMRawData.MuClear(baAudio);

					while(bCont)	// FIX - use failure counter (to prevent infinite looping)
					{
						try
						{
							// Receive data from socket and extract the message.
							iSockBytes = m_SockConn.m_sockARRead.Receive(baMsg, 0, baMsg.Length, SocketFlags.None);

							if(iSockBytes > 0)
							{
								amsData.Extract(baMsg);
								if(baMsg == null)
								{
									Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "] ARMLT Extract returned null message!");
								}
								else
								{
									// If this is a new session coming in, make sure to grab a new VMC, otherwise reuse the one already open.
									sMsgType = amsData.m_Msg.GetType().ToString();
									if(sMsgType == "ISMessaging.Session.ISMSessionBegin")
									{
										iVmcIndex = m_RM.AddSession(m_iThreadIndex, amsData.m_sMsgSrc);
										//iVmcIndex = m_RM.AddSession(m_iThreadIndex, amsData.m_sMsgSrc, 4, 250);
										if (iVmcIndex == -1)
										{
											sSessionId = "";
											// The error will get logged after the check below
										}
										else
										{
											sSessionId = m_RM.GetSessionId(iVmcIndex);
										}
									}
									else
									{
										sSessionId = m_RM.GetSessionId(iVmcIndex);
										if( (sSessionId == null) || (sSessionId.Length == 0) )
										{
											iVmcIndex = -1;
										}
										else
										{
											mVMC = m_RM.GetVMCBySessionid(sSessionId);
											iVmcIndex = mVMC.m_iKey;
										}
									}

									if(iVmcIndex == -1)
									{
										if(sMsgType != "ISMessaging.Audio.ISMRawData")
										{
											Log(Level.Exception, string.Format("[TI:{0}] ARMLT got invalid VMC index: {1} on msg: '{2}'.", iVmcIndex.ToString(), iVmcIndex.ToString(), sMsgType));
										}
									}
									else
									{
										m_SockConn.m_VMC = m_RM.GetVMCByKey(iVmcIndex);
										if(m_SockConn.m_VMC == null)
										{
											Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "] ARMLT.ThreadProc() returned VMC was null on VMC index: " + iVmcIndex.ToString() + "on msg: " + amsData.m_Msg.GetType().ToString());
										}
										else if(m_SockConn.m_VMC.m_iKey != iVmcIndex)
										{
											Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "] ARMLT.ThreadProc() returned VMC key: " + m_SockConn.m_VMC.m_iKey + " didn't match VMC index: " + iVmcIndex.ToString() + "on msg: " + amsData.m_Msg.GetType().ToString());
										}
										else
										{
											switch(sMsgType)
											{
												case "ISMessaging.Audio.ISMRawData" :
												{
													// Grab the session id
													amsData.m_Msg.m_sSessionId = m_asSessionIds[m_iThreadIndex];		// Should also be the same as sSessionId

													// Copy audio data from message, if the 1sec buffer is full, forward it on.
													iMsgBytes = amsData.m_abData.Length;


                                                    // Do we have enough audio data to pass it to AudioInThread?  If not then just save it until we do.

                                                    if ((iArrayIndex + iMsgBytes) <= baAudio.Length)
													{
														Array.Copy(amsData.m_abData, 0, baAudio, iArrayIndex, iMsgBytes);
														iArrayIndex += iMsgBytes;
													}
													else
													{
                                                        // Forward audio data
														mSrc = new ISAppEndpoint();
														mVMC = new ISMVMC();
														mVMC.Init(iVmcIndex, m_SockConn.m_VMC.m_sDescription, m_SockConn.m_VMC.m_sSessionId);
														mSrc.Init(mVMC, amsData.m_sMsgSrc, EApplication.eAudioMgr, "Raw data");

														((ISMessaging.Audio.ISMRawData)(amsData.m_Msg)).Init(mSrc, null, ISMessaging.Audio.SOUND_FORMAT.ULAW_8KHZ, baAudio, iArrayIndex);		// NOTE - i_Dest is currently null, we're assuming that it isn't used.
														m_aqAudioIn[iVmcIndex].Push(amsData.m_Msg);		// FIX - get array index from message.

														// Reset index, clear data
														iArrayIndex = 0;
														ISMessaging.Audio.ISMRawData.MuClear(baAudio);


                                                        // Save the audio data that triggered the sending of audio data to AudioInThread but wasn't included in the actual message sent.

                                                        Array.Copy(amsData.m_abData, 0, baAudio, iArrayIndex, iMsgBytes);
                                                        iArrayIndex += iMsgBytes;
													}
												}
												break;
												case "ISMessaging.Audio.ISMDtmf" :
												{
													// Grab the session id
													amsData.m_Msg.m_sSessionId = m_asSessionIds[m_iThreadIndex];		// Should also be the same as sSessionId

													m_aqAudioIn[iVmcIndex].Push(amsData.m_Msg);
												}
												break;
												case "ISMessaging.Session.ISMSessionBegin" :
												{
													// Store the session-id in the local array and the message
													m_asSessionIds[m_iThreadIndex] = sSessionId;
													amsData.m_Msg.m_sSessionId = sSessionId;

													// Set IpAddress, SessionId in Logger so all subsequent calls to Log() will be identified.  (Need-to/should do this in every thread once the ISMSessionBegin comes in.)
													m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.IpAddress.ToString(), Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(oAddr => oAddr.AddressFamily == AddressFamily.InterNetwork).ToString() ?? "");
													m_Logger.UpdateValue(Thread.CurrentThread.Name, LoggerCore.eRequiredParams.SessionId.ToString(), sSessionId);
													m_Logger.Log(Level.Info, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] <<<ARMLT ISMessaging.Session.ISMSessionBegin(F:'" + ((ISMessaging.Session.ISMSessionBegin)amsData.m_Msg).m_sFromAddr + "', T:'" + ((ISMessaging.Session.ISMSessionBegin)amsData.m_Msg).m_sToAddr + "')>>>");

//													m_aqAudioOut[iVmcIndex].Clear();	// FIX: Is this clear causing messages we want to receive to be deleted?  Or is it helping us by clearing out residual messages that are meaningless to the new session?
													m_aqAudioOut[iVmcIndex].Push(amsData.m_Msg);	// Push to qAudioOut first, so that socket handling gets done first
													//m_aqAudioIn[iVmcIndex].Clear();				// Moved this to Begin handler in AudioOut to ensure proper delivery order.
													//m_aqAudioIn[iVmcIndex].Push((ISMessaging.ISMsg)((ISMessaging.Session.ISMSessionBegin)(amsData.m_Msg)).Clone());	// Clone, just in case Mono is not properly ref-counting.  If it isn't, will this cause a leak?
												}
												break;
												case "ISMessaging.Session.ISMSessionEnd" :
												{
													// Grab the session id
													amsData.m_Msg.m_sSessionId = m_asSessionIds[m_iThreadIndex];		// Should also be the same as sSessionId

													// FIX - What happens if there is a socket exception caught in AOT?  It will forward an ISMSessionEnd to AIT, but we'll never get here and clear baAudio.
													m_Logger.Log(Level.Info, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] <<<ARMLT ISMessaging.Session.ISMSessionEnd>>>");

													m_aqAudioOut[iVmcIndex].Push(amsData.m_Msg);

													// Moved the Push to audio-in thread below to AudioOut's End to ensure no overlap
													//m_aqAudioIn[iVmcIndex].Push((ISMessaging.ISMsg)((ISMessaging.Session.ISMSessionEnd)(amsData.m_Msg)).Clone());	// Pass 'end' on to AudioOut so it can reset socket.	// FIX - get array index from message.  SEE CLONE COMMENT ABOVE

													// Reset index, clear data
													iArrayIndex = 0;
													ISMessaging.Audio.ISMRawData.MuClear(baAudio);
												}
												break;
												default :
												{
													//Console.Error.WriteLine("AudioMgr.AudioEngine_srv.ARMsgListenerThread() Got unknown message from AudioRouter: '{0}'.", amsData.m_Msg.GetType().ToString());
													Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT Got unknown message from AudioRouter: " + amsData.m_Msg.GetType().ToString());
												}
												break;
											} // switch
										} // if(!iVmcIndex)

										//Console.Write("*");
									} // else vmcindex != -1
								} // else msg != null
							} // if
							else
							{
								// Receive returned 0 bytes, socket has probably been closed down by other side.
								// Close socket and wait for new connection.
								m_SockConn.m_VMC = null;
								//m_RM.ReleaseSession(iVmcIndex);	// Fix - Here or ProcessSessionEnd()?

								try
								{
									if(m_SockConn.m_sockARRead.Connected)
									{
										m_SockConn.m_sockARRead.Shutdown(SocketShutdown.Both);
									}
								}
								catch(SocketException exc2)
								{
									Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT caught exception when calling Shutdown: " + exc2.ErrorCode.ToString());
								}
								try
								{
									m_SockConn.m_sockARRead.Close();
								}
								catch(SocketException exc2)
								{
									Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT caught exception when calling Close: " + exc2.ErrorCode.ToString());
								}
								m_SockConn.m_sockARRead = m_SockConn.m_listenARRead.AcceptSocket();
							}
						}
						catch(SocketException exc)
						{
							// Most likely, the AudioRtr dropped out.
							// Close socket and wait for new connection.
							m_SockConn.m_VMC = null;
							//m_RM.ReleaseSession(iVmcIndex);	// Fix - Here or ProcessSessionEnd()?

							try
							{
								// WSAENOTSOCK == 10038, WSAEINTR == 10004
								if(exc.ErrorCode == 10038)
								{
									// Do nothing here, the socket will get nulled below.
									Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: socket NOTSOCK.");
								}
								else if(exc.ErrorCode == 10004)
								{
									// Do nothing here, the socket will get nulled below.
									Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: socket INTR.");
								}
								else
								{
									Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: " + exc.ToString());

									if(m_SockConn == null)
									{
										Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: m_SockConn was null!!! 1");
									}
									else if(m_SockConn.m_sockARRead == null)
									{
										Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: m_SockConn.m_sockARRead was null.");
									}
									else if(!m_SockConn.m_sockARRead.Connected)
									{
										Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: Socket was not connected.");
									}
									else
									{
										if(m_SockConn.m_sockARRead.Connected)
										{
											Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: socket.Shutdown...");
											m_SockConn.m_sockARRead.Shutdown(SocketShutdown.Both);
										}

										Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: socket.Close...");
										m_SockConn.m_sockARRead.Close();

										Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: socket.Closed.");
									}
								}
							}
							catch(SocketException exc2)
							{
								Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "] ARMLT socket error in cleanup: " + exc2.ErrorCode.ToString());
							}
							catch(Exception exc2)
							{
								Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "] ARMLT Exception in cleanup: " + exc2.ToString());
							}

							if(m_SockConn == null)
							{
								Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: m_SockConn was null!!!  Nothing more we can do, exiting...");
								break;
							}
							else
							{
								Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: socket.AcceptSocket...");
								m_SockConn.m_sockARRead = m_SockConn.m_listenARRead.AcceptSocket();
							}

							Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: Socket exception handling complete.");
						}
						catch(Exception exc)
						{
							Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: " + exc.ToString());
						}
					} // while
				}

				if(m_SockConn == null)
				{
					Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: m_SockConn was null!!! 3");
				}
				else
				{
					if(m_SockConn.m_sockARRead != null)
					{
						m_SockConn.m_sockARRead.Shutdown(SocketShutdown.Both);
						m_SockConn.m_sockARRead.Close();
					}
					if(m_SockConn.m_listenARRead != null)
					{
						m_SockConn.m_listenARRead.Stop();
					}
				}

				Log(Level.Exception, "[TI:" + m_iThreadIndex.ToString() + "][VMC:" + iVmcIndex + "] ARMLT: Exiting thread now.");
			} // ThreadProc()
		} // ARMsgListenerThread()
}
