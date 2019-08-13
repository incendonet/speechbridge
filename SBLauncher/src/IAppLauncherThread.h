// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
#pragma once

#include "Poco/Logger.h"
#include "Poco/Thread.h"
#include "Poco/Runnable.h"
#include <iostream>
#include <vector>

using std::string;
using std::vector;
using Poco::Logger;

typedef vector<string> StringVector;


class IAppLauncherThread : public Poco::Runnable
{
public:

	string				m_sName;				// A descriptive name of the app
	string				m_sExeFName;			// The full pathname of the app
	vector<string>		m_vArgs;				// The arguments to be passed to the app

	virtual ~IAppLauncherThread(void)	{}

	virtual void	Init(Logger *i_pLogger, string& i_sName, string& i_sExeFName, StringVector i_vArgs) = 0;	// Note that the StringVector is not passed by reference, so a copy will be made.
	virtual void	run() = 0;
};
