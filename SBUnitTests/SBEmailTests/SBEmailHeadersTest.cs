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
	public class SBEmailHeadersTest
	{
		[Test]
		public void TestThatEmptySBEmailHeadersCollectionHasATotalOfZeroEntries()
		{
			SBEmailHeaders sbEmailHeaders = new SBEmailHeaders();

			Assert.That(sbEmailHeaders.NumberOfEntries, Is.EqualTo(0));
		}

		[Test]
		public void TestThatEmptySBEmailHeadersCollectionHasATotalOfZeroUnreadEntries()
		{
			SBEmailHeaders sbEmailHeaders = new SBEmailHeaders();

			Assert.That(sbEmailHeaders.NumberOfUnreadEntries, Is.EqualTo(0));
		}

		[Test]
		public void TestThatSBEmailHeadersCollectionReturnsCorrectNumberOfEntries()
		{
			int iNumberOfEntries = 2;
			SBEmailHeaders sbEmailHeaders = new SBEmailHeaders();

			for (int i = 0; i < iNumberOfEntries; ++i)
			{
				sbEmailHeaders.Add(new SBEmailHeaders.SBEmailHeader());
			}

			Assert.That(sbEmailHeaders.NumberOfEntries, Is.EqualTo(iNumberOfEntries));
		}

		[Test]
		public void TestThatSBEmailHeadersCollectionReturnsCorrectNumberOfUnreadEntries()
		{
			int iNumberOfUnreadEntries = 2;

			SBEmailHeaders sbEmailHeaders = new SBEmailHeaders();

			for (int i = 0; i < iNumberOfUnreadEntries; ++i)
			{
				SBEmailHeaders.SBEmailHeader sbEmailHeaderUnread = new SBEmailHeaders.SBEmailHeader();
				sbEmailHeaderUnread.m_bRead = false;
				sbEmailHeaders.Add(sbEmailHeaderUnread);
			}

			SBEmailHeaders.SBEmailHeader sbEmailHeaderRead = new SBEmailHeaders.SBEmailHeader();
			sbEmailHeaderRead.m_bRead = true;
			sbEmailHeaders.Add(sbEmailHeaderRead);

			Assert.That(sbEmailHeaders.NumberOfUnreadEntries, Is.EqualTo(iNumberOfUnreadEntries));
		}
	}
}
