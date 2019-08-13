// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging;


namespace SBLocalRM
{
	/// <summary>
	/// 
	/// </summary>
	public class RMMsg : ISMsg		// FIX - Should this be in ISMessaging?
	{
		public	IPEndPoint	m_EP = null;
		public	Byte[]		m_abMsg = null;
		public	string		m_sMsgDecoded = "";

		public RMMsg(IPEndPoint i_EP, Byte[] i_abMsg)
		{
			m_EP = new IPEndPoint(i_EP.Address, i_EP.Port);	// Copy EP
			m_abMsg = i_abMsg;
			m_sMsgDecoded = Encoding.ASCII.GetString(i_abMsg);				// FIX - Should this be UTF8?
		}
	} // RMMsg

	/// <summary>
	/// Doesn't need to be "Serializable" or "ICloneable" unless it is sent across AppDomains.
	/// </summary>
	public class UdpMsgListenerThread
	{
		private ILegacyLogger	m_Logger = null;
		private MsgQueue		m_aqMsgIn = null;
		private IPEndPoint		m_IPEndPoint = null;
		private UdpClient		m_receivingUdpClient = null;

		public UdpMsgListenerThread(ILegacyLogger i_Logger, MsgQueue i_aqMsgIn)
		{
			int		iPort = 0;

			m_Logger = i_Logger;
			m_aqMsgIn = i_aqMsgIn;
			iPort = int.Parse(ConfigurationManager.AppSettings["UdpListenPort"]);
			m_IPEndPoint = new IPEndPoint(IPAddress.Any, iPort);	// Accept any source
			m_receivingUdpClient = new UdpClient(m_IPEndPoint);
		} // UdpMsgListenerThread contstructor

		public void ThreadProc()
		{
			Thread.CurrentThread.Name = "LRMListenerT";
			m_Logger.Init(m_IPEndPoint.Address.ToString(), "", Thread.CurrentThread.Name, "", "", "");
			m_Logger.Log(Level.Verbose, "UdpMsgListenerThread started.");

			NetLoop();

			m_Logger.Log(Level.Verbose, "UdpMsgListenerThread exiting.");
		} // ThreadProc

		public void NetLoop()
		{
			bool		bDone = false, bRes = true;;
			Byte[]		abReceived = null;
			RMMsg		rmMsg = null;

			while(!bDone)
			{
				try
				{
					// Wait for UDP message
					abReceived = m_receivingUdpClient.Receive(ref m_IPEndPoint);

					// Copy data to a RM msg, push it to worker thread, and Ack-Received.
					rmMsg = new RMMsg(m_IPEndPoint, abReceived);
					m_aqMsgIn.Push(rmMsg);

					bRes = SendAckReceived(rmMsg);

					rmMsg = null;
					abReceived = null;
				}
				catch(Exception exc)
				{
					m_Logger.Log(exc);
				}
			}
		} // NetLoop

		public bool SendAckReceived(RMMsg i_Msg)
		{
			bool		bRet = true;
			Byte[]		abMsg = null;
			int			iSeqNum = -1;
			int			iStart = -1, iEnd = -1;
			int			iRes = 0;
			XmlMsg		xMsg = null;

			try
			{
				iStart = i_Msg.m_sMsgDecoded.IndexOf("<" + XmlMsg.eMsgTags.SeqNum.ToString() + ">") + XmlMsg.eMsgTags.SeqNum.ToString().Length + 2;
				iEnd = i_Msg.m_sMsgDecoded.IndexOf("</" + XmlMsg.eMsgTags.SeqNum.ToString() + ">");

				if( (iStart < 0) || (iEnd < 1) )
				{
					m_Logger.Log(Level.Exception, "UdpMsgListenerThread.SendAckReceived: SeqNum tags not found.");
				}
				else
				{
					iSeqNum = int.Parse(i_Msg.m_sMsgDecoded.Substring(iStart, (iEnd - iStart)));

					xMsg = new XmlMsg();
					xMsg.MsgType = XmlMsg.eMsgTypes.Ack;
					xMsg.SeqNum = iSeqNum;
					abMsg = Encoding.ASCII.GetBytes(xMsg.ToString());					// FIX - Should this be UTF8?

					iRes = m_receivingUdpClient.Send(abMsg, abMsg.Length, i_Msg.m_EP);
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				m_Logger.Log(exc);
			}

			return(bRet);
		} // SendAckReceived
	} // UdpMsgListenerThread
}
