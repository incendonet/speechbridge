// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;

using SBEmail;

namespace SBEmailTests
{
	[TestFixture]
    [Explicit("These tests require a working Exchange server (see EmailTestBase for required information).")]
	[Category(".NET Only")]
	public class CompareEwsToWebDavTest : EmailTestBase
	{
		IConnector m_WebDavConnector = null;
		IConnector m_EwsConnector = null;


		[SetUp]
		public void Setup()
		{
			m_WebDavConnector = new WebDavConnectorImplementation(eEmailServerType.Exchange2007WebDAV);
			m_WebDavConnector.Init(m_Logger, m_sExchangeServerAddress, m_sFolder, m_sDomain, m_sUsername, m_bDebugAll);
			m_WebDavConnector.Connect(m_sPassword);

			m_EwsConnector = new EwsConnectorImplementation();
			m_EwsConnector.Init(m_Logger, m_sExchangeServerAddress, m_sFolder, m_sDomain, m_sUsername, m_bDebugAll);
			m_EwsConnector.Connect(m_sPassword);
		}

		[TearDown]
		public void Teardown()
		{
			m_WebDavConnector.Disconnect();
			m_EwsConnector.Disconnect();
		}

		[Test]
        [Explicit("This test fails under Mono with a NullReferenceException in System.Net.WebConnectionGroup.PrepareSharingNtlm.")]
        public void TestThatEwsAndWebDavReturnSameEmails()
		{
			SBEmailHeaders webDavEmailHeaders = new SBEmailHeaders();
			SBEmailHeaders ewsEmailHeaders = new SBEmailHeaders();

			m_EwsConnector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref ewsEmailHeaders);
			m_WebDavConnector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref webDavEmailHeaders);

			Assert.That(ewsEmailHeaders.NumberOfEntries, Is.EqualTo(webDavEmailHeaders.NumberOfEntries), "Total number of e-mails differs.");
			Assert.That(ewsEmailHeaders.NumberOfUnreadEntries, Is.EqualTo(webDavEmailHeaders.NumberOfUnreadEntries), "Number of new e-mails differs.");

			for (int i = 0; i < webDavEmailHeaders.NumberOfEntries; ++i)
			{
				Assert.That(ewsEmailHeaders[i].m_sFromName, Is.EqualTo(webDavEmailHeaders[i].m_sFromName), "From name differs.");
				Assert.That(ewsEmailHeaders[i].m_sSubject, Is.EqualTo(webDavEmailHeaders[i].m_sSubject), "Subject differs.");
				Assert.That(ewsEmailHeaders[i].m_sDate, Is.EqualTo(webDavEmailHeaders[i].m_sDate), "Date differs.");
				Assert.That(ewsEmailHeaders[i].m_sImportance, Is.EqualTo(webDavEmailHeaders[i].m_sImportance), "Importance differs.");
				Assert.That(ewsEmailHeaders[i].m_bRead, Is.EqualTo(webDavEmailHeaders[i].m_bRead), "Read status differs.");
				Assert.That(ewsEmailHeaders[i].m_Type, Is.EqualTo(webDavEmailHeaders[i].m_Type), "Type differs.");
			}
		}

		[Test]
        [Explicit("This test fails under Mono with a NullReferenceException in System.Net.WebConnectionGroup.PrepareSharingNtlm.")]
        public void TestThatEwsAndWebDavReturnSameCalendarItems()
		{
			SBCalendarEntries webDavCalendarEntries = new SBCalendarEntries();
			SBCalendarEntries ewsCalendarEntries = new SBCalendarEntries();

			DateTime dtNow = DateTime.Now;      // Make sure both retrievals start at the same time.

			m_EwsConnector.GetCalendarEntries(dtNow, m_iCalendarRetrievalPeriod, ref ewsCalendarEntries);
			m_WebDavConnector.GetCalendarEntries(dtNow, m_iCalendarRetrievalPeriod, ref webDavCalendarEntries);

			Assert.That(ewsCalendarEntries.Count, Is.EqualTo(webDavCalendarEntries.Count), "Number of calendar entries differ.");

			for (int i = 0; i < webDavCalendarEntries.Count; ++i)
			{
				Assert.That(ewsCalendarEntries[i].m_sFromName, Is.EqualTo(webDavCalendarEntries[i].m_sFromName), "From name differs.");
				Assert.That(ewsCalendarEntries[i].m_sLocation, Is.EqualTo(webDavCalendarEntries[i].m_sLocation), "Location differs.");
				Assert.That(ewsCalendarEntries[i].m_sSubject, Is.EqualTo(webDavCalendarEntries[i].m_sSubject), "Subject differs.");
				Assert.That(ewsCalendarEntries[i].m_sStart, Is.EqualTo(webDavCalendarEntries[i].m_sStart), "Start differs.");
				Assert.That(ewsCalendarEntries[i].m_sEnd, Is.EqualTo(webDavCalendarEntries[i].m_sEnd), "End differs.");
				Assert.That(ewsCalendarEntries[i].m_sBusystatus, Is.EqualTo(webDavCalendarEntries[i].m_sBusystatus), "Busy status differs.");
			}
		}
	}
}
