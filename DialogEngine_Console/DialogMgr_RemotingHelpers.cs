﻿// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Runtime.Remoting;

using ISMessaging;


using SymanticResult = System.String;			// A "visual crutch" to make code easier to read.  FIX - multiple declaration!!!  (see ASRFacade.cs)

namespace DialogMgr_Console
{
	public class DMEvHook
	{
		private static ISMessaging.Delivery.ISMDistributer m_Distr;

		public static ISMessaging.Delivery.ISMDistributer MsgDistr
		{
			// FIX - The accessors should be thread syncronized.
			get
			{
				return (m_Distr);
			}
			set
			{
				m_Distr = value;
			}
		}
	}

	public class DMMessaging : ISMessaging.Delivery.ISMReceiverImpl	// Is this neccessary for hosting an assembly for remoting?
	{
		public override bool Ping()
		//public new bool Ping()
		{
			Console.WriteLine("Pinged!");
			return (true);
		}

		public override bool NewMsg(ISMsg i_Msg)
		{
			bool bRet = true;

			try
			{
				DMEvHook.MsgDistr.Send(i_Msg);
			}
			catch
			{
				bRet = false;
			}

			return (bRet);
		}
	}

	/// <summary>
	/// Kind of a kludge to get a valid type to use in GetObject().  Also tried to put it inside
	/// DialogMgr_srv (and private), but the GetObject() failed.
	/// </summary>
	public interface DMPinger : ISMessaging.Delivery.IISMReceiver
	{
	}

}