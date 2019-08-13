// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

using RestSharp;

namespace SBConfigStor
{
    public sealed class WebServiceLicenseManager : ILicenseManager
    {
        public string GetLicense(string i_sLicenseFile, string i_sProduct)
        {
            string sLicense = "";

            try
            {
                RestClient client = new RestClient(i_sLicenseFile);

                RestRequest request = new RestRequest(Method.GET);
                request.Resource = String.Format("licenses/{0}", i_sProduct);

                var response = client.Execute(request);

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        sLicense = response.Content;
                        break;

                    default:
                        Log(String.Format("WebServiceLicenseManager.GetLicense - Status: {0} -- ({1}, {2})", response.StatusCode, (int)response.StatusCode, response.StatusDescription));
                        break;
                }
            }
            catch (Exception exc)
            {
                Log(String.Format("WebServiceLicenseManager.GetLicense() Caught exception: {0}", exc.ToString()));
            }

            return sLicense;
        }

        private void Log(string i_sMessage)
        {
            Console.Error.WriteLine("{0} {1}", DateTime.Now, i_sMessage);
        }
    }
}
