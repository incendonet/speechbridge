// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using Incendonet.Utilities.LogClient;

namespace SBConfigStor
{
	public sealed class VoiceScriptGen
	{
		private ILegacyLogger m_Logger = null;

		public const string MORE_OPTIONS_MENU = "AAMainMoreOptions";

		public sealed class Result
		{
			private bool m_bSuccess;
			private string m_sMessage;

			public Result(bool i_bSuccess, string i_sMessage)
			{
				m_bSuccess = i_bSuccess;
				m_sMessage = i_sMessage;
			}

			public bool Success
			{
				get { return m_bSuccess; }
			}

			public string Message
			{
				get { return m_sMessage; }
			}
		} // Result

		public VoiceScriptGen() : this(null)
		{
		}

		public VoiceScriptGen(ILegacyLogger i_Logger)
		{
			m_Logger = i_Logger;
		}

		public bool IsValidUserForAutomatedAttendant(Users.User i_user)					//$$$ LP - Should this be a method on Users.User?
		{
			bool bRet = false;

			if (((i_user.FName.Length > 0) || (i_user.LName.Length > 0)) && 
				(i_user.Ext.Length > 0))
			{
				bRet = true;
			}

			return bRet;
		}

		public bool IsValidUserForMessaging(Users.User i_user)							//$$$ LP - Should this be a method on Users.User?
		{
			bool bRet = false;

			if (((i_user.FName.Length > 0) || (i_user.LName.Length > 0)) && 
				(i_user.Username.Length > 0) && 
				(i_user.Domain.Length > 0))
			{
				bRet = true;
			}

			return bRet;
		}

		public string GetLastNDigits(string i_sPhoneNumber, int i_iMaximumNumberOfDigitsToReturn)
		{
			string sLastNDigits = "";
			StringBuilder sbDigitsOnly = new StringBuilder();

			foreach (char c in i_sPhoneNumber)
			{
				if (char.IsDigit(c))
				{
					sbDigitsOnly.Append(c);
				}
			}

			string sDigitsOnly = sbDigitsOnly.ToString();

			if (sDigitsOnly.Length > i_iMaximumNumberOfDigitsToReturn)
			{
				sLastNDigits = sDigitsOnly.Substring(sDigitsOnly.Length - i_iMaximumNumberOfDigitsToReturn);
			}
			else
			{
				sLastNDigits = sDigitsOnly;
			}

			return sLastNDigits;
		} // GetLastNDigits

        public static string GetVxmlComparisonSafeName(string i_sName)
        {
            return StringFilter.RemoveApostrophes(i_sName);
        }

        public static string GetFilenameSafeName(string i_sName)
        {
            return StringFilter.RemoveSpaces(StringFilter.RemoveApostrophes(i_sName));
        }

		public List<Result> GenerateVxmlsFromTemplates()
		{
			List<Result> results = new List<Result>();

			try
			{
				const string csTemplateIndicator = "_Template.vxml.xml";

				string[] sTemplateFiles = System.IO.Directory.GetFiles(FileSystemSupport.VxmlPath, String.Format("*{0}", csTemplateIndicator));

				if (sTemplateFiles.Length > 0)
				{
					Users oUsers = GetUsers();

					VxmlGenerationParameters vxmlGenerationParameters = GetVxmlGenerationParameters();
					Log(Level.Info, String.Format("VXML Generation Parameters used -- {0}", vxmlGenerationParameters.ToString()));

					foreach (string sTemplateFile in sTemplateFiles)
					{
						string sFinalFile = sTemplateFile.Replace(csTemplateIndicator, ".vxml.xml");

						try
						{
							using (StreamReader fileTemplate = new StreamReader(sTemplateFile, Encoding.UTF8))
							using (StreamWriter fileFinal = new StreamWriter(sFinalFile, false, Encoding.UTF8))
							{
								GenerateVxmlFromTemplate(fileTemplate, fileFinal, oUsers, vxmlGenerationParameters);
							}

							results.Add(new Result(true, String.Format("{0} voice script file generation successful.", Path.GetFileName(sFinalFile))));
						}
						catch (Exception exc)
						{
							results.Add(new Result(false, String.Format("There was an error creating the {0} voice script.", Path.GetFileName(sFinalFile))));

							Log(Level.Exception, String.Format("GenerateVxmlsFromTemplates: Caught exception (1): {0}", exc.ToString()));
						}
					}
				}
			}
			catch (Exception exc)
			{
				Log(Level.Exception, String.Format("GenerateVxmlsFromTemplates: Caught exception (2): {0}", exc.ToString()));
			}

			return results;
		} // GenerateVxmlsFromTemplates

		public void GenerateVxmlFromTemplate(TextReader i_trTemplate, TextWriter i_twFinal, Users i_users, VxmlGenerationParameters i_vxmlGenerationParameters)
		{
			const string csNamesTag = "<NAMES_LIST/>";
			const string csTransferTag = "<NAMES_TRANSFERTO/>";
			const string csOperMaxRetries = "<SA_OPERATOREXT_MAXRETRIES/>";
			const string csOperDtmf = "<SA_OPERATOREXT_DTMF/>";
			const string csIntroBargein = "<SA_INTRO_BARGEIN/>";
			const string csCustomCmdsList = "<CUSTOMCOMMANDS_LIST/>";
			const string csCustomCmdsDefs = "<CUSTOMCOMMANDS_DEFINITIONS/>";
			const string csMoreOptionsCommand = "<MOREOPTIONS_COMMAND/>";
			const string csMoreOptionsSpeech = "<MOREOPTIONS_SPEECH/>";
			const string csMoreOptionsGoto = "<MOREOPTIONS_GOTO";										// Needs an attribute so can't check for any more than this.
			const string csTransferTagWithConfirmation = "<NAMES_TRANSFERTO_WITH_CONFIRMATION/>";
			const string csConfirmationVariables = "<CONFIRMATION_VARIABLES/>";
			const string csConfirmationForm = "<CONFIRMATION_FORM";										// Needs an attribute so can't check for any more than this.
			const string csGroupTag = "<GROUP";															// Needs an attribute so can't check for any more than this.
            const string csNumberInDirectory = "<IS_NUMBER_IN_DIRECTORY";                               // Has attributes so can't check for any more than this.      

			const string csOperMaxRetriesOrig = "<if cond = \"iNumMisses == '";
			const string csTransferDest = "<transfer dest=";

			const string csCollabCmd_ReadEmail = "<option>read email</option>";
			const string csCollabCmd_GetEmail = "<option>get email</option>";
			const string csCollabCmd_CheckEmail = "<option>check email</option>";
			const string csCollabCmd_GoToEmail = "<option>go to email</option>";
			const string csCollabCmd_CheckCalendar = "<option>check calendar</option>";
			const string csCollabCmd_GoToCalendar = "<option>go to calendar</option>";

			const string csEmailServerTag = "<EMAIL_SERVER/>";
			const string csEmailNamesTag = "<NAMES_EMAIL_LIST/>";
			const string csEmailAcctInfoTag = "<NAMES_EMAIL_ACCT_INFO/>";

			string[] csMobileNumberFormatStrings = 
							{ 
								"{0} mobile", 
								"{0} cell" 
							};
			string[] csPagerNumberFormatStrings = 
							{ 
								"{0} pager", 
								"page {0}" 
							};

			int iCountNamesList = 0;
			int iCountNamesTransferTo = 0;
			int iCountCustomsCommandsList = 0;
			int iCountCustomsCommandsDefinitions = 0;
			int iCountMoreOptionsCommand = 0;
			int iCountMoreOptionsSpeech = 0;
			int iCountMoreOptionsGoto = 0;
			int iCountNamesTransferToWithConfirmation = 0;
			int iCountConfirmationVariables = 0;
			int iCountConfirmationForm = 0;
			int iCountNamesEmailList = 0;
			int iCountNamesEmailAccountInfo = 0;
			int iCountGroupsTag = 0;
			bool bGroupsTagInWrongPlace = false;

			string sGroupName = Group.ALL;																			// Use all non-hidden contacts unless we are otherwise instructed via <GROUP />

			StringBuilder sbTmp = new StringBuilder();
			string sText = "";

			while ((sText = i_trTemplate.ReadLine()) != null)
			{
				if (sText.EndsWith(csNamesTag))	// Fill in the 'options' section
				{
					++iCountNamesList;

					foreach (Users.User user in i_users)
					{
						if (IsUserInGroup(user, sGroupName))
						{
							if (IsValidUserForAutomatedAttendant(user))
							{
								i_twFinal.WriteLine(GenerateOptionElement(user.GetFullName()));

								if (user.HasMobileNumber)
								{
									foreach (string sFormatString in csMobileNumberFormatStrings)
									{
										string optionContent = String.Format(sFormatString, user.GetFullName());
										i_twFinal.WriteLine(GenerateOptionElement(optionContent));
									}
								}

								if (user.HasPagerNumber)
								{
									foreach (string sFormatString in csPagerNumberFormatStrings)
									{
										string optionContent = String.Format(sFormatString, user.GetFullName());
										i_twFinal.WriteLine(GenerateOptionElement(optionContent));
									}
								}

								foreach (string sAlternatePronunciation in user.AltPronunciationColl)
								{
									i_twFinal.WriteLine(GenerateOptionElement(sAlternatePronunciation));

									if (user.HasMobileNumber)
									{
										foreach (string sFormatString in csMobileNumberFormatStrings)
										{
											string optionContent = String.Format(sFormatString, sAlternatePronunciation);
											i_twFinal.WriteLine(GenerateOptionElement(optionContent));
										}
									}

									if (user.HasPagerNumber)
									{
										foreach (string sFormatString in csPagerNumberFormatStrings)
										{
											string optionContent = String.Format(sFormatString, sAlternatePronunciation);
											i_twFinal.WriteLine(GenerateOptionElement(optionContent));
										}
									}
								}
							}
						}
					}
				}
				else if (sText.EndsWith(csTransferTag))	// Fill in the transfer section
				{
					++iCountNamesTransferTo;

					foreach (Users.User user in i_users)
					{
						if (IsUserInGroup(user, sGroupName))
						{
							if (IsValidUserForAutomatedAttendant(user))
							{
								i_twFinal.WriteLine(GenerateTransferElement(user.GetFullName(), user.GetFullName(), user.GetFullName(), GetLastNDigits(user.Ext, i_vxmlGenerationParameters.NumberOfDigitsForExtension)));

								if (user.HasMobileNumber)
								{
									foreach (string sFormatString in csMobileNumberFormatStrings)
									{
										string matchingName = String.Format(sFormatString, user.GetFullName());
										i_twFinal.WriteLine(GenerateTransferElement(matchingName, matchingName, matchingName, user.MobileNumber));
									}
								}

								if (user.HasPagerNumber)
								{
									foreach (string sFormatString in csPagerNumberFormatStrings)
									{
										string matchingName = String.Format(sFormatString, user.GetFullName());
										i_twFinal.WriteLine(GenerateTransferElement(matchingName, matchingName, matchingName, user.PagerNumber));
									}
								}

								foreach (string sAlternatePronunciation in user.AltPronunciationColl)
								{
									i_twFinal.WriteLine(GenerateTransferElement(sAlternatePronunciation, user.GetFullName(), sAlternatePronunciation, GetLastNDigits(user.Ext, i_vxmlGenerationParameters.NumberOfDigitsForExtension)));

									if (user.HasMobileNumber)
									{
										foreach (string sFormatString in csMobileNumberFormatStrings)
										{
											string matchingName = String.Format(sFormatString, sAlternatePronunciation);
											i_twFinal.WriteLine(GenerateTransferElement(matchingName, matchingName, matchingName, user.MobileNumber));
										}
									}

									if (user.HasPagerNumber)
									{
										foreach (string sFormatString in csPagerNumberFormatStrings)
										{
											string matchingName = String.Format(sFormatString, sAlternatePronunciation);
											i_twFinal.WriteLine(GenerateTransferElement(matchingName, matchingName, matchingName, user.PagerNumber));
										}
									}
								}
							}
						}
					}
				}
				else if (sText.EndsWith(csTransferTagWithConfirmation))	// Fill in the transfer with confirmation section
				{
					++iCountNamesTransferToWithConfirmation;

					foreach (Users.User user in i_users)
					{
						if (IsUserInGroup(user, sGroupName))
						{
							if (IsValidUserForAutomatedAttendant(user))
							{
								i_twFinal.WriteLine(GenerateTransferElementWithConfirmation(user.GetFullName(), user.GetFullName(), user.GetFullName(), GetLastNDigits(user.Ext, i_vxmlGenerationParameters.NumberOfDigitsForExtension), i_vxmlGenerationParameters.ConfirmationCutoffPercentage));

								if (user.HasMobileNumber)
								{
									foreach (string sFormatString in csMobileNumberFormatStrings)
									{
										string matchingName = String.Format(sFormatString, user.GetFullName());
										i_twFinal.WriteLine(GenerateTransferElementWithConfirmation(matchingName, matchingName, matchingName, user.MobileNumber, i_vxmlGenerationParameters.ConfirmationCutoffPercentage));
									}
								}

								if (user.HasPagerNumber)
								{
									foreach (string sFormatString in csPagerNumberFormatStrings)
									{
										string matchingName = String.Format(sFormatString, user.GetFullName());
										i_twFinal.WriteLine(GenerateTransferElementWithConfirmation(matchingName, matchingName, matchingName, user.PagerNumber, i_vxmlGenerationParameters.ConfirmationCutoffPercentage));
									}
								}

								foreach (string sAlternatePronunciation in user.AltPronunciationColl)
								{
									i_twFinal.WriteLine(GenerateTransferElementWithConfirmation(sAlternatePronunciation, user.GetFullName(), sAlternatePronunciation, GetLastNDigits(user.Ext, i_vxmlGenerationParameters.NumberOfDigitsForExtension), i_vxmlGenerationParameters.ConfirmationCutoffPercentage));

									if (user.HasMobileNumber)
									{
										foreach (string sFormatString in csMobileNumberFormatStrings)
										{
											string matchingName = String.Format(sFormatString, sAlternatePronunciation);
											i_twFinal.WriteLine(GenerateTransferElementWithConfirmation(matchingName, matchingName, matchingName, user.MobileNumber, i_vxmlGenerationParameters.ConfirmationCutoffPercentage));
										}
									}

									if (user.HasPagerNumber)
									{
										foreach (string sFormatString in csPagerNumberFormatStrings)
										{
											string matchingName = String.Format(sFormatString, sAlternatePronunciation);
											i_twFinal.WriteLine(GenerateTransferElementWithConfirmation(matchingName, matchingName, matchingName, user.PagerNumber, i_vxmlGenerationParameters.ConfirmationCutoffPercentage));
										}
									}
								}
							}
						}
					}
				}
				else if (sText.EndsWith(csOperDtmf))			// Fill in the DTMF '0' transfer section.  (For now this is the same as the max-retries, but that may change.)
				{
					i_twFinal.WriteLine(String.Format("\t\t\t\t\t\t<transfer dest=\"sip:{0}\">", i_vxmlGenerationParameters.OperatorExtension));
				}
				else if (sText.Trim().StartsWith(csOperMaxRetriesOrig))	// Fill in the old max-retries transfer section, if found
				{
					// This section of code handles two possible conditional transfer to operator situations:
					//
					// The original (no longer used) VXML looked like this:
					//
					//          <if cond = "iNumMisses == '2'">
					//              <transfer dest="sip:0">
					//
					//
					// The current VXML looks like this:
					//
					//          <if cond = "iNumMisses == '2'">
					//              <SA_OPERATOREXT_MAXRETRIES/>


					sbTmp.Length = 0;
					sbTmp.AppendLine(sText);

					sText = i_trTemplate.ReadLine();
					if ((sText.Trim().StartsWith(csTransferDest)) || (sText.EndsWith(csOperMaxRetries)))
					{
						sbTmp.Length = 0;
						sbTmp.AppendFormat("\t\t\t\t\t<if cond = \"iNumMisses == '{0}'\">{1}", i_vxmlGenerationParameters.NumberOfRetries, Environment.NewLine);
						sbTmp.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:{0}\">", i_vxmlGenerationParameters.OperatorExtension);

						i_twFinal.WriteLine(sbTmp.ToString());
					}
					else
					{
						// Wasn't what we were expecting, just write it out and continue on.
						sbTmp.Append(sText);
						i_twFinal.WriteLine(sbTmp.ToString());
					}
				}
				else if (sText.EndsWith(csOperMaxRetries))	// Fill in the max-retries transfer section (this will be encountered when a transfer to the operator is specified in the VXML that isn't following a conditional (i.e. <if cond = "iNumMisses == '2'">) -- c.f. with code above.)
				{
					i_twFinal.WriteLine(String.Format("\t\t\t\t\t\t<transfer dest=\"sip:{0}\">", i_vxmlGenerationParameters.OperatorExtension));
				}
				else if (sText.EndsWith(csIntroBargein))
				{
					i_twFinal.WriteLine(String.Format("\t\t\t<prompt bargein=\"{0}\">", i_vxmlGenerationParameters.IntroBargein.ToString().ToLower()));
				}
				else if (sText.IndexOf(csCollabCmd_ReadEmail) >= 0)
				{
					i_twFinal.WriteLine(ReplaceCollaborationCommand(csCollabCmd_ReadEmail, i_vxmlGenerationParameters.CollaborationCommandsEnabled));
				}
				else if (sText.IndexOf(csCollabCmd_GetEmail) >= 0)
				{
					i_twFinal.WriteLine(ReplaceCollaborationCommand(csCollabCmd_GetEmail, i_vxmlGenerationParameters.CollaborationCommandsEnabled));
				}
				else if (sText.IndexOf(csCollabCmd_CheckEmail) >= 0)
				{
					i_twFinal.WriteLine(ReplaceCollaborationCommand(csCollabCmd_CheckEmail, i_vxmlGenerationParameters.CollaborationCommandsEnabled));
				}
				else if (sText.IndexOf(csCollabCmd_GoToEmail) >= 0)
				{
					i_twFinal.WriteLine(ReplaceCollaborationCommand(csCollabCmd_GoToEmail, i_vxmlGenerationParameters.CollaborationCommandsEnabled));
				}
				else if (sText.IndexOf(csCollabCmd_CheckCalendar) >= 0)
				{
					i_twFinal.WriteLine(ReplaceCollaborationCommand(csCollabCmd_CheckCalendar, i_vxmlGenerationParameters.CollaborationCommandsEnabled));
				}
				else if (sText.IndexOf(csCollabCmd_GoToCalendar) >= 0)
				{
					i_twFinal.WriteLine(ReplaceCollaborationCommand(csCollabCmd_GoToCalendar, i_vxmlGenerationParameters.CollaborationCommandsEnabled));
				}
				else if (sText.EndsWith(csCustomCmdsList))
				{
					++iCountCustomsCommandsList;

					// Do nothing for now.
				}
				else if (sText.EndsWith(csCustomCmdsDefs))
				{
					++iCountCustomsCommandsDefinitions;

					// Do nothing for now.
				}
				else if (sText.EndsWith(csMoreOptionsCommand))
				{
					++iCountMoreOptionsCommand;

					if (i_vxmlGenerationParameters.MoreOptionsEnabled)
					{
						i_twFinal.WriteLine("\t\t\t<option>more options</option>");
					}
				}
				else if (sText.EndsWith(csMoreOptionsSpeech))
				{
					++iCountMoreOptionsSpeech;

					if (i_vxmlGenerationParameters.MoreOptionsEnabled)
					{
						sbTmp.Length = 0;

						sbTmp.AppendFormat("\t\t\t\t<elseif cond=\"callee == 'more options'\"/>{0}", Environment.NewLine);
						sbTmp.AppendFormat("\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MoreOptions.wav\">more options.</audio>{0}", Environment.NewLine);
						sbTmp.AppendFormat("\t\t\t\t\t<goto next=\"file:///opt/speechbridge/VoiceDocStore/{0}.vxml.xml\"/>{1}", MORE_OPTIONS_MENU, Environment.NewLine);

						i_twFinal.Write(sbTmp.ToString());
					}
				}
				else if (sText.Trim().StartsWith(csMoreOptionsGoto))
				{
					++iCountMoreOptionsGoto;

					sbTmp.Length = 0;

					if (i_vxmlGenerationParameters.MoreOptionsEnabled)
					{
						sbTmp.AppendFormat("\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MoreOptions.wav\">more options.</audio>{0}", Environment.NewLine);
						sbTmp.AppendFormat("\t\t\t\t\t\t<goto next=\"file:///opt/speechbridge/VoiceDocStore/{0}.vxml.xml\"/>{1}", MORE_OPTIONS_MENU, Environment.NewLine);
					}
					else
					{
						sbTmp.AppendFormat("\t\t\t\t\t\t<goto next=\"{0}\"/>{1}", GetAttributeValue("DISABLED_URL", sText), Environment.NewLine);
					}

					i_twFinal.Write(sbTmp.ToString());
				}
				else if (sText.EndsWith(csConfirmationVariables))
				{
					++iCountConfirmationVariables;

					sbTmp.Length = 0;

					sbTmp.AppendFormat("\t<var name = \"oSAUtils\" expr = \"\"/>{0}", Environment.NewLine);
					sbTmp.AppendFormat("\t<var name = \"iSAConf\" expr = \"0\"/>{0}", Environment.NewLine);
					sbTmp.AppendFormat("\t<var name = \"sSAUtt\" expr = \"0\"/>{0}", Environment.NewLine);
					sbTmp.AppendFormat("\t<var name = \"sSADest\" expr = \"0\"/>{0}", Environment.NewLine);
					sbTmp.AppendFormat("\t<var name = \"sSANameWav\" expr = \"0\"/>{0}", Environment.NewLine);
					sbTmp.AppendFormat("\t<var name = \"iNumConfirmMisses\" expr = \"0\"/>{0}", Environment.NewLine);
					sbTmp.AppendFormat("\t<var name = \"iNumConfirmRetries\" expr = \"0\"/>{0}", Environment.NewLine);

					i_twFinal.Write(sbTmp.ToString());
				}
				else if (sText.Trim().StartsWith(csConfirmationForm))
				{
					++iCountConfirmationForm;

					i_twFinal.WriteLine(GenerateConfirmationForm(i_vxmlGenerationParameters.NumberOfRetries, i_vxmlGenerationParameters.OperatorExtension, GetAttributeValue("TRY_AGAIN_URL", sText)));
				}
				else if (sText.Trim().StartsWith(csGroupTag))
				{
					if (i_vxmlGenerationParameters.IsGroupsEnabled)
					{
						++iCountGroupsTag;

						if ((iCountNamesList != 0) || (iCountNamesTransferTo != 0) || (iCountNamesTransferToWithConfirmation != 0) || (iCountNamesEmailList != 0) || (iCountNamesEmailAccountInfo != 0))
						{
							bGroupsTagInWrongPlace = true;
						}

						sGroupName = GetAttributeValue("NAME", sText);
					}
				}
                else if (sText.Trim().StartsWith(csNumberInDirectory))
                {
                    sbTmp.Length = 0;

                    if (i_vxmlGenerationParameters.DtmfDialingRestricted)
                    {
                        sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
                        sbTmp.AppendFormat("\t\t\t\t\t\t\tvar oUserDirectory = new Incendonet.Plugins.UserDirectory.UserDirectory();{0}", Environment.NewLine);
                        sbTmp.AppendFormat("\t\t\t\t\t\t\t{0} = oUserDirectory.DoesItContainPhoneNumber({1});{2}", GetAttributeValue("RESULT_VARIABLE", sText), GetAttributeValue("NUMBER_VARIABLE", sText), Environment.NewLine);
                        sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
                    }
                    else
                    {
                        sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
                        sbTmp.AppendFormat("\t\t\t\t\t\t\t{0} = \"true\";{1}", GetAttributeValue("RESULT_VARIABLE", sText), Environment.NewLine);
                        sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
                    }

                    i_twFinal.Write(sbTmp.ToString());
                }
				else if (sText.EndsWith(csEmailServerTag))
				{
					// Server name will be looked up if it is passed in as an empty string.
					i_twFinal.WriteLine("\t<var name = \"sEmailServer\" expr = \"\"/>");
				}
				else if (sText.EndsWith(csEmailNamesTag))	// Fill in the 'options' section
				{
					++iCountNamesEmailList;

					foreach (Users.User user in i_users)
					{
						if (IsUserInGroup(user, sGroupName))
						{
							if (IsValidUserForMessaging(user))
							{
								// FIX - Only output the name if the user has the 'Feature' enabled.

								i_twFinal.WriteLine(GenerateOptionElement(user.GetFullName()));

								foreach (string sAlternatePronunciation in user.AltPronunciationColl)
								{
									i_twFinal.WriteLine(GenerateOptionElement(sAlternatePronunciation));
								}
							}
						}
					}
				}
				else if (sText.EndsWith(csEmailAcctInfoTag))	// Fill in the email account section
				{
					++iCountNamesEmailAccountInfo;

					foreach (Users.User user in i_users)
					{
						if (IsUserInGroup(user, sGroupName))
						{
							if (IsValidUserForMessaging(user))
							{
								// FIX - Only output the acct info if the user has the 'Feature' enabled.

								i_twFinal.WriteLine(GenerateAccountInfoElement(user.GetFullName(), user.GetFullName(), user.GetFullName(), user.Username, user.Domain));

								foreach (string sAlternatePronunciation in user.AltPronunciationColl)
								{
									i_twFinal.WriteLine(GenerateAccountInfoElement(sAlternatePronunciation, user.GetFullName(), sAlternatePronunciation, user.Username, user.Domain));
								}
							}
						}
					}
				}
				else
				{
					i_twFinal.WriteLine(sText);
				}
			}


			// Checks to make sure that MACROS that depend on each other are used together.

			if (iCountCustomsCommandsList != iCountCustomsCommandsDefinitions)
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - CUSTOMCOMMANDS_LIST (Occurrence: {0}) and CUSTOMCOMMANDS_DEFINITIONS (Occurrence: {1}) are expected to be used together.  This is most likely a problem.", iCountCustomsCommandsList, iCountCustomsCommandsDefinitions));
			}

			if (iCountNamesEmailList != iCountNamesEmailAccountInfo)
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - NAMES_EMAIL_LIST (Occurrence: {0}) and NAMES_EMAIL_ACCT_INFO (Occurrence: {1}) are expected to be used together.  This is most likely a problem.", iCountNamesEmailList, iCountNamesEmailAccountInfo));
			}

			if ((iCountMoreOptionsCommand != iCountMoreOptionsSpeech) || (iCountMoreOptionsCommand != iCountMoreOptionsGoto))
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - MOREOPTIONS_COMMAND (Occurrence: {0}), MOREOPTIONS_SPEECH (Occurrence: {1}) and MOREOPTIONS_GOTO (Occurrence: {2}) are expected to be used together.  This is most likely a problem.", iCountMoreOptionsCommand, iCountMoreOptionsSpeech, iCountMoreOptionsGoto));
			}

			if ((iCountNamesTransferTo > 0) && (iCountNamesTransferToWithConfirmation > 0))
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - NAMES_TRANSFERTO (Occurrence: {0}) and NAMES_TRANSFERTO_WITH_CONFIRMATION (Occurrence: {1}) are not expected to be used together.  This is most likely a problem.", iCountNamesTransferTo, iCountNamesTransferToWithConfirmation));
			}

			if ((iCountNamesList == 0) && (iCountNamesTransferTo > 0))
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - NAMES_LIST (Occurrence: {0}) and NAMES_TRANSFERTO (Occurrence: {1}) are expected to be used together.  This is most likely a problem.", iCountNamesList, iCountNamesTransferTo));
			}

			if ((iCountNamesList == 0) && (iCountNamesTransferToWithConfirmation > 0))
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - NAMES_LIST (Occurrence: {0}) and NAMES_TRANSFERTO_WITH_CONFIRMATION (Occurrence: {1}) are expected to be used together.  This is most likely a problem.", iCountNamesList, iCountNamesTransferToWithConfirmation));
			}

			if ((iCountNamesTransferToWithConfirmation != iCountConfirmationVariables) || (iCountNamesTransferToWithConfirmation != iCountConfirmationForm))
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - NAMES_TRANSFERTO_WITH_CONFIRMATION (Occurrence: {0}), CONFIRMATION_VARIABLES (Occurrence: {1}) and CONFIRMATION_FORM (Occurrence: {2}) are expected to be used together.  This is most likely a problem.", iCountNamesTransferToWithConfirmation, iCountConfirmationVariables, iCountConfirmationForm));
			}

			if (iCountGroupsTag > 1)
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - GROUPS (Occurrence: {0}) occurs more than once.  This will result in unexpected behavior.", iCountGroupsTag));
			}

			if (bGroupsTagInWrongPlace)
			{
				Log(Level.Warning, String.Format("VXML Template WARNING - GROUPS occurs in the wrong order relative to other tags that it effects.  This will result in unexpected behavior."));
			}
		} // GenerateVxmlFromTemplate

		private string ReplaceCollaborationCommand(string i_sCmd, bool i_bEnabled)
		{
			string sFormatString = i_bEnabled ? "\t\t\t{0}" : "<!--\t\t\t{0}\t-->";

			return String.Format(sFormatString, i_sCmd);
		}

		private string GenerateOptionElement(string i_sElementContent)
		{
			return (String.Format("\t\t\t<option>{0}</option>", StringFilter.RemoveApostrophes(i_sElementContent)));
		}

		private string GenerateTransferElement(string i_sMatchingName, string i_sWaveFileName, string i_sTTSName, string i_sDestinationNumber)
		{
            string sComparisonName = GetVxmlComparisonSafeName(i_sMatchingName);
            string sWaveFileName = GetFilenameSafeName(i_sWaveFileName);
			
			StringBuilder sbTmp = new StringBuilder();

			sbTmp.AppendFormat("\t\t\t\t<elseif cond=\"callee == '{0}'\"/>{1}", sComparisonName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:{0}\">{1}", i_sDestinationNumber, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<prompt>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/{0}.wav\">{1}.</audio>{2}", sWaveFileName, i_sTTSName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</transfer>");

			return (sbTmp.ToString());
		}

		private string GenerateTransferElementWithConfirmation(string i_sMatchingName, string i_sWaveFileName, string i_sTTSName, string i_sDestinationNumber, int i_iConfirmationCutoffPercentage)
		{
            string sComparisonName = GetVxmlComparisonSafeName(i_sMatchingName);
            string sWaveFileName = GetFilenameSafeName(i_sWaveFileName);

			StringBuilder sbTmp = new StringBuilder();

			sbTmp.AppendFormat("\t\t\t\t<elseif cond=\"callee == '{0}'\"/>{1}", sComparisonName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\toSAUtils = new ISMessaging.Utilities();{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\tiSAConf = oSAUtils.CopyString(callee$.confidence);{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\tsSAUtt = oSAUtils.CopyString(callee$.utterance);{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\tsSADest = oSAUtils.CopyString(\"{0}\");{1}", i_sDestinationNumber, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<if cond=\"iSAConf > '{0}'\">{1}", i_iConfirmationCutoffPercentage, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/{0}.wav\">{1}.</audio>{2}", sWaveFileName, i_sTTSName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = 0;{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\tsSANameWav = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/{0}.wav\";{1}", sWaveFileName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<goto next=\"#AAConfirm\"/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</if>");
			
			return(sbTmp.ToString());
		}

		private string GenerateAccountInfoElement(string i_sMatchingName, string i_sWaveFileName, string i_sTTSName, string i_sUsername, string i_sDomain)
		{
            string sComparisonName = GetVxmlComparisonSafeName(i_sMatchingName);
            string sWaveFileName = GetFilenameSafeName(i_sWaveFileName);

			StringBuilder sbTmp = new StringBuilder();

			sbTmp.AppendFormat("\t\t\t\t<elseif cond = \"sCallerName == '{0}'\"/>{1}", sComparisonName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\tdocument.sUsername = \"{0}\";{1}", i_sUsername, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\tdocument.sDomain = \"{0}\";{1}", i_sDomain, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Names/{0}.wav\">{1}.</audio>{2}", sWaveFileName, i_sTTSName, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<audio src = \"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>");

			return (sbTmp.ToString());
		}

		private string GenerateConfirmationForm(int i_iNumberOfRetries, string i_sOperatorExtension, string i_sTryAgainUrl)
		{
			StringBuilder sbTmp = new StringBuilder();

			sbTmp.AppendFormat("\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t<form id = \"AAConfirm\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t<block>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- Form intro prompt																		-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<prompt bargein=\"true\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<audio expr=\"sSANameWav\"><value expr=\"sSAUtt\"/></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/IsThatCorrect.wav\">Is that correct?</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t</block>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t<field name=\"confirmation\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- Field options																			-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<grammar type=\"application/srgs\" src=\"file:///opt/speechbridge/VoiceDocStore/ABNFBooleanSA.gram\" />{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- Field responses																		-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t<filled>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<if cond=\"confirmation == 'true'\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<elseif cond=\"confirmation == 'false'\"/>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t<if cond = \"iNumConfirmRetries == '{0}'\">{1}", i_iNumberOfRetries, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:{0}\">{1}", i_sOperatorExtension, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\tiNumConfirmRetries = iNumConfirmRetries + 1;{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t<goto next=\"{0}\"/>{1}", i_sTryAgainUrl, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<elseif cond=\"confirmation == 'main menu'\"/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/MainMenu.wav\">Main menu.</audio>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t<goto next=\"{0}\"/>{1}", i_sTryAgainUrl, Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t<elseif cond = \"confirmation$.utterance == '1'\"/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<transfer dest=\"sip:application$.sSADest\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OneMomentPlease.wav\">One moment please.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<elseif cond = \"confirmation$.utterance == '2'\"/>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t<if cond = \"iNumConfirmRetries == '{0}'\">{1}", i_iNumberOfRetries, Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:{0}\">{1}", i_sOperatorExtension, Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/OkLetsTryThatAgain.wav\">OK, let's try that again.</audio>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t\tiNumConfirmRetries = iNumConfirmRetries + 1;{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t<goto next=\"{0}\"/>{1}", i_sTryAgainUrl, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<!-- Unrecognized handler																	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~	-->{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t<else/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<if cond = \"iNumConfirmMisses == '{0}'\">{1}", i_iNumberOfRetries, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<transfer dest=\"sip:{0}\">{1}", i_sOperatorExtension, Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/TransferToOperator.wav\">I'm sorry, but I'm having difficulty understanding you.  Please wait while I transfer you to the operator.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-2sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</transfer>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t<else/>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"false\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/ImSorryWasThat.wav\">I'm sorry, was that</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio expr=\"sSANameWav\"><value expr=\"sSAUtt\"/></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t// This script element is used to allow a second prompt element to flip barge-in behavior.{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t<prompt bargein=\"true\">{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/Silence-05sec.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/PleaseSayYesOrNo.wav\">Please say, yes, or, no.</audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\t<audio src=\"file:///opt/speechbridge/VoiceDocStore/Prompts/beep.wav\"></audio>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</prompt>{0}", Environment.NewLine);
            sbTmp.AppendFormat("\t\t\t\t\t\t<script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t\tiNumConfirmMisses = iNumConfirmMisses + 1;{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t\t</script>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t\t</if>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t\t</if>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t\t</filled>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t\t</field>{0}", Environment.NewLine);
			sbTmp.AppendFormat("\t</form>");

			return sbTmp.ToString();
		} // GenerateConfirmationForm

		private VxmlGenerationParameters GetVxmlGenerationParameters()
		{
            VxmlGenerationDAL vxmlGenerationDAL = new VxmlGenerationDAL(m_Logger);

            return vxmlGenerationDAL.GetVxmlGenerationParameters();
		}

		private Users GetUsers()
		{
			Users users = GroupsDAL.GetUsersInGroup(Group.ALL);

			if (CapabilitiesManager.IsGroupsEnabled)
			{
				foreach (Users.User user in users)
				{
					user.Groups = GroupsDAL.GetGroupsForUser(user.UserID);
				}
			}

			return users;
		}

		private string GetAttributeValue(string i_sAttributeName, string i_sText)
		{
			string sAttributeValue = "";

			using (XmlTextReader reader = new XmlTextReader(i_sText, XmlNodeType.Element, null))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						sAttributeValue = reader.GetAttribute(i_sAttributeName);

						switch (sAttributeValue)
						{
							case null:
								sAttributeValue = "";
								Log(Level.Warning, String.Format("GetAttributeValue: Attribute '{0}' not found.  This might result in incorrect behavior of VXML file. ({1})", i_sAttributeName, i_sText));
								break;

							case "":
								Log(Level.Warning, String.Format("GetAttributeValue: Attribute '{0}' is empty.  This might result in incorrect behavior of VXML file. ({1})", i_sAttributeName, i_sText));
								break;

							default:
								break;
						}
					}
				}
			}

			return sAttributeValue;
		}

		private bool IsUserInGroup(Users.User i_user, string i_sGroupName)
		{
			bool bIsInGroup = false;

			if (i_sGroupName == Group.ALL)
			{
				bIsInGroup = true;
			}
			else
			{
				bIsInGroup = i_user.Groups.Contains(i_sGroupName);
			}

			return bIsInGroup;
		}

		private void Log(Level i_Level, string i_sString)
		{
			if (m_Logger != null)
			{
				m_Logger.Log(i_Level, i_sString);
			}
			else
			{
				Console.Error.WriteLine("{0} {1}: {2}", DateTime.Now, i_Level, i_sString);
			}
		}
	} // VoiceScriptGen
}
