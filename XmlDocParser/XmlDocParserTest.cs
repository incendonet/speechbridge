// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using NUnit.Framework;

using Incendonet.Utilities.LogClient;

namespace XmlDocParser
{
	/// <summary>
	/// Summary description for XmlDocParserTest.
	/// </summary>
	[TestFixture]
	public class XmlDocParserTest
	{
		public XmlDocParserTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[Test]
		public void LoadRSS()
		{
			bool	bRes = true;
			string	sRss = "http://www.cepstral.com/cgi-bin/rss";
			string	sXml = "";
			XElement	xeRoot = new XElement();

			bRes = XmlParser.GetXmlString(null, sRss, out sXml);
			bRes = XmlParser.ParseXml(null, sXml, ref xeRoot);

			xeRoot = null;
		}

		[Test]
		public void TestString()
		{
			string			sXml = "<log>The card id was <value expr=\"card_id\"/>, the card number was <value expr=\"card_num\"/>, val 3 was <value expr=\"three\"/> and so on.</log>";

			bool			bRes = false;
			XElement		xeRoot = new XElement();

			bRes = XmlParser.ParseXml(null, sXml, ref xeRoot);
			XmlParser.DisplayNodeTree(xeRoot);

			xeRoot = null;
		}
	}
}
