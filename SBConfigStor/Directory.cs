// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

//#define USE_INLINEDECRYPT

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Npgsql;


namespace SBConfigStor
{
	public class Directory : IPersistBytes
	{
		private string			m_sSqlConn = "";

		public enum				eErrors
		{
			eSuccess = 0,
			eUnknown = 1,
			eDBError,
			ePasscodeWrong,
			ePasswordWrong,
			eNoMatchingUsername,
		}

		public enum eImportStatus
		{
			eSuccess = 0,
			eFailure = 1,
			eIncomplete = 2,
		}

		public Directory()
		{
		}

		public Directory(string i_sSqlConn)
		{
			m_sSqlConn = i_sSqlConn;
		}

		#region IPersistBytes Members

		public eImportStatus Persist(byte[] i_abBytes, IImportParser i_Parser, int i_iNumberOfUsersThatCanBeAdded)
		{
			eImportStatus importStatus = eImportStatus.eSuccess;									// Assume import to database succeeded.

			try
			{
				Users aUsers;

				if (i_Parser.Parse(i_abBytes, out aUsers))
				{
					importStatus = MergeToTable(aUsers, i_iNumberOfUsersThatCanBeAdded);
				}
				else
				{
					importStatus = eImportStatus.eFailure;
				}
			}
			catch (Exception exc)
			{
				importStatus = eImportStatus.eFailure;
				Console.Error.WriteLine(DateTime.Now.ToString() + "{0} SBConfigStore.Directory.Persist exception: {1}", DateTime.Now, exc.ToString());
			}

			return importStatus;
		}

		#endregion


		public eImportStatus SaveToTable(Users i_Users, int i_iNumberOfUsersThatCanBeAdded)
		{
			eImportStatus saveStatus = eImportStatus.eSuccess;																		// Assume save to database succeeded.

			try
			{
				foreach (Users.User user in i_Users)
				{
					if (user.State == Users.eState.Clean)
					{
						// Skip 'clean' entries (nothing to do).
					}
					else
					{
						user.SetUsernameAndDomainFromEmail();

						switch (user.State)
						{
							case Users.eState.New:
								if (i_iNumberOfUsersThatCanBeAdded > 0)
								{
									--i_iNumberOfUsersThatCanBeAdded;

									AddUser(user);

									if ((user.MobileNumber != "") || (user.PagerNumber != ""))
									{
										string sUserid;
										bool bSuccess = GetUseridFromUser(user, out sUserid);

										if (!bSuccess)
										{
											Console.Error.WriteLine("{0} SBConfigStor.Directory.SaveToTable - unable to retrieve UserID for User: {1}", DateTime.Now, user.GetFullName());
										}
										else
										{
											SaveAlternateNumbers(sUserid, user.MobileNumber, user.PagerNumber);
										}
									}
								}
								else
								{
									saveStatus = eImportStatus.eIncomplete;
									Console.Error.WriteLine("{0} SBconfigStor.Directory.SaveToTable - Maximum number of licensed Directory Entries exceeded.  Import failed for user: {1}", DateTime.Now, user.GetFullName());
								}
								break;

							case Users.eState.Dirty:
								UpdateUser(user);
								SaveAlternateNumbers(user.UserID, user.MobileNumber, user.PagerNumber);
								break;

							default:
								Console.Error.WriteLine("{0} SBConfigStor.Directory.SaveToTable - unexpected User.State ({1}) for User: {2}", DateTime.Now, user.State, user.GetFullName());
								break;
						} // switch
					} // else
				} // foreach
			}			
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStor.Directory.SaveToTable exception: {1}", DateTime.Now, exc.ToString());
				saveStatus = eImportStatus.eFailure;
			}

			return saveStatus;
		} // SaveToTable()

		private void SaveAlternateNumbers(string i_sUserid, string i_sMobileNumber, string i_sPagerNumber)
		{
			AlternateNumbers alternateNumbers = new AlternateNumbers(i_sUserid);
			alternateNumbers.MobileNumber = i_sMobileNumber;
			alternateNumbers.PagerNumber = i_sPagerNumber;

			AlternateNumbers.eStatus status = alternateNumbers.Save();

			if (AlternateNumbers.eStatus.Success != status)
			{
				Console.Error.WriteLine("{0} SBConfigStore.Directory.SaveAlternateNumbers - error saving for UserId: {1} (Mobile: {2}, Pager: {3}, Status: {4})", DateTime.Now.ToString(), i_sUserid, i_sMobileNumber, i_sPagerNumber, status);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_UsersToMergeIn"></param>
		/// <param name="i_iNumberOfUsersThatCanBeAdded"></param>
		/// <returns></returns>
		public eImportStatus MergeToTable(Users i_UsersToMergeIn, int i_iNumberOfUsersThatCanBeAdded)
		{
			eImportStatus mergeStatus = eImportStatus.eSuccess;											// Assume merge to database succeeded.

			try
			{
				Users uInDir = new Users();
				uInDir.LoadFromTable("sUsername");

				MergeUsers(uInDir, i_UsersToMergeIn);

				mergeStatus = SaveToTable(uInDir, i_iNumberOfUsersThatCanBeAdded);
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStore.Directory.MergeToTable exception: {1}", DateTime.Now, exc.ToString());
				mergeStatus = eImportStatus.eFailure;
			}

			return mergeStatus;
		} // MergeToTable

		public void MergeUsers(Users io_UsersInDirectory, Users i_UsersToMergeIn)
		{
			// FIX - Sort i_UsersToMergeIn before merging!!!

			foreach (Users.User userToMerge in i_UsersToMergeIn)
			{
				Users.User foundUser = null;
				int ii = 0;
				int iLen = io_UsersInDirectory.Count;

				while ((ii < iLen) && (foundUser == null))
				{
					Users.User user = io_UsersInDirectory[ii];

					if ((userToMerge.Username.Length > 0) && (userToMerge.Domain.Length > 0))
					{
						if ((userToMerge.Username == user.Username) && (userToMerge.Domain == user.Domain))
						{
							foundUser = user;
						}
						else
						{
							if ((userToMerge.LName.Length > 0) || (userToMerge.FName.Length > 0))
							{
								if ((userToMerge.LName == user.LName) && (userToMerge.FName == user.FName))
								{
									foundUser = user;
								}
								else
								{
									++ii;
								}
							}
							else
							{
								++ii;
							}
						}
					}
					else
					{
						// Didn't have username and domain (most likely CSV import.)

						if ((userToMerge.LName.Length > 0) || (userToMerge.FName.Length > 0))
						{
							if ((userToMerge.LName == user.LName) && (userToMerge.FName == user.FName))
							{
								foundUser = user;
							}
							else
							{
								++ii;
							}
						}
						else
						{
							++ii;
						}
					}
				}

				if (foundUser != null)
				{
					// Compare relevant fields before replacing in list (don't want unnecessary SQL operations)

					if (foundUser == userToMerge)
					{
						// Do nothing.
					}
					else
					{
						UpdateUser(io_UsersInDirectory[ii], userToMerge);
					}
				}
				else
				{
					// Add to the list
					userToMerge.State = Users.eState.New;
					io_UsersInDirectory.Add(userToMerge);
				}
			} // foreach
		} // MergeUsers

		private void UpdateUser(Users.User i_existingUser, Users.User i_importedUser)
		{
			bool bChanged = false;

			if (IsUpdateRequired(i_existingUser.FName, i_importedUser.FName))
			{
				i_existingUser.FName = i_importedUser.FName;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.LName, i_importedUser.LName))
			{
				i_existingUser.LName = i_importedUser.LName;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.MName, i_importedUser.MName))
			{
				i_existingUser.MName = i_importedUser.MName;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.Ext, i_importedUser.Ext))
			{
				i_existingUser.Ext = i_importedUser.Ext;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.Email, i_importedUser.Email))
			{
				i_existingUser.Email = i_importedUser.Email;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.Username, i_importedUser.Username))
			{
				i_existingUser.Username = i_importedUser.Username;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.Domain, i_importedUser.Domain))
			{
				i_existingUser.Domain = i_importedUser.Domain;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.MobileNumber, i_importedUser.MobileNumber))
			{
				i_existingUser.MobileNumber = i_importedUser.MobileNumber;
				bChanged = true;
			}

			if (IsUpdateRequired(i_existingUser.PagerNumber, i_importedUser.PagerNumber))
			{
				i_existingUser.PagerNumber = i_importedUser.PagerNumber;
				bChanged = true;
			}

			if (bChanged)
			{
				// If the existing user that is being updated is a new entry (i.e. hasn't been committed to the DB yet)
				// then don't mark it as Dirty since that will cause the DAL to try an UPDATE rather than INSERT command (which will fail).

				if (i_existingUser.State != Users.eState.New)
				{
					i_existingUser.State = Users.eState.Dirty;
				}
			}
		}

		private bool IsUpdateRequired(string i_sExistingValue, string i_sNewValue)
		{
			return (!String.IsNullOrEmpty(i_sNewValue) && (i_sExistingValue != i_sNewValue));
		}

		public static bool GetUseridByUsername(string i_sUsername, out string o_sUserid)
		{
			bool				bRet = true;
			string				sCmd = "";

			sCmd = "select uUserID from tblDirectory where LOWER(sUsername) = '" + i_sUsername.ToLower() + "'";
			bRet = GetUseridBySql(sCmd, out o_sUserid);

			return(bRet);
		}

		public static bool GetUseridByUsername(string i_sDomain, string i_sUsername, out string o_sUserid)
		{
			bool				bRet = true;
			string				sCmd = "";

			sCmd = "select uUserID from tblDirectory where LOWER(sDomain) = '" + i_sDomain + "' and LOWER(sUsername) = '" + i_sUsername.ToLower() + "'";
			bRet = GetUseridBySql(sCmd, out o_sUserid);

			return(bRet);
		}

		public static bool GetUseridByEmail(string i_sEmail, out string o_sEmail)
		{
			bool				bRet = true;
			string				sCmd = "";

			sCmd = "select uUserID from tblDirectory where LOWER(sEmail) = '" + i_sEmail.ToLower() + "'";
			bRet = GetUseridBySql(sCmd, out o_sEmail);

			return(bRet);
		}

		public static bool GetUseridByExtension(string i_sExtension, out string o_sUserid)
		{
			bool				bRet = true;
			string				sCmd = "";

			sCmd = "select uUserID from tblDirectory where sExt = '" + i_sExtension + "'";
			bRet = GetUseridBySql(sCmd, out o_sUserid);

			return(bRet);
		}

		public static bool GetUseridFromUser(Users.User i_User, out string o_sUserid)
		{
			bool bRet = true;
			IDbConnection sqlConn = null;

			o_sUserid = "";

			try
			{
				if (i_User.UserID != "")
				{
					o_sUserid = i_User.UserID;
				}
				else
				{
					if (RunningSystem.RunningDatabase == Database.MsSql)
					{
						sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
					}
					else if (RunningSystem.RunningDatabase == Database.PostgreSql)
					{
						sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
					}

					using (IDbCommand sqlCmd = sqlConn.CreateCommand())
					{
						// Query based on all the fields passed in when this user was inserted into the database.

						sqlCmd.CommandText = "SELECT uUserID FROM tblDirectory WHERE LOWER(sLName) = @LName AND LOWER(sFName) = @FName AND LOWER(sMName) = @MName AND sExt = @Ext AND LOWER(sUsername) = @Username AND LOWER(sDomain) = @Domain AND LOWER(sEmail) = @Email";

						if (RunningSystem.RunningDatabase == Database.MsSql)
						{
							sqlCmd.Parameters.Add(new SqlParameter("@LName", i_User.LName.ToLower()));
							sqlCmd.Parameters.Add(new SqlParameter("@FName", i_User.FName.ToLower()));
							sqlCmd.Parameters.Add(new SqlParameter("@MName", i_User.MName.ToLower()));
							sqlCmd.Parameters.Add(new SqlParameter("@Ext", i_User.Ext));
							sqlCmd.Parameters.Add(new SqlParameter("@Username", i_User.Username.ToLower()));
							sqlCmd.Parameters.Add(new SqlParameter("@Domain", i_User.Domain.ToLower()));
							sqlCmd.Parameters.Add(new SqlParameter("@Email", i_User.Email.ToLower()));
						}
						else if (RunningSystem.RunningDatabase == Database.PostgreSql)
						{
							sqlCmd.Parameters.Add(new NpgsqlParameter("@LName", i_User.LName.ToLower()));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@FName", i_User.FName.ToLower()));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@MName", i_User.MName.ToLower()));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@Ext", i_User.Ext));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@Username", i_User.Username.ToLower()));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@Domain", i_User.Domain.ToLower()));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@Email", i_User.Email.ToLower()));
						}

						sqlConn.Open();

						using (IDataReader sqlReader = sqlCmd.ExecuteReader())
						{
							// There should only be one, because the email system should only allow unique
							// usernames.  Therefore we'll only take the first one.

							if (sqlReader.Read())
							{
								o_sUserid = sqlReader["uUserID"].ToString();
							}
							else
							{
								bRet = false;
							}
						}

						sqlConn.Close();
					}
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(String.Format("{0} SBConfigStore.Directory.GetUseridfromUser exception: {1}", DateTime.Now, exc.ToString()));
			}
			finally
			{
				sqlConn.Close();
				sqlConn = null;
			}

			return bRet;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUsername"></param>
		/// <param name="o_sUserid"></param>
		/// <returns></returns>
		public static bool GetUseridBySql(string i_sSql, out string o_sUserid)
		{
			bool				bRet = true;
			string				sCmd;
			IDbConnection		sqlConn = null;
			IDbCommand			sqlCmd = null;
			IDataReader			sqlReader = null;

			o_sUserid = "";

			try
			{
				sCmd = i_sSql;

				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if (RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}

				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = sCmd;
				sqlConn.Open();
				sqlReader = sqlCmd.ExecuteReader();

				try
				{
					// There should only be one, because the email system should only allow unique
					// usernames.  Therefore we'll only take the first one.
					if(sqlReader.Read())
					{
						o_sUserid = sqlReader["uUserID"].ToString();
					}
					else
					{
						bRet = false;
					}
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetUseridBySql exception: " + exc.ToString());
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
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetUseridBySql exception: " + exc.ToString());
				// FIX - log error
				string sErr = exc.ToString();
				bRet = false;
			}
			finally
			{
			}

			return(bRet);
		} // GetUseridByUsername

		public static bool DeleteUserByUserid(string i_sUserid)
		{
			bool bUserDeleted = true;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "DELETE FROM tblDirectory WHERE uUserID = @UserID";

					IDbDataParameter sqlParam;

					sqlParam = sqlCmd.CreateParameter();
					sqlParam.DbType = DbType.Int32;
					sqlParam.ParameterName = "@UserID";
					sqlParam.Value = Convert.ToInt32(i_sUserid);
					sqlCmd.Parameters.Add(sqlParam);

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();
					sqlConn.Close();


					// Also delete the user preferences for this user.
					//$$$ LP - This should really be handled inside the database by a foreign key relationship
					//         between tblDirectory and tblUserParams and a cascading delete.

					bUserDeleted = UserPrefs.DeleteFromTableByUserid(i_sUserid);
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} SBConfigStor.Directory.DeleteUserByUserid exception: {1}", DateTime.Now, exc.ToString());
				bUserDeleted = false;
			}

			return bUserDeleted;
		} // DeleteUserByUserid

		/// <summary>
		/// FIX !!! FIX !!! FIX - Returning the string was done because the decrypt fails on Mono
		/// outside of GetPassByUserid for some unknown reason, and should NOT be done for security
		/// reasons.  (Even though the client SHOULD be using HTTPS.
		/// </summary>
		/// <param name="i_sUserID"></param>
		/// <param name="i_sEnteredPasscode"></param>
		/// <param name="o_abPassword"></param>
		/// <param name="o_abIV"></param>
		/// <returns></returns>
		public eErrors GetUserPassword(string i_sUserID, string i_sEnteredPasscode, out byte[] o_abPassword, out byte[] o_abIV, out string o_sPasscode, out string o_sPassword)
		{
			eErrors					eRet = eErrors.eSuccess;
			bool					bRes = true;
			//byte[]					abKey = null;
			byte[]					abPasscode = null, abPassword = null, abIV = null;
			string					sPasscodeFromDb = "", sPasswordFromDb = "";
			//SimpleAES.SimpleAES		aes = null;

			o_abPassword = o_abIV = null;
			o_sPasscode = o_sPassword = "";

			try
			{
				bRes = GetPassByUserid(i_sUserID, out abPasscode, out abPassword, out abIV, out sPasscodeFromDb, out sPasswordFromDb);
				if(!bRes)
				{
					eRet = eErrors.eDBError;
				}
				else
				{
#if(false)	// FIX - using string passed back from GetPassByUserid, which may be removed in the future, see comments in GetPassByUserid.
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####Retrieved in GetUserPassword PIN:{0} Pwd{1} IV {2}.", ByteArrayToPgsqlOctalString(ref abPasscode), ByteArrayToPgsqlOctalString(ref abPassword), ByteArrayToPgsqlOctalString(ref abIV));
					abKey = SimpleAES.SimpleAES.RetrieveKey();
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####Retrieved in GetUserPassword Key:{0}.", ByteArrayToHexString(ref abKey));
					aes = new SimpleAES.SimpleAES(abKey, abIV);

					sPasscodeFromDb = aes.Decrypt(abPasscode);
#endif
					//Console.Error.WriteLine(DateTime.Now.ToString() + " Pin from DB: '{0}', pwd: '{1}'.", sPasscodeFromDb, aes.Decrypt(abPassword));
					//Console.Error.WriteLine(DateTime.Now.ToString() + " Pin from DB: '{0}', pwd: '{1}'.", sPasscodeFromDb, sPasswordFromDb);
					o_sPasscode = sPasscodeFromDb;
					o_sPassword = sPasswordFromDb;

					if(i_sEnteredPasscode != sPasscodeFromDb)
					{
						eRet = eErrors.ePasscodeWrong;
					}
					else
					{
						o_abPassword = abPassword;
						o_abIV = abIV;
					}
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetUserPassword exception: " + exc.ToString());
				eRet = eErrors.eUnknown;
			}

			return(eRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUsername"></param>
		/// <param name="i_sPassword"></param>
		/// <returns></returns>
		public bool Authenticate(string i_sUsername, string i_sPassword)
		{
			bool	bRet = false;

			if(AuthenticateImpl(i_sUsername, i_sPassword) != eErrors.eSuccess)
			{
				bRet = false;
			}
			else
			{
				bRet = true;
			}

			return(bRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUsername"></param>
		/// <param name="i_sPassword"></param>
		/// <returns></returns>
		private eErrors AuthenticateImpl(string i_sUsername, string i_sPassword)
		{
			eErrors					eRet = eErrors.eUnknown;
			bool					bRes = true;
			//byte[]					abKey = null;
			byte[]					abPasscode = null, abPassword = null, abIV = null;
			string					sUserid = "", sPasswordFromDb = "", sPasscodeFromDb = "";
			//SimpleAES.SimpleAES		aes = null;

			try
			{
				bRes = GetUseridByUsername(i_sUsername, out sUserid);
				if(!bRes)
				{
					bRes = GetUseridByEmail(i_sUsername, out sUserid);
				}
				if(!bRes)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.Authenticate failed: No matching username.");
					eRet = eErrors.eNoMatchingUsername;
				}
				else
				{
					bRes = GetPassByUserid(sUserid, out abPasscode, out abPassword, out abIV, out sPasscodeFromDb, out sPasswordFromDb);
					if(!bRes)
					{
						if(abPassword == null)
						{
							Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.Authenticate failed:  No password in the DB.");
							eRet = eErrors.ePasswordWrong;	// Actually, no password stored, so probably not enrolled
						}
						else
						{
							Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.Authenticate failed: DB error.");
							eRet = eErrors.eDBError;
						}
					}
					else
					{
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####AuthenticateImpl Retrieved PIN:{0} Pwd{1} IV {2}.", ByteArrayToPgsqlOctalString(ref abPasscode), ByteArrayToPgsqlOctalString(ref abPassword), ByteArrayToPgsqlOctalString(ref abIV));
						//abKey = SimpleAES.SimpleAES.RetrieveKey();
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####AuthenticateImpl Retrieved Key:{0}.", ByteArrayToHexString(ref abKey));
						//aes = new SimpleAES.SimpleAES(abKey, abIV);

						//sPasswordFromDb = aes.Decrypt(abPassword);
						if(i_sPassword != sPasswordFromDb)
						{
							Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.Authenticate failed: Password wrong.");
//Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.Authenticate failed: DB: '" + sPasswordFromDb + "', typed: '" + i_sPassword + "'.");
							eRet = eErrors.ePasswordWrong;
						}
						else
						{
							eRet = eErrors.eSuccess;
						}
					}
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.Authenticate failed: caught exception: " + exc.ToString());
				eRet = eErrors.eUnknown;
			}

			return(eRet);
		}

		public bool GetPassByUserid(string i_sUserID, out string o_sPasscode, out string o_sPassword)
		{
			byte[]					abPasscode = null, abPassword = null, abIV = null;
			
			return(GetPassByUserid(i_sUserID, out abPasscode, out abPassword, out abIV, out o_sPasscode, out o_sPassword));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUserID"></param>
		/// <param name="o_abPasscode"></param>
		/// <param name="o_abPassword"></param>
		/// <param name="o_abIV"></param>
		/// <returns></returns>
#if(USE_INLINEDECRYPT)
		private bool GetPassByUserid(string i_sUserID, out byte[] o_abPasscode, out byte[] o_abPassword, out byte[] o_abIV, out string o_sPasscode, out string o_sPassword)
#else
		//internal bool GetPassByUserid(string i_sUserID, out byte[] o_abPasscode, out byte[] o_abPassword, out byte[] o_abIV, out string o_sPasscode, out string o_sPassword)
		public bool GetPassByUserid(string i_sUserID, out byte[] o_abPasscode, out byte[] o_abPassword, out byte[] o_abIV, out string o_sPasscode, out string o_sPassword)		// FIX - Should not be public!
#endif
		{
			bool				bRet = true;
			string				sCmd;
			IDbConnection		sqlConn = null;
			IDbCommand			sqlCmd;
			IDataReader			sqlReader;
			long				lRes = 0;
			SimpleAES.SimpleAES	aes = null;
			byte[]				abKey = null, abPwd = null, abPcd = null, abIV = null;

			o_abPasscode = o_abPassword = o_abIV = null;
			o_sPasscode = o_sPassword = "";

			try
			{
				sCmd = "select abPasscode, abPassword, abIV from tblDirectory where uUserID = '" + i_sUserID + "'";

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

				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = sCmd;
				sqlConn.Open();
				sqlReader = sqlCmd.ExecuteReader();

				try
				{
					while(sqlReader.Read())
					{
//Console.WriteLine("SBConfigStor.Directory.GetPassByUserid: Read()ed.");
						// Old way
						//o_abPasscode = (byte[])(sqlReader["abPasscode"]);
						//o_abPassword = (byte[])(sqlReader["abPassword"]);
						//o_abIV = (byte[])(sqlReader["abIV"]);
						// New way
						if(sqlReader.IsDBNull(0))
						{
//Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStor.Directory.GetPassByUserid: Passcode was null.");
						}
						else
						{
							lRes = sqlReader.GetBytes(0, 0, null, 0, 0);
							if(lRes <= 0)
							{
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetPassByUserid: No passcode to retrieve.");
							}
							else
							{
								abPcd = new byte[lRes];
								lRes = sqlReader.GetBytes(0, 0, abPcd, 0, (int)lRes);		// Why on earth does this return a long when the 'length' param is a int???
							}
//console.WriteLine("SBConfigStor.Directory.GetPassByUserid: Pcd {0} bytes.", lRes);
						}

						if(sqlReader.IsDBNull(1))
						{
							Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStor.Directory.GetPassByUserid: Password was null.");
						}
						else
						{
							lRes = sqlReader.GetBytes(1, 0, null, 0, 0);
							if(lRes <= 0)
							{
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetPassByUserid: No password to retrieve.");
							}
							else
							{
								abPwd = new byte[lRes];
								lRes = sqlReader.GetBytes(1, 0, abPwd, 0, (int)lRes);
							}
//Console.WriteLine("SBConfigStor.Directory.GetPassByUserid: Pwd {0} bytes.", lRes);
						}

						if(sqlReader.IsDBNull(2))
						{
							Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStor.Directory.GetPassByUserid: IV was null.");
						}
						else
						{
							lRes = sqlReader.GetBytes(2, 0, null, 0, 0);
							if(lRes <= 0)
							{
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetPassByUserid: No vector to retrieve.");
							}
							else
							{
								abIV = new byte[lRes];
								lRes = sqlReader.GetBytes(2, 0, abIV, 0, (int)lRes);
							}
//Console.WriteLine("SBConfigStor.Directory.GetPassByUserid: IV {0} bytes.", lRes);
						}
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####GetPassByUserid Retrieved PIN:{0} Pwd:{1} IV:{2}.", ByteArrayToPgsqlOctalString(ref abPcd), ByteArrayToPgsqlOctalString(ref abPwd), ByteArrayToPgsqlOctalString(ref abIV));

						if(abIV != null)
						{
							abKey = SimpleAES.SimpleAES.RetrieveKey();
							if( (abKey == null) || (abKey.Length <= 0) )
							{
								Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetPassByUserid: Key not retrieved!");
							}
							else
							{
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####GetPassByUserid Retrieved key:{0}.", ByteArrayToPgsqlOctalString(ref abKey));
								aes = new SimpleAES.SimpleAES(abKey, abIV);

								if(abPcd != null)
								{
									o_sPasscode = aes.Decrypt(abPcd);	// FIX - Returning the string was done because the Decrypt fails on Mono outside of GetPassByUserid for some unknown reason...
								}
								if(abPwd != null)
								{
									o_sPassword = aes.Decrypt(abPwd);	// FIX - Returning the string was done because the Decrypt fails on Mono outside of GetPassByUserid for some unknown reason...
								}
//Console.Error.WriteLine(DateTime.Now.ToString() + " GetPassByUserid: Pin '{0}', pwd '{1}'.", o_sPasscode, o_sPassword);
								o_abPasscode = (byte[])abPcd.Clone();
								o_abPassword = (byte[])abPwd.Clone();
								o_abIV = (byte[])abIV.Clone();
							}
						}
						abKey = null;
						aes = null;
					}
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetPassByUserid exception: " + exc.ToString());
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
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.GetPassByUserid exception: " + exc.ToString());
				// FIX - log error
				string sErr = exc.ToString();
				bRet = false;
			}
			finally
			{
			}

			return(bRet);
		} //GetPassByUserid

		public static string ByteArrayToPgsqlOctalString(ref byte[] i_abData)
		{
			StringBuilder		sbData = null;

			try
			{
				sbData = new StringBuilder();
				sbData.Append("E'");
				foreach(byte bData in i_abData)
				{
					if(bData < 0x08)
					{
						sbData.AppendFormat("\\\\00{0}", Convert.ToString(bData, 8));
					}
					else if(bData < 0x40)
					{
						sbData.AppendFormat("\\\\0{0}", Convert.ToString(bData, 8));
					}
					else
					{
						sbData.AppendFormat("\\\\{0}", Convert.ToString(bData, 8));
					}
				}
				sbData.Append("'");
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " ByteArrayToPgsqlOctalString exception: '{0}'.", exc.ToString());
			}

			return(sbData.ToString());
		}

		public static string ByteArrayToPgsqlOctalString(byte[] i_abData)
		{
			StringBuilder		sbData = null;

			try
			{
				sbData = new StringBuilder();
				sbData.Append("E'");
				foreach(byte bData in i_abData)
				{
					if(bData < 0x08)
					{
						sbData.AppendFormat("\\\\00{0}", Convert.ToString(bData, 8));
					}
					else if(bData < 0x40)
					{
						sbData.AppendFormat("\\\\0{0}", Convert.ToString(bData, 8));
					}
					else
					{
						sbData.AppendFormat("\\\\{0}", Convert.ToString(bData, 8));
					}
				}
				sbData.Append("'");
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " ByteArrayToPgsqlOctalString exception: '{0}'.", exc.ToString());
			}

			return(sbData.ToString());
		}

		protected static string ByteArrayToHexString(ref byte[] i_abData)
		{
			StringBuilder		sbData = null;

			try
			{
				sbData = new StringBuilder();
				foreach(byte bData in i_abData)
				{
					if(bData < 0x10)
					{
						sbData.AppendFormat("0x0{0} ", Convert.ToString(bData, 16));
					}
					else
					{
						sbData.AppendFormat("0x{0} ", Convert.ToString(bData, 16));
					}
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " ByteArrayToHexString exception: '{0}'.", exc.ToString());
			}

			return(sbData.ToString());
		}

		public bool SetPassByUserid(string i_sUserID, string i_sPasscode, string i_sPassword)
		{
			return(SetPassByUserid_impl(i_sUserID, i_sPasscode, i_sPassword));
		}

		private bool SetPassByUserid_impl(string i_sUserID, string i_sPasscode, string i_sPassword)
		{
			bool					bRet = true;
			SimpleAES.SimpleAES		saes = null;
			byte[]					abPasscode = null, abPassword = null, abIV = null, abKey = null;

			try
			{
				// Encrypt password and passcode
				abKey = SimpleAES.SimpleAES.RetrieveKey();
				abIV = SimpleAES.SimpleAES.GenerateEncryptionVector();
				saes = new SimpleAES.SimpleAES(abKey, abIV);
				abPasscode = saes.Encrypt(i_sPasscode);
				abPassword = saes.Encrypt(i_sPassword);
				//Console.Error.WriteLine(DateTime.Now.ToString() + " UEP.OK: saving name '{0}', id '{1}', pwd '{2}', pin '{3}'.", m_editUsername.Text, sUserid, m_editPassword.Text, m_editPasscode.Text);
				saes = null;

				bRet = SetPassByUserid_impl(i_sUserID, ref abPasscode, ref abPassword, ref abIV);
			}
			catch(Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine(DateTime.Now.ToString() + " Directory.SetPassByUserid Caught exception: " + exc.ToString());
			}

			return(bRet);
		}

		public bool SetPassByUserid(string i_sUserID, ref byte[] i_abPasscode, ref byte[] i_abPassword, ref byte[] i_abIV)
		{
			return(SetPassByUserid_impl(i_sUserID, ref i_abPasscode, ref i_abPassword, ref i_abIV));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUserID"></param>
		/// <param name="i_abPasscode"></param>
		/// <param name="i_abPassword"></param>
		/// <param name="i_abIV"></param>
		/// <returns></returns>
		private bool SetPassByUserid_impl(string i_sUserID, ref byte[] i_abPasscode, ref byte[] i_abPassword, ref byte[] i_abIV)
		{
			bool					bRet = true;
			string					sUpdateCmd = "";
			IDbConnection			scConn = null;
			IDbCommand				scCmd = null;

			try
			{
//Console.Error.WriteLine(DateTime.Now.ToString() + " ####SetPassByUserid PIN:{0} Pwd:{1} IV:{2}.", ByteArrayToPgsqlOctalString(ref i_abPasscode), ByteArrayToPgsqlOctalString(ref i_abPassword), ByteArrayToPgsqlOctalString(ref i_abIV));
				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sUpdateCmd = "UPDATE tblDirectory SET abPasscode = @vPasscode, abPassword = @vPassword, abIV = @vIV where uUserID = @vUserID";

					if(m_sSqlConn.Length <= 0)
					{
						scConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
					}
					else
					{
						scConn = new SqlConnection(m_sSqlConn);
					}
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sUpdateCmd = "UPDATE tblDirectory SET abPasscode = @vPasscode, abPassword = @vPassword, abIV = @vIV where uUserID = @vUserID";

					if(m_sSqlConn.Length <= 0)
					{
						scConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
					}
					else
					{
						scConn = new NpgsqlConnection(m_sSqlConn);
					}
				}
				scCmd = scConn.CreateCommand();
				scCmd.CommandText = sUpdateCmd;

				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					scCmd.Parameters.Add(new SqlParameter("@vUserID", i_sUserID));
					scCmd.Parameters.Add(new SqlParameter("@vPasscode", i_abPasscode));
					scCmd.Parameters.Add(new SqlParameter("@vPassword", i_abPassword));
					scCmd.Parameters.Add(new SqlParameter("@vIV", i_abIV));
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					NpgsqlParameter		npgpUserid = null, npgpPasscode = null, npgpPassword = null, npbpIV = null;

					npgpUserid = new NpgsqlParameter("@vUserID", NpgsqlTypes.NpgsqlDbType.Integer);
					npgpUserid.Value = int.Parse(i_sUserID);
					scCmd.Parameters.Add(npgpUserid);

					npgpPasscode = new NpgsqlParameter("@vPasscode", NpgsqlTypes.NpgsqlDbType.Bytea);
					npgpPasscode.Value = i_abPasscode;
					scCmd.Parameters.Add(npgpPasscode);

					npgpPassword = new NpgsqlParameter("@vPassword", NpgsqlTypes.NpgsqlDbType.Bytea);
					npgpPassword.Value = i_abPassword;
					scCmd.Parameters.Add(npgpPassword);

					npbpIV = new NpgsqlParameter("@vIV", NpgsqlTypes.NpgsqlDbType.Bytea);
					npbpIV.Value = i_abIV;
					scCmd.Parameters.Add(npbpIV);
				}

				try
				{
					scConn.Open();
					scCmd.ExecuteNonQuery();
				}
				catch(Exception exc)
				{
					bRet = false;
					Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.SetPassByUserid_impl exception2: " + exc.ToString());
					// FIX - display and log error
				}
				finally
				{
					scCmd.Dispose();
					scCmd = null;
					scConn.Dispose();
					scConn = null;
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine(DateTime.Now.ToString() + " SBConfigStore.Directory.SetPassByUserid_impl exception1: " + exc.ToString());
				// FIX - log error
				string sErr = exc.ToString();
				bRet = false;
			}
			finally
			{
			}

			return(bRet);
		} // SetPassByUserid


		public static void AddUser(Users.User i_User)
		{
			using (IDbConnection sqlConn = GetDbConnection())
			using (IDbCommand sqlCmd = sqlConn.CreateCommand())
			{
				sqlCmd.CommandText = "INSERT INTO tblDirectory (sLName, sFName, sMName, sExt, sEmail, sUsername, sDomain) VALUES (@LName, @FName, @MName, @Ext, @Email, @Username, @Domain)";

				IDbDataParameter sqlParam;

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@LName";
				sqlParam.Value = i_User.LName;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@FName";
				sqlParam.Value = i_User.FName;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@MName";
				sqlParam.Value = i_User.MName;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Ext";
				sqlParam.Value = i_User.Ext;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Email";
				sqlParam.Value = i_User.Email;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Username";
				sqlParam.Value = i_User.Username;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Domain";
				sqlParam.Value = i_User.Domain;
				sqlCmd.Parameters.Add(sqlParam);
					
				sqlConn.Open();
				sqlCmd.ExecuteNonQuery();
				sqlConn.Close();
			}

			return;
		} // AddUser


		public static void UpdateUser(Users.User i_User)
		{
			using (IDbConnection sqlConn = GetDbConnection())
			using (IDbCommand sqlCmd = sqlConn.CreateCommand())
			{
				sqlCmd.CommandText = "UPDATE tblDirectory SET sLName = @LName, sFName = @FName, sMName = @MName, sExt = @Ext, sEmail = @Email, sUsername = @Username, sDomain = @Domain where uUserID = @UserID";

				IDbDataParameter sqlParam;

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@LName";
				sqlParam.Value = i_User.LName;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@FName";
				sqlParam.Value = i_User.FName;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@MName";
				sqlParam.Value = i_User.MName;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Ext";
				sqlParam.Value = i_User.Ext;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Email";
				sqlParam.Value = i_User.Email;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Username";
				sqlParam.Value = i_User.Username;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.String;
				sqlParam.ParameterName = "@Domain";
				sqlParam.Value = i_User.Domain;
				sqlCmd.Parameters.Add(sqlParam);

				sqlParam = sqlCmd.CreateParameter();
				sqlParam.DbType = DbType.Int32;
				sqlParam.ParameterName = "@UserID";
				sqlParam.Value = Convert.ToInt32(i_User.UserID);
				sqlCmd.Parameters.Add(sqlParam);

				sqlConn.Open();
				sqlCmd.ExecuteNonQuery();
				sqlConn.Close();
			}

			return;
		}  // UpdateUser

		public static int GetNumberOfUsersThatCanBeAdded()
		{
			int iNumberOfUsersThatCanBeAdded = 0;


			// Don't count Hidden users, and their aliases, against the limit on number of Directory entries (since they are not used by the system for anything).

			int iNumberOfDirectoryEntries = GroupsDAL.GetNumberOfEntriesInGroup(Group.ALL);
			int iNumberOfAliasEntries = AliasDAL.GetNumberOfAliasesInGroup(Group.ALL);

			if ((iNumberOfDirectoryEntries < 0) || (iNumberOfAliasEntries < 0))
			{
				iNumberOfUsersThatCanBeAdded = -1;
			}
			else
			{
				int iNumberOfTotalEntries = iNumberOfAliasEntries + iNumberOfDirectoryEntries;

				if (iNumberOfTotalEntries < CapabilitiesManager.MaximumNumberOfDirectoryEntries)
				{
					iNumberOfUsersThatCanBeAdded = CapabilitiesManager.MaximumNumberOfDirectoryEntries - iNumberOfTotalEntries;
				}
				else
				{
					iNumberOfUsersThatCanBeAdded = 0;
				}
			}

			return iNumberOfUsersThatCanBeAdded;
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

	} // Directory
}
