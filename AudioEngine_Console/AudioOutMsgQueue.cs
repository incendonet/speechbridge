// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using ISMessaging;

namespace AudioMgr
{
	/// <summary>
	/// Inherits MsgQueue to provide AudioOut specific functions
	/// </summary>
	public class AudioOutMsgQueue : MsgQueue
	{
		public AudioOutMsgQueue(int i_iIndex) : base(i_iIndex)
		{
		}

		public int ClearTopPrompts()
		{
			int					iRet = 0;
			ISMessaging.ISMsg	mMsg;

			try
			{
				Monitor.Enter(this);

				mMsg = (ISMsg)(m_alMsgs[0]);
				while( (mMsg != null) && (mMsg.GetType().ToString() == "ISMessaging.Audio.ISMPlayPrompts") )
				{
					m_alMsgs.RemoveAt(0);
					mMsg = (ISMsg)(m_alMsgs[0]);
					iRet++;
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine("[" + DateTime.Now.ToString() + "][" + m_iIndex.ToString() + "]" + "AudioOutMsgQueue.ClearTopPrompts: " + exc.ToString());
			}
			finally
			{
				Monitor.Exit(this);
			}

			return(iRet);
		} // ClearTopPrompts
	} // class AudioOutMsgQueue
}
