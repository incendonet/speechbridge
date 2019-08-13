// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System.Collections.Generic;

namespace SBConfigStor
{
	public sealed class DtmfKeyToSpokenEquivalentMapping
	{
		private string m_sDtmfKey;
		private string m_sSpokenEquivalent;

		public DtmfKeyToSpokenEquivalentMapping(string i_sDtmfKey, string i_sSpokenEquivalent)
		{
			m_sDtmfKey = i_sDtmfKey;
			m_sSpokenEquivalent = i_sSpokenEquivalent;
		}

		public string Key
		{
			get { return m_sDtmfKey; }
		}

		public string SpokenEquivalent
		{
			get { return m_sSpokenEquivalent; }
		}
	}

	public sealed class DtmfKeyToSpokenEquivalentMappings : List<DtmfKeyToSpokenEquivalentMapping>
	{
	}
}
