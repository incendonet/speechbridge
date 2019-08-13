// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
#pragma once

using Poco::Logger;
using Poco::Thread;


class ALTManager
{
private:
	Logger							*m_pLogger;
	int								m_nPorts;
	StringVector					*m_pvsApps;
	int								m_nApps;
	string							m_sBinDir;
	string							m_sConfigDir;
	vector<Thread *>				m_vpThreads;
	vector<IAppLauncherThread *>	m_vpALTs;

	bool	StartThreads();
	bool	StartAudiortrThreads();

public:
	ALTManager(void);
	virtual ~ALTManager(void);

	void	Init(Logger *i_pLogger, int i_nPorts, StringVector *i_pvApps, string& i_sBinDir, string& i_sConfigDir);
	int		Run();
};

