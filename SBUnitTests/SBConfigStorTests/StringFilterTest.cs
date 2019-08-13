// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
	[TestFixture]
	public class StringFilterTest
	{
		[Test]
		public void TestThatHtmlNbspIsRemoved()
		{
			string sTestString = "&nbsp;";
			string sExpectedString = "";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatMultipleHtmlNbspAreRemoved()
		{
			string sTestString = "&nbsp; &nbsp;&nbsp;";
			string sExpectedString = "";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestReplacementOfAnd()
		{
			string sTestString = "Adam & Eve";
			string sExpectedString = "Adam and Eve";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestReplacementOfMultipleAnd()
		{
			string sTestString = "Going on & on & on";
			string sExpectedString = "Going on and on and on";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringDialOnlyAllowsDigits()
		{
			string sTestString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ (123) 456-7890 abcdefghijklpmnopqrstuvwxyz ~`!@#$%^&*()_-+={[}}\\|:;\"'<,>.?/";
			string sExpectedString = "1234567890";

			Assert.That(StringFilter.GetFilteredStringDial(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringDialCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredStringDial(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringPathLeavesAValidWindowsPathUnchanged()
		{
			string sTestString = @"C:\Program Files\SpeechBridge\VoiceDoc Store\AAMain.xml.vxml";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredStringPath(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringPathLeavesAValidLinuxPathUnchanged()
		{
			string sTestString = "/opt/SpeechBridge/Voice DocStore/AAMain.xml.vxml";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredStringPath(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringPathRemovedIllegalCharacters()
		{
			string sTestString = "`~!@#$%^&*()=+[{]}|;'\",<>?\a\b\t\r\v\f\n\u001B/opt/SpeechBridge`~!@#$%^&*()=+[{]}|;'\",<>?\a\b\t\r\v\f\n\u001B/Voice Doc_Store/AAMain.xml.vxml`~!@#$%^&*()=+[{]}|;'\",<>?\a\b\t\r\v\f\n\u001B";
			string sExpectedString = "/opt/SpeechBridge/Voice Doc_Store/AAMain.xml.vxml";

			Assert.That(StringFilter.GetFilteredStringPath(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringPathCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredStringPath(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringDomainRemovedLeadingAndTrailingHyphensButAllowsHyphenInDomainName()
		{
			string sTestString = "#$%-m-w.c()om&-*(";
			string sExpectedString = "m-w.com";

			Assert.That(StringFilter.GetFilteredStringDomain(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringDomainRemovesIllegalCharacters()
		{
			string sTestString = " `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B-domain-name `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B.com- `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B";
			string sExpectedString = "domain-name.com";

			Assert.That(StringFilter.GetFilteredStringDomain(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringDomainCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredStringDomain(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringUsernameRemoveIllegalCharacters()
		{
			string sTestString = " `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B9test.2 `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B3-D_N_A `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B";
			string sExpectedString = "9test.23-D_N_A";

			Assert.That(StringFilter.GetFilteredStringUsername(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringUsernameRemovesLeadingAndTrailingPeriods()
		{
			string sTestString = "...username.";
			string sExpectedString = "username";

			Assert.That(StringFilter.GetFilteredStringUsername(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringEmailRemovesAllButTheFirstAtSign()
		{
			string sTestString = "name@te@st.co@m@";
			string sExpectedString = "name@test.com";

			Assert.That(StringFilter.GetFilteredStringEmail(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringEmailRemovesIllegalCharacters()
		{
			string sTestString = " `~!#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001Bjohn.doe@-domain-name `~!#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B.com- `~!#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B";
			string sExpectedString = "john.doe@domain-name.com";

			Assert.That(StringFilter.GetFilteredStringEmail(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringEmailCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredStringEmail(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringEmailReturnsAnEmptyStringIfNoAtSymbolFound()
		{
			string sTestString = "This is a test.";
			string sExpectedString = "";

			Assert.That(StringFilter.GetFilteredStringEmail(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringAllowsSpacesInNames()
		{
			string sTestString = "A space";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringAllowsHyphensInNames()
		{
			string sTestString = "Double-Barrel";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringAllowsApostrophesInNames()
		{
			string sTestString = "O'Malley";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringRemovesIllegalCharacters()
		{
			string sTestString = "`~!@#$%^&*()_=+[{]}\\|;:\",<.>/?\a\b\t\r\v\f\n\u001B3`~!@#$%^&*()_=+[{]}\\|;:\",<.>/?\a\b\t\r\v\f\n\u001BM`~!@#$%^&*()_=+[{]}\\|;:\",<.>/?\a\b\t\r\v\f\n\u001B";
			string sExpectedString = "3 M";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringRemovesLeadingAndTrailingSpaces()
		{
			string sTestString = "   John Doe ";
			string sExpectedString = "John Doe";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringReducesConsecutiveSpacesToASingleSpace()
		{
			string sTestString = "Jane    Doe";
			string sExpectedString = "Jane Doe";

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatGetFilteredStringCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.GetFilteredString(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatRemoveSpacesCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.RemoveSpaces(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatRemoveSpacesWorks()
		{
			string sTestString = "This is a test.";
			string sExpectedString = "Thisisatest.";

			Assert.That(StringFilter.RemoveSpaces(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatRemoveApostrophesCanHandleAnEmptyString()
		{
			string sTestString = "";
			string sExpectedString = sTestString;

			Assert.That(StringFilter.RemoveApostrophes(sTestString), Is.EqualTo(sExpectedString));
		}

		[Test]
		public void TestThatRemoveApostrophesWorks()
		{
			string sTestString = "I'd like to help but can't.";
			string sExpectedString = "Id like to help but cant.";

			Assert.That(StringFilter.RemoveApostrophes(sTestString), Is.EqualTo(sExpectedString));
		}


        // Tests for SCR565.
#region SCR565 Tests

        [Test]
        public void TestThatGetFilteredStringReplacesUnderscoreWithSpace()
        {
            string sTestSting = "I_use_underscore_instead_of_spaces";
            string sExpectedString = "I use underscore instead of spaces";

            Assert.That(StringFilter.GetFilteredString(sTestSting), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatGetFilteredStringReplacesMultipleConsecutiveUnderscoresWithASingleSpace()
        {
            string sTestSting = "I__use__underscore__instead____of_________spaces";
            string sExpectedString = "I use underscore instead of spaces";

            Assert.That(StringFilter.GetFilteredString(sTestSting), Is.EqualTo(sExpectedString));
        }

        [Test]
        public void TestThatGetFilteredStringRemovesLeadingAndTrailingUnderscores()
        {
            string sTestSting = "_I use underscore instead of spaces __ ";
            string sExpectedString = "I use underscore instead of spaces";

            Assert.That(StringFilter.GetFilteredString(sTestSting), Is.EqualTo(sExpectedString));
        }

#endregion
	}
}
