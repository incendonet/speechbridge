// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SBWinSvc
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller SBWinSVcServiceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller SBWinSvcServiceInstaller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SBWinSVcServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.SBWinSvcServiceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// SBWinSVcServiceProcessInstaller
			// 
			//this.SBWinSVcServiceProcessInstaller.Password = null;
			//this.SBWinSVcServiceProcessInstaller.Username = null;
			this.SBWinSVcServiceProcessInstaller.Account = ServiceAccount.LocalSystem;
			// 
			// SBWinSvcServiceInstaller
			// 
			this.SBWinSvcServiceInstaller.ServiceName = "SBWinSvc";
			this.SBWinSvcServiceInstaller.DisplayName = "SpeechBridge Process Manager";
			this.SBWinSvcServiceInstaller.StartType = ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(
				new System.Configuration.Install.Installer[]
				{
					this.SBWinSVcServiceProcessInstaller,
					this.SBWinSvcServiceInstaller
				}
			);

		}
		#endregion
	}
}
