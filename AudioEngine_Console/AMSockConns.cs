// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;

namespace AudioMgr
{
	/// <summary>
	/// 
	/// </summary>
	// FIX - Move to separate file.
	public class AMSockConns
	{
		private ILegacyLogger				m_Logger = null;
		public	ArrayList					m_Socks;	// Array of AMSockconn
		private static readonly object		m_Lock = new object();

		public AMSockConns(ILegacyLogger i_Logger)
		{
			m_Logger = i_Logger;
			m_Socks = new ArrayList();
		}

		public AMSockConn NewSockConn(int i_iARReadPort, int i_iARWritePort)
		{
			AMSockConn	ascRet = null;

			ascRet = new AMSockConn();
			m_Socks.Add(ascRet);

			ascRet.m_iARReadPort = i_iARReadPort;
			ascRet.m_iARWritePort = i_iARWritePort;

			ascRet.m_listenARRead = new TcpListener(IPAddress.Any, ascRet.m_iARReadPort);
			ascRet.m_listenARWrite = new TcpListener(IPAddress.Any, ascRet.m_iARWritePort);
			if( (ascRet.m_listenARRead == null) || (ascRet.m_listenARWrite == null) )
			{
				ascRet = null;
				//Console.Error.WriteLine("ERROR AMSockConn.NewSockConn() - Couldn't create sockets!");
				m_Logger.Log(Level.Exception, "AMSockConn.NewSockConn() - Couldn't create sockets!");
			}
			else
			{
				ascRet.m_listenARRead.Start();
				ascRet.m_listenARWrite.Start();
			}

			return(ascRet);
		}

		public AMSockConn GetSockConn(int i_iVMCKey)
		{
			int			ii = 0, iLen = 0;
			AMSockConn	ascTmp = null, ascRet = null;

			Monitor.Enter(m_Lock);

			try
			{
				iLen = m_Socks.Count;
				for (ii = 0; ((ii < iLen) && (ascRet == null)); ii++)
				{
					ascTmp = (AMSockConn)(m_Socks[ii]);
					if (ascTmp.m_VMC != null)					// Otherwise it hasn't been tied to a VMC, and thus isn't in use.
					{
						if (ascTmp.m_VMC.m_iKey == i_iVMCKey)
						{
							ascRet = ascTmp;
						}
					}
				}

				// If a socket wasn't found for the VMC...
				if (ascRet == null)
				{
					m_Logger.Log(Level.Exception, "AMSockConn.GetSockConn() - Socket not found for VMC #" + i_iVMCKey.ToString() + ".");
					m_Logger.Log(Level.Debug, "GSC dump:------------------------------------------------------");
					ii = 0;
					for (ii = 0; ii < iLen; ii++)
					{
						ascTmp = (AMSockConn)(m_Socks[ii]);
						if (ascTmp.m_VMC != null)
						{
							m_Logger.Log(Level.Debug, string.Format("GSC dump: [{0}] == {1}", ii, ascTmp.m_VMC.m_iKey));
						}
					}
					m_Logger.Log(Level.Debug, "GSC dump:------------------------------------------------------");
				}
			}
			catch (Exception exc)
			{
				m_Logger.Log(Level.Exception, string.Format("AMSockConn.GetSockConn() - Exception for VMC #{0}: '{1}'.", i_iVMCKey.ToString(), exc.ToString()));
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(ascRet);
		}

		public AMSockConn GetSockConn(string i_sVMCDescription)
		{
			int			ii = 0, iLen = 0;
			AMSockConn	ascTmp = null, ascRet = null;

			Monitor.Enter(m_Lock);

			try
			{
				iLen = m_Socks.Count;
				for(ii = 0; ( (ii < iLen) && (ascTmp == null) ); ii++)
				{
					ascTmp = (AMSockConn)(m_Socks[ii]);
					if(ascTmp.m_VMC.m_sDescription == i_sVMCDescription)
					{
						ascRet = ascTmp;
					}
				}

				// If a socket wasn't found for the VMC...
				if(ascRet == null)
				{
					m_Logger.Log(Level.Exception, "AMSockConn.GetSockConn() - Socket not found for VMC by description: '" + i_sVMCDescription + "'.");
				}
			}
			catch (Exception exc)
			{
				m_Logger.Log(Level.Exception, string.Format("AMSockConn.GetSockConn() - Exception for VMC '{0}': '{1}'.", i_sVMCDescription, exc.ToString()));
			}
			finally
			{
				Monitor.Exit(m_Lock);
			}

			return(ascRet);
		}

		public class AMSockConn
		{
			// Note:  May need accessor functions in here at some point, but not yet.
			// FIX - Find a way to get C++ 'friend' class functionality, I don't want to have all of these members public, only to AMSockConns.
			public		ISMVMC			m_VMC = null;
			public		int				m_iARReadPort = 1780;	// Default value, the call to NewSockConn will change it.
			public		int				m_iARWritePort = 1781;

			public		TcpListener		m_listenARRead = null;	// FIX - Only sockets declared as public static are thread safe.
			public		Socket			m_sockARRead = null;
			public		TcpListener		m_listenARWrite = null;	// FIX - Only sockets declared as public static are thread safe.
			public		Socket			m_sockARWrite = null;

			public		ISMVMC			VMC
			{
				get
				{	return(m_VMC);
				}
				set
				{	m_VMC = value;
				}
			}
			public		int				ReadPort
			{
				get
				{	return(m_iARReadPort);
				}	
			}
			public		int				WritePort
			{
				get
				{	return(m_iARWritePort);
				}	
			}
		}
	} // class AMSockConns
}
