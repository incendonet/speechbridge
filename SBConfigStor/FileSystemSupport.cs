// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Configuration;
using System.IO;

namespace SBConfigStor
{
	public sealed class FileSystemSupport
	{
		static string BasePath
		{
			get { return ConfigurationManager.AppSettings["VxmlLocation"]; }
		}

		public static string VxmlPath
		{
			get { return BasePath; }
		}

		public static string PromptPath
		{
			get { return Path.Combine(BasePath, "Prompts"); }
		}

        public static string NameWaveFilePath
        {
            get { return Path.Combine(PromptPath, "Names"); }
        }

		public static string GetDIDPromptFolder(string i_sDID)
		{
			string sPath = "";

			sPath = Path.Combine(PromptPath, "DID");
			sPath = Path.Combine(sPath, i_sDID.ToUpper());

			return sPath;
		}

		public static string GetFullyQualifiedPromptFilename(string i_sFilename)
		{
            string sFilename = "";

            if (!String.IsNullOrEmpty(i_sFilename))
            {
                sFilename = IsFilenameFullyQualified(i_sFilename) ? i_sFilename : Path.Combine(PromptPath, i_sFilename);
            }

            return sFilename;
        }

		public static string GetFullyQualifiedVxmlFilename(string i_sFilename)
		{
            string sFilename = "";

            if (!String.IsNullOrEmpty(i_sFilename))
            {
                sFilename = IsFilenameFullyQualified(i_sFilename) ? i_sFilename : Path.Combine(VxmlPath, i_sFilename);
            }

            return sFilename;
		}

		private static bool IsFilenameFullyQualified(string i_sFilename)
		{
			bool bIsFullyQualified = false;

			string sNormalizedFilename = i_sFilename.ToLower();

			if (sNormalizedFilename.StartsWith("/") ||
				sNormalizedFilename.StartsWith("file:") ||
				sNormalizedFilename.StartsWith("http:"))
			{
				bIsFullyQualified = true;
			}

			return bIsFullyQualified;
		}
	}
}
