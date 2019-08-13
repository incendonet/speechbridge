// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;

using SBConfigStor;

namespace SBTTS
{
	public sealed class TtsLanguageCodeMapping
	{
		private Dictionary<string, string> m_LanguageCodeMapping = new Dictionary<string, string>();

		public TtsLanguageCodeMapping(ITtsLanguageCodeMappingDAL i_TtsLanguageCodeMappingDAL)
		{
			if (null != i_TtsLanguageCodeMappingDAL)
			{
				m_LanguageCodeMapping = i_TtsLanguageCodeMappingDAL.GetMapping();
			}
		}

		public string GetMappedLanguageCodeFor(string i_sLanguageCode)
		{
			string sLanguageCode = i_sLanguageCode;

			string sMappedLanguageCode;

			if (m_LanguageCodeMapping.TryGetValue(i_sLanguageCode, out sMappedLanguageCode))
			{
				sLanguageCode = sMappedLanguageCode;
			}

			return sLanguageCode;
		}
	}
}
