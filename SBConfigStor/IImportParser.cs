// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
namespace SBConfigStor
{
	public interface IImportParser
	{
		bool Parse(byte[] i_abBytes, out Users o_Users);
	}
}
