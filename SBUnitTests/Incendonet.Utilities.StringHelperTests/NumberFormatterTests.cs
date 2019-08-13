// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using Incendonet.Utilities.StringHelper;

namespace Incendonet.Utilities.StringHelperTests
{
    [TestFixture]
    public class NumberFormatterTests
    {
        NumberFormatter m_numberFormatter = null;

        [SetUp]
        public void Setup()
        {
            m_numberFormatter = new NumberFormatter();
        }

        [Test]
        public void TestThatSpacifyPhoneNumberOnlyReturnsEmptyStringForNull()
        {
            string sOriginalString = null;
            string sExpectedString = "";

            Assert.That(m_numberFormatter.SpacifyPhoneNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyPhoneNumberReturnsEmptyStringForEmptyString()
        {
            string sOriginalString = "";
            string sExpectedString = "";

            Assert.That(m_numberFormatter.SpacifyPhoneNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyPhoneNumberOnlyPutsSpacesIntoNonUSNumbers()
        {
            string sOriginalString = "234567";
            string sExpectedString = "2 3 4 5 6 7";

            Assert.That(m_numberFormatter.SpacifyPhoneNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyPhoneNumberPutsSpacesAndCommasInto7DigitNumbers()
        {
            string sOriginalString = "5551212";
            string sExpectedString = "5 5 5, 1 2 1 2";

            Assert.That(m_numberFormatter.SpacifyPhoneNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyPhoneNumberPutsSpacesAndCommasInto10DigitNumbers()
        {
            string sOriginalString = "1234567890";
            string sExpectedString = "1 2 3, 4 5 6, 7 8 9 0";

            Assert.That(m_numberFormatter.SpacifyPhoneNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyPhoneNumberReturnsEmptyStringForAnInputContainingOnlySpaces()
        {
            string sOriginalString = "   ";
            string sExpectedString = "";

            Assert.That(m_numberFormatter.SpacifyPhoneNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberOnlyReturnsEmptyStringForNull()
        {
            string sOriginalString = null;
            string sExpectedString = "";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberReturnsEmptyStringForEmptyString()
        {
            string sOriginalString = "";
            string sExpectedString = "";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberPutsSpacesAndCommasInTheCorrectPlaceForDiscoverMastercardAndVisa()
        {
            string sOriginalString = "0123456789078136";
            string sExpectedString = "0 1 2 3, 4 5 6 7, 8 9 0 7, 8 1 3 6";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberPutsSpacesAndCommasInTheCorrectPlaceForAmericanExpress()
        {
            string sOriginalString = "123456789012345";
            string sExpectedString = "1 2 3 4, 5 6 7 8 9 0, 1 2 3 4 5";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberPutsSpacesAndCommasInTheCorrectPlaceForDinersClub()
        {
            string sOriginalString = "12345678901234";
            string sExpectedString = "1 2 3 4, 5 6 7 8 9 0, 1 2 3 4";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberOnlyPutsSpacesIntoNumbersThatDoNotMatchKnownCreditCardFormat()
        {
            string sOriginalString = "1234567890";
            string sExpectedString = "1 2 3 4 5 6 7 8 9 0";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyCreditCardNumberReturnsEmptyStringForAnInputContainingOnlySpaces()
        {
            string sOriginalString = "   ";
            string sExpectedString = "";

            Assert.That(m_numberFormatter.SpacifyCreditCardNumber(sOriginalString), Is.EqualTo(sExpectedString));
        }
    }
}
