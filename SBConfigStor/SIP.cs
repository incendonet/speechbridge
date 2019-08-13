// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.IO;
using System.Text;

namespace SBConfigStor
{
	/// <summary>
	/// Summary description for SIP.
	/// </summary>
	public class SIP : IPersistConfigParams
	{

		////////////////////////////////////////////////////////////////////////////////
		/// Constants
		public const string	m_csSipProxyLabel =						"SipProxy";
		public const string	m_csSipFirstExtLabel =					"SipFirstExt";
		public const string	m_csSipPasswordLabel =					"SipPassword";
		public const string	m_csSipNumExtLabel =					"SipNumExt";
		public const string	m_csFirstLocalSipPortLabel =			"FirstLocalSipPort";
		public const string	m_csDisplayNamePrefixLabel =			"DisplayNamePrefix";
		public const string	m_csSipTransportLabel =					"SipTransport";
		public const string	m_csSipRegistrarIpLabel =				"SipRegistrarIp";
		public const string	m_csSipRegistrationExpirationLabel =	"SipRegistrationExpiration";
		public const string	m_csRtpPortMinLabel =					"RtpPortMin";
		public const string	m_csRtpPortMaxLabel =					"RtpPortMax";
		public const string	m_csNatIpAddrLabel =					"NatIpAddr";
		public const string	m_csAudioMgrIpLabel =					"AudioMgrIp";
		public const string	m_csAudioMgrSendPortFirstLabel =		"AudioMgrSendPortFirst";
		public const string	m_csAudioMgrRecvPortFirstLabel =		"AudioMgrRecvPortFirst";
		public const string	m_csLogLevelLabel =						"LogLevel";
		public const string	m_csLogFilenamePrefixLabel =			"LogFilenamePrefix";
		public const string	m_csSaveAudioLabel =					"SaveAudio";
		public const string m_csMasterIpLabel =						"MasterIp";
		public const string m_csSlaveIpLabel =						"SlaveIp";
		public const string m_csVirtualIpLabel =					"VirtualIp";
		public const string	m_csSipProfileTypeLabel =				"IppbxType";

		public const string	m_csSipProxyCfgtag =					"Proxy_Server";
		public const string	m_csSipExtCfgtag =						"User_Name";
		public const string	m_csSipPasswordCfgtag =					"Pass_Word";
		public const string	m_csLocalSipPortCfgtag =				"Local_SIP_Port";
		public const string	m_csDisplayNameCfgtag =					"Display_Name";
		public const string	m_csSipTransportCfgtag =				"Sip_Transport";
		public const string	m_csSipRegistrarIpCfgtag =				"Register_To";
		public const string	m_csSipRegistrationExpirationCfgtag =	"Register_Expires";
		public const string	m_csRtpPortMinCfgtag =					"Min_RTP_Port";
		public const string	m_csRtpPortMaxCfgtag =					"Max_RTP_Port";
		public const string	m_csNatIpAddrCfgtag =					"NATAddress_IP";
		public const string	m_csAudioMgrIpCfgtag =					"AudioMgrIPAddr";
		public const string	m_csAudioMgrSendPortCfgtag =			"AudioMgrSendPort";
		public const string	m_csAudioMgrRecvPortCfgtag =			"AudioMgrRecvPort";
		public const string	m_csLogLevelCfgtag =					"LogLevel";
		public const string	m_csLogFilenameCfgtag =					"LogFilename";
		//public const string	m_csSaveAudioCfgtag =					"SaveAudio";
		public const string	m_csSipProfileTypeCfgtag =				"SipProfileType";


		////////////////////////////////////////////////////////////////////////////////
		/// Data members
		protected const string		m_csLogExtension = ".log";
		protected int				m_iMaxSessions = 0;
		protected string[]			m_csProxyConfigHeader =
			{
				"[ProxySrv General Parameters]\r\n",
				"Active Sessions=0\r\n",
				"Application Log Level=1\r\n",
				"SIP and RTP Log Level=1\r\n",
				"SBC Mode=Proxy Only Mode\r\n",
				"Always Proxy Media=False\r\n",
				"Application Log File=b2bua\r\n",
				"Interface Address=sip:*:5060\r\n",
				"Syslog Server=127.0.0.1\r\n",
				"SIP Session Timer=False\r\n",
				"Hash Key 1=0\r\n",
				"Hash Key 2=0\r\n",
				"Transaction Thread Count=10\r\n",
				"Session Thread Count=10\r\n",
				"1xx Timeout To Invite=5000\r\n",
				"Connection Timeout=60000\r\n",
				"Alerting Timeout=30000\r\n",
				"Final Response Timeout To Invite=60000\r\n",
				"\r\n",
				"[Local Domain Accounts]\r\n",
				"Accept All Registration=True\r\n",
				"Account Array Size=0\r\n",
				"\r\n",
				"[Relay Routes]\r\n",
			};
		protected string[]			m_csProxyConfigFooter =
			{
				"\r\n",
				"[ProxySrv Routes]\r\n",
				"Rewrite TO URI=False\r\n",
				"Route Array Size=0\r\n",
				"\r\n",
			};
#if(false)
		protected string[]			m_csHostsHeader =
			{
				"# Do not remove the following line, or various programs\r\n",
				"# that require network functionality will fail.\r\n",
				"127.0.0.1	localhost.localdomain	localhost\r\n",
			};
#endif

		/// <summary>
		/// 
		/// </summary>
		public SIP()
		{
			m_iMaxSessions = ConfigParams.GetNumExt();
		}

		#region IPersistConfigParams Members
		public bool Persist(ConfigParams i_ConfigParams, string i_sPath)
		{
			bool bRet = true;

			string	sSipProxy					= "";
			string	sSipPassword				= "";
			string	sDisplayNamePrefix			= "";
			string	sSipTransport				= "";
			string	sSipRegistrarIp				= "";
			string	sSipRegistrationExpiration	= "";
			string	sRtpPortMax					= "";
			string	sNatIpAddr					= "";
			string	sAudioMgrIp					= "";
			string	sLogLevel					= "";
			string	sLogFilenamePrefix			= "";
			string	sMasterIp					= "";
			string	sSlaveIp					= "";
			string	sVirtualIp					= "";
			string	sSipProfileType				= "";

			// The following are numbers instead of strings since they are different in each configuration file
			// and their value depends on which channel number that configuration file corresponds to.

			int		iSipFirstExt				= 0;
			int		iFirstLocalSipPort			= 0;
			int		iRtpPortMin					= 0;
			int		iAudioMgrSendPortFirst		= 0;
			int		iAudioMgrRecvPortFirst		= 0;


			try
			{
				int iNumParams = i_ConfigParams.Count;
				for (int jj = 0; jj < iNumParams; ++jj)
				{
				    ConfigParams.ConfigParam paramTmp = i_ConfigParams[jj];
				    switch (paramTmp.Name)
				    {
						case m_csSipProxyLabel:
							{
								sSipProxy = paramTmp.Value;
							}
							break;
						case m_csSipFirstExtLabel:
							{
								iSipFirstExt = Int32.Parse(paramTmp.Value);
							}
							break;
						case m_csSipPasswordLabel:
							{
								if (paramTmp.Value.Length > 0)
								{
									sSipPassword = paramTmp.Value;
								}
							}
							break;
						case m_csSipNumExtLabel:
							{
								if (Int32.Parse(paramTmp.Value) != m_iMaxSessions)
								{
									// FIX - log warning
								}
							}
							break;
						case m_csFirstLocalSipPortLabel:
							{
								iFirstLocalSipPort = Int32.Parse(paramTmp.Value);
							}
							break;
						case m_csDisplayNamePrefixLabel:
							{
								sDisplayNamePrefix = paramTmp.Value;
							}
							break;
						case m_csSipTransportLabel:
							{
								sSipTransport = paramTmp.Value;
							}
							break;
						case m_csSipRegistrarIpLabel:
							{
								sSipRegistrarIp = paramTmp.Value;
							}
							break;
						case m_csSipRegistrationExpirationLabel:
							{
								sSipRegistrationExpiration = paramTmp.Value;
							}
							break;
						case m_csRtpPortMinLabel:
							{
								iRtpPortMin = Int32.Parse(paramTmp.Value);
							}
							break;
						case m_csRtpPortMaxLabel:
							{
								sRtpPortMax = paramTmp.Value;
							}
							break;
						case m_csNatIpAddrLabel:
							{
								sNatIpAddr = paramTmp.Value;
							}
							break;
						case m_csAudioMgrIpLabel:
							{
								sAudioMgrIp = paramTmp.Value;
							}
							break;
						case m_csAudioMgrSendPortFirstLabel:
							{
								iAudioMgrSendPortFirst = Int32.Parse(paramTmp.Value);
							}
							break;
						case m_csAudioMgrRecvPortFirstLabel:
							{
								iAudioMgrRecvPortFirst = Int32.Parse(paramTmp.Value);
							}
							break;
						case m_csLogLevelLabel:
							{
								sLogLevel = paramTmp.Value;
							}
							break;
						case m_csLogFilenamePrefixLabel:
							{
								sLogFilenamePrefix = paramTmp.Value;
							}
							break;
						case m_csSaveAudioLabel:
							{
								// FIX
							}
							break;
						case m_csMasterIpLabel:
							{
								sMasterIp = paramTmp.Value;
							}
							break;
						case m_csSlaveIpLabel:
							{
								sSlaveIp = paramTmp.Value;
							}
							break;
						case m_csVirtualIpLabel:
							{
								sVirtualIp = paramTmp.Value;
							}
							break;
						case m_csSipProfileTypeLabel:
							{
								sSipProfileType = paramTmp.Value;
							}
							break;
						default:
							{
								// FIX - log warning
							}
							break;
					} // switch
				} // for(jj)


				// If it looks like we aren't configured for a redundant configuration then
				// set those values such that the configuration files will be correctly generated
				// for a non-redundant system.

				if (String.IsNullOrEmpty(sMasterIp) || String.IsNullOrEmpty(sSlaveIp) || String.IsNullOrEmpty(sVirtualIp))
				{
					sMasterIp = sSipProxy;
					sSlaveIp = sSipProxy;
					sVirtualIp = sSipProxy;
				}

				int iNumFiles = m_iMaxSessions;

				// Write out user agent config files (typically /opt/speechbridge/config/X.cfg)
				for (int ii = 0; ii < iNumFiles; ++ii)
				{
					string sFileName;

					if(i_sPath.EndsWith("/"))
					{
						sFileName = String.Format("{0}{1}.cfg", i_sPath, ii);
					}
					else
					{
						sFileName = String.Format("{0}/{1}.cfg", i_sPath, ii);
					}

					using (StreamWriter sw = new StreamWriter(sFileName, false, new UTF8Encoding(false)))		//$$$ LP - Should UTF8 Identifier be emitted to file (if yes, can use Encoding.UTF8).
					{
						string sSipExt = (iSipFirstExt + ii).ToString();
						string sFirstLocalSipPort = (iFirstLocalSipPort + ii).ToString();
						string sRtpPortMin = (iRtpPortMin + (ii * 2)).ToString();
						string sAudioMgrSendPortFirst = (iAudioMgrSendPortFirst + (ii * 2)).ToString();
						string sAudioMgrRecvPortFirst = (iAudioMgrRecvPortFirst + (ii * 2)).ToString();

						sw.Write(FormatAudioRouterEntry(m_csSipProxyCfgtag, sVirtualIp));
						sw.Write(FormatAudioRouterEntry(m_csSipExtCfgtag, sSipExt));
						sw.Write(FormatAudioRouterEntry(m_csSipPasswordCfgtag, sSipPassword));
						sw.Write(FormatAudioRouterEntry(m_csLocalSipPortCfgtag, sFirstLocalSipPort));
						sw.Write(FormatAudioRouterEntry(m_csDisplayNameCfgtag, sDisplayNamePrefix + sSipExt));
						sw.Write(FormatAudioRouterEntry(m_csSipTransportCfgtag, sSipTransport));
						sw.Write(FormatAudioRouterEntry(m_csSipRegistrarIpCfgtag, sSipRegistrarIp));
						sw.Write(FormatAudioRouterEntry(m_csSipRegistrationExpirationCfgtag, sSipRegistrationExpiration));
						sw.Write(FormatAudioRouterEntry(m_csRtpPortMinCfgtag, sRtpPortMin));
						sw.Write(FormatAudioRouterEntry(m_csRtpPortMaxCfgtag, sRtpPortMax));
						sw.Write(FormatAudioRouterEntry(m_csNatIpAddrCfgtag, sNatIpAddr));
						sw.Write(FormatAudioRouterEntry(m_csAudioMgrIpCfgtag, sAudioMgrIp));
						sw.Write(FormatAudioRouterEntry(m_csAudioMgrSendPortCfgtag, sAudioMgrSendPortFirst));
						sw.Write(FormatAudioRouterEntry(m_csAudioMgrRecvPortCfgtag, sAudioMgrRecvPortFirst));
						sw.Write(FormatAudioRouterEntry(m_csLogLevelCfgtag, sLogLevel));
						sw.Write(FormatAudioRouterEntry(m_csLogFilenameCfgtag, sLogFilenamePrefix + sSipExt + m_csLogExtension));
						sw.Write(FormatAudioRouterEntry(m_csSipProfileTypeCfgtag, sSipProfileType));
					}
				} // for(ii)
			}
			catch(Exception)
			{
				bRet = false;
			}

			try
			{
				string sFileName;

				// Write out SIP proxy server config file (typically /opt/speechbridge/config/ProxySrv.config)
				if(i_sPath.EndsWith("/"))
				{
					sFileName = String.Format("{0}ProxySrv.config", i_sPath);
				}
				else
				{
					sFileName = String.Format("{0}/ProxySrv.config", i_sPath);
				}

				using (StreamWriter sw = new StreamWriter(sFileName, false, new UTF8Encoding(false)))		//$$$ LP - Should UTF8 Identifier be emitted to file (if yes, can use Encoding.UTF8).
				{

					// FIX - Clean this up to read from a template!!!
					// Write out header
					for (int ii = 0; ii < m_csProxyConfigHeader.Length; ii++)
					{
						sw.Write(m_csProxyConfigHeader[ii]);
					}

					// Write out routes
					//sw.Write("Route Array Size={0}\r\n", m_iMaxSessions);				// Only 2 routes now.
					sw.Write("Route Array Size=2\r\n");									// Only 2 routes now.

					string sRelayRoutes = GenerateRelayRoutes(sMasterIp, sSlaveIp, iSipFirstExt, iFirstLocalSipPort);

					sw.Write("Route 1=[sip:*@{0}:5060] {1}\r\n", sVirtualIp, sRelayRoutes);
					sw.Write("Route 2=[sip:*@{0}] {1}\r\n", sVirtualIp, sRelayRoutes);	// For now the relay route string is the same, but will it always be?

					// Write out footer
					for (int ii = 0; ii < m_csProxyConfigFooter.Length; ii++)
					{
						sw.Write(m_csProxyConfigFooter[ii]);
					}
				}
			}
			catch(Exception)
			{
				bRet = false;
			}

			// Write out /etc/hosts, if on linux
#if(false)
			try
			{
				string sFileName;

				if(RunningSystem.RunningPlatform == CLRPlatform.Mono)
				{
					if(i_sPath.EndsWith("/"))
					{
						sFileName = String.Format("{0}hosts", i_sPath);
					}
					else
					{
						sFileName = String.Format("{0}/hosts", i_sPath);
					}
				}
				else	// For testing only!!!
				{
					sFileName = "g:/proj/work/incendonet/engineering/testing/config/hosts";
				}

				using (StreamWriter sw = new StreamWriter(sFileName, false, new UTF8Encoding(false)))		//$$$ LP - Should UTF8 Identifier be emitted to file (if yes, can use Encoding.UTF8).
				{
					for (int ii = 0; ii < m_csHostsHeader.Length; ++ii)
					{
						sw.Write(m_csHostsHeader[ii]);
					}

					sw.Write("{0}	speechbridge1	speechbridge1\r\n", sProxyAddr);					// FIX - Can't assume that!!!  They may change name, and need to be able to have more than one SB on a net.
				}
			}
			catch(Exception)
			{
				bRet = false;
			}
#endif

			return(bRet);
		}
		#endregion

		private string FormatAudioRouterEntry(string i_sTag, string i_sValue)
		{
			string sEntry = "";

			if (!String.IsNullOrEmpty(i_sValue))
			{
				sEntry = String.Format("{0} string {1}\r\n", i_sTag, i_sValue);
			}

			return sEntry;
		}

		private string GenerateRelayRoutes(string i_sMasterIPAddress, string i_sSlaveIPAddress, int i_iFirstExtension, int i_iFirstLocalSipPort)
		{
			StringBuilder sbRelayRoutes = new StringBuilder();
			bool bRedundantConfiguration = (i_sMasterIPAddress != i_sSlaveIPAddress) ? true : false;

			for (int i = 0; i < m_iMaxSessions; ++i)
			{
				sbRelayRoutes.AppendFormat("sip:{0}@{1}:{2}, ", (i_iFirstExtension + i), i_sMasterIPAddress, (i_iFirstLocalSipPort + i));	// FIX - Can't always assume UA and proxy are on same machine!!!

				if (bRedundantConfiguration)
				{
					sbRelayRoutes.AppendFormat("sip:{0}@{1}:{2}, ", (i_iFirstExtension + i), i_sSlaveIPAddress, (i_iFirstLocalSipPort + i));	// FIX - Can't always assume UA and proxy are on same machine!!!
				}
			}

			sbRelayRoutes.Remove(sbRelayRoutes.Length - 2, 2);			// Remove ", " from end of string.

			return sbRelayRoutes.ToString();
		}
	}
}
