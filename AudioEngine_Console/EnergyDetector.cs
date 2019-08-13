// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;

namespace AudioMgr
{
	/// <summary>
	/// Summary description for EnergyDetector.
	/// </summary>
	public class EnergyDetector
	{
        public enum eResult
        {
            ERROR = -1,
            UNDETERMINED = 0,
            ENERGY,
            SILENCE,
        };
        
        private class PrevResults
		{
			public byte[]	m_abBuf = null;			// Buffer used to detect speech
			public byte[]	m_abPrevBuf = null;		// Previous buffer saved in case of speech detect. (Size can be variable.)
			public eResult	m_eResult = eResult.UNDETERMINED;
			public Int32	m_iNumHits = 0;
			public Int32	m_iNumTrailingSilenceBytes = 0;

			public PrevResults()
			{
				Set(eResult.UNDETERMINED, 0, 0);
			}

			public void Set(eResult i_eResult, Int32 i_iNumHits, Int32 i_iNumTrailingSilenceBytes)
			{
				m_eResult = i_eResult;
				m_iNumHits = i_iNumHits;
				m_iNumTrailingSilenceBytes = i_iNumTrailingSilenceBytes;
			}

			public void Clear()
			{
				ISMessaging.Audio.ISMRawData.MuClear(m_abBuf);
				ISMessaging.Audio.ISMRawData.MuClear(m_abPrevBuf);
			}
		}

		public const Int16 DEFAULT_NOISE_LEVEL = 1600;                                      // On the 0..32767 scale.

        public const Int32 FRAME_SIZE_1SEC = 8000;                                          // 1 second == 8000 samples.
        public const Int32 FRAME_TIME_MIN = 250;                                            // Can't work on a data frame smaller than 1/4 second (250 ms).
        public const Int32 FRAME_SIZE_MIN = FRAME_SIZE_1SEC * FRAME_TIME_MIN / 1000;		// Can't work on a data frame smaller than 1/4 second.

		private static	Int32[]	m_aiExpLut = { 0, 132, 396, 924, 1980, 4092, 8316, 16764 };		// Used by MuLawToPCM16.
		private			Int16[]	m_aiPcmData;	// Used to hold converted data.

		private			Int16	m_wNoiseLevel = DEFAULT_NOISE_LEVEL;		// Valid range 0..32767

		private			Int32	m_iEndpointInBytes;		// Amount of silence to wait for before determining end-of-speech.
		private	const	Int32	HITS_FOR_DETECT = 200;	// Number of hits to determine that there has been speech energy.	// FIX - This should be configurable from VoiceXML

		private			byte[]	m_abCurrBuf = null;		// Current buffer.
		private			PrevResults	m_PRes = null;		// Previous buffer and related data.


        /// <param name="i_wNoiseLevel">16-bit value for noise level.  Default is 1600.</param>
        /// <param name="i_iEndpointInMilliSeconds">Amount of trailing silence in miliseconds before determining end of speech.</param>
        public EnergyDetector(Int16 i_wNoiseLevel, Int32 i_iEndpointInMilliSeconds)
		{
            m_wNoiseLevel = i_wNoiseLevel;
            m_iEndpointInBytes = ConvertMilliSecondsToBytes(i_iEndpointInMilliSeconds);

			m_aiPcmData = new Int16[FRAME_SIZE_MIN];                                // Pre-alloc array for converting muLaw to PCM.

            m_PRes = new PrevResults();
            Reset();
		}

		/// <summary>
		/// Resets the ED to a clear state.
		/// </summary>
		public void Reset()
		{
			m_PRes.Set(eResult.SILENCE, 0, 0);
			m_PRes.Clear();

			m_abCurrBuf = null;
		}

        /// <summary>
        /// Set the noise level for speech recognition.
        /// </summary>
        /// <param name="i_wNoiseLevel">16-bit value for noise level.  Default is 1600.</param>
        public void SetNoiseLevel(Int16 i_wNoiseLevel)
        {
            // Clear energy detector since any statistics colleced are invalid once noise level is changed.

            Reset();

            m_wNoiseLevel = i_wNoiseLevel;
        }

        /// <summary>
        /// Set the endpoint (in ms) for speech recognition.
        /// </summary>
        /// <param name="i_iEndpointInMilliSeconds">Amount of trailing silence in miliseconds before determining end of speech.</param>
        public void SetEndpoint(int i_iEndpointInMilliSeconds)
        {
            // Clear energy detector since any statistics colleced are invalid once endpoint is changed.

            Reset();

            m_iEndpointInBytes = ConvertMilliSecondsToBytes(i_iEndpointInMilliSeconds);
        }

		public eResult Detect(byte[] i_abData)
		{
			if (m_abCurrBuf != null)
			{
				m_PRes.m_abPrevBuf = m_PRes.m_abBuf;
				m_PRes.m_abBuf = m_abCurrBuf;	// Save previous buffer.
			}

			m_abCurrBuf = i_abData;


            // Reallocate converter array if source isn't the same size (realisiticaly should only happen at most once).

			if (i_abData.Length > m_aiPcmData.Length)
			{
				m_aiPcmData = new Int16[i_abData.Length];
			}

			MuLawToPCM16(i_abData, i_abData.Length, m_aiPcmData);
			return(TimeDomainDetect(m_aiPcmData));
		}

		/// <summary>
		/// Returns any audio data which precedes the initial energy detect (in case intro of
		/// utterance is quiet.)  Caller may want to trim the result since this returns the entire
		/// previous buffer.
		/// </summary>
		public byte[] PreData
		{
			get
			{
				byte[] abRet = null;
				try
				{
					abRet = new byte[m_PRes.m_abPrevBuf.Length + m_PRes.m_abBuf.Length];
					m_PRes.m_abPrevBuf.CopyTo(abRet, 0);
					m_PRes.m_abBuf.CopyTo(abRet, m_PRes.m_abPrevBuf.Length);
				}
				catch (Exception exc)
				{
					Console.Error.WriteLine("EnergyDetector.PreData - Caught exception '{0}'!", exc);
				}

				return abRet;
			}
		}

		/// <summary>
		/// Simple time-domain based detector.  Perhaps not the best approach.
		/// If we search the entire 1 sec buffer with no energy above the noise level, then the
		/// user has completed speaking.
		/// </summary>
		/// <param name="i_abData">Array containing PCM audio data.</param>
		/// <returns></returns>
		public eResult TimeDomainDetect(Int16[] i_abData)
		{
			eResult		eRet = eResult.SILENCE;
			int			iDataLen;
			int			iNumHits = 0;

			try
			{
				iNumHits = m_PRes.m_iNumHits;			// Preset with previous results.  // FIX - This could result in false detects if the frame size is too large!  (Greater than 0.5 second?)
				iDataLen = i_abData.Length;
				if (iDataLen < FRAME_SIZE_MIN)			// Bail if we don't have a FRAME_SIZE_MIN of data.
				{
					eRet = eResult.ERROR;
					Console.Error.WriteLine("ERROR: Not enough data to work on: {0} bytes available, but need {1} bytes.", iDataLen, FRAME_SIZE_MIN);
				}
				else
				{
                    bool bFound = false;
                    
                    for (int ii = 0; ((ii < iDataLen) && (bFound == false)); ++ii)
					{
						if (i_abData[ii] > m_wNoiseLevel)
						{
							++iNumHits;

							if (iNumHits >= HITS_FOR_DETECT)
							{
								bFound = true;
								eRet = eResult.ENERGY;
							}
						}
					} // for
				} // else

				// First subtract out previous hits before updating.
				iNumHits -= m_PRes.m_iNumHits;

				// Update previous results data.
				switch (eRet)
				{
					case eResult.ENERGY:
					    m_PRes.Set(eRet, iNumHits, 0);
					    break;

					case eResult.SILENCE:
						// Doublecheck that we haven't hit the endpoint yet.
						if ( ((m_PRes.m_iNumTrailingSilenceBytes + m_abCurrBuf.Length) < m_iEndpointInBytes) && (m_PRes.m_eResult == eResult.ENERGY) )
						{
							// Not yet, so return this as energy.
							eRet = eResult.ENERGY;
							m_PRes.Set(eResult.ENERGY, iNumHits, (m_PRes.m_iNumTrailingSilenceBytes + m_abCurrBuf.Length));
						}
						else
						{
							m_PRes.Set(eResult.SILENCE, iNumHits, 0);
						}
					    break;

					default:
					    break;
				}
			}
			catch (Exception exc)
			{
				eRet = eResult.UNDETERMINED;
				Console.Error.WriteLine("EnergyDetector.TimeDomainDetect() - Caught exception '{0}'!", exc);
			}

			return eRet;
		}

		/// <summary>
		/// This function is a derivation of the Sox function st_ulaw_to_linear().
		/// See http://sox.sourceforge.net/ for more details.
		/// 
		/// The above must not be correct, as Sox does not contain this code.
		/// </summary>
		/// <param name="i_abMuLawData">Array containing mu-Law audio data.</param>
		/// <param name="i_lNumSamples">Must be &lt;= i_abMuLawData.Length</param>
		/// <param name="io_ai16BitData">Array containing resultant 16-bit PCM data.</param>
		/// <returns></returns>
		public static void MuLawToPCM16(byte[] i_abMuLawData, Int32 i_lNumSamples, Int16[] io_ai16BitData)
		{
            try
			{
				// First check parameters
				if (i_abMuLawData.Length < i_lNumSamples)
				{
					Console.Error.WriteLine("EnergyDetector.MuLawToPCM16() - i_lNumSamples cannot be larger than i_abMuLawData.Length.");
				}
				else if (io_ai16BitData.Length < i_abMuLawData.Length)
				{
					Console.Error.WriteLine("EnergyDetector.MuLawToPCM16() - Destination cannot be smaller than the source.");
				}
				else if (i_lNumSamples <= 0)
				{
					Console.Error.WriteLine("EnergyDetector.MuLawToPCM16() - Invalid number of samples.");
				}
				else
				{
                    Int32 iSign, iExponent, iMantissa, iSample;
                    byte bMuLawByte;

                    // Do the mapping/conversion
					for (int ii = 0; ii < i_lNumSamples; ++ii)
					{
						bMuLawByte = i_abMuLawData[ii];
						bMuLawByte = (byte)(~((UInt16)bMuLawByte));	// Need to "& 0xFF" the result? // FIX - double check that the conversions worked properly.	// Conversion hoops are because '~' isn't defined for the byte type.
						iSign = (bMuLawByte & 0x80);
						iExponent = (bMuLawByte >> 4) & 0x07;
						iMantissa = bMuLawByte & 0x0F;
						iSample = m_aiExpLut[iExponent] + (iMantissa << (iExponent + 3));

						if (iSign != 0)
						{
							iSample = -iSample;
						}

						io_ai16BitData[ii] = (Int16)iSample;
					}
				}
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("EnergyDetector.MuLawToPCM16() - Caught exception '{0}'!", exc);
			}

			return;
		}

        private int ConvertMilliSecondsToBytes(int i_iValueInMilliSeconds)
        {
           return ((i_iValueInMilliSeconds * FRAME_SIZE_1SEC) / 1000);
        }
	}
}
