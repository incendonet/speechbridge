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
	public sealed class DIDPromptDAL
	{
		private const string m_csDateFormat = "MM/dd/yyyy";
		private readonly DateTime dtDummyDate = DateTime.MinValue;			// Eventually we want to allow each holiday to have its own prompt file.  Until then we'll have a dummy date entry that holds the prompt info even if no holidays are specified.

		private string m_sDID = "";

		public DIDPromptDAL() : this(DIDMap.DEFAULT_DID)
		{
		}

		public DIDPromptDAL(string i_sDID)
		{
			SetDID(i_sDID);
		}

		public void SetDID(string i_sDID)
		{
			m_sDID = i_sDID;
		}

		public bool CreatePromptEntry()
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDToPromptMapping (sDID) VALUES (@DID)";

					IDbDataParameter sqlParam;
					
					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.CreatePromptEntry exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool CreateAfterHoursPromptEntry()
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDToAfterHoursPromptMapping (sDID) VALUES (@DID)";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.CreateAfterHoursPromptEntry exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		// Eventually we want to allow each holiday to have its own prompt file.  Until then we'll have a dummy date entry that holds the prompt info even if no holidays are specified.

		public bool CreateHolidaysPromptEntry()
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDToHolidaysPromptMapping (sDID, sDate) VALUES (@DID, @Date)";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@Date";
					sqlParam.Value = dtDummyDate.ToString(m_csDateFormat);
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.CreateHolidaysPromptEntry exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool CreatePromptSettingEntry()
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDPromptSetting (sDID) VALUES (@DID)";

					IDbDataParameter sqlParam;
					
					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.CreatePromptSettingEntry exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool GetPrompt(out string o_sPromptFileName, out string o_sPromptText)
		{
			bool bSuccess = false;

			o_sPromptFileName = "";
			o_sPromptText = "";

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT sPromptFile, sPromptText FROM tblDIDToPromptMapping WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							o_sPromptFileName = sqlReader["sPromptFile"].ToString();
							o_sPromptText = sqlReader["sPromptText"].ToString();
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetPrompt exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool GetAfterHoursPrompt(out string o_sPromptFileName, out string o_sPromptText)
		{
			bool bSuccess = false;

			o_sPromptFileName = "";
			o_sPromptText = "";

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT sPromptFile, sPromptText FROM tblDIDToAfterHoursPromptMapping WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							o_sPromptFileName = sqlReader["sPromptFile"].ToString();
							o_sPromptText = sqlReader["sPromptText"].ToString();
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetAfterHoursPrompt exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool GetHolidaysPrompt(out string o_sPromptFileName, out string o_sPromptText)
		{
			bool bSuccess = false;

			o_sPromptFileName = "";
			o_sPromptText = "";

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					// Eventually we want to allow each holiday to have its own prompt file.  Until then we'll have a dummy date entry that holds the prompt info even if no holidays are specified.

					string sCmd = "SELECT sPromptFile, sPromptText FROM tblDIDToHolidaysPromptMapping WHERE sDID = @DID AND sDate = @Date";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@Date";
					sqlParam.Value = dtDummyDate.ToString(m_csDateFormat);
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							o_sPromptFileName = sqlReader["sPromptFile"].ToString();
							o_sPromptText = sqlReader["sPromptText"].ToString();
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetHolidaysPrompt exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdatePromptFile(string i_sPromptFileName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDToPromptMapping SET sPromptFile = @PromptFile WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@PromptFile";
					sqlParam.Value = i_sPromptFileName;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter(); 
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdatePromptFile exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdateAfterHoursPromptFile(string i_sPromptFileName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDToAfterHoursPromptMapping SET sPromptFile = @PromptFile WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@PromptFile";
					sqlParam.Value = i_sPromptFileName;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter(); 
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdateAfterHoursPromptFile exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdateHolidaysPromptFile(string i_sPromptFileName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDToHolidaysPromptMapping SET sPromptFile = @PromptFile WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@PromptFile";
					sqlParam.Value = i_sPromptFileName;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdateHolidaysPromptFile exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdatePromptText(string i_sPromptText)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDToPromptMapping SET sPromptText = @PromptText WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@PromptText";
					sqlParam.Value = i_sPromptText;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter(); 
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdatePromptText exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdateAfterHoursPromptText(string i_sPromptText)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDToAfterHoursPromptMapping SET sPromptText = @PromptText WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@PromptText";
					sqlParam.Value = i_sPromptText;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter(); 
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdateAfterHoursPromptText exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdateHolidaysPromptText(string i_sPromptText)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDToHolidaysPromptMapping SET sPromptText = @PromptText WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@PromptText";
					sqlParam.Value = i_sPromptText;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdateHolidaysPromptText exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool GetPromptSetting(out bool o_bAfterHoursEnabled, out bool o_bHolidaysEnabled)
		{
			bool bSuccess = false;

			o_bAfterHoursEnabled = false;
			o_bHolidaysEnabled = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT bAfterHoursEnabled, bHolidaysEnabled FROM tblDIDPromptSetting WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							o_bAfterHoursEnabled = Convert.ToBoolean(sqlReader["bAfterHoursEnabled"]);
							o_bHolidaysEnabled = Convert.ToBoolean(sqlReader["bHolidaysEnabled"]);
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetPromptSetting exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdatePromptSetting(bool i_bAfterHoursEnabled, bool i_bHolidaysEnabled)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDPromptSetting SET bAfterHoursEnabled = @AfterHoursEnabled, bHolidaysEnabled = @HolidaysEnabled WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Boolean;
					sqlParam.ParameterName = "@AfterHoursEnabled";
					sqlParam.Value = i_bAfterHoursEnabled;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Boolean;
					sqlParam.ParameterName = "@HolidaysEnabled";
					sqlParam.Value = i_bHolidaysEnabled;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdatePromptSetting exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool CreateBusinessHoursEntry(List<BusinessHours> i_businessHoursCollection)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDBusinessHours (sDID, sWeekday, sStartTime, sEndTime) VALUES (@DID, @Weekday, @StartTime, @EndTime)";

					IDbDataParameter sqlParamStartTime;
					IDbDataParameter sqlParamEndTime;
					IDbDataParameter sqlParamDID;
					IDbDataParameter sqlParamWeekday;

					sqlParamStartTime = sqlCmd.CreateParameter();
					sqlParamStartTime.DbType = DbType.String;
					sqlParamStartTime.ParameterName = "@StartTime";
					sqlCmd.Parameters.Add(sqlParamStartTime);

					sqlParamEndTime = sqlCmd.CreateParameter();
					sqlParamEndTime.DbType = DbType.String;
					sqlParamEndTime.ParameterName = "@EndTime";
					sqlCmd.Parameters.Add(sqlParamEndTime);

					sqlParamDID = sqlCmd.CreateParameter();
					sqlParamDID.DbType = DbType.String;
					sqlParamDID.ParameterName = "@DID";
					sqlParamDID.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParamDID);

					sqlParamWeekday = sqlCmd.CreateParameter();
					sqlParamWeekday.DbType = DbType.String;
					sqlParamWeekday.ParameterName = "@Weekday";
					sqlCmd.Parameters.Add(sqlParamWeekday);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					foreach (BusinessHours businessHours in i_businessHoursCollection)
					{
						sqlParamStartTime.Value = businessHours.StartTime;
						sqlParamEndTime.Value = businessHours.EndTime;
						sqlParamWeekday.Value = businessHours.Weekday;

						sqlCmd.ExecuteNonQuery();
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.CreateBusinessHoursEntry exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool GetBusinessHours(out List<BusinessHours> o_BusinessHoursCollection)
		{
			bool bSuccess = false;

			o_BusinessHoursCollection = new List<BusinessHours>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT sWeekday, sStartTime, sEndTime FROM tblDIDBusinessHours WHERE sDID = @DID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					int iNumberOfRows = 0;

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							++iNumberOfRows;

							string sWeekday = sqlReader["sWeekday"].ToString();
							string sStartTime = sqlReader["sStartTime"].ToString();
							string sEndTime = sqlReader["sEndTime"].ToString();

							o_BusinessHoursCollection.Add(new BusinessHours(sWeekday, sStartTime, sEndTime));
						}
					}

					sqlConn.Close();

					if (0 == iNumberOfRows)
					{
						Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetBusinessHours couldn't find Business Hours in database for DID '{1}' - using default.", DateTime.Now.ToString(), m_sDID);

						o_BusinessHoursCollection = GetDefaultBusinessHours();
					}


					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetBusinessHours exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public List<BusinessHours> GetDefaultBusinessHours()
		{
			List<BusinessHours> businessHoursCollection = new List<BusinessHours>();

			businessHoursCollection.Add(new BusinessHours("Monday", BusinessHours.NINE_AM, BusinessHours.FIVE_PM));
			businessHoursCollection.Add(new BusinessHours("Tuesday", BusinessHours.NINE_AM, BusinessHours.FIVE_PM));
			businessHoursCollection.Add(new BusinessHours("Wednesday", BusinessHours.NINE_AM, BusinessHours.FIVE_PM));
			businessHoursCollection.Add(new BusinessHours("Thursday", BusinessHours.NINE_AM, BusinessHours.FIVE_PM));
			businessHoursCollection.Add(new BusinessHours("Friday", BusinessHours.NINE_AM, BusinessHours.FIVE_PM));
			businessHoursCollection.Add(new BusinessHours("Saturday", BusinessHours.CLOSED, BusinessHours.CLOSED));
			businessHoursCollection.Add(new BusinessHours("Sunday", BusinessHours.CLOSED, BusinessHours.CLOSED));

			return businessHoursCollection;
		}

		public bool UpdateBusinessHours(List<BusinessHours> i_businessHoursCollection)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblDIDBusinessHours SET sStartTime = @StartTime, sEndTime = @EndTime WHERE sDID = @DID AND sWeekday = @Weekday";

					IDbDataParameter sqlParamStartTime;
					IDbDataParameter sqlParamEndTime;
					IDbDataParameter sqlParamDID;
					IDbDataParameter sqlParamWeekday;

					sqlParamStartTime = sqlCmd.CreateParameter();
					sqlParamStartTime.DbType = DbType.String;
					sqlParamStartTime.ParameterName = "@StartTime";
					sqlCmd.Parameters.Add(sqlParamStartTime);

					sqlParamEndTime = sqlCmd.CreateParameter();
					sqlParamEndTime.DbType = DbType.String;
					sqlParamEndTime.ParameterName = "@EndTime";
					sqlCmd.Parameters.Add(sqlParamEndTime);

					sqlParamDID = sqlCmd.CreateParameter();
					sqlParamDID.DbType = DbType.String;
					sqlParamDID.ParameterName = "@DID";
					sqlParamDID.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParamDID);

					sqlParamWeekday = sqlCmd.CreateParameter();
					sqlParamWeekday.DbType = DbType.String;
					sqlParamWeekday.ParameterName = "@Weekday";
					sqlCmd.Parameters.Add(sqlParamWeekday);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					foreach (BusinessHours businessHours in i_businessHoursCollection)
					{
						sqlParamStartTime.Value = businessHours.StartTime;
						sqlParamEndTime.Value = businessHours.EndTime;
						sqlParamWeekday.Value = businessHours.Weekday;

						sqlCmd.ExecuteNonQuery();
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdateBusinessHours exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool GetHolidays(out List<DateTime> o_HolidaysCollection)
		{
			bool bSuccess = false;

			o_HolidaysCollection = new List<DateTime>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					// Eventually we want to allow each holiday to have its own prompt file.  Until then we'll have a dummy date entry that holds the prompt info even if no holidays are specified.

					string sCmd = "SELECT sDate FROM tblDIDToHolidaysPromptMapping WHERE sDID = @DID AND sDate <> @Date";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@Date";
					sqlParam.Value = dtDummyDate.ToString(m_csDateFormat);
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string sDate = sqlReader["sDate"].ToString();
							DateTime date;

							if (DateTime.TryParse(sDate, out date))
							{
								o_HolidaysCollection.Add(date);
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

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.GetHolidays exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		public bool UpdateHolidays(List<DateTime> i_holidaysCollection)
		{
			bool bSuccess = false;

			try
			{
				// Easiest way to do this is to delete all the holidays currently in the DB and then insert the new list.

				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					// Eventually we want to allow each holiday to have its own prompt file.  Until then we'll have a dummy date entry that holds the prompt info even if no holidays are specified.

					string sCmd = "DELETE FROM tblDIDToHolidaysPromptMapping WHERE sDID = @DID AND sDate <> @Date";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@DID";
					sqlParam.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@Date";
					sqlParam.Value = dtDummyDate.ToString(m_csDateFormat);
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}

				// Eventually we want to allow each holiday to have its own prompt file.  Until then we'll have a dummy date entry that holds the prompt info even if no holidays are specified.
				// However, to make migration easier we need to make sure that each holiday entry contains the current prompt file and text entry.

				string sPromptFile = "";
				string sPromptText = "";

				GetHolidaysPrompt(out sPromptFile, out sPromptText);


				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblDIDToHolidaysPromptMapping (sDID, sDate, sPromptFile, sPromptText) VALUES (@DID, @Date, @PromptFile, @PromptText)";

					IDbDataParameter sqlParamDID;
					IDbDataParameter sqlParamDate;
					IDbDataParameter sqlParamPromptFile;
					IDbDataParameter sqlParamPromptText;

					sqlParamDID = sqlCmd.CreateParameter();
					sqlParamDID.DbType = DbType.String;
					sqlParamDID.ParameterName = "@DID";
					sqlParamDID.Value = m_sDID;
					sqlCmd.Parameters.Add(sqlParamDID);

					sqlParamDate = sqlCmd.CreateParameter();
					sqlParamDate.DbType = DbType.String;
					sqlParamDate.ParameterName = "@Date";
					sqlCmd.Parameters.Add(sqlParamDate);

					sqlParamPromptFile = sqlCmd.CreateParameter();
					sqlParamPromptFile.DbType = DbType.String;
					sqlParamPromptFile.ParameterName = "@PromptFile";
					sqlParamPromptFile.Value = sPromptFile;
					sqlCmd.Parameters.Add(sqlParamPromptFile);

					sqlParamPromptText = sqlCmd.CreateParameter();
					sqlParamPromptText.DbType = DbType.String;
					sqlParamPromptText.ParameterName = "@PromptText";
					sqlParamPromptText.Value = sPromptText;
					sqlCmd.Parameters.Add(sqlParamPromptText);


					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					foreach (DateTime date in i_holidaysCollection)
					{
						sqlParamDate.Value = date.ToString(m_csDateFormat);

						sqlCmd.ExecuteNonQuery();
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.DIDPromptDAL.UpdateHolidays exception: {1}", DateTime.Now.ToString(), exc.ToString());
			}

			return bSuccess;
		}

		private IDbConnection GetDbConnection()
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
