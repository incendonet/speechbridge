// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Xml;

using XmlDocParser;

namespace sbtest
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class SBTest
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ConsoleKeyInfo		key;

			Console.WriteLine("Which test do you want to run?");
			Console.WriteLine("  1 - XmlDocParser");
			Console.WriteLine("  2 - Logger");
			Console.Write("? ");

			key = Console.ReadKey(false);
			switch (key.KeyChar)
			{
				case '1' :
					TestXmlDocParser.TestStdin();
					break;
				case '2' :
					Logging.LoggingTests.Run();
					break;
				default :
					break;
			}
		}

	} // class SBTest

	public class TestXmlDocParser
	{
		public static void TestStdin()
		{
			bool		bRes = true;
			string		sXml = "";
			XElement	xeRoot = new XElement();


			try
			{
				Console.Write("Enter the XML string: ");
				sXml = Console.ReadLine();

				bRes = XmlParser.ParseXml(null, sXml, ref xeRoot);
				if(!bRes)
				{
					Console.Error.WriteLine("ERROR TestWebdavSchema: Failed at ParseXml.");
				}
				else
				{
					Console.WriteLine("TestWebdavSchema: Success.");
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("ERROR TestWebdavSchema: Caught exception: " + e.ToString());
			}

			xeRoot = null;
		} // TestStdin

		public static void TestWebdavSchema()
		{
			bool		bRes = true;
			string		sXml = "";
			XElement	xeRoot = new XElement();

//			sXml = "<?xml version=\"1.0\"?><a:multistatus xmlns:b=\"urn:uuid:c2f41010-65b3-11d1-a29f-00aa00c14882/\" xmlns:d=\"urn:schemas:httpmail:\" xmlns:c=\"xml:\" xmlns:e=\"http://schemas.microsoft.com/exchange/\" xmlns:a=\"DAV:\"><a:response></a:response></a:multistatus>";
			sXml = "<?xml version=\"1.0\"?><a:multistatus xmlns:a=\"DAV:\" xmlns:b=\"urn:uuid:c2f41010-65b3-11d1-a29f-00aa00c14882/\" xmlns:d=\"urn:schemas:httpmail:\" xmlns:c=\"xml:\" xmlns:e=\"http://schemas.microsoft.com/exchange/\"><a:response></a:response></a:multistatus>";

			try
			{
				bRes = XmlParser.ParseXml(null, sXml, ref xeRoot);
				if(!bRes)
				{
					Console.Error.WriteLine("ERROR TestWebdavSchema: Failed at ParseXml.");
				}
				else
				{
					Console.WriteLine("TestWebdavSchema: Success.");
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine("ERROR TestWebdavSchema: Caught exception: " + e.ToString());
			}

			xeRoot = null;
		} // TestWebdavSchema
	} // TestXmlDocParser
} // namespace sbtest
