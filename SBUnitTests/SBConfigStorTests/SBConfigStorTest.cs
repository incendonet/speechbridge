// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;

using NUnit.Framework;

//using SimpleAES;
using SBConfigStor;

namespace SBConfigStorTests
{
	/// <summary>
	/// Summary description for SBConfigStorTest.
	/// </summary>
	[TestFixture]
	[Explicit("These tests were originally in the SBConfigStor project.")]
	public class SBConfigStorTest
	{
		public SBConfigStorTest()
		{
		}

		[Test]
		public void TestGetUseridByUsername()
		{
			bool						bRes;
			string						sUsername, sUserid;

			sUsername = "sharris";
			bRes = SBConfigStor.Directory.GetUseridByUsername(sUsername, out sUserid);

			Console.WriteLine("Got userid '" + sUserid + "' from username '" + sUsername + "'.");
		}

		/*
		[Test]
		public void TestDirectoryEncryp()
		{
			Directory					dir = null;
			SimpleAES.SimpleAES			saes = null;
			string						sUsername, sUserid, sPasscode, sPassword;
			byte[]						abPasscode = null, abPassword = null, abIV = null, abKey = null;
			bool						bRes;

			dir = new Directory("server=(local);database=dSpeechBridge;uid=;pwd=");
			sUsername = "sharris";
			bRes = dir.GetUseridByUsername(sUsername, out sUserid);
			if(!bRes)
			{
				Console.Error.WriteLine("Couldn't get userid of '" + sUsername + "'!");
			}
			else
			{
				sPasscode = "12345";
				sPassword = "";
				Console.WriteLine("Storing passcode '{0}' and password '{1}' for userid {2}.", sPasscode, sPassword, sUserid);

				//abKey = SimpleAES.SimpleAES.GenerateEncryptionKey();
				abKey = SimpleAES.SimpleAES.ExtractKey("");

				abIV = SimpleAES.SimpleAES.GenerateEncryptionVector();
				saes = new SimpleAES.SimpleAES(abKey, abIV);
				abPasscode = saes.Encrypt(sPasscode);
				abPassword = saes.Encrypt(sPassword);
				saes = null;

				bRes = dir.SetPassByUserid(sUserid, ref abPasscode, ref abPassword, ref abIV);

				sPasscode = sPassword = "";
				bRes = dir.GetPassByUserid(sUserid, out abPasscode, out abPassword, out abIV);

				saes = new SimpleAES.SimpleAES(abKey, abIV);
				sPasscode = saes.Decrypt(abPasscode);
				sPassword = saes.Decrypt(abPassword);

				Console.WriteLine("Retrieved passcode '{0}', password '{1}'.", sPasscode, sPassword);
			}
		}
		*/

		[Test]
		public void TestDIDMap()
		{
			DIDMap dmTest = new DIDMap();
			dmTest.Load();

			Console.WriteLine("DID Map:");

			foreach(DictionaryEntry de in dmTest)
			{
				Console.WriteLine("'{0}', '{1}'", de.Key, de.Value);
			}

			Console.WriteLine("Done.");
		}
	}
}
