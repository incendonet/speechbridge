// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using SBConfigStor;

namespace AppGenerator
{
	public sealed class DialogBuilder
	{
        private enum eBargeIn
        {
            NotSpecified,
            Disabled,
            Enabled
        };

		private XmlTextWriter m_xmlTextWriter;
		private Menus.Menu m_menu;
		private Commands m_commands;
		private string m_sFormName;
		private DtmfKeyToSpokenEquivalentMappings m_DtmfToSpokenMapping;
        private VxmlGenerationParameters m_VxmlGenerationParameters;

		private const int m_ciCommentWidth = 84;

		private string FormName
		{
			get { return m_sFormName; }
			set { m_sFormName = StringFilter.GetFilteredStringPath(value); }
		}

		public DialogBuilder(Menus.Menu i_menu, Commands i_commands, DtmfKeyToSpokenEquivalentMappings i_DtmfToSpokenMapping, VxmlGenerationParameters i_VxmlGenerationParameters)
		{
			m_menu = i_menu;
			m_commands = i_commands;
			m_commands.DtmfCanBeSpoken = m_menu.DtmfCanBeSpoken;		// This setting is stored in the menu object but the logic becomes cleaner if the commands collection knows its value.

			if (m_menu.DtmfCanBeSpoken)
			{
                if (null == i_DtmfToSpokenMapping)
                {
                    Console.Error.WriteLine("{0} DialogBuilder: ERROR - DtmfCanBeSpoken is true but no DtmfToSpokenMapping is provided.", DateTime.Now.ToString());		//$$$ LP - use logger.
                    throw new ArgumentNullException("i_DtmfToSpokenMapping", "DtmfCanBeSpoken is true but no DtmfToSpokenMapping is provided.");
                }
                else if (0 == i_DtmfToSpokenMapping.Count)
                {
                    Console.Error.WriteLine("{0} DialogBuilder: ERROR - DtmfCanBeSpoken is true but DtmfToSpokenMapping is empty.", DateTime.Now.ToString());		//$$$ LP - use logger.
                    throw new ArgumentOutOfRangeException("i_DtmfToSpokenMapping", "DtmfCanBeSpoken is true but DtmfToSpokenMapping is empty.");
                }
			}

			m_DtmfToSpokenMapping = i_DtmfToSpokenMapping;

            if (m_menu.ConfirmationEnabled)
            {
                if (null == i_VxmlGenerationParameters)
                {
                    Console.Error.WriteLine("{0} DialogBuilder: ERROR - ConfirmationEnabled is true but no VxmlGenerationParameters is provided.", DateTime.Now.ToString());		//$$$ LP - use logger.
                    throw new ArgumentNullException("i_VxmlGenerationParameters", "ConfirmationEnabled is true but no VxmlGenerationParameters is provided.");
                }
            }

            m_VxmlGenerationParameters = i_VxmlGenerationParameters;
			FormName = i_menu.MenuName;
		}

		public bool GenerateVxml(string i_sOutputFileName)
		{
			bool bVxmlGenerated = false;

			try
			{
				CheckForUnimplementedFeatures();

				OpenDocument(i_sOutputFileName);
				VxmlElementStart();
				CopyrightMessage(i_sOutputFileName);
				Variables();

                if (m_menu.ConfirmationEnabled)
                {
                    ConfirmationVariables();
                }

				Comment("");
				FormElementStart(FormName);
				IntroBlock();
                FieldElementStart("callee");
				Comment("Field options");

				if (UseGrammar() || !m_commands.HasSpeechCommands)
				{
					if (UseGrammar())
					{
						GrammarElement(m_menu.GrammarUrl);
					}
					else
					{
						Comment("This grammar is not used.  It is referenced here to avoid ASR errors.");
						GrammarElement("file:///opt/speechbridge/VoiceDocStore/ABNFDigits.gram");
					}
				}
				else
				{
					SpeechOptions();
				}

				Comment("Field responses");
				FilledElementStart();
				Comment("General commands");
				ProcessCommands();
				FilledElementEnd();
				FieldElementEnd();
				FormElementEnd();

                if (m_menu.ConfirmationEnabled)
                {
                    ConfirmationForms();
                }

				VxmlElementEnd();
				CloseDocument();

				bVxmlGenerated = true;
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} GenerateVxml: Caught exception: {1}", DateTime.Now.ToString(), exc.ToString());		//$$$ LP - use logger.
				bVxmlGenerated = false;
			}

			return bVxmlGenerated;
		}

		/// <summary>
		/// Generate dialogs for all enabled menus in default folder.
		/// </summary>
		public static bool ProcessEnabledMenus()
		{
			return ProcessEnabledMenus(FileSystemSupport.VxmlPath);
		}

		/// <summary>
		/// Generate dialogs for all enabled menus in specified folder.  
		/// If the specified directory is a relative path then it will be rooted in the local folder.
		/// </summary>
		/// <param name="i_sVxmlDirectory"></param>
		public static bool ProcessEnabledMenus(string i_sVxmlDirectory)
		{
			bool bMenuGenerated = true;
			DialogDesignerDAL dal = new DialogDesignerDAL();

			Menus menus = dal.GetEnabledMenus();

			foreach (Menus.Menu menu in menus)
			{
				Commands commands = dal.GetCommandsForMenu(menu);

				bMenuGenerated &= ProcessMenu(menu, commands, i_sVxmlDirectory);
			}

			return bMenuGenerated;
		}


		// The reason the menu and its associated commands are passed in (rather then retrieve the commands from the database inside the method)
		// is that this method is used to create a VXML file when a menu is saved on the Menu Editor page (at which point the commands already
		// exists in memory so no point wasting time retrieving them again).

		public static bool ProcessMenu(Menus.Menu i_menu, Commands i_commands, string i_sVxmlDirectory)
		{
			DtmfKeyToSpokenEquivalentMappings dtmfToSpokenMapping = null;
            VxmlGenerationParameters vxmlGenerationParameters = null;

			if (ConfigParams.IsVoiceRecognitionEnabled())
			{
				if (i_menu.DtmfCanBeSpoken)
				{
					DialogDesignerDAL dal = new DialogDesignerDAL();

					DialogDesignerDAL.ErrorCode ecDal = dal.GetDtmfToSpokenMapping(i_menu.LanguageCode, out dtmfToSpokenMapping);
				}
			}
			else
			{
				// If the system is not capable of voice recognition then it doesn't matter what the "DTMF keys can be spoken" setting in the menu is.

				i_menu.DtmfCanBeSpoken = false;
			}

            if (i_menu.ConfirmationEnabled)
            {
                vxmlGenerationParameters = GetVxmlGenerationParameters();
            }

			DialogBuilder dialogBuilder = new DialogBuilder(i_menu, i_commands, dtmfToSpokenMapping, vxmlGenerationParameters);
			bool bMenuGenerated = dialogBuilder.GenerateVxml(GetFileNameForMenu(i_menu, i_sVxmlDirectory));

			return bMenuGenerated;
		}

		private static string GetFileNameForMenu(Menus.Menu i_menu, string i_sVxmlDirectory)
		{
			string sFullFileName = Path.Combine(i_sVxmlDirectory, String.Format("{0}.vxml.xml", i_menu.MenuName));

			return StringFilter.GetFilteredStringPath(sFullFileName);
		}

		private void OpenDocument(string i_sVxmlFileName)
		{
			m_xmlTextWriter = new XmlTextWriter(i_sVxmlFileName, new UTF8Encoding(false));
			m_xmlTextWriter.Formatting = Formatting.Indented;
			m_xmlTextWriter.Indentation = 1;
			m_xmlTextWriter.IndentChar = '\t';
			m_xmlTextWriter.Namespaces = false;

			m_xmlTextWriter.WriteStartDocument();
		}

		private void CloseDocument()
		{
			m_xmlTextWriter.WriteEndDocument();
			m_xmlTextWriter.Close();
		}

		private void VxmlElementStart()
		{
			m_xmlTextWriter.WriteStartElement("vxml");

			m_xmlTextWriter.WriteAttributeString("version", "2.0");
			m_xmlTextWriter.WriteAttributeString("lang", m_menu.LanguageCode);

			if (NotNullOrEmpty(m_menu.Include))
			{
				m_xmlTextWriter.WriteAttributeString("application", m_menu.Include);
			}
		}

		private void VxmlElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

		private void FormElementStart(string i_sFormName)
		{
			m_xmlTextWriter.WriteStartElement("form");
            m_xmlTextWriter.WriteAttributeString("id", i_sFormName);
		}

		private void FormElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

		private void FieldElementStart(string i_sName)
		{
			m_xmlTextWriter.WriteStartElement("field");
            m_xmlTextWriter.WriteAttributeString("name", i_sName);
		}

		private void FieldElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

		private void FilledElementStart()
		{
			m_xmlTextWriter.WriteStartElement("filled");
		}

		private void FilledElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

		private void IfElementStart(string i_sCondition)
		{
			m_xmlTextWriter.WriteStartElement("if");
			m_xmlTextWriter.WriteAttributeString("cond", i_sCondition);
		}

		private void IfElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

		private void ElseIfElement(string i_sCondition)
		{
			m_xmlTextWriter.WriteStartElement("elseif");
			m_xmlTextWriter.WriteAttributeString("cond", i_sCondition);
			m_xmlTextWriter.WriteEndElement();
		}

        private void ElseElement()
        {
            m_xmlTextWriter.WriteStartElement("else");
            m_xmlTextWriter.WriteEndElement();
        }

		private void TransferElementStart(string i_sDestination)
		{
			m_xmlTextWriter.WriteStartElement("transfer");
			m_xmlTextWriter.WriteAttributeString("dest", String.Format("sip:{0}", i_sDestination));
		}

		private void TransferElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

		private void PromptElementStart(eBargeIn i_eBargeIn)
		{
			m_xmlTextWriter.WriteStartElement("prompt");

            if (i_eBargeIn != eBargeIn.NotSpecified)
            {
                m_xmlTextWriter.WriteAttributeString("bargein", (i_eBargeIn == eBargeIn.Enabled ? "true" : "false"));
            }
		}

		private void PromptElementEnd()
		{
			m_xmlTextWriter.WriteEndElement();
		}

        private void ExitElement()
        {
            m_xmlTextWriter.WriteElementString("exit", "");
        }

		private void CopyrightMessage(string i_sOutputFileName)
		{
			WriteCommentDelimiter();
			WriteCommentDelimiter();
			WriteCommentLine(String.Format("{0} - {1}", Path.GetFileName(i_sOutputFileName), GetVersion()));
			WriteCommentLine(" ");
			WriteCommentLine(" ");
			WriteCommentLine(String.Format("Copyright {0}, Incendonet Inc.  All rights reserved.", DateTime.Now.Year.ToString()));
			WriteCommentDelimiter();
			WriteCommentDelimiter();
		}

		private void Comment(string i_sComment)
		{
			WriteCommentDelimiter();
			WriteCommentLine(i_sComment);
			WriteCommentDelimiter();
		}

		private void WriteCommentDelimiter()
		{
			WriteCommentLine("".PadRight(m_ciCommentWidth, '~'));
		}

		private void WriteCommentLine(string i_sComment)
		{
			if (NotNullOrEmpty(i_sComment))
			{
				m_xmlTextWriter.WriteComment(String.Format(" {0} ", i_sComment.PadRight(m_ciCommentWidth)));
			}
		}

		private void Variables()
		{
			Comment("Document variables");

			if (null != m_menu.Variables)
			{
				foreach (Variables.Variable var in m_menu.Variables)
				{
                    VariableElement(var.Name, var.Value);
				}
			}
		}

		private void IntroBlock()
		{
			m_xmlTextWriter.WriteStartElement("block");

			foreach (Commands.Command command in m_commands)
			{
				switch (command.CommandType)
				{
					case Commands.eCommandType.IntroBlock:
						ProcessIntroBlockCommand(command);
						break;

					case Commands.eCommandType.Dtmf:
					case Commands.eCommandType.Speech:
					case Commands.eCommandType.Touch:
					case Commands.eCommandType.UnrecognizedDtmfHandler:
					case Commands.eCommandType.UnrecognizedSpeechHandler:
						break;

					default:
						Console.Error.WriteLine(String.Format("{0} DialogBuilder.IntroBlock: Unknown CommandType encountered ({1})", DateTime.Now, command.CommandType));					//$$$ LP - Write to log instead of console.
						break;
				}
			}

			m_xmlTextWriter.WriteEndElement();
		}

		private void SpeechOptions()
		{
			foreach (Commands.Command command in m_commands)
			{
				switch (command.CommandType)
				{
					case Commands.eCommandType.Speech:
						m_xmlTextWriter.WriteElementString("option", command.CommandOption);
						break;

					case Commands.eCommandType.Dtmf:
						if (m_commands.IsDtmfCommandSpeakable(command))
						{
							foreach (DtmfKeyToSpokenEquivalentMapping dtmf in m_DtmfToSpokenMapping)
							{
								if (dtmf.Key == command.CommandOption)
								{
									// Only add spoken equivalent if user hasn't already specified is explicitly.

									if (!m_commands.HasCommand(dtmf.SpokenEquivalent))
									{
										m_xmlTextWriter.WriteElementString("option", dtmf.SpokenEquivalent);
									}
								}
							}
						}
						break;

					case Commands.eCommandType.IntroBlock:
					case Commands.eCommandType.Touch:
					case Commands.eCommandType.UnrecognizedDtmfHandler:
					case Commands.eCommandType.UnrecognizedSpeechHandler:
						break;

					default:
						Console.Error.WriteLine(String.Format("{0} DialogBuilder.SpeechOptions: Unknown CommandType encountered ({1})", DateTime.Now, command.CommandType));					//$$$ LP - Write to log instead of console.
						break;
				}
			}
		}

		private void AudioElement(string i_sWaveUrl, string i_sTtsText)
		{
            m_xmlTextWriter.WriteStartElement("audio");
            
            if (NotNullOrEmpty(i_sWaveUrl) || 
				NotNullOrEmpty(i_sTtsText))
			{
				if (NotNullOrEmpty(i_sWaveUrl))
				{
					m_xmlTextWriter.WriteAttributeString("src", i_sWaveUrl);
				}

				if (NotNullOrEmpty(i_sTtsText))
				{
					m_xmlTextWriter.WriteString(i_sTtsText);
				}
			}

            m_xmlTextWriter.WriteEndElement();
        }

        private void AudioElementDynamic(string i_sWaveUrlExpression, string i_sTtsTextExpression)
        {
            m_xmlTextWriter.WriteStartElement("audio");

            if (NotNullOrEmpty(i_sWaveUrlExpression) || 
                NotNullOrEmpty(i_sTtsTextExpression))
            {
                if (NotNullOrEmpty(i_sWaveUrlExpression))
                {
                    m_xmlTextWriter.WriteAttributeString("expr", i_sWaveUrlExpression);
                }

                if (NotNullOrEmpty(i_sTtsTextExpression))
                {
                    m_xmlTextWriter.WriteStartElement("value");
                    m_xmlTextWriter.WriteAttributeString("expr", i_sTtsTextExpression);
                    m_xmlTextWriter.WriteEndElement();
                }
            }

            m_xmlTextWriter.WriteEndElement();
        }

		private void GotoElement(string i_sDestinationUrl)
		{
			m_xmlTextWriter.WriteStartElement("goto");
			m_xmlTextWriter.WriteAttributeString("next", i_sDestinationUrl);
			m_xmlTextWriter.WriteEndElement();
		}

        private void GotoElementDynamic(string i_sDestinationUrlExpression)
        {
            m_xmlTextWriter.WriteStartElement("goto");
            m_xmlTextWriter.WriteAttributeString("expr", i_sDestinationUrlExpression);
            m_xmlTextWriter.WriteEndElement();
        }

        private void ScriptElementIncrementVariable(string i_sVariable)
        {
            m_xmlTextWriter.WriteStartElement("script");
            m_xmlTextWriter.WriteString(String.Format("{0} = {0} + 1;", i_sVariable));
            m_xmlTextWriter.WriteEndElement();
        }

		private void ScriptElementCode(string i_sCode)
		{
			m_xmlTextWriter.WriteStartElement("script");
			m_xmlTextWriter.WriteString(String.Format("{0}", i_sCode));
			m_xmlTextWriter.WriteEndElement();
		}

		private void Response(Commands.Command i_command, bool bIsConfirmationEnabled)
		{
			switch (i_command.OperationType)
			{
				case Commands.eOperationType.DoNothing:
					GotoElement(CurrentFormUrl());
					break;

				case Commands.eOperationType.Repeat:
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
					GotoElement(CurrentFormUrl());
					break;

				case Commands.eOperationType.Transfer:
                    if (bIsConfirmationEnabled)
                    {
                        m_xmlTextWriter.WriteStartElement("script");
                        m_xmlTextWriter.WriteString(String.Format("oSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("iSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteEndElement();
                        IfElementStart(String.Format("iSAConf > '{0}'", m_VxmlGenerationParameters.ConfirmationCutoffPercentage));
                        TransferElementStart(i_command.Response);
                        PromptElementStart(eBargeIn.Disabled);
                        AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                        PromptElementEnd();
                        TransferElementEnd();
                        ElseElement();
                        m_xmlTextWriter.WriteStartElement("script");
                        m_xmlTextWriter.WriteString(String.Format("iNumConfirmMisses = 0;{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATarget = oSAUtils.CopyString(\"{0}\");{1}", i_command.Response, Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATargetWavefile = oSAUtils.CopyString(\"{0}\");{1}", i_command.ConfirmationWavUrl, Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATargetTts = oSAUtils.CopyString(\"{0}\");{1}", i_command.ConfirmationText, Environment.NewLine));
                        m_xmlTextWriter.WriteEndElement();
                        GotoElement("#ConfirmTransfer");
                        IfElementEnd();
                    }
                    else
                    {
                        TransferElementStart(i_command.Response);
                        PromptElementStart(eBargeIn.NotSpecified);
                        AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                        PromptElementEnd();
                        TransferElementEnd();
                    }
					break;

				case Commands.eOperationType.GotoMenu:
                    if (bIsConfirmationEnabled)
                    {
                        m_xmlTextWriter.WriteStartElement("script");
                        m_xmlTextWriter.WriteString(String.Format("oSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("iSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteEndElement();
                        IfElementStart(String.Format("iSAConf > '{0}'", m_VxmlGenerationParameters.ConfirmationCutoffPercentage));
                        AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                        GotoElement(i_command.Response);
                        ElseElement();
                        m_xmlTextWriter.WriteStartElement("script");
                        m_xmlTextWriter.WriteString(String.Format("iNumConfirmMisses = 0;{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATarget = oSAUtils.CopyString(\"{0}\");{1}", i_command.Response, Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATargetWavefile = oSAUtils.CopyString(\"{0}\");{1}", i_command.ConfirmationWavUrl, Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATargetTts = oSAUtils.CopyString(\"{0}\");{1}", i_command.ConfirmationText, Environment.NewLine));
                        m_xmlTextWriter.WriteEndElement();
                        GotoElement("#ConfirmGoto");
                        IfElementEnd();
                    }
                    else
                    {
                        AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                        GotoElement(i_command.Response);
                    }
					break;

				case Commands.eOperationType.Prompt:
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
					break;

				case Commands.eOperationType.CodeBlock:
					WriteRaw(i_command.Response);
					break;

                case Commands.eOperationType.Hangup:
                    if (bIsConfirmationEnabled)
                    {
                        m_xmlTextWriter.WriteStartElement("script");
                        m_xmlTextWriter.WriteString(String.Format("oSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("iSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteEndElement();
                        IfElementStart(String.Format("iSAConf > '{0}'", m_VxmlGenerationParameters.ConfirmationCutoffPercentage));
                        AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                        ExitElement();
                        ElseElement();
                        m_xmlTextWriter.WriteStartElement("script");
                        m_xmlTextWriter.WriteString(String.Format("iNumConfirmMisses = 0;{0}", Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATargetWavefile = oSAUtils.CopyString(\"{0}\");{1}", i_command.ConfirmationWavUrl, Environment.NewLine));
                        m_xmlTextWriter.WriteString(String.Format("sSATargetTts = oSAUtils.CopyString(\"{0}\");{1}", i_command.ConfirmationText, Environment.NewLine));
                        m_xmlTextWriter.WriteEndElement();
                        GotoElement("#ConfirmHangup");
                        IfElementEnd();
                    }
                    else
                    {
                        AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                        ExitElement();
                    }
                    break;

                default:
					Console.Error.WriteLine(String.Format("{0} DialogBuilder.WriteResponse: Unknown OperationType encountered ({1})", DateTime.Now, i_command.OperationType));					//$$$ LP - Write to log instead of console.
					break;
			}
		}

		private void ProcessIntroBlockCommand(Commands.Command i_command)
		{
			switch (i_command.OperationType)
			{
				case Commands.eOperationType.Prompt:
					if (NotNullOrEmpty(i_command.ConfirmationText) || NotNullOrEmpty(i_command.ConfirmationWavUrl))
					{
						PromptElementStart(eBargeIn.NotSpecified);
						AudioElement(i_command.ConfirmationWavUrl, PunctuateLines(i_command.ConfirmationText));
						PromptElementEnd();
					}
					break;

				case Commands.eOperationType.CodeBlock:
					WriteRaw(i_command.Response);
					break;

                case Commands.eOperationType.Hangup:
 					if (NotNullOrEmpty(i_command.ConfirmationText) || NotNullOrEmpty(i_command.ConfirmationWavUrl))
					{
						PromptElementStart(eBargeIn.NotSpecified);
						AudioElement(i_command.ConfirmationWavUrl, PunctuateLines(i_command.ConfirmationText));
						PromptElementEnd();
					}
                    ExitElement();
                    break;

				case Commands.eOperationType.DoNothing:
				case Commands.eOperationType.GotoMenu:
				case Commands.eOperationType.Repeat:
				case Commands.eOperationType.Transfer:
					break;

				default:
					Console.Error.WriteLine(String.Format("{0} DialogBuilder.ProcessIntroBlockCommand: Unknown OperationType encountered ({1})", DateTime.Now, i_command.OperationType));					//$$$ LP - Write to log instead of console.
					break;
			}
		}

		private void UnrecognizedResponse(Commands.Command i_command)
		{
            ElseElement();

			switch (i_command.OperationType)
			{
				case Commands.eOperationType.DoNothing:
					GotoElement(CurrentFormUrl());
					break;

				case Commands.eOperationType.GotoMenu:
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
					GotoElement(i_command.Response);
					break;

				case Commands.eOperationType.Repeat:
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
					GotoElement(CurrentFormUrl());
					break;

				case Commands.eOperationType.Transfer:
					TransferElementStart(i_command.Response);
					PromptElementStart(eBargeIn.NotSpecified);
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
					PromptElementEnd();
					TransferElementEnd();
					break;

				case Commands.eOperationType.CodeBlock:
					if (String.Empty == i_command.Response)
					{
						// Cannot have an empty default for speech since if it hits then user will stay in MOH since an empty else clause doesn't cause any action (hence no Inactivity Timer restart).
						// Shouldn't have an empty default for DTMF since if it hits then the system waits until the Inactivity Timer expires before responding, rather than once the DTMF end detection has happened.

						GotoElement(CurrentFormUrl());
					}
					else
					{
						WriteRaw(i_command.Response);
					}
					break;

                case Commands.eOperationType.Hangup:
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
                    ExitElement();
                    break;

				case Commands.eOperationType.Prompt:
					AudioElement(i_command.ConfirmationWavUrl, i_command.ConfirmationText);
					break;

				default:
					Console.Error.WriteLine(String.Format("{0} DialogBuilder.UnrecognizedResponse: Unknown OperationType encountered ({1})", DateTime.Now, i_command.OperationType));					//$$$ LP - Write to log instead of console.
					break;
			}
		}

		private void WriteRaw(string i_sRawText)
		{
			if (NotNullOrEmpty(i_sRawText))
			{
				if (IsVoiceXml(i_sRawText))
				{
					m_xmlTextWriter.WriteRaw(Environment.NewLine);
					m_xmlTextWriter.WriteRaw(i_sRawText);
					if (!i_sRawText.EndsWith(Environment.NewLine))
					{
						m_xmlTextWriter.WriteRaw(Environment.NewLine);
					}
				}
				else
				{
					PromptElementStart(eBargeIn.NotSpecified);

					// Need an <audio> element inside the <prompt> element as 
					// without it the system does not speak the text.

					AudioElement("", PunctuateLines(i_sRawText));
					PromptElementEnd();
				}
			}
		}

		private bool IsVoiceXml(string i_sText)
		{
			return i_sText.Trim().StartsWith("<");
		}

		private void GrammarElement(string i_sGrammarLocation)
		{
			m_xmlTextWriter.WriteStartElement("grammar");
			m_xmlTextWriter.WriteAttributeString("type", "application/srgs");
			m_xmlTextWriter.WriteAttributeString("src", i_sGrammarLocation);
			m_xmlTextWriter.WriteEndElement();
		}

		private void ProcessCommands()
		{
			if (!m_commands.HasSpeechCommands)
			{
				// Cannot compare to empty string since that causes an error (see Bug #532 in Mantis).

				IfElementStart("callee == ' '");
			}
			else
			{
				bool bUseElseIf = false;

				foreach (Commands.Command command in m_commands)
				{
					switch (command.CommandType)
					{
						case Commands.eCommandType.Speech:
							{
								string sCondition = String.Format("callee == '{0}'", command.CommandOption);

								ProcessCommand(sCondition, command, m_menu.ConfirmationEnabled, ref bUseElseIf);
							}
							break;

						case Commands.eCommandType.Dtmf:
							if (m_commands.IsDtmfCommandSpeakable(command))
							{
								foreach (DtmfKeyToSpokenEquivalentMapping dtmf in m_DtmfToSpokenMapping)
								{
									if (dtmf.Key == command.CommandOption)
									{
										// Only add spoken equivalent if user hasn't already specified is explicitly.

										if (!m_commands.HasCommand(dtmf.SpokenEquivalent))
										{
											string sCondition = String.Format("callee == '{0}'", dtmf.SpokenEquivalent);
											ProcessCommand(sCondition, command, m_menu.ConfirmationEnabled, ref bUseElseIf);
										}
									}
								}
							}
							break;

						case Commands.eCommandType.IntroBlock:
						case Commands.eCommandType.Touch:
						case Commands.eCommandType.UnrecognizedDtmfHandler:
						case Commands.eCommandType.UnrecognizedSpeechHandler:
							break;

						default:
							Console.Error.WriteLine(String.Format("{0} DialogBuilder.ProcessCommands: Unknown CommandType encountered ({1})", DateTime.Now, command.CommandType));					//$$$ LP - Write to log instead of console.
							break;
					}
				}
			}


			// Process any DTMF commands we might have.

			ElseIfElement("callee$.inputmode == 'dtmf'");

			if (!m_commands.HasDtmfCommands)
			{
                // Cannot compare to empty string since that causes an error (see Bug #532 in Mantis).
                
                IfElementStart("callee$.utterance == ' '");
			}
			else
			{
				bool bUseElseIf = false;

				foreach (Commands.Command command in m_commands)
				{
					if (Commands.eCommandType.Dtmf == command.CommandType)
					{
						string sCondition = String.Format("callee$.utterance == '{0}'", command.CommandOption);

						ProcessCommand(sCondition, command, false, ref bUseElseIf); 
					}
				}
			}

			UnrecognizedDTMFResponse();
			IfElementEnd();

			Comment("Unrecognized handler");
			UnrecognizedSpeechResponse();

			IfElementEnd();
		}

		private void ProcessCommand(string i_sCondition, Commands.Command i_command, bool i_bIsConfirmationEnabled, ref bool io_bUseElseIf)
		{
			if (!io_bUseElseIf)
			{
				IfElementStart(i_sCondition);
				io_bUseElseIf = true;
			}
			else
			{
				ElseIfElement(i_sCondition);
			}

			Response(i_command, i_bIsConfirmationEnabled);
		}

		private void UnrecognizedDTMFResponse()
		{
			foreach (Commands.Command command in m_commands)
			{
				switch (command.CommandType)
				{
					case Commands.eCommandType.UnrecognizedDtmfHandler:
						UnrecognizedResponse(command);
						break;

					case Commands.eCommandType.Dtmf:
					case Commands.eCommandType.IntroBlock:
					case Commands.eCommandType.Speech:
					case Commands.eCommandType.Touch:
					case Commands.eCommandType.UnrecognizedSpeechHandler:
						break;

					default:
						Console.Error.WriteLine(String.Format("{0} DialogBuilder.UnrecognizedDTMFResponse: Unknown CommandType encountered ({1})", DateTime.Now, command.CommandType));					//$$$ LP - Write to log instead of console.
						break;
				}
			}
		}

		private void UnrecognizedSpeechResponse()
		{
			foreach (Commands.Command command in m_commands)
			{
				switch (command.CommandType)
				{
					case Commands.eCommandType.UnrecognizedSpeechHandler:
						UnrecognizedResponse(command);
						break;

					case Commands.eCommandType.Dtmf:
					case Commands.eCommandType.IntroBlock:
					case Commands.eCommandType.Speech:
					case Commands.eCommandType.Touch:
					case Commands.eCommandType.UnrecognizedDtmfHandler:
						break;

					default:
						Console.Error.WriteLine(String.Format("{0} DialogBuilder.UnrecognizedSpeechResponse: Unknown CommandType encountered ({1})", DateTime.Now, command.CommandType));					//$$$ LP - Write to log instead of console.
						break;
				}
			}
		}

		private bool UseGrammar()
		{
			return NotNullOrEmpty(m_menu.GrammarUrl);
		}

		private string GetVersion()
		{
			return Assembly.GetAssembly(typeof(AppGenerator.DialogBuilder)).GetName().Version.ToString();
		}

		private bool NotNullOrEmpty(string i_string)
		{
			return !String.IsNullOrEmpty(i_string);
		}

		private string CurrentFormUrl()
		{
			return String.Format("#{0}", FormName);
		}

		private string PunctuateLines(string i_sInputString)
		{
			Regex regexLineEndsInPunctuation = new Regex(@"[\.\?!,:;]$");
			string[] lines = i_sInputString.Split(new char[] {'\n'});

			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i].Trim();

				if (line.Length > 0)
				{
					if (!regexLineEndsInPunctuation.IsMatch(line))
					{
						line = String.Format("{0}.", line);
					}
				}

				lines[i] = line;
			}
			
			return String.Join(Environment.NewLine, lines);
		}

		private void CheckForUnimplementedFeatures()			//$$$ LP - Remove once features are implemented.
		{
			if (m_commands.HasTouchCommands)
			{
				string message = String.Format("Menu '{0}' contains Touch commands.  They are currently not implemented and will be ignored.", m_menu.MenuName);
				Console.Error.WriteLine(message);		//$$$ LP - Write to log instead of console.
			}

			Commands macroCommands = new Commands();

			foreach (Commands.Command command in m_commands)
			{
				if (IsVoiceXml(command.CommandOption))
				{
					macroCommands.Add(command);
				}
			}

			foreach (Commands.Command command in macroCommands)
			{
				m_commands.Remove(command);
			}

			if (macroCommands.Count > 0)
			{
				string message = String.Format("Menu '{0}' contains 'Macro' commands.  They are currently not implemented and will be ignored.", m_menu.MenuName);
				Console.Error.WriteLine(message);		//$$$ LP - Write to log instead of console.
			}
		}

        private void ConfirmationVariables()
        {
            VariableElement("oSAUtils", "");
            VariableElement("iSAConf", "0");
            VariableElement("sSAUtt", "");
            VariableElement("sSATarget", "");
            VariableElement("sSATargetWavefile", "");
            VariableElement("sSATargetTts", "");
            VariableElement("iNumConfirmMisses", "0");
            VariableElement("iNumConfirmRetries", "0");
        }

        private void VariableElement(string i_Name, string i_sValue)
        {
            m_xmlTextWriter.WriteStartElement("var");

            m_xmlTextWriter.WriteAttributeString("name", i_Name);
            m_xmlTextWriter.WriteAttributeString("expr", i_sValue);

            m_xmlTextWriter.WriteEndElement();
        }

        private void ConfirmationForms()
        {
            ConfirmationFormGoto();
            ConfirmationFormHangup();
            ConfirmationFormTransfer();
        }

        private void ConfirmationFormHangup()
        {
            Comment("Confirmation for Hangup Command.");
            FormElementStart("ConfirmHangup");
            ConfirmationIntroBlock();
            FieldElementStart("confirmation");
            GrammarElement("file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram");
            FilledElementStart();
            IfElementStart("confirmation == 'true'");
            AudioElementDynamic("sSATargetWavefile", "sSATargetTts");
            ExitElement();
            ElseIfElement("confirmation == 'false'");
            ConfirmationFalse();
            ElseIfElement("confirmation == 'main menu'");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav", "Main menu.");
            GotoElement(CurrentFormUrl());
            ElseIfElement("confirmation$.utterance == '1'");
            AudioElementDynamic("sSATargetWavefile", "sSATargetTts");
            ExitElement();
            ElseIfElement("confirmation$.utterance == '2'");
            ConfirmationFalse();
            ElseElement();
            ConfirmationUnrecognizedResponse();
            IfElementEnd();
            FilledElementEnd();
            FieldElementEnd();
            FormElementEnd();
        }

        private void ConfirmationFormGoto()
        {
            Comment("Confirmation for Goto Command.");
            FormElementStart("ConfirmGoto");
            ConfirmationIntroBlock();
            FieldElementStart("confirmation");
            GrammarElement("file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram");
            FilledElementStart();
            IfElementStart("confirmation == 'true'");
            AudioElementDynamic("sSATargetWavefile", "sSATargetTts");
            GotoElementDynamic("sSATarget");
            ElseIfElement("confirmation == 'false'");
            ConfirmationFalse();
            ElseIfElement("confirmation == 'main menu'");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav", "Main menu.");
            GotoElement(CurrentFormUrl());
            ElseIfElement("confirmation$.utterance == '1'");
            AudioElementDynamic("sSATargetWavefile", "sSATargetTts");
            GotoElementDynamic("sSATarget");
            ElseIfElement("confirmation$.utterance == '2'");
            ConfirmationFalse();
            ElseElement();
            ConfirmationUnrecognizedResponse();
            IfElementEnd();
            FilledElementEnd();
            FieldElementEnd();
            FormElementEnd();
        }

        private void ConfirmationFormTransfer()
        {
            Comment("Confirmation for Transfer Command.");
            FormElementStart("ConfirmTransfer");
            ConfirmationIntroBlock();
            FieldElementStart("confirmation");
            GrammarElement("file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram");
            FilledElementStart();
            IfElementStart("confirmation == 'true'");
            TransferElementStart("application$.sSATarget");
            PromptElementStart(eBargeIn.Disabled);
            AudioElementDynamic("sSATargetWavefile", "sSATargetTts");
            PromptElementEnd();
            TransferElementEnd();
            ElseIfElement("confirmation == 'false'");
            ConfirmationFalse();
            ElseIfElement("confirmation == 'main menu'");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav", "Main menu.");
            GotoElement(CurrentFormUrl());
            ElseIfElement("confirmation$.utterance == '1'");
            TransferElementStart("application$.sSATarget");
            PromptElementStart(eBargeIn.Disabled);
            AudioElementDynamic("sSATargetWavefile", "sSATargetTts");
            PromptElementEnd();
            TransferElementEnd();
            ElseIfElement("confirmation$.utterance == '2'");
            ConfirmationFalse();
            ElseElement();
            ConfirmationUnrecognizedResponse();
            IfElementEnd();
            FilledElementEnd();
            FieldElementEnd();
            FormElementEnd();
        }

        private void ConfirmationIntroBlock()
        {
            m_xmlTextWriter.WriteStartElement("block");

            PromptElementStart(eBargeIn.Enabled);
            AudioElementDynamic("", "sSAUtt");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav", "");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/IsThatCorrect.wav", "Is that correct?");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav", "");
            PromptElementEnd();

            m_xmlTextWriter.WriteEndElement();
        }

        private void ConfirmationFalse()
        {
            IfElementStart(String.Format("iNumConfirmRetries == '{0}'", m_VxmlGenerationParameters.NumberOfRetries));
            TransferToOperator();
            ElseElement();
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav", "OK, let's try that again.");
            ScriptElementIncrementVariable("iNumConfirmRetries");
            IfElementEnd();
            GotoElement(CurrentFormUrl());
        }

        private void ConfirmationUnrecognizedResponse()
        {
            IfElementStart(String.Format("iNumConfirmMisses == '{0}'", m_VxmlGenerationParameters.NumberOfRetries));
            TransferToOperator();
            ElseElement();
            PromptElementStart(eBargeIn.Disabled);
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/ImSorryWasThat.wav", "I'm sorry, was that");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav", "");
            AudioElementDynamic("", "sSAUtt");
            PromptElementEnd();
			ScriptElementCode(string.Format("{0}\t// This script element is used to allow a second prompt element to flip barge-in behavior.{0}", Environment.NewLine));
			PromptElementStart(eBargeIn.Enabled);
			AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav", "");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/PleaseSayYesOrNo.wav", "Please say, yes, or, no.");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav", "");
            PromptElementEnd();
            ScriptElementIncrementVariable("iNumConfirmMisses");
            IfElementEnd();
        }

        private void TransferToOperator()
        {
            TransferElementStart(m_VxmlGenerationParameters.OperatorExtension);
            PromptElementStart(eBargeIn.Disabled);
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav", "I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.");
            AudioElement("file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav", "");
            PromptElementEnd();
            TransferElementEnd();
        }

        private static VxmlGenerationParameters GetVxmlGenerationParameters()
        {
            VxmlGenerationDAL vxmlGenerationDAL = new VxmlGenerationDAL(null);

            return vxmlGenerationDAL.GetVxmlGenerationParameters();
        }
    }
}
