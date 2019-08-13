// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

namespace SBTTS
{
	/// <summary>
	/// Summary description for ITTS.
	/// </summary>
	public interface ITTS
	{
		bool Init();
		bool Release();

		bool TextToWav(string i_sText, string i_sWavFName, string i_sLang, string i_sGender, string i_sVoiceName);
	}
}
