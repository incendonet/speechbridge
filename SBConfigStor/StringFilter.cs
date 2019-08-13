// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Text;


namespace SBConfigStor
{
	public class StringFilter
	{
		private enum e_FilterMode
		{
			Normal,
			Domain,
			Username,
			Path,
		};

		private const string		m_sBadChars =			"`~!@#$%^&*()_=+[{]}\\|;:\",<.>/?\a\b\t\r\v\f\n\u001B";			// FIX - More are in Unicode char set.
		private const string		m_sBadCharsDomain =		" `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B";			// FIX - More are in Unicode char set.
		private const string		m_sBadCharsUsername =	" `~!@#$%^&*()=+[{]}\\|;:'\",<>/?\a\b\t\r\v\f\n\u001B";			// FIX - More are in Unicode char set.
		private const string		m_sBadCharsPath =		"`~!@#$%^&*()=+[{]}|;'\",<>?\a\b\t\r\v\f\n\u001B";				// FIX - More are in Unicode char set.

		private static string RemoveChars(string i_sInput, char[] i_caChars)
		{
			string			sRet = "";
			int				iIndex = -1, iNumChars = 0;
			int				ii = 0;

			try
			{
				// This is more than likely not the most efficient way to do this...
				if( (i_sInput == null) || (i_caChars == null) || (i_sInput == "") || (i_caChars.Length <= 0) )
				{
					//Console.Error.WriteLine(DateTime.Now.ToString() + " RemoveChars given null input.");		// Commented out, because it will happen with great frequency.
				}
				else
				{
					iNumChars = i_caChars.Length;
					sRet = i_sInput;

					for(ii = 0; ii < iNumChars; ii++)
					{
						iIndex = sRet.IndexOf(i_caChars[ii]);
						while(iIndex >= 0)
						{
							sRet = sRet.Remove(iIndex, 1);
							iIndex = sRet.IndexOf(i_caChars[ii], iIndex);
						}
					}
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " RemoveChars caught exception: " + exc.ToString());
			}

			return(sRet);
		}

		private static string GetFilteredString(e_FilterMode i_eMode, string i_sSrc)
		{
			string		sRet = "";

			// FIX - Do more sophisticated filtering, and allow for externally defined filters.
			try
			{
				if (i_sSrc.Length > 0)
				{
					// Replacing strings - this has to be done first since the string being replaced might contain characters that are stripped out by RemoveChars() below.

					if (i_sSrc.IndexOf("&nbsp;") >= 0)
					{
						i_sSrc = i_sSrc.Replace("&nbsp;", "");
					}

					if (i_sSrc.IndexOf("&nbsp") >= 0)
					{
						i_sSrc = i_sSrc.Replace("&nbsp", "");
					}

					if (i_sSrc.IndexOf(" & ") >= 0)
					{
						i_sSrc = i_sSrc.Replace(" & ", " and ");
					}
				}

				switch (i_eMode)
				{
					case e_FilterMode.Normal :
                        if (i_sSrc.IndexOf("_") >= 0)
                        {
                            i_sSrc = i_sSrc.Replace("_", " ");
                        }

						sRet = RemoveChars(i_sSrc, m_sBadChars.ToCharArray());
						break;
					case e_FilterMode.Domain :
						sRet = RemoveChars(i_sSrc, m_sBadCharsDomain.ToCharArray());
						break;
					case e_FilterMode.Username :
						sRet = RemoveChars(i_sSrc, m_sBadCharsUsername.ToCharArray());
						break;
					case e_FilterMode.Path :
						sRet = RemoveChars(i_sSrc, m_sBadCharsPath.ToCharArray());
						break;
					default :
						sRet = RemoveChars(i_sSrc, m_sBadChars.ToCharArray());
						Console.Error.WriteLine(String.Format("{0} GetFilteredString received unknown i_eMode parameter ('{1}')", DateTime.Now.ToString(), i_eMode));
						break;
				};


				// Remove leading and trailing spaces.

				if (sRet.Length > 0)
				{
					sRet = sRet.Trim();
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " GetFilteredString caught exception: {0}", exc.ToString());
			}

//Console.Error.WriteLine("### GetFilteredString out:  '{0}'.", sRet);
			return(sRet);
		} // GetFilteredString

		public static string GetFilteredString(string i_sSrc)
		{
			string sRet = GetFilteredString(e_FilterMode.Normal, i_sSrc);

			// Compress consecutive spaces into one space.

			while (sRet.IndexOf("  ") >= 0)
			{
				sRet = sRet.Replace("  ", " ");
			}

			return sRet;
		}

		public static string GetFilteredStringDial(string i_sSrc)
		{
			StringBuilder sbRet = new StringBuilder(i_sSrc.Length);

			foreach (char c in i_sSrc)
			{
				if (char.IsDigit(c))
				{
					sbRet.Append(c);
				}
			}

			return sbRet.ToString();
		}

		public static string GetFilteredStringPath(string i_sSrc)
		{
			return GetFilteredString(e_FilterMode.Path, i_sSrc);
		}

		public static string GetFilteredStringDomain(string i_sSrc)
		{
			// According to RFC 1123 (http://www.ietf.org/rfc/rfc1123.txt Section 2.1) the legal characters are letters, digits, hyphen and period.  A hyphen is not allowed to be the first or last character.
			// This applies to every "label" (the sections between periods) but we don't check that closely here.  All we are trying to do is make sure that we don't allow the user to enter something flagrantly incorrect.

			// Remove all characters that are illegal anywhere in the domain name.

			string sRet = GetFilteredString(e_FilterMode.Domain, i_sSrc);

			// Remove any leading an trailing hyphens (has to be done after removing all illegal characters).

			return sRet.Trim(new char[] {'-'});
		}

		public static string GetFilteredStringEmail(string i_sSrc)
		{
			string sRet = "";

			/*
			   The best summary of what is allowed for a valid e-mail address is the Wikipedia entry for "E-mail address".  Specifically see http://en.wikipedia.org/wiki/Email_address#Local_part.
			   As this points out most of the e-mail providers are actually more restrictive than the rules allows.  A quick test conducted on 5/31/12 shows the following limitations by the big
			   online e-mail providers for the username portion:
									 GMail        - allows letters, digits and period
									 Yahoo! Mail  - allows letters, digits, period and underscore
									 Windows Live - allows letters, digits, period, underscore and hyphen
			
			   Thus, we'll go with the least restrictive of the above.
			*/

			// Make sure that string provided contains an @ sign before we try any further processing.

			if (i_sSrc.IndexOf('@') > 0)
			{

				// Split e-mail address into username and domain at first @ sign.

				string[] sSplitString = i_sSrc.Split(new char[] { '@' }, 2);
				string sUsername = GetFilteredStringUsername(sSplitString[0]);
				string sDomain = GetFilteredStringDomain(sSplitString[1]);


				sRet = String.Format("{0}@{1}", sUsername, sDomain);
			}
			else
			{
				// No @ sign means that everything in the provided string is 'illegal' since it can't be a valid e-mail address without an @.

				sRet = "";
			}

			return sRet;
		}

		public static string GetFilteredStringUsername(string i_sSrc)
		{
			// See the comment in GetFilteredStringEmail() for what is allowed in the username.
			// In addition to which characters are allowed also enforce that the username cannot start or end with a period.

			string sRet = GetFilteredString(e_FilterMode.Username, i_sSrc);

			return sRet.Trim(new char[] { '.' });
		}

		public static string RemoveSpaces(string i_sSrc)
		{
			return i_sSrc.Replace(" ", "");
		}

		public static string RemoveApostrophes(string i_sSrc)
		{
			return i_sSrc.Replace("'", "");
		}
	} // StringFilter
}
