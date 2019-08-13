// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

namespace Incendonet.Utilities.LogClient
{
	using ParamCollection = List<LoggerCore.Param>;

	public interface ILogger
	{
		bool	Init(ref ParamCollection i_Params);		// Init() needs to be called before Open()
		bool	Open();									// The ILogger should only be Open()ed once
		bool	UpdateValue(string i_sThreadId, string i_sName, string i_sValue);	// Updates a single value in the ParamCollection
		bool	Close();								// Calling Close() more than once should not cause an error
		string	GetErrorStr();							// Returns a description of the error from the last time Log() returned 'false'.
		int		GetErrorCode();							// Returns the ID of the error from the last time Log() returned 'false'.

		bool	Log(LoggerCore.eSeverity i_eSeverity, string i_sMsg);
		bool	Log(LoggerCore.eSeverity i_eSeverity, int i_iVmcId, string i_sComponentName, string i_sMsg);
	}
}
