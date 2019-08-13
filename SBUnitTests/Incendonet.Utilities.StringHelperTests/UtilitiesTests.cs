// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using System.Collections.Specialized;

namespace Incendonet.Utilities.StringHelperTests
{
    [TestFixture]
    public class UtilitiesTests
    {
        Incendonet.Utilities.StringHelper.Utilities m_utilities = null;

        [SetUp]
        public void Setup()
        {
            m_utilities = new Incendonet.Utilities.StringHelper.Utilities();
        }

        [Test]
        public void TestThatCopyStringWorks()
        {
            string sOriginalString = "Hello World!";
            string sExpectedString = sOriginalString;

            Assert.That(m_utilities.CopyString(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatGetItemsFromStringReturnsNoItemsForAnEmptyString()
        {
            string sOriginalString = "";
            char cSeparator = ',';

            StringCollection results = new StringCollection();

            Incendonet.Utilities.StringHelper.Utilities.GetItemsFromString(sOriginalString, cSeparator, results);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestThatGetItemsFromStringWorksForAStringContainingOneItem()
        {
            string sOriginalString = "Alpha";
            char cSeparator = ',';

            StringCollection results = new StringCollection();

            Incendonet.Utilities.StringHelper.Utilities.GetItemsFromString(sOriginalString, cSeparator, results);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0], Is.EqualTo("Alpha"));
        }

        [Test]
        public void TestThatGetItemsFromStringWorksForAStringContainingMultipleItems()
        {
            string sOriginalString = "Alpha, Beta, Gamma,";
            char cSeparator = ',';

            StringCollection results = new StringCollection();

            Incendonet.Utilities.StringHelper.Utilities.GetItemsFromString(sOriginalString, cSeparator, results);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results[0], Is.EqualTo("Alpha"));
            Assert.That(results[1], Is.EqualTo(" Beta"));
            Assert.That(results[2], Is.EqualTo(" Gamma"));
        }

        [Test]
        public void TestThatSpacifyStringReturnsEmptyStringForNull()
        {
            string sOriginalString = null;
            string sExpectedString = "";

            Assert.That(m_utilities.SpacifyString(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringReturnsEmptyStringForEmptyString()
        {
            string sOriginalString = "";
            string sExpectedString = "";

            Assert.That(m_utilities.SpacifyString(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringWorks()
        {
            string sOriginalString = "iBm";
            string sExpectedString = "I B M";

            Assert.That(m_utilities.SpacifyString(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringWorksForSingleCharacterString()
        {
            string sOriginalString = "i";
            string sExpectedString = "I";

            Assert.That(m_utilities.SpacifyString(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringSlowReturnsEmptyStringForNull()
        {
            string sOriginalString = null;
            string sExpectedString = "";

            Assert.That(m_utilities.SpacifyStringSlow(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringSlowReturnsEmptyStringForEmptyString()
        {
            string sOriginalString = "";
            string sExpectedString = "";

            Assert.That(m_utilities.SpacifyStringSlow(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringSlowWorks()
        {
            string sOriginalString = "iBm";
            string sExpectedString = "I, B, M";

            Assert.That(m_utilities.SpacifyStringSlow(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatSpacifyStringSlowWorksForSingleCharacterString()
        {
            string sOriginalString = "i";
            string sExpectedString = "I";

            Assert.That(m_utilities.SpacifyStringSlow(sOriginalString), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatCorrectLengthReturnsFalseIfStringIsNull()
        {
            string sOriginalString = null;
            int iDummyLength = 0;

            Assert.That(m_utilities.CorrectLength(sOriginalString, iDummyLength), Is.False);
        }

        [Test]
        public void TestThatCorrectLengthReturnsTrueIfLengthIsAsSpecified()
        {
            string sOriginalString = "Hello World!";
            string sExpectedLength = sOriginalString.Length.ToString();

            Assert.That(m_utilities.CorrectLength(sOriginalString, sExpectedLength), Is.True);
        }

        [Test]
        public void TestThatCorrectLengthReturnsFalseIfLengthIsNotAsSpecified()
        {
            string sOriginalString = "Hello World!";
            int iIncorrectLength = sOriginalString.Length + 1;

            Assert.That(m_utilities.CorrectLength(sOriginalString, iIncorrectLength), Is.False);
        }
    }
}
