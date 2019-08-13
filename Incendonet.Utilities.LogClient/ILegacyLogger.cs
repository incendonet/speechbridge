// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

namespace Incendonet.Utilities.LogClient
{
	public interface ILegacyLogger : ILogger
	{
		bool Init(string i_sIpAddr, string i_sSessionId, string i_sThreadId, string i_sVmcId, string i_sComponentName, string i_sPath);		// Every thread should call Init()

		void Log(Level i_Level, string i_sMessage);
		void Log(Exception i_Exc);
	}
}
