// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;

namespace SBConfigStor
{
	public static class CapabilitiesManager
	{
		private static InternalCapabilitiesManager m_Capabilies;

		static CapabilitiesManager()
		{
            ILicenseManager licenseManager = new EncryptedLicenseManager();
            string sLicenseFile = ConfigurationManager.AppSettings["License"];

            if (String.IsNullOrEmpty(sLicenseFile))
            {
                Log("CapabilitiesManager() - missing 'License' configuration file entry.");
            }
            else if (!(new Uri(sLicenseFile).IsFile))
            {
                licenseManager = new WebServiceLicenseManager();
            }

            m_Capabilies = new InternalCapabilitiesManager(licenseManager);
			m_Capabilies.ProcessLicense(sLicenseFile);
		}

		public static bool IsLicenseValid
		{
			get { return m_Capabilies.IsLicenseValid; }
		}

		public static bool IsActiveDirectoryImportEnabled
		{
			get { return m_Capabilies.IsActiveDirectoryImportEnabled; }
		}

		public static bool IsCollaborationEnabled
		{
			get { return m_Capabilies.IsCollaborationEnabled; }
		}

		public static bool IsDialogDesignerEnabled
		{
			get { return m_Capabilies.IsDialogDesignerEnabled; }
		}

		public static bool IsDialogDesignerDtmfOnly
		{
			get { return m_Capabilies.IsDialogDesignerDtmfOnly; }
		}

		public static bool IsDialogDesignerPressOrSay
		{
			get { return m_Capabilies.IsDialogDesignerPressOrSay; }
		}

		public static bool IsDialogDesignerSpeech
		{
			get { return m_Capabilies.IsDialogDesignerSpeech; }
		}

		public static bool IsGroupsEnabled
		{
			get { return m_Capabilies.IsGroupsEnabled; }
		}

		public static int MaximumNumberOfDIDs
		{
			get { return m_Capabilies.MaximumNumberOfDIDs; }
		}

		public static int MaximumNumberOfDirectoryEntries
		{
			get { return m_Capabilies.MaximumNumberOfDirectoryEntries; }
		}

		public static int MaximumNumberOfPorts
		{
			get { return m_Capabilies.MaximumNumberOfPorts; }
		}

        public static string Product
        {
            get { return m_Capabilies.Product;  }
        }

        private static void Log(string i_sMessage)
        {
            Console.Error.WriteLine("{0} {1}", DateTime.Now, i_sMessage);
        }
    }
}
