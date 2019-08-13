// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Text.RegularExpressions;

namespace Incendonet.Utilities.StringHelper
{
	/// <summary>
	/// Note:  The input URI is assumed to be in the format:  username@domain:port
	/// </summary>
	public class SipFormatter
	{
        private readonly string m_csSipPattern;
		private readonly Regex	m_Regex;

		public enum eUriComponents
		{
			Username,
			Domain,
			Port
		}

        public SipFormatter()
        {
            try
            {
                m_csSipPattern = String.Format(@"^(?<{0}>.*)@(?<{1}>.*):(?<{2}>\d+)", eUriComponents.Username, eUriComponents.Domain, eUriComponents.Port);     // This pattern only handles URIs in the format:  username@domain:port
                m_Regex = new Regex(m_csSipPattern);
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(String.Format("{0} SipFormatter exception: '{1}'.", DateTime.Now, exc.ToString()));
            }
        }


        /// <summary>
		/// </summary>
		/// <param name="i_sUri"></param>
		/// <param name="i_eComp"></param>
		/// <returns></returns>
		public string GetComponentFromUri(string i_sUri, eUriComponents i_eComp)
		{
			string sRet = "";

			try
			{
				Match oMatch = m_Regex.Match(i_sUri);
				if (!oMatch.Success)
				{
					Console.Error.WriteLine(string.Format("{0} SipFormatter.GetComponentFromUri could not get a regex match for {1} from '{2}'.", DateTime.Now, i_eComp, i_sUri));
				}
				else
				{
                    sRet = oMatch.Result(string.Format("${{{0}}}", i_eComp));
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(string.Format("{0} SipFormatter.GetComponentFromUri exception: '{1}'.", DateTime.Now, exc.ToString()));
			}

			return(sRet);
		} // GetComponentFromUri

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUri"></param>
		/// <returns></returns>
		public string GetUsernameFromUri(string i_sUri)
		{
			return (GetComponentFromUri(i_sUri, eUriComponents.Username));
		} // GetUsernameFromUri

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUri"></param>
		/// <returns></returns>
		public string GetDomainFromUri(string i_sUri)
		{
			return (GetComponentFromUri(i_sUri, eUriComponents.Domain));
		} // GetDomainFromUri

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sUri"></param>
		/// <returns></returns>
		public string GetPortFromUri(string i_sUri)
		{
			return (GetComponentFromUri(i_sUri, eUriComponents.Port));
		} // GetPortFromUri
	} // class SipFormatter
}
