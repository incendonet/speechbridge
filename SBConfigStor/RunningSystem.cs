// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

namespace SBConfigStor
{
	public enum CLRPlatform
	{
		unknown = 0,
		DotNet,
		Mono,
	};

	public enum Database
	{
		unknown = 0,
		MsSql,
		PostgreSql,
	};

	/// <summary>
	/// Summary description for System.
	/// </summary>
	public class RunningSystem
	{
		public RunningSystem()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static CLRPlatform RunningPlatform
		{
			get
			{
				CLRPlatform		clrRet = CLRPlatform.Mono;

				// For now, assumption is that Windows is always DotNet (when in the future it could also be Mono.)
				switch(Environment.OSVersion.Platform)
				{
					case PlatformID.Win32NT :
					case PlatformID.Win32S :
					case PlatformID.Win32Windows :
					case PlatformID.WinCE :
						clrRet = CLRPlatform.DotNet;
						break;
					default :
						clrRet = CLRPlatform.Mono;
						break;
				}

				return(clrRet);
			}
		}

		public static Database RunningDatabase
		{
			get
			{
				Database	dRet = Database.PostgreSql;

				if(RunningPlatform == CLRPlatform.DotNet)
				{
					//dRet = Database.MsSql;		// MS SQL is deprecated in SpeechBridge, Postgres will be used on all platforms
					dRet = Database.PostgreSql;
				}
				else if(RunningPlatform == CLRPlatform.Mono)
				{
					dRet = Database.PostgreSql;
				}
				else
				{
					dRet = Database.PostgreSql;
				}

				return(dRet);
			}
		}
	}
}
