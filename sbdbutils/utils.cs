// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

using Npgsql;

using SBConfigStor;

namespace sbdbutils
{
    class utils
	{
		private		const	string				g_sArgHelp =						"--help";
		private		const	string				g_sArgResetAdmin =					"--reset-admin";
		private		const	string				g_sArgRunSqlscriptCommands =		"--run-sqlscript-commands";
		private		const	string				g_sArgRunSqlscriptQuery =			"--run-sqlscript-query";
		private		const	string				g_sArgRunSqlstringCommand =			"--run-sqlstring-command";
		private		const	string				g_sArgRunSqlstringQuery =			"--run-sqlstring-query";
		private		const	string				g_sArgAddUser =						"--add-user";
		private		const	string				g_sArgStandalone =					"--standalone";
		private		const	string				g_sArgCluster =						"--cluster";
		private		const	string				g_sArgGetSwVer =					"--get-swver";
		private		const	string				g_sArgGetDbVer =					"--get-dbver";
		private		const	string				g_sArgGetLangs =					"--get-langs";
		private		const	string				g_sArgGenerateVxml =				"--generate-vxml";
		private		const	string				g_sArgImportDirectory =				"--import";
		private		const	string				g_sArgMergeDirectory =				"--merge";
		private		const	string				g_sArgWriteConfigsSip =				"--write-configs-sip";
        private     const   string              g_sArgGenKey =                      "--gen-key";
		private		const	string				g_sArgSetAdminPassword =			"--set-admin";

        private const	string				g_sAdminUsername =	"admin";

        private static string g_sHelp = Environment.NewLine + "SpeechBridge Database Utility" + Environment.NewLine +
                                                "  Options are:" + Environment.NewLine +
                                                "  --add-user                   Adds a new user." + Environment.NewLine +
                                                "                                   Usage:  --add-user username password [defaultwebpage]" + Environment.NewLine +
                                                "  --cluster                    Configures SB to run as part of a load-balancing configuration" + Environment.NewLine +
                                                "                                   Usage:  --cluster VirtualIP MasterIP SlaveIP" + Environment.NewLine +
                                                "  --gen-key                    Generate a unique key to be used for encryption by SimpleAES" + Environment.NewLine +
                                                "  --generate-vxml              Generate VXML files for the existing templates" + Environment.NewLine +
                                                "  --get-dbver                  Gets the database schema version for the (case sensitive) named component." + Environment.NewLine +
                                                "                                   Usage:  --get-dbver ComponentName" + Environment.NewLine +
                                                "  --get-langs                  Prints to stdout a list of the language codes for the installed languages." + Environment.NewLine +
                                                "                                   Usage:  --get-langs" + Environment.NewLine +
                                                "  --get-swver                  Gets the software version for the (case sensitive) named component." + Environment.NewLine +
                                                "                                   Usage:  --get-swver ComponentName" + Environment.NewLine +
                                                "  --help                       Displays this message" + Environment.NewLine +
                                                "  --import                     Import a user directory file" + Environment.NewLine +
                                                "                                   IMPORTANT: This will delete ALL existing directory entries (except for admin)." + Environment.NewLine +
                                                "                                   Usage:  --import type filename" + Environment.NewLine +
                                                "                                           Allowed values for type are csv, allworx, shoretel." + Environment.NewLine +
                                                "  --merge                      Merge a user directory file" + Environment.NewLine +
                                                "                                   Usage:  --merge type filename" + Environment.NewLine +
                                                "                                           Allowed values for type are csv, allworx, shoretel." + Environment.NewLine +
                                                "  --reset-admin                Resets the admin password" + Environment.NewLine +
                                                "  --run-sqlscript-commands     Runs a SQL script at the file named in the following argument, containing commands (queries will not write to stdout)" + Environment.NewLine +
                                                "  --run-sqlscript-query        Runs a SQL script at the file named in the following argument, containing a query, and prints the result to stdout" + Environment.NewLine +
                                                "  --run-sqlstring-command      Runs the subsequent argument as a SQL command string  (queries will not write to stdout)" + Environment.NewLine +
                                                "  --run-sqlstring-query        Runs the subsequent argument as a SQL query string, and prints the result to stdout" + Environment.NewLine +
                                                "  --set-admin                  Set the admin password" + Environment.NewLine +
                                                "                                   Usage:  --set-admin OldPassword NewPassword" + Environment.NewLine +
                                                "  --standalone                 Configures SB to run as a standalone system" + Environment.NewLine +
                                                "  --write-configs-sip          Uses the DB settings to generate the SIP config files" + Environment.NewLine;


		private static string g_sConfigPath = "/opt/speechbridge/config";

        private static int EXIT_CODE_SUCCESS = 0;
        private static int EXIT_CODE_UNKNOWN_ERROR = 1;
        private static int EXIT_CODE_MISSING_ARGUMENTS = 2;
        private static int EXIT_CODE_UNKNOWN_COMMAND = 3;
        private static int EXIT_CODE_ILLEGAL_ARGUMENT = 4;
        private static int EXIT_CODE_IMPORT_ERROR = 5;
        private static int EXIT_CODE_IMPORT_INCOMPLETE = 6;


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            int					iExitCode = EXIT_CODE_SUCCESS;
			StringBuilder		sbTmp = null;

			try
			{
				if(args.Length > 0)
				{
					switch(args[0].ToLower())
					{
						case g_sArgResetAdmin :
							ResetAdmin();
							break;
						case g_sArgRunSqlscriptCommands :
							if(args.Length != 2)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine(g_sHelp);
							}
							else
							{
								RunScript(args[1]);
							}
							break;
						case g_sArgRunSqlscriptQuery :
							if(args.Length != 2)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine(g_sHelp);
							}
							else
							{
								Console.Out.WriteLine(RunQueryScript(args[1]));
							}
							break;
						case g_sArgRunSqlstringCommand :
							// Piece together the remaining arguments for the sql string
							sbTmp = new StringBuilder();
							for (int ii = 1; ii < (args.Length - 1); ii++)
							{
								sbTmp.Append(args[ii]);
								sbTmp.Append(' ');
							}
							sbTmp.Append(args[args.Length - 1]);
							Console.Out.WriteLine(RunSqlCmd(sbTmp.ToString().Trim("\"".ToCharArray())));
							break;
						case g_sArgRunSqlstringQuery :
							// Piece together the remaining arguments for the sql string
							sbTmp = new StringBuilder();
							for (int ii = 1; ii < (args.Length - 1); ii++)
							{
								sbTmp.Append(args[ii]);
								sbTmp.Append(' ');
							}
							sbTmp.Append(args[args.Length - 1]);
							Console.Out.WriteLine(RunSqlQuery(sbTmp.ToString().Trim("\"".ToCharArray())));
							break;
						case g_sArgAddUser :
							if(args.Length == 3)
							{
								AddUser(args[1], args[2], "");
							}
							else if(args.Length == 4)
							{
								AddUser(args[1], args[2], args[3]);
							}
							else
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --add-user Username Password [DefaultWebpage]");
							}
							break;
						case g_sArgCluster:
							if (4 != args.Length)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --cluster VirtualIP MasterIP SlaveIP");
							}
							else
							{
								GenerateClusterConfig(args[2], args[3], args[1]);
							}
							break;
						case g_sArgStandalone:
							GenerateStandaloneConfig();
							break;
						case g_sArgGetSwVer:
							if (2 != args.Length)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --get-swver ComponentName");
							}
							else
							{
								GetLatestSoftwareVersionByComponent(args[1]);
							}
							break;
						case g_sArgGetDbVer:
							if (2 != args.Length)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --get-dbver ComponentName");
							}
							else
							{
								GetLatestDatabaseVersionByComponent(args[1]);
							}
							break;
						case g_sArgGetLangs:
							if (1 != args.Length)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --get-langs");
							}
							else
							{
								GetLanguageCodes();
							}
							break;
						case g_sArgGenerateVxml:
							GenerateVxml();
							break;
						case g_sArgImportDirectory:
							if (3 != args.Length)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --import type filename");
							}
							else
							{
								iExitCode = ImportDirectory(args[1], args[2], false);
							}
							break;
						case g_sArgMergeDirectory:
							if (3 != args.Length)
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --merge type filename");
							}
							else
							{
								iExitCode = ImportDirectory(args[1], args[2], true);
							}
							break;
						case g_sArgWriteConfigsSip :
							g_sConfigPath = ConfigurationManager.AppSettings["DefaultConfigPath"];
							if (!UpdateSipConfigFiles())
							{
								Console.Error.WriteLine("ERROR, SIP configuration files not written.");
							}
							else
							{
								Console.WriteLine("SIP configuration files written.");
							}
							break;
                        case g_sArgGenKey:
                            Console.Write(GenerateEncryptionKey());
                            break;
                        case g_sArgSetAdminPassword:
							if(args.Length == 3)
							{
								iExitCode = SetAdminPassword(args[1], args[2]);
							}
							else
							{
                                iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
                                Console.Error.WriteLine("ERROR, usage:  sbdbutil --set-admin OldPassword NewPassword");
							}
                            break;
						case g_sArgHelp:
						default :
                            if (args[0].ToLower() != g_sArgHelp)
                            {
                                iExitCode = EXIT_CODE_UNKNOWN_COMMAND;
                            }

							Console.WriteLine(g_sHelp);
							break;
					}
				}
				else
				{
                    iExitCode = EXIT_CODE_MISSING_ARGUMENTS;
					Console.WriteLine(g_sHelp);
				}
			}
			catch(Exception exc)
			{
                iExitCode = EXIT_CODE_UNKNOWN_ERROR;
				Console.Error.WriteLine("ERROR: Caught exception: {0}", exc.ToString());
			}

            System.Environment.Exit(iExitCode);
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

		protected static bool ResetAdmin()
		{
			bool bRet = true;

			try
			{
				SBConfigStor.Directory dTmp = new SBConfigStor.Directory();
				if(dTmp == null)
				{
					Console.Error.WriteLine("ERROR: Couldn't create Directory object!");
				}
				else
				{
					bool bRes = true;
					string sUserid = "";

					bool bFound = SBConfigStor.Directory.GetUseridByUsername(g_sAdminUsername, out sUserid);
					if(!bFound)
					{	
						// Add admin user before continuing.

						Console.Error.WriteLine("WARNING: Admin user not found, adding now.");

						SBConfigStor.Users.User uTmp = new SBConfigStor.Users.User();
						uTmp.Username = g_sAdminUsername;

						SBConfigStor.Directory.AddUser(uTmp);

						bFound = SBConfigStor.Directory.GetUseridByUsername(g_sAdminUsername, out sUserid);
						if (!bFound)
						{
							bRes = false;
							Console.Error.WriteLine("ERROR: Couldn't find admin user that was just added to the DB!");
						}
					}

					// Set the password
					if(bRes)
					{
						bRes = dTmp.SetPassByUserid(sUserid, g_sAdminUsername.ToUpper(), g_sAdminUsername.ToLower());
						if(!bRes)
						{
							Console.Error.WriteLine("ERROR:  Couldn't set the password.");
						}
						else
						{
							Console.WriteLine("Admin password was reset.");
						}
					}
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR: Caught exception: {0}", exc.ToString());
			}

			return(bRet);
		} // ResetAdmin

        protected static int SetAdminPassword(string i_sOldPassword, string i_sNewPassword)
        {
            int iExitCode = EXIT_CODE_UNKNOWN_ERROR;

            try
            {
                SBConfigStor.Directory directory = new SBConfigStor.Directory();

                if (directory == null)
                {
                    Console.Error.WriteLine("ERROR: Couldn't create Directory object!");
                }
                else
                {
                    if (directory.Authenticate(g_sAdminUsername, i_sOldPassword))
                    {
                        string sUserid = "";

                        if (SBConfigStor.Directory.GetUseridByUsername(g_sAdminUsername, out sUserid))
                        {
                            bool bRes = directory.SetPassByUserid(sUserid, i_sNewPassword.ToUpper(), i_sNewPassword);
                            if (!bRes)
                            {
                                Console.Error.WriteLine("ERROR: Couldn't set the password.");
                            }
                            else
                            {
                                iExitCode = EXIT_CODE_SUCCESS;
                                Console.WriteLine("Admin password was set to '{0}'.", i_sNewPassword);
                            }
                        }
                        else
                        {
                            // This should not be possible, as we have already authenticated the admin user.

                            Console.Error.WriteLine("ERROR: Admin user not found.");
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("ERROR: Old password incorrect.");
                    }
                }
            }
            catch (Exception exc)
            {
                iExitCode = EXIT_CODE_UNKNOWN_ERROR;
                Console.Error.WriteLine("ERROR: Caught exception: {0}", exc.ToString());
            }

            return iExitCode;
        }

		protected static bool AddUser(string i_sUsername, string i_sPassword, string i_sDefaultWebpage)
		{
			bool						bRet = true;
			StringBuilder				sbSql = null;

			try
			{
				SBConfigStor.Directory dTmp = new SBConfigStor.Directory();
				if(dTmp == null)
				{
					Console.Error.WriteLine("ERROR: Couldn't create Directory object!");
				}
				else
				{
					bool bRes = true;
					string sUserid = "";

		
					// Create the user

					SBConfigStor.Users.User uTmp = new SBConfigStor.Users.User();
					uTmp.Username = i_sUsername;

					SBConfigStor.Directory.AddUser(uTmp);

					bool bFound = SBConfigStor.Directory.GetUseridByUsername(i_sUsername, out sUserid);
					if(!bFound)
					{
						bRes = false;
						Console.Error.WriteLine("ERROR: Couldn't find user that was just added to the DB!");
					}
					else
					{
						Console.WriteLine("  User created.");

						// Set the password
						if(bRes)
						{
							bRes = dTmp.SetPassByUserid(sUserid, i_sUsername, i_sPassword);
							if(!bRes)
							{
								Console.Error.WriteLine("ERROR:  Couldn't set the password.");
							}
							else
							{
								Console.WriteLine("  User's password set.");
							}
						}

						if( (i_sDefaultWebpage == null) || (i_sDefaultWebpage.Length <= 0) )
						{
							// Not an error, just don't do anything.
						}
						else
						{
							// Set the default webpage
							sbSql = new StringBuilder();
							sbSql.AppendFormat("INSERT INTO tblUserParams (uUserId, iParamType, sParamName, sParamValue) VALUES ('{0}', '{1}', '{2}', '{3}');", sUserid, (int)(UserPrefs.eParamType.DefaultWebpage), UserPrefs.eParamType.DefaultWebpage.ToString(), i_sDefaultWebpage);

							bRes = RunSqlCmd(sbSql.ToString());
							if(bRes)
							{
								Console.WriteLine("  User's default webpage set.");
							}
						}
					}
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR: Caught exception: {0}", exc.ToString());
			}

			return(bRet);
		} // AddUser

		protected static bool RunScript(string i_sFName)
		{
			bool			bRet = true;
			StringBuilder	sbCmd = null;
			StreamReader	srSrc = null;
			string			sTmp = "";

			try
			{
				if(!File.Exists(i_sFName))
				{
					Console.Error.WriteLine("ERROR: Script file '{0}' does not exist.", i_sFName);
					bRet = false;
				}
				else
				{
					srSrc = new StreamReader(i_sFName, Encoding.UTF8);
					sbCmd = new StringBuilder();

					while( (sTmp = srSrc.ReadLine()) != null)
					{
						if(sTmp.EndsWith(";"))
						{
							sbCmd.Append(sTmp);
							bRet &= RunSqlCmd(sbCmd.ToString());
							sbCmd.Length = 0;
						}
						else if(sTmp.ToLower() == "go")
						{
							bRet &= RunSqlCmd(sbCmd.ToString());
							sbCmd.Length = 0;
						}
						else
						{
							sbCmd.Append(sTmp);
						}
					}

					srSrc.Close();
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR: RunScript Caught exception: {0}", exc.ToString());
			}

			return(bRet);
		} // RunScript

		protected static string RunQueryScript(string i_sFName)
		{
			string			sRet = "";
			StringBuilder	sbCmd = null;
			StreamReader	srSrc = null;
			string			sTmp = "";
			bool			bDone = false;

			try
			{
				if(!File.Exists(i_sFName))
				{
					Console.Error.WriteLine("ERROR: Script file '{0}' does not exist.", i_sFName);
					sRet = "";
				}
				else
				{
					srSrc = new StreamReader(i_sFName, Encoding.UTF8);
					sbCmd = new StringBuilder();

					// Unlike RunScript(), it only makes sense for RunQueryScript to run one query since it returns one string.
					while( ((sTmp = srSrc.ReadLine()) != null) && (bDone == false) )
					{
						if(sTmp.EndsWith(";"))
						{
							sbCmd.Append(sTmp);
							sRet = RunSqlQuery(sbCmd.ToString());
							bDone = true;
						}
						else if(sTmp.ToLower() == "go")
						{
							sRet = RunSqlQuery(sbCmd.ToString());
							bDone = true;
						}
						else
						{
							sbCmd.Append(sTmp);
						}
					}

					srSrc.Close();
				}
			}
			catch(Exception exc)
			{
				sRet = "";
				Console.Error.WriteLine("ERROR: RunQueryScript Caught exception: {0}", exc.ToString());
			}

			return(sRet);
		} // RunQueryScript

		protected static bool RunSqlCmd(string i_sSql)
		{
			bool			bRet = true;
			IDbConnection	sqlConn = null;
			IDbCommand		sqlCmd = null;

			try
			{
				Console.WriteLine("  About to run: '{0}'.", i_sSql);

				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}

				try
				{
					sqlCmd = sqlConn.CreateCommand();
					sqlCmd.CommandText = i_sSql;

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();

					sqlConn.Close();
					sqlCmd.Parameters.Clear();

					Console.WriteLine("  Ran SQL command successfully.");
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine("ERROR: RunSqlCmd Caught exception: {0}", exc.ToString());
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
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR: RunSqlCmd Caught exception: {0}", exc.ToString());
			}

			return(bRet);
		} // RunSqlCmd

		protected static string RunSqlQuery(string i_sSql)
		{
			string			sRet = "";
			IDbConnection	sqlConn = null;
			IDbCommand		sqlCmd = null;
			Object			oRes = null;

			try
			{
				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}

				try
				{
					sqlCmd = sqlConn.CreateCommand();
					sqlCmd.CommandText = i_sSql;

					sqlConn.Open();
					oRes = sqlCmd.ExecuteScalar();
					try
					{
						sRet = (string)oRes;
					}
					catch
					{
						sRet = "";
					}

					sqlConn.Close();
					sqlCmd.Parameters.Clear();
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine("ERROR: RunSqlCmd Caught exception: {0}", exc.ToString());
					sRet = "";
					// FIX - log error
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
				sRet = "";
				Console.Error.WriteLine("ERROR: RunSqlCmd Caught exception: {0}", exc.ToString());
			}

			return(sRet);
		} // RunSqlQuery

		private static void GenerateStandaloneConfig()
		{
			string sLocalIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[0].ToString();		// FIX - Assumes active NIC is first one.

			GenerateClusterConfig(sLocalIp, sLocalIp, sLocalIp);
		}

		private static void GenerateClusterConfig(string i_sMasterIp, string i_sSlaveIp, string i_sVirtualIp)
		{
			g_sConfigPath = ConfigurationManager.AppSettings["DefaultConfigPath"];

			if (String.IsNullOrEmpty(g_sConfigPath))
			{
				Console.Error.WriteLine("Action aborted - Missing 'DefaultConfigPath' entry in sbdbutils.exe.config file.");
				return;
			}

			UpdateClusterSettingsInDb(i_sMasterIp, i_sSlaveIp, i_sVirtualIp);
			UpdateSipConfigFiles();
			GenerateConfigFileForShellScript(i_sMasterIp, i_sSlaveIp, i_sVirtualIp);
		}

		private static void UpdateClusterSettingsInDb(string i_sMasterIp, string i_sSlaveIp, string i_sVirtualIp)
		{
			string sSqlUpdateCommand = "UPDATE tblConfigParams SET sValue = '{0}' WHERE sName = '{1}';";

			RunSqlCmd(String.Format(sSqlUpdateCommand, i_sMasterIp, "MasterIp"));
			RunSqlCmd(String.Format(sSqlUpdateCommand, i_sSlaveIp, "SlaveIp"));
			RunSqlCmd(String.Format(sSqlUpdateCommand, i_sVirtualIp, "VirtualIp"));
		}

		private static bool UpdateSipConfigFiles()
		{
			ConfigParams cfgs = new ConfigParams();
			cfgs.LoadFromTableByComponent(ConfigParams.e_Components.SIP.ToString());

			if (0 == cfgs.Count)
			{
				Console.Error.WriteLine("ERROR: Unable to generate SIP configuration files - couldn't load SIP settings from database.");
				return(false);
			}

			ConfigParams cfgs2 = new ConfigParams();
			cfgs2.LoadFromTableByComponent(ConfigParams.e_Components.SIP2.ToString());

			if (0 == cfgs2.Count)
			{
				Console.Error.WriteLine("ERROR: Unable to generate SIP configuration files - couldn't load SIP2 settings from database.");
				return(false);
			}
			else
			{
				cfgs.Add(GetPbxValue(cfgs2));
			}

			ConfigParams cfgs3 = new ConfigParams();
			cfgs3.LoadFromTableByComponent(ConfigParams.e_Components.SIP3.ToString());

			if (0 == cfgs3.Count)
			{
				Console.Error.WriteLine("ERROR: Unable to generate SIP configuration files - couldn't load SIP3 settings from database.");
				return(false);
			}
			else
			{
				foreach (ConfigParams.ConfigParam cfg in cfgs3)
				{
					cfgs.Add(cfg);
				}
			}

			SIP sip = new SIP();
			return(sip.Persist(cfgs, g_sConfigPath));
		} // UpdateSipConfigFiles

		private static ConfigParams.ConfigParam GetPbxValue(ConfigParams i_cfgs)
		{
			// NOTE: The following code is identical to what exists in SBLocalRM.SBLocalRM
			//       so it should be refactored to a common location.

			const string csLableIppbxType = "IP-PBX Type";
			const string csIppbxTypeAdtran = "Adtran";
			const string csIppbxTypeAlcatelOxe = "Alcatel/Lucent OXE";
			const string csIppbxTypeAlcatelOxo = "Alcatel/Lucent OXO";
			const string csIppbxTypeAllworx = "Allworx";
			const string csIppbxTypeAsterisk = "Asterisk";
			const string csIppbxTypeFonalityTrixbox = "Fonality/Trixbox";
			const string csIppbxTypeNecSphericall = "NEC Sphericall";
			const string csIppbxTypeNortelCs = "Nortel CS";
			const string csIppbxTypeNortelBcm = "Nortel BCM";
			const string csIppbxTypeNortelScs = "Nortel SCS";
			const string csIppbxTypeSipxecs = "sipXecs";
			const string csIppbxTypeShoretel = "ShoreTel";
			const string csIppbxTypeShoretelLegacy140 = "ShoreTel legacy (14.0 and earlier)";
			const string csIppbxTypeSwitchvox = "SwitchVox";
			const string csIppbxTypeZultys = "Zultys";

			ConfigParams.ConfigParam cfg = i_cfgs[csLableIppbxType];

			if (null != cfg)
			{
				switch (cfg.Value)
				{
					case csIppbxTypeAsterisk:
					case csIppbxTypeFonalityTrixbox:
					case csIppbxTypeNortelScs:
					case csIppbxTypeSipxecs:
					case csIppbxTypeShoretel:
					case csIppbxTypeSwitchvox:
					case csIppbxTypeZultys:
						cfg.Value = "0";
						break;
					case csIppbxTypeAdtran:
					case csIppbxTypeAlcatelOxe:
					case csIppbxTypeAlcatelOxo:
					case csIppbxTypeAllworx:
					case csIppbxTypeNecSphericall:
					case csIppbxTypeNortelCs:
					case csIppbxTypeNortelBcm:
					case csIppbxTypeShoretelLegacy140:
					default:
						cfg.Value = "1";
						break;
				}
			}

			return cfg;
		} // GetPbxValue

		private static void GenerateConfigFileForShellScript(string i_sMasterIp, string i_sSlaveIp, string i_sVirtualIp)
		{
			const string sConfigFileName = "cluster.config";

			string sFullFileName = Path.Combine(g_sConfigPath, sConfigFileName);

			using (StreamWriter sw = new StreamWriter(sFullFileName, false, new UTF8Encoding(false)))
			{
				sw.WriteLine("# IMPORTANT: This file is regenerated by sbdbutils when invoked with the {0} or {1} switches.", g_sArgCluster, g_sArgStandalone);
				sw.WriteLine("");
				sw.WriteLine("MASTER_IP=\"{0}\"", i_sMasterIp);
				sw.WriteLine("SLAVE_IP=\"{0}\"", i_sSlaveIp);
				sw.WriteLine("VIRTUAL_IP=\"{0}\"", i_sVirtualIp);
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prints to stdout the most recent software version for the named component.  Currently this only returns the
		/// value in the database, which only applies to SpeechBridge >= 4.0.  The component name passed in is case
		/// sensitive.
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private static bool GetLatestSoftwareVersionByComponent(string i_sComponentName)
		{
			bool		bRet = true;
			string		sSql = "", sVersion = "";

			if(i_sComponentName.Length <= 0)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR:  A component name was not specified.");
			}
			else
			{
				sSql = string.Format("SELECT sSoftwareVersion FROM tblComponentUpdates WHERE dtApplied IN (SELECT MAX(dtApplied) FROM tblComponentUpdates WHERE sSoftwareModule = '{0}');", i_sComponentName);

				using (IDbConnection sqlConn = GetDbConnection())
				{
					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						sqlCmd.CommandText = sSql;
						sqlConn.Open();
						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							try
							{
								while (sqlReader.Read())			// There should only be one result.
								{
									sVersion = sqlReader.GetString(0);
									Console.WriteLine(sVersion);
								}
							}
							catch (Exception exc)
							{
								bRet = false;
								Console.Error.WriteLine("ERROR:  Caught exception: " + exc.ToString());
							}
						} // using (IDataReader)

						sqlConn.Close();
					} // using (IDbCommand)
				} // using (IDbConnection)
			} // else

			return (bRet);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prints to stdout the most recent database schema version for the named component.  Currently this only returns the
		/// value in the database, which only applies to SpeechBridge >= 4.0.  The component name passed in is case
		/// sensitive.
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private static bool GetLatestDatabaseVersionByComponent(string i_sComponentName)
		{
			bool bRet = true;

			if (i_sComponentName.Length <= 0)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR:  A component name was not specified.");
			}
			else
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = String.Format("SELECT sDatabaseVersion FROM tblComponentUpdates WHERE dtApplied IN (SELECT MAX(dtApplied) FROM tblComponentUpdates WHERE sSoftwareModule = '{0}');", i_sComponentName);
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						try
						{
							while (sqlReader.Read())			// There should only be one result.
							{
								string sVersion = sqlReader.GetString(0);
								Console.WriteLine(sVersion);
							}
						}
						catch (Exception exc)
						{
							bRet = false;
							Console.Error.WriteLine("ERROR:  Caught exception: " + exc.ToString());
						}
					} // using (IDataReader)

					sqlConn.Close();
				} // using (IDbConnection)
			} // else

			return (bRet);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prints to stdout the language codes of the currently installed languages, each to a separate line.
		/// </summary>
		/// <returns></returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private static bool GetLanguageCodes()
		{
			bool bRet = true;
			Languages languages;

			DialogDesignerDAL ddDal = new DialogDesignerDAL();
			DialogDesignerDAL.ErrorCode errorCode = ddDal.GetInstalledLanguages(out languages);

			if (errorCode == DialogDesignerDAL.ErrorCode.Success)
			{
				foreach (Languages.Language language in languages)
				{
					Console.WriteLine(language.Code);
				}
			}
			else
			{
				bRet = false;
			}

			return bRet;
		}

		private static void GenerateVxml()
		{
			bool bFailure = false;


			// Generate VXML files regardless of whether the Dirty Bit is set in the database.

			VoiceScriptGen voiceXmlTemplateProcessor = new VoiceScriptGen();
			List<VoiceScriptGen.Result> results = voiceXmlTemplateProcessor.GenerateVxmlsFromTemplates();

			foreach (VoiceScriptGen.Result result in results)
			{
				if (result.Success)
				{
					Console.WriteLine(String.Format("SUCCESS: {0}", result.Message));
				}
				else
				{
					bFailure = true;
					Console.Error.WriteLine(String.Format("FAILURE: {0}", result.Message));
				}
			}


			// If all the files were generated successfully then clear the Dirty Bit in the database.

			if (!bFailure)
			{
				ConfigParams.SetVoicexmlDirty(false);
			}


			// If the files didn't exist prior to executing this command then they will have incorrect owner and permission.
			// This needs to be rectified so that generating the files from the web site can succeed.

			Console.WriteLine();
			Console.WriteLine("IMPORTANT: Ensure that the VXML files generated have appropriate owner and permissions set.");
			Console.WriteLine();
		}

		private static int ImportDirectory(string i_sFileType, string i_sFileName, bool i_bMerge)
		{
            int iExitCode = EXIT_CODE_SUCCESS;

			IImportParser importParser = null;

			switch (i_sFileType.ToLower())
			{
				case "csv":
					importParser = new CsvImportParser();
					break;

				case "allworx":
					importParser = new AllworxImportParser();
					break;

				case "shoretel":
					importParser = new ShoreTelImportParser();
					break;

				default:
                    iExitCode = EXIT_CODE_ILLEGAL_ARGUMENT;
					Console.Error.WriteLine("ERROR: Invalid type specified {0} - valid types are: csv, allworx, shoretel", i_sFileType);
					break;
			}

			if (null != importParser)
			{
				byte[] importBytes = GetBytesFromImportFile(i_sFileName);

				if ((null != importBytes) && (importBytes.Length > 0))
				{
                    bool bContinueImport = true;

					if (!i_bMerge)
					{
						bContinueImport = DeleteAllUsersExceptAdmin();
					}

                    if (bContinueImport)
                    {
                        SBConfigStor.Directory sbcDir = new SBConfigStor.Directory();
                        SBConfigStor.Directory.eImportStatus importStatus = sbcDir.Persist(importBytes, importParser, GetNumberOfUsersThatCanBeAdded());

                        if (SBConfigStor.Directory.eImportStatus.eFailure == importStatus)
                        {
                            iExitCode = EXIT_CODE_IMPORT_ERROR;
                            Console.Error.WriteLine("ERROR: There was an error importing the file ('{0}', '{1}').", i_sFileType, i_sFileName);
                        }
                        else
                        {
                            if (SBConfigStor.Directory.eImportStatus.eIncomplete == importStatus)
                            {
                                iExitCode = EXIT_CODE_IMPORT_INCOMPLETE;
                                Console.Error.WriteLine("WARNING: Import incomplete since licensed number of Directory Entries exceeded.");
                            }

                            ConfigParams.SetVoicexmlDirty(true);
                        }
                    }
                    else
                    {
                        iExitCode = EXIT_CODE_IMPORT_ERROR;
                    }
				}
				else
				{
                    iExitCode = EXIT_CODE_IMPORT_ERROR;
					Console.Error.WriteLine("ERROR: Import file was empty or doesn't exist {0}.", i_sFileName);
				}
			}

            return iExitCode;
		}

		private static bool DeleteAllUsersExceptAdmin()
		{
			bool bSuccess = false;

			using (IDbConnection sqlConn = GetDbConnection())
			using (IDbCommand sqlCmd = sqlConn.CreateCommand())
			{
				sqlCmd.CommandText = String.Format("DELETE FROM tblDirectory WHERE sUsername != '{0}'", g_sAdminUsername);
				sqlConn.Open();

				try
				{
					sqlCmd.ExecuteNonQuery();

					DeleteAllUnreferencedUserParams();

					bSuccess = true;
				}
				catch (Exception exc)
				{
					bSuccess = false;
					Console.Error.WriteLine("ERROR:  Caught exception: " + exc.ToString());
				}

				sqlConn.Close();
			}

			return bSuccess;
		}

		private static void DeleteAllUnreferencedUserParams()
		{
			using (IDbConnection sqlConn = GetDbConnection())
			using (IDbCommand sqlCmd = sqlConn.CreateCommand())
			{
				sqlCmd.CommandText = "DELETE FROM tblUserParams WHERE CAST(uUserID AS int) NOT IN (SELECT uUserID FROM tblDirectory)";

				sqlConn.Open();
				sqlCmd.ExecuteNonQuery();
				sqlConn.Close();
			}

			return;
		}

		private static byte[] GetBytesFromImportFile(string i_sFilename)
		{
			byte[] abBytes = null;

			try
			{
				using (Stream s = File.Open(i_sFilename, FileMode.Open))
				using (BinaryReader br = new BinaryReader(s))
				{
					abBytes = br.ReadBytes((int)s.Length);
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("ERROR:  Caught exception: " + exc.ToString());
			}

			return abBytes;
		}

		private static int GetNumberOfUsersThatCanBeAdded()					//$$$ LP - Refactor (c.f. ServerSettingsLdap.aspx.cs)
		{
			int iNumberOfUsersThatCanBeAdded = SBConfigStor.Directory.GetNumberOfUsersThatCanBeAdded();

			if (iNumberOfUsersThatCanBeAdded == -1)
			{
				Console.Error.WriteLine("There was an error reading the database.");

				iNumberOfUsersThatCanBeAdded = 0;
			}

			return iNumberOfUsersThatCanBeAdded;
		}

        private static string GenerateEncryptionKey()
        {
            byte[] key = SimpleAES.SimpleAES.GenerateEncryptionKey();

            StringBuilder sb = new StringBuilder();

            foreach (byte value in key)
            {
                sb.AppendFormat("{0},", value);
            }

            return sb.ToString();
        }
	}
}
