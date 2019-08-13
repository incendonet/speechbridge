// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using DialogModel;
using Incendonet.Utilities.LogClient;

namespace DialogMgrTests
{
	[TestFixture]
	public class DialogEngineTest
	{
		private DialogEngine.DialogEngine m_oDE = null;
		private LegacyLogger m_Log = null;

		[SetUp]
		public void Setup()
		{
			m_Log = new LegacyLogger();
			m_Log.Init("", "", "", "", "", "");
			m_Log.Open();

			m_oDE = new DialogEngine.DialogEngine(m_Log, 0);
		} // Setup

		[TearDown]
		public void Teardown()
		{
			m_oDE = null;
			m_Log.Close();
			m_Log = null;
		} // Teardown

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Test]
		public void TestThatFindLastVariablenameInStringReturnsEmptyStringIfNoVariableInInputString()
		{
			string sInputStringWithNoVariable = "This string contains no variable.";
			string sVariableName = null;
			int iRes = -1;

			iRes = m_oDE.FindLastVariablenameInString(sInputStringWithNoVariable, out sVariableName);

			Assert.That(sVariableName, Is.EqualTo(""));
			Assert.That(iRes, Is.EqualTo(-1));
		}

		[Test]
		public void Test_FindLastVariablenameInString()
		{
			string					sOnlyVarTest = "^First";
			string					sVarPeriodTest = "^First.";
			string					sVarCommaTest = "^First,";
			string					sVarMultiTest = "^First.p2";
			string					sLongTest = "I drive a ^car1, but I'm saving up for a ^car2.";
			int						iRes = -1;
			string					sRes = "";

			iRes = m_oDE.FindLastVariablenameInString(sOnlyVarTest, out sRes);
			Assert.That(sRes == "First");
			Assert.That(iRes, Is.EqualTo(1));

			iRes = m_oDE.FindLastVariablenameInString(sVarPeriodTest, out sRes);
			Assert.That(sRes == "First");
			Assert.That(iRes, Is.EqualTo(1));

			iRes = m_oDE.FindLastVariablenameInString(sVarCommaTest, out sRes);
			Assert.That(sRes == "First");
			Assert.That(iRes, Is.EqualTo(1));

			iRes = m_oDE.FindLastVariablenameInString(sVarMultiTest, out sRes);
			Assert.That(sRes == "p2");
			Assert.That(iRes, Is.EqualTo(1));

			iRes = m_oDE.FindLastVariablenameInString(sLongTest, out sRes);
			Assert.That(sRes == "car2");
			Assert.That(iRes, Is.EqualTo(42));
		} // Test_FindLastVariablenameInString

		[Test]
		public void Test_ReplaceVariableWithValue_Field()
		{
			string					sTest = "I drive a ^car1, but I'm saving up for a ^car2.";
			string					sRes = "";
			DField					fieldTmp = null;
			DForm					formTmp = null;

			formTmp = new DForm(null);
			fieldTmp = new DField(formTmp);
			fieldTmp.Variables.Add(new DVariable("car1", "Yugo"));
			fieldTmp.Variables.Add(new DVariable("car2", "Kia"));

			sRes = m_oDE.ReplaceVariablenameWithValue(fieldTmp, sTest);
			Assert.That(sRes == "Kia");
		} // Test_ReplaceVariableWithValue_Field

		[Test]
		public void Test_ReplaceVariableWithValue_Form()
		{
			string					sTest = "I drive a ^car1, but I'm saving up for a ^car2.";
			string					sRes = "";
			DForm					formTmp = null;

			formTmp = new DForm(null);
			formTmp.Variables.Add(new DVariable("car1", "Yugo"));
			formTmp.Variables.Add(new DVariable("car2", "Kia"));

			sRes = m_oDE.ReplaceVariablenameWithValue(formTmp, sTest);
			Assert.That(sRes == "Kia");
		} // Test_ReplaceVariableWithValue_Field

		[Test]
		public void Test_ReplaceAllVarsInPlace()
		{
			string					sTest = "I drive a ^car1, but I'm saving up for a ^car2.";
			string					sRes = "";
			DSession				sesTmp = null;
			DDocument				docTmp = null;
			DForm					formTmp = null;
			DField					fieldTmp = null;

			sesTmp = new DSession(0);
			docTmp = new DDocument(sesTmp);
			formTmp = new DForm(docTmp);
			fieldTmp = new DField(formTmp);
			formTmp.Variables.Add(new DVariable("car1", "Yugo"));
			fieldTmp.Variables.Add(new DVariable("car2", "Kia"));

			sRes = m_oDE.ReplaceAllVarsInPlace(fieldTmp, sTest);
			Assert.That(sRes == "I drive a Yugo, but I'm saving up for a Kia.");
		} // 

        [Test]
        public void TestThatPageOnlyUrlIsBrokenDownCorrectly()
        {
            string sTestUrl = "file:///opt/speechbridge/VoiceDocStore/AAMain.vxml.xml";
            string sExpectedPageName = sTestUrl;
            string sExpectedFormName = "";

            string sPageName = "";
            string sFormName = "";

            m_oDE.GetTargetPageAndForm(sTestUrl, ref sPageName, ref sFormName);

            Assert.That(sPageName, Is.EqualTo(sExpectedPageName));
            Assert.That(sFormName, Is.EqualTo(sExpectedFormName));
        }

        [Test]
        public void TestThatPageAndFormUrlIsBrokenDownCorrectly()
        {
            string sTestUrl = "file:///opt/speechbridge/VoiceDocStore/AAMain.vxml.xml#AAMain";
            string sExpectedPageName = "file:///opt/speechbridge/VoiceDocStore/AAMain.vxml.xml";
            string sExpectedFormName = "AAMain";

            string sPageName = "";
            string sFormName = "";

            m_oDE.GetTargetPageAndForm(sTestUrl, ref sPageName, ref sFormName);

            Assert.That(sPageName, Is.EqualTo(sExpectedPageName));
            Assert.That(sFormName, Is.EqualTo(sExpectedFormName));
        }

        [Test]
        public void TestThatRelativeUrlIsBrokenDownCorrectly()
        {
            string sTestUrl = "#AAMain";
            string sExpectedPageName = "";
            string sExpectedFormName = "AAMain";

            string sPageName = "";
            string sFormName = "";

            m_oDE.GetTargetPageAndForm(sTestUrl, ref sPageName, ref sFormName);

            Assert.That(sPageName, Is.EqualTo(sExpectedPageName));
            Assert.That(sFormName, Is.EqualTo(sExpectedFormName));
        }
    } // DialogEngineTest
} // namespace DialogMgrTests