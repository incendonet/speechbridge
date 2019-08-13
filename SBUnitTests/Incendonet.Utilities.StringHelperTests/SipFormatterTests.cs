// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using Incendonet.Utilities.StringHelper;

namespace Incendonet.Utilities.StringHelperTests
{
    [TestFixture]
    public class SipFormatterTests
    {
        SipFormatter m_sipFormatter = null;

        [SetUp]
        public void Setup()
        {
            m_sipFormatter = new SipFormatter();
        }

        [Test]
        public void TestThatPassingInNullForTheUriReturnsEmptyStringForEachElement()
        {
            string sSipURI = null;
            string sExpectedUsername = "";
            string sExpectedDomain = "";
            string sExpectedPort = "";

            Assert.That(m_sipFormatter.GetUsernameFromUri(sSipURI), Is.EqualTo(sExpectedUsername));
            Assert.That(m_sipFormatter.GetDomainFromUri(sSipURI), Is.EqualTo(sExpectedDomain));
            Assert.That(m_sipFormatter.GetPortFromUri(sSipURI), Is.EqualTo(sExpectedPort));
        }

        [Test]
        public void TestThatPassingInAnEmptyStringForTheUriReturnsEmptyStringForEachElement()
        {
            string sSipURI = "";
            string sExpectedUsername = "";
            string sExpectedDomain = "";
            string sExpectedPort = "";

            Assert.That(m_sipFormatter.GetUsernameFromUri(sSipURI), Is.EqualTo(sExpectedUsername));
            Assert.That(m_sipFormatter.GetDomainFromUri(sSipURI), Is.EqualTo(sExpectedDomain));
            Assert.That(m_sipFormatter.GetPortFromUri(sSipURI), Is.EqualTo(sExpectedPort));
        }

        [Test]
        public void TestThatPassingInANonUriStringForTheUriReturnsEmptyStringForEachElement()
        {
            string sSipURI = "Mary had a little lamb.";
            string sExpectedUsername = "";
            string sExpectedDomain = "";
            string sExpectedPort = "";

            Assert.That(m_sipFormatter.GetUsernameFromUri(sSipURI), Is.EqualTo(sExpectedUsername));
            Assert.That(m_sipFormatter.GetDomainFromUri(sSipURI), Is.EqualTo(sExpectedDomain));
            Assert.That(m_sipFormatter.GetPortFromUri(sSipURI), Is.EqualTo(sExpectedPort));
        }

        [Test]
        public void TestThatUsernameIsExtracted()
        {
            string sSipURI = "7220@192.168.1.220:5060";
            string sExpectedUsername = "7220";

            Assert.That(m_sipFormatter.GetUsernameFromUri(sSipURI), Is.EqualTo(sExpectedUsername));
        }

        [Test]
        public void TestThatDomanIsExtracted()
        {
            string sSipURI = "7220@192.168.1.220:5060";
            string sExpectedDomain = "192.168.1.220";

            Assert.That(m_sipFormatter.GetDomainFromUri(sSipURI), Is.EqualTo(sExpectedDomain));
        }

        [Test]
        public void TestThatPortIsExtracted()
        {
            string sSipURI = "7220@192.168.1.220:5060";
            string sExpectedPort = "5060";

            Assert.That(m_sipFormatter.GetPortFromUri(sSipURI), Is.EqualTo(sExpectedPort));
        }
    }
}
