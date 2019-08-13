// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Incendonet.Utilities.StringHelper
{
	public class PaymentCard
	{
		public static bool IsValidExp(string i_sExp)
		{
			bool		bRet = true, bResMonth = true, bResYear = true;
			int			iMonth = 0, iYear = 0, iYearNow = 0, iMonthNow = 0;

			try
			{
				if (i_sExp == null)
				{
					bRet = false;
				}
				else if (i_sExp.Length != 4)
				{
					bRet = false;
				}
				else
				{
					iYearNow = DateTime.Now.Year - 2000;
					iMonthNow = DateTime.Now.Month;

					bResMonth = int.TryParse(i_sExp.Substring(0, 2), out iMonth);
					bResYear = int.TryParse(i_sExp.Substring(2, 2), out iYear);

					if ((!bResMonth) || (!bResYear) || (iMonth < 1) || (iMonth > 12) || (iYear < iYearNow))
					{
						bRet = false;
					}
					else if ((iYear == iYearNow) && (iMonth < iMonthNow))
					{
						bRet = false;
					}
					else if (iYear > (iYearNow + 20))
					{
						bRet = false;
					}
					else
					{
						bRet = true;
					}
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} PaymentCard.IsValidExp ERROR: Caught exception: '{1}'.", exc.Message);
				bRet = false;
			}

			return (bRet);
		} // IsValidExp

		public static string SpacifyCreditCardNumber(string i_sCreditCardNumber)
		{
			NumberFormatter			numFmt = new NumberFormatter();

			return (numFmt.SpacifyCreditCardNumber(i_sCreditCardNumber));
		} // SpacifyCreditCardNumber

	} // PaymentCard
}
