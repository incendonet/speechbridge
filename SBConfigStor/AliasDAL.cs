// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Npgsql;

namespace SBConfigStor
{
	public sealed class AliasDAL
	{
		public AliasDAL()
		{
		}

		public static bool DoesAliasExist(string i_sAlias)
		{
			bool bAliasExists = false;

			UserPrefs upTmp = new UserPrefs();
			upTmp.LoadFromTableByTypeValue(UserPrefs.eParamType.Alias, i_sAlias);

			if (upTmp.Count > 0)
			{
				bAliasExists = true;
			}

			return bAliasExists;
		}

		public static bool GetAliases(string i_sUserId, out List<Alias> o_AliasCollection)
		{
			bool bSuccess = false;
			o_AliasCollection = new List<Alias>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT uParamKey, sParamValue FROM tblUserParams WHERE uUserID = @UserId AND iParamType = @ParamType";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;					// The column name is uUserID but it actually is defined as a string.
					sqlParam.ParameterName = "@UserId";
					sqlParam.Value = i_sUserId;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@ParamType";
					sqlParam.Value = UserPrefs.eParamType.Alias;
					sqlCmd.Parameters.Add(sqlParam);
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							int iKey = Convert.ToInt32(sqlReader["uParamKey"]);
							string sValue = sqlReader["sParamValue"].ToString();

							o_AliasCollection.Add(new Alias(iKey, sValue));
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;
				Console.Error.WriteLine("{0} SBConfigStore.AliasDAL.GetAliases('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sUserId);
			}

			return bSuccess;
		}
		

		public static bool AddAlias(string i_sUserId, string i_sAlias)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "INSERT INTO tblUserParams (uUserID, iParamType, sParamValue) VALUES (@UserId, @ParamType, @ParamValue)";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;					// The column name is uUserID but it actually is defined as a string.
					sqlParam.ParameterName = "@UserId";
					sqlParam.Value = i_sUserId;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@ParamType";
					sqlParam.Value = UserPrefs.eParamType.Alias;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@ParamValue";
					sqlParam.Value = i_sAlias;
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}

				if (UpdateUserAliases(i_sUserId))
				{
					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;
				Console.Error.WriteLine("{0} SBConfigStore.AliasDAL.AddAlias('{2}', '{3}') exception: {1}", DateTime.Now, exc.ToString(), i_sUserId, i_sAlias);
			}

			return bSuccess;
		}

		public static bool UpdateAlias(int i_iParamKey, string i_sAlias, string i_sUserId)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "UPDATE tblUserParams SET sParamValue = @ParamValue WHERE uParamKey = @ParamKey";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@ParamValue";
					sqlParam.Value = i_sAlias;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@ParamKey";
					sqlParam.Value = i_iParamKey;
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}

				if (UpdateUserAliases(i_sUserId))
				{
					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;
				Console.Error.WriteLine("{0} SBConfigStore.AliasDAL.UpdateAlias({2}, '{3}') exception: {1}", DateTime.Now, exc.ToString(), i_iParamKey, i_sAlias);
			}

			return bSuccess;
		}

		public static bool DeleteAlias(int i_iParamKey, string i_sUserId)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "DELETE FROM tblUserParams WHERE uParamKey = @ParamKey";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@ParamKey";
					sqlParam.Value = i_iParamKey;
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}

				if (UpdateUserAliases(i_sUserId))
				{
					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;
				Console.Error.WriteLine("{0} SBConfigStore.AliasDAL.DeleteAlias({2}) exception: {1}", DateTime.Now, exc.ToString(), i_iParamKey);
			}

			return bSuccess;
		}

		public static int GetNumberOfAliasesInGroup(string i_sGroupName)
		{
			int iNumberOfEntries = -1;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					if (i_sGroupName == Group.ALL)
					{
						// "All" means NOT "Hidden".

						sqlCmd.CommandText = String.Format("SELECT COUNT(*) FROM tblUserParams WHERE CAST(uUserID AS int) NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = '{0}') AND iParamType = {1}", Group.HIDDEN, (int)UserPrefs.eParamType.Alias);
					}
					else
					{
						sqlCmd.CommandText = String.Format("SELECT COUNT(*) FROM tblUserParams WHERE CAST(uUserID AS int) IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = '{0}') AND iParamType = {1}", i_sGroupName, (int)UserPrefs.eParamType.Alias);
					}

					sqlConn.Open();

					iNumberOfEntries = Convert.ToInt32(sqlCmd.ExecuteScalar());

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.AliasDAL.GetNumberOfAliasesInGroup('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName);
			}

			return iNumberOfEntries;

		}

		private static bool UpdateUserAliases(string i_sUserId)
		{
			bool bSuccess = false;

			List<Alias> aliases = null;

			if (GetAliases(i_sUserId, out aliases))
			{
				StringBuilder sbAliases = new StringBuilder();

				foreach (Alias alias in aliases)
				{
					sbAliases.AppendFormat("{0};", alias.Value);
				}

				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "UPDATE tblDirectory SET sAltPronunciations = @AltPronunciations WHERE uUserID = @UserId";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@AltPronunciations";
					sqlParam.Value = sbAliases.ToString();
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@UserId";
					sqlParam.Value = Convert.ToInt32(i_sUserId);
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();

					bSuccess = true;
				}
			}

			return bSuccess;
		}

		private static IDbConnection GetDbConnection()
		{
			IDbConnection sqlConn = null;

			if (RunningSystem.RunningDatabase == Database.MsSql)
			{
				sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
			}
			else if (RunningSystem.RunningDatabase == Database.PostgreSql)
			{
				sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
			}

			return sqlConn;
		}
	}
}
