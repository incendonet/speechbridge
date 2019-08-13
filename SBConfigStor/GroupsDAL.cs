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
	public sealed class GroupsDAL
	{
		public GroupsDAL()
		{
		}


		public static bool GetGroupNames(out List<string> o_groupNames)
		{
			bool bSuccess = false;
			o_groupNames = new List<string>();

			try
			{
				o_groupNames.Add(Group.ALL);																	// Add virtual group "All" to top of list.

				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT sGroupName FROM tblGroups ORDER BY sGroupName";

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							o_groupNames.Add(sqlReader["sGroupName"].ToString());
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetGroupNames() exception: {1}", DateTime.Now, exc.ToString());
			}

			return bSuccess;
		}


		public static bool GetGroups(out List<Group> o_groups)
		{
			bool bSuccess = false;
			o_groups = new List<Group>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT uGroupID, sGroupName FROM tblGroups ORDER BY sGroupName";

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							Group group = new Group();

							group.ID = Convert.ToInt32(sqlReader["uGroupID"]);
							group.Name = sqlReader["sGroupName"].ToString();

							o_groups.Add(group);
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetGroups() exception: {1}", DateTime.Now, exc.ToString());
			}

			return bSuccess;
		}


		public static bool CreateGroup(string i_sGroupName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "INSERT INTO tblGroups (sGroupName) VALUES (@GroupName)";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@GroupName";
					sqlParam.Value = i_sGroupName;
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

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.CreateGroup('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName);
			}

			return bSuccess;
		}


		public static bool DeleteGroup(int i_iID)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "DELETE FROM tblGroups WHERE uGroupID = @GroupID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@GroupID";
					sqlParam.Value = i_iID;
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

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.DeleteGroup({2}) exception: {1}", DateTime.Now, exc.ToString(), i_iID);
			}

			return bSuccess;
		}


		public static bool RenameGroup(int i_iID, string i_sGroupName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "UPDATE tblGroups SET sGroupName = @GroupName WHERE uGroupID = @GroupID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@GroupName";
					sqlParam.Value = i_sGroupName;
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@GroupID";
					sqlParam.Value = i_iID;
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

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.RenameGroup({2}, '{3}') exception: {1}", DateTime.Now, exc.ToString(), i_iID, i_sGroupName);
			}

			return bSuccess;
		}


		public static int GetNumberOfEntriesInGroup(string i_sGroupName)
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

						sqlCmd.CommandText = String.Format("SELECT COUNT(*) FROM tblDirectory WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = '{0}')", Group.HIDDEN);
					}
					else
					{
						sqlCmd.CommandText = String.Format("SELECT COUNT(*) FROM tblDirectory WHERE uUserID IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = '{0}')", i_sGroupName);
					}

					sqlConn.Open();

					iNumberOfEntries = Convert.ToInt32(sqlCmd.ExecuteScalar());

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetNumberOfEntriesInGroup('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName);
			}

			return iNumberOfEntries;
		}


		public static bool GetEntriesInGroupPaged(string i_sGroupName, string i_sSortOrder, int i_iPageSize, int i_iStartIndex, out Users o_Users)
		{
			bool bSuccess = false;
			o_Users = new Users();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sSelectCmd;

					if (i_sGroupName == Group.ALL)
					{
						// "All" means NOT "Hidden".

						sSelectCmd = String.Format("SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupId = g.uGroupID) WHERE g.sGroupName = '{3}') ORDER BY {0} LIMIT {1} OFFSET {2}",
												   i_sSortOrder, i_iPageSize, i_iStartIndex, Group.HIDDEN);
					}
					else
					{
						sSelectCmd = String.Format("SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d WHERE uUserID IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupId = g.uGroupID) WHERE g.sGroupName = '{3}') ORDER BY {0} LIMIT {1} OFFSET {2}",
												   i_sSortOrder, i_iPageSize, i_iStartIndex, i_sGroupName);
					}

					sqlCmd.CommandText = sSelectCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							Users.User user = new Users.User();

							user.PopulateFromReader(sqlReader);

							o_Users.Add(user);
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetEntriesInGroupPaged('{2}', '{3}', {4}, {5}) exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName, i_sSortOrder, i_iPageSize, i_iStartIndex);
			}

			return bSuccess;
		}


		public static int GetNumberOfEntriesNotInGroup(string i_sGroupName)
		{
			int iNumberOfEntries = -1;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					if (i_sGroupName == Group.HIDDEN)
					{
						sqlCmd.CommandText = String.Format("SELECT COUNT(*) FROM tblDirectory WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = '{0}')", Group.HIDDEN);
					}
					else
					{
						// Not being in group "X" also means not being in "Hidden".

						sqlCmd.CommandText = String.Format("SELECT COUNT(*) FROM tblDirectory WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupID = g.uGroupID) WHERE g.sGroupName = '{0}' OR g.sGroupName = '{1}')", i_sGroupName, Group.HIDDEN);
					}

					sqlConn.Open();

					iNumberOfEntries = Convert.ToInt32(sqlCmd.ExecuteScalar());

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetNumberOfEntriesNotInGroup('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName);
			}

			return iNumberOfEntries;
		}


		public static bool GetEntriesNotInGroupPaged(string i_sGroupName, string i_sSortOrder, int i_iPageSize, int i_iStartIndex, out Users o_Users)
		{
			bool bSuccess = false;
			o_Users = new Users();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sSelectCmd;

					if (i_sGroupName == Group.HIDDEN)
					{
						sSelectCmd = String.Format("SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupId = g.uGroupID) WHERE g.sGroupName = '{3}') ORDER BY {0} LIMIT {1} OFFSET {2}",
												   i_sSortOrder, i_iPageSize, i_iStartIndex, Group.HIDDEN);
					}
					else
					{
						// Not being in group "X" also means not being in "Hidden".

						sSelectCmd = String.Format("SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupId = g.uGroupID) WHERE g.sGroupName = '{3}' OR g.sGroupName = '{4}') ORDER BY {0} LIMIT {1} OFFSET {2}",
												   i_sSortOrder, i_iPageSize, i_iStartIndex, i_sGroupName, Group.HIDDEN);
					}

					sqlCmd.CommandText = sSelectCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							Users.User user = new Users.User();

							user.PopulateFromReader(sqlReader);

							o_Users.Add(user);
						}
					}

					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetEntriesNotInGroupPaged('{2}', '{3}', {4}, {5}) exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName, i_sSortOrder, i_iPageSize, i_iStartIndex);
			}

			return bSuccess;
		}


		public static bool AddUserToGroup(string i_sUserId, string i_sGroupName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					StringBuilder sbCmd = new StringBuilder();

					if (i_sGroupName == Group.HIDDEN)
					{
						// A user added to the Hidden group CANNOT be a member of any other group.

						sbCmd.Append("DELETE FROM tblUsersInGroups WHERE uUserID = @UserID;");
					}

					sbCmd.Append("INSERT INTO tblUsersInGroups (uGroupID, uUserID) VALUES ((SELECT uGroupID FROM tblGroups WHERE sGroupName = @GroupName), @UserID)");

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@UserID";
					sqlParam.Value = Convert.ToInt32(i_sUserId);
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@GroupName";
					sqlParam.Value = i_sGroupName;
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sbCmd.ToString();
					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();

					bSuccess = true;
				}
			}
			catch (Exception exc)
			{
				bSuccess = false;

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.AddUserToGroup('{2}', '{3}') exception: {1}", DateTime.Now, exc.ToString(), i_sUserId, i_sGroupName);
			}

			return bSuccess;
		}


		public static bool RemoveUserFromGroup(string i_sUserId, string i_sGroupName)
		{
			bool bSuccess = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "DELETE FROM tblUsersInGroups WHERE uUserID = @UserID AND uGroupID = (SELECT uGroupID FROM tblGroups WHERE sGroupName = @GroupName)";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@UserID";
					sqlParam.Value = Convert.ToInt32(i_sUserId);
					sqlCmd.Parameters.Add(sqlParam);

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.String;
					sqlParam.ParameterName = "@GroupName";
					sqlParam.Value = i_sGroupName;
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

				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.RemoveUserFromGroup('{2}', '{3}') exception: {1}", DateTime.Now, exc.ToString(), i_sUserId, i_sGroupName);
			}

			return bSuccess;
		}


		public static Users GetUsersInGroup(string i_sGroupName)
		{
			Users users = new Users();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sSelectCmd;

					if (i_sGroupName == Group.ALL)
					{
						// "All" means NOT "Hidden".

						sSelectCmd = String.Format("SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d WHERE uUserID NOT IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupId = g.uGroupID) WHERE g.sGroupName = '{0}') ORDER BY sLName, sFName", Group.HIDDEN);
					}
					else
					{
						sSelectCmd = String.Format("SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d WHERE uUserID IN (SELECT uUserID FROM tblUsersInGroups AS ug INNER JOIN tblGroups AS g ON (ug.uGroupId = g.uGroupID) WHERE g.sGroupName = '{0}') ORDER BY sLName, sFName", i_sGroupName);
					}

					sqlCmd.CommandText = sSelectCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							Users.User user = new Users.User();

							user.PopulateFromReader(sqlReader);

							users.Add(user);
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetUsersInGroup('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sGroupName);
			}

			return users;
		}


		public static List<string> GetGroupsForUser(string i_sUserId)
		{
			List<string> groupsForUser = new List<string>();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "SELECT sGroupName FROM tblGroups AS g INNER JOIN tblUsersInGroups AS ug ON (g.uGroupID = ug.uGroupID) WHERE ug.uUserID = @UserID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@UserID";
					sqlParam.Value = Convert.ToInt32(i_sUserId);
					sqlCmd.Parameters.Add(sqlParam);

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							groupsForUser.Add(sqlReader["sGroupName"].ToString());
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.GroupsDAL.GetGroupsForUser('{2}') exception: {1}", DateTime.Now, exc.ToString(), i_sUserId);
			}

			return groupsForUser;
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
