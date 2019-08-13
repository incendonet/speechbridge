// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Runtime.CompilerServices;
using System.Xml;

[assembly: InternalsVisibleTo("SBUnitTests")]

namespace SBConfigStor
{
	internal sealed class InternalCapabilitiesManager
	{
		private readonly ILicenseManager m_LicenseManager;
		private XmlDocument m_License;

		private bool m_bIsLicenseValid = true;

		private bool m_bIsActiveDirectoryImportEnabled = false;
		private bool m_bIsCollaborationEnabled = false;
		private bool m_bIsDialogDesignerEnabled = false;
		private bool m_bIsDialogDesignerDtmfOnly = false;
		private bool m_bIsDialogDesignerPressOrSay = false;
		private bool m_bIsDialogDesignerSpeech = false;
		private bool m_bIsGroupsEnabled = false;
		private int m_iMaximumNumberOfDIDs = 0;
		private int m_iMaximumNumberOfDirectoryEntries = 0;
		private int m_iMaximumNumberOfPorts = 0;
        private string m_sProduct = "Unknown";
		

		public InternalCapabilitiesManager(ILicenseManager i_LicenseManager)
		{
			m_LicenseManager = i_LicenseManager;
		}

		public void ProcessLicense(string i_sLicenseFile)
		{
			string sLicense = m_LicenseManager.GetLicense(i_sLicenseFile, "SpeechBridge");

            if (String.IsNullOrEmpty(sLicense))
            {
                IsLicenseValid = false;
            }
            else
            {
                m_License = new XmlDocument();

                m_License.LoadXml(sLicense);

                IsActiveDirectoryImportEnabled = GetBooleanValue("ActiveDirectoryImport");
                IsCollaborationEnabled = GetBooleanValue("Collaboration");
                ProcessDialogDesignerSetting(GetStringValue("DialogDesigner"));
                IsGroupsEnabled = GetBooleanValue("Groups");
                MaximumNumberOfDIDs = GetUnlimitedIntegerValue("MaximumNumberOfDids");
                MaximumNumberOfDirectoryEntries = GetIntegerValue("MaximumNumberOfDirectoryEntries");
                MaximumNumberOfPorts = GetIntegerValue("MaximumNumberOfPorts");
                ProcessProduct(GetStringValue("Product"));
            }
		}

		public bool IsLicenseValid
		{
			get { return m_bIsLicenseValid; }
			private set { m_bIsLicenseValid = value; }
		}

		public bool IsActiveDirectoryImportEnabled
		{
			get { return m_bIsActiveDirectoryImportEnabled; }
			private set { m_bIsActiveDirectoryImportEnabled = value; }
		}

		public bool IsCollaborationEnabled
		{
			get { return m_bIsCollaborationEnabled; }
			private set { m_bIsCollaborationEnabled = value; }
		}

		public bool IsDialogDesignerEnabled
		{
			get { return m_bIsDialogDesignerEnabled; }
			private set { m_bIsDialogDesignerEnabled = value; }
		}

		public bool IsDialogDesignerDtmfOnly
		{
			get { return m_bIsDialogDesignerDtmfOnly; }
			private set { m_bIsDialogDesignerDtmfOnly = value; }
		}

		public bool IsDialogDesignerPressOrSay
		{
			get { return m_bIsDialogDesignerPressOrSay; }
			private set { m_bIsDialogDesignerPressOrSay = value; }
		}

		public bool IsDialogDesignerSpeech
		{
			get { return m_bIsDialogDesignerSpeech; }
			private set { m_bIsDialogDesignerSpeech = value; }
		}

		public bool IsGroupsEnabled
		{
			get { return m_bIsGroupsEnabled; }
			private set { m_bIsGroupsEnabled = value; }
		}

		public int MaximumNumberOfDIDs
		{
			get { return m_iMaximumNumberOfDIDs; }
			private set { m_iMaximumNumberOfDIDs = value; }
		}

		public int MaximumNumberOfDirectoryEntries
		{
			get { return m_iMaximumNumberOfDirectoryEntries; }
			private set { m_iMaximumNumberOfDirectoryEntries = value; }
		}

		public int MaximumNumberOfPorts
		{
			get { return m_iMaximumNumberOfPorts; }
			private set { m_iMaximumNumberOfPorts = value; }
		}

        public string Product
        {
            get { return m_sProduct; }
            private set { m_sProduct = value; }
        }

        private void ProcessProduct(string i_sProduct)
        {
            switch (i_sProduct.Trim().ToLower())
            {
                case "small office":
                case "smalloffice":
                    Product = "Small Office";
                    break;

                case "smb":
                    Product = "SMB";
                    break;

                case "smb+":
                    Product = "SMB+";
                    break;

                case "pro":
                    Product = "Pro";
                    break;

                case "demo":
                    Product = "Demo";
                    break;

                default:
                    Product = "Unknown";
                    break;
            }
        }

		private void ProcessDialogDesignerSetting(string i_sDialogDesignerSetting)
		{
			switch (i_sDialogDesignerSetting.Trim().ToLower())
			{
				case "disabled":
					IsDialogDesignerEnabled = false;
					IsDialogDesignerDtmfOnly = false;
					IsDialogDesignerPressOrSay = false;
					IsDialogDesignerSpeech = false;
					break;

				case "dtmfonly":
					IsDialogDesignerEnabled = true;
					IsDialogDesignerDtmfOnly = true;
					IsDialogDesignerPressOrSay = false;
					IsDialogDesignerSpeech = false;
					break;

				case "pressorsay":
					IsDialogDesignerEnabled = true;
					IsDialogDesignerDtmfOnly = false;
					IsDialogDesignerPressOrSay = true;
					IsDialogDesignerSpeech = false;
					break;

				case "speech":
					IsDialogDesignerEnabled = true;
					IsDialogDesignerDtmfOnly = false;
					IsDialogDesignerPressOrSay = false;
					IsDialogDesignerSpeech = true;
					break;

				default:
					IsDialogDesignerEnabled = false;
					IsDialogDesignerDtmfOnly = false;
					IsDialogDesignerPressOrSay = false;
					IsDialogDesignerSpeech = false;
					break;
			}
		}

		private bool GetBooleanValue(string i_sElementName)
		{
			bool bElementValue = false;

			XmlNode node = GetElementNode(i_sElementName);

			if (null == node)
			{
				IsLicenseValid = false;
			}
			else
			{
				switch (node.InnerText.Trim().ToLower())
				{
					case "enabled":
						bElementValue = true;
						break;

					case "disabled":
						bElementValue = false;
						break;

					default:
						bElementValue = false;
						break;
				}
			}

			return bElementValue;
		}

		private int GetIntegerValue(string i_sElementName)
		{
			int iValue = 0;

			XmlNode node = GetElementNode(i_sElementName);

			if (null == node)
			{
				IsLicenseValid = false;
			}
			else
			{
				Int32.TryParse(node.InnerText, out iValue);
			}

			return iValue;
		}

		private int GetUnlimitedIntegerValue(string i_sElementName)
		{
			int iValue = 0;

			XmlNode node = GetElementNode(i_sElementName);

			if (null == node)
			{
				IsLicenseValid = false;
			}
			else
			{
				string sValue = node.InnerText;

				if (sValue.Trim().ToLower() == "unlimited")
				{
					iValue = Int32.MaxValue;
				}
				else
				{
					Int32.TryParse(sValue, out iValue);
				}
			}

			return iValue;
		}

		private string GetStringValue(string i_sElementName)
		{
			string sValue = "";

			XmlNode node = GetElementNode(i_sElementName);

			if (null == node)
			{
				IsLicenseValid = false;
			}
			else
			{
				sValue = node.InnerText;
			}

			return sValue;
		}

		private XmlNode GetElementNode(string i_sElementName)
		{
			return m_License.SelectSingleNode(String.Format("/SpeechBridge/{0}", i_sElementName));
		}
	}
}
