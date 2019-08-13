// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Npgsql;
using Incendonet.Utilities.LogClient;


namespace SBConfigStor
{
    public sealed class UserDirectoryDAL
    {
        private readonly ILegacyLogger m_Logger = null;

        public UserDirectoryDAL(ILegacyLogger i_Logger)
        {
            m_Logger = i_Logger;
        }

        public bool IsAKnownExtension(string i_sPhoneNumber, int i_iNumberOfDigitsInExtension)
        {
            bool bExtensionFound = false;

            try
            {
                using (IDbConnection sqlConn = GetDbConnection())
                using (IDbCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "SELECT COUNT(*) FROM tblDirectory WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = @Group) AND SUBSTR(sExt, LENGTH(sExt) - @NumberOfDigits) = @PhoneNumber";

                    IDbDataParameter sqlParam;

                    sqlParam = sqlCmd.CreateParameter();
                    sqlParam.DbType = DbType.String;
                    sqlParam.ParameterName = "@Group";
                    sqlParam.Value = Group.HIDDEN;
                    sqlCmd.Parameters.Add(sqlParam);

                    sqlParam = sqlCmd.CreateParameter();
                    sqlParam.DbType = DbType.Int32;
                    sqlParam.ParameterName = "@NumberOfDigits";
                    sqlParam.Value = i_iNumberOfDigitsInExtension - 1;
                    sqlCmd.Parameters.Add(sqlParam);

                    sqlParam = sqlCmd.CreateParameter();
                    sqlParam.DbType = DbType.String;
                    sqlParam.ParameterName = "@PhoneNumber";
                    sqlParam.Value = i_sPhoneNumber;
                    sqlCmd.Parameters.Add(sqlParam);

                    sqlConn.Open();

                    if (Convert.ToInt32(sqlCmd.ExecuteScalar()) > 0)
                    {
                        bExtensionFound = true;
                    }

                    sqlConn.Close();
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("SBConfigStore.UserDirectoryDAL.IsAKnownExtension({0}, {1}) exception: {2}", i_sPhoneNumber, i_iNumberOfDigitsInExtension, exc.ToString()));
            }

            return bExtensionFound;
        }

        public bool IsAKnownAlternateNumber(string i_sPhoneNumber)
        {
            bool bAlterateNumberFound = false;

            try
            {
                using (IDbConnection sqlConn = GetDbConnection())
                using (IDbCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = "SELECT COUNT(*) FROM tblUserParams WHERE CAST(uUserID AS int) NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = @Group) AND iParamType = @Type AND sParamValue = @PhoneNumber";

                    IDbDataParameter sqlParam;

                    sqlParam = sqlCmd.CreateParameter();
                    sqlParam.DbType = DbType.String;
                    sqlParam.ParameterName = "@Group";
                    sqlParam.Value = Group.HIDDEN;
                    sqlCmd.Parameters.Add(sqlParam);

                    sqlParam = sqlCmd.CreateParameter();
                    sqlParam.DbType = DbType.Int32;
                    sqlParam.ParameterName = "@Type";
                    sqlParam.Value = (int)UserPrefs.eParamType.AlternateNumber;
                    sqlCmd.Parameters.Add(sqlParam);

                    sqlParam = sqlCmd.CreateParameter();
                    sqlParam.DbType = DbType.String;
                    sqlParam.ParameterName = "@PhoneNumber";
                    sqlParam.Value = i_sPhoneNumber;
                    sqlCmd.Parameters.Add(sqlParam);

                    sqlConn.Open();

                    if (Convert.ToInt32(sqlCmd.ExecuteScalar()) > 0)
                    {
                        bAlterateNumberFound = true;
                    }

                    sqlConn.Close();
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("SBConfigStore.UserDirectoryDAL.IsAKnownAlternateNumber({0}) exception: {1}", i_sPhoneNumber, exc.ToString()));
            }

            return bAlterateNumberFound;
        }

        private void Log(Level i_Level, string i_sMessage)
        {
            if (m_Logger != null)
            {
                m_Logger.Log(i_Level, i_sMessage);
            }
            else
            {
                Console.Error.WriteLine(String.Format("{0} {1}: {2}", DateTime.Now, i_Level, i_sMessage));
            }
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
