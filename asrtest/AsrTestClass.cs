// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.IO;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;
using ISMessaging.SpeechRec;
//using AsrFacadeLumenvox;
//using AsrFacadeLumenvox2;

namespace asrtest
{

	enum eAsrType
	{
		unknown,
		julius,
		loquendo,
		lumenvox,
		lumenvox2,
		nuance,
		pocketsphinx,
		vestec,
	};

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class AsrTestClass
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ILegacyLogger	logger = null;
			bool			bRes = true;
			string			sAsr = "", sGramPath = "", sUttPath = "";
			int				iRes, iNumIt = 0, iPause = 0, iNumArgsExpected = 5, iAsrIdx = 0, iGramIdx = 1, iUttIdx = 2, iItIdx = 3, iPauseIdx = 4;
			eAsrType		eAsr = eAsrType.unknown;

			try
			{
for(int ii = 0; ii < args.Length; ii++)
{
	Console.Error.WriteLine("Arg {0}: '{1}'", ii, args[ii].ToString());
}
				Thread.CurrentThread.Name = "asrtest";
				logger = new LegacyLogger();
				logger.Init("", "", Thread.CurrentThread.Name, "", "", "/opt/speechbridge/logs");
				bRes = logger.Open();

				if(!bRes || (args.Length != iNumArgsExpected) )
				{
					if (!bRes)
					{
						Console.Error.WriteLine("Unable to open the console logger!");
					}
					else
					{
						logger.Log(Level.Warning, "Usage:  asrtest AsrType GrammarFile UttWav NumIterations PauseInSecs");
						logger.Log(Level.Warning, "    AsrType can be:  lumenvox, lumenvox2, OR vestec");
					}
					return;
				}
				else
				{
					sAsr = args[iAsrIdx];
					sGramPath = args[iGramIdx];
					sUttPath = args[iUttIdx];
					iNumIt = int.Parse(args[iItIdx]);
					iPause = int.Parse(args[iPauseIdx]);

					if( (sAsr == eAsrType.julius.ToString()) || (sAsr == eAsrType.loquendo.ToString()) || (sAsr == eAsrType.pocketsphinx.ToString()) )
					{
						logger.Log(Level.Exception, "That AsrType is not yet supported.");
					}
					else if (sAsr == eAsrType.lumenvox.ToString())
					{
						eAsr = eAsrType.lumenvox;
					}
					else if(sAsr == eAsrType.lumenvox2.ToString())
					{
						eAsr = eAsrType.lumenvox2;
					}
					else if (sAsr == eAsrType.vestec.ToString())
					{
						eAsr = eAsrType.vestec;
					}

					if(eAsr != eAsrType.unknown)
					{
						iRes = RunTestBuffer(logger, eAsr, sGramPath, sUttPath, iNumIt, iPause);

						logger.Log(Level.Info,"Successfully completed " + iRes.ToString() + " iterations.");
					}
				}

				logger.Close();
			}
			catch(Exception exc)
			{
				logger.Log(Level.Exception, "Main caught exception: " + exc.ToString());
			}

			return;
		}

		private static int RunTestBuffer(ILegacyLogger i_Logger, eAsrType i_eAsrType, string i_sGramPath, string i_sUttPath, int i_iNumIt, int i_iPause)
		{
			StreamReader			srGram = null;
			FileInfo				fiUtt = null;
			FileStream				fsUtt = null;
			byte[]					abUtt = null;
			StringBuilder			sbGram = null;
			string					sTmp = "", sGram = "";
			int						ii = 0, iRead = 0, iLen = 0, iRem = 0, jj = 0;
			bool					bRes = true;
			IASR					oAsr = null;
			RecognitionResult[]		aResults = null;

			try
			{
				// Load grammar file and utterance WAV into buffers
				sbGram = new StringBuilder();
				srGram = new StreamReader(i_sGramPath, Encoding.UTF8);
				while( (sTmp = srGram.ReadLine()) != null)
				{
					sbGram.Append(sTmp + System.Environment.NewLine);
				}
				sGram = sbGram.ToString();

				i_Logger.Log(Level.Info, "Grammar:  " + sGram + "");
				
				fiUtt = new FileInfo(i_sUttPath);
				fsUtt = fiUtt.OpenRead();
				abUtt = new byte[fsUtt.Length];
				iLen = iRem = (int)fsUtt.Length;
				do
				{
					iRead = fsUtt.Read(abUtt, 0, iRem);
					iRem -= iRead;
				} while (iRem > 0);

				// Do ASR init
				bRes = InitAsr(i_Logger, i_eAsrType, out oAsr);
				if(!bRes)
				{
					i_Logger.Log(Level.Exception, "InitAsr failed.");
				}
				else
				{
					// Run test
					for(ii = 0; ii < i_iNumIt; ii++)
					{
						bRes = oAsr.Open();
						if(!bRes)
						{
							i_Logger.Log(Level.Exception, "Asr.Open failed.");
						}
						bRes = oAsr.ResetGrammar();
						if(!bRes)
						{
							i_Logger.Log(Level.Exception, "Asr.ResetGrammar failed.");
						}
						bRes = oAsr.LoadGrammar(false, "main", sGram);
						if(!bRes)
						{
							i_Logger.Log(Level.Exception, "Asr.LoadGrammar failed.");
							bRes = oAsr.Close();
							return(ii);
						}

						SendData(oAsr, abUtt, true);	// Use streaming
//						SendData(oAsr, abUtt, false);	// Doesn't stream

						bRes = oAsr.Results(out aResults);
						if(!bRes)
						{
							i_Logger.Log(Level.Exception, "Asr.Results failed.");
						}
						else
						{
							for(jj = 0; jj < aResults.Length; jj++)
							{
								i_Logger.Log(Level.Info, "Iteration #" + ii.ToString() + ", result #" + jj.ToString() + ", decoded '" + aResults[jj].Result + "' with confidence " + aResults[jj].Probability + "%.");
							}
						}

						// Pause between iterations
						if( (i_iNumIt > 0) && (i_iPause > 0) )
						{
							Thread.Sleep(i_iPause * 1000);
						}
					} // for
				}
			}
			catch(Exception exc)
			{
				i_Logger.Log(Level.Exception, "RunTestBuffer Iteration #" + ii + ", caught exception: " + exc.ToString());
			}

			return(ii);
		} // RunTestBuffer

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_Asr"></param>
		/// <param name="i_abData"></param>
		/// <param name="bStream"></param>
		/// <returns></returns>
		private static bool SendData(IASR i_Asr, byte[] i_abData, bool bStream = true)
		{
			bool bRet = true;

			if (bStream)
			{
				i_Asr.UtteranceStart();
				UtteranceData(i_Asr, i_abData);
				bRet = i_Asr.UtteranceStop();
			}
			else
			{
				bRet = i_Asr.AddUtterance(i_abData);
			}

			return (bRet);
		}

		private static bool UtteranceData(IASR i_Asr, byte[] i_abData)
		{
			bool		bRet = true;
			int			iChunkSize = 160;       // See ISDevice NETWORK_RTP_RATE
			byte[]		abSubData = new byte[iChunkSize];
			int			ii = 0, iNumChunks = 0;

			iNumChunks = i_abData.Length / iChunkSize;

			// FIX - This will drop the remainder, but we probably don't care in a test scenario
			for (ii = 0; ii < iNumChunks; ii++)
			{
				Array.Copy(i_abData, ii * iChunkSize, abSubData, 0, iChunkSize);
				i_Asr.UtteranceData(abSubData);
			}

			return (bRet);
		}

		private static bool InitAsr(ILegacyLogger i_Logger, eAsrType i_eAsrType, out IASR o_Asr)
		{
			bool		bRet = true;

			o_Asr = null;
			try
			{
				switch(i_eAsrType)
				{
					case eAsrType.lumenvox:
//						bRet = Init_Lumenvox(i_Logger, out o_Asr);
						break;
					case eAsrType.lumenvox2:
//						bRet = Init_Lumenvox2(i_Logger, out o_Asr);
						break;
					case eAsrType.vestec:
						i_Logger.Log(Level.Exception, "InitAsr: That ASR is not yet implemented");
						break;
					default :
						i_Logger.Log(Level.Exception, "InitAsr: That ASR is not yet implemented");
						break;
				}
			}
			catch(Exception exc)
			{
				bRet = false;
				i_Logger.Log(Level.Exception, "InitAsr caught exception: " + exc.ToString());
			}

			return(bRet);
		}

		//private static bool Init_Lumenvox(ILegacyLogger i_Logger, out IASR o_Asr)
		//{
		//	bool				bRet = true;
		//	LumenvoxFacade		lfAsr = null;

		//	o_Asr = null;

		//	try
		//	{
		//		lfAsr = new LumenvoxFacade();

		//		bRet = lfAsr.SetPropertyBool((int)AsrFacadeLumenvox.eLumenvoxProperties.LogDecode, true);
		//		bRet = lfAsr.SetPropertyBool((int)AsrFacadeLumenvox.eLumenvoxProperties.UseSemantic, true);
		//		bRet = lfAsr.SetPropertyBool((int)AsrFacadeLumenvox.eLumenvoxProperties.LatticeConfScore, true);

		//		bRet = lfAsr.Init(i_Logger, GrammarBuilder.eGramFormat.SRGS_ABNF);

		//		o_Asr = lfAsr;
		//	}
		//	catch (Exception exc)
		//	{
		//		bRet = false;
		//		i_Logger.Log(Level.Exception, "Init_Lumenvox caught exception: " + exc.ToString());
		//	}

		//	return (bRet);
		//} // Init_Lumenvox

		//private static bool Init_Lumenvox2(ILegacyLogger i_Logger, out IASR o_Asr)
		//{
		//	bool				bRet = true;
		//	LumenvoxFacade2		lfAsr = null;

		//	o_Asr = null;

		//	try
		//	{
		//		lfAsr = new LumenvoxFacade2();

		//		bRet = lfAsr.SetPropertyBool((int)AsrFacadeLumenvox2.eLumenvoxProperties.LogDecode, true);
		//		bRet = lfAsr.SetPropertyBool((int)AsrFacadeLumenvox2.eLumenvoxProperties.UseSemantic, true);
		//		bRet = lfAsr.SetPropertyBool((int)AsrFacadeLumenvox2.eLumenvoxProperties.LatticeConfScore, true);

		//		bRet = lfAsr.Init(i_Logger, GrammarBuilder.eGramFormat.SRGS_ABNF);

		//		o_Asr = lfAsr;
		//	}
		//	catch (Exception exc)
		//	{
		//		bRet = false;
		//		i_Logger.Log(Level.Exception, "Init_Lumenvox caught exception: " + exc.ToString());
		//	}

		//	return (bRet);
		//} // Init_Lumenvox
	} // class AsrTestClass
} // namespace asrtest
