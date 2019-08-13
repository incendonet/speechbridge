// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using AppGenerator;
using SBConfigStor;


namespace AppGeneratorTests
{
	[TestFixture]
	public class DialogBuilderTest
	{
		private readonly char[] m_cDtmfKeys = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '*', '#'};
		private const string m_csTestFilename = "TestFile.vxml.xml";

		[TearDown]
		public void Teardown()
		{
            File.Delete(m_csTestFilename);
		}

		[Test]
		public void TestEmptyMenu()
		{
			string[] sExpectedFormElement = { "<form id=\"EmptyMenu\">" };
			string[] sExpectedGrammarElement = { "<grammar type=\"application/srgs\" src=\"file:///opt/speechbridge/VoiceDocStore/ABNFDigits.gram\" />" };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "EmptyMenu";

			Commands commands = new Commands();

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFormElement), "TestFile doesn't contain expected FORM element.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedGrammarElement), "TestFile doesn't contain expected GRAMMAR element.");
		}

		[Test]
		public void TestEmptyMenuEnglish()
		{
			string[] sExpectedVxmlElement = { "<vxml version=\"2.0\" lang=\"en-US\">" };

			Menus.Menu menu = new Menus.Menu();

			menu.LanguageCode = "en-US";

			Commands commands = new Commands();

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedVxmlElement), "TestFile doesn't contain expected VXML element.");
		}

		[Test]
		public void TestEmptyMenuSpanish()
		{
			string[] sExpectedVxmlElement = { "<vxml version=\"2.0\" lang=\"es-MX\">" };

			Menus.Menu menu = new Menus.Menu();

			menu.LanguageCode = "es-MX";

			Commands commands = new Commands();

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedVxmlElement), "TestFile doesn't contain expected VXML element.");
		}

		[Test]
		public void TestMenuWithAllCommandsDoNothing()
		{
			string[] sExpectedLines = { 
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <goto next=\"#MenuWithAllCommandsDoNothing\" />",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
									  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithAllCommandsDoNothing";

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.DoNothing));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithAllCommandsRepeat()
		{
			string[] sExpectedLines = { 
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <audio src=\"0.wav\">Pressed 0</audio>",
										"    <goto next=\"#MenuWithAllCommandsRepeat\" />",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
									  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithAllCommandsRepeat";

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Repeat));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithAllCommandsGotoMenu()
		{
			string[] sExpectedLines = { 
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <audio src=\"0.wav\">Pressed 0</audio>",
										"    <goto next=\"file:///opt/speechbridge/VoiceDocStore/0.vxml.xml\" />",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
									  };

			Menus.Menu menu = new Menus.Menu();
			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.GotoMenu));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithAllCommandsTransfer()
		{
			string[] sExpectedLines = { 
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <transfer dest=\"sip:0\">",
										"      <prompt>",
										"        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">one moment please</audio>",
										"      </prompt>",
										"    </transfer>",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
									  };

			Menus.Menu menu = new Menus.Menu();
			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Transfer));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithAllCommandsCodeBlock()
		{
			string[] sExpectedLines = { 
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <script>",
										"      value = 0;",
										"    </script>",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
									  };

			Menus.Menu menu = new Menus.Menu();
			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.CodeBlock));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

        [Test]
        public void TestMenuWithAllCommandsHangup()
        {
            string [] sExpectedLines = {
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <audio src=\"0.wav\">Pressed 0</audio>",
										"    <exit />",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
                                       };

            Menus.Menu menu = new Menus.Menu();

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Hangup));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
        }

        [Test]
        public void TestMenuWithAllCommandsPrompt()
        {
            string[] sExpectedLines = {
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <audio src=\"0.wav\">Pressed 0</audio>",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
                                       };

            Menus.Menu menu = new Menus.Menu();

            Commands commands = new Commands();

            foreach (char c in m_cDtmfKeys)
            {
                commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Prompt));
            }

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
        }

        [Test]
//		[ExpectedException(typeof(ArgumentNullException), UserMessage = "i_DtmfToSpokenMapping")]
		public void TestThatArgumentNullExceptionIsThrownIfNoDtmfToSpokenMappingIsProvidedWhenDtmfCanBeSpoken()
		{
			Menus.Menu menu = new Menus.Menu();
			Commands commands = new Commands();

			menu.DtmfCanBeSpoken = true;

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
		}

		[Test]
//		[ExpectedException(typeof(ArgumentOutOfRangeException), UserMessage = "i_DtmfToSpokenMapping")]
		public void TestThatArgumentOutOfRangeExceptionIsThrownIfEmptyDtmfToSpokenMappingIsProvidedWhenDtmfCanBeSpoken()
		{
			Menus.Menu menu = new Menus.Menu();
			Commands commands = new Commands();

			menu.DtmfCanBeSpoken = true;

			DtmfKeyToSpokenEquivalentMappings dtmfMapping = new DtmfKeyToSpokenEquivalentMappings();

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, dtmfMapping, null);
		}

		[Test]
		public void TestThatMenuWithDtmfCanBeSpokenEnabledContainsOptionsElements()
		{
			string[] sExpectedLines = { 
										"<option>zero</option>",
										"<option>null</option>",
										"<option>nil</option>",
										"<option>one</option>",
										"<option>two</option>",
										"<option>three</option>",
										"<option>four</option>",
										"<option>five</option>",
										"<option>six</option>",
										"<option>seven</option>",
										"<option>eight</option>",
										"<option>nine</option>",
										"<option>star</option>",
										"<option>asterisk</option>",
										"<option>hash</option>",
										"<option>pound</option>",
									  };

			Menus.Menu menu = new Menus.Menu();

			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Repeat));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithAllCommandsDoNothingAndDtmfCanBeSpoken()
		{
			// DtmfCanBeSpoken flag should have no effect on DoNothing commands so we use the same expected lines as in TestMenuWithAllCommandsDoNothing().

			string[] sExpectedLines = { 
										"<if cond=\"callee == ' '\">",
										"<elseif cond=\"callee$.inputmode == 'dtmf'\" />",
										"  <if cond=\"callee$.utterance == '0'\">",
										"    <goto next=\"#MenuWithAllCommandsDoNothing\" />",
										"  <elseif cond=\"callee$.utterance == '1'\" />"
									  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithAllCommandsDoNothing";
			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.DoNothing));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithAllCommandsRepeatAndDtmfCanBeSpoken()
		{
			string[] sExpectedLinesSpeech = { 
											   "<if cond=\"callee == 'zero'\">",
											   "  <audio src=\"0.wav\">Pressed 0</audio>",
											   "  <goto next=\"#MenuWithAllCommandsRepeat\" />",
											   "<elseif cond=\"callee == 'null'\" />"
											};
			string[] sExpectedLinesDtmf = { 
											"<if cond=\"callee$.utterance == '0'\">",
											"  <audio src=\"0.wav\">Pressed 0</audio>",
											"  <goto next=\"#MenuWithAllCommandsRepeat\" />",
											"<elseif cond=\"callee$.utterance == '1'\" />"
										  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithAllCommandsRepeat";
			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Repeat));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
		}

		[Test]
		public void TestMenuWithAllCommandsGotoMenuAndDtmfCanBeSpoken()
		{
			string[] sExpectedLinesSpeech = { 
											   "<if cond=\"callee == 'zero'\">",
											   "  <audio src=\"0.wav\">Pressed 0</audio>",
											   "  <goto next=\"file:///opt/speechbridge/VoiceDocStore/0.vxml.xml\" />",
											   "<elseif cond=\"callee == 'null'\" />"
											};
			string[] sExpectedLinesDtmf = { 
											"<if cond=\"callee$.utterance == '0'\">",
											"  <audio src=\"0.wav\">Pressed 0</audio>",
											"  <goto next=\"file:///opt/speechbridge/VoiceDocStore/0.vxml.xml\" />",
											"<elseif cond=\"callee$.utterance == '1'\" />"
										  };

			Menus.Menu menu = new Menus.Menu();

			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.GotoMenu));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
		}

		[Test]
		public void TestMenuWithAllCommandsTransferAndDtmfCanBeSpoken()
		{
			string[] sExpectedLinesSpeech = { 
											   "<if cond=\"callee == 'zero'\">",
											   "  <transfer dest=\"sip:0\">",
											   "    <prompt>",
											   "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">one moment please</audio>",
											   "    </prompt>",
											   "  </transfer>",
											   "<elseif cond=\"callee == 'null'\" />"
											};
			string[] sExpectedLinesDtmf = { 
											"<if cond=\"callee$.utterance == '0'\">",
											"  <transfer dest=\"sip:0\">",
											"    <prompt>",
											"      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">one moment please</audio>",
											"    </prompt>",
											"  </transfer>",
											"<elseif cond=\"callee$.utterance == '1'\" />"
										  };

			Menus.Menu menu = new Menus.Menu();

			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Transfer));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
		}

		[Test]
		public void TestMenuWithAllCommandsCodeBlockAndDtmfCanBeSpoken()
		{
			string[] sExpectedLinesSpeech = { 
												"<if cond=\"callee == 'zero'\">",
												"  <script>",
												"    value = 0;",
												"  </script>",
												"<elseif cond=\"callee == 'null'\" />"
											};
			string[] sExpectedLinesDtmf = { 
												"<elseif cond=\"callee$.inputmode == 'dtmf'\" /><if cond=\"callee$.utterance == '0'\">",			//$$$ LP - This is just because of the strange way that the XML is generated when writing raw XML to the XmlTextWriter.
												"  <script>",
												"    value = 0;",
												"  </script>",
												"<elseif cond=\"callee$.utterance == '1'\" />"
										  };

			Menus.Menu menu = new Menus.Menu();

			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			foreach (char c in m_cDtmfKeys)
			{
				commands.Add(CreateDtmfCommand(c, Commands.eOperationType.CodeBlock));
			}

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
		}

        [Test]
        public void TestMenuWithAllCommandsHangupAndDtmfCanBeSpoken()
        {
            string[] sExpectedLinesSpeech = { 
											   "<if cond=\"callee == 'zero'\">",
											   "  <audio src=\"0.wav\">Pressed 0</audio>",
                                               "  <exit />",
											   "<elseif cond=\"callee == 'null'\" />"
											};
            string[] sExpectedLinesDtmf = { 
											"<if cond=\"callee$.utterance == '0'\">",
											"  <audio src=\"0.wav\">Pressed 0</audio>",
                                            "  <exit />",
											"<elseif cond=\"callee$.utterance == '1'\" />"
										  };

            Menus.Menu menu = new Menus.Menu();

            menu.DtmfCanBeSpoken = true;

            Commands commands = new Commands();

            foreach (char c in m_cDtmfKeys)
            {
                commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Hangup));
            }

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
        }

        [Test]
        public void TestMenuWithAllCommandsPromptAndDtmfCanBeSpoken()
        {
            string[] sExpectedLinesSpeech = { 
											   "<if cond=\"callee == 'zero'\">",
											   "  <audio src=\"0.wav\">Pressed 0</audio>",
											   "<elseif cond=\"callee == 'null'\" />"
											};
            string[] sExpectedLinesDtmf = { 
											"<if cond=\"callee$.utterance == '0'\">",
											"  <audio src=\"0.wav\">Pressed 0</audio>",
											"<elseif cond=\"callee$.utterance == '1'\" />"
										  };

            Menus.Menu menu = new Menus.Menu();

            menu.DtmfCanBeSpoken = true;

            Commands commands = new Commands();

            foreach (char c in m_cDtmfKeys)
            {
                commands.Add(CreateDtmfCommand(c, Commands.eOperationType.Prompt));
            }

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
        }

        [Test]
		public void TestIntroBlockPromptWithNoTextOrWaveFileSpecified()
		{
			string[] sExpectedIntroBlock = {
											   "<form id=\"MenuWithEmptyIntroPrompt\">",
											   "<block />" ,
											   "<field name=\"callee\">"
										   };

			Menus.Menu menu = new Menus.Menu();
			menu.MenuName = "MenuWithEmptyIntroPrompt";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.IntroBlock;
			command.OperationType = Commands.eOperationType.Prompt;

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
		}

		[Test]
		public void TestIntroBlockPrompt()
		{
			string[] sExpectedIntroBlock = { 
											   "<form id=\"MenuWithIntroPrompt\">",
											   "<block>",
 											   "  <prompt>",
											   "    <audio src=\"intro.wav\">Intro Prompt.</audio>",
											   "  </prompt>",
											   "</block>",
											   "<field name=\"callee\">"
										   };

			Menus.Menu menu = new Menus.Menu();
			menu.MenuName = "MenuWithIntroPrompt";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.IntroBlock;
			command.OperationType = Commands.eOperationType.Prompt;
			command.ConfirmationText = "Intro Prompt";
			command.ConfirmationWavUrl = "intro.wav";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
		}

        [Test]
		public void TestThatMultilineTextIntroIsPunctuated()
		{
			string[] sExpectedIntroBlock = { 
											  "<block>",
											  "  <prompt>",
											  "    <audio>Line 1.",
											  "           Line 2.",
											  "           Line 3.</audio>",
											  "  </prompt>",
											  "</block>"
										   };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.IntroBlock;
			command.OperationType = Commands.eOperationType.Prompt;
			command.ConfirmationText = "Line 1" + Environment.NewLine + "Line 2" + Environment.NewLine + "Line 3";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
		}

		[Test]
		public void TestIntroBlockWithEmptyCodeblock()
		{
			string[] sExpectedIntroBlock = {
											   "<form id=\"MenuWithEmptyIntroBlockCodeblock\">",
											   "<block />" ,
											   "<field name=\"callee\">"
										   };

			Menus.Menu menu = new Menus.Menu();
			menu.MenuName = "MenuWithEmptyIntroBlockCodeblock";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.IntroBlock;
			command.OperationType = Commands.eOperationType.CodeBlock;

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
		}

		[Test]
		public void TestIntroBlockWithCodeblock()
		{
			string[] sExpectedIntroBlock = { 
											  "<block>",
											  "  <prompt>",
											  "    <audio>Hello</audio>",
											  "  </prompt>",
											  "</block>"
										   };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.IntroBlock;
			command.OperationType = Commands.eOperationType.CodeBlock;
			command.Response = "<prompt>" + Environment.NewLine + "<audio>Hello</audio>" + Environment.NewLine + "</prompt>";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
		}

        [Test]
        public void TestIntroBlockWithHangupWithNoTextOrWaveFileSpecified()
        {
            string[] sExpectedIntroBlock = {
                                               "<block>",
                                               "  <exit />",
                                               "</block>"
                                           };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.IntroBlock;
            command.OperationType = Commands.eOperationType.Hangup;

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
        }

        [Test]
        public void TestIntroBlockWithHangupWithPrompt()
        {
            string[] sExpectedIntroBlock = {
                                               "<block>",
                                               "  <prompt>",
                                               "    <audio src=\"hangup.wav\">Hangup in Intro Block.</audio>",
                                               "  </prompt>",
                                               "  <exit />",
                                               "</block>"
                                           };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.IntroBlock;
            command.OperationType = Commands.eOperationType.Hangup;
            command.ConfirmationText = "Hangup in Intro Block";
            command.ConfirmationWavUrl = "hangup.wav";

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlock), "TestFile doesn't contain expected IntroBlock lines.");
        }

        [Test]
		public void TestMenuWithUnrecognizedDtmfHandler_DoNothing()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <goto next=\"#MenuWithUnrecognizedDtmfHandler_DoNothing\" />",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithUnrecognizedDtmfHandler_DoNothing";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
			command.OperationType = Commands.eOperationType.DoNothing;

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithUnrecognizedDtmfHandler_Repeat()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio src=\"DTMFNotRecognized.wav\">DTMF not recognized.</audio>",
													 "  <goto next=\"#MenuWithUnrecognizedDtmfHandler_Repeat\" />",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithUnrecognizedDtmfHandler_Repeat";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
			command.OperationType = Commands.eOperationType.Repeat;
			command.ConfirmationText = "DTMF not recognized.";
			command.ConfirmationWavUrl = "DTMFNotRecognized.wav";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
		public void TestMenuWithUnrecognizedDtmfHandler_GotoMenu()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio src=\"DTMFNotRecognized.wav\">DTMF not recognized.</audio>",
													 "  <goto next=\"file:///opt/speechbridge/VoiceDocStore/UnrecognizedDTMF.vxml.xml\" />",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
			command.OperationType = Commands.eOperationType.GotoMenu;
			command.ConfirmationText = "DTMF not recognized.";
			command.ConfirmationWavUrl = "DTMFNotRecognized.wav";
			command.Response = "file:///opt/speechbridge/VoiceDocStore/UnrecognizedDTMF.vxml.xml";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
		public void TestMenuWithUnrecognizedDtmfHandler_Transfer()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <transfer dest=\"sip:1234\">",
													 "    <prompt>",
													 "      <audio src=\"DTMFNotRecognized.wav\">DTMF not recognized.</audio>",
													 "    </prompt>",
													 "  </transfer>",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
			command.OperationType = Commands.eOperationType.Transfer;
			command.ConfirmationText = "DTMF not recognized.";
			command.ConfirmationWavUrl = "DTMFNotRecognized.wav";
			command.Response = "1234";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
		public void TestMenuWithUnrecognizedDtmfHandler_Codeblock()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <script>",
													 "    value = \"DTMF Oops\";",
													 "  </script>",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
			command.OperationType = Commands.eOperationType.CodeBlock;
			command.Response = "<script>\nvalue = \"DTMF Oops\";\n</script>";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
        public void TestMenuWithUnrecognizedDtmfHandler_Hangup()
        {
            string[] sExpectedUnrecognizedBlock = {
													 "<else />",
													 "  <audio src=\"DTMFNotRecognized.wav\">DTMF not recognized.</audio>",
													 "  <exit />",
													 "</if>"
                                                  };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
            command.OperationType = Commands.eOperationType.Hangup;
            command.ConfirmationText = "DTMF not recognized.";
            command.ConfirmationWavUrl = "DTMFNotRecognized.wav";

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
        }

        [Test]
        public void TestMenuWithUnrecognizedDtmfHandler_Prompt()
        {
            string[] sExpectedUnrecognizedBlock = {
													 "<else />",
													 "  <audio src=\"DTMFNotRecognized.wav\">DTMF not recognized.</audio>",
													 "</if>"
                                                  };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.UnrecognizedDtmfHandler;
            command.OperationType = Commands.eOperationType.Prompt;
            command.ConfirmationText = "DTMF not recognized.";
            command.ConfirmationWavUrl = "DTMFNotRecognized.wav";

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
        }

        [Test]
		public void TestMenuWithUnrecognizedSpeechHandler_DoNothing()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <goto next=\"#MenuWithUnrecognizedSpeechHandler_DoNothing\" />",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithUnrecognizedSpeechHandler_DoNothing";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
			command.OperationType = Commands.eOperationType.DoNothing;

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

		[Test]
		public void TestMenuWithUnrecognizedSpeechHandler_Repeat()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio src=\"SpeechNotRecognized.wav\">Speech not recognized.</audio>",
													 "  <goto next=\"#MenuWithUnrecognizedSpeechHandler_Repeat\" />",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithUnrecognizedSpeechHandler_Repeat";

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
			command.OperationType = Commands.eOperationType.Repeat;
			command.ConfirmationText = "Speech not recognized.";
			command.ConfirmationWavUrl = "SpeechNotRecognized.wav";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
		public void TestMenuWithUnrecognizedSpeechHandler_GotoMenu()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio src=\"SpeechNotRecognized.wav\">Speech not recognized.</audio>",
													 "  <goto next=\"file:///opt/speechbridge/VoiceDocStore/UnrecognizedSpeech.vxml.xml\" />",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
			command.OperationType = Commands.eOperationType.GotoMenu;
			command.ConfirmationText = "Speech not recognized.";
			command.ConfirmationWavUrl = "SpeechNotRecognized.wav";
			command.Response = "file:///opt/speechbridge/VoiceDocStore/UnrecognizedSpeech.vxml.xml";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
		public void TestMenuWithUnrecognizedSpeechHandler_Transfer()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <transfer dest=\"sip:9999\">",
													 "    <prompt>",
													 "      <audio src=\"SpeechNotRecognized.wav\">Speech not recognized.</audio>",
													 "    </prompt>",
													 "  </transfer>",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
			command.OperationType = Commands.eOperationType.Transfer;
			command.ConfirmationText = "Speech not recognized.";
			command.ConfirmationWavUrl = "SpeechNotRecognized.wav";
			command.Response = "9999";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
		public void TestMenuWithUnrecognizedSpeechHandler_Codeblock()
		{
			string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <script>",
													 "    value = \"Speech Oops\";",
													 "  </script>",
													 "</if>"
												  };

			Menus.Menu menu = new Menus.Menu();

			Commands.Command command = new Commands.Command();
			command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
			command.OperationType = Commands.eOperationType.CodeBlock;
			command.Response = "<script>\nvalue = \"Speech Oops\";\n</script>";

			Commands commands = new Commands();
			commands.Add(command);

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
		}

        [Test]
        public void TestMenuWithUnrecognizedSpeechHandler_Hangup()
        {
            string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio src=\"SpeechNotRecognized.wav\">Speech not recognized.</audio>",
													 "  <exit />",
													 "</if>"
												  };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
            command.OperationType = Commands.eOperationType.Hangup;
            command.ConfirmationText = "Speech not recognized.";
            command.ConfirmationWavUrl = "SpeechNotRecognized.wav";

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
        }

        [Test]
        public void TestMenuWithUnrecognizedSpeechHandler_Prompt()
        {
            string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio src=\"SpeechNotRecognized.wav\">Speech not recognized.</audio>",
													 "</if>"
												  };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
            command.OperationType = Commands.eOperationType.Prompt;
            command.ConfirmationText = "Speech not recognized.";
            command.ConfirmationWavUrl = "SpeechNotRecognized.wav";

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
        }

        [Test]
        public void TestMenuWithUnrecognizedSpeechHandler_PromptMenuWithNoTextOrWaveFileSpecified()
        {
            string[] sExpectedUnrecognizedBlock = { 
													 "<else />",
													 "  <audio />",
													 "</if>"
												  };

            Menus.Menu menu = new Menus.Menu();

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.UnrecognizedSpeechHandler;
            command.OperationType = Commands.eOperationType.Prompt;

            Commands commands = new Commands();
            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedBlock), "TestFile doesn't contain expected lines.");
        }

        [Test]
		public void TestThatIfEquivalentSpeechCommandExistThenDtmfCanBeSpokenCommandWillNotBeCreated()
		{
			string[] sExpectedLinesSpeech = { 
											   "<elseif cond=\"callee == 'four'\" />",
											   "  <audio src=\"four.wav\">Said four</audio>",
											   "  <goto next=\"#MenuWithAllCommandsRepeat\" />",
											   "<elseif cond=\"callee == 'hello'\" />"
											};
			string[] sExpectedLinesDtmf = { 
											"<elseif cond=\"callee$.utterance == '4'\" />",
											"  <audio src=\"4.wav\">Pressed 4</audio>",
											"  <goto next=\"#MenuWithAllCommandsRepeat\" />",
										  };

			Menus.Menu menu = new Menus.Menu();

			menu.MenuName = "MenuWithAllCommandsRepeat";
			menu.DtmfCanBeSpoken = true;

			Commands commands = new Commands();

			commands.Add(CreateDtmfCommand('3', Commands.eOperationType.Repeat));
			commands.Add(CreateDtmfCommand('4', Commands.eOperationType.Repeat));
			commands.Add(CreateSpeechCommand("four", Commands.eOperationType.Repeat));
			commands.Add(CreateSpeechCommand("hello", Commands.eOperationType.Repeat));

			DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, GetDtmfToSpokenMapping(), null);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

			Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesSpeech), "TestFile doesn't contain expected lines for handling speech commands.");
			Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLinesDtmf), "TestFile doesn't contain expected lines for handling DTMF commands.");
		}

        [Test]
        public void TestSpeechCommandPromptWithNoTextOrWaveFileSpecified()
        {
            string[] sExpectedLines = { 
										"<if cond=\"callee == 'test'\">",
										"    <audio />",
									  };

            Menus.Menu menu = new Menus.Menu();

            Commands commands = new Commands();
            Commands.Command command = new Commands.Command();

            command.CommandType = Commands.eCommandType.Speech;
            command.CommandOption = "test";
            command.OperationType = Commands.eOperationType.Prompt;

            commands.Add(command);

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GenerateVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
        }

        [Test]
//        [ExpectedException(typeof(ArgumentNullException), UserMessage = "i_VxmlGenerationParameters")]
        public void TestThatArgumentNullExceptionIsThrownIfNoVxmlGenerationParametersIsProvidedWhenConfirmationEnabled()
        {
            Menus.Menu menu = new Menus.Menu();
            Commands commands = new Commands();

            menu.ConfirmationEnabled = true;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, null);
        }

        [Test]
        public void TestThatConfirmationVariablesAreAddedIfConfirmationEnabled()
        {
            string[] sExpectedLines = {
                                        "<var name=\"oSAUtils\" expr=\"\" />",
                                        "<var name=\"iSAConf\" expr=\"0\" />",
                                        "<var name=\"sSAUtt\" expr=\"\" />",
                                        "<var name=\"sSATarget\" expr=\"\" />",
                                        "<var name=\"sSATargetWavefile\" expr=\"\" />",
                                        "<var name=\"sSATargetTts\" expr=\"\" />",
                                        "<var name=\"iNumConfirmMisses\" expr=\"0\" />",
                                        "<var name=\"iNumConfirmRetries\" expr=\"0\" />",
                                      };

            Menus.Menu menu = new Menus.Menu();
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedLines), "TestFile doesn't contain expected lines.");
        }

        [Test]
        public void TestThatConfirmationLogicIsAddedToHangupResponseIfConfirmationIsEnabled()
        {
            string[] sExpectedConfirmationLogicLines = {
				                                            "<if cond=\"callee == 'All done'\">",
					                                        "  <script>oSAUtils = new ISMessaging.Utilities();",
                                                            "          iSAConf = oSAUtils.CopyString(callee$.confidence);",
                                                            "          sSAUtt = oSAUtils.CopyString(callee$.utterance);",
                                                            "  </script>",
					                                        "  <if cond=\"iSAConf &gt; '70'\">",
						                                    "    <audio src=\"Hangup Wavefile.wav\">Hangup Prompt</audio>",
						                                    "    <exit />",
						                                    "  <else />",
						                                    "    <script>iNumConfirmMisses = 0;",
                                                            "            sSATargetWavefile = oSAUtils.CopyString(\"Hangup Wavefile.wav\");",
                                                            "            sSATargetTts = oSAUtils.CopyString(\"Hangup Prompt\");",
                                                            "    </script>",
						                                    "    <goto next=\"#ConfirmHangup\" />",
					                                        "  </if>",
                                                        };

            Menus.Menu menu = new Menus.Menu();
            menu.MenuName = "ConfirmHangupTest";
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;
            command.OperationType = Commands.eOperationType.Hangup;
            command.CommandOption = "All done";
            command.ConfirmationWavUrl = "Hangup Wavefile.wav";
            command.ConfirmationText = "Hangup Prompt";

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
            vxmlGenerationParameters.ConfirmationCutoffPercentage = 70;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationLogicLines), "TestFile doesn't contain expected Confirmation Logic element.");

        }

        [Test]
        public void TestThatConfirmationHangupFormIsAddedIfConfirmationIsEnabled()
        {
            string[] sExpectedFormElement = { "<form id=\"ConfirmHangup\">" };
            string[] sExpectedIntroBlockLines = {
                                                    "<prompt bargein=\"true\">",
                                                    "  <audio>",
                                                    "    <value expr=\"sSAUtt\" />",
                                                    "  </audio>",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/IsThatCorrect.wav\">Is that correct?</audio>",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\" />",
			                                        "</prompt>"
                                                };
            string[] sExpectedFieldElement = { "<field name=\"confirmation\">" };
            string[] sExpectedGrammarElement = { "<grammar type=\"application/srgs\" src=\"file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram\" />" };

            string[] sExpectedConfirmationSuccessLines = {
				                                            "<if cond=\"confirmation == 'true'\">",
					                                        "  <audio expr=\"sSATargetWavefile\">",
						                                    "    <value expr=\"sSATargetTts\" />",
					                                        "  </audio>",
					                                        "  <exit />"
                                                         };
            string[] sExpectedConfirmationFailedLines = {
					                                        "<elseif cond=\"confirmation$.utterance == '2'\" />",
					                                        "  <if cond=\"iNumConfirmRetries == '3'\">",
						                                    "    <transfer dest=\"sip:0\">",
							                                "      <prompt bargein=\"false\">",
								                            "        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>",
								                            "        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\" />",
							                                "      </prompt>",
						                                    "    </transfer>",
						                                    "  <else />",
						                                    "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>",
						                                    "    <script>iNumConfirmRetries = iNumConfirmRetries + 1;</script>",
					                                        "  </if>",
					                                        "  <goto next=\"#ConfirmHangupTest\" />"
                                                        };
            string[] sExpectedMainMenuLines = { 
                                                "<elseif cond=\"confirmation == 'main menu'\" />",
					                            "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav\">Main menu.</audio>",
					                            "  <goto next=\"#ConfirmHangupTest\" />"
                                              };
            string[] sExpectedUnrecognizedLines = {
					                                "<else />",
					                                "<if cond=\"iNumConfirmMisses == '3'\">",
						                             "  <transfer dest=\"sip:0\">",
							                         "    <prompt bargein=\"false\">",
								                     "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>",
								                     "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\" />",
							                         "    </prompt>",
						                             "  </transfer>",
						                             "<else />",
						                             "  <prompt bargein=\"false\">",
							                         "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/ImSorryWasThat.wav\">I'm sorry, was that</audio>",
							                         "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
							                         "    <audio>",
								                     "      <value expr=\"sSAUtt\" />",
							                         "    </audio>",
													 "  </prompt>",
													 "  <script>",
													 "    // This script element is used to allow a second prompt element to flip barge-in behavior.",
													 "  </script>",
													 "  <prompt bargein=\"true\">",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/PleaseSayYesOrNo.wav\">Please say, yes, or, no.</audio>",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\" />",
						                             "  </prompt>",
						                             "  <script>iNumConfirmMisses = iNumConfirmMisses + 1;</script>",
					                                 "</if>"
                                                  };

            Menus.Menu menu = new Menus.Menu();
            menu.MenuName = "ConfirmHangupTest";
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;
            command.OperationType = Commands.eOperationType.Hangup;

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
            vxmlGenerationParameters.OperatorExtension = "0";
            vxmlGenerationParameters.NumberOfRetries = 3;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFormElement), "TestFile doesn't contain expected FORM element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlockLines), "TestFile doesn't contain expected Intro Block lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFieldElement), "TestFile doesn't contain expected FIELD element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedGrammarElement), "TestFile doesn't contain expected GRAMMAR element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationSuccessLines), "TestFile doesn't contain expected Confirmation Success lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationFailedLines), "TestFile doesn't contain expected Confirmation Failed lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedMainMenuLines), "TestFile doesn't contain expected Main Menu lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedLines), "TestFile doesn't contain expected Unrecognized lines.");
        }

        [Test]
        public void TestThatConfirmationLogicIsAddedToGotoResponseIfConfirmationIsEnabled()
        {
            string[] sExpectedConfirmationLogicLines = {
				                                            "<if cond=\"callee == 'Let's Go'\">",
					                                        "  <script>oSAUtils = new ISMessaging.Utilities();",
                                                            "          iSAConf = oSAUtils.CopyString(callee$.confidence);",
                                                            "          sSAUtt = oSAUtils.CopyString(callee$.utterance);",
                                                            "  </script>",
					                                        "  <if cond=\"iSAConf &gt; '89'\">",
						                                    "    <audio src=\"Goto Wavefile.wav\">Goto Prompt</audio>",
						                                    "    <goto next=\"GotoTarget.vxml.xml\" />",
						                                    "  <else />",
						                                    "    <script>iNumConfirmMisses = 0;",
                                                            "            sSATarget = oSAUtils.CopyString(\"GotoTarget.vxml.xml\");",
                                                            "            sSATargetWavefile = oSAUtils.CopyString(\"Goto Wavefile.wav\");",
                                                            "            sSATargetTts = oSAUtils.CopyString(\"Goto Prompt\");",
                                                            "    </script>",
						                                    "    <goto next=\"#ConfirmGoto\" />",
					                                        "  </if>",
                                                        };

            Menus.Menu menu = new Menus.Menu();
            menu.MenuName = "ConfirmGotoTest";
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;
            command.OperationType = Commands.eOperationType.GotoMenu;
            command.CommandOption = "Let's Go";
            command.ConfirmationWavUrl = "Goto Wavefile.wav";
            command.ConfirmationText = "Goto Prompt";
            command.Response = "GotoTarget.vxml.xml";

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
            vxmlGenerationParameters.ConfirmationCutoffPercentage = 89;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationLogicLines), "TestFile doesn't contain expected Confirmation Logic element.");

        }

        [Test]
        public void TestThatConfirmationGotoFormIsAddedIfConfirmationIsEnabled()
        {
            string[] sExpectedFormElement = { "<form id=\"ConfirmGoto\">" };
            string[] sExpectedIntroBlockLines = {
                                                    "<prompt bargein=\"true\">",
                                                    "  <audio>",
                                                    "    <value expr=\"sSAUtt\" />",
                                                    "  </audio>",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/IsThatCorrect.wav\">Is that correct?</audio>",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\" />",
			                                        "</prompt>"
                                                };
            string[] sExpectedFieldElement = { "<field name=\"confirmation\">" };
            string[] sExpectedGrammarElement = { "<grammar type=\"application/srgs\" src=\"file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram\" />" };

            string[] sExpectedConfirmationSuccessLines = {
				                                            "<if cond=\"confirmation == 'true'\">",
					                                        "  <audio expr=\"sSATargetWavefile\">",
						                                    "    <value expr=\"sSATargetTts\" />",
					                                        "  </audio>",
					                                        "  <goto expr=\"sSATarget\" />"
                                                         };
            string[] sExpectedConfirmationFailedLines = {
					                                        "<elseif cond=\"confirmation$.utterance == '2'\" />",
					                                        "  <if cond=\"iNumConfirmRetries == '7'\">",
						                                    "    <transfer dest=\"sip:999\">",
							                                "      <prompt bargein=\"false\">",
								                            "        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>",
								                            "        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\" />",
							                                "      </prompt>",
						                                    "    </transfer>",
						                                    "  <else />",
						                                    "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>",
						                                    "    <script>iNumConfirmRetries = iNumConfirmRetries + 1;</script>",
					                                        "  </if>",
					                                        "  <goto next=\"#ConfirmGotoTest\" />"
                                                        };
            string[] sExpectedMainMenuLines = { 
                                                "<elseif cond=\"confirmation == 'main menu'\" />",
					                            "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav\">Main menu.</audio>",
					                            "  <goto next=\"#ConfirmGotoTest\" />"
                                              };
            string[] sExpectedUnrecognizedLines = {
					                                "<else />",
					                                "<if cond=\"iNumConfirmMisses == '7'\">",
						                             "  <transfer dest=\"sip:999\">",
							                         "    <prompt bargein=\"false\">",
								                     "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>",
								                     "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\" />",
							                         "    </prompt>",
						                             "  </transfer>",
						                             "<else />",
						                             "  <prompt bargein=\"false\">",
							                         "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/ImSorryWasThat.wav\">I'm sorry, was that</audio>",
							                         "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
							                         "    <audio>",
								                     "      <value expr=\"sSAUtt\" />",
							                         "    </audio>",
													 "  </prompt>",
													 "  <script>",
													 "    // This script element is used to allow a second prompt element to flip barge-in behavior.",
													 "  </script>",
													 "  <prompt bargein=\"true\">",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/PleaseSayYesOrNo.wav\">Please say, yes, or, no.</audio>",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\" />",
						                             "  </prompt>",
						                             "  <script>iNumConfirmMisses = iNumConfirmMisses + 1;</script>",
					                                 "</if>"
                                                  };

            Menus.Menu menu = new Menus.Menu();
            menu.MenuName = "ConfirmGotoTest";
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;
            command.OperationType = Commands.eOperationType.GotoMenu;

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
            vxmlGenerationParameters.OperatorExtension = "999";
            vxmlGenerationParameters.NumberOfRetries = 7;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFormElement), "TestFile doesn't contain expected FORM element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlockLines), "TestFile doesn't contain expected Intro Block lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFieldElement), "TestFile doesn't contain expected FIELD element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedGrammarElement), "TestFile doesn't contain expected GRAMMAR element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationSuccessLines), "TestFile doesn't contain expected Confirmation Success lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationFailedLines), "TestFile doesn't contain expected Confirmation Failed lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedMainMenuLines), "TestFile doesn't contain expected Main Menu lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedLines), "TestFile doesn't contain expected Unrecognized lines.");
        }

        [Test]
        public void TestThatConfirmationLogicIsAddedToTransferResponseIfConfirmationIsEnabled()
        {
            string[] sExpectedConfirmationLogicLines = {
				                                            "<if cond=\"callee == 'Beam me up Scotty.'\">",
					                                        "  <script>oSAUtils = new ISMessaging.Utilities();",
                                                            "          iSAConf = oSAUtils.CopyString(callee$.confidence);",
                                                            "          sSAUtt = oSAUtils.CopyString(callee$.utterance);",
                                                            "  </script>",
					                                        "  <if cond=\"iSAConf &gt; '89'\">",
                                                            "    <transfer dest=\"sip:1234\">",
                                                            "      <prompt bargein=\"false\">",
						                                    "        <audio src=\"Transfer Wavefile.wav\">Transfer Prompt</audio>",
						                                    "      </prompt>",
                                                            "    </transfer>",
						                                    "  <else />",
						                                    "    <script>iNumConfirmMisses = 0;",
                                                            "            sSATarget = oSAUtils.CopyString(\"1234\");",
                                                            "            sSATargetWavefile = oSAUtils.CopyString(\"Transfer Wavefile.wav\");",
                                                            "            sSATargetTts = oSAUtils.CopyString(\"Transfer Prompt\");",
                                                            "    </script>",
						                                    "    <goto next=\"#ConfirmTransfer\" />",
					                                        "  </if>",
                                                        };

            Menus.Menu menu = new Menus.Menu();
            menu.MenuName = "ConfirmTransferTest";
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;
            command.OperationType = Commands.eOperationType.Transfer;
            command.CommandOption = "Beam me up Scotty.";
            command.ConfirmationWavUrl = "Transfer Wavefile.wav";
            command.ConfirmationText = "Transfer Prompt";
            command.Response = "1234";

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
            vxmlGenerationParameters.ConfirmationCutoffPercentage = 89;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationLogicLines), "TestFile doesn't contain expected Confirmation Logic element.");

        }

        [Test]
        public void TestThatConfirmationTransferFormIsAddedIfConfirmationIsEnabled()
        {
            string[] sExpectedFormElement = { "<form id=\"ConfirmTransfer\">" };
            string[] sExpectedIntroBlockLines = {
                                                    "<prompt bargein=\"true\">",
                                                    "  <audio>",
                                                    "    <value expr=\"sSAUtt\" />",
                                                    "  </audio>",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/IsThatCorrect.wav\">Is that correct?</audio>",
				                                    "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\" />",
			                                        "</prompt>"
                                                };
            string[] sExpectedFieldElement = { "<field name=\"confirmation\">" };
            string[] sExpectedGrammarElement = { "<grammar type=\"application/srgs\" src=\"file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram\" />" };

            string[] sExpectedConfirmationSuccessLines = {
				                                            "<if cond=\"confirmation == 'true'\">",
					                                        "  <transfer dest=\"sip:application$.sSATarget\">",
                                                            "    <prompt bargein=\"false\">",
					                                        "      <audio expr=\"sSATargetWavefile\">",
						                                    "        <value expr=\"sSATargetTts\" />",
					                                        "      </audio>",
                                                            "    </prompt>",
                                                            "  </transfer>"
                                                         };
            string[] sExpectedConfirmationFailedLines = {
					                                        "<elseif cond=\"confirmation$.utterance == '2'\" />",
					                                        "  <if cond=\"iNumConfirmRetries == '1'\">",
						                                    "    <transfer dest=\"sip:0\">",
							                                "      <prompt bargein=\"false\">",
								                            "        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>",
								                            "        <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\" />",
							                                "      </prompt>",
						                                    "    </transfer>",
						                                    "  <else />",
						                                    "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>",
						                                    "    <script>iNumConfirmRetries = iNumConfirmRetries + 1;</script>",
					                                        "  </if>",
					                                        "  <goto next=\"#ConfirmTransferTest\" />"
                                                        };
            string[] sExpectedMainMenuLines = { 
                                                "<elseif cond=\"confirmation == 'main menu'\" />",
					                            "  <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav\">Main menu.</audio>",
					                            "  <goto next=\"#ConfirmTransferTest\" />"
                                              };
            string[] sExpectedUnrecognizedLines = {
					                                "<else />",
					                                "<if cond=\"iNumConfirmMisses == '1'\">",
						                             "  <transfer dest=\"sip:0\">",
							                         "    <prompt bargein=\"false\">",
								                     "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>",
								                     "      <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\" />",
							                         "    </prompt>",
						                             "  </transfer>",
						                             "<else />",
						                             "  <prompt bargein=\"false\">",
							                         "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/ImSorryWasThat.wav\">I'm sorry, was that</audio>",
							                         "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
							                         "    <audio>",
								                     "      <value expr=\"sSAUtt\" />",
							                         "    </audio>",
													 "  </prompt>",
													 "  <script>",
													 "    // This script element is used to allow a second prompt element to flip barge-in behavior.",
													 "  </script>",
													 "  <prompt bargein=\"true\">",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\" />",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/PleaseSayYesOrNo.wav\">Please say, yes, or, no.</audio>",
													 "    <audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\" />",
						                             "  </prompt>",
						                             "  <script>iNumConfirmMisses = iNumConfirmMisses + 1;</script>",
					                                 "</if>"
                                                  };

            Menus.Menu menu = new Menus.Menu();
            menu.MenuName = "ConfirmTransferTest";
            menu.ConfirmationEnabled = true;

            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;
            command.OperationType = Commands.eOperationType.Transfer;

            Commands commands = new Commands();
            commands.Add(command);

            VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();
            vxmlGenerationParameters.OperatorExtension = "0";
            vxmlGenerationParameters.NumberOfRetries = 1;

            DialogBuilder dialogBuilder = new DialogBuilder(menu, commands, null, vxmlGenerationParameters);
            bool bMenuGenerated = dialogBuilder.GenerateVxml(m_csTestFilename);

            Assert.IsTrue(bMenuGenerated, "GeneratedVxml() failed.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFormElement), "TestFile doesn't contain expected FORM element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedIntroBlockLines), "TestFile doesn't contain expected Intro Block lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedFieldElement), "TestFile doesn't contain expected FIELD element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedGrammarElement), "TestFile doesn't contain expected GRAMMAR element.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationSuccessLines), "TestFile doesn't contain expected Confirmation Success lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedConfirmationFailedLines), "TestFile doesn't contain expected Confirmation Failed lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedMainMenuLines), "TestFile doesn't contain expected Main Menu lines.");
            Assert.IsTrue(DoesContainLines(m_csTestFilename, sExpectedUnrecognizedLines), "TestFile doesn't contain expected Unrecognized lines.");
        }

        // Helper methods

		private Commands.Command CreateDtmfCommand(char i_cDtmfKey, Commands.eOperationType i_eOperationType)
		{
			Commands.Command command = new Commands.Command();

			command.CommandName = String.Format("DTMF {0}", i_cDtmfKey);
			command.CommandType = Commands.eCommandType.Dtmf;
			command.CommandOption = i_cDtmfKey.ToString();

			switch (i_eOperationType)
			{
				case Commands.eOperationType.DoNothing:
					command.OperationType = Commands.eOperationType.DoNothing;
					break;

				case Commands.eOperationType.Repeat:
					command.OperationType = Commands.eOperationType.Repeat;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_cDtmfKey);
					command.ConfirmationText = String.Format("Pressed {0}", i_cDtmfKey);
					break;

				case Commands.eOperationType.GotoMenu:
					command.OperationType = Commands.eOperationType.GotoMenu;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_cDtmfKey);
					command.ConfirmationText = String.Format("Pressed {0}", i_cDtmfKey);
					command.Response = String.Format(@"file:///opt/speechbridge/VoiceDocStore/{0}.vxml.xml", i_cDtmfKey);
					break;

				case Commands.eOperationType.Transfer:
					command.OperationType = Commands.eOperationType.Transfer;
					command.Response = i_cDtmfKey.ToString();
					command.ConfirmationWavUrl = "file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav";
					command.ConfirmationText = "one moment please";

					break;

				case Commands.eOperationType.CodeBlock:
					command.OperationType = Commands.eOperationType.CodeBlock;
					command.Response = String.Format("                        <script>\n                           value = {0};\n                        </script>", i_cDtmfKey);
					break;

                case Commands.eOperationType.Hangup:
                    command.OperationType = Commands.eOperationType.Hangup;
                    command.ConfirmationWavUrl = String.Format("{0}.wav", i_cDtmfKey);
                    command.ConfirmationText = String.Format("Pressed {0}", i_cDtmfKey);
                    break;

                case Commands.eOperationType.Prompt:
					command.OperationType = Commands.eOperationType.Prompt;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_cDtmfKey);
					command.ConfirmationText = String.Format("Pressed {0}", i_cDtmfKey);
                    break;
            }

			return command;
		}

		private Commands.Command CreateSpeechCommand(string i_sRequest, Commands.eOperationType i_eOperationType)
		{
			Commands.Command command = new Commands.Command();

			command.CommandType = Commands.eCommandType.Speech;
			command.CommandOption = i_sRequest;

			switch (i_eOperationType)
			{
				case Commands.eOperationType.DoNothing:
					command.OperationType = Commands.eOperationType.DoNothing;
					break;

				case Commands.eOperationType.Repeat:
					command.OperationType = Commands.eOperationType.Repeat;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_sRequest);
					command.ConfirmationText = String.Format("Said {0}", i_sRequest);
					break;

				case Commands.eOperationType.GotoMenu:
					command.OperationType = Commands.eOperationType.GotoMenu;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_sRequest);
					command.ConfirmationText = String.Format("Said {0}", i_sRequest);
					command.Response = String.Format(@"file:///opt/speechbridge/VoiceDocStore/{0}.vxml.xml", i_sRequest);
					break;

				case Commands.eOperationType.Transfer:
					command.OperationType = Commands.eOperationType.Transfer;
					command.Response = i_sRequest;
					command.ConfirmationWavUrl = "file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav";
					command.ConfirmationText = "one moment please";

					break;

				case Commands.eOperationType.CodeBlock:
					command.OperationType = Commands.eOperationType.CodeBlock;
					command.Response = String.Format("                        <script>\n                           value = {0};\n                        </script>", i_sRequest);
					break;

                case Commands.eOperationType.Hangup:
                    command.OperationType = Commands.eOperationType.Hangup;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_sRequest);
					command.ConfirmationText = String.Format("Said {0}", i_sRequest);
                    break;

                case Commands.eOperationType.Prompt:
                    command.OperationType = Commands.eOperationType.Prompt;
					command.ConfirmationWavUrl = String.Format("{0}.wav", i_sRequest);
					command.ConfirmationText = String.Format("Said {0}", i_sRequest);
                    break;
            }

			return command;
		}

		private bool DoesContainLines(string i_sTestFilename, string[] i_sExpectedLines)
		{
			int largestNumberOfLinesMatched = 0;

			using (StreamReader srText = new StreamReader(i_sTestFilename, new UTF8Encoding(false)))
			{
				int i = 0;

				while (srText.Peek() > 0)
				{
					// Don't care about indentation.

					if (srText.ReadLine().Trim() == i_sExpectedLines[i].Trim())
					{
						++i;
					}
					else
					{
						largestNumberOfLinesMatched = Math.Max(largestNumberOfLinesMatched, i);
						i = 0;
					}

					if (i == i_sExpectedLines.Length)
					{
						return true;
					}
				}
			}

			Console.Error.WriteLine("Largest number of lines matched: {0}", largestNumberOfLinesMatched);

			return false;
		}

		DtmfKeyToSpokenEquivalentMappings GetDtmfToSpokenMapping()
		{
			DtmfKeyToSpokenEquivalentMappings dtmfToSpokenMapping = new DtmfKeyToSpokenEquivalentMappings();

			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("0", "zero"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("0", "null"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("0", "nil"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("1", "one"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("2", "two"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("3", "three"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("4", "four"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("5", "five"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("6", "six"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("7", "seven"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("8", "eight"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("9", "nine"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("*", "star"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("*", "asterisk"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("#", "hash"));
			dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping("#", "pound"));

			return dtmfToSpokenMapping;
		}
	}
}
