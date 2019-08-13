// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using Incendonet.Utilities.LogClient;
using SBConfigStor;


namespace Incendonet.Plugins.UserDirectory
{
    public sealed class UserDirectory
    {
        ILegacyLogger m_Logger = null;
        int m_iNumberOfDigitsInExtension;
        UserDirectoryDAL m_userDirectoryDAL;

        public UserDirectory()
        {
            m_iNumberOfDigitsInExtension = GetNumberOfDigitsInExtension();
            m_userDirectoryDAL = new UserDirectoryDAL(m_Logger);

        }

        public string DoesItContainPhoneNumber(string i_sPhoneNumber)
        {
            bool bExistsInDirectory = false;

            try
            {
                string sPhoneNumber = StringFilter.GetFilteredStringDial(i_sPhoneNumber);                   // Just to make sure we are only dealing with digits.


                // If we have more digits than are allowed for a valid extension then we know that 
                // what we have is not a valid extension, so don't even bother to check against what
                // is in the DB.

                if (sPhoneNumber.Length <= m_iNumberOfDigitsInExtension)
                {
                    bExistsInDirectory = IsAKnownExtension(sPhoneNumber, m_iNumberOfDigitsInExtension);
                }


                // Only check Alternate Numbers if we didn't match any of the extensions in the DB.

                if (!bExistsInDirectory)
                {
                    bExistsInDirectory = IsAKnownAlternateNumber(sPhoneNumber);
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("UserDirectory.DoesItContainPhoneNumber({0}) caught exception: {1}", i_sPhoneNumber, exc.ToString()));
            }

            return (bExistsInDirectory ? "true" : "false");
        }

        private int GetNumberOfDigitsInExtension()
        {
            VxmlGenerationDAL vxmlGenerationDAL = new VxmlGenerationDAL(m_Logger);
            VxmlGenerationParameters vxmlGenerationParameters = vxmlGenerationDAL.GetVxmlGenerationParameters();

            return vxmlGenerationParameters.NumberOfDigitsForExtension;
        }

        private bool IsAKnownExtension(string i_sPhoneNumber, int i_iNumberOfDigitsInExtension)
        {
            return m_userDirectoryDAL.IsAKnownExtension(i_sPhoneNumber, i_iNumberOfDigitsInExtension);
        }

        private bool IsAKnownAlternateNumber(string i_sPhoneNumber)
        {
            return m_userDirectoryDAL.IsAKnownAlternateNumber(i_sPhoneNumber);
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
