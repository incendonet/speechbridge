// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
#include "Poco/Logger.h"
#include "Poco/Process.h"
#include "Poco/Path.h"
#include "Poco/File.h"
#include <iostream>
#include <sstream>
#include <vector>

#include "IAppLauncherThread.h"
#include "AudiortrLauncherThread.h"

using std::string;
using std::vector;
using std::ostringstream;
using Poco::Logger;
using Poco::Process;
using Poco::ProcessHandle;
using Poco::Path;
using Poco::File;


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
AudiortrLauncherThread::AudiortrLauncherThread(void)
{
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
AudiortrLauncherThread::~AudiortrLauncherThread(void)
{
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Note that the StringVector is not passed by reference.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void AudiortrLauncherThread::Init(Logger *i_pLogger, string& i_sName, string& i_sExeFName, StringVector i_vsArgs)
{
	m_pLogger =		i_pLogger;
	m_sName =		i_sName;
	m_sExeFName =	i_sExeFName;
	m_vsArgs =		i_vsArgs;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void AudiortrLauncherThread::run()
{
	bool					bDone = false;
	string					sErr, sTmp;
	int						ii = 0, iNumArgs = 0;
	ostringstream			ossTmp;
	bool					bFound = false;
	Path					oPath;
	File					oFile;

	m_pLogger->information("ALT::run Checking for: " + m_sExeFName);

	// Check to see if the executable is there
	oPath = m_sExeFName;
	if(oPath.isFile())
	{
		oFile = oPath;
		if(oFile.canExecute())
		{
			bFound = true;
		}
		else
		{
			m_pLogger->critical("ALT::run Not an excutable file: " + m_sExeFName);
		}
	}
	else
	{
		m_pLogger->critical("ALT::run File does not exist: " + m_sExeFName);
	}
	
	while(bFound && !bDone)
	{
		try
		{
			// Set up the args
			sTmp = "ALT::run About to launch: " + m_sExeFName + " ";
			iNumArgs = m_vsArgs.size();
			for(ii = 0; ii < iNumArgs; ii++)
			{
				sTmp += m_vsArgs[ii] + " ";
			}
			m_pLogger->information(sTmp);

			// Launch the process
			ProcessHandle	hProc = Process::launch(m_sExeFName, m_vsArgs);		// Is this declaration going to leak?  It's the only way to 'launch'.
			ossTmp << "Process id: " << hProc.id();
			sTmp = ossTmp.str();
			m_pLogger->information(sTmp);
			ossTmp.str("");

			hProc.wait();
			m_pLogger->information("ALT::run Process exited: " + m_sName);
		}
		catch(Poco::Exception& exc)
		{
			m_pLogger->error("ALT::run Process couldn't start: " + exc.displayText());
			bDone = true;			// Should we sleep and try again rather than exiting?
		}
	}

	m_pLogger->information("Exiting AudiortrLauncherThread::run");
}
