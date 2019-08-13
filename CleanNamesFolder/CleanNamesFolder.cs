// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.IO;

using SBConfigStor;

namespace CleanNamesFolder
{
    public sealed class CleanNamesFolder
    {
        public static void Main(string[] args)
        {
            int iExitCode = 0;

            try
            {
                List<string> nameFiles = GetNameFilesList();
                Users users = GetUsers();


                // Remove any files that are still in use from the list of files.

                foreach (Users.User user in users)
                {
                    string fileName = Path.Combine(FileSystemSupport.NameWaveFilePath, String.Format("{0}.wav", VoiceScriptGen.GetFilenameSafeName(user.GetFullName())));

                    if (nameFiles.Contains(fileName))
                    {
                        nameFiles.Remove(fileName);
                    }
                }


                // Any files left over in the list of files are not associated with any entry in the directory and should thus be deleted.

                foreach (string nameFile in nameFiles)
                {
                    File.Delete(nameFile);
                }
            }
            catch (Exception exc)
            {
                iExitCode = 1;
                Console.Error.WriteLine("{0} ERROR: CleanNamesFolder.Main: Caught exception: '{1}' ({2})", DateTime.Now, exc.Message, iExitCode);
            }

            System.Environment.Exit(iExitCode);
        }


        private static List<string> GetNameFilesList()
        {
            List<string> nameFiles = new List<string>(System.IO.Directory.GetFiles(FileSystemSupport.NameWaveFilePath));

            return nameFiles;
        }

        private static Users GetUsers()
        {
            Users users = new Users();
            users.LoadFromTable();

            return users;
        }
    }
}
