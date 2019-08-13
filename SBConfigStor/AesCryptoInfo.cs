// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System.Security.Cryptography;
using System.Text;

namespace SBConfigStor
{
    //$$$ Refactor - duplicated in SBLicenseServer and LicenseGeneratorCLI

    public sealed class AesCryptoInfo : CryptoInfoBase
    {
        public override SymmetricAlgorithm Algo
        {
            get { return Aes.Create(); }
        }

        public override int BlockSize
        {
            get { return 128; }
        }

        public AesCryptoInfo(string i_sMacAddress) : base(i_sMacAddress)
        {
        }

        public override byte[] GetIV()
        {
            // Extract the 16 bytes that change the most in the string used as the source for the IV.

            StringBuilder sb = new StringBuilder(16);
            sb.Append(m_sIVSource.Substring(2, 2));
            sb.Append(m_sIVSource.Substring(5, 2));
            sb.Append(m_sIVSource.Substring(8, 2));
            sb.Append(m_sIVSource.Substring(11, 2));
            sb.Append(m_sIVSource.Substring(14, 2));
            sb.Append(m_sIVSource.Substring(17, 2));
            sb.Append(m_sIVSource.Substring(20, 4));

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
