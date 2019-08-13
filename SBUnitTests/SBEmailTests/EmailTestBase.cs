// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using Incendonet.Utilities.LogClient;

namespace SBEmailTests
{
	public abstract class EmailTestBase
	{
		protected ILegacyLogger m_Logger = null;
		protected bool          m_bDebugAll = false;

		protected string        m_sExchangeServerAddress = "192.168.1.200";
		protected string        m_sFolder = "Inbox";
		protected string        m_sDomain = "testdc08";
		protected string        m_sUsername = "sharris";
		protected string        m_sPassword = "sharris";
		protected string        m_sUserDisplayName = "Steve Harris";

		protected int m_iEmailLookbackPeriod = 14;
		protected int m_iCalendarRetrievalPeriod = 28;

		protected string EmailServerInfo()
		{
			return string.Format("Exchange Server Address: {0}, User name: {1}@{2}, Password: {3}", m_sExchangeServerAddress, m_sUsername, m_sDomain, m_sPassword);
		}
	}
}
