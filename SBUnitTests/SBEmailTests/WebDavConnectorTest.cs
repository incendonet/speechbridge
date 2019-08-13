// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;

using Incendonet.Utilities.LogClient;
using SBEmail;

namespace SBEmailTests
{
	/// <summary>
	/// Summary description for WebDavTest.
	/// </summary>
	[TestFixture]
	[Explicit("These tests were originally in the SBEmail project.  Their bodies are currently commented out.")]
	public class WebDavConnectorTest
	{
		/*private	ILegacyLogger				m_LogConsole = null;*/

		private void StartLogger()
		{
/*
			m_LogConsole = Logger.CreateConsoleLogger("{timeStamp} [{levelName}][{threadName}]  {msg}");
			m_LogConsole.Open();
			m_LogConsole.Log(Level.Info, "Logger started.");
*/
		}

		private void StopLogger()
		{
 /*
			m_LogConsole.Log(Level.Info, "Logger stopping.");
			m_LogConsole.Close();
			m_LogConsole = null;
*/
		}

		[Test]
		[Explicit("Body is commented out.")]
		public void GetEmailHeaders()
		{
/*
			WebDavConnector			wdc = null;
			SBEmail.eResults		eRes = SBEmail.eResults.OK;
			int						ii, iNumNew = 0, iNumTotal = 0;
			string					sId, sBodyText;

			StartLogger();

			wdc = new WebDavConnector("server=(local);database=dSpeechBridge;uid=;pwd=");
			//eRes = wdc.Init(m_LogConsole, "192.168.1.105", "Inbox", "", "sharris", "");	// Does not work, server name needs to be FQN.
			//eRes = wdc.Init(m_LogConsole, "wsbs01", "Inbox", "", "sharris", "");			// Does not work, needs to have domain specified.
			//eRes = wdc.Init(m_LogConsole, "wsbs01.incendonet.local", "Inbox", "", "sharris", "");	// Does not work, needs to have domain specified.
			//eRes = wdc.Init(m_LogConsole, "wsbs01", "Inbox", "incendonet", "sharris", "");	// Short form of domain name does not work with Exchange 2003 SP2.
			eRes = wdc.Init(m_LogConsole, "wsbs01", "Inbox", "incendonet.local", "sharris", "");	// Works.
eRes = wdc.Init(m_LogConsole, "WS03R2EEEXCHLCS", "Inbox", "contoso", "dmurray", "dmurray");
			eRes = wdc.SetPasscode("123");
			eRes = wdc.SetKey("");
			eRes = wdc.Connect();

			if(eRes != SBEmail.eResults.OK)
			{
				Console.Error.WriteLine("GetEmailHeaders: Connect failed!");
			}
			else
			{
				// Get all headers
				eRes = wdc.GetHeaders(180, "DESC");
				if(eRes != SBEmail.eResults.OK)
				{
					Console.Error.WriteLine("GetEmailHeaders: GetHeaders failed!");
				}
				else
				{
					iNumTotal = wdc.NumTotal;
					iNumNew = wdc.NumNew;
					for(ii = 0; ii < iNumTotal; ii++)
					{
						// "Read" most recent email
						sId = wdc.EmailHeaders[ii].m_sId;
						sBodyText = wdc.GetBodyTextById(sId);

						m_LogConsole.Log(Level.Info, "From: " + wdc.GetProperty(sId, SBEmail.eMsgProperty.FromName.ToString()));
						m_LogConsole.Log(Level.Info, "Date: " + wdc.GetProperty(sId, SBEmail.eMsgProperty.Date.ToString()));
						m_LogConsole.Log(Level.Info, "Subject: " + wdc.GetProperty(sId, SBEmail.eMsgProperty.Subject.ToString()));
						//m_LogConsole.Log(Level.Info, "Read: " + wdc.GetProperty(sId, SBEmail.eMsgProperty..ToString()));
						m_LogConsole.Log(Level.Info, "Text: " + wdc.GetProperty(sId, SBEmail.eMsgProperty.BodyText.ToString()));
					}
				}

				eRes = wdc.Disconnect();
			}

			StopLogger();
*/
		}

		[Test]
		[Explicit("Body is commented out.")]
		public void GetCalendarEntries()
		{
/*
			WebDavConnector			wdc = null;
			SBEmail.eResults		eRes = SBEmail.eResults.OK;
			DateTime				dtStart;
			int						iNumDays = 7;

			StartLogger();

			wdc = new WebDavConnector("server=(local);database=dSpeechBridge;uid=;pwd=");
			eRes = wdc.Init(m_LogConsole, "wsbs01", "Inbox", "incendonet.local", "sharris", "");	// Works.
eRes = wdc.Init(m_LogConsole, "WS03R2EEEXCHLCS", "Inbox", "contoso", "dmurray", "dmurray");
			eRes = wdc.SetPasscode("123");
			eRes = wdc.SetKey("");
			eRes = wdc.Connect();

			if(eRes != SBEmail.eResults.OK)
			{
				Console.Error.WriteLine("GetCalendarEntries: Connect failed!");
			}
			else
			{
				dtStart = DateTime.Now;

				eRes = wdc.GetCalendarEntries(dtStart, iNumDays);

				eRes = wdc.Disconnect();
			}

			StopLogger();
*/
		}

		[Test]
		[Explicit("Body is commented out.")]
		public void GetDirectory()
		{
/*
			LdapDirectory			dir = null;
			SBConfigStor.Users		users = null;

			dir = new LdapDirectory();
			//dir.Init("192.168.1.105", "incendonet", "sharris", "");  // Does not work with SBS2003, need full domain name, error: "Logon failure: unknown user name or bad password"
			//dir.Init("192.168.1.105", "incendonet.local", "sharris", "");	// Error: "Logon failure: unknown user name or bad password"
			//dir.Init("192.168.1.105", "incendonet", "sharris@incendonet.local", "");	// Error: "An operations error occurred"
			dir.Init("192.168.1.105", "incendonet.local", "sharris@incendonet.local", "");	// Works.
			users = dir.GetDirectory();

			if(users != null)
			{
				foreach(SBConfigStor.Users.User user in users)
				{
					Console.WriteLine(user.Username);
				}
			}

*/
			return;
		}
	}
}
