// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// NOTE:  All member functions were originally intended to be static, but the legacy js parser can't handle calling static members.			//
// NOTE:  There NEEDS to be validity checking in the input params in a number of the functions.												//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Incendonet.Utilities.StringHelper
{
	public class DateFormatter
	{
		/// <summary>
		/// 
		/// </summary>
		public class Errors
		{
			public const string		m_csOK =				"OK";
			public const string		m_csError =				"ERROR";
			public const string		m_csErrorException =	"ERROR_EXCEPTION";
			public const string		m_csErrorNullString =	"ERROR_NULL_STRING";
			public const string		m_csErrorEmptyString =	"ERROR_EMPTY_STRING";
			public const string		m_csErrorTooLong =		"ERROR_TOO_LONG";
			public const string		m_csErrorTooShort =		"ERROR_TOO_SHORT";
		} // class Errors

		public class Formats
		{
			public const string		m_csDate_YYYYMMDD =		"YYYYMMDD";
			public const string		m_csDate_MMDDYYYY =		"MMDDYYYY";
			public const string		m_csDate_DDMMYYYY =		"DDMMYYYY";
		} // class Formats

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Init()
		{
			return (true);
		}

		/// <summary>
		/// Returns a string in the format:  YYYYMMDD:HHMMSS.mmm
		/// </summary>
		/// <param name="i_dtStamp"></param>
		/// <returns></returns>
		public static string GetNumericDateTime(DateTime i_dtStamp)
		{
			string sRet = "";
			StringBuilder sbTmp = null;

			try
			{
				sbTmp = new StringBuilder();

				sbTmp.AppendFormat("{0}{1}{2}:{3}{4}{5}.{6}", i_dtStamp.Year.ToString("D4"), i_dtStamp.Month.ToString("D2"), i_dtStamp.Day.ToString("D2"), i_dtStamp.Hour.ToString("D2"), i_dtStamp.Minute.ToString("D2"), i_dtStamp.Second.ToString("D2"), i_dtStamp.Millisecond.ToString("D3"));

				sRet = sbTmp.ToString();
			}
			catch (Exception exc)
			{
				sRet = "";
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.GetNumericDateTime exception: " + exc.ToString());
			}

			return (sRet);
		} // GetNumericDateTime

		//		public static DateTime GetDatetimeFromString(string i_sInStr)
		public DateTime GetDatetimeFromString(string i_sInStr)
		{
			DateTime				dtRet = DateTime.MinValue;
			int						iYear = 0, iMonth = 0, iDay = 0;

			if ((i_sInStr == null) || (i_sInStr.Length <= 0))
			{
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.GetDatetimeFromString No date.");
				dtRet = DateTime.MinValue;
			}
			else if (i_sInStr.IndexOf('$') >= 0)
			{
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.GetDatetimeFromString Invalid date.");
				dtRet = DateTime.MinValue;
			}
			else
			{
				iYear =		int.Parse(i_sInStr.Substring(0, 4));
				iMonth =	int.Parse(i_sInStr.Substring(4, 2));
				iDay =		int.Parse(i_sInStr.Substring(6, 2));

				dtRet = new DateTime(iYear, iMonth, iDay);
			}

			return (dtRet);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sInStr"></param>
		/// <param name="i_sInOrder"></param>
		/// <param name="i_sOutOrder"></param>
		/// <returns></returns>
//		public static string ReorderDateDigits(string i_sInStr, string i_sInOrder, string i_sOutOrder)
		public string ReorderDateDigits(string i_sInStr, string i_sInOrder, string i_sOutOrder)
		{
			string sRet = Errors.m_csError;
			string sYear = "", sMonth = "", sDay = "";

			try
			{
				if ((i_sInStr == null) || (i_sInOrder == null) || (i_sOutOrder == null))
				{
                    Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.ReorderDateDigits One or more parameters was null.");
					sRet = Errors.m_csError;
				}
				else if ((i_sInStr.Length != 8) || (i_sInOrder.Length != 8) || (i_sOutOrder.Length != 8))
				{
                    Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.ReorderDateDigits One or more parameters was not of length 8.");
					Console.Error.WriteLine("InStr = '{0}'.", i_sInStr);
					Console.Error.WriteLine("InOrder = '{0}'.", i_sInOrder);
					Console.Error.WriteLine("OutOrder = '{0}'.", i_sOutOrder);
					sRet = Errors.m_csError;
				}
				else
				{
					if (i_sInOrder == Formats.m_csDate_MMDDYYYY)
					{
						sYear = i_sInStr.Substring(4, 4);
						sMonth = i_sInStr.Substring(0, 2);
						sDay = i_sInStr.Substring(2, 2);
					}
					else if (i_sInOrder == Formats.m_csDate_DDMMYYYY)
					{
						sYear = i_sInStr.Substring(4, 4);
						sMonth = i_sInStr.Substring(2, 2);
						sDay = i_sInStr.Substring(0, 2);
					}
					else if (i_sInOrder == Formats.m_csDate_YYYYMMDD)
					{
						sYear = i_sInStr.Substring(0, 4);
						sMonth = i_sInStr.Substring(4, 2);
						sDay = i_sInStr.Substring(6, 2);
					}
					else
					{
                        Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.ReorderDateDigits In-ordering not supported: " + i_sInOrder);
						sRet = Errors.m_csError;
					}

					if (sYear.Length > 0)
					{
						if (i_sOutOrder == Formats.m_csDate_YYYYMMDD)
						{
							sRet = sYear + sMonth + sDay;
						}
						else
						{
                            Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.ReorderDateDigits Out-ordering not supported: " + i_sInOrder);
							sRet = Errors.m_csError;
						}
					}
				}
			}
			catch (Exception exc)
			{
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.ReorderDateDigits exception: " + exc.ToString());
				sRet = Errors.m_csError;
			}

			return (sRet);
		} // ReorderDateDigits

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sYear"></param>
		/// <param name="i_sMonth"></param>
		/// <param name="i_sDay"></param>
		/// <returns></returns>
//		public static bool IsValidDateComponents(string i_sYear, string i_sMonth, string i_sDay)
		public bool IsValidDateComponents(string i_sYear, string i_sMonth, string i_sDay)
		{
			bool bRet = false;
			int iYear = -1, iMonth = -1, iDay = -1;
			DateTime dtNow = DateTime.Now;

			try
			{
				iYear = int.Parse(i_sYear);
				iMonth = int.Parse(i_sMonth);
				iDay = int.Parse(i_sDay);

				if ((iYear < 1850) || (iYear > dtNow.Year))
				{
					bRet = false;
				}
				else if ((iMonth <= 0) || (iMonth > 12))
				{
					bRet = false;
				}
				else if ((iDay <= 0) || (iDay > 31))
				{
					bRet = false;
				}
				else
				{
					bRet = true;
				}
			}
			catch (Exception exc)
			{
				bRet = false;
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.IsValidDateComponents exception: " + exc.ToString());
			}

			return (bRet);
		} // IsValidDateComponents

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sDate"></param>
		/// <param name="i_sDateFormat"></param>
		/// <returns></returns>
//		public static bool IsValidDateStr(string i_sDate, string i_sDateFormat)
		public bool IsValidDateStr(string i_sDate, string i_sDateFormat)
		{
			bool bRet = false;
			string sYear = "", sMonth = "", sDay = "";

			try
			{
				if ((i_sDate == null) || (i_sDateFormat == null))
				{
					bRet = false;
                    Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.IsValidDateStr failed, a parameter was null.");
				}
				else if ((i_sDate.Length != 8) || (i_sDateFormat.Length != 8))
				{
					bRet = false;
                    Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.IsValidDateStr failed, a parameter was not of the correct length.");
				}
				else if (i_sDate.IndexOf('?') >= 0)
				{
					bRet = false;
                    Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.IsValidDateStr failed, Date contained a question mark.");
				}
				else if (i_sDateFormat == Formats.m_csDate_YYYYMMDD)
				{
					sYear = i_sDate.Substring(0, 4);
					sMonth = i_sDate.Substring(4, 2);
					sDay = i_sDate.Substring(6, 2);

					bRet = IsValidDateComponents(sYear, sMonth, sDay);
				}
				else if (i_sDateFormat == Formats.m_csDate_MMDDYYYY)
				{
					sYear = i_sDate.Substring(4, 4);
					sMonth = i_sDate.Substring(0, 2);
					sDay = i_sDate.Substring(2, 2);

					bRet = IsValidDateComponents(sYear, sMonth, sDay);
				}
				else if (i_sDateFormat == Formats.m_csDate_DDMMYYYY)
				{
					sYear = i_sDate.Substring(4, 4);
					sMonth = i_sDate.Substring(2, 2);
					sDay = i_sDate.Substring(0, 2);

					bRet = IsValidDateComponents(sYear, sMonth, sDay);
				}
				else
				{
					bRet = false;
				}
			}
			catch (Exception exc)
			{
				bRet = false;
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.IsValidDateStr exception: " + exc.ToString());
			}

			return (bRet);
		} // IsValidDateStr

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sDate"></param>
		/// <returns></returns>
//		public static string GetDateFormat(string i_sDate)
		public string GetDateFormat(string i_sDate)
		{
			string sRet = Errors.m_csError;

			if (IsValidDateStr(i_sDate, Formats.m_csDate_YYYYMMDD))
			{
				sRet = Formats.m_csDate_YYYYMMDD;
			}
			else if (IsValidDateStr(i_sDate, Formats.m_csDate_MMDDYYYY))
			{
				sRet = Formats.m_csDate_MMDDYYYY;
			}
			else
			{
				sRet = Errors.m_csError;
			}

			return (sRet);
		} // GetDateFormat

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sLocale"></param>
		/// <param name="i_sDate"></param>
		/// <returns></returns>
//		public static string GetSpeakableDate(string i_sLocale, string i_sDate)
		public string GetSpeakableDate(string i_sLocale, string i_sDate)
		{
			string				sRet = "";
			DateTime			dtDate = DateTime.MinValue;

			dtDate = GetDatetimeFromString(i_sDate);
			sRet = GetSpeakableDate(i_sLocale, dtDate);

			return (sRet);
		} // GetSpeakableDate

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i_sLocale"></param>
		/// <param name="i_dtDate"></param>
		/// <returns></returns>
		public string GetSpeakableDate(string i_sLocale, DateTime i_dtDate)
		{
			string				sRet = "";
			CultureInfo			ciOld = null, ciNew = null;
			int					iDOW = -1;

			ciOld = Thread.CurrentThread.CurrentCulture;

			// Set CurrentCulture
			try
			{
				ciNew = new CultureInfo(i_sLocale);
			}
			catch (ArgumentException exc)
			{
				// Bad locale string
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.GetSpeakableDate(i_sLocale) exception creating the new CI: " + exc.ToString());
				ciNew = ciOld;
			}
			try
			{
				Thread.CurrentThread.CurrentCulture = ciNew;
			}
			catch (Exception exc)
			{
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.GetSpeakableDate(i_sLocale) exception setting the new CI: " + exc.ToString());
			}

			sRet = i_dtDate.ToLongDateString();

			iDOW = sRet.IndexOf(',');
			if (iDOW >= 0)
			{
				sRet = sRet.Remove(0, (iDOW + 2));
			}

			// Reset CurrentCulture
			try
			{
				Thread.CurrentThread.CurrentCulture = ciOld;
			}
			catch (Exception exc)
			{
                Console.Error.WriteLine(DateTime.Now.ToString() + " DateFormatter.GetSpeakableDate(i_sLocale) exception resetting to the old CI: " + exc.ToString());
			}

			return (sRet);
		} // GetSpeakableDate

		/// <summary>
		/// Uses the default system locale.
		/// </summary>
		/// <param name="i_dtDate"></param>
		/// <returns></returns>
/*		public string GetSpeakableDate(DateTime i_dtDate)
		{
			string				sRet = "", sLocale = "";

			sLocale = Thread.CurrentThread.CurrentCulture.Name;
			sRet = GetSpeakableDate(sLocale, i_dtDate);

			return (sRet);
		} // GetSpeakableDate*/

		/// <summary>
		/// Uses the default system locale.
		/// </summary>
		/// <param name="i_sDate"></param>
		/// <returns></returns>
		public string GetSpeakableDate(string i_sDate)
		{
			string sRet = "", sLocale = "";

			sLocale = Thread.CurrentThread.CurrentCulture.Name;
			sRet = GetSpeakableDate(sLocale, i_sDate);

			return (sRet);
		} // GetSpeakableDate

	} // class DateFormatter
} // namespace Incendonet.Utilities.Strings
