// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace SBConfigStor
{
    //$$$ Refactor - duplicated in SBLicenseServer and LicenseGeneratorCLI


    public abstract class CryptoInfoBase : ICryptoInfo
    {
        private string m_sMacAddress;
        protected string m_sIVSource;

        public abstract SymmetricAlgorithm Algo { get; }
        public abstract int BlockSize { get; }

        public CryptoInfoBase(string i_sMacAddress)
        {
            m_sMacAddress = i_sMacAddress;
        }

        public void SetIVSource(string i_sIVSource)
        {
            m_sIVSource = i_sIVSource;
        }

        public byte[] GetKey()
        {
            byte[] key = null;


            // Make legal MAC addresses conform to the restrictions imposed by PhysicalAddress.Parse().

            string sMacAddress = m_sMacAddress.Replace(":", "").ToUpper();

            byte[] macAddress = PhysicalAddress.Parse(sMacAddress).GetAddressBytes();


            // MAC address is only 6 bytes but we need a 32 byte key, so just duplicate it as often as necessary
            // (and then truncate to the right length).

            List<byte> list = new List<byte>();

            for (int i = 0; i < 6; ++i)
            {
                list.AddRange(macAddress);
            }

            list.RemoveRange(32, 4);
            key = list.ToArray();

            return key;
        }

        public abstract byte[] GetIV();
    }
}
