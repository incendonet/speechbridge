// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Npgsql;


namespace SBConfigStor
{
	public class UserPrefs : CollectionBase
	{
		public const string m_csParamKeyColumn =	"uParamKey";
		public const string m_csUserIDColumn =		"uUserID";
		public const string m_csParamTypeColumn =	"iParamType";
		public const string m_csParamNameColumn =	"sParamName";
		public const string m_csParamValueColumn =	"sParamValue";

		private string		m_sSqlConn = "";

		public enum eParamType
		{
			Undefined = 0,
			GeneralPurpose,
			EmailLen,
			EmailAge,
			CalendarAge,
			Alias,
			TTSRate,				// Scale (1..10), 10 fastest, 0 not set
			AlternateNumber,
			DefaultWebpage,				// Default webpage to redirect to after login.  (Is this necessary, or can we just use "GeneralPurpose"?)
		};

		public static eParamType GetParamType(int i_iParamType)
		{
			eParamType	eRet = eParamType.Undefined;

			switch(i_iParamType)
			{
				case (int)eParamType.GeneralPurpose :
					eRet = eParamType.GeneralPurpose;
					break;
				case (int)eParamType.EmailLen :
					eRet = eParamType.EmailLen;
					break;
				case (int)eParamType.EmailAge :
					eRet = eParamType.EmailAge;
					break;
				case (int)eParamType.CalendarAge :
					eRet = eParamType.CalendarAge;
					break;
				case (int)eParamType.Alias :
					eRet = eParamType.Alias;
					break;
				case (int)eParamType.TTSRate :
					eRet = eParamType.TTSRate;
					break;
				case (int)eParamType.AlternateNumber :
					eRet = eParamType.AlternateNumber;
					break;
				case (int)eParamType.DefaultWebpage :
					eRet = eParamType.DefaultWebpage;
					break;
				default :
					eRet = eParamType.Undefined;
					break;
			}

			return(eRet);
		}

		public class UserPref
		{
			private	string		m_sParamKey = "";
			private	string		m_sUserid = "";
			private	eParamType	m_eParamType = 0;
			private	string		m_sParamName = "";
			private	string		m_sParamValue = "";

			public string ParamKey
			{
				get	{	return(m_sParamKey);	}
				set {	m_sParamKey = value;	}
			}
			public string Userid
			{
				get	{	return(m_sUserid);	}
				set {	m_sUserid = value;	}
			}
			public eParamType ParamType
			{
				get	{	return(m_eParamType);	}
				set {	m_eParamType = value;	}
			}
			public string ParamName
			{
				get	{	return(m_sParamName);	}
				set {	m_sParamName = value;	}
			}
			public string ParamValue
			{
				get	{	return(StringFilter.GetFilteredString(m_sParamValue));	}
				set {	m_sParamValue = StringFilter.GetFilteredString(value);	}
			}
		}

		public UserPrefs()
		{
		}

		public UserPrefs(string i_sSqlConn)
		{
			m_sSqlConn = i_sSqlConn;
		}

		public int Add(UserPref i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(UserPref i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(UserPref i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, UserPref i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(UserPref i_Elem)
		{
			List.Remove(i_Elem);
		}

		public UserPref this[int i_iIndex]
		{
			get {	return((UserPref)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		public bool LoadFromTableByUserid(string i_sUserid)
		{
			bool				bRet = true;
			string				sCmd = "";
			IDbConnection		sqlConn = null;
			IDbCommand			sqlCmd = null;
			IDataReader			sqlReader = null;
			UserPref			userprefTmp = null;

			try
			{
				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					if(m_sSqlConn.Length <= 0)
					{
						sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
					}
					else
					{
						sqlConn = new SqlConnection(m_sSqlConn);
					}
				}
				else if (RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					if(m_sSqlConn.Length <= 0)
					{
						sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
					}
					else
					{
						sqlConn = new NpgsqlConnection(m_sSqlConn);
					}
				}
				sCmd = "select uParamKey, uUserID, iParamType, sParamName, sParamValue from tblUserParams where uUserID='" + i_sUserid + "'";

				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = sCmd;
				sqlConn.Open();
				sqlReader = sqlCmd.ExecuteReader();

				try
				{
					while(sqlReader.Read())
					{
						userprefTmp = new UserPref();

						//sqlReader.GetString(0);	// Fast way, but not maintainable
						userprefTmp.ParamKey =		sqlReader[m_csParamKeyColumn].ToString();
						userprefTmp.Userid =		sqlReader[m_csUserIDColumn].ToString();
						userprefTmp.ParamType =		GetParamType((int)sqlReader[m_csParamTypeColumn]);
						userprefTmp.ParamName =		sqlReader[m_csParamNameColumn].ToString();
						userprefTmp.ParamValue =	sqlReader[m_csParamValueColumn].ToString();

						Add(userprefTmp);
					}
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.UserPrefs.LoadFromTableByUserid exception: " + exc.ToString());
					bRet = false;
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
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.UserPrefs.LoadFromTableByUserid exception: " + exc.ToString());
				// FIX - log error
				string sErr = exc.ToString();
				bRet = false;
			}

			return(bRet);
		} // LoadFromTableByUserid

		public bool LoadFromTableByTypeValue(eParamType i_eType, string i_sValue)
		{
			bool bRet = true;

			try
			{
				IDbConnection sqlConn = null;

				if (RunningSystem.RunningDatabase == Database.MsSql)
				{
					if(m_sSqlConn.Length <= 0)
					{
						sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
					}
					else
					{
						sqlConn = new SqlConnection(m_sSqlConn);
					}
				}
				else if (RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					if(m_sSqlConn.Length <= 0)
					{
						sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
					}
					else
					{
						sqlConn = new NpgsqlConnection(m_sSqlConn);
					}
				}

				using (sqlConn)
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT uParamKey, uUserID, sParamName FROM tblUserParams WHERE iParamType = @ParamType AND sParamValue = @ParamValue";

					if (RunningSystem.RunningDatabase == Database.MsSql)
					{
						sqlCmd.Parameters.Add(new SqlParameter("@ParamType", ((int)i_eType).ToString()));
						sqlCmd.Parameters.Add(new SqlParameter("@ParamValue", i_sValue));
					}
					else if (RunningSystem.RunningDatabase == Database.PostgreSql)
					{
						sqlCmd.Parameters.Add(new NpgsqlParameter("@ParamType", ((int)i_eType).ToString()));
						sqlCmd.Parameters.Add(new NpgsqlParameter("@ParamValue", i_sValue));
					}

					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							UserPref userprefTmp = new UserPref();

							userprefTmp.ParamKey = sqlReader[m_csParamKeyColumn].ToString();
							userprefTmp.Userid = sqlReader[m_csUserIDColumn].ToString();
							userprefTmp.ParamType = i_eType;
							userprefTmp.ParamName = sqlReader[m_csParamNameColumn].ToString();
							userprefTmp.ParamValue = i_sValue;

							Add(userprefTmp);
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(String.Format("{0} SBConfigStore.UserPrefs.LoadFromTableByTypeValue exception: {1}", DateTime.Now, exc.ToString()));
				// FIX - log error
				string sErr = exc.ToString();
				bRet = false;
			}

			return bRet;
		} // LoadFromTableByTypeValue

		public static string GetValueForUserByParamname(string i_sUserid, string i_sParamname)
		{
			string				sRet = "";
			string				sCmd = "";
			IDbConnection		sqlConn = null;
			IDbCommand			sqlCmd = null;
			object				oRes = null;

			try
			{
				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if (RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}
				sCmd = string.Format("select sParamValue from tblUserParams where uUserId='{0}' and sParamName='{1}'", i_sUserid, i_sParamname);

				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = sCmd;
				sqlConn.Open();

				try
				{
					oRes = sqlCmd.ExecuteScalar();
					sRet = (string)oRes;
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.UserPrefs.LoadFromTableByTypeValue exception: " + exc.ToString());
					sRet = "";
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
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.UserPrefs.LoadFromTableByTypeValue exception: " + exc.ToString());
				// FIX - log error
				string sErr = exc.ToString();
				sRet = "";
			}

			return(sRet);
		}	//GetValueForUserByParamname

		public static bool DeleteFromTableByUserid(string i_sUserid)
		{
			bool			bRet = true;
			string			sDelCmd = "";
			IDbConnection	scConn = null;
			IDbCommand		scCmd = null;

			sDelCmd = "DELETE FROM tblUserParams WHERE uUserID='" + i_sUserid + "'";

			if(RunningSystem.RunningDatabase == Database.MsSql)
			{
				scConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
			}
			else if(RunningSystem.RunningDatabase == Database.PostgreSql)
			{
				scConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
			}
			scCmd = scConn.CreateCommand();
			scCmd.CommandText = sDelCmd;

			try
			{
				scConn.Open();
				scCmd.ExecuteNonQuery();
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + " UserPrefs.DeleteFromTableByUserid Caught exception: {0}", exc.ToString());
			}
			finally
			{
				scCmd.Dispose();
				scCmd = null;
				scConn.Dispose();
				scConn = null;
			}

			return(bRet);
		} // DeleteFromTableByUserid

		public bool SaveToTable()
		{
			bool			bRet = true;
			string			sSql = "";
			IDbConnection	scConn = null;
			IDbCommand		scCmd = null;

			try
			{
				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					scConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					scConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}
				
				foreach(UserPref tmpPref in this)
				{
					sSql = "INSERT INTO tblUserParams (uUserid, iParamType, sParamName, sParamValue) VALUES ('" +
						tmpPref.Userid + "', '" + ((int)tmpPref.ParamType).ToString() + "', '" + tmpPref.ParamName + "', '" + tmpPref.ParamValue + "')";

					scCmd = scConn.CreateCommand();
					scCmd.CommandText = sSql;

					try
					{
						scConn.Open();
						scCmd.ExecuteNonQuery();
					}
					catch(Exception exc)
					{
						bRet = false;
						Console.Error.WriteLine(DateTime.Now.ToString() + " UserPrefs.SaveToTable Caught inner exception: {0}", exc.ToString());
					}
					finally
					{
						scConn.Close();
						scCmd.Dispose();
						scCmd = null;
					}
				} // foreach
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + " UserPrefs.SaveToTable Caught outer exception: {0}", exc.ToString());
			}
			finally
			{
				scConn.Dispose();
				scConn = null;
			}
			
			return(bRet);
		} // SaveToTable
	} // UserPrefs
}
