// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

using Npgsql;

namespace SBConfigStor
{
	////////////////////////////////////////////////////////////////////////////////
	/// FIX - Replace with DictionaryBase derived class if further functionality
	/// is required.  (List implementation not terribly efficient when using string
	/// based array accessor.)

	public class ConfigParams : CollectionBase
	{
		public const string			c_Separator = ":";

		// FIX - Move the two enums below somewhere else!  We really don't want to recompile SBConfigStore every time there is a new val.  Also, they are used in multiple places.
		public enum e_Components
		{
			General,
			SIP,
			SIP2,
			SIP3,
			LDAP,
			MSExchange,
			Collaboration,
			Apps,
				global,
				SpeechAttendant,
			VoiceXML,
			Licensing,
		};

		public enum e_SpeechAppSettings		// Why is this here?
		{
			AudioRecording,
			OperatorExtension,
			IntroBargein,
			CollabCommands,
			MaxRetriesBeforeOperator,
			EDNoiseLevel,
			EDNoiseLevelInternal,
			AlternateNumbersEditing,
			AliasEditing,
			CallTimeout,
			MoreOptions,
			RecognitionCutoff,
			ConfirmationCutoff,
			DIDTruncationLength,
            DtmfDialingRestricted,
		};

		public enum e_CollabSettings		// Why is this here?
		{
			Type,
			Address,
		};


		public const string	m_csParamIDColumn = "uParamID";
		public const string	m_csGroupNameColumn = "sGroupName";
		public const string	m_csServerIPColumn = "sServerIP";
		public const string	m_csComponentColumn = "sComponent";
		public const string	m_csNameColumn = "sName";
		public const string	m_csValueColumn = "sValue";
		public const string	m_csLabelColumn = "sLabel";
		public const string	m_csHintColumn = "sHint";
		public const string	m_csAdvancedColumn = "bAdvanced";


		public class ConfigParam
		{
			private string		m_sParamID = "";
			private string		m_sGroupName = "";
			private string		m_sServerIP = "";
			private string		m_sComponent = "";
			private string		m_sName = "";
			private string		m_sValue = "";
			private string		m_sLabel = "";
			private string		m_sHint = "";
			private bool		m_bAdvanced = false;
			private bool		m_bDirty = false;			// Initially 'clean'.

			public ConfigParam()
			{
			}

			public string ParamID
			{
				get {	return(m_sParamID);	}
				set
				{
					if(m_sParamID != value)
					{
						m_sParamID = value;
						m_bDirty = true;
					}
				}
			}

			public string GroupName
			{
				get {	return(m_sGroupName);	}
				set
				{
					if(m_sGroupName != value)
					{
						m_sGroupName = value;
						m_bDirty = true;
					}
				}
			}

			public string ServerIP
			{
				get {	return(m_sServerIP);	}
				set
				{
					if(m_sServerIP != value)
					{
						m_sServerIP = value;
						m_bDirty = true;
					}
				}
			}

			public string Component
			{
				get {	return(m_sComponent);	}
				set
				{
					if(m_sComponent != value)
					{
						m_sComponent = value;
						m_bDirty = true;
					}
				}
			}

			public string Name
			{
				get {	return(m_sName);	}
				set
				{
					if(m_sName != value)
					{
						m_sName = value;
						m_bDirty = true;
					}
				}
			}

			public string Value
			{
				get {	return(m_sValue);	}
				set
				{
					if(m_sValue != value)
					{
						m_sValue = value;
						m_bDirty = true;
					}
				}
			}

			public string Label
			{
				get {	return(m_sLabel);	}
				set
				{
					if(m_sLabel != value)
					{
						m_sLabel = value;
						m_bDirty = true;
					}
				}
			}

			public string Hint
			{
				get {	return(m_sHint);	}
				set
				{
					if(m_sHint != value)
					{
						m_sHint = value;
						m_bDirty = true;
					}
				}
			}

			public bool Advanced
			{
				get {	return(m_bAdvanced);	}
				set
				{
					if(m_bAdvanced != value)
					{
						m_bAdvanced = value;
						m_bDirty = true;
					}
				}
			}

			public bool Dirty
			{
				get {	return(m_bDirty);	}
				set {	m_bDirty = value;	}
			}
		} // ConfigParam

		public int Add(ConfigParam i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(ConfigParam i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(ConfigParam i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, ConfigParam i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(ConfigParam i_Elem)
		{
			List.Remove(i_Elem);
		}

		public ConfigParam this[int i_iIndex]
		{
			get {	return((ConfigParam)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		/// <summary>
		/// Unfortunately, the label is the only somewhat unique ID we'll have to work with, unless
		/// we were to use state mechanisms in ASP.NET.
		/// </summary>
		public ConfigParam this[string i_sLabel]		// FIX - Why is this the 'label' and not the 'name'?
		{
			get
			{
				ConfigParam		paramRet = null;
				int				ii, iNumParams;

				iNumParams = List.Count;
				for(ii = 0; ii < iNumParams; ii++)
				{
					if(((ConfigParam)(List[ii])).Label == i_sLabel)
					{
						paramRet = ((ConfigParam)(List[ii]));
					}
				}

				return(paramRet);
			}
		}

		//		public void RemoveAt(int i_iIndex)
//		{
//			List.RemoveAt(i_iIndex);
//		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("SBConfigStor.ConfigParams+ConfigParam");
			t2 = typeof(SBConfigStor.ConfigParams.ConfigParam);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type SBConfigStor.ConfigParams+ConfigParam.", "i_oValue");
			}
		}


		// FIX - We'll eventually need other forms of this func: LoadFromTable(string i_sServer), (string i_sComponent), (string i_sServer, string i_sComponent), ...
		public bool LoadFromTable()
		{
			return(LoadFromTableByComponent(""));
		}

		public bool LoadFromTableByComponent(string i_sComponent)
		{
			bool				bRet = true;
			string				sCmd;
			IDbConnection		sqlConn = null;
			IDbCommand			sqlCmd = null;
			IDataReader			sqlReader;
			ConfigParam			paramTmp;
			string				sTmp;

			try
			{
//NpgsqlEventLog.Level = LogLevel.Debug;
//NpgsqlEventLog.EchoMessages = true;
//Console.WriteLine("In ConfigParams.LoadFromTable().");
//Console.WriteLine("Connection string: '{0}'.", ConfigurationSettings.AppSettings["NpgsqlConnStr"]);
				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					sqlConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					sqlConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}

				if(i_sComponent.Length > 0)
				{
					sCmd = "select * from tblConfigParams WHERE sComponent LIKE '" + i_sComponent + ":%' ORDER BY " + m_csAdvancedColumn + ", " + m_csComponentColumn;
				}
				else
				{
					sCmd = "select * from tblConfigParams ORDER BY " + m_csAdvancedColumn + ", " + m_csComponentColumn;
				}

				//sqlCmd = new SqlCommand(sCmd, sqlConn);
				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandText = sCmd;
				sqlConn.Open();
//Console.WriteLine("Open()ed.");
				sqlReader = sqlCmd.ExecuteReader();
//Console.WriteLine("ExecuteReader()ed.");

				try
				{
					while(sqlReader.Read())
					{
//Console.WriteLine("Read().");
						paramTmp = new ConfigParam();

						//sqlReader.GetString(0);	// Fast way, but not maintainable
						paramTmp.ParamID = sqlReader[m_csParamIDColumn].ToString();
						paramTmp.GroupName = sqlReader[m_csGroupNameColumn].ToString();
						paramTmp.ServerIP = sqlReader[m_csServerIPColumn].ToString();
						paramTmp.Component = sqlReader[m_csComponentColumn].ToString();
						paramTmp.Name = sqlReader[m_csNameColumn].ToString();
						paramTmp.Value = sqlReader[m_csValueColumn].ToString();
						paramTmp.Label = sqlReader[m_csLabelColumn].ToString();
						paramTmp.Hint = sqlReader[m_csHintColumn].ToString();
						sTmp = sqlReader[m_csAdvancedColumn].ToString();
						if( (sTmp == "1") || (sTmp == "True") )
						{
							paramTmp.Advanced = true;
						}
						else
						{
							paramTmp.Advanced = false;
						}
						paramTmp.Dirty = false;

						Add(paramTmp);
					}
				}
				catch(Exception exc)
				{
Console.WriteLine(exc.ToString());
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
Console.WriteLine(exc.ToString());
				bRet = false;
			}
			finally
			{
			}

			return(bRet);
		} // LoadFromTable

		public bool SaveToTable()
		{
			return(SaveToTableByComponent(""));
		}

		// FIX - Is there a more efficient way to update multiple records at once?  DataSet?
		// FIX - If update fails because record isn't there, do an INSERT.
		// Note - This only saves the 'sValue' field.  Currently no need to set other values, except in the case of an INSERT as described above.
		public bool SaveToTableByComponent(string i_sComponent)
		{
			bool			bRet = true;
			int				ii, iNumParams;
			string			sUpdateCmd;
			IDbConnection	sqlConn = null;
			IDbCommand		sqlCmd = null;

			try
			{
				if(i_sComponent.Length > 0)
				{
					sUpdateCmd = "UPDATE tblConfigParams SET sValue = @vValue WHERE uParamID = CAST(@vParamID as int) AND sComponent LIKE @vComponent";
				}
				else
				{
					sUpdateCmd = "UPDATE tblConfigParams SET sValue = @vValue WHERE uParamID = CAST(@vParamID as int)";
				}

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
					sqlCmd.CommandText = sUpdateCmd;
//Console.WriteLine("SBConfigStor.ConfigParams.SaveTable: sqlCmd == '{0}', numparams == {1}.", sUpdateCmd, Count);

					iNumParams = Count;
					for(ii = 0; ii < iNumParams; ii++)
					{
						if(RunningSystem.RunningDatabase == Database.MsSql)
						{
							sqlCmd.Parameters.Add(new SqlParameter("@vParamID", ((ConfigParam)(List[ii])).ParamID));
							sqlCmd.Parameters.Add(new SqlParameter("@vValue", ((ConfigParam)(List[ii])).Value));
							if(i_sComponent.Length > 0)
							{
								sqlCmd.Parameters.Add(new SqlParameter("@vComponent", i_sComponent + '%'));
							}
						}
						else if(RunningSystem.RunningDatabase == Database.PostgreSql)
						{
							sqlCmd.Parameters.Add(new NpgsqlParameter("@vParamID", ((ConfigParam)(List[ii])).ParamID));
							sqlCmd.Parameters.Add(new NpgsqlParameter("@vValue", ((ConfigParam)(List[ii])).Value));
							if(i_sComponent.Length > 0)
							{
								sqlCmd.Parameters.Add(new NpgsqlParameter("@vComponent", i_sComponent + '%'));
							}
						}

						sqlConn.Open();
//Console.WriteLine("SBConfigStor.ConfigParams.SaveTable: 1");
						sqlCmd.ExecuteNonQuery();
//Console.WriteLine("SBConfigStor.ConfigParams.SaveTable: 2");

						sqlConn.Close();
//Console.WriteLine("SBConfigStor.ConfigParams.SaveTable: 3");
						sqlCmd.Parameters.Clear();
					}
				}
				catch(Exception exc)
				{
Console.WriteLine(exc.ToString());
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
Console.WriteLine(exc.ToString());
				bRet = false;
				// FIX - log error
			}
			finally
			{
			}

			return(bRet);
		} // SaveToTable

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sName"></param>
		/// <param name="o_bBool"></param>
		/// <returns></returns>
		public static bool GetBoolFromNamedValue(string i_sName, out bool o_bBool)
		{
			bool				bRet = true;
			string				sCmd = "", sTmp = "";
			IDbConnection		sqlConn = null;
			IDbCommand			sqlCmd = null;
			IDataReader			sqlReader = null;

			o_bBool = false;

			try
			{
				sCmd = "SELECT sValue FROM tblConfigParams WHERE sName='" + i_sName + "'";

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
					if(sqlReader.Read())
					{
						sTmp = sqlReader["sValue"].ToString();
						o_bBool = (sTmp == "true");
					}
					else
					{
						bRet = false;
					}
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine("{0} ConfigParams.GetBoolFromNamedValue('{1}') exception1: {2}", DateTime.Now.ToString(), i_sName,  exc.ToString());
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
				Console.Error.WriteLine("{0} ConfigParams.GetBoolFromNamedValue('{1}') exception2: {2}", DateTime.Now.ToString(), i_sName, exc.ToString());
				bRet = false;
			}

			return(bRet);
		} // GetBoolFromNamedValue

		public static bool InsertToTable(string i_sGroupName, string i_sServerIP, string i_sComponent, string i_sName, string i_sLabel, string i_sValue, string i_sHint/*, bool i_bAdvanced*/)
		{
			bool			bRet = true;
			StringBuilder	sbInsertCmd = null;
			IDbConnection	sqlConn = null;
			IDbCommand		sqlCmd = null;

			try
			{
				sbInsertCmd = new StringBuilder();
				sbInsertCmd.AppendFormat
				(
					"INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint) Values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
					i_sGroupName, i_sServerIP, i_sComponent, i_sName, i_sLabel, i_sValue, i_sHint
				);

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
					sqlCmd.CommandText = sbInsertCmd.ToString();

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();

					sqlConn.Close();
					sqlCmd.Parameters.Clear();
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " ConfigParams.InsertToTable exception: " + exc.ToString());
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
				Console.Error.WriteLine(DateTime.Now.ToString() + " ConfigParams.InsertToTable exception: " + exc.ToString());
				bRet = false;
				// FIX - log error
			}
			finally
			{
			}

			return(bRet);
		} // InsertToTable

		public static bool DeleteAllFromTableByComponent(string i_sComponent)
		{
			bool			bRet = true;
			string			sDelCmd = "";
			IDbConnection	sqlConn = null;
			IDbCommand		sqlCmd = null;

			try
			{
				sDelCmd = "DELETE FROM tblConfigParams WHERE sComponent LIKE '" + i_sComponent + "%'";

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
					sqlCmd.CommandText = sDelCmd;

					sqlConn.Open();
					sqlCmd.ExecuteNonQuery();

					sqlConn.Close();
					sqlCmd.Parameters.Clear();
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine(DateTime.Now.ToString() + " ConfigParams.InsertToTable exception: " + exc.ToString());
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
				Console.Error.WriteLine(DateTime.Now.ToString() + " ConfigParams.DeleteAllFromTableByComponent exception: " + exc.ToString());
				bRet = false;
				// FIX - log error
			}
			finally
			{
			}

			return(bRet);
		} // DeleteAllFromTableByComponent

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sName"></param>
		/// <param name="i_bBool"></param>
		/// <returns></returns>
		public static bool SetBoolFromNamedValue(string i_sName, bool i_bBool)
		{
			bool			bRet = true;
			string			sCmd = "";
			IDbConnection	scConn = null;
			IDbCommand		scCmd = null;

			try
			{
				// FIX - Should INSERT if the value record isn't in the DB.
				if(i_bBool)
				{
					sCmd = "UPDATE tblConfigParams SET sValue='true' WHERE sName='" + i_sName + "'";
				}
				else
				{
					sCmd = "UPDATE tblConfigParams SET sValue='false' WHERE sName='" + i_sName + "'";
				}

				if(RunningSystem.RunningDatabase == Database.MsSql)
				{
					scConn = new SqlConnection(ConfigurationManager.AppSettings["SqlConnStr"]);
				}
				else if(RunningSystem.RunningDatabase == Database.PostgreSql)
				{
					scConn = new NpgsqlConnection(ConfigurationManager.AppSettings["NpgsqlConnStr"]);
				}
				scCmd = scConn.CreateCommand();
				scCmd.CommandText = sCmd;

				try
				{
					scConn.Open();
					scCmd.ExecuteNonQuery();
				}
				catch(Exception exc)
				{
					Console.Error.WriteLine("ConfigParams.SetBoolFromNamedValue('{0}') exception: {1}", i_sName, exc.ToString());
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
				Console.Error.WriteLine("ConfigParams.SetBoolFromNamedValue('{0}') exception: {1}", i_sName, exc.ToString());
				bRet = false;
			}

			return(bRet);
		}

		public static int GetNumExt()
		{
			const int DEFAULT_NUMBER_OF_EXTENSIONS = 2;
			int iNumExt = DEFAULT_NUMBER_OF_EXTENSIONS;

			try
			{
				using (IDbConnection sqlConn = GetDbConnection())
				using (IDbCommand sqlCmd = sqlConn.CreateCommand())
				{
					sqlCmd.CommandText = "SELECT sValue FROM tblConfigParams WHERE sName = 'SipNumExt'";

					sqlConn.Open();

					if (!Int32.TryParse(Convert.ToString(sqlCmd.ExecuteScalar()), out iNumExt))
					{
						iNumExt = DEFAULT_NUMBER_OF_EXTENSIONS;
						Console.Error.WriteLine("{0} ConfigParams.GetNumExt - Unable to retrieve SipNumExt value from DB, using default value ({1}).", DateTime.Now, iNumExt);
					}

					sqlConn.Close();
				}
			}
			catch (Exception exc)
			{
				iNumExt = DEFAULT_NUMBER_OF_EXTENSIONS;
				Console.Error.WriteLine("{0} ConfigParams.GetNumExt exception: {1}", DateTime.Now, exc.ToString());
			}

			return iNumExt;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_bDirty"></param>
		/// <returns></returns>
		public static bool SetVoicexmlDirty(bool i_bDirty)
		{
			bool			bRet = true;

			bRet = SetBoolFromNamedValue("VoicexmlDirty", i_bDirty);

			return(bRet);
		} // SetVoicexmlDirty

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static bool GetVoicexmlDirty(out bool o_bDirty)
		{
			bool				bRet = true;

			bRet = GetBoolFromNamedValue("VoicexmlDirty", out o_bDirty);

			return(bRet);
		} // GetVoicexmlDirty

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_bAccepted"></param>
		/// <returns></returns>
		public static bool SetEulaAccepted(bool i_bAccepted)
		{
			bool				bRet = true;

			bRet = SetBoolFromNamedValue("EULAAccepted", i_bAccepted);

			return(bRet);
		} // SetEulaAccepted

		/// <summary>
		/// 
		/// </summary>
		/// <param name="o_bAccepted"></param>
		/// <returns></returns>
		public static bool GetEulaAccepted(out bool o_bAccepted)
		{
			bool				bRet = true;

			bRet = GetBoolFromNamedValue("EULAAccepted", out o_bAccepted);

			return(bRet);
		} // GetEulaAccepted

        public static bool GetBoolFromConfigParams(ConfigParams.e_SpeechAppSettings i_parameterName)
        {
            bool bValue = false;

            try
            {
                ConfigParams cfgs = new ConfigParams();
                bool bRes = cfgs.LoadFromTableByComponent(ConfigParams.e_Components.Apps.ToString() + ConfigParams.c_Separator + ConfigParams.e_Components.SpeechAttendant.ToString());

                if (!bRes)
                {
                    Console.Error.WriteLine(DateTime.Now.ToString() + " GetBoolFromConfigParams - call to LoadFromTableByComponent failed.");
                }
                else
                {
                    for (int i = 0; i < cfgs.Count; ++i)
                    {
                        if (i_parameterName.ToString() == cfgs[i].Name)
                        {
                            bValue = (cfgs[i].Value == "1" ? true : false);
                            break;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(DateTime.Now.ToString() + " GetBoolFromConfigParams exception: " + exc.ToString());
            }

            return bValue;
        }

		public static bool IsVoiceRecognitionEnabled()
		{
			ConfigParams cfgs = new ConfigParams();
			bool bRes = cfgs.LoadFromTableByComponent(ConfigParams.e_Components.Apps.ToString() + ConfigParams.c_Separator + ConfigParams.e_Components.global.ToString());

			if (!bRes)
			{
				Console.Error.WriteLine("{0} ConfigParams.IsVoiceRecognitionEnabled - Call to LoadFromTableByComponent failed", DateTime.Now);
			}
			else
			{
				foreach (ConfigParams.ConfigParam param in cfgs)
				{
					if (param.Name == ConfigParams.e_SpeechAppSettings.EDNoiseLevel.ToString())
					{
						return IsVoiceRecognitionEnabled(param.Value);
					}
				}
			}


			// If accessing the database fails then assume that voice recognition is enabled since that is
			// what happens in AudioEngine_Console.AudioEngine_srv.AudioInThread.ProcessSessionBegin().

			return true;
		}

		//$$$ LP - This is really just a helper function created for use in ServerSettings.aspx.cs.  Don't like the fact that it a) exists and b) is public.
		public static bool IsVoiceRecognitionEnabled(string i_sEDNoiseLevel)
		{
			short wEDNoiseLevel;

			if (!Int16.TryParse(i_sEDNoiseLevel, out wEDNoiseLevel))
			{
				Console.Error.WriteLine("{0} ConfigParams.IsVoiceRecognitionEnabled - Unable to parse EDNoiseLevel ('{1}') - using default value ({2})", DateTime.Now, i_sEDNoiseLevel, wEDNoiseLevel);
			}


			// If parsing the value fails then assume that voice recognition is enabled since that is what 
			// happens in AudioEngine_Console.AudioEngine_srv.AudioInThread.ProcessSessionBegin().

			return (wEDNoiseLevel < 32000);
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
	} // ConfigParams
}