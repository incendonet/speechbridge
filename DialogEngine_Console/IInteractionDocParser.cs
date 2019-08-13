// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using DialogModel;
using XmlDocParser;

namespace DialogEngine
{
	/// <summary>
	/// Summary description for IInteractionDocParser.
	/// </summary>
	public interface IInteractionDocParser
	{
		eParseError ParseDoc(XElement i_xeElem, DSession i_dsSession, ref DDocument io_ddDoc);
	}
}
