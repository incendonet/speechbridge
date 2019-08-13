// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;

namespace SBWinSvc
{
	public class WinSvc : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private Process	m_pLocalRM = null;

		public WinSvc()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
		}

		// The main entry point for the process
		static void Main()
		{
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			// More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new WinSvc(), new MySecondUserService()};
			//
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new WinSvc() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "SBWinSvc";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			bool	bRes = true;

			// Start SBLocalRM with "--console" argument
			try
			{
//Debugger.Launch();
				m_pLocalRM = new Process();
				m_pLocalRM.StartInfo.FileName = ConfigurationManager.AppSettings["LocalrmBinPath"];;
				m_pLocalRM.StartInfo.Arguments = "--console";
				bRes = m_pLocalRM.Start();
				if(!bRes)
				{
					Console.Error.WriteLine("SBWinSvc.OnStart Couldn't start SBLocalRM!");
				}
			}
			catch(Exception exc)
			{
				Console.Error.WriteLine("SBWinSvc.OnStart Exception: ", exc.ToString());
			}
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			Process[]	apProc = null;

//Debugger.Launch();
			// Kill SBLocalRM and processes that it didn't clean up.
			try
			{
				if(m_pLocalRM != null)
				{
					m_pLocalRM.Kill();
				}
			}
			catch {}

			// FIX - read list of processes from config file
			apProc = Process.GetProcessesByName("AudioMgr");
			foreach(Process pProc in apProc)
			{
				try
				{
					pProc.Kill();
				}
				catch {}
			}

			apProc = Process.GetProcessesByName("DialogMgr");
			foreach(Process pProc in apProc)
			{
				try
				{
					pProc.Kill();
				}
				catch {}
			}
		}
	}
}
