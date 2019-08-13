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
	public sealed class DIDMappingDAL
	{
		public DIDMappingDAL()
		{
		}

		public static bool GetDIDMappings(out List<DIDMapping> o_DIDMappingCollection)
		{
			bool bSuccess = false;

			o_DIDMappingCollection = new List<DIDMapping>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT sDID, sVoicexmlUrl FROM tblDIDMap ORDER BY sDID";

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string sDID = sqlReader["sDID"].ToString();
							string sVoiceXml = sqlReader["sVoicexmlUrl"].ToString();

							if (sDID == DIDMap.DEFAULT_DID)												
							{
								// Ensure that DEFAULT is always listed first.

								o_DIDMappingCollection.Insert(0, new DIDMapping(sDID, sVoiceXml));
							}
							else
							{
								o_DIDMappingCollection.Add(new DIDMapping(sDID, sVoiceXml));
							}
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDMappingDAL.GetDIDMappings exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public static int GetNumberOfDIDMappings()
		{
			int iNumberOfDIDMappings = 0;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT COUNT(*) FROM tblDIDMap";

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					iNumberOfDIDMappings = Convert.ToInt32(sqlCmd.ExecuteScalar());

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.DIDMappingDAL.GetNumberOfDIDMappings exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return iNumberOfDIDMappings;
		}

		public static bool IsMappingDefined(string i_sDID)
		{
			bool bMappingDefined = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT COUNT(*) FROM tblDIDMap WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = i_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					if (Convert.ToInt32(sqlCmd.ExecuteScalar()) > 0)
					{
						bMappingDefined = true;
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.DIDMappingDAL.IsMappingDefined exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bMappingDefined;
		}

		public static bool CreateMappingForDID(string i_sDID, string i_sVoiceXml)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDMap (sDID, sVoicexmlUrl) VALUES (@DID, @VoiceXml)";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = i_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@VoiceXml";
					sqlParam.Value = i_sVoiceXml;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();

					//$$$ LP - ToDo - should we trap constraint violation exceptions here?
					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDMappingDAL.CreateMappingForDID exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public static bool UpdateMappingForDID(string i_sDID, string i_sVoiceXml)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDMap SET sVoicexmlUrl = @VoiceXml WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@VoiceXml";
					sqlParam.Value = i_sVoiceXml;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = i_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDMappingDAL.UpdateMappingForDID exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public static bool DeleteMappingForDID(string i_sDID)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "DELETE FROM tblDIDMap WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = i_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDMappingDAL.DeleteMappingForDID exception: {1}", DateTime.Now.ToString(), exc.ToString());
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
