// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Text;

using Incendonet.Utilities.LogClient;

namespace AudioMgr
{
	/// <summary>
	/// Class to encapsulate messages from uaIS, providing means to de-serialize.
	/// FIX - Should this be moved to ISMsgs, or is it strictly internal to AudioEngine?
	/// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	/// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	/// !!																		!!
	/// !! IF THE STRUCTURE OF ANY OF AMSOCKDATA CHANGES, THE MESSAGE CLASSES	!!
	/// !! IN ISDEVICE.CXX WILL HAVE TO BE UPDATED!								!!
	/// !!																		!!
	/// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	/// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	/// </summary>
	public class AMSockData
	{
		private ILegacyLogger		m_Logger = null;

		public static int[]			m_iIndexes = {0, 4, 8, 72, 76};		// FIX - This will not work for future msg definitions.
		private static int			INDEXLASTHDRFIELD = 3;

		// NOTE: The following values are duplicated in ISDevice, make sure they match.
		private static int			SIZEMSGHDR = m_iIndexes[INDEXLASTHDRFIELD];		// FIX - Too easy to make a mistake
		private static int			SIZESRC = 64;                                    // Bytes allocated for "source" string.
		private static int			SIZE1SECAUDIO = 8000;                            // Indicates 64kSamp/sec ==> 1sec of audio == ( (8 bits/sample * 8000 samples/sec) / (8 bits/byte) )
		public static int			SIZEOPPACKET = 256;
		public static int			SIZEAUDIODATA = 160;	                            // See ISDevice NETWORK_RTP_RATE
        public static int			SIZE025SECAUDIO = SIZE1SECAUDIO / 4;	            // 1/4 second of audio

		public static int			SIZETRANSFERADDR = 128;	                        // Size of address to transfer to.

		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		// vvvvvvvvvvvvvvvvv Data in messages send to AudioRtr vvvvvvvvvvvvvvvvv
		public AMDMsgType			m_Type;
		public UInt32				m_iSeqNum;			// Sequence number of msg, so server knows if one has been dropped or received out of order (when using UDP.)
		public string				m_sMsgSrc;			// Sent as a char[SIZESRC].
		// ^^^^^^^^^^^^^^^^^ Data in messages send to AudioRtr ^^^^^^^^^^^^^^^^^
		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

		// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
		// vvvvvvvvvvvvvvvvv Optional data in messages send to AudioRtr vvvvvvvv
		public Int32				m_lDataSize;		// Indicates size of data below.
		public byte[]				m_abData;			// If this is a message other than AudioData, this field may be empty.
		// ^^^^^^^^^^^^^^^^^ Optional data in messages send to AudioRtr ^^^^^^^^
		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

		public ISMessaging.ISMsg	m_Msg;		// If this is an AudioData message, this field may be empty.

		public enum AMDMsgType	                // NOTE:  These values must be kept in sync with AudioMgrMsgs.hxx, ISAudioDevice.cxx.
		{
			// AudioMgr <--> AudioRtr
			eUndefined = 0,
			eAudioData,							// No start/stop for record, since it is a continuous stream.  Playback audio is only sent by AudioMgr when needed.

			// AudioMgr <--  AudioRtr
			eSessionBegin,
			eSessionEnd,
			eDTMF,

			// AudioMgr  --> AudioRtr
			eSpeechStart,
			eSpeechStop,
			eTerminateSession,		            // "Hang up now"
			eTransferSession,
			eShutdown,				            // "Clean up".  For now, only used internal to AudioRtr.  11/23/2007
			eTerminateSessionAfterPrompts,		// "Hang up after prompts complete"
		}

		public AMSockData(ref ILegacyLogger i_Logger)
		{
			m_Logger = i_Logger;
			Clear();
		}

		private void Clear()
		{
			m_Type = AMDMsgType.eUndefined;
			m_iSeqNum = 0;
			m_sMsgSrc = "";
			m_abData = null;
			m_Msg = null;
		}

		/// <summary>
		/// Sets member data from the msg passed in.
		/// </summary>
		/// <param name="i_Msg"></param>
		/// <returns></returns>
		public void Set(ISMessaging.ISMsg i_Msg)
		{
			try
			{
				m_Msg = i_Msg;
				m_iSeqNum = 0;

				if (i_Msg.m_Source != null)
				{
					m_sMsgSrc = i_Msg.m_Source.m_sMachine;
				}
				else
				{
					m_sMsgSrc = "AudioMgr";
				}

				switch (i_Msg.GetType().ToString())
				{
					case "ISMessaging.Audio.ISMSpeechStart":
						m_Type = AMDMsgType.eSpeechStart;
					    break;

					case "ISMessaging.Audio.ISMSpeechStop":
						m_Type = AMDMsgType.eSpeechStop;
					    break;

					case "ISMessaging.Audio.ISMRawData":
						m_Type = AMDMsgType.eAudioData;
						m_abData = ((ISMessaging.Audio.ISMRawData)i_Msg).m_abData;
					    break;

					case "ISMessaging.Session.ISMTransferSession":
						m_Type = AMDMsgType.eTransferSession;
					    break;

					case "ISMessaging.Session.ISMTerminateSession":
						m_Type = AMDMsgType.eTerminateSession;
					    break;

					case "ISMessaging.Session.ISMTerminateSessionAfterPrompts":
						m_Type = AMDMsgType.eTerminateSessionAfterPrompts;
					    break;

					default:
                        m_Type = AMDMsgType.eUndefined;
						Log(Level.Exception, $"ERROR AudioMgr.AMSockData.Set():  Got unknown msg type '{i_Msg.GetType().ToString()}'.");
					    break;
				}
			}
			catch (Exception e)
			{
				Log(Level.Exception, $"ERROR AudioMgr.AMSockData.Set():  Caught exception '{e.ToString()}'.");
			}

			return;
		}

		/// <summary>
		/// Creates an array of bytes for the message, and fills in the header.
		/// </summary>
		/// <param name="o_abData"></param>
		/// <returns></returns>
		public void GetMsgHdr(out byte[] o_abData)
		{
			o_abData = null;

			try
			{
				switch (m_Type)
				{
					case AMSockData.AMDMsgType.eAudioData:
						m_lDataSize = AMSockData.SIZEAUDIODATA;		// FIX - use a larger packet for more optimal network usage.
						break;

					case AMSockData.AMDMsgType.eSpeechStart:
					case AMSockData.AMDMsgType.eSpeechStop:
					case AMSockData.AMDMsgType.eTerminateSession:
						m_lDataSize = 0;
						break;

					case AMSockData.AMDMsgType.eTransferSession:
						m_lDataSize = AMSockData.SIZETRANSFERADDR;
						break;

					default:
						m_lDataSize = 0;
						break;
				}

				int iHdrLen = AMSockData.SIZEOPPACKET;

				if (iHdrLen <= 0)
				{
					Log(Level.Exception, "ERROR AudioMgr.AMSockData.Get():  Array length can't be less than 1.");
				}
				else
				{
                    int ii, jj;
                    
                    o_abData = new byte[iHdrLen];

					byte[] abTmp = BitConverter.GetBytes((UInt32)m_Type);
					Array.Copy(abTmp, 0, o_abData, AMSockData.m_iIndexes[0], abTmp.Length);

					abTmp = BitConverter.GetBytes(m_iSeqNum);
					Array.Copy(abTmp, 0, o_abData, AMSockData.m_iIndexes[1], abTmp.Length);

					int iStrLen = m_sMsgSrc.Length;

					for (ii = 0, jj = AMSockData.m_iIndexes[2]; ( (ii < iStrLen) && (jj < iHdrLen) ); ii++, jj++)
					{
						o_abData[jj] = (byte)m_sMsgSrc[ii];
					}

					if (jj < iHdrLen)
					{
						o_abData[jj] = 0;
					}
					else
					{
						o_abData[iHdrLen - 1] = 0;
					}
				}
			}
			catch (Exception e)
			{
				Log(Level.Exception, $"ERROR AudioMgr.AMSockData.Get():  Caught exception '{e.ToString()}'.");
			}

			return;
		}

		/// <summary>
		/// Extracts member data from the buffer passed in.
		/// </summary>
		/// <param name="i_Data"></param>
		/// <returns></returns>
		public void Extract(byte[] i_abData)
		{
			try
			{
				m_Type = (AMDMsgType)BitConverter.ToInt32(i_abData, m_iIndexes[0]);
				m_iSeqNum = BitConverter.ToUInt32(i_abData, m_iIndexes[1]);
				m_sMsgSrc = Encoding.UTF8.GetString(i_abData, m_iIndexes[2], SIZESRC);

				switch (m_Type)
				{
					case AMDMsgType.eAudioData:
						m_Msg = new ISMessaging.Audio.ISMRawData();

						int iDataLen = BitConverter.ToInt32(i_abData, m_iIndexes[3]);
						m_abData = new byte[iDataLen];
						Array.Copy(i_abData, m_iIndexes[4], m_abData, 0, iDataLen);
    					break;

					case AMDMsgType.eSessionBegin:
					    string sFrom = "";
                        string sTo = "";

						try
						{
							sFrom = Encoding.UTF8.GetString(i_abData, m_iIndexes[3], 64).Trim('\0');
							sTo = Encoding.UTF8.GetString(i_abData, m_iIndexes[3] + 64, 64).Trim('\0');
						}
						catch (Exception exc)
						{
							sFrom = sTo = "";
							Log(Level.Exception, $"AMSockData.Extract(): Caught exception extracting From/To: '{exc.ToString()}'");
						}

						m_Msg = new ISMessaging.Session.ISMSessionBegin(sFrom, sTo);

						// FIX - Should convert C time recorded by UA to C# DateTime.  This will be more important with
						// distributed/redundand configurations.
						((ISMessaging.Session.ISMSessionBegin)m_Msg).m_tBegan = DateTime.Now;	// FIX - see above.
    					break;

					case AMDMsgType.eSessionEnd:
						m_Msg = new ISMessaging.Session.ISMSessionEnd();

						// FIX - Should convert C time recorded by UA to C# DateTime.  This will be more important with
						// distributed/redundand configurations.
						((ISMessaging.Session.ISMSessionEnd)m_Msg).m_tEnded = DateTime.Now;	// FIX - see above.
    					break;

					case AMDMsgType.eDTMF:
						m_Msg = new ISMessaging.Audio.ISMDtmf();
						((ISMessaging.Audio.ISMDtmf)m_Msg).m_sDtmf = Encoding.UTF8.GetString(i_abData, m_iIndexes[3], 1);
    					break;

					default:
                        m_Msg = null;

						Log(Level.Exception, $"AMSockData.Extract() got unknown type '{m_Type.ToString()}'.");
					break;
				}
			}
			catch (Exception e)
			{
				Log(Level.Exception, $"AMSockData.Extract(): Caught exception: '{e.ToString()}'");
			}

			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Level"></param>
		/// <param name="i_sLogStr"></param>
		/// <returns></returns>
		public void Log(Level i_Level, string i_sLogStr)
		{
			if (m_Logger != null)
			{
				m_Logger.Log(i_Level, i_sLogStr);
			}
			else
			{
				Console.Error.WriteLine($"[{DateTime.Now}] AMSockData: {i_sLogStr}");
			}
		}

	} // class AMSockData
}
