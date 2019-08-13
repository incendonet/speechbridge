#pragma once

#include "IAppLauncherThread.h"

class AudiortrLauncherThread :
	public IAppLauncherThread
{
public:
	AudiortrLauncherThread(void);
	virtual ~AudiortrLauncherThread(void);

	void	Init(string& i_sName, string& i_sExeFName, vector<string>& i_vArgs);
	void	run();

};

