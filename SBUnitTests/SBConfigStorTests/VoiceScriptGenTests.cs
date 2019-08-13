// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
	[TestFixture]
	public class VoiceScriptGenTests
	{
		private VoiceScriptGen m_voiceXmlTemplateProcessor = null;
		
		[SetUp]
		public void Setup()
		{
			m_voiceXmlTemplateProcessor = new VoiceScriptGen();
		}

		[TearDown]
		public void Teardown()
		{
			m_voiceXmlTemplateProcessor = null;
		}

		[Test]
		public void TestThatUserWithoutNameIsInvalidForAutomatedAttendant()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "";
			user.Ext = "1111";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForAutomatedAttendant(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithoutExtensionIsInvalidForAutomatedAttendant()
		{
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForAutomatedAttendant(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithoutNameAndExtensionIsInvalidForAutomatedAttendant()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "";
			user.Ext = "";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForAutomatedAttendant(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithNameAndExtensionIsValidForAutomatedAttendant()
		{
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "1111";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForAutomatedAttendant(user);

			Assert.That(bIsValid, Is.True);
		}

		[Test]
		public void TestThatUserWithFirstNameOnlyAndExtensionIsValidForAutomatedAttendant()
		{
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "";
			user.Ext = "1111";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForAutomatedAttendant(user);

			Assert.That(bIsValid, Is.True);
		}

		[Test]
		public void TestThatUserWithLastNameOnlyAndExtensionIsValidForAutomatedAttendant()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "Doe";
			user.Ext = "1111";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForAutomatedAttendant(user);

			Assert.That(bIsValid, Is.True);
		}

		[Test]
		public void TestThatUserWithoutNameUsernameAndDomainIsInvalidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "";
			user.Username = "";
			user.Domain = "";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithoutFirstNameUsernameAndDomainIsInvalidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "Doe";
			user.Username = "";
			user.Domain = "";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithoutLastNameUsernameAndDomainIsInvalidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "";
			user.Username = "";
			user.Domain = "";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithoutNameAndDomainIsInvalidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "";
			user.Username = "john";
			user.Domain = "";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithoutNameAndUsernameIsInvalidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "";
			user.Username = "";
			user.Domain = "test.com";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.False);
		}

		[Test]
		public void TestThatUserWithNameUsernameAndDomainIsValidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Username = "john";
			user.Domain = "test.com";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.True);
		}

		[Test]
		public void TestThatUserWithFirstNameUsernameAndDomainIsValidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "";
			user.Username = "john";
			user.Domain = "test.com";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.True);
		}

		[Test]
		public void TestThatUserWithLastNameUsernameAndDomainIsValidForMessaging()
		{
			Users.User user = new Users.User();

			user.FName = "";
			user.LName = "Doe";
			user.Username = "john";
			user.Domain = "test.com";

			bool bIsValid = m_voiceXmlTemplateProcessor.IsValidUserForMessaging(user);

			Assert.That(bIsValid, Is.True);
		}

		[Test]
		public void TestThatGetLastNDigitsReturnsReturnsEmptyStringIfNoDigitsArePassedIn()
		{
			Assert.That(m_voiceXmlTemplateProcessor.GetLastNDigits("", 4), Is.EqualTo(""));
		}

		[Test]
		public void TestThatIfExtensionContainsLessDigitsThanLimitThenAllDigitsAreReturned()
		{
			Assert.That(m_voiceXmlTemplateProcessor.GetLastNDigits("123", 4), Is.EqualTo("123"));
		}

		[Test]
		public void TestThatIfExtensionContainsLimitDigitsThenLimitDigitsAreReturned()
		{
			Assert.That(m_voiceXmlTemplateProcessor.GetLastNDigits("1234", 4), Is.EqualTo("1234"));
		}

		[Test]
		public void TestThatIfExtensionContainsMoreDigitsThanLimitThenOnlyTheLastLimitDigitsAreReturned()
		{
			Assert.That(m_voiceXmlTemplateProcessor.GetLastNDigits("12345", 4), Is.EqualTo("2345"));
		}

		[Test]
		public void ThatThatNonDigitCharactersAreIgnored()
		{
			Assert.That(m_voiceXmlTemplateProcessor.GetLastNDigits("(555) 123-4567", 7), Is.EqualTo("1234567"));
		}

		[Test]
		public void TestThatEmailServerMacroIsExpandedCorrectly()
		{
			string sMacro = "<EMAIL_SERVER/>";
			string sMacroExpanded = String.Format("\t<var name = \"sEmailServer\" expr = \"\"/>{0}", Environment.NewLine);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCollaborationNamesListMacroIsExpandedCorrectly()
		{
			string sMacro = "<NAMES_EMAIL_LIST/>";
			string sMacroExpanded = String.Format("\t\t\t<option>John Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Username = "john";
			user.Domain = "test.com";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCollaborationNamesListMacroOnlyProcessesUsersThatHaveCollaborationCapability()
		{
			string sMacro = "<NAMES_EMAIL_LIST/>";
			string sMacroExpanded = String.Format("\t\t\t<option>Jane Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Username = "jane";
			user.Domain = "test.com";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCollaborationNamesListMacroWorksForAliases()
		{
			string sMacro = "<NAMES_EMAIL_LIST/>";
			string sMacroExpanded = String.Format("\t\t\t<option>John Doe</option>{0}\t\t\t<option>Jack Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Username = "John";
			user.Domain = "test.com";
			user.AltPronunciations = "Jack Doe;";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatEmailAccountInfoMacroIsExpandedCorrectly()
		{
			string sMacro = "<NAMES_EMAIL_ACCT_INFO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond = \"sCallerName == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sUsername = \"john\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sDomain = \"test.com\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Username = "john";
			user.Domain = "test.com";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatEmailAccountInfoMacroOnlyContainsUsersThatHaveCollaborationCapability()
		{
			string sMacro = "<NAMES_EMAIL_ACCT_INFO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond = \"sCallerName == 'Jane Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sUsername = \"jane\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sDomain = \"test.com\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JaneDoe.wav\">Jane Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Username = "jane";
			user.Domain = "test.com";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatEmailAccountInfoMacroIsExpandedCorrectlyForAliases()
		{
			string sMacro = "<NAMES_EMAIL_ACCT_INFO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond = \"sCallerName == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sUsername = \"john\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sDomain = \"test.com\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond = \"sCallerName == 'Jack Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sUsername = \"john\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tdocument.sDomain = \"test.com\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
            sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">Jack Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Username = "john";
			user.Domain = "test.com";
			user.AltPronunciations = "Jack Doe;";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesListMacroIsExpandedCorrectly()
		{
			string sMacro = "<NAMES_LIST/>";
			string sMacroExpanded = String.Format("\t\t\t<option>John Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatNamesListMacroOnlyProcessesUsersThatHaveAutomatedAttendantCapability()
		{
			string sMacro = "<NAMES_LIST/>";
			string sMacroExpanded = String.Format("\t\t\t<option>Jane Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatNamesListMacroWorksForAliases()
		{
			string sMacro = "<NAMES_LIST/>";
			string sMacroExpanded = String.Format("\t\t\t<option>John Doe</option>{0}\t\t\t<option>Jack Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";
			user.AltPronunciations = "Jack Doe;";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatNamesListMacroIsExpandedCorrectlyForAlternateNumbers()
		{
			string sMacro = "<NAMES_LIST/>";
			StringBuilder sbMacroExpanded = new StringBuilder();
			
			sbMacroExpanded.AppendFormat("\t\t\t<option>John Doe</option>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<option>John Doe mobile</option>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<option>John Doe cell</option>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<option>John Doe pager</option>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<option>page John Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";
			user.MobileNumber = "5551213";
			user.PagerNumber = "5551214";

			users.Add(user);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToMacroIsExpandedCorrectly()
		{
			string sMacro = "<NAMES_TRANSFERTO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:1212\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToMacroOnlyProcessesUsersThatHaveAutomatedAttendantCapability()
		{
			string sMacro = "<NAMES_TRANSFERTO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'Jane Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:1212\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JaneDoe.wav\">Jane Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToMacroWorksForAliases()
		{
			string sMacro = "<NAMES_TRANSFERTO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:1212\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'Jack Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:1212\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
            sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">Jack Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";
			user.AltPronunciations = "Jack Doe;";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToMacroIsExpandedCorrectlyForAlternateNumbers()
		{
			string sMacro = "<NAMES_TRANSFERTO/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:1212\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe mobile'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:5551213\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoemobile.wav\">John Doe mobile.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe cell'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:5551213\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoecell.wav\">John Doe cell.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe pager'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:5551214\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoepager.wav\">John Doe pager.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'page John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:5551214\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/pageJohnDoe.wav\">page John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";
			user.MobileNumber = "5551213";
			user.PagerNumber = "5551214";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatOperatorMaxRetryMacroIsExpandedCorrectly()
		{
			string sMacro = "<SA_OPERATOREXT_MAXRETRIES/>";
			string sMacroExpanded = String.Format("\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.OperatorExtension = "5020";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatOperatorDtmfMacroIsExpandedCorrectly()
		{
			string sMacro = "<SA_OPERATOREXT_DTMF/>";
			string sMacroExpanded = String.Format("\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.OperatorExtension = "5020";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatIntroBargeinMacroIsExpandedCorrectlyWhenEnabled()
		{
			string sMacro = "<SA_INTRO_BARGEIN/>";
			string sMacroExpanded = String.Format("\t\t\t<prompt bargein=\"true\">{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.IntroBargein = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatIntroBargeinMacroIsExpandedCorrectlyWhenDisabled()
		{
			string sMacro = "<SA_INTRO_BARGEIN/>";
			string sMacroExpanded = String.Format("\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.IntroBargein = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCustomCommandsListMacroIsExpandedCorrectly()
		{
			string sMacro = "<CUSTOMCOMMANDS_LIST/>";
			string sMacroExpanded = "";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCustomCommandsDefinitionMacroIsExpandedCorrectly()
		{
			string sMacro = "<CUSTOMCOMMANDS_DEFINITIONS/>";
			string sMacroExpanded = "";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatMoreOptionsCommandMacroIsExpandedCorrectlyWhenEnabled()
		{
			string sMacro = "<MOREOPTIONS_COMMAND/>";
			string sMacroExpanded = String.Format("\t\t\t<option>more options</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.MoreOptionsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatMoreOptionsCommandMacroIsExpandedCorrectlyWhenDisabled()
		{
			string sMacro = "<MOREOPTIONS_COMMAND/>";
			string sMacroExpanded = "";

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.MoreOptionsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatMoreOptionsSpeechMacroIsExpandedCorrectlyWhenEnabled()
		{
			string sMacro = "<MOREOPTIONS_SPEECH/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'more options'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MoreOptions.wav\">more options.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<goto next=\"file:///opt/speechbridge/VoiceDocStore/AAMainMoreOptions.vxml.xml\"/>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.MoreOptionsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatMoreOptionsSpeechMacroIsExpandedCorrectlyWhenDisabled()
		{
			string sMacro = "<MOREOPTIONS_SPEECH/>";
			string sMacroExpanded = "";

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.MoreOptionsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatMoreOptionsGotoMacroIsExpandedCorrectlyWhenEnabled()
		{
			string sMacro = "<MOREOPTIONS_GOTO DISABLED_URL=\"#AAMain\"/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MoreOptions.wav\">more options.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"file:///opt/speechbridge/VoiceDocStore/AAMainMoreOptions.vxml.xml\"/>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.MoreOptionsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatMoreOptionsGotoMacroIsExpandedCorrectlyWhenDisabled()
		{
			string sMacro = "<MOREOPTIONS_GOTO DISABLED_URL=\"#AAMain\"/>";
			string sMacroExpanded = String.Format("\t\t\t\t\t\t<goto next=\"#AAMain\"/>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.MoreOptionsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatNamesTransferToWithConfirmationMacroIsExpandedCorrectly()
		{
			string sMacro = "<NAMES_TRANSFERTO_WITH_CONFIRMATION/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"1212\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;
			vxmlGenerationParameters.ConfirmationCutoffPercentage = 85;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToWithConfirmationMacroOnlyProcessesUsersThatHaveAutomatedAttendantCapability()
		{
			string sMacro = "<NAMES_TRANSFERTO_WITH_CONFIRMATION/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'Jane Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"1212\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JaneDoe.wav\">Jane Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JaneDoe.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;
			vxmlGenerationParameters.ConfirmationCutoffPercentage = 85;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToWithConfirmationMacroWorksForAliases()
		{
			string sMacro = "<NAMES_TRANSFERTO_WITH_CONFIRMATION/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"1212\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'Jack Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"1212\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
            sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">Jack Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
            sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";
			user.AltPronunciations = "Jack Doe;";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;
			vxmlGenerationParameters.ConfirmationCutoffPercentage = 85;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNamesTransferToWithConfirmationMacroIsExpandedCorrectlyForAlternateNumbers()
		{
			string sMacro = "<NAMES_TRANSFERTO_WITH_CONFIRMATION/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"1212\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\">John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoe.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe mobile'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"5551213\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoemobile.wav\">John Doe mobile.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoemobile.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe cell'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"5551213\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoecell.wav\">John Doe cell.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoecell.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'John Doe pager'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"5551214\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoepager.wav\">John Doe pager.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/JohnDoepager.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'page John Doe'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"5551214\");{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '85'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/pageJohnDoe.wav\">page John Doe.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/pageJohnDoe.wav\";{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";
			user.MobileNumber = "5551213";
			user.PagerNumber = "5551214";

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfDigitsForExtension = 4;
			vxmlGenerationParameters.ConfirmationCutoffPercentage = 85;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatConfirmationVariablesMacroIsExpandedCorrectly()
		{
			string sMacro = "<CONFIRMATION_VARIABLES/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t<var name = \"oSAUtils\" expr = \"\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<var name = \"iSAConf\" expr = \"0\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<var name = \"sSAUtt\" expr = \"0\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<var name = \"sSADest\" expr = \"0\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<var name = \"sSANameWav\" expr = \"0\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<var name = \"iNumConfirmMisses\" expr = \"0\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<var name = \"iNumConfirmRetries\" expr = \"0\"/>{0}", Environment.NewLine);

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, null);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatConfirmationFormMacroIsExpandedCorrectly()
		{
			string sMacro = "<CONFIRMATION_FORM TRY_AGAIN_URL=\"#AAMain\"/>";
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t<form id = \"AAConfirm\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t<block>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- Form intro prompt																		-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<prompt bargein=\"true\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<audio expr=\"sSANameWav\"><value expr=\"sSAUtt\"/></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/IsThatCorrect.wav\">Is that correct?</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t</block>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t<field name=\"confirmation\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- Field options																			-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<grammar type=\"application/srgs\" src=\"file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram\" />{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- Field responses																		-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<filled>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<if cond=\"confirmation == 'true'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"confirmation == 'false'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond = \"iNumConfirmRetries == '3'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmRetries = iNumConfirmRetries + 1;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<goto next=\"#AAMain\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond=\"confirmation == 'main menu'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav\">Main menu.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<goto next=\"#AAMain\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond = \"confirmation$.utterance == '1'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<elseif cond = \"confirmation$.utterance == '2'\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond = \"iNumConfirmRetries == '3'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmRetries = iNumConfirmRetries + 1;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<goto next=\"#AAMain\"/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<!-- Unrecognized handler																	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<if cond = \"iNumConfirmMisses == '3'\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/ImSorryWasThat.wav\">I'm sorry, was that</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio expr=\"sSANameWav\"><value expr=\"sSAUtt\"/></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t// This script element is used to allow a second prompt element to flip barge-in behavior.{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"true\">{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/PleaseSayYesOrNo.wav\">Please say, yes, or, no.</audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\"></audio>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = iNumConfirmMisses + 1;{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t\t</if>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t</filled>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t</field>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t</form>{0}", Environment.NewLine);


			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfRetries = 3;
			vxmlGenerationParameters.OperatorExtension = "5020";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatLegacyConditionalMaxRetryMacroIsExpandedCorrectly()
		{
			string sMacro = String.Format("<if cond = \"iNumMisses == '2'\">{0}<transfer dest=\"sip:0\">", Environment.NewLine);
			string sMacroExpanded = String.Format("\t\t\t\t\t<if cond = \"iNumMisses == '3'\">{0}\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfRetries = 3;
			vxmlGenerationParameters.OperatorExtension = "5020";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatConditionalMaxRetryMacroIsExpandedCorrectly()
		{
			string sMacro = String.Format("<if cond = \"iNumMisses == '2'\">{0}<SA_OPERATOREXT_MAXRETRIES/>", Environment.NewLine);
			string sMacroExpanded = String.Format("\t\t\t\t\t<if cond = \"iNumMisses == '3'\">{0}\t\t\t\t\t\t<transfer dest=\"sip:5020\">{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.NumberOfRetries = 3;
			vxmlGenerationParameters.OperatorExtension = "5020";

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatReadEmailMacroIsExpandedCorrectlyWhenCollaborationIsEnabled()
		{
			string sMacro = "<option>read email</option>";
			string sMacroExpanded = String.Format("\t\t\t<option>read email</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatReadEmailMacroIsExpandedCorrectlyWhenCollaborationIsDisabled()
		{
			string sMacro = "<option>read email</option>";
			string sMacroExpanded = String.Format("<!--\t\t\t<option>read email</option>\t-->{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGetEmailMacroIsExpandedCorrectlyWhenCollaborationIsEnabled()
		{
			string sMacro = "<option>get email</option>";
			string sMacroExpanded = String.Format("\t\t\t<option>get email</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGetEmailMacroIsExpandedCorrectlyWhenCollaborationIsDisabled()
		{
			string sMacro = "<option>get email</option>";
			string sMacroExpanded = String.Format("<!--\t\t\t<option>get email</option>\t-->{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCheckEmailMacroIsExpandedCorrectlyWhenCollaborationIsEnabled()
		{
			string sMacro = "<option>check email</option>";
			string sMacroExpanded = String.Format("\t\t\t<option>check email</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCheckEmailMacroIsExpandedCorrectlyWhenCollaborationIsDisabled()
		{
			string sMacro = "<option>check email</option>";
			string sMacroExpanded = String.Format("<!--\t\t\t<option>check email</option>\t-->{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGoToEmailMacroIsExpandedCorrectlyWhenCollaborationIsEnabled()
		{
			string sMacro = "<option>go to email</option>";
			string sMacroExpanded = String.Format("\t\t\t<option>go to email</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGoToEmailMacroIsExpandedCorrectlyWhenCollaborationIsDisabled()
		{
			string sMacro = "<option>go to email</option>";
			string sMacroExpanded = String.Format("<!--\t\t\t<option>go to email</option>\t-->{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCheckCalendarMacroIsExpandedCorrectlyWhenCollaborationIsEnabled()
		{
			string sMacro = "<option>check calendar</option>";
			string sMacroExpanded = String.Format("\t\t\t<option>check calendar</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatCheckCalendarMacroIsExpandedCorrectlyWhenCollaborationIsDisabled()
		{
			string sMacro = "<option>check calendar</option>";
			string sMacroExpanded = String.Format("<!--\t\t\t<option>check calendar</option>\t-->{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGoToCalendarMacroIsExpandedCorrectlyWhenCollaborationIsEnabled()
		{
			string sMacro = "<option>go to calendar</option>";
			string sMacroExpanded = String.Format("\t\t\t<option>go to calendar</option>{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGoToCalendarMacroIsExpandedCorrectlyWhenCollaborationIsDisabled()
		{
			string sMacro = "<option>go to calendar</option>";
			string sMacroExpanded = String.Format("<!--\t\t\t<option>go to calendar</option>\t-->{0}", Environment.NewLine);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.CollaborationCommandsEnabled = false;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, null, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sMacroExpanded));
			}
		}

		[Test]
		public void TestThatGroupsMacroSetToAllIncludesAllContacts()
		{
			string sMacro = String.Format("<GROUPS NAME=\"All\" />{0}<NAMES_LIST/>", Environment.NewLine);
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t<option>John Doe</option>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<option>Jane Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Ext = "5553434";

			user.Groups = new List<string>();
			user.Groups.Add("Sales");

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.IsGroupsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatGroupsMacroOnlyIncludesContactsInSpecifiedGroup()
		{
			string sMacro = String.Format("<GROUPS NAME=\"Sales\" />{0}<NAMES_LIST/>", Environment.NewLine);
			StringBuilder sbMacroExpanded = new StringBuilder();

			sbMacroExpanded.AppendFormat("\t\t\t<option>Jane Doe</option>{0}", Environment.NewLine);
			sbMacroExpanded.AppendFormat("\t\t\t<option>Eve Doe</option>{0}", Environment.NewLine);

			Users users = new Users();
			Users.User user = new Users.User();

			user.FName = "John";
			user.LName = "Doe";
			user.Ext = "5551212";

			users.Add(user);

			user = new Users.User();

			user.FName = "Jane";
			user.LName = "Doe";
			user.Ext = "5553434";

			user.Groups.Add("Sales");

			users.Add(user);

			user = new Users.User();

			user.FName = "Adam";
			user.LName = "Doe";
			user.Ext = "5551111";

			user.Groups.Add("Marketing");

			users.Add(user);

			user = new Users.User();

			user.FName = "Eve";
			user.LName = "Doe";
			user.Ext = "5552222";

			user.Groups.Add("Marketing");
			user.Groups.Add("Sales");

			users.Add(user);

			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
			vxmlGenerationParameters.IsGroupsEnabled = true;

			using (StringReader srTemplate = new StringReader(sMacro))
			using (StringWriter swFinal = new StringWriter())
			{
				m_voiceXmlTemplateProcessor.GenerateVxmlFromTemplate(srTemplate, swFinal, users, vxmlGenerationParameters);

				Assert.That(swFinal.ToString(), Is.EqualTo(sbMacroExpanded.ToString()));
			}
		}

		[Test]
		public void TestThatNameIsMadeSafeForVXMLComparison()
		{
			string sNameInDirectory = "John O'Conner-O'Malley";
			string sExpectedNameUsedInVXML = "John OConner-OMalley";

			string sMatchingNameWithoutApostrophes = VoiceScriptGen.GetVxmlComparisonSafeName(sNameInDirectory);

			Assert.That(sMatchingNameWithoutApostrophes, Is.EqualTo(sExpectedNameUsedInVXML));
		}

		[Test]
		public void TestThatNameIsMadeSafeForWAVFileName()
		{
			string sNameInDirectory = "John O'Conner-O'Malley";
			string sExpectedNameUsedForWAVFile = "JohnOConner-OMalley";

			string sFileName = VoiceScriptGen.GetFilenameSafeName(sNameInDirectory);

			Assert.That(sFileName, Is.EqualTo(sExpectedNameUsedForWAVFile));
		}
	}
}
