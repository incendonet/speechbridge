// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.IO;
using System.Reflection;

namespace SBConfigStorTests
{
	public abstract class ImportTestBase
	{
		protected byte[] GetBytesFromImportFile(string i_sFilename)
		{
			byte[] abBytes = null;


			// Extract import file from resources in assembly.

			Assembly a = Assembly.GetExecutingAssembly();
			using (Stream s = a.GetManifestResourceStream(String.Format("SBUnitTests.SBConfigStorTests.{0}", i_sFilename)))
			{
				using (BinaryReader br = new BinaryReader(s))
				{
					abBytes = br.ReadBytes((int)s.Length);
				}
			}

			return abBytes;
		}
	}
}
