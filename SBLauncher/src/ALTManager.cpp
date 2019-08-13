// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
#include "Poco/Logger.h"
#include "Poco/Process.h"
#include <stdio.h>
#include <iostream>
#include <sstream>
#include <vector>

#include "IAppLauncherThread.h"
#include "AudiortrLauncherThread.h"
#include "ALTManager.h"
#include "AppTypes.h"

using std::string;
using std::ostringstream;
using Poco::Logger;
using Poco::Process;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ALTManager::ALTManager(void)
{
	m_pLogger = NULL;
	m_nPorts = 0;
	m_pvsApps = NULL;
	m_nApps = 0;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
ALTManager::~ALTManager(void)
{
	int			ii = 0;

	for(ii = 0; ii < m_nPorts; ii++)
	{
		if(m_vpALTs[ii] != NULL)
		{
			delete m_vpALTs[ii];
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ALTManager::Init(Logger *i_pLogger, int i_nPorts, StringVector *i_pvsApps, string& i_sBinDir, string& i_sConfigDir)
{
	m_pLogger =		i_pLogger;
	m_nPorts =		i_nPorts;
	m_pvsApps =		i_pvsApps;
	m_nApps =		m_pvsApps->size();
	m_sBinDir =		i_sBinDir;
	m_sConfigDir =	i_sConfigDir;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
int ALTManager::Run()
{
	int			iRet = 0;
	bool		bRes = true;

	bRes = StartThreads();

	return(iRet);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
bool ALTManager::StartThreads()
{
	bool			bRet = true;
	int				ii = 0, iNumThreads = 0;
	bool			bFoundAudiortr = false;
	string			sTmp;
	ostringstream	ossTmp;
	bool			bRes = true;

	if( (m_nPorts <= 0) || (m_nApps <= 0) )
	{
		bRet = false;
		m_pLogger->critical("An invalid number of ports or applications was specified.");
	}
	else
	{
		// Loop through apps, ticking off known apps
		for(ii = 0; ii < m_nApps; ii++)
		{
			if((*m_pvsApps)[ii].compare(g_sAppTypeAudioRtr) == 0)
			{
				bFoundAudiortr = true;
			}
			else
			{
				sTmp = "Cannot start unknown app type:  '" + (*m_pvsApps)[ii] + "'";
				m_pLogger->warning(sTmp);
			}
		}

		// Start launching apps

		// AudioRtr should be last
		if(bFoundAudiortr)
		{
			m_pLogger->information("About to start AudioRtr threads.");
			bRes = StartAudiortrThreads();
		}

		// Wait for all threads to complete.
		iNumThreads = m_vpThreads.size();
		for(ii = 0; ii < iNumThreads; ii++)
		{
			m_vpThreads[ii]->join();
			ossTmp << "Joined thread: " << ii;
			m_pLogger->information(ossTmp.str());
		}
	}

	return(bRet);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
bool ALTManager::StartAudiortrThreads()
{
	bool			bRet = true;
	int				ii = 0;
	char			csTmp1[32], csTmp2[32];
	StringVector	vsArgs;
	string			sTmp1, sTmp2, sTmp3;

	try
	{
		for(ii = 0; ii < m_nPorts; ii++)
		{
			m_vpThreads.push_back(new Thread);
			m_vpALTs.push_back(new AudiortrLauncherThread());
			sprintf(csTmp1, "AudioRtr[%d]", ii);
			sprintf(csTmp2, "/%d.cfg", ii);
			vsArgs.push_back("-f");
			vsArgs.push_back(m_sConfigDir + string(csTmp2));
			sTmp1 = string(csTmp1);
			sTmp2 = string(csTmp2);
			sTmp3 = m_sBinDir + string("/AudioRtr");

			m_vpALTs[ii]->Init(m_pLogger, sTmp1, sTmp3, vsArgs);
			m_vpThreads[ii]->start(*(m_vpALTs[ii]));

			vsArgs.clear();		// Note that vsArgs is not passed as a reference, so a copy is made for every instance.
		}
	}
	catch(Poco::Exception& exc)
	{
		bRet = false;
		m_pLogger->error("ALTMgr::StartAudiortrThreads Exception: " + exc.displayText());
	}
	catch(...)
	{
		bRet = false;
		m_pLogger->error("ALTMgr::StartAudiortrThreads Unknown exception");
	}

	return(bRet);
}
