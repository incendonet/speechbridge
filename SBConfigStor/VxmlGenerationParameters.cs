// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

namespace SBConfigStor
{
    public sealed class VxmlGenerationParameters
    {
        private string m_sOperatorExtension = "0";
        private bool m_bIntroBargein = true;
        private bool m_bCollaborationCommandsEnabled = false;
        private int m_iNumberOfRetries = 2;
        private bool m_bMoreOptionsEnabled = false;
        private int m_iConfirmationCutoffPercentage = 90;
        private int m_iNumberOfDigitsForExtension = 15;
        private bool m_bIsGroupsEnabled = false;
        private bool m_bDtmfDialingRestricted = true;

        public string OperatorExtension
        {
            get { return m_sOperatorExtension; }
            set { m_sOperatorExtension = value; }
        }

        public bool IntroBargein
        {
            get { return m_bIntroBargein; }
            set { m_bIntroBargein = value; }
        }

        public bool CollaborationCommandsEnabled
        {
            get { return m_bCollaborationCommandsEnabled; }
            set { m_bCollaborationCommandsEnabled = value; }
        }

        public int NumberOfRetries
        {
            get { return m_iNumberOfRetries; }
            set { m_iNumberOfRetries = value; }
        }

        public bool MoreOptionsEnabled
        {
            get { return m_bMoreOptionsEnabled; }
            set { m_bMoreOptionsEnabled = value; }
        }

        public int ConfirmationCutoffPercentage
        {
            get { return m_iConfirmationCutoffPercentage; }
            set { m_iConfirmationCutoffPercentage = value; }
        }

        public int NumberOfDigitsForExtension
        {
            get { return m_iNumberOfDigitsForExtension; }
            set { m_iNumberOfDigitsForExtension = value; }
        }

        public bool IsGroupsEnabled
        {
            get { return m_bIsGroupsEnabled; }
            set { m_bIsGroupsEnabled = value; }
        }

        public bool DtmfDialingRestricted
        {
            get { return m_bDtmfDialingRestricted; }
            set { m_bDtmfDialingRestricted = value; }
        }

        public override string ToString()
        {
            return String.Format("Operator Extension: '{0}', Intro Barge-in Enabled: {1}, Collaboration Commands Enabled: {2}, Number of Retries: {3}, More Options Enabled: {4}, Confirmation Cutoff Percentage: {5}, Number of Digits for Extension: {6}, Groups Enabled: {7}, DTMF Dialing Restricted: {8}.", OperatorExtension, IntroBargein, CollaborationCommandsEnabled, NumberOfRetries, MoreOptionsEnabled, ConfirmationCutoffPercentage, NumberOfDigitsForExtension, IsGroupsEnabled, DtmfDialingRestricted);
        }
    } // VxmlGenerationParameters
}
