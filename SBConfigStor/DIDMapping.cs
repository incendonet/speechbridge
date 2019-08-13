// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
namespace SBConfigStor
{
	public sealed class DIDMapping
	{
		string m_sDID;
		string m_sVoiceXml;

		public string DID
		{
			get { return m_sDID; }
			private set { m_sDID = value; }
		}

		public string VoiceXml
		{
			get { return m_sVoiceXml; }
			private set { m_sVoiceXml = value; }
		}

		public DIDMapping(string i_sDID, string i_sVoiceXml)
		{
			DID = i_sDID;
			VoiceXml = i_sVoiceXml;
		}
	}
}
