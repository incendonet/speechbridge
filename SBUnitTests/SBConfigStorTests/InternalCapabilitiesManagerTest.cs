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
	public sealed class InternalCapabilitiesManagerTest
	{
		[Test]
		public void TestThatIsLicenseValidReturnsFalseForAnInvalidLicense()
		{
            License license = new License(License.Status.Invalid);

			InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
			capabilities.ProcessLicense(license.GetLicense());

			Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
		}

		[Test]
		public void TestThatIsGroupsEnabledReturnsFalseForAMissingLicense()
		{
            License license = new License(License.Status.Missing);

			InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
			capabilities.ProcessLicense(license.GetLicense());

			Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.IsGroupsEnabled, Is.False, "Groups should be disabled.");
		}

		[Test]
		public void TestThatIsGroupsEnabledReturnsFalseIfGroupsElementIsSetToDisabled()
		{
            License license = new License();
            license.SetLicenseElement("Groups", "Disabled");

			InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
			capabilities.ProcessLicense(license.GetLicense());

			Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsGroupsEnabled, Is.False, "Groups should be disabled.");
		}

		[Test]
		public void TestThatIsGroupsEnabledReturnsTrueIfGroupsElementIsSetToEnabled()
		{
            License license = new License();
			license.SetLicenseElement("Groups", "Enabled");

			InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
			capabilities.ProcessLicense(license.GetLicense());

			Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsGroupsEnabled, Is.True, "Groups should be enabled.");
		}

        [Test]
		public void TestThatIsActiveDirectoryImportEnabledReturnsFalseForAMissingLicense()
		{
            License license = new License(License.Status.Missing);

			InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
			capabilities.ProcessLicense(license.GetLicense());

			Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.IsActiveDirectoryImportEnabled, Is.False, "Active Directory Import should be disabled.");
		}

		[Test]
		public void TestThatIsActiveDirectoryImportEnabledReturnsFalseIfActiveDirectoryImportElementIsSetToDisabled()
		{
            License license = new License();
			license.SetLicenseElement("ActiveDirectoryImport", "disabled");

			InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
			capabilities.ProcessLicense(license.GetLicense());

			Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsActiveDirectoryImportEnabled, Is.False, "Active Directory Import should be disabled.");
		}

		[Test]
		public void TestThatIsActiveDirectoryImportEnabledReturnsTrueIfActiveDirectoryImportElementIsSetToEnabled()
		{
            License license = new License();
            license.SetLicenseElement("ActiveDirectoryImport", "enabled");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsActiveDirectoryImportEnabled, Is.True, "Active Directory Import should be enabled.");
		}

		[Test]
		public void TestThatIsCollaborationEnabledReturnsFalseForAMissingLicense()
		{
            License license = new License(License.Status.Missing);

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.IsCollaborationEnabled, Is.False, "Collaboration should be disabled.");
		}

		[Test]
		public void TestThatIsCollaborationEnabledReturnsFalseIfCollaborationElementIsSetToDisabled()
		{
            License license = new License();
            license.SetLicenseElement("Collaboration", " DisAbled  ");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsCollaborationEnabled, Is.False, "Collaboration should be disabled.");
		}

		[Test]
		public void TestThatIsCollaborationEnabledReturnsTrueIfCollaborationElementIsSetToEnabled()
		{
            License license = new License();
            license.SetLicenseElement("Collaboration", "  EnabLed  ");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsCollaborationEnabled, Is.True, "Collaboration should be enabled.");
		}

		[Test]
		public void TestThatDialogDesignerIsDisabledWhenLicenseIsMissing()
		{
            License license = new License(License.Status.Missing);

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.IsDialogDesignerEnabled, Is.False, "Dialog Designer should be disabled.");
			Assert.That(capabilities.IsDialogDesignerDtmfOnly, Is.False, "Dialog Designer DTMF mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerPressOrSay, Is.False, "Dialog Designer Press Or Say mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerSpeech, Is.False, "Dialog Designer Speech mode should be disabled.");
		}

		[Test]
		public void TestThatDialogDesignerIsDisabledWhenDialogDesignerElementIsSetToDisabled()
		{
            License license = new License();
            license.SetLicenseElement("DialogDesigner", "disabled");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsDialogDesignerEnabled, Is.False, "Dialog Designer should be disabled.");
			Assert.That(capabilities.IsDialogDesignerDtmfOnly, Is.False, "Dialog Designer DTMF mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerPressOrSay, Is.False, "Dialog Designer Press Or Say mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerSpeech, Is.False, "Dialog Designer Speech mode should be disabled.");
		}

		[Test]
		public void TestThatDialogDesignerIsInDtmfOnlyModeWhenDialogDesignerElementIsSetToDtmfOnly()
		{
            License license = new License();
            license.SetLicenseElement("DialogDesigner", "  DTMFOnly  ");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsDialogDesignerEnabled, Is.True, "Dialog Designer should be enabled.");
			Assert.That(capabilities.IsDialogDesignerDtmfOnly, Is.True, "Dialog Designer DTMF mode should be enabled.");
			Assert.That(capabilities.IsDialogDesignerPressOrSay, Is.False, "Dialog Designer Press Or Say mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerSpeech, Is.False, "Dialog Designer Speech mode should be disabled.");
		}

		[Test]
		public void TestThatDialogDesignerIsInPressOrSayModeWhenDialogDesignerElementIsSetToPressOrSay()
		{
            License license = new License();
            license.SetLicenseElement("DialogDesigner", "PressOrSay");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsDialogDesignerEnabled, Is.True, "Dialog Designer should be enabled.");
			Assert.That(capabilities.IsDialogDesignerDtmfOnly, Is.False, "Dialog Designer DTMF mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerPressOrSay, Is.True, "Dialog Designer Press Or Say mode should be enabled.");
			Assert.That(capabilities.IsDialogDesignerSpeech, Is.False, "Dialog Designer Speech mode should be disabled.");
		}

		[Test]
		public void TestThatDialogDesignerIsInSpeechModeWhenDialogDesignerElementIsSetToSpeech()
		{
            License license = new License();
            license.SetLicenseElement("DialogDesigner", "speech");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.IsDialogDesignerEnabled, Is.True, "Dialog Designer should be enabled.");
			Assert.That(capabilities.IsDialogDesignerDtmfOnly, Is.False, "Dialog Designer DTMF mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerPressOrSay, Is.False, "Dialog Designer Press Or Say mode should be disabled.");
			Assert.That(capabilities.IsDialogDesignerSpeech, Is.True, "Dialog Designer Speech mode should be enabled.");
		}

		[Test]
		public void TestThatMaximumNumberOfDIDsReturns0ForAMissingLicense()
		{
            License license = new License(License.Status.Missing);

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.MaximumNumberOfDIDs, Is.EqualTo(0), "Allowed DIDs should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfDIDsReturnsCorrectValueIfMaximumNumberOfDIDsElementIsSetToAValue()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfDids", "5");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfDIDs, Is.EqualTo(5), "Allowed DIDs should be 5.");
		}

		[Test]
		public void TestThatMaximumNumberOfDIDsReturns0IfMaximumNumberOfDIDsElementIsSetToANonNumericValue()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfDids", "5 apples");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfDIDs, Is.EqualTo(0), "Allowed DIDs should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfDIDsReturnsMaxValueIfMaximumNumberOfDIDsElementIsSetToUnlimited()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfDids", "  Unlimited  ");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfDIDs, Is.EqualTo(Int32.MaxValue), "Allowed DIDs should be Int32.MaxValue.");
		}

		[Test]
		public void TestThatMaximumNumberOfDirectoryEntriesReturns0ForAMissingLicense()
		{
            License license = new License(License.Status.Missing);

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.MaximumNumberOfDirectoryEntries, Is.EqualTo(0), "Allowed number of directory entries should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfDirectoryEntriesReturnsCorrectValueIfMaximumNumberOfDirectoryEntriesElementIsSetToAValue()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfDirectoryEntries", "25");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfDirectoryEntries, Is.EqualTo(25), "Allowed number of directory entries should be 25.");
		}

		[Test]
		public void TestThatMaximumNumberOfDirectoryEntriesReturns0IfMaximumNumberOfDirectoryEntriesElementIsSetToANonNumericValue()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfDirectoryEntries", "25!");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfDirectoryEntries, Is.EqualTo(0), "Allowed number of directory entries should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfDirectoryEntriesReturns0IfMaximumNumberOfDirectoryEntriesElementIsSetToUnlimited()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfDirectoryEntries", "  Unlimited  ");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfDirectoryEntries, Is.EqualTo(0), "Allowed number of directory entries should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfPortsReturns0ForAMissingLicense()
		{
            License license = new License(License.Status.Missing);

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
			Assert.That(capabilities.MaximumNumberOfPorts, Is.EqualTo(0), "Allowed number of ports should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfPortsReturnsCorrectValueIfMaximumNumberOfPortsElementIsSetToAValue()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfPorts", "6");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfPorts, Is.EqualTo(6), "Allowed number of ports should be 6.");
		}

		[Test]
		public void TestThatMaximumNumberOfPortsReturns0IfMaximumNumberOfPortsElementIsSetToANonNumericValue()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfPorts", ".2");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfPorts, Is.EqualTo(0), "Allowed number of ports should be 0.");
		}

		[Test]
		public void TestThatMaximumNumberOfPortsReturns0IfMaximumNumberOfPortsElementIsSetToUnlimited()
		{
            License license = new License();
            license.SetLicenseElement("MaximumNumberOfPorts", "Unlimited");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
			Assert.That(capabilities.MaximumNumberOfPorts, Is.EqualTo(0), "Allowed number of ports should be 0.");
		}

        [Test]
        public void TestThatProductReturnsUnknownForAMissingLicense()
        {
            License license = new License(License.Status.Missing);

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.False, "License is expected to be invalid.");
            Assert.That(capabilities.Product, Is.EqualTo("Unknown"), "Should return \"Unknown\".");
        }

        [Test]
        public void TestThatProductReturnsSmallOffice()
        {
            License license = new License();
            license.SetLicenseElement("Product", "SmallOffice");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
            Assert.That(capabilities.Product, Is.EqualTo("Small Office"), "Should return \"Small Office\".");
        }

        [Test]
        public void TestThatProductReturnsSMB()
        {
            License license = new License();
            license.SetLicenseElement("Product", "Smb");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
            Assert.That(capabilities.Product, Is.EqualTo("SMB"), "Should return \"SMB\".");
        }

        [Test]
        public void TestThatProductReturnsSMBPlus()
        {
            License license = new License();
            license.SetLicenseElement("Product", "SmB+");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
            Assert.That(capabilities.Product, Is.EqualTo("SMB+"), "Should return \"SMB+\".");
        }

        [Test]
        public void TestThatProductReturnsPro()
        {
            License license = new License();
            license.SetLicenseElement("Product", "prO");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
            Assert.That(capabilities.Product, Is.EqualTo("Pro"), "Should return \"Pro\".");
        }

        [Test]
        public void TestThatProductReturnsDemo()
        {
            License license = new License();
            license.SetLicenseElement("Product", "demo");

            InternalCapabilitiesManager capabilities = new InternalCapabilitiesManager(new TestLicenseManager());
            capabilities.ProcessLicense(license.GetLicense());

            Assert.That(capabilities.IsLicenseValid, Is.True, "License is expected to be valid.");
            Assert.That(capabilities.Product, Is.EqualTo("Demo"), "Should return \"Demo\".");
        }


        // Mock license manager to support testing of InternalCapabiliesMangager.

		private sealed class TestLicenseManager : ILicenseManager
		{
			private Dictionary<string, string> m_Elements = new Dictionary<string, string>();

			public string GetLicense(string i_sLicense, string i_sProduct)
			{
				return i_sLicense;
			}
		}

        private sealed class License
        {
            private Dictionary<string, string> m_Elements = new Dictionary<string, string>();
            private Status m_licenseStatus = Status.Valid;

            public enum Status
            {
                Valid,
                Invalid,
                Missing
            }

            public License() : this(Status.Valid)
            {
            }

            public License(Status i_licenseStatus)
            {
                m_licenseStatus = i_licenseStatus;

                if (m_licenseStatus == Status.Valid)
                {
                    m_Elements.Add("Product", "");
                    m_Elements.Add("ActiveDirectoryImport", "");
                    m_Elements.Add("Collaboration", "");
                    m_Elements.Add("DialogDesigner", "");
                    m_Elements.Add("Groups", "");
                    m_Elements.Add("MaximumNumberOfDids", "");
                    m_Elements.Add("MaximumNumberOfDirectoryEntries", "");
                    m_Elements.Add("MaximumNumberOfPorts", "");
                }
            }

            public string GetLicense()
            {
                StringBuilder output = new StringBuilder();

                if (m_licenseStatus != Status.Missing)
                {
                    output.AppendLine("<SpeechBridge version=\"1.0\">");

                    foreach (string elementName in m_Elements.Keys)
                    {
                        output.AppendLine(String.Format("  <{0}>{1}</{0}>", elementName, m_Elements[elementName]));
                    }

                    output.AppendLine("</SpeechBridge>");
                }

                return output.ToString();
            }

            public void SetLicenseElement(string i_sCapabilityName, string i_sCapabilityValue)
            {
                m_Elements[i_sCapabilityName] = i_sCapabilityValue;
            }
        }
    }
}
