// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
namespace SBConfigStor
{
	public sealed class Group
	{
		public static readonly string ALL = "All";
		public static readonly string HIDDEN = "Hidden";

		private int m_iID;
		private string m_sName;

		public Group()
		{
		}

		public Group(int i_iID, string i_sName)
		{
			ID = i_iID;
			Name = i_sName;
		}

		public string Name
		{
			get { return m_sName; }
			set { m_sName = value; }
		}

		public int ID
		{
			get { return m_iID; }
			set { m_iID = value; }
		}
	}
}
