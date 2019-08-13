// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using NUnit.Framework;

using SBEmail;

namespace SBEmailTests
{
	[TestFixture]
	public class ConnectorTest
	{
		[Test]
		public void TestThatCorrectTypeIsInstantiatedForExchange2000()
		{
			IConnector connector = Connector.Create(eEmailServerType.Exchange2000);

			Assert.IsInstanceOf<WebDavConnectorImplementation>(connector); 
		}

		[Test]
		public void TestThatCorrectTypeIsInstantiatedForExchange2003()
		{
			IConnector connector = Connector.Create(eEmailServerType.Exchange2003);

			Assert.IsInstanceOf<WebDavConnectorImplementation>(connector);
		}

		[Test]
		public void TestThatCorrectTypeIsInstantiatedForExchange2007WebDAV()
		{
			IConnector connector = Connector.Create(eEmailServerType.Exchange2007WebDAV);

			Assert.IsInstanceOf<WebDavConnectorImplementation>(connector);
		}

		[Test]
		public void TestThatCorrectTypeIsInstantiatedForExchange2007Legacy()
		{
			IConnector connector = Connector.Create(eEmailServerType.Exchange2007Legacy);

			Assert.IsInstanceOf<WebDavConnectorImplementation>(connector);
		}

		[Test]
		public void TestThatCorrectTypeIsInstantiatedForUnknownServerType()
		{
			IConnector connector = Connector.Create(eEmailServerType.unknown);

			Assert.IsInstanceOf<WebDavConnectorImplementation>(connector);
		}

		[Test]
		public void TestThatCorrectTypeIsInstantiatedForExchange2007()
		{
			IConnector connector = Connector.Create(eEmailServerType.Exchange2007);

			Assert.IsInstanceOf<EwsConnectorImplementation>(connector);
		}
		[Test]
		public void TestThatCorrectTypeIsInstantiatedForExchange2010()
		{
			IConnector connector = Connector.Create(eEmailServerType.Exchange2010);

			Assert.IsInstanceOf<EwsConnectorImplementation>(connector);
		}
	}
}
