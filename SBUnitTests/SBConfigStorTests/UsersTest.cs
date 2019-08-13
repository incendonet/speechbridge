// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Specialized;

using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
	[TestFixture]
	public class UsersTest
	{
		[Test]
		public void TestThatEmailSetsEmptyUsernameAndDomain()
		{
			string sUsername = "account";
			string sDomain = "domain.com";

			Users.User user = new Users.User();

			user.Email = String.Format("{0}@{1}", sUsername, sDomain);

			user.SetUsernameAndDomainFromEmail();

			Assert.That(user.Username, Is.EqualTo(sUsername));
			Assert.That(user.Domain, Is.EqualTo(sDomain));
		}

		[Test]
		public void TestThatEmptyEmailDoesNotAffectExistingUsernameAndDomain()
		{
			string sUsername = "account";
			string sDomain = "domain.com";

			Users.User user = new Users.User();

			user.Email = "";
			user.Username = sUsername;
			user.Domain = sDomain;

			user.SetUsernameAndDomainFromEmail();

			Assert.That(user.Username, Is.EqualTo(sUsername));
			Assert.That(user.Domain, Is.EqualTo(sDomain));
		}

		[Test]
		public void TestThatNonEmptyEmailDoesNotAffectExistingUsernameAndDomain()
		{
			string sUsername = "account";
			string sDomain = "domain.com";

			Users.User user = new Users.User();

			user.Email = "test@test.com";
			user.Username = sUsername;
			user.Domain = sDomain;

			user.SetUsernameAndDomainFromEmail();

			Assert.That(user.Username, Is.EqualTo(sUsername));
			Assert.That(user.Domain, Is.EqualTo(sDomain));
		}

		[Test]
		public void TestThatAltPronunciationsPropertyHasLeadingAndTrailingSpacesRemoved()
		{
			string sAltPronunciations = "Alias1;Alias2;";

			Users.User user = new Users.User();

			user.AltPronunciations = String.Format("   {0}   ", sAltPronunciations);

			Assert.That(user.AltPronunciations, Is.EqualTo(sAltPronunciations));
		}

		[Test]
		public void TestThatAltPronunciationsPropertySetToNullReturnsAnEmptyString()
		{
			Users.User user = new Users.User();

			user.AltPronunciations = null;

			Assert.That(user.AltPronunciations, Is.EqualTo(""));
		}

		[Test]
		public void TestThatAltPronunciationsPropertySetToNullReturnsAnEmptyAltPronunciationCollection()
		{
			Users.User user = new Users.User();

			user.AltPronunciations = null;

			Assert.That(user.AltPronunciationColl, Is.Not.Null);
			Assert.That(user.AltPronunciationColl.Count, Is.EqualTo(0));
		}

		[Test]
		public void TestThatEmptyAltPronunciationsPropertyReturnsEmptyAltPronunciationCollection()
		{
			Users.User user = new Users.User();

			user.AltPronunciations = "";
			StringCollection alternatePronunciationCollection = user.AltPronunciationColl;

			Assert.That(alternatePronunciationCollection, Is.Not.Null);
			Assert.That(alternatePronunciationCollection.Count, Is.EqualTo(0));
		}

		[Test]
		public void TestThatAltPronunciationsPropertyWithOneEntryReturnsAltPronunciationCollectionWithThatEntry()
		{
			Users.User user = new Users.User();

			user.AltPronunciations = "Alias;";
			StringCollection alternatePronunciationCollection = user.AltPronunciationColl;

			Assert.That(alternatePronunciationCollection.Count, Is.EqualTo(1));
			Assert.That(alternatePronunciationCollection[0], Is.EqualTo("Alias"));
		}

		[Test]
		public void TestThatAltPronunciationsPropertyWithManyEntriesReturnsAltPronunciationCollectionWithThoseEntries()
		{
			Users.User user = new Users.User();

			user.AltPronunciations = "Alias1;Alias2;";
			StringCollection alternatePronunciationCollection = user.AltPronunciationColl;

			Assert.That(alternatePronunciationCollection.Count, Is.EqualTo(2));
			Assert.That(alternatePronunciationCollection[0], Is.EqualTo("Alias1"));
			Assert.That(alternatePronunciationCollection[1], Is.EqualTo("Alias2"));
		}
	}
}
