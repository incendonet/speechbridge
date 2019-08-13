// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

//#define USE_INLINEDECRYPT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Npgsql;


namespace SBConfigStor
{
	public class Users : CollectionBase
	{
		public enum eState
		{
			Clean,
			Dirty,
			New,
		};

		public class User
		{
			private	eState	m_eState = eState.Clean;
			private string	m_sUserID = "";
			private Int32	m_iFeatures1 = 0;
			private string	m_sLName = "";
			private string	m_sFName = "";
			private string	m_sMName = "";
			private string	m_sExt = "";
			private string	m_sUsername = "";
			private string	m_sDomain = "";
			private string	m_sEmail = "";
			private string	m_sWavPath = "";
			private string	m_sAltPronunciations = "";
			private byte[]	m_abPasscode = null;
			private byte[]	m_abPassword = null;
			private byte[]	m_abIV = null;
			/*internal*/public string	m_sPassword = "";			// FIX - Deprecate this ASAP!
			private string m_sMobileNumber = "";			// Only used when importing users.
			private string m_sPagerNumber = "";				// Only used when importing users.

			private StringCollection	m_asAltPronunciations = null;

			private List<string> m_groupsForUser = null;

			public eState State
			{
				get	{	return(m_eState);	}
				set	{	m_eState = value;	}
			}

			public string UserID
			{
				get	{	return(m_sUserID);	}
				set {	m_sUserID = value;	}
			}
			public int Features1
			{
				get	{	return(m_iFeatures1);	}
				set	{	m_iFeatures1 = value;	}
			}
			public string LName
			{
				get	{	return(m_sLName);	}
				set {	m_sLName = StringFilter.GetFilteredString(value);	}
			}
			public string FName
			{
				get	{	return(m_sFName);	}
				set {	m_sFName = StringFilter.GetFilteredString(value);	}
			}
			public string MName
			{
				get	{	return(m_sMName);	}
				set {	m_sMName = StringFilter.GetFilteredString(value);	}
			}
			public string Ext
			{
				get	{	return(m_sExt);	}
				set {	m_sExt = StringFilter.GetFilteredStringDial(value);	}
			}
			public string Username
			{
				get	{	return(m_sUsername);	}
				set {	m_sUsername = StringFilter.GetFilteredStringUsername(value);	}
			}
			public string Domain
			{
				get	{	return(m_sDomain);	}
				set {	m_sDomain = StringFilter.GetFilteredStringDomain(value);	}
			}
			public string Email
			{
				get	{	return(m_sEmail);	}
				set {	m_sEmail = StringFilter.GetFilteredStringEmail(value);	}
			}
			public string WavPath
			{
				get	{	return(m_sWavPath);	}
				set {	m_sWavPath = StringFilter.GetFilteredStringPath(value);	}
			}
			public string AltPronunciations
			{
				get	{ return m_sAltPronunciations; }
				set
				{
					if (null == value)
					{
						m_sAltPronunciations = "";
						m_asAltPronunciations.Clear();
					}
					else
					{
						m_sAltPronunciations = value.Trim();

						string[] sSplitString = m_sAltPronunciations.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						m_asAltPronunciations.AddRange(sSplitString);
					}
				}
			}
			public StringCollection AltPronunciationColl
			{
				get {	return(m_asAltPronunciations);	}
			}
			public byte[] Passcode
			{
				get	{	return(m_abPasscode);	}
				set {	m_abPasscode = value;	}
			}
			public byte[] Password
			{
				get	{	return(m_abPassword);	}
				set {	m_abPassword = value;	}
			}
			public byte[] IV
			{
				get	{	return(m_abIV);	}
				set {	m_abIV = value;	}
			}
			public string MobileNumber
			{
				get {	return(m_sMobileNumber); }
				set {   m_sMobileNumber = value; }
			}
			public string PagerNumber
			{
				get {	return(m_sPagerNumber); }
				set {   m_sPagerNumber = value; }
			}

			public List<string> Groups
			{
				get 
				{ 
					if (null == m_groupsForUser) 
					{
						m_groupsForUser = new List<string>();
					}

					return m_groupsForUser;
				}

				set { m_groupsForUser = value; }
			}

			public string GetFullName()
			{
                return String.Format("{0} {1}", FName, LName).Trim();
			}


			// Extract username and domain from email address if they are blank (assuming we have an e-mail address).

			public void SetUsernameAndDomainFromEmail()
			{
				if (!String.IsNullOrEmpty(Email))
				{
					string[] sSplitString = Email.Split(new char[] { '@' }, 2);

					if (String.IsNullOrEmpty(Username))
					{
						Username = sSplitString[0];
					}

					if (String.IsNullOrEmpty(Domain))
					{
						Domain = sSplitString[1];
					}
				}
			}

			public bool HasMobileNumber
			{
				get { return MobileNumber != ""; }
			}

			public bool HasPagerNumber
			{
				get { return PagerNumber != ""; }
			}

			public User()
			{
				m_asAltPronunciations = new StringCollection();
			}

			public static bool operator ==(User a, User b)
			{
				return(Equals(a, b));
			}

			public static bool operator !=(User a, User b)
			{
				return(!(Equals(a, b)));
			}

			public override bool Equals(object obj)
			{
				return Equals((User)obj);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public virtual bool Equals(User i_uToCompare)
			{
				bool		bRet = true;

				try
				{
					if( (i_uToCompare.UserID.Length > 0) && (m_sUserID.Length > 0) && (m_sUserID != i_uToCompare.UserID) )
					{
						bRet = false;
					}
					else if(m_sLName != i_uToCompare.LName)
					{
						bRet = false;
					}
					else if(m_sFName != i_uToCompare.FName)
					{
						bRet = false;
					}
					else if(m_sMName != i_uToCompare.MName)
					{
						bRet = false;
					}
					else if(m_sExt != i_uToCompare.Ext)
					{
						bRet = false;
					}
					else if(m_sUsername != i_uToCompare.Username)
					{
						bRet = false;
					}
					else if(m_sDomain != i_uToCompare.Domain)
					{
						bRet = false;
					}
					else if(m_sEmail != i_uToCompare.Email)
					{
						bRet = false;
					}
					else if (m_sMobileNumber != i_uToCompare.MobileNumber)
					{
						bRet = false;
					}
					else if (m_sPagerNumber != i_uToCompare.PagerNumber)
					{
						bRet = false;
					}

					// FIX - Check for the following?
//					else if(m_iFeatures1)
//					else if(m_sWavPath)
//					else if(m_sAltPronunciations)
//					else if(m_abPasscode)
//					else if(m_abPassword)
//					else if(m_abIV)
				}
				catch
				{
					bRet = false;
				}

				return(bRet);
			}

			public bool LoadFromTable(string i_sUserid)
			{
				bool bRet = true;

				try
				{

#if(USE_INLINEDECRYPT)
					string sCmd = "select uUserId, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber, abPasscode, abPassword, abIV from tblDirectory d where uUserId = '" + i_sUserid + "'";		// NOTE: If parameters are added, removed or reordered then the processing code in LoadUsingSql might need to be changed since it uses explicit position numbers.
#else
					string sCmd = "select uUserId, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber from tblDirectory d where uUserId = '" + i_sUserid + "'";
#endif

					bRet = LoadUsingSql(sCmd);
				}
				catch (Exception exc)
				{
					Console.Error.WriteLine(String.Format("{0} SBConfigStore.User.LoadFromTable(userid) exception: {1}", DateTime.Now, exc.ToString()));		// FIX - log error
					bRet = false;
				}

				return bRet;
			} // LoadFromTable

			public bool LoadFromTable(string i_sDomain, string i_sUsername)
			{
				bool bRet = true;

				try
				{

#if(USE_INLINEDECRYPT)
					string sCmd = "select uUserid, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber, abPasscode, abPassword, abIV from tblDirectory d where sDomain = '" + i_sDomain + "' and sUsername = '" + i_sUsername + "'";		// NOTE: If parameters are added, removed or reordered then the processing code in LoadUsingSql might need to be changed since it uses explicit position numbers.
#else
					string sCmd = "select uUserid, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber from tblDirectory d where sDomain = '" + i_sDomain + "' and sUsername = '" + i_sUsername + "'";
#endif
					
					bRet = LoadUsingSql(sCmd);
				}
				catch (Exception exc)
				{
					Console.Error.WriteLine(String.Format("{0} SBConfigStore.User.LoadFromTable(domain,username) exception: {1}", DateTime.Now, exc.ToString()));			// FIX - log error
					bRet = false;
				}

				return bRet;
			} // LoadFromTable

			public bool LoadUsingSql(string i_sSqlCommandText)
			{
				bool bRet = true;

				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = i_sSqlCommandText;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						if (sqlReader.Read())	// Only gets first match
						{
							PopulateFromReader(sqlReader);

#if(USE_INLINEDECRYPT)
							long lRes = 0;
							// This may not be safe, since the member is public
							lRes = sqlReader.GetBytes(12, 0, null, 0, 0);
							this.Passcode = new byte[lRes];
							lRes = sqlReader.GetBytes(12, 0, this.Passcode, 0, (int)lRes);

							lRes = sqlReader.GetBytes(13, 0, null, 0, 0);
							this.Password = new byte[lRes];
							lRes = sqlReader.GetBytes(13, 0, this.Password, 0, (int)lRes);

							lRes = sqlReader.GetBytes(14, 0, null, 0, 0);
							this.IV = new byte[lRes];
							lRes = sqlReader.GetBytes(14, 0, this.IV, 0, (int)lRes);

							byte[] abKey = SimpleAES.SimpleAES.RetrieveKey();
							SimpleAES.SimpleAES saes = new SimpleAES.SimpleAES(abKey, this.IV);
							m_sPassword = saes.Decrypt(this.Password);

Console.Error.WriteLine(DateTime.Now.ToString() + " ###Pwd in LoadFromTable(userid): '" + m_sPassword + "'");
Console.Error.WriteLine(DateTime.Now.ToString() + " ###Data in LoadFromTable: abPassword = " + SBConfigStor.Directory.ByteArrayToPgsqlOctalString(this.Password) + ", abIV = " + SBConfigStor.Directory.ByteArrayToPgsqlOctalString(this.IV));
#else
							bool bRes = true;
							byte[] abPcd = null, abPwd = null, abIV = null;
							string sPcd = "", sPwd = "";
							Directory dir = new Directory();
							bRes = dir.GetPassByUserid(this.UserID, out abPcd, out abPwd, out abIV, out sPcd, out sPwd);

							this.Passcode = abPcd;
							this.Password = abPwd;
							this.IV = abIV;
							m_sPassword = sPwd;
#endif
						}
						else
						{
							bRet = false;
						}
					}

					sqlConn.Close();
				}

				return bRet;
			} // LoadUsingSql

			public void PopulateFromReader(IDataReader i_sqlReader)
			{
				UserID = i_sqlReader["uUserID"].ToString();
				Features1 = Convert.ToInt32(i_sqlReader["iFeatures1"]);
				LName = i_sqlReader["sLName"].ToString();
				FName = i_sqlReader["sFName"].ToString();
				MName = i_sqlReader["sMName"].ToString();
				Ext = i_sqlReader["sExt"].ToString();
				Username = i_sqlReader["sUsername"].ToString();
				Domain = i_sqlReader["sDomain"].ToString();
				Email = i_sqlReader["sEmail"].ToString();
				WavPath = i_sqlReader["sWavPath"].ToString();
				AltPronunciations = i_sqlReader["sAltPronunciations"].ToString();
				MobileNumber = i_sqlReader["sMobileNumber"].ToString();
				PagerNumber = i_sqlReader["sPagerNumber"].ToString();

				return;
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
		} // User

		public Users()
		{
		}

		public int Add(User i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(User i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(User i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, User i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(User i_Elem)
		{
			List.Remove(i_Elem);
		}

		public User this[int i_iIndex]
		{
			get {	return((User)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		public bool LoadFromTable()
		{
			return LoadFromTable("sLName, sFName");
		}

		public bool LoadFromTable(string i_sOrderBy)
		{
			bool bRet = true;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT uUserID, iFeatures1, sLName, sFName, sMName, sAltPronunciations, sWavPath, sExt, sUsername, sDomain, sEmail, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Mobile') AS sMobileNumber, (SELECT sParamValue FROM tblUserParams WHERE CAST(uUserId AS int) = d.uUserId AND sParamName = 'Pager') AS sPagerNumber FROM tblDirectory d ORDER BY " + i_sOrderBy;
					sqlConn.Open();

					using (IDataReader sqlReader = sqlCmd.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							User userTmp = new User();

							userTmp.PopulateFromReader(sqlReader);

							Add(userTmp);
						}
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(String.Format("{0} SBConfigStore.Users.LoadFromTable exception: {1}", DateTime.Now, exc.ToString()));		// FIX - log error			
				bRet = false;
			}

			return bRet;
		} // LoadFromTable

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
	} // Users
}
