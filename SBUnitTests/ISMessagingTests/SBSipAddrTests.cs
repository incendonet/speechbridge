// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using ISMessaging;

namespace SBUnitTests.ISMessagingTests
{
	[TestFixture]
	public class SBSipAddrTests
	{
		[SetUp]
		public void Setup()
		{
		} // Setup

		[TearDown]
		public void Teardown()
		{
		} // Teardown

		[Test]
		public void Test_QuickParse_ValidUris()
		{
			bool				bRes = false;
			string				sName = "", sUser = "", sAddr = "", sPort = "";

			const string		csUser =			"133";
			const string		csIpAddr =			"192.168.1.163";
			const string		csDnsAddr =			"sip.incendonet.com";
			const string		csSipPortDef =		"5060";
			const string		csDisplay1 =		"Montgomery Scott";
			const string		csDisplayHyphen =	"Hyphen - here";

			const string		csUriFull =			@"""" + csDisplay1 + @""" <sip:" + csUser + "@" + csIpAddr + ":5060>";
			const string		csUriFullHyph =		@"""" + csDisplayHyphen + @""" <sip:" + csUser + "@" + csIpAddr + ":5060>";
			const string		csUriNoPort =		@"""" + csDisplay1 + @""" <sip:" + csUser + "@" + csIpAddr + ">";
			const string		csUriHyphNoPort =	@"""" + csDisplayHyphen + @""" <sip:" + csUser + "@" + csIpAddr + ">";
			const string		csUriNoName =		@"<sip:" + csUser + "@" + csIpAddr + ":5060>";
			const string		csUriNoNameNoPort =	@"<sip:" + csUser + "@" + csIpAddr + ">";
			const string		csUriBasicAddr =	@"sip:" + csUser + "@" + csIpAddr + "";
			const string		csUriBasicDns =		@"sip:" + csUser + "@" + csDnsAddr;

			bRes = SBSipAddr.QuickParse(csUriHyphNoPort, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);
			Assert.That(sName == csDisplayHyphen);

			bRes = SBSipAddr.QuickParse(csUriFullHyph, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);
			Assert.That(sName == csDisplayHyphen);

			bRes = SBSipAddr.QuickParse(csUriNoPort, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);
			Assert.That(sName == csDisplay1);

			bRes = SBSipAddr.QuickParse(csUriFull, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);
			Assert.That(sName == csDisplay1);

			bRes = SBSipAddr.QuickParse(csUriNoName, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);

			bRes = SBSipAddr.QuickParse(csUriNoNameNoPort, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);

			bRes = SBSipAddr.QuickParse(csUriBasicAddr, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csIpAddr);
			Assert.That(sPort == csSipPortDef);

			bRes = SBSipAddr.QuickParse(csUriBasicDns, out sUser, out sAddr, out sPort, out sName);
			Assert.IsTrue(bRes);
			Assert.That(sUser == csUser);
			Assert.That(sAddr == csDnsAddr);
			Assert.That(sPort == csSipPortDef);
		}

		[Test]
		public void Test_QuickParse_InvalidUris()
		{
			bool		bRes = false;
			string		sName = "", sUser = "", sAddr = "", sPort = "";

			bRes = SBSipAddr.QuickParse("", out sUser, out sAddr, out sPort, out sName);
			Assert.IsFalse(bRes);

			bRes = SBSipAddr.QuickParse("root@localhost.local", out sUser, out sAddr, out sPort, out sName);
			Assert.IsFalse(bRes);

			bRes = SBSipAddr.QuickParse("133", out sUser, out sAddr, out sPort, out sName);
			Assert.IsFalse(bRes);

		}
	}
}