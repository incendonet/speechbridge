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
	public class DialogDesignerDAL
	{
		public enum ErrorCode
		{
			Success,
			Unknown,
			UnknownSql,
			UniqueViolation,
		};

		private const string m_csMenuIDColumn = "iMenuID";
		private const string m_csMenuNameColumn = "sMenuName";
		private const string m_csLanguageCode = "sLanguageCode";
		private const string m_csIncludeColumn = "sInclude";
		private const string m_csGrammarUrlColumn = "sGrammarUrl";
		private const string m_csVariablesColumn = "sVariables";
		private const string m_csMinConfScoreColumn = "iMinConfScore";
		private const string m_csHighConfScoreColumn = "iHighConfScore";
		private const string m_csDtmfCanBeSpokenColumn = "bDtmfCanBeSpoken";
        private const string m_csConfirmationEnabled = "bConfirmationEnabled";
		private const string m_csEnabledColumn = "bEnabled";

		private const string m_csCommandIDColumn = "iCommandID";
		private const string m_csCommandNameColumn = "sCommandName";
		private const string m_csCommandTypeColumn = "iCommandType";
		private const string m_csOperationTypeColumn = "iOperationType";
		private const string m_csCommandOptionColumn = "sCommandOption";
		private const string m_csConfirmationTextColumn = "sConfirmationText";
		private const string m_csConfirmationWavUrlColumn = "sConfirmationWavUrl";
		private const string m_csResponseColumn = "sResponse";

		private const string m_csLanguageNameColumn = "sLanguageName";

		private const string m_csDtmfKeyColumn = "sDtmfKey";
		private const string m_csSpokenEquivalentColumn = "sSpokenEquivalent";


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public DialogDesignerDAL()
		{
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// FIX:  This is modified from sbdbutils.RunSqlCmd().  Put it somewhere reuseable!
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool RunSqlNonQuery(string i_sSql)
		{
			bool			bRet = true;
			IDbConnection	sqlConn = null;
			IDbCommand		sqlCmd = null;

			try
			{
				sqlConn = GetDbConnection();
				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = i_sSql;

				sqlConn.Open();
				sqlCmd.ExecuteNonQuery();
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.RunSqlNonQuery('{0}') Caught exception: {1}", i_sSql, exc.ToString());
				bRet = false;
				// FIX - log error
			}
			finally
			{
				sqlCmd.Dispose();
				sqlCmd = null;
				sqlConn.Close();
				sqlConn = null;
			}

			return(bRet);
		} // RunSqlNonQuery

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public Menus GetEnabledMenus()
		{
			return(GetMenus(true));
		} // GetEnabledMenus

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public Menus GetMenus(bool i_bEnabledOnly)
		{
			Menus menus = new Menus();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						string sCmd = BasicMenuSelectString();

						if (i_bEnabledOnly)
						{
							sCmd = String.Format("{0} WHERE bEnabled = '1'", sCmd);
						}

						sCmd = String.Format("{0} ORDER BY sMenuName", sCmd);

						sqlCmd.CommandText = sCmd;
						sqlConn.Open();

						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							try
							{
								while (sqlReader.Read())
								{
                                    menus.Add(GetMenuFromSqlReader(sqlReader));
								}
							}
							catch (Exception exc)
							{
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetMenus exception1: " + exc.ToString());
							}
						} // using (IDataReader)

						sqlConn.Close();
					} // using (IDbCommand)
				} // using (IDbConnection)
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetMenus exception2: " + exc.ToString());
			}

			return menus;
		} // GetMenus

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode GetMenu(int i_iMenuid, out Menus.Menu o_oMenu)
		{
			return GetMenuUsingSqlQuery(String.Format("{0} WHERE iMenuid = '{1}'", BasicMenuSelectString(), i_iMenuid), out o_oMenu);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode GetMenu(string i_sMenuName, out Menus.Menu o_oMenu)
		{
			return GetMenuUsingSqlQuery(String.Format("{0} WHERE sMenuName = '{1}'", BasicMenuSelectString(), i_sMenuName), out o_oMenu);
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public Commands GetCommandsForMenu(Menus.Menu i_Menu)
		{
			Commands commands = new Commands();

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						string sCmd = String.Format("SELECT c.{0}, c.{1}, c.{2}, c.{3}, c.{4}, c.{5}, c.{6}, c.{7} FROM tblCommands c, tblMenuCommandsMap mc WHERE c.iCommandID = mc.iCommandID AND mc.iMenuID = {8}", 
							m_csCommandIDColumn, 
							m_csCommandNameColumn, 
							m_csCommandTypeColumn, 
							m_csOperationTypeColumn,
							m_csCommandOptionColumn, 
							m_csConfirmationTextColumn, 
							m_csConfirmationWavUrlColumn, 
							m_csResponseColumn,
							i_Menu.MenuId);

						sqlCmd.CommandText = sCmd;
						sqlConn.Open();

						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							try
							{
								while (sqlReader.Read())
								{
									Menus.Menu menu = new Menus.Menu();
									Commands.Command command = new Commands.Command();

									command.CommandId = Convert.ToInt32(sqlReader[m_csCommandIDColumn]);
									command.CommandName = sqlReader[m_csCommandNameColumn].ToString();
									command.CommandType = (Commands.eCommandType)sqlReader[m_csCommandTypeColumn];
									command.OperationType = (Commands.eOperationType)sqlReader[m_csOperationTypeColumn];
									command.CommandOption = sqlReader[m_csCommandOptionColumn].ToString();
									command.ConfirmationText = sqlReader[m_csConfirmationTextColumn].ToString();
									command.ConfirmationWavUrl = sqlReader[m_csConfirmationWavUrlColumn].ToString();
									command.Response = sqlReader[m_csResponseColumn].ToString();

									commands.Add(command);
								}
							}
							catch (Exception exc)
							{
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetCommandsForMenu exception2: " + exc.ToString());
							}
						} // using (IDataReader)

						sqlConn.Close();
					} // using (IDbCommand)
				} // using (IDbConnection)
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetCommandsForMenu exception1: " + exc.ToString());
			}

			return commands;
		} // GetCommandsForMenu


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private Variables ExtractVariables(string i_sVariablesString)
		{
			Variables variables = new Variables();

			//$$$ LP - Replace this approach with RegEx since the code below has problem if the variable value contains either a ';' or '='.

			foreach (string sVariableString in i_sVariablesString.Split(new char[] { ';' }))
			{
				if (String.Empty != sVariableString)
				{
					string[] variableComponents = sVariableString.Split(new char[] { '=' });
					variables.Add(variableComponents[0], variableComponents[1].Trim(new char[] { '"' }));
				}
			}

			return variables;
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private string CreateVariablesString(Variables i_variables)
		{
			StringBuilder sbVariableString = new StringBuilder();

			foreach (Variables.Variable variable in i_variables)
			{
				sbVariableString.AppendFormat("{0}=\"{1}\";", variable.Name, variable.Value);
			}

			return sbVariableString.ToString();
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode AddMenu(string i_sMenuName, Languages.Language i_language)
		{
			ErrorCode eRet = ErrorCode.Success;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					IDbDataParameter param;

                    sqlCmd.CommandText = "INSERT INTO tblMenus (sMenuName, sLanguageCode, bConfirmationEnabled) VALUES (@MenuName, @LanguageCode, '1')";

					param = sqlCmd.CreateParameter();
					param.DbType = DbType.String;
					param.ParameterName = "@MenuName";
					param.Value = i_sMenuName;
					sqlCmd.Parameters.Add(param);

					param = sqlCmd.CreateParameter();
					param.DbType = DbType.String;
					param.ParameterName = "@LanguageCode";
					param.Value = i_language.Code;
					sqlCmd.Parameters.Add(param);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}
			}
			catch( System.Data.SqlClient.SqlException excSql)
			{
				if (excSql.Number == 2627)
				{
					eRet = ErrorCode.UniqueViolation;
					Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.AddMenu caught exception, UNIQUE violation:" + excSql.ToString());
				}
				else
				{
					eRet = ErrorCode.UnknownSql;
					Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.AddMenu caught exception:" + excSql.ToString());
				}
			}
			catch (Npgsql.NpgsqlException excSql)
			{
				if (excSql.ErrorCode == 23505)
				{
					eRet = ErrorCode.UniqueViolation;
					Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.AddMenu caught exception, UNIQUE violation:" + excSql.ToString());
				}
				else
				{
					eRet = ErrorCode.UnknownSql;
					Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.AddMenu caught exception:" + excSql.ToString());
				}
			}
			catch (Exception exci)
			{
				eRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.AddMenu caught exception:" + exci.ToString());
			}

			return eRet;
		} // AddMenu

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode UpdateMenu(Menus.Menu i_oMenu, Commands i_oCommands)
		{
			ErrorCode			ecRet = ErrorCode.Success;
			StringBuilder		sbSql = null;
			string				sSql = "";
			bool				bRes = true;
			int					ii = 0, iNumCmds = 0;
			Database			dbType = Database.unknown;

			try
			{
//Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu IN.");
				dbType = RunningSystem.RunningDatabase;
				if(dbType == Database.unknown)
				{
					ecRet = ErrorCode.Unknown;
					Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu Invalid DB type.");
				}
				else
				{
					// FIX!!!  Do this in a stored procedure, this approach is way too inefficient

					// Delete previous commands and mapping
					if(dbType == Database.MsSql)
					{
						throw new Exception("Code for MS SQL not yet implemented.");
					}
					else if(dbType == Database.PostgreSql)
					{
						sSql = string.Format("DELETE FROM tblCommands USING tblMenuCommandsMap WHERE tblCommands.iCommandID = tblMenuCommandsMap.iCommandID AND tblMenuCommandsMap.iMenuID = {0}", i_oMenu.MenuId);
						bRes = RunSqlNonQuery(sSql);
					}

					if(!bRes)
					{
						ecRet = ErrorCode.UnknownSql;
						Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu DB error deleting previous commands.");
					}
					else
					{
						// Update Menu Info - currently user can change Language, whether DTMF commands can be spoken and if Confirmation is enabled for spoken commands.

						bRes = UpdateMenuInfo(i_oMenu);

						if(!bRes)
						{
							ecRet = ErrorCode.UnknownSql;
							Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu Error updating Menu table.");
						}
						else
						{
							sbSql = new StringBuilder();

							// Save commands, hold on to IDs
							iNumCmds = i_oCommands.Count;
//Console.Error.WriteLine(DateTime.Now.ToString() + " iNumCmds='{0}'.", iNumCmds);
							for(ii = 0; ii < iNumCmds; ii++)
							{
								i_oCommands[ii].CommandId = InsertCommand(i_oCommands[ii]);

								if (-1 == i_oCommands[ii].CommandId)
								{
									ecRet = ErrorCode.Unknown;
									Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu Inserting Command ii={0} failed.", ii);
								}
								else
								{
									// Map command to menu
									if(dbType == Database.MsSql)
									{
										throw new Exception("Code for MS SQL not yet implemented.");
									}
									else if(dbType == Database.PostgreSql)
									{
										sSql = string.Format("INSERT INTO tblMenuCommandsMap(iMenuID, iCommandID) VALUES({0}, {1})", i_oMenu.MenuId, i_oCommands[ii].CommandId);
									}
									bRes = RunSqlNonQuery(sSql);
									if(!bRes)
									{
										ecRet = ErrorCode.Unknown;
										Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu Error updating MenuCommandsMap ({0}, {1}), ii={2}.", i_oMenu.MenuId, i_oCommands[ii].CommandId, ii);
									}
									else
									{
//Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu Inserted MenuCommandsMap({0}, {1}), ii={2}.", i_oMenu.MenuId, i_oCommands[ii].CommandId, ii);
									}
								} // else
							} // for
						} // else
					} // else
				} // else
			}
			catch(Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenu caught exception:" + exc.ToString());
			}

			return(ecRet);
		} // UpdateMenu

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode DeleteMenu(int i_iMenuId)
		{
			ErrorCode ecRet = ErrorCode.Success;

			try
			{
				Database dbType = RunningSystem.RunningDatabase;
				if (dbType == Database.unknown)
				{
					ecRet = ErrorCode.Unknown;
					Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.DeleteMenu Invalid DB type.");
				}
				else
				{
					// FIX!!!  Do this in a stored procedure, this approach is way too inefficient

					StringBuilder sbSql = new StringBuilder();
					sbSql.AppendFormat("DELETE FROM tblCommands USING tblMenuCommandsMap WHERE tblCommands.iCommandID = tblMenuCommandsMap.iCommandID AND tblMenuCommandsMap.iMenuID = {0};", i_iMenuId);
					sbSql.AppendFormat("DELETE FROM tblMenus WHERE iMenuID = {0};", i_iMenuId);

					bool bRes = RunSqlNonQuery(sbSql.ToString());

					if(!bRes)
					{
						ecRet = ErrorCode.UnknownSql;
						Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.DeleteMenu DB error deleting menu.");
					}
				}
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.DeleteMenu caught exception:" + exc.ToString());
			}

			return ecRet;
		} // DeleteMenu

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode GetMenuNames(bool i_bEnabledOnly, out List<string> o_asMenuNames)
		{
			ErrorCode ecRet = ErrorCode.Success;
			o_asMenuNames = null;

			try
			{
				o_asMenuNames = new List<string>();

				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					string sCmd = "";

					if (i_bEnabledOnly)
					{
						sCmd = "SELECT sMenuName FROM tblMenus WHERE bEnabled = '1' ORDER BY sMenuName";
					}
					else
					{
						sCmd = "SELECT sMenuName FROM tblMenus ORDER BY sMenuName";
					}

					sqlCmd.CommandText = sCmd;
					sqlConn.Open();

					try
					{
						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							while (sqlReader.Read())
							{
								string sName = sqlReader.GetString(0);
								if (!String.IsNullOrEmpty(sName))
								{
									o_asMenuNames.Add(sName);
								}
							}
						}
					}
					catch (Exception exc)
					{
						ecRet = ErrorCode.UnknownSql;
						Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetMenuNames exception1: " + exc.ToString());
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetMenuNames exception2:" + exc.ToString());
			}

			return ecRet;
		} // GetMenuNames

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode GetInstalledLanguages(out Languages o_oLanguages)
		{
			ErrorCode ecRet = ErrorCode.Success;

			o_oLanguages = null;

			try
			{
				o_oLanguages = new Languages();

				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						string sCmd = String.Format("SELECT {0}, {1} FROM tblLanguages",
													m_csLanguageNameColumn,
													m_csLanguageCode);

						sqlCmd.CommandText = sCmd;
						sqlConn.Open();

						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							try
							{
								while (sqlReader.Read())
								{
									o_oLanguages.Add(sqlReader[m_csLanguageNameColumn].ToString(), sqlReader[m_csLanguageCode].ToString());
								}
							}
							catch (Exception exc)
							{
								ecRet = ErrorCode.UnknownSql;
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetInstalledLanguages exception1: " + exc.ToString());
							}
						} // using (IDataReader)

						sqlConn.Close();
					} // using (IDbCommand)
				} // using (IDbConnection)
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetInstalledLanguages exception2: " + exc.ToString());
			}

			return ecRet;
		} // GetInstalledLanguages


		// For the time being assume that the first language installed is the "default" system language.
		// NOTE: This relies on the assumption that an unordered SQL query will return the information in the order in which it was entered into the database.

		public Languages.Language GetDefaultLanguage()
		{
			Languages installedLanguages;
			Languages.Language defaultLanguage;

			ErrorCode errorCode = GetInstalledLanguages(out installedLanguages);

			if ((errorCode == ErrorCode.Success) &&
				(installedLanguages.Count > 0))
			{
				defaultLanguage = installedLanguages[0];
			}
			else
			{
				defaultLanguage = new Languages.Language("", "");

				Console.Error.WriteLine("{0} No languages installed - cannot determine default language.", DateTime.Now);
			}

			return defaultLanguage;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ErrorCode GetDtmfToSpokenMapping(string i_sLanguageCode, out DtmfKeyToSpokenEquivalentMappings o_dtmfToSpokenMapping)
		{
			ErrorCode ecRet = ErrorCode.Success;

			o_dtmfToSpokenMapping = null;

			try
			{
				o_dtmfToSpokenMapping = new DtmfKeyToSpokenEquivalentMappings();

				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						string sCmd = String.Format("SELECT {0}, {1} FROM tblDtmfKeyToSpokenEquivalent WHERE {2} = '{3}'",
													m_csDtmfKeyColumn,
													m_csSpokenEquivalentColumn,
													m_csLanguageCode,
													i_sLanguageCode);

						sqlCmd.CommandText = sCmd;
						sqlConn.Open();

						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							try
							{
								while (sqlReader.Read())
								{
									o_dtmfToSpokenMapping.Add(new DtmfKeyToSpokenEquivalentMapping(sqlReader[m_csDtmfKeyColumn].ToString(), sqlReader[m_csSpokenEquivalentColumn].ToString()));
								}
							}
							catch (Exception exc)
							{
								ecRet = ErrorCode.UnknownSql;
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetDtmfToSpokenMapping exception1: " + exc.ToString());
							}
						} // using (IDataReader)

						sqlConn.Close();
					} // using (IDbCommand)
				} // using (IDbConnection)
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetDtmfToSpokenMapping exception2: " + exc.ToString());
			}

			return ecRet;
		} // GetDtmfToSpokenMapping


		public ErrorCode DisableMenu(string i_sMenuName)
		{
			return SetMenuEnabledState(i_sMenuName, false);
		}


		public ErrorCode EnableMenu(string i_sMenuName)
		{
			return SetMenuEnabledState(i_sMenuName, true);
		}

		
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private bool UpdateMenuInfo(Menus.Menu i_oMenu)
		{
			bool bRes = false;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						IDbDataParameter sqlParam;

                        sqlCmd.CommandText = "UPDATE tblMenus SET sLanguageCode = @LanguageCode, bDtmfCanBeSpoken = @DtmfCanBeSpoken, bConfirmationEnabled = @ConfirmationEnabled WHERE iMenuID = @MenuId";

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.String;
						sqlParam.ParameterName = "@LanguageCode";
						sqlParam.Value = i_oMenu.LanguageCode;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.Boolean; 
						sqlParam.ParameterName = "@DtmfCanBeSpoken";
						sqlParam.Value = i_oMenu.DtmfCanBeSpoken;
						sqlCmd.Parameters.Add(sqlParam);

                        sqlParam = sqlCmd.CreateParameter();
                        sqlParam.DbType = DbType.Boolean;
                        sqlParam.ParameterName = "@ConfirmationEnabled";
                        sqlParam.Value = i_oMenu.ConfirmationEnabled;
                        sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.Int32;
						sqlParam.ParameterName = "@MenuId";
						sqlParam.Value = i_oMenu.MenuId;
						sqlCmd.Parameters.Add(sqlParam);

						sqlConn.Open();
						sqlCmd.ExecuteNonQuery();
						sqlConn.Close();

						bRes = true;
					}
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.UpdateMenuInfo caught exception: " + exc.ToString());
				bRes = false;
			}

			return bRes;
		} // UpdateMenuInfo

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private int InsertCommand(Commands.Command i_command)
		{
			int iCommandId = -1;

			try
			{
				StringBuilder sbSql = new StringBuilder();

				sbSql.Append("INSERT INTO tblCommands(sCommandName, iCommandType, iOperationType, sCommandOption, sConfirmationText, sConfirmationWavUrl, sResponse) ");
				sbSql.Append("VALUES(@CommandName, @CommandType, @OperationType, @CommandOption, @ConfirmationText, @ConfirmationWavUrl, @Response); ");

				Database databaseType = RunningSystem.RunningDatabase;

				switch (databaseType)
				{
					case Database.MsSql:
						throw new Exception("Code for MS SQL not yet implemented.");

					case Database.PostgreSql:

						// According to http://www.programmingado.net/a-117/Insert-data-and-retrieve-serial-autonumber-id.aspx, this
						// query is session safe, no need for a transaction.

						sbSql.Append("SELECT currval('tblCommands_iCommandID_seq')");
						break;

					case Database.unknown:
					default:
						throw new Exception(String.Format("Unknown Database Type ({0}).", databaseType));
				}

				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						IDbDataParameter sqlParam;

						sqlCmd.CommandText = sbSql.ToString();

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.String;
						sqlParam.ParameterName = "@CommandName";
						sqlParam.Value = i_command.CommandName;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.Int32;
						sqlParam.ParameterName = "@CommandType";
						sqlParam.Value = (int)i_command.CommandType;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.Int32;
						sqlParam.ParameterName = "@OperationType";
						sqlParam.Value = (int)i_command.OperationType;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.String;
						sqlParam.ParameterName = "@CommandOption";
						sqlParam.Value = i_command.CommandOption;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.String;
						sqlParam.ParameterName = "@ConfirmationText";
						sqlParam.Value = i_command.ConfirmationText;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.String;
						sqlParam.ParameterName = "@ConfirmationWavUrl";
						sqlParam.Value = i_command.ConfirmationWavUrl;
						sqlCmd.Parameters.Add(sqlParam);

						sqlParam = sqlCmd.CreateParameter();
						sqlParam.DbType = DbType.String;
						sqlParam.ParameterName = "@Response";
						sqlParam.Value = i_command.Response;
						sqlCmd.Parameters.Add(sqlParam);

						sqlConn.Open();
						iCommandId = (int)((long)sqlCmd.ExecuteScalar());
						sqlConn.Close();
					}
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " DialogDesignerDAL.InsertCommand caught exception: " + exc.ToString());
				iCommandId = -1;
			}

			return iCommandId;
		} // InsertCommand

        private string BasicMenuSelectString()
        {
            string sCmd = String.Format("SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} FROM tblMenus",
                            m_csMenuIDColumn,
                            m_csMenuNameColumn,
                            m_csLanguageCode,
                            m_csIncludeColumn,
                            m_csGrammarUrlColumn,
                            m_csVariablesColumn,
                            m_csMinConfScoreColumn,
                            m_csHighConfScoreColumn,
                            m_csDtmfCanBeSpokenColumn,
                            m_csConfirmationEnabled,
                            m_csEnabledColumn);

            return sCmd;
        }

        private Menus.Menu GetMenuFromSqlReader(IDataReader i_sqlReader)
        {
            Menus.Menu menu = new Menus.Menu();

            menu.MenuId = Convert.ToInt32(i_sqlReader[m_csMenuIDColumn]);
            menu.MenuName = i_sqlReader[m_csMenuNameColumn].ToString();
            menu.LanguageCode = i_sqlReader[m_csLanguageCode].ToString();
            menu.Include = i_sqlReader[m_csIncludeColumn].ToString();
            menu.GrammarUrl = i_sqlReader[m_csGrammarUrlColumn].ToString();
            menu.Variables = ExtractVariables(i_sqlReader[m_csVariablesColumn].ToString());
            menu.MinConfScore = Convert.ToInt32(i_sqlReader[m_csMinConfScoreColumn]);
            menu.HighConfScore = Convert.ToInt32(i_sqlReader[m_csHighConfScoreColumn]);
            menu.DtmfCanBeSpoken = Convert.ToBoolean(i_sqlReader[m_csDtmfCanBeSpokenColumn]);
            menu.ConfirmationEnabled = Convert.ToBoolean(i_sqlReader[m_csConfirmationEnabled]);
            menu.Enabled = Convert.ToBoolean(i_sqlReader[m_csEnabledColumn]);

            return menu;
        }

		private ErrorCode GetMenuUsingSqlQuery(string i_sQuery, out Menus.Menu o_oMenu)
		{
			ErrorCode ecRet = ErrorCode.Success;

			o_oMenu = null;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = i_sQuery;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						try
						{
							while (sqlReader.Read())
							{
								o_oMenu = GetMenuFromSqlReader(sqlReader);
							}
						}
						catch (Exception exc)
						{
							ecRet = ErrorCode.UnknownSql;
							Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetMenuUsingSqlQuery exception1: " + exc.ToString());
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.GetMenuUsingSqlQuery exception2: " + exc.ToString());
			}

			return ecRet;
		}  // GetMenuUsingSqlQuery

		public ErrorCode SetMenuEnabledState(string i_sMenuName, bool i_bEnabled)
		{
			ErrorCode ecRet = ErrorCode.Success;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = String.Format("UPDATE tblMenus SET bEnabled = '{0}' WHERE sMenuName = '{1}'", i_bEnabled ? '1' : '0', i_sMenuName);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.DialogDesignerDAL.SetMenuEnabledState exception: " + exc.ToString());
			}

			return ecRet;
		}

		public ErrorCode AddCommand(Menus.Menu i_menu, Commands.Command i_command)
		{
			ErrorCode ecRet = ErrorCode.Success;

			try
			{
				i_command.CommandId = InsertCommand(i_command);

				if (-1 == i_command.CommandId)
				{
					ecRet = ErrorCode.Unknown;
					Console.Error.WriteLine("{0} SBConfigStore.DialogDesignerDAL.AddCommand Inserting Command ('{1}') failed.", DateTime.Now, i_command.CommandOption);
				}
				else
				{
					// Map command to menu.

					string sSql = String.Format("INSERT INTO tblMenuCommandsMap(iMenuID, iCommandID) VALUES({0}, {1})", i_menu.MenuId, i_command.CommandId);
					bool bRes = RunSqlNonQuery(sSql);

					if (!bRes)
					{
						ecRet = ErrorCode.Unknown;
						Console.Error.WriteLine("{0} SBConfigStore.DialogDesignerDAL.AddCommand Error updating MenuCommandsMap ({1}, {2}) ('{3}').", DateTime.Now, i_menu.MenuId, i_command.CommandId, i_command.CommandOption);
					}
				}
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine("{0} SBConfigStore.DialogDesignerDAL.AddCommand exception: {1}", DateTime.Now, exc.ToString());
			}

			return ecRet;
		}

		public ErrorCode DeleteCommand(int i_commandId)
		{
			ErrorCode ecRet = ErrorCode.Success;
			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					IDbDataParameter sqlParam;

					sqlCmd.CommandText = "DELETE FROM tblCommands WHERE iCommandID = @CommandId";

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@CommandId";
					sqlParam.Value = i_commandId;
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				ecRet = ErrorCode.Unknown;
				Console.Error.WriteLine("{0} SBConfigStore.DialogDesignerDAL.DeleteCommand exception: {1}", DateTime.Now, exc.ToString());
			}

			return ecRet;
		}
	} // DialogDesignerDAL
}
