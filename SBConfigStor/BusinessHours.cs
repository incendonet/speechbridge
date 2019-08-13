// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
namespace SBConfigStor
{
	public sealed class BusinessHours
	{
		public const string NINE_AM = "0900";
		public const string FIVE_PM = "1700";
		public const string CLOSED = "0";

		string m_sWeekday;
		string m_sStartTime;
		string m_sEndTime;

		public string Weekday
		{
			get { return m_sWeekday; }
			private set { m_sWeekday = value; }
		}

		public string StartTime
		{
			get { return m_sStartTime; }
			private set { m_sStartTime = value; }
		}

		public string EndTime
		{
			get { return m_sEndTime; }
			private set { m_sEndTime = value; }
		}

		public BusinessHours(string i_sWeekday, string i_sStartTime, string i_sEndTime)
		{
			Weekday = i_sWeekday;
			StartTime = i_sStartTime;
			EndTime = i_sEndTime;
		}
	}
}
