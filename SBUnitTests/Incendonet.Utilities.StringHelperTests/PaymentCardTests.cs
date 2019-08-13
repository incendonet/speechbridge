// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;

using Incendonet.Utilities.StringHelper;

namespace Incendonet.Utilities.StringHelperTests
{
	[TestFixture]
	public class PaymentCardTests
	{
		[Test]
		public void TestIsValidExp()
		{
			string sValidExp = "", sMonth = "";

			if (DateTime.Now.Month < 10)
			{
				sMonth = "0" + DateTime.Now.Month.ToString();
			}
			else
			{
				sMonth = DateTime.Now.Month.ToString();
			}

			sValidExp = sMonth + (DateTime.Now.Year - 1999).ToString();

			Assert.IsTrue(PaymentCard.IsValidExp(sValidExp));
			Assert.IsFalse(PaymentCard.IsValidExp(null));
			Assert.IsFalse(PaymentCard.IsValidExp(""));
			Assert.IsFalse(PaymentCard.IsValidExp("111"));
			Assert.IsFalse(PaymentCard.IsValidExp("11111"));
			Assert.IsFalse(PaymentCard.IsValidExp("0000"));
			Assert.IsFalse(PaymentCard.IsValidExp("0101"));
			Assert.IsFalse(PaymentCard.IsValidExp("1320"));
			Assert.IsFalse(PaymentCard.IsValidExp(sMonth + (DateTime.Now.Year - 2001).ToString()));
		} // TestIsValidExp

		// Ludwig's tests:
		[Test]
		public void TestThatIsValidExpirationDateReturnsFalseForNull()
		{
			Assert.That(PaymentCard.IsValidExp(null), Is.EqualTo(false));
		}

		[Test]
		public void TestThatIsValidExpirationDateReturnsFalseForTooFewDigits()
		{
			Assert.That(PaymentCard.IsValidExp("123"), Is.EqualTo(false));
		}

		[Test]
		public void TestThatIsValidExpirationDateReturnsFalseForTooManyDigits()
		{
			Assert.That(PaymentCard.IsValidExp("12345"), Is.EqualTo(false));
		}

		[Test]
		public void TestThatIsValidExpirationDateReturnsFalseForInvalidMonth()
		{
			Assert.That(PaymentCard.IsValidExp("0019"), Is.EqualTo(false));
			Assert.That(PaymentCard.IsValidExp("1319"), Is.EqualTo(false));
		}

		[Test]
		public void TestThatIsValidExpirationDateReturnsFalseForInvalidYear()
		{
			Assert.That(PaymentCard.IsValidExp("0314"), Is.EqualTo(false));
		}

		[Test]
		public void TestThatIsValidExpirationDateReturnsTrueForValidExpirationDate()
		{
			Assert.That(PaymentCard.IsValidExp("1117"), Is.EqualTo(true));
		}

		[Test]
		public void TestThatIsValidExpirationDateReturnsFalseForNonNumericCharacters()
		{
			Assert.That(PaymentCard.IsValidExp("031#"), Is.EqualTo(false));
		}

	} // PaymentCardTests
}
