// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;


namespace SimpleAESTests
{
	[TestFixture]
	public class SimpleAESTest
	{
		[Test]
		public void GenerateKey()
		{
			byte [] abKey = SimpleAES.SimpleAES.GenerateEncryptionKey();

			foreach (byte value in abKey)
			{
				Console.Write("{0},", value);
			}

            Assert.That(abKey.Length, Is.EqualTo(32));
		}

		[Test]
		public void GenerateIV()
		{
			byte[] abIV = SimpleAES.SimpleAES.GenerateEncryptionVector();;

			foreach (byte value in abIV)
			{
				Console.Write("{0},", value);
			}

            Assert.That(abIV.Length, Is.EqualTo(16));
		}

		[Test]
		public void TestExtractKey()
		{
			string sKey = "18,120,247,56,55,239,152,28,84,246,69,222,98,188,146,212,134,237,183,193,223,111,179,147,74,187,86,77,154,63,153,197,";

			byte[] abTmp = SimpleAES.SimpleAES.ExtractKey(sKey);

            Assert.That(abTmp, Is.EqualTo(new byte[] { 18, 120, 247, 56, 55, 239, 152, 28, 84, 246, 69, 222, 98, 188, 146, 212, 134, 237, 183, 193, 223, 111, 179, 147, 74, 187, 86, 77, 154, 63, 153, 197 }));
		}

		[Test]
		public void TestEncrypt()
		{
			byte[] abKey = SimpleAES.SimpleAES.GenerateEncryptionKey();
			byte[] abIV = SimpleAES.SimpleAES.GenerateEncryptionVector();;

			string sMessageIn = "I'm a lumberjack and I'm OK, I sleep all night and I work all day.";

			SimpleAES.SimpleAES crypt = new SimpleAES.SimpleAES(abKey, abIV);
			byte[] abEncrypted = crypt.Encrypt(sMessageIn);
			string sMessageOut = crypt.Decrypt(abEncrypted);

			Console.WriteLine(sMessageOut);

            Assert.That(sMessageOut, Is.EqualTo(sMessageIn));
		}
	}
}
