// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Incendonet.Utilities.LogClient;

namespace sbtest.Logging
{
	public class LoggingTests
	{
		public static void Run()
		{
			int			iThreads = 0, iIter = 0, iSleep = 0;
			string		sFname = "";

			Console.WriteLine();

			iThreads = GetNumThreads();
			iIter = GetNumIterations();
			iSleep = GetSleep();
			sFname = GetPath();

			TestMultipleThreads(iThreads, iIter, iSleep, sFname);

			Console.WriteLine("Done!");
		}

		public static int GetNumThreads()
		{
			int			iRet = 0;

			Console.Write("How many threads? ");
			iRet = int.Parse(Console.ReadLine().Trim());

			return (iRet);
		}

		public static int GetSleep()
		{
			int			iRet = 0;

			Console.Write("How many msec should each thread sleep between iterations? ");
			iRet = int.Parse(Console.ReadLine().Trim());

			return (iRet);
		}

		public static int GetNumIterations()
		{
			int iRet = 0;

			Console.Write("How many iterations per thread? ");
			iRet = int.Parse(Console.ReadLine().Trim());

			return (iRet);
		}

		public static string GetPath()
		{
			Console.Write("What is the full path (no filename) for the log file? ");
			return (Console.ReadLine());
		}

		public static void TestMultipleThreads(int i_iNumThreads, int i_iNumIterations, int i_iSleep, string i_sPath)
		{
			Console.WriteLine("Starting test on {0} threads with {1} iterations to '{2}'...", i_iNumThreads, i_iNumIterations, i_sPath);

			ILegacyLogger		log = new LegacyLogger();
			Thread[]			aThreads = new Thread[i_iNumThreads];
			LoggingThread[]		aLTs = new LoggingThread[i_iNumThreads];
			string				sThreadName = Thread.CurrentThread.Name = "LoggingTestsT";
//			string				sThreadName = Thread.CurrentThread.Name = "";

//			log.Init("", "", sThreadName, "", "LogTester", i_sPath);
			log.Init("", "", sThreadName, "", "", i_sPath);			// Let logger look up the component name
			log.Open();

			log.Log(Level.Debug, "Starting threads...");

			for(int ii = 0; ii < i_iNumThreads; ii++)
			{
				aLTs[ii] = new LoggingThread(log, ii, i_iNumIterations, i_iSleep);
				aThreads[ii] = new Thread(new ThreadStart(aLTs[ii].ThreadProc));
				aThreads[ii].Start();
			}

			foreach (Thread tThread in aThreads)
			{
				tThread.Join();
			}

			log.Log(Level.Debug, "Done.");

			log.Close();
		}
	} // class LoggingTests

	public class LoggingThread
	{
		int				m_iIndex = 0, m_iNumIterations = 0, m_iSleep = 0;
		ILegacyLogger	m_Log = null;

		public LoggingThread(ILegacyLogger i_Log, int i_iThreadIndex, int i_iNumIterations, int i_iSleep)
		{
			m_Log =				i_Log;
			m_iIndex =			i_iThreadIndex;
			m_iNumIterations =	i_iNumIterations;
			m_iSleep =			i_iSleep;
		}

		public void ThreadProc()
		{
			string			sThreadName = Thread.CurrentThread.Name = m_iIndex.ToString();

//			m_Log.Init("", "", m_iIndex.ToString(), "", "LoggingThread", "");
//			m_Log.Init("", "", sThreadName, "VMC-" + sThreadName, "LoggingThread", "");
			m_Log.Init("", "", sThreadName, "VMC-" + sThreadName, "", "");			// Let logger look up the component name

			for (int ii = 0; ii < m_iNumIterations; ii++)
			{
				m_Log.Log(Level.Info, string.Format("Thread #{0}, iteration {1}.", sThreadName, ii));
				if (m_iSleep > 0)
				{
					Thread.Sleep(m_iSleep);
				}
			}
		}
	} // class LoggingThread
}
