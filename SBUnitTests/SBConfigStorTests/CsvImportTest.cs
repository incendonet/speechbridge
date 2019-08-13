// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
	[TestFixture]
	public class CsvImportTest : ImportTestBase
	{
		[Test]
		public void TestCsvImportWithMandatoryFieldsOnly()
		{
			Users importedUsers = null;
			byte[] abBytes = GetBytesFromImportFile("MandatoryFields.csv");

			IImportParser csvParser = new CsvImportParser();
			csvParser.Parse(abBytes, out importedUsers);

			Assert.That(importedUsers, Is.Not.Null);
			Assert.That(importedUsers.Count, Is.EqualTo(1));
			Assert.That(importedUsers[0].LName, Is.EqualTo("Abercrombie"));
			Assert.That(importedUsers[0].FName, Is.EqualTo("Neil"));
			Assert.That(importedUsers[0].Ext, Is.EqualTo("8888"));
			Assert.That(importedUsers[0].Email, Is.EqualTo(""));
			Assert.That(importedUsers[0].Username, Is.EqualTo(""));
			Assert.That(importedUsers[0].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[0].MobileNumber, Is.EqualTo(""));
			Assert.That(importedUsers[0].PagerNumber, Is.EqualTo(""));
		}

		[Test]
		public void TestCsvImportWithOptionalFields()
		{
			Users importedUsers = null;
			byte[] abBytes = GetBytesFromImportFile("OptionalFields.csv");

			IImportParser csvParser = new CsvImportParser();
			csvParser.Parse(abBytes, out importedUsers);

			Assert.That(importedUsers, Is.Not.Null);
			Assert.That(importedUsers.Count, Is.EqualTo(1));
			Assert.That(importedUsers[0].LName, Is.EqualTo("Chatilov"));
			Assert.That(importedUsers[0].FName, Is.EqualTo("Alexandre"));
			Assert.That(importedUsers[0].Ext, Is.EqualTo("8888"));
			Assert.That(importedUsers[0].Email, Is.EqualTo(""));
			Assert.That(importedUsers[0].Username, Is.EqualTo(""));
			Assert.That(importedUsers[0].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[0].MobileNumber, Is.EqualTo("8580000000"));
			Assert.That(importedUsers[0].PagerNumber, Is.EqualTo("6190000000"));
		}
	}
}
