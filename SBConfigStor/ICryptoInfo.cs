// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System.Security.Cryptography;

namespace SBConfigStor
{
    //$$$ Refactor - duplicated in SBLicenseServer and LicenseGeneratorCLI

    public interface ICryptoInfo
    {
        SymmetricAlgorithm Algo { get; }
        int BlockSize { get; }

        void SetIVSource(string i_sIVSource);
        byte[] GetKey();
        byte[] GetIV();
    }
}
