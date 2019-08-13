// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

//
// SBLauncher.cpp
//
// $Id: //poco/1.4/Foundation/samples/SBLauncher/src/SBLauncher.cpp#1 $
//
//
// Usage:  SBLauncher NumPorts BinDir ConfigDir App1 [App2] [...]

#define LOG_TO_CONSOLE					1


#include "Poco/AutoPtr.h"
#include "Poco/ConsoleChannel.h"
#include "Poco/SplitterChannel.h"
#include "Poco/FileChannel.h"
#include "Poco/PatternFormatter.h"
#include "Poco/FormattingChannel.h"
#include "Poco/Message.h"
#include "Poco/Logger.h"
#include "Poco/PipeStream.h"
#include "Poco/StreamCopier.h"
#include <iostream>
#include <sstream>
#include <fstream>
#include <vector>
#include <cstdio>

#include "AppInfo.h"
#include "IAppLauncherThread.h"
#include "ALTManager.h"

using std::string;
using std::vector;
using std::cerr;
using std::cout;
using std::endl;

using Poco::AutoPtr;
using Poco::Channel;
using Poco::ConsoleChannel;
using Poco::SplitterChannel;
using Poco::FileChannel;
using Poco::FormattingChannel;
using Poco::Formatter;
using Poco::PatternFormatter;
using Poco::Logger;
using Poco::Message;

// Global contstants
static const int g_iNumPortsIdx =		1;
static const int g_iBinDirIdx =			2;
static const int g_iConfigDir =			3;
static const int g_iLogDir =			4;
static const int g_iFirstApp =			5;

// Function prototypes
void PullApps(int i_iArgs, char **i_ppsArgs, int i_iFirstAppIdx, StringVector& o_vApps);


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
int main (int i_iArgc, char** i_ppsArgv)
{
	int							iRet = 0;
	int							iNumPorts = 1;
	string						sBinDir = "/opt/speechbridge/bin/";
	string						sConfigDir = "/opt/speechbridge/config/";
	string						sLogDir = "/opt/speechbridge/logs/";
	StringVector				vsApps;
	ALTManager					oMgr;

	if(i_iArgc < (g_iFirstApp + 1))
	{
		cerr << "Invalid number of parameters." << endl;
		cout << "Usage:  sblauncher NumPorts BinDir ConfigDir LogDir App1 [App2] [...]" << endl;
		//cout << "  Directory names must end in the OS specific directory delimiter (eg., '/' on linux or '\\' on Windows.)" << endl;
		iRet = 1;
	}
	else
	{
		// Get the arguments
		iNumPorts =		atoi(i_ppsArgv[g_iNumPortsIdx]);
		sBinDir =		i_ppsArgv[g_iBinDirIdx];
		sConfigDir =	i_ppsArgv[g_iConfigDir];
		sLogDir =		i_ppsArgv[g_iLogDir];
		PullApps(i_iArgc, i_ppsArgv, g_iFirstApp, vsApps);

		// Set up the logger
		AutoPtr<SplitterChannel>	splitterChannel(new SplitterChannel());
#if LOG_TO_CONSOLE
		AutoPtr<Channel>			consoleChannel(new ConsoleChannel(std::cout));
#endif
		AutoPtr<FileChannel>		rotatedFileChannel(new FileChannel(string(sLogDir) + "/SBLauncherLog.log.txt"));

		rotatedFileChannel->setProperty("rotation", "5 M");
		//rotatedFileChannel->setProperty("archive", "timestamp");		// Combined with rotation setting, creates a new file every __ MB.
		rotatedFileChannel->setProperty("times", "local");

#if LOG_TO_CONSOLE
		splitterChannel->addChannel(consoleChannel);
#endif
		splitterChannel->addChannel(rotatedFileChannel);
	
		AutoPtr<Formatter>			formatter(new PatternFormatter("%L %Y-%m-%d %h:%M:%S.%i: %t"));
		AutoPtr<Channel>			formattingChannel(new FormattingChannel(formatter, splitterChannel));
		Logger& logger = Logger::create("SBLauncher", formattingChannel, Message::PRIO_TRACE);

		logger.information(string("SBLauncher v") + string(AssemblyVersion));

		// Initialize and start the thread Manager
		oMgr.Init(&logger, iNumPorts, &vsApps, sBinDir, sConfigDir);
		oMgr.Run();

		logger.information("SBLauncher exiting.");
	}

	return(iRet);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void PullApps(int i_iArgs, char **i_ppsArgs, int i_iFirstAppIdx, StringVector& o_vApps)
{
	int		ii = 0;

	for(ii = i_iFirstAppIdx; ii < i_iArgs; ii++)
	{
		o_vApps.push_back(i_ppsArgs[ii]);
	}
}
