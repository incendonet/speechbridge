using System;
using System.Text;

namespace SBLocalRM
{
	public enum eMsgTags
	{
		Msg = 0,
		MsgType,
		SeqNum,
		DataSize,
		Data,
	}

	// NOTE: If the spelling of any member below changes, MsgtypeFromString() below will need to reflect the change.
	public enum eMsgTypes
	{
		NOP = 0,
		Ack,				// ie., msg received
		Ping,
		Stat,
		SBResetComp,
		SBResetCompDone,
		SBStop,
		SBStopDone,
		SBUpdate,
		SBUpdateDone,
		OSUpdate,
		OSUpdateDone,
		NetSet,
		NetSetDone,
		SrvReset,
	}

	/// <summary>
	/// Summary description for RmXmlMsg.
	/// </summary>
	public class RmXmlMsg
	{
		private	StringBuilder	m_sbMsg = null;

		private	eMsgTypes		m_eMsgType = eMsgTypes.NOP;
		private	int				m_iSeqNum = -1;
		private	int				m_iDataSize = 0;
		private	string			m_sData = "";

		public RmXmlMsg()
		{
			m_sbMsg = new StringBuilder();
		} // RmXmlMsg constructor

		public eMsgTypes MsgType
		{
			get { return(m_eMsgType); }
			set { m_eMsgType = value; }
		}
		public int SeqNum
		{
			get { return(m_iSeqNum); }
			set { m_iSeqNum = value; }
		}
		public int DataSize
		{
			get { return(m_iDataSize); }
		}
		public string Data
		{
			get { return(m_sData); }
			set { m_sData = value; }
		}

		override public string ToString()
		{
			m_sbMsg.Length = 0;

			m_sbMsg.AppendFormat("<{0}>", eMsgTags.Msg.ToString());
			m_sbMsg.AppendFormat("<{0}>{1}</{0}>", eMsgTags.MsgType.ToString(), m_eMsgType.ToString());
			m_sbMsg.AppendFormat("<{0}>{1}</{0}>", eMsgTags.SeqNum.ToString(), m_iSeqNum.ToString());
			m_sbMsg.AppendFormat("<{0}>{1}</{0}>", eMsgTags.DataSize.ToString(), m_iDataSize.ToString());
			m_sbMsg.AppendFormat("<{0}>{1}</{0}>", eMsgTags.Data.ToString(), m_sData);
			m_sbMsg.AppendFormat("</{0}>", eMsgTags.Msg.ToString());

			return(m_sbMsg.ToString());
		} // ToString

		public static RmXmlMsg Parse(string i_sMsg)
		{
			RmXmlMsg		mTmp = null;

			try
			{
				mTmp = new RmXmlMsg();

				mTmp.MsgType = MsgtypeFromString(GetElemValFromXml(i_sMsg, eMsgTags.MsgType.ToString()));
				mTmp.SeqNum = int.Parse(GetElemValFromXml(i_sMsg, eMsgTags.SeqNum.ToString()));
				mTmp.Data = GetElemValFromXml(i_sMsg, eMsgTags.Data.ToString());
			}
			catch
			{
				mTmp = null;
			}

			return(mTmp);
		} // Parse

		/// <summary>
		/// FIX: This is an inefficient implementation as it searches from the beginning for each tag.
		/// </summary>
		/// <param name="i_sXMl"></param>
		/// <returns></returns>
		public static string GetElemValFromXml(string i_sXml, string i_sTag)
		{
			int			iStart = -1, iEnd = -1;
			string		sVal = "";

			try
			{
				iStart = i_sXml.IndexOf("<" + i_sTag + ">") + i_sTag.Length + 2;
				iEnd = i_sXml.IndexOf("</" + i_sTag + ">");
				if( (iStart < 0) || (iEnd < 1) )
				{
					// Tag not found.
				}
				else
				{
					sVal = i_sXml.Substring(iStart, (iEnd - iStart));
				}
			}
			catch
			{
				sVal = "";
			}

			return(sVal);
		} // GetElemValFromXml

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sString"></param>
		/// <returns></returns>
		public static eMsgTypes MsgtypeFromString(string i_sString)
		{
			eMsgTypes	mRet = eMsgTypes.NOP;

			switch(i_sString)
			{
				case "NOP" :
					mRet = eMsgTypes.NOP;
					break;
				case "Ack" :
					mRet = eMsgTypes.Ack;
					break;
				case "Ping" :
					mRet = eMsgTypes.Ping;
					break;
				case "Stat" :
					mRet = eMsgTypes.Stat;
					break;
				case "SBResetComp" :
					mRet = eMsgTypes.SBResetComp;
					break;
				case "SBResetCompDone" :
					mRet = eMsgTypes.SBResetCompDone;
					break;
				case "SBStop" :
					mRet = eMsgTypes.SBStop;
					break;
				case "SBStopDone" :
					mRet = eMsgTypes.SBStopDone;
					break;
				case "SBUpdate" :
					mRet = eMsgTypes.SBUpdate;
					break;
				case "SBUpdateDone" :
					mRet = eMsgTypes.SBUpdateDone;
					break;
				case "OSUpdate" :
					mRet = eMsgTypes.OSUpdate;
					break;
				case "OSUpdateDone" :
					mRet = eMsgTypes.OSUpdateDone;
					break;
				case "NetSet" :
					mRet = eMsgTypes.NetSet;
					break;
				case "NetSetDone" :
					mRet = eMsgTypes.NetSetDone;
					break;
				case "SrvReset" :
					mRet = eMsgTypes.SrvReset;
					break;
				default :
					mRet = eMsgTypes.NOP;
					break;
			}

			return(mRet);
		} // MsgtypeFromString
	} // RmXmlMsg
}
