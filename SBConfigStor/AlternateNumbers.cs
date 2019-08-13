// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Npgsql;

namespace SBConfigStor
{
	/// <summary>
	/// Summary description for AlternateNumbers.
	/// </summary>
	public class AlternateNumbers
	{
		private const string m_csMobileParamName = "Mobile";
		private const string m_csPagerParamName = "Pager";

		private string m_sUserid = "";
		private string m_sMobileNumber = "";
		private string m_sPagerNumber = "";

		private bool m_bValidMobileNumber = true;
		private bool m_bValidPagerNumber = true;

		[Flags]
		public enum eStatus
		{
			Success       = 0,
			UnknownError  = 1,
			DBError       = 2,
			InvalidMobile = 4,
			InvalidPager  = 8,
		}

		public AlternateNumbers(string i_sUserid)
		{
			m_sUserid = i_sUserid;
		}

		public string MobileNumber
		{
			get { return m_sMobileNumber; }
			set { m_sMobileNumber = ValidateInput(value, out m_bValidMobileNumber); }
		}

		public string PagerNumber
		{
			get { return m_sPagerNumber; }
			set { m_sPagerNumber = ValidateInput(value, out m_bValidPagerNumber); }
		}

		public bool HasMobileNumber
		{
			get { return m_sMobileNumber != ""; }
		}

		public bool HasPagerNumber
		{
			get { return m_sPagerNumber != ""; }
		}

		public eStatus Retrieve()
		{
			eStatus status = eStatus.UnknownError;

			try
			{
				string sMobileNumber = "";
				string sPagerNumber = "";

				bool bSuccess = GetAlternateNumbersFromDB(out sMobileNumber, out sPagerNumber);

				if (!bSuccess)
				{
					status = eStatus.DBError;
				}
				else
				{
					m_sMobileNumber = sMobileNumber;
					m_sPagerNumber = sPagerNumber;
					status = eStatus.Success;
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.Retrieve exception: " + exc.ToString());
			}

			return status;
		}

		public eStatus Save()
		{
			eStatus status = eStatus.UnknownError;

			try
			{
				// Only bother to try and save the numbers if at least one of them is valid.

				if (m_bValidMobileNumber || m_bValidPagerNumber)
				{
					string sMobileNumberInDB = "";
					string sPagerNumberInDB = "";

					bool bSuccess = GetAlternateNumbersFromDB(out sMobileNumberInDB, out sPagerNumberInDB);

					if (!bSuccess)
					{
						status = eStatus.DBError;
					}
					else
					{
						status = eStatus.Success;

						if (m_bValidMobileNumber)
						{
							bSuccess &= ProcessNumber(m_csMobileParamName, m_sMobileNumber, sMobileNumberInDB);
						}
						else
						{
							status = eStatus.InvalidMobile;
						}

						if (m_bValidPagerNumber)
						{
							bSuccess &= ProcessNumber(m_csPagerParamName, m_sPagerNumber, sPagerNumberInDB);
						}
						else
						{
							status = eStatus.InvalidPager;
						}
					}

					if (!bSuccess)
					{
						status = eStatus.DBError;
					}
				}
				else
				{
					status = eStatus.InvalidMobile | eStatus.InvalidPager;
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.Save exception: " + exc.ToString());
			}

			return status;
		}

		private bool GetAlternateNumbersFromDB(out string o_sMobileNumber, out string o_sPagerNumber)
		{
			bool bSuccess = false;

			o_sMobileNumber = "";
			o_sPagerNumber = "";

			try
			{
				IDbConnection	sqlConn = null;
				IDbCommand		sqlCmd = null;
				IDataReader		sqlReader = null;

				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}

				string sCmd = String.Format("SELECT sParamName, sParamValue FROM tblUserParams where uUserID='{0}' AND iParamType={1}", m_sUserid, (int)UserPrefs.eParamType.AlternateNumber);

				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = sCmd;

				try
				{
					sqlConn.Open();
					sqlReader = sqlCmd.ExecuteReader();

					while(sqlReader.Read())
					{
						string sParamName = sqlReader["sParamName"].ToString();
						string sParamValue = sqlReader["sParamValue"].ToString();

						switch (sParamName)
						{
							case m_csMobileParamName:
								o_sMobileNumber = sParamValue;
								break;

							case m_csPagerParamName:
								o_sPagerNumber = sParamValue;
								break;

							default:
								Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.GetAlternateNumbersFromDB - Unexpected sParamName: " + (sParamName == null ? "null" : sParamName));
								break;
						}
					}

					bSuccess = true;
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.GetAlternateNumbersFromDB exception 2: " + exc.ToString());
				}
				finally
				{
					sqlReader.Close();
					sqlReader = null;
					sqlCmd.Dispose();
					sqlCmd = null;
					sqlConn.Close();
					sqlConn = null;
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.GetAlternateNumbersFromDB exception 1: " + exc.ToString());
			}

			return bSuccess;
		} // GetAlternateNumbersFromDB

		private bool ProcessNumber(string i_sNumberType, string i_sNewNumber, string i_sNumberInDB)
		{
			bool bSuccess = true;

			// Figure out what needs to be done if the number has changed.

			if (i_sNewNumber != i_sNumberInDB)
			{
				if (i_sNewNumber == "")
				{
					bSuccess = DeleteNumber(i_sNumberType);
				}
				else if (i_sNumberInDB == "")
				{
					bSuccess = InsertNumber(i_sNumberType, i_sNewNumber);
				}
				else
				{
					bSuccess = UpdateNumber(i_sNumberType, i_sNewNumber);
				}
			}

			return bSuccess;
		}

		private bool InsertNumber(string i_sParamName, string i_sParamValue)
		{
			bool bSuccess = false;

			try
			{
				string sCmd = String.Format("INSERT INTO tblUserParams (uUserID, iParamType, sParamName, sParamValue) VALUES ('{0}', {1}, '{2}', '{3}')", m_sUserid, (int)UserPrefs.eParamType.AlternateNumber, i_sParamName, i_sParamValue);

				bSuccess = ExecuteSqlCommand(sCmd);
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.InsertNumber exception: " + exc.ToString());
			}

			return bSuccess;
		}

		private bool UpdateNumber(string i_sParamName, string i_sParamValue)
		{
			bool bSuccess = false;

			try
			{
				string sCmd = String.Format("UPDATE tblUserParams SET sParamValue = '{0}' WHERE uUserID = '{1}' AND iParamType = {2} AND sParamName = '{3}'", i_sParamValue, m_sUserid, (int)UserPrefs.eParamType.AlternateNumber, i_sParamName);

				bSuccess = ExecuteSqlCommand(sCmd);
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.UpdateNumber exception: " + exc.ToString());
			}

			return bSuccess;
		}

		private bool DeleteNumber(string i_sParamName)
		{
			bool bSuccess = false;

			try
			{
				string sCmd = String.Format("DELETE FROM tblUserParams WHERE uUserID = '{0}' AND iParamType = {1} AND sParamName = '{2}'", m_sUserid, (int)UserPrefs.eParamType.AlternateNumber, i_sParamName);

				bSuccess = ExecuteSqlCommand(sCmd);
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.DeleteNumber exception: " + exc.ToString());
			}

			return bSuccess;
		}

		private bool ExecuteSqlCommand(string i_sSqlCmd)
		{
			bool bSuccess = false;

			try
			{
				IDbConnection	sqlConn = null;
				IDbCommand		sqlCmd = null;

				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}

				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = i_sSqlCmd;

				try
				{
					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					bSuccess = true;
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.ExecuteSqlCommand exception 2: " + exc.ToString());
				}
				finally
				{
					sqlCmd.Dispose();
					sqlCmd = null;
					sqlConn.Close();
					sqlConn = null;
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " AlternateNumbers.ExecuteSqlCommand exception 1: " + exc.ToString());
			}

			return bSuccess;
		} // ExecuteSqlCommand

		private string ValidateInput(string i_sNumber, out bool i_bIsValid)
		{
			string sNumber = "";

			sNumber = DigitsOnly(i_sNumber);

			// An empty string is acceptable as this simply means that the user has deleted that alternate number.
			// If we have 1 or more digits then we need to verify that this is a "valid" number.  Currently only
			// numbers that match the US numbering format are accepted.

			if (0 == sNumber.Length)
			{
				i_bIsValid = true;
			}
			else
			{
				i_bIsValid = IsValidNumberFormat(sNumber);
			}

			return sNumber;
		}

		private string DigitsOnly(string i_sNumber)
		{
			string sTmp = "";

			for(int i = 0; i < i_sNumber.Length; ++i)
			{
				if(char.IsDigit(i_sNumber[i]))
				{
					sTmp += i_sNumber[i];
				}
			}

			return sTmp;
		}


		// Allow for point codes and extra digits for dialing plans and international numbers.

		private bool IsValidNumberFormat(string i_sNumber)
		{
			bool bIsValidUSNumberFormat = false;

			switch (i_sNumber.Length)
			{
				case 3:
				case 4:
				case 5:
				case 7:
				case 8:
				case 10:
				case 11:
				case 12:
				case 15:
				case 16:
				case 17:
					bIsValidUSNumberFormat = true;
					break;

				default:
					bIsValidUSNumberFormat = false;
					break;
			}

			return bIsValidUSNumberFormat;
		}
	}
}
