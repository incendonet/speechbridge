// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Text;

namespace Incendonet.Utilities.StringHelper
{
	public sealed class NumberFormatter
	{
		/// <summary>
		/// Returns a copy of the string with spaces between each letter and commas for pauses.  Handy when TTSing phone numbers.
		/// </summary>
		/// <param name="i_sPhoneNumber"></param>
		/// <returns></returns>
		public string SpacifyPhoneNumber(string i_sPhoneNumber)
		{
			string sRet = "";

			try
			{
                if(!String.IsNullOrEmpty(i_sPhoneNumber))
				{
					string sDigits = i_sPhoneNumber.Trim();
					int iNumberOfDigits = sDigits.Length;

                    switch (iNumberOfDigits)
                    {
                        case 7:                     // Assume it is a US 7 digit phone number so add comma at appropriate place to break up number readback.
                            sRet = String.Format("{0} {1} {2}, {3} {4} {5} {6}", sDigits[0], sDigits[1], sDigits[2], sDigits[3], sDigits[4], sDigits[5], sDigits[6]);
                            break;

                        case 10:                    // Assume it is a US 10 digit phone number so add commas in appropriate places to break up number readback.
                            sRet = String.Format("{0} {1} {2}, {3} {4} {5}, {6} {7} {8} {9}", sDigits[0], sDigits[1], sDigits[2], sDigits[3], sDigits[4], sDigits[5], sDigits[6], sDigits[7], sDigits[8], sDigits[9]);
                            break;

                        default:                    // Given the number of digits we have no idea how they should be "grouped" so just put a space between each of them.
                            sRet = SpacifyNumber(sDigits);
                            break;
                    }
				}
			}
			catch (Exception exc)
			{
				sRet = "";
                Console.Error.WriteLine(String.Format("{0} NumberFormatter.SpacifyPhoneNumber exception: '{1}'.", DateTime.Now, exc.ToString()));
			}

			return sRet;
		} // SpacifyPhoneNumber
		
		public string SpacifyCreditCardNumber(string i_sCreditCardNumber)
		{
			string sRet = "";
			
			try
			{
                if(!String.IsNullOrEmpty(i_sCreditCardNumber))
				{
                    string sDigits = i_sCreditCardNumber.Trim();
                    int iNumberOfDigits = sDigits.Length;

                    switch (iNumberOfDigits)
                    {
                        case 14:                    // Diners Club - grouping is 4 6 4.
                            sRet = String.Format("{0} {1} {2} {3}, {4} {5} {6} {7} {8} {9}, {10} {11} {12} {13}", sDigits[0], sDigits[1], sDigits[2], sDigits[3], sDigits[4], sDigits[5], sDigits[6], sDigits[7], sDigits[8], sDigits[9], sDigits[10], sDigits[11], sDigits[12], sDigits[13]);
                            break;

                        case 15:                    // American Express - grouping of 4 6 5.
                            sRet = String.Format("{0} {1} {2} {3}, {4} {5} {6} {7} {8} {9}, {10} {11} {12} {13} {14}", sDigits[0], sDigits[1], sDigits[2], sDigits[3], sDigits[4], sDigits[5], sDigits[6], sDigits[7], sDigits[8], sDigits[9], sDigits[10], sDigits[11], sDigits[12], sDigits[13], sDigits[14]);
                            break;

                        case 16:                    // Visa, Mastercard, Discover - grouping of 4 4 4 4.
                            sRet = String.Format("{0} {1} {2} {3}, {4} {5} {6} {7}, {8} {9} {10} {11}, {12} {13} {14} {15}", sDigits[0], sDigits[1], sDigits[2], sDigits[3], sDigits[4], sDigits[5], sDigits[6], sDigits[7], sDigits[8], sDigits[9], sDigits[10], sDigits[11], sDigits[12], sDigits[13], sDigits[14], sDigits[15]);
                            break;

                        default:                    // Given the number of digits we have no idea how they should be "grouped" so just put a space between each of them.
                            sRet = SpacifyNumber(sDigits);
                            break;
                    }
				}
			}
			catch (Exception exc)
			{
				sRet = "";
                Console.Error.WriteLine(String.Format("{0} NumberFormatter.SpacifyCreditCardNumber exception: '{1}'.", DateTime.Now, exc.ToString()));
			}
			
			return sRet;
		} // SpacifyCreditCardNumber


		public string ToDollars(string i_sDigits, string i_sIncludesCents)
		{
			return (ToDollars(i_sDigits, bool.Parse(i_sIncludesCents)));
		} // ToDollars

		public string ToDollars(string i_sDigits, bool i_bIncludesCents)
		{
			string				sRet = "";

			if (i_bIncludesCents)
			{
				sRet = string.Format("${0}.{1}", i_sDigits.Substring(0, (i_sDigits.Length - 2)), i_sDigits.Substring((i_sDigits.Length - 2), 2));
			}
			else
			{
				sRet = string.Format("${0}", i_sDigits);
			}

			return (sRet);

		} // ToDollars

        private string SpacifyNumber(string i_sDigits)
        {
            string sRet = "";

            int iNumberOfDigits = i_sDigits.Length;

            if (iNumberOfDigits < 2)
            {
                sRet = i_sDigits;
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(i_sDigits[0]);

                for (int i = 1; i < iNumberOfDigits; ++i)
                {
                    sb.Append(" ");
                    sb.Append(i_sDigits[i]);
                }

                sRet = sb.ToString();
            }

            return sRet;
        }
	} // class NumberFormatter
}
