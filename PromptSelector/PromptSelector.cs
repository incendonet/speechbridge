// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;

using SBConfigStor;

namespace PromptSelector
{
	public sealed class PromptSelector
	{
		private string m_sDID = "";
		private string m_sPromptFile = "";
		private string m_sPromptText = "";
		private string m_sAfterHoursPromptFile = "";
		private string m_sAfterHoursPromptText = "";
		private string m_sHolidaysPromptFile = "";
		private string m_sHolidaysPromptText = "";
		private bool m_bAfterHoursEnabled = false;
		private bool m_bHolidaysEnabled = false;
		private List<BusinessHours> m_businessHoursCollection = null;
		private List<DateTime> m_holidaysCollection = null;

		private bool m_bNeedData = true;


		public PromptSelector(string i_sDID)
		{
			SetNewDID(i_sDID);
		}

		public string GetPromptFile()
		{
			GetData();

			return SelectPrompt(m_sPromptFile, m_sAfterHoursPromptFile, m_sHolidaysPromptFile);
		}

		public string GetPromptText()
		{
			GetData();

			return SelectPrompt(m_sPromptText, m_sAfterHoursPromptText, m_sHolidaysPromptText);
		}

		public bool IsHoliday()
		{
			bool bIsHoliday = false;

			GetData();

			if (m_bHolidaysEnabled)
			{
				foreach (DateTime date in m_holidaysCollection)
				{
					if (date.Date == DateTime.Now.Date)
					{
						bIsHoliday = true;
						break;
					}
				}
			}
			else
			{
				bIsHoliday = false;
			}

			return bIsHoliday;
		}

		public bool IsAfterHours()
		{
			bool bIsBusinessHours = false;

			GetData();

			if (m_bAfterHoursEnabled)
			{
				DateTime now = DateTime.Now;
				string sDayOfWeek = now.DayOfWeek.ToString();

				foreach (BusinessHours businessHours in m_businessHoursCollection)
				{
					if (sDayOfWeek == businessHours.Weekday)
					{
						if ((BusinessHours.CLOSED == businessHours.StartTime) || (BusinessHours.CLOSED == businessHours.EndTime))
						{
							bIsBusinessHours = false;
							break;
						}
						else
						{
							TimeSpan startTime = ConvertStringToTimeOfDay(businessHours.StartTime);
							TimeSpan endTime = ConvertStringToTimeOfDay(businessHours.EndTime);
							TimeSpan currentTime = now.TimeOfDay;

							if ((currentTime >= startTime) && (currentTime <= endTime))
							{
								bIsBusinessHours = true;
								break;
							}
						}
					}
				}
			}
			else
			{
				bIsBusinessHours = true;
			}

			return !bIsBusinessHours;
		}

		private void SetNewDID(string i_sDID)
		{
			m_sDID = GetDIDFromLocalURI(i_sDID);
		}

		private void GetData()
		{
			if (m_bNeedData)
			{
				// If no mapping is defined for the DID we received then use the DEFAULT entries.

				if (!DIDMappingDAL.IsMappingDefined(m_sDID))
				{
					SetNewDID(DIDMap.DEFAULT_DID);
				}

				DIDPromptDAL promptDAL = new DIDPromptDAL(m_sDID);

				promptDAL.GetPrompt(out m_sPromptFile, out m_sPromptText);
				promptDAL.GetPromptSetting(out m_bAfterHoursEnabled, out m_bHolidaysEnabled);

				if (m_bAfterHoursEnabled)
				{
					promptDAL.GetAfterHoursPrompt(out m_sAfterHoursPromptFile, out m_sAfterHoursPromptText);
					promptDAL.GetBusinessHours(out m_businessHoursCollection);
				}

				if (m_bHolidaysEnabled)
				{
					promptDAL.GetHolidaysPrompt(out m_sHolidaysPromptFile, out m_sHolidaysPromptText);
					promptDAL.GetHolidays(out m_holidaysCollection);
				}

				m_bNeedData = false;
			}
		}

		private string SelectPrompt(string i_sDefaultPrompt, string i_sAfterHoursPrompt, string i_sHolidayPrompt)
		{
			string sPrompt = "";

			if (IsHoliday())
			{
				sPrompt = i_sHolidayPrompt;
			}
			else if (IsAfterHours())
			{
				sPrompt = i_sAfterHoursPrompt;
			}
			else
			{
				sPrompt = i_sDefaultPrompt;
			}

			return sPrompt;
		}


		// This assumes that the passed in string contains the time in HHMM format.
		// Currently no provision is made to handle the case of a missing leading 0.

		private TimeSpan ConvertStringToTimeOfDay(string i_sTime)
		{
			TimeSpan timeOfDay;
			string sTime = String.Format("{0}:{1}", i_sTime.Substring(0, 2), i_sTime.Substring(2));
			
			TimeSpan.TryParse(sTime, out timeOfDay);

			return timeOfDay;
		}


		// At the moment the only VXML variable that contains the DID is 'session.connection_local_uri' which
		// contains the full URI for this SIP connection.  The DID is the part prior to the '@' symbol.
		// The following code will either extract the DID portion of the session.connection_local_uri variable or,
		// if the VXML eventually passes in just the DID it will just return it.

		private string GetDIDFromLocalURI(string i_sLocalURI)
		{
			return i_sLocalURI.Split(new char[] { '@' }, 2)[0];
		}

	}
}
