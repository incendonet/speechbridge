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
	public sealed class TtsLanguageCodeMappingDAL : ITtsLanguageCodeMappingDAL
	{
		public TtsLanguageCodeMappingDAL()
		{
		}

		public Dictionary<string, string> GetMapping()
		{
			Dictionary<string, string> languageCodeMapping = new Dictionary<string, string>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT sRequestedLanguageCode, sMappedLanguageCode FROM tblTTSLanguageCodeMapping";
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string sRequestedLanguageCode = sqlReader["sRequestedLanguageCode"].ToString();
							string sMappedLanguageCode = sqlReader["sMappedLanguageCode"].ToString();

							languageCodeMapping.Add(sRequestedLanguageCode, sMappedLanguageCode);
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.TtsLanguageCodeMappingDAL.GetMapping exception: {1}", DateTime.Now, exc.ToString());
			}

			return languageCodeMapping;
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
