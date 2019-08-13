// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Specialized;
using System.Text;

namespace Incendonet.Utilities.StringHelper
{
	public class Utilities
	{
		public string CopyString(string i_sIn)
		{
			return (String.Copy(i_sIn));
		}

		public static bool GetItemsFromString(string i_sLine, char i_cSeparator, StringCollection o_asFields)
		{
			bool			bRet = true, bDone = false;
			int				iPrevIndex = 0, iNextIndex = 0, iLen;
			string			sTmp = "";

			try
			{
				iLen = i_sLine.Length;
				o_asFields.Clear();
				bDone = false;

				while (!bDone)
				{
					sTmp = "";
					iNextIndex = i_sLine.IndexOf(i_cSeparator, iPrevIndex);
					if ((iNextIndex == -1) || (iNextIndex == (iLen - 1)) || (iNextIndex == i_sLine.Length - 1))
					{
						bDone = true;
						if ((iNextIndex - iPrevIndex) > 0)
						{
							sTmp = i_sLine.Substring(iPrevIndex, iNextIndex - iPrevIndex);
						}
						else if ((iNextIndex == -1) && ((iPrevIndex + 1) < iLen))
						{
							sTmp = i_sLine.Substring(iPrevIndex, iLen - iPrevIndex);
						}
					}
					else
					{
						sTmp = i_sLine.Substring(iPrevIndex, iNextIndex - iPrevIndex);
						iPrevIndex = iNextIndex + 1;
					}

					if (sTmp.Length > 0)
					{
						o_asFields.Add(sTmp);
					}
				}
			}
			catch
			{
				bRet = false;
			}
			return (bRet);
		} // GetItemsFromString()

		/// <summary>
		/// Returns a copy of the string that is upper-cased and with spaces between each letter.  Handy when TTSing acronyms.
		/// </summary>
		/// <param name="i_sIn"></param>
		/// <returns></returns>
		public string SpacifyString(string i_sIn)
		{
            return StringInterlace(i_sIn, " ");
		}

        /// <summary>
        /// Returns a copy of the string that is upper-cased and with a comma and space between each letter.  Handy when TTSing acronyms.
        /// </summary>
        /// <param name="i_sIn"></param>
        /// <returns></returns>
        public string SpacifyStringSlow(string i_sIn)
        {
            return StringInterlace(i_sIn, ", ");
        }

        /// <summary>
        /// Returns a copy of the string that is upper-cased and the value specified by i_sSeparatorString between each letter.
        /// </summary>
        /// <param name="i_sOriginalString"></param>
        /// <param name="i_sSeparatorString"></param>
        /// <returns></returns>
        private string StringInterlace(string i_sOriginalString, string i_sSeparatorString)
        {
            string sRet = "";

            try
            {
                if (String.IsNullOrEmpty(i_sOriginalString))
                {
                    sRet = "";
                }
                else
                {
                    string sUpperCaseString = i_sOriginalString.ToUpper();
                    int iNumberOfCharacters = sUpperCaseString.Length;

                    if (iNumberOfCharacters == 1)
                    {
                        sRet = sUpperCaseString;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.Append(sUpperCaseString[0]);

                        for (int i = 1; i < iNumberOfCharacters; ++i)
                        {
                            sb.Append(i_sSeparatorString);
                            sb.Append(sUpperCaseString[i]);
                        }

                        sRet = sb.ToString();
                    }
                }
            }
            catch (Exception exc)
            {
                sRet = "";
                Console.Error.WriteLine(String.Format("{0} Utilities.StringInterlace exception: {1}", DateTime.Now, exc.ToString()));
            }

            return sRet;
        } // StringInterlace

		/// <summary>
		/// Returns whether or not a given string is the given length.
		/// </summary>
		/// <param name="i_sIn"></param>
		/// <param name="i_sLen"></param>
		/// <returns></returns>
		public bool CorrectLength(string i_sIn, string i_sLen)
		{
			return(CorrectLength(i_sIn, int.Parse(i_sLen)));
		}

		/// <summary>
		/// Returns whether or not a given string is the given length.
		/// </summary>
		/// <param name="i_sIn"></param>
		/// <param name="i_iLen"></param>
		/// <returns></returns>
		public bool CorrectLength(string i_sIn, int i_iLen)
		{
			bool			bRet = true;
			
			if( (i_sIn != null) && (i_sIn.Length == i_iLen) )
			{
				bRet = true;
			}
			else
			{
				bRet = false;
			}
			
			return(bRet);
		} // CorrectLength

	} // Utilities
}
