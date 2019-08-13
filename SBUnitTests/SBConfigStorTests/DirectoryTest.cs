// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
	[TestFixture]
	public class DirectoryTest
	{
		[Test]
		public void TestThatIfNoUsersAreMergedTheOriginalUsersCollectionIsUnchanged()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("0", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("0", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, new Users());

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatANewUserIsAddedAtEndOfUsersCollection()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("0", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "Jane", "Doe", "2222", "", "", "", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("0", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));
			expectedUsers.Add(CreateUser("", "Jane", "Doe", "2222", "", "", "", "", "", Users.eState.New));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatUsersWithSameUsernameAndDomainAreDeemedToBeTheSameUser()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("0", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "", "", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "Jack", "Doe", "2222", "jdoe@test.com", "jdoe", "test.com", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("0", "Jack", "Doe", "2222", "jdoe@test.com", "jdoe", "test.com", "", "", Users.eState.Dirty));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatUsersWithSameFirstAndLastNameAreDeemedToBeTheSameUserIfNoUsernameAndDomainPresent()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("1", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "John", "Doe", "2222", "", "", "", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("1", "John", "Doe", "2222", "", "", "", "", "", Users.eState.Dirty));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatIfAnExistingUserIsMergedWithoutAnyDataChangesThenTheUserWillBeMarkedAsClean()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("1", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("1", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatEmptyFieldsInMergedUserDoNotOverwriteExistingData()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("1", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "1111", "2222", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "John", "Doe", "", "", "", "", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("1", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "1111", "2222", Users.eState.Clean));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatUserIsOnlyMarkedAsDirtyIfDataHasChanged()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("1", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "1111", "2222", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "John", "Doe", "", "", "", "", "", "3333", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("1", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "1111", "3333", Users.eState.Dirty));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void ThatThatIfNoMatchOnUsernameAndDomainThenFirstAndLastNameAreUsedToDetermineIfUsersAreTheSame()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("1", "John", "Doe", "1111", "", "", "", "", "", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "1111", "2222", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("1", "John", "Doe", "1111", "jdoe@test.com", "jdoe", "test.com", "1111", "2222", Users.eState.Dirty));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatUsersWithoutFirstAndLastNameAreTreatedAsSeperateUsers()
		{
			Users existingUsers = new Users();
			existingUsers.Add(CreateUser("1", "", "", "", "", "John", "", "", "", Users.eState.Clean));

			Users newUsers = new Users();
			newUsers.Add(CreateUser("", "", "", "", "", "Jane", "", "", "", Users.eState.Clean));

			Users expectedUsers = new Users();
			expectedUsers.Add(CreateUser("1", "", "", "", "", "John", "", "", "", Users.eState.Clean));
			expectedUsers.Add(CreateUser("", "", "", "", "", "Jane", "", "", "", Users.eState.New));

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}


		/*
		 * This test was created in response to a problem Tim encountered in importing contacts from a CSV file.
		 * Specifically, the problem was that some duplicate contacts were not imported while others were.
		 * 
		 * The simplest case to illustrate this is a CSV file containing the following four contacts
		 * (none of which exist in the Directory prior to import):
		 * 
		 *		Billing,,7500
		 *		Billing,,7501
		 *		Claims,,7600
		 *		Claims,,7600
		 *		
		 * During the import "Billing" was not imported, while "Claims" was imported.
		 * 
		 * The problem was traced to the fact that the two instances of "Billing" have different extensions
		 * while the two instances of "Claims" have the same extension (i.e. are identical).
		 * 
		 * When checking the web log it was found that when we were trying to save "Billing" to the DB the following SQL exception was reported:
		 * 
		 *		Invalid input syntax for integer: "", Severity: ERROR, Code: 22P02
		 *		
		 * Turns out this was due to an UPDATE command being run for a non-existing UserID.  In other words, you can't update a record that does not exist.
		 * 
		 * The underlying problem was due to the logic that merges imported contacts into the Directory (Directory.MergeUsers()).
		 * As each contact is imported it is compared against all the contacts already in the Directory.  If no match is found (in this case 
		 * a match means the same name) then the contact is added to the Directory.  Thus, this contact now exists as a NEW contact in the
		 * Directory when the next imported contact is processed.
		 * If a match is found (i.e. same name) then one of two actions are taken:
		 *		* Nothing is done if the imported contact is an exact match (i.e. all the fields are identical).
		 *		* The existing contact in the Directory is marked as DIRTY and the various fields are updated with those from the imported contact.
		 * When it comes time to save the Directory to the DB any NEW contact is added via an INSERT statement and any DIRTY contact is updated via an UPDATE statement.
		 * 
		 * Thus, in this case the processing for "Claims" was as follows:
		 *		Add first instance of "Claims" as a NEW contact to the Directory.
		 *		Since the second instance of "Claims" is identical to the first instance it is just ignored.
		 *		"Claims" is saved to DB via INSERT.
		 * 
		 * The processing for "Billing" proceeded as follows:
		 *		Add first instance of "Billing" as a NEW contact to the Directory.
		 *		Since the second instance of "Billing" differs from the first one in the extension the entry for "Billing" in the Directory
		 *		was changed to DIRTY and the extension was changed from 7500 to 7501.
		 *		"Billing" is saved to DB via UPDATE - this fails since there is no record to update.
		 *		
		 * The problem was fixed by modifying the merge logic so that if a NEW contact is modified its state is left as NEW rather than changed to DIRTY.
		 * 
		 */

		[Test]
		public void TestThatIfANewUserIsAddedTwiceThenItIsTreatedAsANewUserRatherThanAnUpdatedUser()
		{
			Users existingUsers = new Users();

			Users.User importedUser1 = CreateUser("", "First", "Last", "1234", "", "", "", "", "", Users.eState.New);
			Users.User importedUser2 = CreateUser("", "First", "Last", "9999", "", "", "", "", "", Users.eState.New);

			Users newUsers = new Users();
			newUsers.Add(importedUser1);
			newUsers.Add(importedUser2);

			Users expectedUsers = new Users();
			expectedUsers.Add(importedUser2);

			Directory dir = new Directory();
			dir.MergeUsers(existingUsers, newUsers);

			Assert.That(AreUsersCollectionsTheSame(existingUsers, expectedUsers), Is.True);
		}

		[Test]
		public void TestThatUserEqualityOperationIsSymmetricalWithRespectToOptionalUserId()
		{
			Users.User user1 = new Users.User();
			Users.User user2 = new Users.User();

			user1.UserID = "";
			user2.UserID = "1";

			Assert.That(user1 == user2, Is.True, "user1 == user2");
			Assert.That(user2 == user1, Is.True, "user2 == user1");
		}

	
		// Utility methods

		private Users.User CreateUser(string i_sUserId, string i_sFirstName, string i_sLastName, string i_sExtension, string i_sEmail, string i_sUsername, string i_sDomain, string i_sMobileNumber, string i_sPagerNumber, Users.eState i_eState)
		{
			Users.User user = new Users.User();

			user.UserID = i_sUserId;
			user.FName = i_sFirstName;
			user.LName = i_sLastName;
			user.Ext = i_sExtension;
			user.Email = i_sEmail;
			user.Username = i_sUsername;
			user.Domain = i_sDomain;
			user.MobileNumber = i_sMobileNumber;
			user.PagerNumber = i_sPagerNumber;
			user.State = i_eState;

			return user;
		}

		private bool AreUsersCollectionsTheSame(Users i_actualUsers, Users i_expectedUsers)
		{
			bool bCollectionsAreTheSame = true;

			if ((i_expectedUsers == null) || (i_actualUsers == null))
			{
				if (i_expectedUsers == null)
				{
					Console.Error.WriteLine("i_expectedUsers is null.");
				}

				if (i_actualUsers == null)
				{
					Console.Error.WriteLine("i_actualUsers is null.");
				}

				return false;
			}

			if (i_expectedUsers.Count != i_actualUsers.Count)
			{
				Console.Error.WriteLine("i_expectedUsers.Count: {0}, i_actualUsers.Count: {1}", i_expectedUsers.Count, i_actualUsers.Count);

				return false;
			}

			for (int i = 0; i < i_expectedUsers.Count; ++i)
			{
				Users.User expectedUser = i_expectedUsers[i];
				Users.User actualUser = i_actualUsers[i];

				if ((expectedUser.UserID != actualUser.UserID) ||
					(expectedUser.FName != actualUser.FName) ||
					(expectedUser.LName != actualUser.LName) ||
					(expectedUser.Ext != actualUser.Ext) ||
					(expectedUser.Email != actualUser.Email) ||
					(expectedUser.Username != actualUser.Username) ||
					(expectedUser.Domain != actualUser.Domain) ||
					(expectedUser.MobileNumber != actualUser.MobileNumber) ||
					(expectedUser.PagerNumber != actualUser.PagerNumber) ||
					(expectedUser.State != actualUser.State))
				{
					bCollectionsAreTheSame = false;

					Console.Error.WriteLine("[{0}] expectedUser --> {1}", i, PrintUser(expectedUser));
					Console.Error.WriteLine("[{0}] actualUser   --> {1}", i, PrintUser(actualUser));
				}
			}

			return bCollectionsAreTheSame;
		}

		private string PrintUser(Users.User i_user)
		{
			return String.Format("UserID: {0}, FName: '{1}', LName: '{2}', Ext: '{3}', Username: '{4}', Domain: '{5}', Mobile: '{6}', Pager: '{7}', State: {8}", i_user.UserID, i_user.FName, i_user.LName, i_user.Ext, i_user.Username, i_user.Domain, i_user.MobileNumber, i_user.PagerNumber, i_user.State);
		}
	}
}
