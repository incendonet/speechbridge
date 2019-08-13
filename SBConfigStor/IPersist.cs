// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

namespace SBConfigStor
{
	////////////////////////////////////////////////////////////////////////////////
	// IPersist interfaces

	public interface IPersist
	{
		bool Persist();
	}

//	public interface IPersistFile
//	{
//		bool Persist(FileStream i_fsStor);
//	}

//	public interface IPersistSql
//	{
//		bool Persist(string i_sSqlCmd);
//	}

	public interface IPersistConfigParams
	{
		bool Persist(ConfigParams i_ConfigParams, string i_sPath);
	}

	public interface IPersistBytes
	{
		Directory.eImportStatus Persist(byte[] i_abBytes, IImportParser i_Parser, int i_iNumberOfUsersThatCanBeAdded);
	}
}
