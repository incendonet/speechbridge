// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Xml;

namespace SBConfigStor
{
    //$$$ LP - Refactor - duplicated in SBLicenseServer

    public sealed class EncryptedLicenseManager : ILicenseManager
	{
		public string GetLicense(string i_sLicenseFile, string i_sProduct)
		{
			string speechbridgeLicense = "";

			try
			{
				if (String.IsNullOrEmpty(i_sLicenseFile))
				{
					Log("EncryptedLicenseManager.GetLicense() - no license file specified.");
				}
				else
				{
					XmlDocument license = new XmlDocument();

                    using (FileStream fs = new FileStream(i_sLicenseFile, FileMode.Open, FileAccess.Read))
                    {
                        license.Load(fs);
                    }

					XmlNode encryptedSpeechbridgeLicense = license.SelectSingleNode(String.Format("/Incendonet/Licenses/License[@product='{0}']", i_sProduct));

					if (null == encryptedSpeechbridgeLicense)
					{
						Log("EncryptedLicenseManager.GetLicense() - license not found.");
					}
					else
					{
						try
						{
							foreach (string sMacAddress in GetMacAddresses())
							{
								try
								{
                                    ICryptoInfo cryptoInfo = new AesCryptoInfo(sMacAddress);
                                    cryptoInfo.SetIVSource(encryptedSpeechbridgeLicense.Attributes["created"].Value);

                                    speechbridgeLicense = Decrypt(encryptedSpeechbridgeLicense.InnerXml, cryptoInfo);
									break;
								}
                                catch (Exception exc)
                                {
                                    Log(String.Format("EncryptedLicenseManager.GetLicense() - Caught exception: {0}", exc.ToString()));
                                }
                            }
                        }
						catch (Exception)																			// Vague on purpose to not offer up too many clues as to what is wrong.
						{
							Log("EncryptedLicenseManager.GetLicense() - license invalid.");
						}
					}
				}
			}
			catch (FileNotFoundException)
			{
				Log("EncryptedLicenseManager.GetLicense() - unable to load license.");
			}
			catch (UnauthorizedAccessException)
			{
				Log("EncryptedLicenseManager.GetLicense() - unable to read license.");
			}
			catch (Exception exc)
			{
				Log(String.Format("EncryptedLicenseManager.GetLicense() Caught exception: {0}", exc.ToString()));
			}

			return speechbridgeLicense;
		}

		private string Decrypt(string i_sEncryptedText, ICryptoInfo i_cryptoInfo)
		{
			string sDecryptedText;

			using (SymmetricAlgorithm algo = i_cryptoInfo.Algo)
			{
                algo.BlockSize = i_cryptoInfo.BlockSize;

				using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(i_sEncryptedText)))
				using (CryptoStream cs = new CryptoStream(ms, algo.CreateDecryptor(i_cryptoInfo.GetKey(), i_cryptoInfo.GetIV()), CryptoStreamMode.Read))
				using (StreamReader sr = new StreamReader(cs))
				{
					sDecryptedText = sr.ReadToEnd();
				}
			}

			return sDecryptedText;
		}

		private List<string> GetMacAddresses()
		{
            List<string> macAddresses = new List<string>();

			foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
			{
                // Grab the MAC address of each Ethernet interface that is up.
                // NOTE: Need to also allow an OperationalStatus of Unknown since there is a bug in MONO were it returns Unknown for interfaces that are up.
                //       (see here: http://stackoverflow.com/questions/17868420/networkinterface-getallnetworkinterfaces-returns-interfaces-with-operationalst/17869984#17869984).

                if ((adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && ((adapter.OperationalStatus == OperationalStatus.Up) || (adapter.OperationalStatus == OperationalStatus.Unknown)))
				{
					macAddresses.Add(adapter.GetPhysicalAddress().ToString());
				}
			}

			return macAddresses;
		}

        private void Log(string i_sMessage)
		{
			Console.Error.WriteLine("{0} {1}", DateTime.Now, i_sMessage);
		}
	}
}
