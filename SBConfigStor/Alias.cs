// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
namespace SBConfigStor
{
	public sealed class Alias
	{
		int m_iKey;
		string m_sValue;

		public int Key
		{
			get { return m_iKey; }
			private set { m_iKey = value; }
		}

		public string Value
		{
			get { return m_sValue; }
			private set { m_sValue = value; }
		}

		public Alias(int i_iKey, string i_sValue)
		{
			Key = i_iKey;
			Value = i_sValue;
		}
	}
}
