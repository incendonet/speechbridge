// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;

using Npgsql;


namespace MigrationTool
{
	public sealed class MigrationTool
	{
		private static string g_sHelp = Environment.NewLine + "SpeechBridge Migration Tool" + Environment.NewLine +
												"  Options are:" + Environment.NewLine +
												"  --help           Displays this message" + Environment.NewLine +
												"" + Environment.NewLine;

		public static void Main(string[] args)
		{
			int	iExitCode = 0;

			try
			{
				Console.WriteLine("{0} MigrationTool - Nothing to do.", DateTime.Now);
			}
			catch (Exception exc)
			{
				iExitCode = 1;
				Console.Error.WriteLine("{0} ERROR: MigrationTool.Main: Caught exception: '{1}' ({2})", DateTime.Now, exc.Message, iExitCode);
			}
			finally
			{
				Console.WriteLine("{0} MigrationTool - Finished.", DateTime.Now);
			}

			System.Environment.Exit(iExitCode);
		} // Main()

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static bool ShouldRun()
		{
			bool bRet = true;

			return bRet;
		} // ShouldRun


    } // class MigrationTool
} // namespace MigrationTool
