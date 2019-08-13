// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Text.RegularExpressions;

namespace ISMessaging
{
	/// <summary>
	/// Helper class for working with SIP addresses passed up from AudioRtr
	/// </summary>
	public class SBSipAddr
	{
		private static char []	s_acTrimChars = {' ', '\"'};
		private const string	m_csSipPortDef = "5060";

		public SBSipAddr()
		{
		}

		public static bool QuickParse(string i_sInput, out string o_sUser, out string o_sAddress, out string o_sPort, out string o_sDisplayName)
		{
			bool			bRet = true;
			int				iAt = -1, iColon = -1, iBar = -1;
			const string	sPatFull =			@"^""(?<name>[a-zA-Z0-9.\s\-]+)""[\s]+<sip:(?<user>\w+)@(?<addr>[a-zA-Z0-9.]+):(?<port>\d+)>";
			const string	sPatNoPort =		@"^""(?<name>[a-zA-Z0-9.\s\-]+)""[\s]+<sip:(?<user>\w+)@(?<addr>[a-zA-Z0-9.]+)>";
			const string	sPatNoName =		@"^<sip:(?<user>\w+)@(?<addr>[a-zA-Z0-9.]+):(?<port>\d+)>";
			const string	sPatNoNameNoPort =	@"^<sip:(?<user>\w+)@(?<addr>[a-zA-Z0-9.]+)>";
			const string	sPatBasic =			@"^sip:(?<user>\w+)@(?<addr>[a-zA-Z0-9.]+):(?<port>\d+)";
			const string	sPatBasicNoPort =	@"^sip:(?<user>\w+)@(?<addr>[a-zA-Z0-9.]+)";

			o_sUser = o_sAddress = o_sPort = o_sDisplayName = "";

			try
			{
				iAt = i_sInput.IndexOf('@');
				iColon = i_sInput.IndexOf(':');
				iBar = i_sInput.IndexOf('|');

				if( (iAt < 0) || (iColon < 0) || (iBar < 0) )
				{
					if(i_sInput.Contains("sip:") == false)
					{
						Console.Error.WriteLine("{0} SBSipAddr.QuickParse Input was invalid. (1)", DateTime.Now.ToString());
						bRet = false;
					}
					else
					{
						// Cascade (somewhat) ordered by decreasing likelihood of receipt
						if (Regex.IsMatch(i_sInput, sPatNoPort))
						{
							Regex regex = new Regex(sPatNoPort, RegexOptions.None, TimeSpan.FromMilliseconds(250));
							GroupCollection groups = regex.Match(i_sInput).Groups;

							o_sDisplayName = groups["name"].Value;
							o_sUser = groups["user"].Value;
							o_sAddress = groups["addr"].Value;
							o_sPort = m_csSipPortDef;
						}
						else if (Regex.IsMatch(i_sInput, sPatNoNameNoPort))
						{
							Regex				regex = new Regex(sPatNoNameNoPort, RegexOptions.None, TimeSpan.FromMilliseconds(250));
							GroupCollection		groups = regex.Match(i_sInput).Groups;

							o_sUser = groups["user"].Value;
							o_sAddress = groups["addr"].Value;
							o_sPort = m_csSipPortDef;
						}
						else if (Regex.IsMatch(i_sInput, sPatNoName))
						{
							Regex				regex = new Regex(sPatNoName, RegexOptions.None, TimeSpan.FromMilliseconds(250));
							GroupCollection		groups = regex.Match(i_sInput).Groups;

							o_sUser = groups["user"].Value;
							o_sAddress = groups["addr"].Value;
							o_sPort = groups["port"].Value;
						}
						else if (Regex.IsMatch(i_sInput, sPatFull))
						{
							Regex				regex = new Regex(sPatFull, RegexOptions.None, TimeSpan.FromMilliseconds(250));
							GroupCollection		groups = regex.Match(i_sInput).Groups;

							o_sDisplayName = groups["name"].Value;
							o_sUser = groups["user"].Value;
							o_sAddress = groups["addr"].Value;
							o_sPort = groups["port"].Value;
						}
						else if (Regex.IsMatch(i_sInput, sPatBasic))
						{
							Regex				regex = new Regex(sPatBasic, RegexOptions.None, TimeSpan.FromMilliseconds(250));
							GroupCollection		groups = regex.Match(i_sInput).Groups;

							o_sUser = groups["user"].Value;
							o_sAddress = groups["addr"].Value;
							o_sPort = groups["port"].Value;
						}
						else if (Regex.IsMatch(i_sInput, sPatBasicNoPort))
						{
							Regex				regex = new Regex(sPatBasicNoPort, RegexOptions.None, TimeSpan.FromMilliseconds(250));
							GroupCollection		groups = regex.Match(i_sInput).Groups;

							o_sUser = groups["user"].Value;
							o_sAddress = groups["addr"].Value;
							o_sPort = m_csSipPortDef;
						}
						else
						{
							Console.Error.WriteLine("{0} SBSipAddr.QuickParse Input was invalid. (2)", DateTime.Now.ToString());
							bRet = false;
						}
					}
				}
				else
				{
					o_sUser =			i_sInput.Substring(0, iAt);
					o_sAddress =		i_sInput.Substring((iAt + 1), (iColon - iAt - 1));
					o_sPort =			i_sInput.Substring((iColon + 1), (iBar - iColon - 1));
					o_sDisplayName =	i_sInput.Substring((iBar + 1), (i_sInput.Length - iBar - 1)).Trim(s_acTrimChars);

					if(o_sPort == "0")
					{
						o_sPort = "5060";
					}
				}
			}
			catch(Exception exc)
			{
				o_sUser = o_sAddress = o_sPort = o_sDisplayName = "";

				Console.Error.WriteLine("{0} SBSipAddr.QuickParse caught exception: {1}", DateTime.Now.ToString(), exc.ToString());

				return(bRet);
			}

			return(bRet);
		}
	}
}
