// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using MlkPwgen;

namespace passgen
{
    class PassGen
    {
        static void Main(string[] args)
        {
            const int       iNumChars = 16;
            string          sPass = "";

            sPass = PasswordGenerator.GenerateComplex(iNumChars);

            Console.Out.Write(sPass);
        }
    }
}
