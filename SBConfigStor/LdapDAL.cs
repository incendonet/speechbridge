// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Npgsql;

namespace SBConfigStor
{
	public sealed class LdapDAL
	{
		public LdapDAL()
		{
		}

		public static Dictionary<string, string> GetSBtoADPropertyMapping()
		{
			Dictionary<string, string> SBtoADPropertyMapping = new Dictionary<string, string>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT sSBPropertyName, sADPropertyName FROM tblSBDirProperties SB, tblADDirProperties AD, tblSBtoADPropertyMapping WHERE SB.iSBPropertyId = tblSBtoADPropertyMapping.iSBPropertyId AND AD.iADPropertyId = tblSBtoADPropertyMapping.iADPropertyId";
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string sSBProperty = sqlReader["sSBPropertyName"].ToString();
							string sADProperty = sqlReader["sADPropertyName"].ToString();

							SBtoADPropertyMapping.Add(sSBProperty, sADProperty);
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.LdapDAL.GetSBtoADPropertyMapping exception: {1}", DateTime.Now, exc.ToString());
			}

			return SBtoADPropertyMapping;
		}

		public static bool SaveSBtoADPropertyMapping(Dictionary<string, string> i_SBtoADPropertyMapping)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				{
					sqlConn.Open();


					// Because of the uniqueness constraint put on the columns in tblSBtoADPropertyMapping we have to delete all the entries before we can "update" them.

					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						sqlCmd.CommandText = "DELETE FROM tblSBtoADPropertyMapping";

						sqlCmd.ExecuteNonQuery();
					}

					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						sqlCmd.CommandText = "INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES ((SELECT iSBPropertyId FROM tblSBDirProperties WHERE sSBPropertyName = @SBProperty), (SELECT iADPropertyId FROM tblADDirProperties WHERE sADPropertyName = @ADProperty))";

						IDbDataParameter sqlParamSBProperty;
						IDbDataParameter sqlParamADProperty;

						sqlParamSBProperty = sqlCmd.CreateParameter();
						sqlParamSBProperty.DbType = DbType.String;
						sqlParamSBProperty.ParameterName = "@SBProperty";
						sqlCmd.Parameters.Add(sqlParamSBProperty);

						sqlParamADProperty = sqlCmd.CreateParameter();
						sqlParamADProperty.DbType = DbType.String;
						sqlParamADProperty.ParameterName = "@ADProperty";
						sqlCmd.Parameters.Add(sqlParamADProperty);

						foreach (string sSBProperty in i_SBtoADPropertyMapping.Keys)
						{
							string sADProperty = i_SBtoADPropertyMapping[sSBProperty];

							sqlParamSBProperty.Value = sSBProperty;
							sqlParamADProperty.Value = sADProperty;

							sqlCmd.ExecuteNonQuery();
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;
				Console.Error.WriteLine("{0} SBConfigStore.LdapDAL.SaveSBtoADPropertyMapping exception: {1}", DateTime.Now, exc.ToString());
			}

			return bSuccess;
		}

		public static Dictionary<string, string> GetADtoSBPropertyMapping()
		{
			Dictionary<string, string> ADtoSBPropertyMapping = new Dictionary<string, string>();

			try
			{
				Dictionary<string, string> SBtoADPropertyMapping = GetSBtoADPropertyMapping();

				foreach (string sSBProperty in SBtoADPropertyMapping.Keys)
				{
					string sADProperty = SBtoADPropertyMapping[sSBProperty].ToLower();							// LDAP queries return the properties in all lowercase so we need to account for that so the directory lookup works.

					ADtoSBPropertyMapping.Add(sADProperty, sSBProperty);
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.LdapDAL.GetADtoSBPropertyMapping exception: {1}", DateTime.Now, exc.ToString());
			}

			return ADtoSBPropertyMapping;
		}

		public static List<string> GetAvailableADPropertiesForSpecifiedSBProperty(string i_sSBProperty)
		{
			List<string> availableADProperties = new List<string>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT sADPropertyName FROM tblSBDirProperties SB, tblADDirProperties AD, tblAvailableSBtoADPropertyMappings WHERE SB.sSBPropertyName = @SBProperty AND SB.iSBPropertyId = tblAvailableSBtoADPropertyMappings.iSBPropertyId AND AD.iADPropertyId = tblAvailableSBtoADPropertyMappings.iADPropertyId ORDER BY sADPropertyName";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@SBProperty";
					sqlParam.Value = i_sSBProperty;
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string sADPropertyName = sqlReader["sADPropertyName"].ToString();

							availableADProperties.Add(sADPropertyName);
						}
					}

					sqlConn.Close();
				}

			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.LdapDAL.GetAvailableADPropertiesForSpecifiedSBProperty exception: {1}", DateTime.Now, exc.ToString());
			}

			return availableADProperties;
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
