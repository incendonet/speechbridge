// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using ISMessaging;

namespace SBResourceMgr
{
	/// <summary>
	/// Summary description for IResourceMgr.
	/// </summary>
	public interface IResourceMgr
	{
		int GetMaxSessions();
		int GetNumSessions();

		//int AddSession(string i_sSessionId);
		int AddSession(int i_iThreadIndex, string i_sSrc);
		int AddSession(int i_iThreadIndex, string i_sSrc, int i_iMaxRetries, int i_iRetryLen);
		//int AddSessionIfNew(int i_iThreadIndex, string i_sSrc);
		//bool ReleaseSession(string i_sSessionId);
		bool ReleaseSession(int i_iKey);

		//ISMVMC GetVMCBySrc(string i_sSrc);
		ISMVMC GetVMCByKey(int i_iKey);
		ISMVMC GetVMCBySessionid(string i_sSessionid);

		string GetSessionId(int i_iKey);
	}
}
