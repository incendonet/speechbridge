// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;
using SBConfigStor;

namespace SBResourceMgr
{
	using VMCList = List<ResourceMgr_local.VMCInfo>;

	/// <summary>	/// A simple local IResourceMgr implementation for keeping track of session information.  It
	/// currently does NOT synchronize with remote ResourceMgr_local-s or independent ResourceMgr-s.
	/// </summary>
	public class ResourceMgr_local : IResourceMgr
	{
		public class VMCInfo
		{
			public enum VMCState
			{
				Unused,
				InUse,
			} // enum VMCState

			public VMCState		m_State = VMCState.Unused;
			public ISMVMC		m_VMC = null;

			public VMCInfo()
			{
				m_State = VMCState.Unused;
				m_VMC = new ISMVMC();
				m_VMC.Clear();
			}

			public VMCInfo(ref ISMVMC i_VMC)
			{
				m_State = VMCState.Unused;
				m_VMC = i_VMC;
			} // VMCInfo ctor
		} // class VMCInfo

		//private static	Mutex		m_Lock = new Mutex();
		private static	VMCList			m_VMCs;					// FIX - May be inefficient with larger installations (use dictionary instead.)
		private	static	int				m_iMaxSessions = -1;
		private	static	int				m_iNumSessions = 0;
		private static readonly object	m_Lock = new object();
		protected		ILegacyLogger	m_Logger = null;

		public ResourceMgr_local(ILegacyLogger i_Logger)
		{
			int		ii;
			ISMVMC	vmc;

			try
			{
				m_Logger = i_Logger;
				m_iMaxSessions = ConfigParams.GetNumExt();

				m_VMCs = new VMCList();

				Monitor.Enter(m_Lock);

				for(ii = 0; ii < m_iMaxSessions; ii++)
				{
					vmc = new ISMVMC();
					vmc.Clear();
					m_VMCs.Add(new VMCInfo(ref vmc));
				}
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl ctor caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}
		} // ctor

		public virtual int GetMaxSessions()
		{
			return(m_iMaxSessions);
		} // GetMaxSessions

		public virtual int GetNumSessions()
		{
			int		iRes = 0;

			try
			{
				// Probably unnecessary to lock, but just in case...
				Monitor.Enter(m_Lock);
				iRes = m_iNumSessions;
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl GetNumSessions caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(iRes);
		} // GetNumSessions

		/// <summary>
		/// Creates a session-id to keep track of each session.
		/// </summary>
		/// <param name="i_iThreadIndex"></param>
		/// <param name="i_iVmcIndex"></param>
		/// <returns></returns>
		public string CreateSessionId(int i_iThreadIndex, int i_iVmcIndex)
		{
			string sRet = "";

			sRet = ISMessaging.Utilities.GetNumericDateTime(DateTime.Now) + "_" + i_iThreadIndex + "_" + i_iVmcIndex.ToString();		// FIX - Should we also include the IP address?

			return (sRet);
		}

		/// <summary>
		/// NOTE:  This is NOT thread safe, it is assumed that the caller will be performing
		/// any necessary syncronization.
		/// </summary>
		/// <returns></returns>
		protected ISMVMC GetFreeVMC(int i_iThreadIndex, string i_sSrc)
		{
			int		ii;
			ISMVMC	vRet = null;

			for(ii = 0; ( (ii < m_iMaxSessions) && (vRet == null) ); ii++)
			{
				//if(((ISMVMC)(m_VMCs[ii].m_VMC)).m_iKey == -1)
				if(m_VMCs[ii].m_State == VMCInfo.VMCState.Unused)
				{
					m_VMCs[ii].m_State = VMCInfo.VMCState.InUse;

					vRet = (ISMVMC)m_VMCs[ii].m_VMC;
					vRet.m_iKey = ii;
					vRet.m_sDescription = i_sSrc;
					vRet.m_sSessionId = CreateSessionId(i_iThreadIndex, ii);
				}
			}

			return(vRet);
		} // GetFreeVMC

		/// <summary>
		/// Adds a new session, given the thread-index and source string.  If the i_sSrc is already in use, this
		/// means that it wasn't released properly, and we can reuse it.
		/// </summary>
		/// <param name="i_iThreadIndex"></param>
		/// <param name="i_sSrc"></param>
		/// <returns></returns>
		public virtual int AddSession(int i_iThreadIndex, string i_sSrc)
		{
			int iRet = -1;
			ISMVMC vmc;
			string sSrc = "";
			char[] acTrim = { '\0' };

			try
			{
				sSrc = i_sSrc.Trim(acTrim);

				Monitor.Enter(m_Lock);

				// Check to see if the sSrc already exists.  If it does, reuse it.
				// FIX - Depending on how the "src" (as of now, it is just the SIP extension) is used, this could be problematic.  If
				// we allow multiple src entries (to allow time for the previous session to clear out of the system), we could run out,
				// and sessions wouldn't be correctly connected.
				vmc = GetVMCBySrc_NoSync(sSrc);
				if (vmc.m_iKey != -1)
				{
#if(true)
					iRet = vmc.m_iKey;
					vmc.Init(iRet, sSrc, CreateSessionId(i_iThreadIndex, iRet));
					m_Logger.Log(Level.Warning, string.Format("RMl AddSession('{0}') reused for '{1}'.", sSrc, vmc.m_sSessionId));
#else
					m_Logger.Log(Level.Warning, "RMl AddSession(id=" + sSrc + ") still lingering!");
#endif
				}
				else
				{
					vmc = null;		// Release vmc returned by GetVMCNoSync
				}

				// If not, add a new entry.
				if (iRet == -1)
				{
					// FIX - Needs to store additional data: session start, resources used, etc...
					vmc = GetFreeVMC(i_iThreadIndex, sSrc);
					if (vmc == null)
					{
						// FIX - If we can't get a free VMC now, should we sleep and try again?
						iRet = -1;
						m_Logger.Log(Level.Exception, "RMl AddSession(id=" + sSrc + ") couldn't GetFreeVMC!");
						DumpVMCs();
					}
					else
					{
						//vmc.Init(vmc.m_iKey, vmc.m_sDescription, vmc.m_sSessionId);		// Isn't this redundant?
						iRet = vmc.m_iKey;

						m_iNumSessions++;
					}
				}
			}
			catch (Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl AddSession(id=" + sSrc + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return (iRet);
		} // AddSession

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iThreadIndex"></param>
		/// <param name="i_sSrc"></param>
		/// <param name="i_iMaxRetries"></param>
		/// <param name="i_iRetryLen">In milliseconds</param>
		/// <returns></returns>
		public virtual int AddSession(int i_iThreadIndex, string i_sSrc, int i_iMaxRetries, int i_iRetryLen)
		{
			int					iRet = -1;
			int					ii = 0;

			for(ii = 0; ((ii <= i_iMaxRetries) && (iRet == -1)); ii++)
			{
				iRet = AddSession(i_iThreadIndex, i_sSrc);
				if( (iRet == -1) && (ii < i_iMaxRetries) )
				{
m_Logger.Log(Level.Exception, "RMl AddSession failed, sleeping...");
					Thread.Sleep(i_iRetryLen);
				}
			}

			return (iRet);
		} // AddSession

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iKey"></param>
		/// <returns></returns>
		public virtual bool ReleaseSession(int i_iKey)
		{
			bool	bRet = true;
			VMCInfo	vRes;

			try
			{
				Monitor.Enter(m_Lock);

				vRes = GetVMCInfoByKey_NoSync(i_iKey);
				vRes.m_VMC.Clear();
				vRes.m_State = VMCInfo.VMCState.Unused;

				m_iNumSessions--;
			}
			catch(Exception exc)
			{
				//Console.Error.WriteLine("ResourceMgr_local.RemoveSession('{0}') Caught exception: '{1}'.", i_iKey.ToString(), e.ToString());
				m_Logger.Log(Level.Exception, "RMl ReleaseSession(key=" + i_iKey.ToString() + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(bRet);
		} // ReleaseSession

/*
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sSessionId"></param>
		/// <returns></returns>
		public virtual bool ReleaseSession(string i_sSessionId)
		{
			bool	bRet = true;
			ISMVMC	vRes;

			try
			{
				Monitor.Enter(m_Lock);

				vRes = GetVMCNoSync(i_sSessionId);
				vRes.Clear();
			}
			catch(Exception exc)
			{
				//Console.Error.WriteLine("ResourceMgr_local.RemoveSession('{0}') Caught exception: '{1}'.", i_sSessionId, e.ToString());
				m_Logger.Log(Level.Exception, "RMl ReleaseSession(id=" + i_sSessionId + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(bRet);
		} // ReleaseSession
*/

		protected ISMVMC GetVMCBySrc_NoSync(string i_sSrc)
		{
			ISMVMC		vRet = null;
			int			ii, iLen;
			string		sTmp;

			iLen = m_VMCs.Count;
			for(ii = 0; ((ii < iLen) && (vRet == null)); ii++)
			{
				sTmp = ((ISMVMC)(m_VMCs[ii].m_VMC)).m_sDescription;
				if (i_sSrc == sTmp)
				{
					vRet = (ISMVMC)m_VMCs[ii].m_VMC;
				}
			}

			if(vRet == null)	// VMC wasn't found
			{
				// Allocate new VMC so we don't return null
				vRet = new ISMVMC();
				vRet.Clear();
			}

			return(vRet);
		} // GetVMCBySrc_NoSync

		protected VMCInfo GetVMCInfoByKey_NoSync(int i_iKey)
		{
			VMCInfo			vRet = null;
			StringBuilder	sbTmp = null;

			if(i_iKey > (m_VMCs.Count - 1))
			{
				//Console.Error.WriteLine("ERROR: ResourceMgr_local.GetVMCNoSyncByKey() invalid index '{0}'.", i_iKey.ToString());
				sbTmp = new StringBuilder();
				sbTmp.AppendFormat("ResourceMgr_local.GetVMCInfoByKey_NoSync() invalid index '{0}'.  Stack: {1}", i_iKey.ToString(), System.Environment.StackTrace.ToString().Replace(System.Environment.NewLine, "|"));
				m_Logger.Log(Level.Exception, sbTmp.ToString());

				// Allocate new VMCInfo so null isn't returned
				vRet = new VMCInfo();
			}
			else
			{
				vRet = m_VMCs[i_iKey];

				if(i_iKey != vRet.m_VMC.m_iKey)
				{
					//Console.Error.WriteLine("WARNING: ResourceMgr_local.GetVMCNoSyncByKey() VMC index '{0}' doesn't match key passed in '{1}'.", vRet.m_iKey.ToString(), i_iKey.ToString());
					sbTmp = new StringBuilder();
					sbTmp.AppendFormat("ResourceMgr_local.GetVMCInfoByKey_NoSync() VMC index '{0}' doesn't match key passed in '{1}'.  Stack: {2}", vRet.m_VMC.m_iKey.ToString(), i_iKey.ToString(), System.Environment.StackTrace.ToString().Replace(System.Environment.NewLine, "|"));
					m_Logger.Log(Level.Warning, sbTmp.ToString());

					vRet.m_VMC.m_iKey = -1;
				}
			}

			return(vRet);
		} // GetVMCInfoByKey_NoSync

		protected ISMVMC GetVMCBySessionid_NoSync(string i_sSessionid)
		{
			ISMVMC			vRet = null;

			vRet = m_VMCs.Find(vmc => vmc.m_VMC.m_sSessionId == i_sSessionid).m_VMC;

			return(vRet);
		} // GetVMCBySessionid_NoSync

		/// <summary>
		/// This is dangerous IF it is possible for the src to be reused (for example, if a call from a AR hasn't completely cleared AM before a new call comes in.)
		/// </summary>
		/// <param name="i_sSrc"></param>
		/// <returns></returns>
/*
		protected virtual ISMVMC GetVMCBySrc(string i_sSrc)
		{
			ISMVMC		vRet = null;

			try
			{
				Monitor.Enter(m_Lock);
				vRet = GetVMCBySrc_NoSync(i_sSrc);
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl GetVMCBySrc(id=" + i_sSrc + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(vRet);
		} // GetVMCBySrc
*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iKey"></param>
		/// <returns>VMC, or null if not found.</returns>
		public virtual ISMVMC GetVMCByKey(int i_iKey)
		{
			ISMVMC		vRet = null;

			try
			{
				Monitor.Enter(m_Lock);
				vRet = GetVMCInfoByKey_NoSync(i_iKey).m_VMC;
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl GetVMCByKey(key=" + i_iKey.ToString() + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(vRet);
		} // GetVMCByKey

		public virtual ISMVMC GetVMCBySessionid(string i_sSessionid)
		{
			ISMVMC		vRet = null;

			try
			{
				Monitor.Enter(m_Lock);
				vRet = GetVMCBySessionid_NoSync(i_sSessionid);
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl GetVMCBySessionid(key=" + i_sSessionid + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(vRet);
		} // GetVMCBySessionid

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iKey"></param>
		/// <returns></returns>
		public virtual string GetSessionId(int i_iKey)
		{
			string		sRet = "";
			ISMVMC		vmcTmp = null;

			try
			{
				if(i_iKey == -1)	// Catch if an invalid key was passed in
				{
					vmcTmp = new ISMVMC();
					vmcTmp.Clear();
				}
				else
				{
					Monitor.Enter(m_Lock);

					vmcTmp = GetVMCInfoByKey_NoSync(i_iKey).m_VMC;
					sRet = vmcTmp.m_sSessionId;
				}
			}
			catch(Exception exc)
			{
				m_Logger.Log(Level.Exception, "RMl GetSessionId(key=" + i_iKey.ToString() + ") caught exception: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(sRet);
		} // GetSessionId

		/// <summary>
		/// Prints the state of the VMCs to the log.
		/// </summary>
		private void DumpVMCs()
		{
			int				ii = 0;

			for(ii = 0; ii < m_iMaxSessions; ii++)
			{
				if(m_VMCs[ii] == null)
				{
					m_Logger.Log(Level.Debug, string.Format("RMl.vmc[{0}] null", ii));
				}
				else
				{
					m_Logger.Log(Level.Debug, string.Format("RMl.vmc[{0}] state:'{1}', key:'{2}', id:'{3}', descr:'{4}'.", ii, m_VMCs[ii].m_State.ToString(), m_VMCs[ii].m_VMC.m_iKey.ToString(), m_VMCs[ii].m_VMC.m_sSessionId, m_VMCs[ii].m_VMC.m_sDescription));
				}
			}
		} // DumpVMCs

	} // class ResourceMgr_local
}
