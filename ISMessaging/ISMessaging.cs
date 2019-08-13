// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;

// FIX - In members where the constructor of an object to be Clone()ed includes an endpoint, how do you
// make sure the endpoint is cloned, since Clone() has no params???

namespace ISMessaging
{
	public enum Gender
	{
		male,
		Female,
	}

	public class Utilities
	{
		public string CopyString(string i_sIn)
		{
			return(String.Copy(i_sIn));
		}

		public static bool GetItemsFromString(string i_sLine, char i_cSeparator, StringCollection o_asFields)
		{
			bool			bRet = true, bDone;
			int				iPrevIndex = 0, iNextIndex = 0, iLen;
			string			sTmp = "";

			try
			{
				iLen = i_sLine.Length;
				o_asFields.Clear();
				bDone = false;

				while(!bDone)
				{
					sTmp = "";
					iNextIndex = i_sLine.IndexOf(i_cSeparator, iPrevIndex);
					if( (iNextIndex == -1) || (iNextIndex == (iLen - 1)) || (iNextIndex == i_sLine.Length - 1) )
					{
						bDone = true;
						if( (iNextIndex - iPrevIndex) > 0)
						{
							sTmp = i_sLine.Substring(iPrevIndex, iNextIndex - iPrevIndex);
						}
						else if( (iNextIndex == -1) && ((iPrevIndex + 1) < iLen) )
						{
							sTmp = i_sLine.Substring(iPrevIndex, iLen - iPrevIndex);
						}
					}
					else
					{
						sTmp = i_sLine.Substring(iPrevIndex, iNextIndex - iPrevIndex);
						iPrevIndex = iNextIndex + 1;
					}

					if(sTmp.Length > 0)
					{
						o_asFields.Add(sTmp);
					}
				}
			}
			catch
			{
				bRet = false;
			}
			return(bRet);
		} // GetItemsFromString()

		/// <summary>
		/// Returns a copy of the string that is upper-cased and with spaces between each letter.  Handy when TTSing acronyms.
		/// </summary>
		/// <param name="i_sIn"></param>
		/// <returns></returns>
		public string SpacifyString(string i_sIn)
		{
			string			sRet = "";
			char[]			acIn = null;
			int				ii = 0, iLen = 0;

			try
			{
				if(i_sIn == null)
				{
					sRet = "";
				}
				else if(i_sIn.Length <= 0)
				{
					sRet = "";
				}
				else
				{
					acIn = i_sIn.ToUpper().ToCharArray();
					iLen = acIn.Length;
					for(ii = 0; ii < iLen; ii++)
					{
						sRet += acIn[ii];
						sRet += ' ';
					}

				}
			}
			catch(Exception exc)
			{
				sRet = "";
				Console.Error.WriteLine(DateTime.Now.ToString() + " SpacifyString exception: " + exc.ToString());
			}

			return(sRet);
		} // SpacifyString

		/// <summary>
		/// Returns a copy of the string with spaces between each letter and commas for pauses.  Handy when TTSing phone numbers.
		/// </summary>
		/// <param name="i_sIn"></param>
		/// <returns></returns>
		public string SpacifyPhonenum(string i_sIn)
		{
			string			sRet = "";
			char[]			acIn = null;
			int				ii = 0, iLen = 0;

			try
			{
				if(i_sIn == null)
				{
					sRet = "";
				}
				else if(i_sIn.Length <= 0)
				{
					sRet = "";
				}
				else
				{

					acIn = i_sIn.ToUpper().ToCharArray();
					iLen = acIn.Length;

					// FIX - What about international and non 7|10 conventions?
					for(ii = 0; ii < iLen; ii++)
					{
						sRet += acIn[ii];

						if( (iLen == 7) || (iLen == 10) )
						{
							if(ii == 2)
							{
								sRet += ',';
							}

							if( (iLen == 10) && (ii == 5) )
							{
								sRet += ',';
							}
						}

						sRet += ' ';
					}

				}
			}
			catch(Exception exc)
			{
				sRet = "";
				Console.Error.WriteLine(DateTime.Now.ToString() + " SpacifyPhonenum exception: " + exc.ToString());
			}

			return(sRet);
		} // SpacifyPhonenum

		/// <summary>
		/// Returns a string in the format:  YYYYMMDD:HHMMSS.mmm
		/// </summary>
		/// <param name="i_dtStamp"></param>
		/// <returns></returns>
		public static string GetNumericDateTime(DateTime i_dtStamp)
		{
			string				sRet = "";
			StringBuilder		sbTmp = null;

			try
			{
				sbTmp = new StringBuilder();

				sbTmp.AppendFormat("{0}{1}{2}:{3}{4}{5}.{6}", i_dtStamp.Year.ToString("D4"), i_dtStamp.Month.ToString("D2"), i_dtStamp.Day.ToString("D2"), i_dtStamp.Hour.ToString("D2"), i_dtStamp.Minute.ToString("D2"), i_dtStamp.Second.ToString("D2"), i_dtStamp.Millisecond.ToString("D3"));

				sRet = sbTmp.ToString();
			}
			catch(Exception exc)
			{
				sRet = "";
				Console.Error.WriteLine(DateTime.Now.ToString() + " GetNumericDateTime exception: " + exc.ToString());
			}

			return(sRet);
		} // GetNumericDateTime

	} // Utilities

	/// <summary>
	/// Used to queue messages between threads.
	/// </summary>
	public class MsgQueue
	{
		// NOTE:  Wouldn't the implementation for this class have been more straightforward with Mutex.TryEnter() ?
		protected	AutoResetEvent		m_eReady;
		protected	ArrayList			m_alMsgs;
		protected	int					m_iIndex;		// The parent's thread index, which is passed in.
		private		readonly object		m_Lock = new object();

		public MsgQueue(int i_iIndex)
		{
			m_eReady = new AutoResetEvent(false);
			m_alMsgs = new ArrayList();
			m_iIndex = i_iIndex;
		}

		public bool Push(ISMsg i_Msg)		// FIX - parameter as ref?
		{
			bool	bRet = true;

			try
			{
				Monitor.Enter(m_Lock);

				m_alMsgs.Add(i_Msg);

				m_eReady.Set();
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Push() - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e);
				bRet = false;
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(bRet);
		} // Push

		public ISMsg Pop()
		{
			ISMsg	mTmp = null;
			bool	bRes = true;

			try
			{
				bRes = m_eReady.WaitOne();
				Monitor.Enter(m_Lock);

				if(bRes == false)
				{
					Console.Error.WriteLine("[{0}][{1}]ERROR - ISMessaging.MsgQueue.Pop() WaitOne returned false!  Stack: {2}", DateTime.Now.ToString(), m_iIndex, System.Environment.StackTrace);
				}

				if(m_alMsgs.Count <= 0)
				{
					Console.Error.WriteLine("[{0}][{1}]ERROR - ISMessaging.MsgQueue.Pop() no elements!  Stack: {2}", DateTime.Now.ToString(), m_iIndex, System.Environment.StackTrace);
				}
				else
				{
					mTmp = (ISMsg)(m_alMsgs[0]);		// Reference assigned to ISMsg in ArrayList.
					m_alMsgs.RemoveAt(0);	// Remove from array (but not deleted due to reference assignment above.)

					// Check if there are any more elements
					if(m_alMsgs.Count > 0)
					{
						m_eReady.Set();
					}
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Pop() - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e);
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(mTmp);
		} // Pop

		public ISMsg Pop(int i_iTimeout)	// Timeout in mSec.
		{
			ISMsg	mTmp = null;

			try
			{
				if(!m_eReady.WaitOne(i_iTimeout, false))
				{
					// No message waiting in timeout period.
				}
				else
				{
					Monitor.Enter(m_Lock);

					try
					{
						if(m_alMsgs.Count <= 0)
						{
							Console.Error.WriteLine("[{0}][{1}]ERROR - ISMessaging.MsgQueue.PopT() no elements!  Stack: {2}", DateTime.Now.ToString(), m_iIndex, System.Environment.StackTrace);
						}
						else
						{
							mTmp = (ISMsg)(m_alMsgs[0]);		// Reference assigned to ISMsg in ArrayList.
							m_alMsgs.RemoveAt(0);	// Remove from array (but not deleted due to reference assignment above.)

							// Check if there are any more elements
							if(m_alMsgs.Count > 0)
							{
								m_eReady.Set();
							}
						}
					}
					catch(Exception e)
					{
						Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.PopT1() - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e);
					}
					finally
					{
						Monitor.Exit(m_Lock);
					}
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.PopT2() - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e);
			}

			return(mTmp);
		} // Pop

		public ISMsg Peek()
		{
			ISMsg	mRet = null;

			try
			{
				Monitor.Enter(m_Lock);

				if(m_alMsgs.Count > 0)
				{
					mRet = (ISMsg)(m_alMsgs[0]);		// Reference assigned to ISMsg in ArrayList.
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Peek() - Caught exception '{2}'!", DateTime.Now.ToString(), e);
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(mRet);
		} // Peek

		public ISMsg Find(string i_sType)
		{
			ISMsg	mRet = null;
			int		ii = 0, iLen = 0;

			try
			{
				Monitor.Enter(m_Lock);

				// From which end of queue would be most effective to start?
				iLen = m_alMsgs.Count;
				ii = 0;
				while( (ii < iLen) && (mRet == null) )
				{
					if(m_alMsgs[ii].GetType().ToString() == i_sType)
					{
						mRet = (ISMsg)(m_alMsgs[ii]);
					}
					ii++;
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Find(System.Type) - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e);
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(mRet);
		} // Find

		public ISMsg Find(StringCollection i_asTypes)
		{
			ISMsg	mRet = null;
			int		ii = 0, iLen = 0, jj = 0;

			try
			{
				Monitor.Enter(m_Lock);

				// From which end of queue would be most effective to start?
				iLen = m_alMsgs.Count;
				ii = 0;
				while( (ii < iLen) && (mRet == null) )
				{
					jj = 0;
					while( (jj < i_asTypes.Count) && (mRet == null) )
					{
						if(m_alMsgs[ii].GetType().ToString() == i_asTypes[jj])
						{
							mRet = (ISMsg)(m_alMsgs[ii]);
						}
						jj++;
					}
					ii++;
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Find(System.Type) - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e);
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(mRet);
		} // Find

		/// <summary>
		/// Be very careful when using Clear(), as it could remove messages you may actually want.
		/// </summary>
		public void Clear()
		{
			ISMsg mTmp = null;

			try
			{
				Monitor.Enter(m_Lock);

				while (m_alMsgs.Count > 0)
				{
					mTmp = (ISMsg)(m_alMsgs[0]);		// Reference assigned to ISMsg in ArrayList.
					m_alMsgs.RemoveAt(0);	// Remove from array (but not deleted due to reference assignment above.)
					mTmp = null;
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Clear() - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}
		} // Clear

		/// <summary>
		/// Be very careful when using ClearUpTo(), as it could remove messages you may actually want.  Also, because
		/// it requires a full message to be created and added to the list, its performance may not be great.
		/// </summary>
		/// <param name="i_aStopAtMsg">A list of messages, any of which stop the clear operation.</param>
		public void ClearUpTo(List<ISMsg> i_aStopAtMsg)
		{
			ISMsg			mTmp = null;
			bool			bFound = false;
			int				ii = 0;

			try
			{
				Monitor.Enter(m_Lock);

				while( (m_alMsgs.Count > 0) && (!bFound) )
				{
					bFound = false;
					mTmp = (ISMsg)(m_alMsgs[0]);		// Reference assigned to ISMsg in ArrayList.
					if(i_aStopAtMsg != null)
					{
						for(ii = 0; ((!bFound) && (ii < i_aStopAtMsg.Count)); ii++)
						{
							if(i_aStopAtMsg[ii].GetType() == mTmp.GetType())
							{
								bFound = true;
							}
						}
					}

					if(!bFound)
					{
						m_alMsgs.RemoveAt(0);	// Remove from array (but not deleted due to reference assignment above.)
						mTmp = null;
					}
				}

				// If we found one of the message types specified in the list, we'll want to set the event so a Pop gets triggered
				if(bFound)
				{
					m_eReady.Set();
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("[{0}][{1}]ISMessaging.MsgQueue.Clear() - Caught exception '{2}'!", DateTime.Now.ToString(), m_iIndex, e.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}
		} // Clear

		public string GetDump()
		{
			int				ii = 0, iLen = 0;
			string			sTmp = "", sType = "";
			StringBuilder	sbTmp = null;

			try
			{
				sbTmp = new StringBuilder();

				Monitor.Enter(m_Lock);

				iLen = m_alMsgs.Count;
				for(ii = 0; ii < iLen; ii++)
				{
					sType = m_alMsgs[ii].GetType().ToString();

					sbTmp.AppendFormat("Msg #{0}, type '{1}.  ", ii.ToString(), sType);

					if(sType == "ISMessaging.Audio.ISMPlayPrompts")		// FIX - This won't work when obfuscated.
					{
						sTmp = ((ISMessaging.Audio.ISMPlayPrompts)(m_alMsgs[ii])).m_Prompts[0].m_sText;
						if( (sTmp != null) && (sTmp.Length > 0) )
						{
							sbTmp.AppendFormat("1'st prompt: '{0}'.", sTmp);
						}
						else
						{
							sTmp = ((ISMessaging.Audio.ISMPlayPrompts)(m_alMsgs[ii])).m_Prompts[0].m_sPath;
							if( (sTmp != null) && (sTmp.Length > 0) )
							{
								sbTmp.AppendFormat("1'st prompt: '{0}'.", sTmp);
							}
						}
					}

					sbTmp.Append(Environment.NewLine);
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("ISMessaging.MsgQueue.DumpToConsole() - Caught exception '{0}'!", e);
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(sbTmp.ToString());
		} // GetDump
	} // MsgQueue

	/// <summary>
	/// VirtualMediaChannel
	/// 
	/// A note on current m_sDescription usage.
	/// The AudioMgr uses the description received from the AudioRouter to determine which VMC
	/// the message came from, and assigns the m_iKey accordingly.  The description in this
	/// case is the SIP address.  When the DialogEngine (or other components) assign its value,
	/// it may have another meaning, such as the machine name or component name.  We should
	/// decide on conventions for m_sDescription, or add more fields to the ISMVMC.
	/// </summary>
	[Serializable()]
	public class ISMVMC	: ICloneable// Should this be a reference or value (struct) type?
	{
		public const string				m_csUnallocatedVmcTag = "(none)";

		public int						m_iKey = -1;
		public string					m_sDescription = m_csUnallocatedVmcTag;
		public string					m_sSessionId = m_csUnallocatedVmcTag;

		/// <summary>
		/// For when you know the values.
		/// </summary>
		/// <param name="i_iKey"></param>
		/// <param name="i_sDescription"></param>
		public void Init(int i_iKey, string i_sDescription, string i_sSessionId)
		{
			m_iKey = i_iKey;
			m_sDescription = i_sDescription;
			m_sSessionId = i_sSessionId;
		}

		public void Clear()
		{
			/*
			m_iKey = -1;
			m_sDescription = "";
			m_sSessionId = "";
			*/
			Init(-1, m_csUnallocatedVmcTag, m_csUnallocatedVmcTag);
		}

		/// <summary>
		/// Create a new VMC, coordinating with central source to guarantee uniqueness.
		/// </summary>
		/// <returns></returns>
		public bool AllocNew()
		{
			bool	bRet = true;
// FIX - get values!
			return(bRet);
		}

		#region ICloneable Members

		public object Clone()
		{
			ISMVMC	vmcClone = new ISMVMC();

			vmcClone.m_iKey = this.m_iKey;
			vmcClone.m_sDescription = (string)this.m_sDescription.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.
			vmcClone.m_sSessionId = (string)this.m_sSessionId.Clone();

			return(vmcClone);
		}

		#endregion
	}

	/// <summary>
	/// Used to describe the audio source/dest.
	/// </summary>
	[Serializable()]
	public class ISAudioEndpoint : ICloneable
	{
		public string		m_sSignallingProtocol = "";		// SHOULD BE ENUM TYPE
		public string		m_sStreamingProtocol = "";		// SHOULD BE ENUM TYPE
		public string		m_sPlatform = "";
		public string		m_sSignallingVersion = "";
		public string		m_sStreamingVersion = "";
		public string		m_sPlatformVersion = "";
		public string		m_sDisplayName = "";			// For ex., SIP display name
		public string		m_sSignallingAddr = "";			// For ex., SIP address:  1234@sip.com:5060
		public string		m_sStreamSendAddr = "";			// Fox ex., RTP:  10000
		public string		m_sStreamRecvAddr = "";			// Fox ex., RTP:  10001


		#region ICloneable Members
		public object Clone()
		{
			ISAudioEndpoint		aeClone = new ISAudioEndpoint();

			aeClone.m_sSignallingProtocol = (string)this.m_sSignallingProtocol.Clone();
			aeClone.m_sStreamingProtocol = (string)this.m_sStreamingProtocol.Clone();
			aeClone.m_sPlatform = (string)this.m_sPlatform.Clone();
			aeClone.m_sSignallingVersion = (string)this.m_sSignallingVersion.Clone();
			aeClone.m_sStreamingVersion = (string)this.m_sStreamingVersion.Clone();
			aeClone.m_sPlatformVersion = (string)this.m_sPlatformVersion.Clone();
			aeClone.m_sDisplayName = (string)this.m_sDisplayName.Clone();
			aeClone.m_sSignallingAddr = (string)this.m_sSignallingAddr.Clone();
			aeClone.m_sStreamSendAddr = (string)this.m_sStreamSendAddr.Clone();
			aeClone.m_sStreamRecvAddr = (string)this.m_sStreamRecvAddr.Clone();

			return(aeClone);
		}
		#endregion
	}


	public enum EApplication
	{
		eUndefined,
		eInteractionMgr,
		eDialogMgr,
		eAudioMgr,
		eSpeechMgr,
	}

/// <summary>
/// Used for source and destination.
/// 
/// Note:  Destination can be overriden when an object has subscribed to an event
/// delivering the message.
/// </summary>
	[Serializable()]
	public class ISAppEndpoint : ICloneable
	{
		public ISMVMC					m_VMC;
		public string					m_sMachine = "";
		public EApplication				m_App = EApplication.eUndefined;
		public string					m_sDescription = "";

		public ISAppEndpoint()
		{
			m_VMC = new ISMVMC();
		}

		public void Init(ISMVMC i_VMC, string i_sMachine, EApplication i_App, string i_sDescription)
		{
			m_VMC = i_VMC;
			m_sMachine = i_sMachine;
			m_App = i_App;
			m_sDescription = i_sDescription;
		}

		#region ICloneable Members

		public object Clone()
		{
			ISAppEndpoint	aeClone = new ISAppEndpoint();

			aeClone.m_VMC = (ISMVMC)this.m_VMC.Clone();
			aeClone.m_sMachine = (string)this.m_sMachine.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.
			aeClone.m_App = this.m_App;
			this.m_sDescription = (string)this.m_sDescription.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.

			return(aeClone);
		}

		#endregion
	}

	/// <summary>
	/// The base message type
	/// </summary>
	[Serializable()]
	public class ISMsg : ICloneable
	{
		public enum eMsgDistribution
		{
			eUndefined,
			eBroadcast,
			eGroup,
			eSingle,
		}

		public ISAppEndpoint			m_Source = null;			// Where the message came from
		public ISAudioEndpoint			m_SourceAudio = null;		// Where the audio comes from
		public ISAppEndpoint			m_Dest = null;				// Where the message is (intented) to go.  Resolved in the derived class(?)
		public ISAudioEndpoint			m_DestAudio = null;			// Where the audio terminates
		public eMsgDistribution			m_Distribution = eMsgDistribution.eUndefined;
		public DateTime					m_tCreated;					// Time the message was created.
		public DateTime					m_tTouched;					// Last time the message was accessed/modified.
		public string					m_sSessionId = "";			// Unique identifier for the session.

		public ISMsg()
		{
			m_Source = new ISAppEndpoint();
			m_SourceAudio = new ISAudioEndpoint();
			m_Dest = new ISAppEndpoint();
			m_DestAudio = new ISAudioEndpoint();
			m_tCreated = m_tTouched = DateTime.Now;
			m_sSessionId = "";
		}

		public ISMsg(ISAppEndpoint i_Source)
		{
			//m_iVMC = ;
			m_Source = i_Source;
			m_tCreated = m_tTouched = DateTime.Now;
			m_sSessionId = "";

			m_SourceAudio = new ISAudioEndpoint();
			m_DestAudio = new ISAudioEndpoint();
		}

		public static bool Clone(ISMsg i_mSrc, ISMsg i_mDest)
		{
			bool	bRet = true;

			try
			{
				i_mDest.m_Source = (ISAppEndpoint)i_mSrc.m_Source.Clone();
				i_mDest.m_SourceAudio = (ISAudioEndpoint)i_mSrc.m_SourceAudio.Clone();
				i_mDest.m_Dest = (ISAppEndpoint)i_mSrc.m_Dest.Clone();
				i_mDest.m_DestAudio = (ISAudioEndpoint)i_mSrc.m_DestAudio.Clone();
				i_mDest.m_Distribution = i_mSrc.m_Distribution;
				i_mDest.m_tCreated = i_mSrc.m_tCreated;
				i_mDest.m_tTouched = i_mSrc.m_tTouched;
				i_mDest.m_sSessionId = i_mSrc.m_sSessionId;
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("ERROR: Caught exception in ISMsg.Copy() - '{0}'.", e);
				bRet = false;
			}

			return(bRet);
		}

		#region ICloneable Members

		public object Clone()
		{
			bool	bRes;
			ISMsg	mClone = new ISMsg();

			bRes = Clone(this, mClone);

			return(mClone);
		}

		#endregion
	}

	namespace Session
	{
		/// <summary>
		/// Client must init m_Dest info.
		/// </summary>
		[Serializable()]
		public class ISMSessionBegin : ISMsg, ICloneable
		{
			public DateTime		m_tBegan = DateTime.Now;
			public string		m_sFromAddr = "";
			public string		m_sToAddr = "";

			public ISMSessionBegin(string i_sFromAddr, string i_sToAddr)
			{
				m_sFromAddr = i_sFromAddr;
				m_sToAddr = i_sToAddr;
			}

			public ISMSessionBegin(string i_sFromAddr, string i_sToAddr, ISAppEndpoint i_Source) : base(i_Source)
			{
				m_Dest = new ISAppEndpoint();
				m_sFromAddr = i_sFromAddr;
				m_sToAddr = i_sToAddr;
			}

			#region ICloneable Members

			new public object Clone()
			{
				ISMSessionBegin		sbClone = new ISMSessionBegin(this.m_sFromAddr, this.m_sToAddr);

				// Make a clone of the base class info
				ISMsg.Clone(this, sbClone);

				// Then copy the derived class data
				sbClone.m_tBegan = this.m_tBegan;

				return(sbClone);
			}

			#endregion
		}

		[Serializable()]
		public class ISMSessionEnd : ISMsg, ICloneable
		{
			public DateTime		m_tEnded;
            public string m_sReason;

			public ISMSessionEnd()
			{
			}

			public ISMSessionEnd(ISAppEndpoint i_Source) : base(i_Source)
			{
				m_Dest = new ISAppEndpoint();
			}
			#region ICloneable Members

			new public object Clone()
			{
				ISMSessionEnd		seClone = new ISMSessionEnd();

				// Make a clone of the base class info
				ISMsg.Clone(this, seClone);

				// Then copy the derived class data
				seClone.m_tEnded = this.m_tEnded;
                seClone.m_sReason = this.m_sReason;

				return(seClone);
			}

			#endregion
		}

		[Serializable()]
		public class ISMTransferSession : ISMsg, ICloneable
		{
			public string			m_sTransferToAddr;
			//Audio.ISMPlayPrompts	m_Prompts;

			public ISMTransferSession()
			{
			}

			public ISMTransferSession(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			#region ICloneable Members
			new public object Clone()
			{
				ISMTransferSession	mtClone = new ISMTransferSession();

				ISMsg.Clone(this, mtClone);
				mtClone.m_sTransferToAddr = (string)this.m_sTransferToAddr.Clone();	// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.
				return(mtClone);
			}
			#endregion
		}

		[Serializable()]
		public class ISMTerminateSession : ISMsg, ICloneable
		{
			public ISMTerminateSession()
			{
			}

			public ISMTerminateSession(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			#region ICloneable Members
			new public object Clone()
			{
				ISMTerminateSession	mtClone = new ISMTerminateSession();

				ISMsg.Clone(this, mtClone);
				return(mtClone);
			}
			#endregion
		}

		[Serializable()]
		public class ISMTerminateSessionAfterPrompts : ISMsg, ICloneable
		{
			public ISMTerminateSessionAfterPrompts()
			{
			}

			public ISMTerminateSessionAfterPrompts(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			#region ICloneable Members
			new public object Clone()
			{
				ISMTerminateSessionAfterPrompts	mtClone = new ISMTerminateSessionAfterPrompts();

				ISMsg.Clone(this, mtClone);
				return(mtClone);
			}
			#endregion
		}

		public enum eTimerType
		{
			Custom = 0,
			Inactivity,
		};

		[Serializable()]
		public class ISMSetTimer : ISMsg, ICloneable
		{
			public eTimerType	m_Type = eTimerType.Custom;
			public string		m_sId = "";

			public ISMSetTimer()
			{
			}

			public ISMSetTimer(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			#region ICloneable Members
			new public object Clone()
			{
				ISMSetTimer	stClone = new ISMSetTimer();

				ISMsg.Clone(this, stClone);
				stClone.m_Type = this.m_Type;
				stClone.m_sId = (string)this.m_sId.Clone();
				return(stClone);
			}
			#endregion
		}

		[Serializable()]
		public class ISMTimerExpired : ISMsg, ICloneable
		{
			public eTimerType	m_Type = eTimerType.Custom;
			public string		m_sId = "";

			public ISMTimerExpired()
			{
			}

			public ISMTimerExpired(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			#region ICloneable Members
			new public object Clone()
			{
				ISMTimerExpired	teClone = new ISMTimerExpired();

				ISMsg.Clone(this, teClone);
				teClone.m_Type = this.m_Type;
				teClone.m_sId = (string)this.m_sId.Clone();
				return(teClone);
			}
			#endregion
		}

        [Serializable()]
        public class ISMDialogManagerIdle : ISMsg, ICloneable
        {
            public ISMDialogManagerIdle()
            {
            }

            public ISMDialogManagerIdle(ISAppEndpoint i_Source)
                : base(i_Source)
            {
            }

            #region ICloneable Members
            new public object Clone()
            {
                ISMDialogManagerIdle mtClone = new ISMDialogManagerIdle();

                ISMsg.Clone(this, mtClone);
                return (mtClone);
            }
            #endregion
        }
    } // namespace Session

	namespace Audio
	{
		// FIX - duplicated from ASRFacade.cs
		public enum SOUND_FORMAT
		{
			UNK_FORMAT = 0,
			ULAW_8KHZ,
			PCM_8KHZ,
			PCM_16KHZ,
			ALAW_8KHZ,
			SPX_8KHZ,
			SPX_16KHZ
		};

		/// <summary>
		/// Doesn't need to be "Serializable" or "ICloneable" unless it is sent across AppDomains.  It is only
		/// used for AudioRtr<-->AudioMgr messages, which is across a socket and doesn't need .NET marshalling.
		/// </summary>
		public class ISMRawData : ISMsg
		{
			public SOUND_FORMAT	m_Format = SOUND_FORMAT.UNK_FORMAT;
			public byte[]		m_abData = null;
			public int			m_iLen = 0;

			/// <summary>
			/// Sets array to mu-law "0".
			/// </summary>
			/// <param name="i_abBuff"></param>
			public static void MuClear(byte[] i_abBuff)
			{
				int		ii;

				if(i_abBuff != null)
				{
					for(ii = 0; ii < i_abBuff.Length; ii++)
					{
						i_abBuff[ii] = 127;
					}
				}
			}

			public ISMRawData()
			{
			}

			public ISMRawData(ISAppEndpoint i_Source, SOUND_FORMAT i_Format, byte[] i_abData, int i_iLen) : base(i_Source)
			{
				m_Format = i_Format;
				if( (i_iLen > 0) && (i_iLen <= i_abData.Length) )
				{
					m_abData = new byte[i_iLen];
					if(i_Format == SOUND_FORMAT.ULAW_8KHZ)
					{
						MuClear(m_abData);
					}
					Array.Copy(i_abData, m_abData, m_abData.Length);
				}
				else
				{
					Console.Error.WriteLine("ERROR: ISMessaging.Audio.ISMRawData - invalid data size.");
				}
			}

			public bool Init(ISAppEndpoint i_Source, ISAppEndpoint i_Dest, SOUND_FORMAT i_Format, byte[] i_abData, int i_iLen)
			{
				bool		bRet = true;

				try
				{
					m_Source = i_Source;
					m_Dest = i_Dest;
					m_Format = i_Format;
					if( (i_iLen > 0) && (i_iLen <= i_abData.Length) )
					{
						m_abData = new byte[i_iLen];
						if(i_Format == SOUND_FORMAT.ULAW_8KHZ)
						{
							MuClear(m_abData);
						}
						Array.Copy(i_abData, m_abData, m_abData.Length);
					}
					else
					{
						Console.Error.WriteLine("ERROR: ISMessaging.Audio.ISMRawData - invalid data size.");
					}
				}
				catch(Exception e)
				{
					bRet = false;
					Console.Error.WriteLine("ERROR: ISMessaging.Audio.ISMRawData.Init() caught exception '{0}'.", e.ToString());
				}

				return(bRet);
			}
		} // ISMRawData

		/// <summary>
		/// Doesn't need to be "Serializable" or "ICloneable" unless it is sent across AppDomains.  It is only
		/// used for AudioMgr-->AudioRtr messages, which is across a socket and doesn't need .NET marshalling.
		/// (It is currently only used to send the message from the AudioInThread to AudioOutThread.)
		/// </summary>
		public class ISMSpeechStart : ISMsg
		{
			// Don't need any info beyond what's included in ISMsg.
		}

		public class ISMSpeechStop : ISMsg
		{
			// Don't need any info beyond what's included in ISMsg.
		}

        public class ISMDtmfStart : ISMsg 
        {
            // Don't need any info beyond what's included in ISMsg.
        }

        public class ISMDtmfStop : ISMsg
        {
            // Don't need any info beyond what's included in ISMsg.
        }

        public class ISMBargeinEnable : ISMsg		// Same comment as above, but these two are AudioInThread <-- AudioOutThread
		{
			// Don't need any info beyond what's included in ISMsg.
		}

		public class ISMBargeinDisable : ISMsg		// Same comment as above, but these two are AudioInThread <-- AudioOutThread
		{
			// Don't need any info beyond what's included in ISMsg.
		}

		/// <summary>
		/// DTMF received from AudioRtr.
		/// </summary>
		[Serializable()]
		public class ISMDtmf : ISMsg
		{
			public string		m_sDtmf = "";

			public ISMDtmf()
			{
			}
		}

		/// <summary>
		/// Msg for when all DTMF digits have been collected.
		/// </summary>
		[Serializable()]
		public class ISMDtmfComplete : ISMDtmf, ICloneable
		{
			public ISMDtmfComplete()
			{
			}

			public ISMDtmfComplete(ISAppEndpoint i_Source, ISAppEndpoint i_Dest, string i_sDtmf)
			{
				m_Source = i_Source;
				m_Dest = i_Dest;
				m_sDtmf = i_sDtmf;
			}

			public bool Init(ISAppEndpoint i_Source, ISAppEndpoint i_Dest, string i_sDtmf)
			{
				bool bRet = true;

				m_Source = i_Source;
				m_Dest = i_Dest;
				m_sDtmf = i_sDtmf;

				return(bRet);
			}

			#region ICloneable Members

			new public object Clone()
			{
				ISMDtmfComplete	dcClone = new ISMDtmfComplete();

				// Make a clone of the base class info
				ISMsg.Clone(this, dcClone);

				// Then copy the derived class data
				dcClone.m_sDtmf = this.m_sDtmf;

				return(dcClone);
			}

			#endregion
		}

		/*
		[Serializable()]
		public class ISMUtteranceStart : ISMsg
		{
			public ISMUtteranceStart(ISAppEndpoint i_Source) : base(i_Source)
			{
			}
		}

		[Serializable()]
		public class ISMUtteranceData : ISMsg
		{
			public ISMUtteranceData(ISAppEndpoint i_Source) : base(i_Source)
			{
			}
		}

		[Serializable()]
		public class ISMUtteranceStop : ISMsg
		{
			public ISMUtteranceStop(ISAppEndpoint i_Source) : base(i_Source)
			{
			}
		}
		*/

		[Serializable()]
		public class ISMPlayPrompts : ISMsg, ICloneable
		{
			// FIX - This should either contain a DPrompt or be able to contain all DPrompt data members.
			public enum PromptType
			{
				eUnknown,
				eSSML,
				eTTS_Text,
				eWavFilePath,
				eBuffer,
				eSocket,
				eEval,		// I.e., evaluate at runtime.  In VoiceXML, an 'audio expr'.
			}

			[Serializable()]
			public class Prompt
			{
				public PromptType	m_Type = PromptType.eUnknown;
				public string		m_sText = "";						// TTS string
				public string		m_sPath = "";						// WAV (or other) path and filename
				public string		m_sLang = "en-US";					// Language (for TTS), default to US english
				public Gender		m_Gender = Gender.Female;			// Gender (for TTS), default to female
				public string		m_sVoiceName = "";					// Name of a specific voice to use for TTS (for example, "Callie-8kHz")
				public string		m_sTTSVendorProduct = "";			// Vendor make & model of TTS (for example, "Cepstral" or "Nuance|RealSpeak")

				public Prompt()
				{
					Clear();
				}

				public void Clear()
				{
					m_Type = PromptType.eUnknown;
					m_sText = "";
					m_sPath = "";
					m_sLang = "en-US";
					m_Gender = Gender.Female;
					m_sVoiceName = "";
					m_sTTSVendorProduct = "";
				}
			}

			public Prompt[]			m_Prompts = null;
			public bool				m_bBargeinEnabled = true;

			public ISMPlayPrompts()
			{
				Clear();
			}

			public ISMPlayPrompts(ISAppEndpoint i_Source) : base(i_Source)
			{
				Clear();
			}

			public void Clear()
			{
				m_Prompts = null;
			}

			#region ICloneable Members

			new public object Clone()
			{
				ISMPlayPrompts	ppClone = new ISMPlayPrompts();

				// Make a clone of the base class info
				ISMsg.Clone(this, ppClone);

				ppClone.m_bBargeinEnabled = m_bBargeinEnabled;
				if(m_Prompts != null)
				{
					// Then copy the derived class data
					if(m_Prompts.Length > 0)
					{
						ppClone.m_Prompts = new Prompt[m_Prompts.Length];
						for(int ii = 0; ii < m_Prompts.Length; ii++)
						{
							if(m_Prompts[ii] != null)
							{
								ppClone.m_Prompts[ii] = new Prompt();
								ppClone.m_Prompts[ii].m_Type = m_Prompts[ii].m_Type;
								ppClone.m_Prompts[ii].m_sText = m_Prompts[ii].m_sText;
								ppClone.m_Prompts[ii].m_sPath = m_Prompts[ii].m_sPath;
								ppClone.m_Prompts[ii].m_sLang = m_Prompts[ii].m_sLang;
								ppClone.m_Prompts[ii].m_Gender = m_Prompts[ii].m_Gender;
								ppClone.m_Prompts[ii].m_sVoiceName = m_Prompts[ii].m_sVoiceName;
								ppClone.m_Prompts[ii].m_sTTSVendorProduct = m_Prompts[ii].m_sTTSVendorProduct;
							}
						}
					}
				}

				return(ppClone);
			}

			#endregion
		}

        /// <summary>
        /// Doesn't need to be "Serializable" or "ICloneable" unless it is sent across AppDomains.  It is only
        /// used for AudioMgr-->AudioMgr messages, which doesn't need .NET marshalling.
        /// (It is currently only used to send the message from the AudioOutThread to AudioOutThread.)
        /// </summary>
        public class ISMPlayPromptsComplete : ISMsg
        {
            // Don't need any info beyond what's included in ISMsg.
        }

        [Serializable()]
        public class ISMSetProperty : ISMsg, ICloneable
        {
            public string m_sName = "";
            public string m_sValue = "";

			public ISMSetProperty()
			{
			}

            public ISMSetProperty(ISAppEndpoint i_Source) : base(i_Source)
            {
            }

			#region ICloneable Members

			new public object Clone()
			{
                ISMSetProperty spClone = new ISMSetProperty();

                // Make a clone of the base class info
                ISMsg.Clone(this, spClone);

                // Then copy the derived class data
                spClone.m_sName = this.m_sName;
                spClone.m_sValue = this.m_sValue;

                return spClone;
			}

			#endregion
        }
		/* *************************************************************************
		[Serializable()]
		public class ISMPlaybackStart : ISMsg
		{
			public ISMPlaybackStart(ISAppEndpoint i_Source) : base(i_Source)
			{
			}
		}

		[Serializable()]
		public class ISMPlaybackData : ISMsg
		{
			public byte[] m_abData;

			public ISMPlaybackData(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			public ISMPlaybackData(ISAppEndpoint i_Source, byte[] i_abData) : base(i_Source)
			{
				//m_abcData = abData.Clone();
				Array.Copy(i_abData, m_abData, m_abData.GetLength(0));
			}
		}

		[Serializable()]
		public class ISMPlaybackStop : ISMsg
		{
			public ISMPlaybackStop(ISAppEndpoint i_Source) : base(i_Source)
			{
			}
		}
		************************************************************************* */
	}

	namespace SpeechRec
	{
		using SymanticResult = System.String;			// A "visual crutch" to make code easier to read.  FIX - multiple declaration!!!  (see AudioEngine_Console.cs)

		public enum SOUND_FORMAT
		{
			UNK_FORMAT = 0,
			ULAW_8KHZ,
			PCM_8KHZ,
			PCM_16KHZ,
			ALAW_8KHZ,
			SPX_8KHZ,
			SPX_16KHZ
		};

		///////////////////////////////////////////////////////////////
		public interface IASR
		{
			bool SetPropertyInt(int i_iPropIndex, int i_iPropValue);
			bool SetPropertyBool(int i_iPropIndex, bool i_bPropValue);
			bool SetPropertyStr(int i_iPropIndex, string i_sPropValue);

			bool Init(ILegacyLogger i_Logger, GrammarBuilder.eGramFormat i_GrammarFormat);
			bool Release();

			bool Open();
			bool Close();

			bool AddPhrase(string i_sPhrase, SymanticResult i_sResult);
			bool LoadGrammar(bool i_bGlobal, string i_sName, string i_sGram);
			bool LoadGrammarFromFile(bool i_bGlobal, string i_sName, string i_sPath);
			bool ResetGrammar();

			// Full audio of utterance.
			bool AddUtterance(byte[] i_abData);

			// Audio streaming memebers
			bool UtteranceStart();
			bool UtteranceData(byte[] i_abData);
			bool UtteranceStop();

			//bool Results(out SymanticResult[] o_aResults);			// Should use RecognitionResult instead.
			bool Results(out RecognitionResult[] o_aResults);
		} // IASR

		///////////////////////////////////////////////////////////////
		public class GrammarBuilder
		{
			private	string			m_sName = "";
			private string			m_sLangCode = "en-US";
			private	PhrasePairMaps	m_aPPMaps = null;

			public enum eGramFormat
			{
				Phrases = 0,
				SRGS_GRXML = 1,
				SRGS_ABNF = 2,
			};

			public enum eGramErr
			{
				Success = 0,
				Unknown,				// Unknown error (should never return one of these)
				Exception,				// An exception was caught
				InvalidData,			// Indicates that the data it had to work with was erroneous
				NotImplemented,			// Functionality requested is not currently implemented
			};

			public string Name
			{
				get { return(m_sName); }
				set { m_sName = value; }
			}

			public string LangCode
			{
				get { return (m_sLangCode); }
				set { m_sLangCode = value; }
			}

			public PhrasePairMaps PPMaps
			{
				get { return(m_aPPMaps); }
			}

			public static string RemoveSpaces(string i_sString)
			{
				string		sRet = "";
				int			iSpace;

				try
				{
					sRet = i_sString;
					iSpace = sRet.IndexOf(' ');
					while(iSpace >= 0)
					{
						sRet = sRet.Remove(iSpace, 1);
						iSpace = sRet.IndexOf(' ');
					}
				}
				catch
				{
				}

				return(sRet);
			}

			public GrammarBuilder()
			{
				m_aPPMaps = new PhrasePairMaps();
			}

//			public eGramErr Add(PhrasePairMaps i_aPPMaps)
//			{
//			}

			public eGramErr Add(PhrasePairMap i_PPMap)
			{
				return(Add_impl(i_PPMap));
			} // Add

			private eGramErr Add_impl(PhrasePairMap i_PPMap)
			{
				eGramErr		eRet = eGramErr.Success;

				try
				{
					m_aPPMaps.Add(i_PPMap);
				}
				catch(Exception exc)
				{
Console.Error.WriteLine("GrammarBuilder.Add caught exception: " + exc);
					eRet = eGramErr.Exception;
				}

				return(eRet);
			} // Add_impl

			public eGramErr Encode(eGramFormat i_eFormat, out string o_sGram)
			{
				return(Encode_impl(i_eFormat, out o_sGram));
			} // Encode

			private eGramErr Encode_impl(eGramFormat i_eFormat, out string o_sGram)
			{
				eGramErr		eRet = eGramErr.Success;

				o_sGram = "";

				try
				{
					switch(i_eFormat)
					{
						case eGramFormat.SRGS_GRXML :
							eRet = EncodeSrgsXml(out o_sGram);
							break;
						case eGramFormat.SRGS_ABNF :
							eRet = EncodeSrgsAbnf(out o_sGram);
							break;
					}
				}
				catch(Exception exc)
				{
Console.Error.WriteLine("GrammarBuilder.Encode caught exception: " + exc);
					eRet = eGramErr.Exception;
				}

				return(eRet);
			} // Encode_impl

			private eGramErr EncodeSrgsAbnf(out string o_sGram)
			{
				eGramErr		eRet = eGramErr.Success;
				string			sPreamble = "", sClose = "";
				StringBuilder	sbAbnf = null;
				int				ii = 0;
				string			sRootGramName = "MainGrammar";		// FIX - Use PhrasePairMap name

				o_sGram = "";

				try
				{
					sbAbnf = new StringBuilder();
					sPreamble =	"#ABNF 1.0;" + Environment.NewLine +
								"language " + m_sLangCode + ";" + Environment.NewLine +
								"mode voice;" + Environment.NewLine +
								"root $" + sRootGramName + ";" + Environment.NewLine;
					sClose = Environment.NewLine;

					sbAbnf.Append(sPreamble);
					sbAbnf.AppendFormat("${0} = ", sRootGramName);
					for(ii = 0; ii < m_aPPMaps.Count; ii++)
					{
						foreach(string sTmp in m_aPPMaps[ii].Phrases)
						{
							sbAbnf.AppendFormat("{0}|", sTmp);
						}
					}

					if(m_aPPMaps.Count > 0)
					{
						sbAbnf.Remove(sbAbnf.Length - 1, 1);
					}
					sbAbnf.Append(";" + Environment.NewLine);

					sbAbnf.Append(sClose);

					o_sGram = sbAbnf.ToString();
				}
				catch(Exception exc)
				{
Console.Error.WriteLine("GrammarBuilder.EncodeSrgsAbnf caught exception: " + exc);
					eRet = eGramErr.Exception;
				}

				return(eRet);
			}

			private eGramErr EncodeSrgsXml(out string o_sGram)
			{
				eGramErr		eRet = eGramErr.Success;
				string			sPreamble = "", sClose = "";
				StringBuilder	sbXml = null;
				int				ii = 0;

				string			sRootGramName = "MainGrammar";		// FIX - Use PhrasePairMap name

				o_sGram = "";

				try
				{
					sbXml = new StringBuilder();
					//sPreamble = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><grammar version=\"1.0\" xmlns=\"http://www.w3.org/2001/06/grammar\" xml:lang=\"" + sLang + "\" tag-format=\"semantics/1.0-literals\" root=\"" + m_sName + "\">";
					sPreamble =	"<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + 
								"    <grammar version=\"1.0\" xmlns=\"http://www.w3.org/2001/06/grammar\"" +
								"        xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
								"        xsi:schemaLocation=\"http://www.w3.org/2001/06/grammar http://www.w3.org/TR/speech-grammar/grammar.xsd\"" +
								"        xml:lang=\"" + m_sLangCode + "\" root=\"" + sRootGramName + "\" mode=\"voice\" tag-format=\"semantics/1.0\">";
					sClose =	"    </grammar>";

					sbXml.Append(sPreamble);

					for(ii = 0; ii < m_aPPMaps.Count; ii++)
					{
						sbXml.AppendFormat("<rule id=\"{0}\" scope=\"public\"><one-of>", m_aPPMaps[ii].ResultTag);		// Using Result as rule id

						foreach(string sTmp in m_aPPMaps[ii].Phrases)
						{
							sbXml.AppendFormat("<item>{0}</item>", sTmp);
							//sbXml.AppendFormat("<item>{0}<tag>out = \"{0}\";</tag></item>", sTmp);	// "out" not required by LV, removed to shorten VoiceXML.
						}

						sbXml.AppendFormat("</one-of></rule>" + Environment.NewLine);
					}

					sbXml.AppendFormat("<rule id=\"{0}\" scope=\"public\"> ", sRootGramName);
					sbXml.Append("<one-of>" + Environment.NewLine);
					for(ii = 0; ii < m_aPPMaps.Count; ii++)
					{
						sbXml.Append("<item>");
						sbXml.AppendFormat("<ruleref uri=\"#{0}\"/>", m_aPPMaps[ii].ResultTag);
						sbXml.Append("</item>" + Environment.NewLine);
					}
					sbXml.Append("</one-of>" + Environment.NewLine);
					sbXml.Append("</rule>" + Environment.NewLine);

					sbXml.Append(sClose);

					o_sGram = sbXml.ToString();
				}
				catch(Exception exc)
				{
Console.Error.WriteLine("GrammarBuilder.EncodeSrgsXml caught exception: " + exc);
					eRet = eGramErr.Exception;
				}

				return(eRet);
			}

		} // GrammarBuilder

		public class PhrasePairMaps : CollectionBase
		{
			public int Add(PhrasePairMap i_Elem)
			{
				return(List.Add(i_Elem));
			}

			public bool Contains(PhrasePairMap i_Elem)
			{
				return(List.Contains(i_Elem));
			}

			public int IndexOf(PhrasePairMap i_Elem)
			{
				return(List.IndexOf(i_Elem));
			}

			public void Insert(int i_iIndex, PhrasePairMap i_Elem)
			{
				List.Insert(i_iIndex, i_Elem);
			}

			public void Remove(PhrasePairMap i_Elem)
			{
				List.Remove(i_Elem);
			}

			public PhrasePairMap this[int i_iIndex]
			{
				get {	return((PhrasePairMap)List[i_iIndex]);	}
				set {	List[i_iIndex] = value;	}
			}

		}

		///////////////////////////////////////////////////////////////
		// Data classes
		[Serializable()]
		public class PhrasePairMap : ICloneable					// It's not really a map...
		{
			//private	string				m_sName = "";
			private StringCollection	m_asPhrases = null;
			private SymanticResult		m_sResultTag = "";

			public PhrasePairMap()
			{
				m_asPhrases = new StringCollection();
			}

//			public string Name
//			{
//				get { return(m_sName); }
//				set { m_sName = value; }
//			}

			public StringCollection Phrases
			{
				get { return(m_asPhrases); }
				set { m_asPhrases = value; }
			}

			public SymanticResult ResultTag
			{
				get { return(m_sResultTag); }
				set { m_sResultTag = value; }
			}

			public void Clear()
			{
				m_sResultTag = "";
				if(m_asPhrases == null)
				{
					m_asPhrases = new StringCollection();
				}
				else
				{
					m_asPhrases.Clear();
				}
			}

			#region ICloneable Members

			public object Clone()
			{
				PhrasePairMap	ppClone = new PhrasePairMap();

//				ppClone.Name = (string)this.Name.Clone();
				//ppClone.Phrases = this.Phrases.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.
				ppClone.ResultTag = (SymanticResult)this.ResultTag.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.

				// Manually copy each phrase, since StringCollection doesn't have a Clone member
				foreach(string sPhrase in this.Phrases)
				{
					ppClone.Phrases.Add(sPhrase);
				}

				return(ppClone);
			}

			#endregion
		} // PhrasePairMap

		///////////////////////////////////////////////////////////////
		[Serializable()]
		public class PhrasePair : ICloneable
		{
			private string			m_sPhrase = "";
			private SymanticResult	m_sResult = "";

			public string Phrase
			{
				get { return(m_sPhrase); }
				set { m_sPhrase = value; }
			}

			public SymanticResult Result
			{
				get { return(m_sResult); }
				set { m_sResult = value; }
			}

			#region ICloneable Members

			public object Clone()
			{
				PhrasePair	ppClone = new PhrasePair();

				ppClone.m_sPhrase = (string)this.m_sPhrase.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.
				ppClone.m_sResult = (string)this.m_sResult.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.

				return(ppClone);
			}

			#endregion
		} // PhrasePair

		[Serializable()]
		public class RecognitionResult : ICloneable
		{
			private SymanticResult	m_sResult = "";
			private	string			m_sText = "";
			private int				m_iProbability = 0;

			public SymanticResult Result
			{
				get { return(m_sResult); }
				set { m_sResult = value; }
			}

			public string Text
			{
				get { return(m_sText); }
				set { m_sText = value; }
			}

			public int Probability
			{
				get { return(m_iProbability); }
				set { m_iProbability = value; }
			}

			#region ICloneable Members

			public object Clone()
			{
				RecognitionResult	rrClone = new RecognitionResult();

				rrClone.m_sResult = (string)this.m_sResult.Clone();		// FIX - will a reference to this member by the remote object still call back?  Undesireable if so.
				rrClone.m_iProbability = this.m_iProbability;

				return(rrClone);
			}

			#endregion
		}

		///////////////////////////////////////////////////////////////
		// Message classes

		[Serializable()]
		public class ISMLoadPhrases : ISMsg, ICloneable
		{
			public string		m_sLangCode = "en-US";
			public string[]		m_asPhrases = null;

			public ISMLoadPhrases()
			{
			}

			public ISMLoadPhrases(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			public ISMLoadPhrases(ISAppEndpoint i_Source, string i_sLangCode, string[] i_asPhrases) : base(i_Source)
			{
				int		ii, iNumPhrases = 0;

				m_sLangCode = i_sLangCode;

				iNumPhrases = i_asPhrases.Length;
				m_asPhrases = new string[iNumPhrases];

				for(ii = 0; ii < iNumPhrases; ii++)
				{
					m_asPhrases[ii] = (string)(i_asPhrases[ii].Clone());
				}
			}

			#region ICloneable Members

			new public object Clone()
			{
				ISMLoadPhrases	phrasesClone = new ISMLoadPhrases();

				// Make a clone of the base class info
				ISMsg.Clone(this, phrasesClone);

				// Then copy the derived class data
				phrasesClone.m_sLangCode = this.m_sLangCode;
				phrasesClone.m_asPhrases = (string[])this.m_asPhrases.Clone();

				return(phrasesClone);
			}

			#endregion
		}

		[Serializable()]
		public class ISMLoadGrammarFile : ISMsg, ICloneable
		{
			public string		m_sUri = "";

			public ISMLoadGrammarFile()
			{
			}

			public ISMLoadGrammarFile(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			public ISMLoadGrammarFile(ISAppEndpoint i_Source, string i_sUri) : base(i_Source)
			{
				m_sUri = (string)i_sUri.Clone();
			}
			#region ICloneable Members

			new public object Clone()
			{
				ISMLoadGrammarFile	lgfClone = new ISMLoadGrammarFile();

				ISMsg.Clone(this, lgfClone);

				lgfClone.m_sUri = (string)this.m_sUri.Clone();

				return(lgfClone);
			}

			#endregion
		}

		[Serializable()]
		public class ISMResults : ISMsg, ICloneable
		{
			public enum Source
			{
				Unknown,
				Audio,
				//Visual/Screen/KVM/...
			}

			public RecognitionResult[]		m_Results;

			public ISMResults()
			{
			}

			public ISMResults(ISAppEndpoint i_Source) : base(i_Source)
			{
			}

			#region ICloneable Members

			new public object Clone()
			{
				ISMResults	mrClone = new ISMResults();

				// Make a clone of the base class info
				ISMsg.Clone(this, mrClone);

				// Then copy the derived class data
				mrClone.m_Results = (RecognitionResult[])this.m_Results.Clone();	// Will this successfully copy the whole array?

				return(mrClone);
			}

			#endregion
		}
	}

	namespace Delivery
	{
		/// <summary>
		/// 
		/// </summary>
		public interface IISMReceiver
		{
			bool Ping();
			bool NewMsg(ISMsg i_Msg);		// Handoff the new message.
		}

		/// <summary>
		/// The class that hosts the remotable interface (the remoting server) for receiving messages.
		/// </summary>
		public class ISMReceiverImpl : MarshalByRefObject, IISMReceiver
		{
			#region IISMReceiver Members

			public virtual bool Ping()		// virtual needed so that method isn't sealed.
			{
				Console.WriteLine("ISMReceiverImpl.Ping received.");
				return(true);
			}

			public virtual bool NewMsg(ISMsg i_Msg)
			{
				Console.WriteLine("ISMReceiverImpl.NewMsg received on TID {0}.", Thread.CurrentThread.ManagedThreadId);
				return (true);
			}

			#endregion
		}

		/// <summary>
		/// The class that distributes the received messages to the objects that subscribed to them.
		/// </summary>
/**/
		public class ISMDistributer
		{
			public class ISMDistributerEventArgs : EventArgs
			{
				public readonly ISMsg	m_Msg;
				public ISMDistributerEventArgs(ISMsg i_Msg)
				{
					m_Msg = i_Msg;
				}
			}

			public delegate void ISMDistributerEventHandler(Object i_sender, ISMDistributerEventArgs i_args);
			public event ISMDistributerEventHandler ISMMsg;

			protected virtual void OnISMMsg(ISMDistributerEventArgs i_args)
			{
				if(ISMMsg != null)
				{
					ISMMsg(this, i_args);
				}
			}

			public bool Send(ISMsg i_Msg)
			{
				ISMDistributerEventArgs evArgs = new ISMDistributerEventArgs(i_Msg);

				OnISMMsg(evArgs);
				return(true);
			}
		}
/**/
/*
		/// <summary>
		/// ISMSender decides where to send the message and sends it.
		/// </summary>
		public class ISMSender
		{
			public bool Send(ISMsg i_Msg)
			{
				ISMsgDistributerEventArgs evArgs = new ISMDistributerEventArgs(i_Msg);
				e
				return(true);
			}
		}
*/
	}
}
