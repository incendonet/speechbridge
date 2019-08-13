// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;

using SBEmail;

namespace SBEmailTests
{
	[TestFixture]
	public class DateHelperTest
	{
		[Test]
		public void TestNoDateNonSpeakable()
		{
			string sUtcDateTimeString = ConvertLocalDateTimeToUtcString(new DateTime(2006, 04, 05, 22, 37, 41, 538, DateTimeKind.Local));
			bool bIncludeDate = false;
			bool bSpeakable = false;

			string sResult = WebDavConnectorImplementation.GetLocalDateText(sUtcDateTimeString, bIncludeDate, bSpeakable);

			Assert.That(sResult, Is.EqualTo("Wednesday, 10:37:41 PM"));
		}


		[Test]
		public void TestNoDateSpeakable()
		{
			string sUtcDateTimeString = ConvertLocalDateTimeToUtcString(new DateTime(2006, 04, 05, 22, 37, 41, 538, DateTimeKind.Local));
			bool bIncludeDate = false;
			bool bSpeakable = true;

			string sResult = WebDavConnectorImplementation.GetLocalDateText(sUtcDateTimeString, bIncludeDate, bSpeakable);

			Assert.That(sResult, Is.EqualTo("Wednesday, 10, 37, PM"));
		}

		[Test]
		public void TestIncludeDateNonSpeakable()
		{
			string sUtcDateTimeString = ConvertLocalDateTimeToUtcString(new DateTime(2006, 04, 05, 22, 37, 41, 538, DateTimeKind.Local));
			bool bIncludeDate = true;
			bool bSpeakable = false;

			string sResult = WebDavConnectorImplementation.GetLocalDateText(sUtcDateTimeString, bIncludeDate, bSpeakable);

			Assert.That(sResult, Is.EqualTo("Wednesday, April 05, 2006, 10:37:41 PM"));
		}

		[Test]
		public void TestIncludeDateSpeakable()
		{
			string sUtcDateTimeString = ConvertLocalDateTimeToUtcString(new DateTime(2006, 04, 05, 22, 37, 41, 538, DateTimeKind.Local));
			bool bIncludeDate = true;
			bool bSpeakable = true;

			string sResult = WebDavConnectorImplementation.GetLocalDateText(sUtcDateTimeString, bIncludeDate, bSpeakable);

			Assert.That(sResult, Is.EqualTo("Wednesday, April 05, 2006, 10, 37, PM"));
		}


		// Since WebDavConnector.GetLocalDateText() converts a date/time expressed in UTC into the local timezone 
		// this function is necessary to allow the tests to work regardless of which timezone they are running in.

		private string ConvertLocalDateTimeToUtcString(DateTime i_LocalDateTime)
		{
			DateTime UtcDateTime = i_LocalDateTime.ToUniversalTime();
			string UtcDateTimeString = String.Format("{0}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}.{6:D3}Z", UtcDateTime.Year, UtcDateTime.Month, UtcDateTime.Day, UtcDateTime.Hour, UtcDateTime.Minute, UtcDateTime.Second, UtcDateTime.Millisecond);

			return UtcDateTimeString;
		}
	}
}
