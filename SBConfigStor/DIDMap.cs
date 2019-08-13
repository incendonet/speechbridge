// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SBConfigStor
{
	/// <summary>
	/// Summary description for DIDMap.
	/// </summary>
	public class DIDMap : StringDictionary
	{
		//
		// NOTE:  A StringDictionary may not be the best implementation choice, since it converts the key to lower case.
		//

		public const string	DEFAULT_DID = "DEFAULT";

		public DIDMap()
		{
		}

		public void Load()
		{
			Load_Impl();
		}

		private void Load_Impl()
		{
			try
			{
				List<DIDMapping> didMappings = null;

				if (DIDMappingDAL.GetDIDMappings(out didMappings))
				{
					foreach (DIDMapping didMapping in didMappings)
					{
						Add(didMapping.DID, FileSystemSupport.GetFullyQualifiedVxmlFilename(didMapping.VoiceXml));
					}
				}
			}
			catch (Exception exc)
			{
                // FIX - log error
                Console.Error.WriteLine("{0} SBConfigStore.DIDMap.Load exception: '{1}'", DateTime.Now, exc.ToString());
			}

			return;
		}
	}
}
