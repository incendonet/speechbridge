// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Threading;

using NUnit.Framework;

using SBEmail;

namespace SBEmailTests
{
    [TestFixture]
    [Explicit("These tests require a working Exchange server (see EmailTestBase for required information).")]
    public class EwsConnectorImplementationTest : EmailTestBase
    {
        IConnector m_connector = null;


        [SetUp]
        public void Setup()
        {
            m_connector = new EwsConnectorImplementation();
            m_connector.Init(m_Logger, m_sExchangeServerAddress, m_sFolder, m_sDomain, m_sUsername, m_bDebugAll);
            m_connector.Connect(m_sPassword);
        }

        [TearDown]
        public void Teardown()
        {
            m_connector.Disconnect();
        }

        [Test]
        public void TestThatNumberOfEmailsIsReturned()
        {
            SBEmailHeaders emailHeaders = new SBEmailHeaders();

            eResults result = m_connector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref emailHeaders);

            Assert.That(result, Is.EqualTo(eResults.OK), "Call to GetHeaders() failed.");
            Assert.That(emailHeaders.NumberOfEntries, Is.GreaterThan(0), "For this test to work there must be at least one e-mail in the '{0}' folder within the last {1} days.\n{2}", m_sFolder, m_iEmailLookbackPeriod, EmailServerInfo());
        }

		[Test]
		public void TestThatEmptyInboxReturnsZeroEmails()
		{
			SBEmailHeaders emailHeaders = new SBEmailHeaders();

			int iEmailLookkbackPeriod = -1;				// This means we are looking for e-mails received tomorrow, which clearly there are none.

			eResults result = m_connector.GetHeaders(iEmailLookkbackPeriod, "ASC", ref emailHeaders);

			Assert.That(result, Is.EqualTo(eResults.OK), "Call to GetHeaders() failed.");
			Assert.That(emailHeaders.NumberOfEntries, Is.EqualTo(0), "Hmm, how is it possible that we found an e-mail in the '{0}' folder that we won't receive until tomorrow?\n{1}", m_sFolder, EmailServerInfo());
		}

        [Test]
        public void TestThatUsingIncorrectPasswordReturnsFail()
        {
            SBEmailHeaders emailHeaders = new SBEmailHeaders();

            IConnector connector = new EwsConnectorImplementation();
            connector.Init(m_Logger, m_sExchangeServerAddress, m_sFolder, m_sDomain, m_sUsername, m_bDebugAll);
            connector.Connect("Wrong Password");
            eResults result = connector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref emailHeaders);

            Assert.That(result, Is.EqualTo(eResults.Fail));
        }

        [Test]
        public void TestThatSetFolderWorks()
        {
            string sFolder = "Sent Items";
            SBEmailHeaders emailHeaders = new SBEmailHeaders();

            m_connector.SetFolder(sFolder);
            m_connector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref emailHeaders);

            Assert.That(emailHeaders.NumberOfEntries, Is.GreaterThan(0), "For this test to work there must be at least one e-mail in the '{0}' folder within the last {1} days.\n{2}", sFolder, m_iEmailLookbackPeriod, EmailServerInfo());

            for (int i = 0; i < emailHeaders.NumberOfEntries; ++i)
            {
                Assert.That(emailHeaders[i].m_sFromName, Is.EqualTo(m_sUserDisplayName));
            }
        }

        [Test]
        [Category("Slow Tests")]
        public void TestThatSentEmailIsPutIntoSentItemsFolder()
        {
            int iEmailLookbackPeriod = 1;
            SBEmailHeaders emailHeaders = new SBEmailHeaders();

            m_connector.SetFolder("Sent Items");
            m_connector.GetHeaders(iEmailLookbackPeriod, "ASC", ref emailHeaders);

            int numberOfEmailsPriorToSend = emailHeaders.NumberOfEntries;

            eResults result = m_connector.SendEmail(m_sUsername, "EWS Unit Test Send", 0, DateTime.Now.ToString(), eEmailBodyType.Text);
            Assert.That(result, Is.EqualTo(eResults.OK), "Call to SendEmail() failed.\n{0}", EmailServerInfo());


            // At times the sent e-mail doesn't show up right away in the Sent Items folder.  
            // To ensure that this doesn't cause the test to fail we'll just have to add a little wait here.

            Thread.Sleep(5000);

            m_connector.GetHeaders(iEmailLookbackPeriod, "ASC", ref emailHeaders);

            int numberOfEmailsAfterSend = emailHeaders.NumberOfEntries;

            Assert.That(numberOfEmailsAfterSend, Is.GreaterThan(numberOfEmailsPriorToSend));
        }

        [Test]
        public void TestThatSortOrderWorks()
        {
            SBEmailHeaders emailHeadersAscending = new SBEmailHeaders();
            SBEmailHeaders emailHeadersDescending = new SBEmailHeaders();

            m_connector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref emailHeadersAscending);
            m_connector.GetHeaders(m_iEmailLookbackPeriod, "DESC", ref emailHeadersDescending);

            Assert.That(emailHeadersAscending.NumberOfEntries, Is.GreaterThan(1), "For this test to work at least two e-mails are required in the '{0}' folder.\n{1}", m_sFolder, EmailServerInfo());
            Assert.That(emailHeadersAscending.NumberOfEntries, Is.EqualTo(emailHeadersDescending.NumberOfEntries), "For this test to work the content of the '{0}' folder is not allowed to change.\n{1}", m_sFolder, EmailServerInfo());

            bool bMismatchFound = false;

            for (int i = 0; i < emailHeadersAscending.NumberOfEntries; ++i)
            {
                int iDescendingIndex = (emailHeadersDescending.NumberOfEntries - 1) - i;
                if (emailHeadersAscending[i].m_sId != emailHeadersDescending[iDescendingIndex].m_sId)
                {
                    bMismatchFound = true;
                    break;
                }
            }

            Assert.That(bMismatchFound, Is.False);
        }

        [Test]
		[Explicit("Not a real test - provided to simplify troubleshooting.")]
		[Category("Troubleshooting")]
        public void ShowAllEmails()
        {
            SBEmailHeaders emailHeaders = new SBEmailHeaders();

            m_connector.GetHeaders(m_iEmailLookbackPeriod, "ASC", ref emailHeaders);

            for (int i = 0; i < emailHeaders.NumberOfEntries; ++i)
            {
                emailHeaders[i].m_sBodyText = m_connector.GetBodyTextById(emailHeaders[i].m_sId);
            }

            Console.WriteLine("Number of e-mails: " + emailHeaders.NumberOfEntries);
            Console.WriteLine("New e-mails      : " + emailHeaders.NumberOfUnreadEntries);

            for (int i = 0; i < emailHeaders.NumberOfEntries; ++i)
            {
                Console.WriteLine("=================================");
                Console.WriteLine("Email     : " + (i + 1));
                Console.WriteLine("From      : " + emailHeaders[i].m_sFromName);
                Console.WriteLine("Date      : " + emailHeaders[i].m_sDate);
                Console.WriteLine("Subject   : " + emailHeaders[i].m_sSubject);
                Console.WriteLine("Importance: " + emailHeaders[i].m_sImportance);
                Console.WriteLine("IsRead    : " + emailHeaders[i].m_bRead);
                Console.WriteLine("Body      : " + emailHeaders[i].m_sBodyText);
                Console.WriteLine("Type      : " + emailHeaders[i].m_Type);
                Console.WriteLine("=================================");
            }
        }

        [Test]
        public void TestThatNumberOfCalendarEntriesIsReturned()
        {
            SBCalendarEntries calendarEntries = new SBCalendarEntries();

            eResults result = m_connector.GetCalendarEntries(DateTime.Now, m_iCalendarRetrievalPeriod, ref calendarEntries);

            Assert.That(result, Is.EqualTo(eResults.OK), "Call to GetCalendarEntries() failed.");
            Assert.That(calendarEntries.Count, Is.GreaterThan(0), "For this test to work there must be at least one calendar entry within the next {0} days.\n{1}", m_iCalendarRetrievalPeriod, EmailServerInfo());
        }

		[Test]
		public void TestThatEmptyCalendarPeriodReturnsZeroCalendarEntries()
		{
			SBCalendarEntries calendarEntries = new SBCalendarEntries();

			DateTime dtStartDate = new DateTime(2012, 5, 24);					//$$$ LP - This date & period were picked since the calendar for the sharris account didn't have any entries.
			int iCalendarRetrievalPeriod = 2;									//$$$ LP - This date & period were picked since the calendar for the sharris account didn't have any entries.

			eResults result = m_connector.GetCalendarEntries(dtStartDate, iCalendarRetrievalPeriod, ref calendarEntries);

			Assert.That(result, Is.EqualTo(eResults.OK), "Call to GetCalendarEntries() failed.");
			Assert.That(calendarEntries.Count, Is.EqualTo(0), "For this test to work there must be no calendar entries between {0} and {1}.\n", dtStartDate, dtStartDate.AddDays(2));		//$$$ LP - Note that the end date displayed is not quite right since the actual end date calculation is more involved.  Good enough for now but that whole calculation should really be refactored out of the connector implementation.
		}

        [Test]
		[Explicit("Not a real test - provided to simplify troubleshooting.")]
		[Category("Troubleshooting")]
		public void ShowAllCalendarEntries()
        {
            SBCalendarEntries calendarEntries = new SBCalendarEntries();

            m_connector.GetCalendarEntries(DateTime.Now, m_iCalendarRetrievalPeriod, ref calendarEntries);

            Console.WriteLine("Number of appointments: " + calendarEntries.Count);

            for (int i = 0; i < calendarEntries.Count; ++i)
            {
                Console.WriteLine("=================================");
                Console.WriteLine("Appointment : " + (i + 1));
                Console.WriteLine("From        : " + calendarEntries[i].m_sFromName);
                Console.WriteLine("Start       : " + calendarEntries[i].m_sStart);
                Console.WriteLine("End         : " + calendarEntries[i].m_sEnd);
                Console.WriteLine("Subject     : " + calendarEntries[i].m_sSubject);
                Console.WriteLine("Location    : " + calendarEntries[i].m_sLocation);
                Console.WriteLine("BusyStatus  : " + calendarEntries[i].m_sBusystatus);
                Console.WriteLine("=================================");
            }
        }

        [Test]
        public void TestThatAcceptingAMeetingRequestAddsTheMeetingIntoTheCalendar()
        {
            eResults result = eResults.OK;

            int iEmailLookbackPeriod = 1;
            int iMeetingTimePeriod = 3;

            string sTestRequirement = String.Format("For this test to work the following have to be true:\n" +
                                                    "     A meeting request with the subject \"EWS Meeting Request Test\" has to be received within the last {0} days.\n" +
                                                    "     The requested meeting has to occur within the next {1} days.\n{2}", iEmailLookbackPeriod, iMeetingTimePeriod, EmailServerInfo());

            SBEmailHeaders emailHeaders = new SBEmailHeaders();

            m_connector.GetHeaders(iEmailLookbackPeriod, "ASC", ref emailHeaders);


            // Look for the meeting request e-mail required by this test.

            bool bMeetingRequestFound = false;
            string sMeetingRequestId = null;

            for (int i = 0; (i < emailHeaders.NumberOfEntries && !bMeetingRequestFound); ++i)
            {
                if (emailHeaders[i].m_Type == eItemType.MeetingRequest)
                {
                    if (emailHeaders[i].m_sSubject.ToLower() == "ews meeting request test")
                    {
                        bMeetingRequestFound = true;
                        sMeetingRequestId = emailHeaders[i].m_sId;
                    }
                }
            }

            Assert.That(bMeetingRequestFound, Is.True, sTestRequirement);

            SBCalendarEntries calendarEntriesInitially = new SBCalendarEntries();

            m_connector.GetCalendarEntries(DateTime.Now, iMeetingTimePeriod, ref calendarEntriesInitially);

            int iNumberOfTentativeMeetingsInitially = 0;
            int iNumberOfAcceptedMeetingsInitially = 0;

            for (int i = 0; i < calendarEntriesInitially.Count; ++i)
            {
                switch (calendarEntriesInitially[i].m_sBusystatus)
                {
                    case "TENTATIVE":
                        ++iNumberOfTentativeMeetingsInitially;
                        break;

                    case "BUSY":
                        ++iNumberOfAcceptedMeetingsInitially;
                        break;
                }
            }

            result = m_connector.SendMeetingRequestResp(sMeetingRequestId, "EWS Meeting Request Reponse Test", eMeetingRequestResp.Accept, true);

            Assert.That(result, Is.EqualTo(eResults.OK), "Call to SendMeetingRequestResp() failed.\n{0}", EmailServerInfo());

            SBCalendarEntries calendarEntriesFinally = new SBCalendarEntries();

            m_connector.GetCalendarEntries(DateTime.Now, iMeetingTimePeriod, ref calendarEntriesFinally);

            int iNumberOfTentativeMeetings = 0;
            int iNumberOfAcceptedMeetings = 0;

            for (int i = 0; i < calendarEntriesFinally.Count; ++i)
            {
                switch (calendarEntriesFinally[i].m_sBusystatus)
                {
                    case "TENTATIVE":
                        ++iNumberOfTentativeMeetings;
                        break;

                    case "BUSY":
                        ++iNumberOfAcceptedMeetings;
                        break;
                }
            }

            Assert.That((iNumberOfAcceptedMeetings - iNumberOfAcceptedMeetingsInitially), Is.EqualTo(1), "Number of accepted meetings hasn't changed as expected");
            Assert.That((iNumberOfTentativeMeetings - iNumberOfTentativeMeetingsInitially), Is.EqualTo(-1), "Number of tentative meetings hasn't changed as expected");
        }
    }
}