// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using Incendonet.Utilities.LogClient;


namespace SBConfigStor
{
    public sealed class VxmlGenerationDAL
    {
        private readonly ILegacyLogger m_Logger = null;

        public VxmlGenerationDAL(ILegacyLogger i_Logger)
        {
            m_Logger = i_Logger;
        }

        public VxmlGenerationParameters GetVxmlGenerationParameters()
        {
			VxmlGenerationParameters vxmlGenerationParameters = new VxmlGenerationParameters();

			ConfigParams cfgs = new ConfigParams();
			bool bRes = cfgs.LoadFromTableByComponent(ConfigParams.e_Components.Apps.ToString() + ConfigParams.c_Separator + ConfigParams.e_Components.SpeechAttendant.ToString());
			if (!bRes)
			{
                Log(Level.Exception, String.Format("VxmlGenerationDAL.GetVxmlGenerationParameters() - There was an error retrieving VXML generation parameters from the database - using default values ({0}).", vxmlGenerationParameters.ToString()));
			}
			else
			{
				foreach (ConfigParams.ConfigParam parameter in cfgs)
				{
					if (parameter.Name == ConfigParams.e_SpeechAppSettings.OperatorExtension.ToString())
					{
						if (parameter.Value.Length == 0)
						{
                            Log(Level.Exception, String.Format("VxmlGenerationDAL.GetVxmlGenerationParameters - Operator Extension not found in database - using default ({0}).", vxmlGenerationParameters.OperatorExtension));
						}
						else
						{
							vxmlGenerationParameters.OperatorExtension = parameter.Value;
						}
					}
					else if (parameter.Name == ConfigParams.e_SpeechAppSettings.IntroBargein.ToString())
					{
						vxmlGenerationParameters.IntroBargein = (parameter.Value == "1" ? true : false);
					}
					else if (parameter.Name == ConfigParams.e_SpeechAppSettings.CollabCommands.ToString())						// Collaboration commands ('read email', etc.)
					{
						if (CapabilitiesManager.IsCollaborationEnabled)
						{
							vxmlGenerationParameters.CollaborationCommandsEnabled = (parameter.Value == "1" ? true : false);
						}
						else
						{
							vxmlGenerationParameters.CollaborationCommandsEnabled = false;
						}
					}
					else if (parameter.Name == ConfigParams.e_SpeechAppSettings.MaxRetriesBeforeOperator.ToString())			// Max retries due to misrecognition before transferring to the operator
					{
						int iNumberOfRetries;

						if (Int32.TryParse(parameter.Value, out iNumberOfRetries))
						{
							vxmlGenerationParameters.NumberOfRetries = iNumberOfRetries - 1;
						}
						else
						{
                            Log(Level.Exception, String.Format("VxmlGenerationDAL.GetVxmlGenerationParameters - Unable to parse MaxRetriesBeforeOperator from DB ('{0}') - using default ({1}).", parameter.Value, vxmlGenerationParameters.NumberOfRetries));
						}
					}
					else if (parameter.Name == ConfigParams.e_SpeechAppSettings.MoreOptions.ToString())
					{
						vxmlGenerationParameters.MoreOptionsEnabled = (parameter.Value == "1" ? true : false);
					}
					else if (parameter.Name == ConfigParams.e_SpeechAppSettings.ConfirmationCutoff.ToString())
					{
						int iConfirmationCutoffPercentage;

						if (Int32.TryParse(parameter.Value, out iConfirmationCutoffPercentage))
						{
							vxmlGenerationParameters.ConfirmationCutoffPercentage = iConfirmationCutoffPercentage;
						}
						else
						{
                            Log(Level.Exception, String.Format("VxmlGenerationDAL.GetVxmlGenerationParameters - Unable to parse ConfirmationCutoffPercentage from DB ('{0}') - using default ({1}).", parameter.Value, vxmlGenerationParameters.ConfirmationCutoffPercentage));
						}
					}
					else if (parameter.Name == ConfigParams.e_SpeechAppSettings.DIDTruncationLength.ToString())
					{
						int iNumberOfDigitsForExtension;

						if (Int32.TryParse(parameter.Value, out iNumberOfDigitsForExtension))
						{
							vxmlGenerationParameters.NumberOfDigitsForExtension = iNumberOfDigitsForExtension;
						}
						else
						{
                            Log(Level.Exception, String.Format("VxmlGenerationDAL.GetVxmlGenerationParameters - Unable to parse DIDTruncationLength from DB ('{0}') - using default ({1}).", parameter.Value, vxmlGenerationParameters.NumberOfDigitsForExtension));
						}
					}
                    else if (parameter.Name == ConfigParams.e_SpeechAppSettings.DtmfDialingRestricted.ToString())
                    {
                        vxmlGenerationParameters.DtmfDialingRestricted = (parameter.Value == "1" ? true : false);
                    }
				}
			}

			vxmlGenerationParameters.IsGroupsEnabled = CapabilitiesManager.IsGroupsEnabled;

			return vxmlGenerationParameters;
        }

        private void Log(Level i_Level, string i_sMessage)
        {
            if (m_Logger != null)
            {
                m_Logger.Log(i_Level, i_sMessage);
            }
            else
            {
                Console.Error.WriteLine(String.Format("{0} {1}: {2}", DateTime.Now, i_Level, i_sMessage));
            }
        }
    }
}
