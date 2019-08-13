// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Text;

namespace SBConfigStor
{
	public sealed class CsvImportParser : IImportParser
	{
		private static char[] m_acSeparators = { '\0', '\n', '\r' };
		private enum eSeparators
		{
			NULL = 0,
			CR,
			LF,
		};

		#region IImportParser Members

		/// <summary>
		/// This supports CSV upload with the required columns being LName, FName, and Ext.
		/// Optional columns currently supported are E-mail, Username, Domain, Mobile Number and Pager Number.
		/// Future support for any additional columns will have to be added here.
		/// </summary>
		public bool Parse(byte[] i_abBytes, out Users o_Users)
		{
			bool bRet = true;

			o_Users = new Users();

			try
			{
				int ii, iArrSize, iPrevStart, iNumStrs;
				ArrayList lIndexes, lFields;
				string sLine = "", sLName = "", sFName = "", sExt = "";
				string sEmail = "", sUser = "", sDomain = "";
				string sMobileNumber = "";
				string sPagerNumber = "";
				Users.User tmpUser = null;
				Encoding oEncoding = null;
				byte[] abLine = null;

				// Break byte array into individual strings
				iArrSize = i_abBytes.Length;
				iPrevStart = 0;
				iNumStrs = 0;
				lIndexes = new ArrayList();

				for (ii = 0; ii < iArrSize; ii++)
				{
					if ((i_abBytes[ii] == m_acSeparators[(int)eSeparators.CR]) || (i_abBytes[ii] == m_acSeparators[(int)eSeparators.LF]))
					{
						i_abBytes[ii] = 0;
						if ((ii + 1) < iArrSize)
						{
							ii++;
							while ((ii < iArrSize) && ((i_abBytes[ii] == m_acSeparators[(int)eSeparators.CR]) || (i_abBytes[ii] == m_acSeparators[(int)eSeparators.LF])))
							{
								//i_abBytes[ii] = 0;	//string.TrimEnd doesn't seem to work on '\0'.
								ii++;
							}

							lIndexes.Add(iPrevStart);
							iPrevStart = ii;
							iNumStrs++;
						}
					}
				}
				lIndexes.Add(iArrSize - 1);

				// Parse each string
				lFields = new ArrayList();
				for (ii = 0; ii < iNumStrs; ii++)
				{
					sLine = "";
					sLName = "";
					sFName = "";
					sExt = "";
					sEmail = "";
					sUser = "";
					sDomain = "";
					sMobileNumber = "";
					sPagerNumber = "";

					// Convert from Windows file encoding to UTF8
					// FIX - Always safe to assume the file is Windows encoded???  Will probably cause LumenVox to fail if they upload a CSV file created on Linux.
					oEncoding = Encoding.GetEncoding("windows-1252");
					abLine = Encoding.Convert(oEncoding, Encoding.UTF8, i_abBytes, (int)lIndexes[ii], ((int)lIndexes[ii + 1] - (int)lIndexes[ii]));
					sLine = Encoding.UTF8.GetString(abLine);

					sLine = sLine.TrimEnd(m_acSeparators);

					if (ii > 0)	// Skip the first line, assumed to be a header.
					{
						bool bRes = GetFields(sLine, lFields);

						// Insert valid entries into DB
						sLName = (string)lFields[0];		// FIX - Assumes [lname, fname, ext]
						sFName = (string)lFields[1];
						sExt = (string)lFields[2];
						if (lFields.Count > 3)
						{
							sEmail = (string)lFields[3];
						}
						if (lFields.Count > 4)
						{
							sUser = (string)lFields[4];
						}
						if (lFields.Count > 5)
						{
							sDomain = (string)lFields[5];
						}
						if (lFields.Count > 6)
						{
							sMobileNumber = (string)lFields[6];
						}
						if (lFields.Count > 7)
						{
							sPagerNumber = (string)lFields[7];
						}

						//if( (sLName.Length == 0) && (sFName.Length == 0 && (sExt.Length == 0) )	// Ignore line, it was empty
						//if(sExt.Length == 0)	// Ignore line, it was malformed
						if ((sLName.Length == 0) && (sFName.Length == 0) && (sExt.Length == 0) && (sEmail.Length == 0) && (sUser.Length == 0) && (sDomain.Length == 0))
						{	// NOP
						}
						else
						{
							tmpUser = new SBConfigStor.Users.User();
							tmpUser.LName = sLName;
							tmpUser.FName = sFName;
							tmpUser.Ext = sExt;
							tmpUser.Email = sEmail;
							tmpUser.Username = sUser;
							tmpUser.Domain = sDomain;
							tmpUser.MobileNumber = sMobileNumber;
							tmpUser.PagerNumber = sPagerNumber;

							o_Users.Add(tmpUser);
						}
					} // if
				} // for
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + "SBConfigStore.CsvImportParser.Parse exception: " + exc.ToString());
			}

			return bRet;
		}

		#endregion

		private bool GetFields(string i_sLine, ArrayList o_lFields)
		{
			bool bRet = true;

			try
			{
				bool bDone;
				const char cSeparator = ',';
				int iPrevIndex = 0, iNextIndex = 0, iLen;
				string sTmp;

				iLen = i_sLine.Length;
				o_lFields.Clear();
				bDone = false;

				while (!bDone)
				{
					sTmp = "";
					iNextIndex = i_sLine.IndexOf(cSeparator, iPrevIndex);
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

					o_lFields.Add(sTmp);
				}

				for (int ii = o_lFields.Count; ii < 7; ii++)	// FIX constant!
				{
					o_lFields.Add("");
				}
			}
			catch
			{
				bRet = false;
			}

			return bRet;
		} // GetFields()
	}
}
