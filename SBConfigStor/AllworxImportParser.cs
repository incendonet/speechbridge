// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Text;
using System.Xml;

namespace SBConfigStor
{
	public sealed class AllworxImportParser : IImportParser
	{
		#region IImportParser Members

		public bool Parse(byte[] i_abBytes, out Users o_Users)
		{
			bool bRet = true;

			o_Users = new Users();

			try
			{
				string xmlFragment = Encoding.UTF8.GetString(i_abBytes);

				using (XmlTextReader reader = new XmlTextReader(xmlFragment, XmlNodeType.Element, null))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(reader);
					XmlNodeList nodes = doc.SelectNodes("/settings/SystemUser");

					foreach (XmlNode node in nodes)
					{

						string sLName = node.SelectSingleNode("ContactInfo/clientLastName").InnerText;
						string sFName = node.SelectSingleNode("ContactInfo/clientFirstName").InnerText;
						string sMName = node.SelectSingleNode("ContactInfo/clientMiddleName").InnerText;
						string sExt = node.SelectSingleNode("ContactInfo/clientBusinessPhone").InnerText;
						string sEmail = node.SelectSingleNode("ContactInfo/clientBusinessEmail").InnerText;


                        // Allworx prepends a leading "11" to any extension exported.
                        // In discussion with Bryan it was decided that any extension longer than 3 digits 
                        // will have the leading two digits removed (if they are "11") before it is added into the directory.

                        if ( (sExt.Trim().Length > 3) && (sExt.Trim().StartsWith("11")) )
                        {
                            sExt = sExt.Trim().Remove(0, 2);
                        }


						Users.User user = new Users.User();

						user.LName = sLName;
						user.FName = sFName;
						user.MName = sMName;
						user.Ext = sExt;
						user.Email = sEmail;

						o_Users.Add(user);
					}
				}
			}
			catch (Exception exc)
			{
				bRet = false;
				Console.Error.WriteLine("{0} SBConfigStore.AllworxImportParser.Parse exception: {1}", DateTime.Now, exc.ToString());
			}

			return bRet;
		}

		#endregion
	}
}
