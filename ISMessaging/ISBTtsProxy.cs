using System;

namespace ISMessaging
{
	public interface ISBTtsProxy
	{
		int		ID { get; }

		bool	Init();
		bool	Release();
		bool	TextToWav(string i_sText, string i_sWavFName);
	}
}
