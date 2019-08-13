// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
	[TestFixture]
	public class ShoreTelImportTest : ImportTestBase
	{
		[Test]
		public void TestThatShoreTelImportReturnsFalseIfNoDataPassedToIt()
		{
			Users importedUsers = null;
			byte[] abBytes = null;

			IImportParser shoreTelParser = new ShoreTelImportParser();
			bool bRet = shoreTelParser.Parse(abBytes, out importedUsers);

			Assert.That(bRet, Is.False);
		}


		[Test]
		public void TestShoreTelImport()
		{
			Users importedUsers = null;
			byte[] abBytes = GetBytesFromImportFile("ShoreTelExport.xls");

			IImportParser shoreTelParser = new ShoreTelImportParser();
			bool bRet = shoreTelParser.Parse(abBytes, out importedUsers);

			Assert.That(bRet, Is.True);
			Assert.That(importedUsers, Is.Not.Null);
			Assert.That(importedUsers.Count, Is.EqualTo(11));

			Assert.That(importedUsers[2].LName, Is.EqualTo("Lundgrön"));
			Assert.That(importedUsers[2].FName, Is.EqualTo("Dålf"));
			Assert.That(importedUsers[2].MName, Is.EqualTo(""));
			Assert.That(importedUsers[2].Ext, Is.EqualTo("116"));
			Assert.That(importedUsers[2].Email, Is.EqualTo(""));
			Assert.That(importedUsers[2].Username, Is.EqualTo(""));
			Assert.That(importedUsers[2].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[2].MobileNumber, Is.EqualTo(""));
			Assert.That(importedUsers[2].PagerNumber, Is.EqualTo(""));

			Assert.That(importedUsers[3].LName, Is.EqualTo("Mitterrand"));
			Assert.That(importedUsers[3].FName, Is.EqualTo("François"));
			Assert.That(importedUsers[3].MName, Is.EqualTo(""));
			Assert.That(importedUsers[3].Ext, Is.EqualTo("117"));
			Assert.That(importedUsers[3].Email, Is.EqualTo(""));
			Assert.That(importedUsers[3].Username, Is.EqualTo(""));
			Assert.That(importedUsers[3].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[3].MobileNumber, Is.EqualTo(""));
			Assert.That(importedUsers[3].PagerNumber, Is.EqualTo(""));

			Assert.That(importedUsers[4].LName, Is.EqualTo("قرآن، مرآ"));
			Assert.That(importedUsers[4].FName, Is.EqualTo("آداب، آية،"));
			Assert.That(importedUsers[4].MName, Is.EqualTo(""));
			Assert.That(importedUsers[4].Ext, Is.EqualTo("118"));
			Assert.That(importedUsers[4].Email, Is.EqualTo(""));
			Assert.That(importedUsers[4].Username, Is.EqualTo(""));
			Assert.That(importedUsers[4].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[4].MobileNumber, Is.EqualTo(""));
			Assert.That(importedUsers[4].PagerNumber, Is.EqualTo(""));

			Assert.That(importedUsers[6].LName, Is.EqualTo("Ayers"));
			Assert.That(importedUsers[6].FName, Is.EqualTo("Bryan"));
			Assert.That(importedUsers[6].MName, Is.EqualTo(""));
			Assert.That(importedUsers[6].Ext, Is.EqualTo("203"));
			Assert.That(importedUsers[6].Email, Is.EqualTo(""));
			Assert.That(importedUsers[6].Username, Is.EqualTo(""));
			Assert.That(importedUsers[6].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[6].MobileNumber, Is.EqualTo(""));
			Assert.That(importedUsers[6].PagerNumber, Is.EqualTo(""));

			Assert.That(importedUsers[8].LName, Is.EqualTo("Harris"));
			Assert.That(importedUsers[8].FName, Is.EqualTo("Steve"));
			Assert.That(importedUsers[8].MName, Is.EqualTo(""));
			Assert.That(importedUsers[8].Ext, Is.EqualTo("205"));
			Assert.That(importedUsers[8].Email, Is.EqualTo(""));
			Assert.That(importedUsers[8].Username, Is.EqualTo(""));
			Assert.That(importedUsers[8].Domain, Is.EqualTo(""));
			Assert.That(importedUsers[8].MobileNumber, Is.EqualTo(""));
			Assert.That(importedUsers[8].PagerNumber, Is.EqualTo(""));
		}
	}
}
