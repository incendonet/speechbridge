// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace SBTTS
{
	/// <summary>
	/// Summary description for ExeTTS.
	/// </summary>
	public class ExeTTS : ITTS
	{
		private readonly string m_sProgPath;
		private readonly TtsLanguageCodeMapping m_TtsLanguageCodeMapping;

		public ExeTTS(TtsLanguageCodeMapping i_TtsLanguageCodeMapping)
		{
			m_sProgPath = ConfigurationManager.AppSettings["TtsProgPath"];
			m_TtsLanguageCodeMapping = i_TtsLanguageCodeMapping;
		}

		#region ITTS Members

		public bool Init()
		{
			return true;
		}

		public bool Release()
		{
			return true;
		}

		public bool TextToWav(string i_sText, string i_sWavFName, string i_sLang, string i_sGender, string i_sVoiceName)
		{
			bool bRet = true;
			const int MAX_TTS_LENGTH = 8192;

			try
			{
				string sTmp;
				string sArgs;
				string sLang;
				int iExitCode = 0;

				if (i_sText.Length > MAX_TTS_LENGTH)
				{
					sTmp = i_sText.Substring(0, MAX_TTS_LENGTH);
					Console.Error.WriteLine("{0} ExeTTS.TextToWav: Text truncated at: '...{1}' (Original length: {2})", DateTime.Now, sTmp.Substring(MAX_TTS_LENGTH - 40), i_sText.Length);
				}
				else
				{
					sTmp = i_sText;
				}

				sTmp = ReplaceIllegalCharacters(sTmp);

				sLang = GetLanguageCode(i_sLang);

				if (sLang != i_sLang)
				{
					Console.Error.WriteLine("{0} ExeTTS.TextToWav: Language code changed from '{1}' to '{2}'.", DateTime.Now, i_sLang, sLang);
				}

				sArgs = i_sWavFName + " \"" + sTmp + "\"" + " \"" + sLang + "\"" + " \"" + i_sGender + "\"" + " \"" + i_sVoiceName + "\"";

				iExitCode = RunProcess(m_sProgPath, sArgs);

#if(true)
				// If the TTS failed and language was English, try again with the older format using two arguments.
				if (iExitCode != 0)
				{
					if (IsLanguageUSEnglish(sLang))
					{
						sArgs = i_sWavFName + " " + "\"" + sTmp + "\"";

						iExitCode = RunProcess(m_sProgPath, sArgs);

						if (iExitCode != 0)
						{
							Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}') failed - ExitCode = {3}.", DateTime.Now, i_sText, i_sWavFName, iExitCode);
							bRet = false;
						}
						else
						{
							bRet = true;
						}
					}
					else
					{
						Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}', '{3}', '{4}', '{5}') failed - ExitCode = {6}.", DateTime.Now, i_sText, i_sWavFName, sLang, i_sGender, i_sVoiceName, iExitCode);
						bRet = false;
					}
				}
				else
				{
					bRet = true;
				}
#else
				// If the file wasn't created and language was English, try again with the older format of two arguments.
				if (!File.Exists(i_sWavFName))
				{
					if (IsLanguageUSEnglish(sLang))
					{
						sArgs = i_sWavFName + " " + "\"" + sTmp + "\"";

						RunProcess(m_sProgPath, sArgs);

						if (!File.Exists(i_sWavFName))
						{
							Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}') failed - file does not exist.", DateTime.Now, i_sText, i_sWavFName);
							bRet = false;
						}
						else
						{
							bRet = true;
						}
					}
					else
					{
						Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}', '{3}', '{4}', '{5}') failed - file does not exist.", DateTime.Now, i_sText, i_sWavFName, sLang, i_sGender, i_sVoiceName);
						bRet = false;
					}
				}
				else
				{
					// If there was a file, but it was of size 0 (What happens if the file gets deleted before we get here?)

					FileInfo fiTmp = new FileInfo(i_sWavFName);
					if (fiTmp.Length == 0)
					{
						if (IsLanguageUSEnglish(sLang))
						{
							sArgs = i_sWavFName + " " + "\"" + sTmp + "\"";

							RunProcess(m_sProgPath, sArgs);

							if (!File.Exists(i_sWavFName))
							{
								Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}') failed - file does not exist.", DateTime.Now, i_sText, i_sWavFName);
								bRet = false;
							}
							else
							{
								fiTmp = new FileInfo(i_sWavFName);
								if (fiTmp.Length == 0)
								{
									Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}') failed - file is empty.", DateTime.Now, i_sText, i_sWavFName);
									bRet = false;
								}
								else
								{
									bRet = true;
								}
							}
						}
						else
						{
							Console.Error.WriteLine("{0} ExeTTS.TextToWav('{1}', '{2}', '{3}', '{4}', '{5}') failed - file is empty.", DateTime.Now, i_sText, i_sWavFName, sLang, i_sGender, i_sVoiceName);
							bRet = false;
						}
					}
					else
					{
						bRet = true;
					}
				}
#endif
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("{0} ExeTTS.TextToWav exception: {1}", DateTime.Now, exc.ToString());
				bRet = false;
			}

			return bRet;
		}

		#endregion

		private string GetLanguageCode(string i_sLang)
		{
			string sLang = i_sLang;

			if (null != m_TtsLanguageCodeMapping)
			{
				sLang = m_TtsLanguageCodeMapping.GetMappedLanguageCodeFor(i_sLang);
			}

			return sLang;
		}

		private bool IsLanguageUSEnglish(string i_sLanguageCode)
		{
			return ((i_sLanguageCode != null) && ((i_sLanguageCode == "en-US") || (i_sLanguageCode == "en")));
		}

		private string ReplaceIllegalCharacters(string i_sText)
		{
			char[] acIllegal = { '\"' };

			string sRet = i_sText;

			foreach (char c in acIllegal)
			{
				sRet = sRet.Replace(c, ' ');
			}

			return (sRet);
		}

		private int RunProcess(string i_sProgramPath, string i_sCommandLineArguments)
		{
			int iExitCode = 0;

			using (Process process = Process.Start(i_sProgramPath, i_sCommandLineArguments))
			{
				process.WaitForExit();

				iExitCode = process.ExitCode;
			}

			return iExitCode;
		}
	}
}
