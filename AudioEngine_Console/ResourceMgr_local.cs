using System;
using System.Collections;
using System.Configuration;
using System.Text;
using System.Threading;

using NSpring.Logging;
using NSpring.Logging.Loggers;
using ISMessaging;

namespace ResourceMgr
{
	public interface IResourceMgr
	{
		int GetMaxSessions();
		int GetNumSessions();
		
		//int AddSession(string i_sSessionId);
		int AddSessionIfNew(string i_sSessionId);
		bool ReleaseSession(string i_sSessionId);
		bool ReleaseSession(int i_iKey);

		ISMVMC GetVMC(string i_sSessionId);
		ISMVMC GetVMC(int i_iKey);
	}

	/// <summary>
	/// A simple local IResourceMgr implementation for keeping track of session information.  It
	/// currently does NOT synchronize with remote ResourceMgr_local-s or independent ResourceMgr-s.
	/// </summary>
	public class ResourceMgr_local : IResourceMgr
	{
		private static	Mutex		m_Lock = new Mutex();
		private static	ArrayList	m_VMCs;					// FIX - May be inefficient with larger installations (use dictionary instead.)
		private	static	int			m_iMaxSessions;
		private	static	int			m_iNumSessions = 0;
		protected		Logger		m_Logger = null;

		public ResourceMgr_local(Logger i_Logger)
		{
			int		ii;
			ISMVMC	vmc;

			m_Logger = i_Logger;

			m_iMaxSessions = int.Parse(ConfigurationSettings.AppSettings["MaxSessions"]);

			m_Lock.WaitOne();

			if(m_VMCs == null)
			{
				m_VMCs = new ArrayList();

				for(ii = 0; ii < m_iMaxSessions; ii++)
				{
					vmc = new ISMVMC();
					m_VMCs.Add(vmc);
				}
			}

			m_Lock.ReleaseMutex();
		}

		public virtual int GetMaxSessions()
		{
			return(m_iMaxSessions);
		}

		public virtual int GetNumSessions()
		{
			int		iRes = 0;

			// Probably unnecessary to lock, but just in case...
			m_Lock.WaitOne();
			iRes = m_iNumSessions;
			m_Lock.ReleaseMutex();

			return(iRes);
		}

		/// <summary>
		/// NOTE:  This is NOT thread safe, it is assumed that the caller will be performing
		/// any necessary syncronization.
		/// </summary>
		/// <returns></returns>
		protected ISMVMC GetFreeVMC()
		{
			int		ii;
			ISMVMC	vRet = null;

			for(ii = 0; ( (ii < m_iMaxSessions) && (vRet == null) ); ii++)
			{
				if(((ISMVMC)(m_VMCs[ii])).m_iKey == -1)
				{
					vRet = (ISMVMC)m_VMCs[ii];
					vRet.m_iKey = ii;
				}
			}

			return(vRet);
		}

		/// <summary>
		/// Note - This function incorporates the behavior of GetVMCIdFromSessionId and AddSession, but it
		/// is unable to simply call both, because everything needs to be done as one atomic operation.  Any
		/// updates to behavior should be reflected in the others!
		/// </summary>
		/// <param name="i_sSessionId"></param>
		/// <returns></returns>
		public virtual int AddSessionIfNew(string i_sSessionId)
		{
			int		iRet = -1;
			ISMVMC	vmc;

			try
			{
				m_Lock.WaitOne();

				// Check to see if the SessionId already exists.
				vmc = GetVMCNoSync(i_sSessionId);
				if(vmc != null)
				{
					iRet = vmc.m_iKey;
				}

				// If not, add a new entry.
				if(iRet == -1)
				{
					// FIX - Needs to store additional data: session start, resources used, etc...
					vmc = GetFreeVMC();
					if(vmc == null)
					{
					}
					else
					{
						vmc.Init(vmc.m_iKey, i_sSessionId);	// Keep the m_iKey assigned in GetFreeVMC().
						iRet = vmc.m_iKey;

						m_iNumSessions++;
					}
				}
			}
			catch(Exception e)
			{
				//Console.Error.WriteLine("ResourceMgr_local.AddSession('{0}') Caught exception: '{1}'.", i_sSessionId, e.ToString());
				m_Logger.Log(e);
			}
			finally
			{
				m_Lock.ReleaseMutex();
			}

			return(iRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iKey"></param>
		/// <returns></returns>
		public virtual bool ReleaseSession(int i_iKey)
		{
			bool	bRet = true;
			ISMVMC	vRes;

			try
			{
				m_Lock.WaitOne();

				vRes = GetVMCNoSync(i_iKey);
				vRes.Clear();

				m_iNumSessions--;
			}
			catch(Exception e)
			{
				//Console.Error.WriteLine("ResourceMgr_local.RemoveSession('{0}') Caught exception: '{1}'.", i_iKey.ToString(), e.ToString());
				m_Logger.Log(e);
			}
			finally
			{
				m_Lock.ReleaseMutex();
			}

			return(bRet);
		}

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
				m_Lock.WaitOne();

				vRes = GetVMCNoSync(i_sSessionId);
				vRes.Clear();
			}
			catch(Exception e)
			{
				//Console.Error.WriteLine("ResourceMgr_local.RemoveSession('{0}') Caught exception: '{1}'.", i_sSessionId, e.ToString());
				m_Logger.Log(e);
			}
			finally
			{
				m_Lock.ReleaseMutex();
			}

			return(bRet);
		}

		protected ISMVMC GetVMCNoSync(string i_sSessionId)
		{
			ISMVMC		vRet = null;
			int			ii, iLen;
			string		sTmp;

			iLen = m_VMCs.Count;
			for(ii = 0; ((ii < iLen) && (vRet == null)); ii++)
			{
				sTmp = ((ISMVMC)(m_VMCs[ii])).m_sDescription;
				if(i_sSessionId == sTmp)
				{
					vRet = (ISMVMC)m_VMCs[ii];
				}
			}

			return(vRet);
		}

		protected ISMVMC GetVMCNoSync(int i_iKey)
		{
			ISMVMC			vRet = null;
			StringBuilder	sbTmp = null;

			if(i_iKey > (m_VMCs.Count - 1))
			{
				//Console.Error.WriteLine("ERROR: ResourceMgr_local.GetVMCNoSync() invalid index '{0}'.", i_iKey.ToString());
				sbTmp = new StringBuilder();
				sbTmp.AppendFormat("ResourceMgr_local.GetVMCNoSync() invalid index '{0}'.", i_iKey.ToString());
				m_Logger.Log(Level.Exception, sbTmp.ToString());
			}
			else
			{
				vRet = (ISMVMC)(m_VMCs[i_iKey]);

				if(i_iKey != vRet.m_iKey)
				{
					//Console.Error.WriteLine("WARNING: ResourceMgr_local.GetVMCNoSync() VMC index '{0}' doesn't match key passed in '{1}'.", vRet.m_iKey.ToString(), i_iKey.ToString());
					sbTmp = new StringBuilder();
					sbTmp.AppendFormat("ResourceMgr_local.GetVMCNoSync() VMC index '{0}' doesn't match key passed in '{1}'.", vRet.m_iKey.ToString(), i_iKey.ToString());
					m_Logger.Log(Level.Warning, sbTmp.ToString());
				}
			}

			return(vRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sSessionId"></param>
		/// <returns></returns>
		public virtual ISMVMC GetVMC(string i_sSessionId)
		{
			ISMVMC		vRet = null;

			try
			{
				m_Lock.WaitOne();
				vRet = GetVMCNoSync(i_sSessionId);
			}
			catch(Exception e)
			{
				//Console.Error.WriteLine("ResourceMgr_local.GetVMC('{0}') Caught exception: '{1}'.", i_sSessionId, e.ToString());
				m_Logger.Log(e);
			}
			finally
			{
				m_Lock.ReleaseMutex();
			}

			return(vRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_iKey"></param>
		/// <returns>VMC, or null if not found.</returns>
		public virtual ISMVMC GetVMC(int i_iKey)
		{
			ISMVMC		vRet = null;

			try
			{
				m_Lock.WaitOne();
				vRet = GetVMCNoSync(i_iKey);
			}
			catch(Exception e)
			{
				//Console.Error.WriteLine("ResourceMgr_local.GetVMC('{0}') Caught exception: '{1}'.", i_iKey.ToString(), e.ToString());
				m_Logger.Log(e);
			}
			finally
			{
				m_Lock.ReleaseMutex();
			}

			return(vRet);
		}

		/*
		public int AddResource()
		{
			int		iRet = -1;

			return(-1);
		}
		*/
	}
}
