// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SBConfigStor
{
	public sealed class ShoreTelImportParser : IImportParser
	{
		private sealed class Entry
		{
			public string FName;
			public string LName;
			public string Ext;
		}

		#region IImportParser Members

		public bool Parse(byte[] i_abBytes, out Users o_Users)
		{
			bool bRet = true;

			o_Users = new Users();

			try
			{
				string importString = Encoding.UTF8.GetString(i_abBytes);

				List<Entry> entries = GetEntries(importString);

				foreach (Entry entry in entries)
				{
					Users.User user = new Users.User();

					user.LName = entry.LName;
					user.FName = entry.FName;
					user.Ext = entry.Ext;

					o_Users.Add(user);
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + "SBConfigStore.ShoreTelImportParser.Parse exception: " + exc.ToString());
			}

			return bRet;
		}

		#endregion

		private List<Entry> GetEntries(string i_ImportString)
		{
			List<Entry> entries = new List<Entry>();

			// The ShoreTel export file is basically a malformed HTML table.
			// Specifically the "header" row is delimited by a <tr>...<tr> combination (note incorrect closing tag and lower case).
			// Each of the rows that contain actual user data are correctly delimited by a <TR>...</TR> combination (note upper case).
			// Thus we can build a regular expression that extracts each of the user data rows.

			Regex regexEntries = new Regex(@"<TR>\s*<TD>(?<FName>.*)</TD>\s*<TD>(?<LName>.*)</TD>(?:\s*<TD>.*</TD>){3}\s*<TD>(?<Ext>.*)</TD>(?:\s*<TD>.*</TD>){4}\s*</TR>");

			MatchCollection matches = regexEntries.Matches(i_ImportString);

			foreach (Match match in matches)
			{
				if (match.Success)
				{
					Entry entry = new Entry();

					entry.FName = match.Groups["FName"].Value;
					entry.LName = match.Groups["LName"].Value;
					entry.Ext = match.Groups["Ext"].Value;

					entries.Add(entry);
				}
			}

			return entries;
		}
	}
}
