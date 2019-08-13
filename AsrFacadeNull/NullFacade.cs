// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using Incendonet.Utilities.LogClient;
using ISMessaging.SpeechRec;

namespace AsrFacadeNull
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class NullFacade : IASR
	{
		public NullFacade()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region IASR Members

		public bool LoadGrammar(bool i_bGlobal, string i_sName, string i_sGram)
		{
			// TODO:  Add NullFacade.LoadGrammar implementation
			return true;
		}

		public bool Init(ILegacyLogger i_Logger, GrammarBuilder.eGramFormat i_GrammarFormat)
		{
			// TODO:  Add NullFacade.Init implementation
			return true;
		}

		public bool UtteranceStop()
		{
			// TODO:  Add NullFacade.UtteranceStop implementation
			return true;
		}

		public bool Release()
		{
			// TODO:  Add NullFacade.Release implementation
			return true;
		}

		public bool ResetGrammar()
		{
			// TODO:  Add NullFacade.ResetGrammar implementation
			return true;
		}

		public bool AddUtterance(byte[] i_abData)
		{
			// TODO:  Add NullFacade.AddUtterance implementation
			return true;
		}

		public bool Close()
		{
			// TODO:  Add NullFacade.Close implementation
			return true;
		}

		public bool Open()
		{
			// TODO:  Add NullFacade.Open implementation
			return true;
		}

		public bool LoadGrammarFromFile(bool i_bGlobal, string i_sName, string i_sPath)
		{
			// TODO:  Add NullFacade.LoadGrammarFromFile implementation
			return true;
		}

		public bool AddPhrase(string i_sPhrase, string i_sResult)
		{
			// TODO:  Add NullFacade.AddPhrase implementation
			return true;
		}

		public bool Results(out RecognitionResult[] o_aResults)
		{
			// TODO:  Add NullFacade.Results implementation
			o_aResults = null;
			return false;
		}

		public bool UtteranceStart()
		{
			// TODO:  Add NullFacade.UtteranceStart implementation
			return true;
		}

		public bool SetPropertyInt(int i_iPropIndex, int i_iPropValue)
		{
			// TODO:  Add NullFacade.SetPropertyInt implementation
			return true;
		}

		public bool SetPropertyBool(int i_iPropIndex, bool i_bPropValue)
		{
			// TODO:  Add NullFacade.SetPropertyBool implementation
			return true;
		}

		public bool UtteranceData(byte[] i_abData)
		{
			// TODO:  Add NullFacade.UtteranceData implementation
			return true;
		}

		public bool SetPropertyStr(int i_iPropIndex, string i_sPropValue)
		{
			// TODO:  Add NullFacade.SetPropertyStr implementation
			return true;
		}

		#endregion
	} // class NullFacade
} // namspace AsrFacadeNull
