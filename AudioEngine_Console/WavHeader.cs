// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMgr
{
	// WAV file header.  (Not as flexible as the standard, but good enough for here.)
	// FIX - Find a way to use custom serialization so that this doesn't have to be done (painfully) by hand.
	[Serializable()]
	public class WavHeader// : ISerializable
	{
		public enum WaveFormat		// A subset, taken from MMReg.h
		{
			UNKNOWN =			0x0000,
			PCM =				0x0001,		// With PCM, there are supposedly "no other fields".  (Does this mean that the 'data' chunk is immediately after the 'fmt ' tag?)
			ALAW =				0x0006,
			MULAW =				0x0007,
			G723ADPCM =			0x0014,
			G721ADPCM =			0x0040,
			G728CELP =			0x0041,
			MPEG =				0x0050,
			MP3 =				0x0055,
			G723LUCENT =		0x0059,
			G726ADPCM =			0x0064,
			G722ADPCM =			0x0065,
			G729A =				0x0083,
			G726DF =			0x0085,
			GSM610DF =			0x0086,
			G723MEDIASONIC =	0x0093,
			G723VIVO =			0x0111,
			G723DIGITAL =		0x0123,
			G729SIPROLAB =		0x0133,
			G729ASIPROLAB =		0x0134,
			G726ADPCMDICT =		0x0140,
			GSMTUB =			0x0155,
			ADPCMNAP =			0x0170,
			MULAWNAP =			0x0171,
			ALAWNAP =			0x0172,
			ADPCMCREATIVE =		0x0200,
		}

		//[NonSerialized] public static int		WAVHEADERSIZE = 46;
		[NonSerialized] public static int		WAVHEADERSIZE = 58;		// To include optional 'fact'.
		[NonSerialized] public static int[]		m_iIndexes = {0, 4, 8, 12, 16, 20, 22, 24, 28, 32, 34, 36, 38, 42, (int)WAVHEADERSIZE};

		//public char[]		m_caGroupID = {(char)0, (char)0, (char)0, (char)0};			// Should be "RIFF"
		public char			m_caGroupID_0 = (char)0;
		public char			m_caGroupID_1 = (char)0;
		public char			m_caGroupID_2 = (char)0;
		public char			m_caGroupID_3 = (char)0;
		public UInt32		m_dwFileSize = 0;
		//public char[]		m_caRiffType = {(char)0, (char)0, (char)0, (char)0};			// Should be "WAVE"
		public char			m_caRiffType_0 = (char)0;
		public char			m_caRiffType_1 = (char)0;
		public char			m_caRiffType_2 = (char)0;
		public char			m_caRiffType_3 = (char)0;
		//public char[]		m_caFormatChunkID = {(char)0, (char)0, (char)0, (char)0};		// Should be "fmt "
		public char			m_caFormatChunkID_0 = (char)0;
		public char			m_caFormatChunkID_1 = (char)0;
		public char			m_caFormatChunkID_2 = (char)0;
		public char			m_caFormatChunkID_3 = (char)0;
		public UInt32		m_dwFormatChunkSize = 18;
		//public WaveFormat	m_wFormatTag = WaveFormat.UNKNOWN;		// Switched to UInt16 because WaveFormat doesn't serialize to a UInt16
		public UInt16		m_wFormatTag = (UInt16)WaveFormat.UNKNOWN;
		public UInt16		m_wChannels = 1;
		public UInt32		m_dwSamplesPerSec = 0;
		public UInt32		m_dwAvgBytesPerSec = 0;
		public UInt16		m_wBlockAlign = 1;				// Sample files had 1 rather than 0.  Why?
		public UInt16		m_wBitsPerSample = 0;
		public UInt16		m_wExtraInfoSize = 0;
		//public char[]		m_caDataChunkID = {(char)0, (char)0, (char)0, (char)0};		// Should be "data"
		public char			m_caDataChunkID_0 = (char)0;
		public char			m_caDataChunkID_1 = (char)0;
		public char			m_caDataChunkID_2 = (char)0;
		public char			m_caDataChunkID_3 = (char)0;
		public UInt32		m_dwDataChunkSize = 0;

		// Buffer in case there is extra info in the header (ie., the 'fact'.)
		public byte[]			m_abReserved1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

//		public void GetObjectData(SerializationInfo i_siInfo, StreamingContext i_scContext)
//		{
//			i_siInfo.AssemblyName = "";
//			i_siInfo.FullTypeName = "";
//		}

		public bool Set(Int32 i_lDataSize)
		{
			// FIX - Shouldn't always assume 64kb/s mu-law
			//m_caGroupID[0] = 'R'; m_caGroupID[1] = 'I'; m_caGroupID[2] = 'F'; m_caGroupID[3] = 'F';
			m_caGroupID_0 = 'R'; m_caGroupID_1 = 'I'; m_caGroupID_2 = 'F'; m_caGroupID_3 = 'F';
			m_dwFileSize = (uint)i_lDataSize + (uint)WAVHEADERSIZE;
			//m_caRiffType[0] = 'W'; m_caRiffType[1] = 'A'; m_caRiffType[2] = 'V'; m_caRiffType[3] = 'E';
			m_caRiffType_0 = 'W'; m_caRiffType_1 = 'A'; m_caRiffType_2 = 'V'; m_caRiffType_3 = 'E';
			//m_caFormatChunkID[0] = 'f'; m_caFormatChunkID[1] = 'm'; m_caFormatChunkID[2] = 't'; m_caFormatChunkID[3] = ' ';
			m_caFormatChunkID_0 = 'f'; m_caFormatChunkID_1 = 'm'; m_caFormatChunkID_2 = 't'; m_caFormatChunkID_3 = ' ';
			m_dwFormatChunkSize = 18;
			m_wFormatTag = (UInt16)WaveFormat.MULAW;
			m_wChannels = 1;
			m_dwSamplesPerSec = 8000;
			m_dwAvgBytesPerSec = 8000;
			m_wBlockAlign = 1;				// Sample files had 1 rather than 0.  Why?
			m_wBitsPerSample = 8;
			m_wExtraInfoSize = 0;
			//m_caDataChunkID[0] = 'd'; m_caDataChunkID[1] = 'a'; m_caDataChunkID[2] = 't'; m_caDataChunkID[3] = 'a';
			m_caDataChunkID_0 = 'd'; m_caDataChunkID_1 = 'a'; m_caDataChunkID_2 = 't'; m_caDataChunkID_3 = 'a';
			m_dwDataChunkSize = (uint)i_lDataSize;

			return(true);
		}

		public bool Extract(byte[] i_Data)
		{
			bool	bRet = true;
			int		iOffset = 0;

			m_caGroupID_0 = (char)i_Data[m_iIndexes[0]];
			m_caGroupID_1 = (char)i_Data[m_iIndexes[0] + 1];
			m_caGroupID_2 = (char)i_Data[m_iIndexes[0] + 2];
			m_caGroupID_3 = (char)i_Data[m_iIndexes[0] + 3];

			if( (m_caGroupID_0 != 'R') || (m_caGroupID_1 != 'I') || (m_caGroupID_2 != 'F') )
			{
				bRet = false;
			}
			else
			{
				m_dwFileSize = BitConverter.ToUInt32(i_Data, m_iIndexes[1]);
				m_caRiffType_0 = (char)i_Data[m_iIndexes[2]];
				m_caRiffType_1 = (char)i_Data[m_iIndexes[2] + 1];
				m_caRiffType_2 = (char)i_Data[m_iIndexes[2] + 2];
				m_caRiffType_3 = (char)i_Data[m_iIndexes[2] + 3];

				m_caFormatChunkID_0 = (char)i_Data[m_iIndexes[3]];
				m_caFormatChunkID_1 = (char)i_Data[m_iIndexes[3] + 1];
				m_caFormatChunkID_2 = (char)i_Data[m_iIndexes[3] + 2];
				m_caFormatChunkID_3 = (char)i_Data[m_iIndexes[3] + 3];
				m_dwFormatChunkSize = BitConverter.ToUInt32(i_Data, m_iIndexes[4]);
				m_wFormatTag = (BitConverter.ToUInt16(i_Data, m_iIndexes[5]));
				m_wChannels = BitConverter.ToUInt16(i_Data, m_iIndexes[6]);
				m_dwSamplesPerSec = BitConverter.ToUInt32(i_Data, m_iIndexes[7]);
				m_dwAvgBytesPerSec = BitConverter.ToUInt32(i_Data, m_iIndexes[8]);
				m_wBlockAlign = 0;
				m_wBitsPerSample = BitConverter.ToUInt16(i_Data, m_iIndexes[10]);
				m_wExtraInfoSize = BitConverter.ToUInt16(i_Data, m_iIndexes[11]);

				m_caDataChunkID_0 = (char)i_Data[m_iIndexes[12]];
				m_caDataChunkID_1 = (char)i_Data[m_iIndexes[12] + 1];
				m_caDataChunkID_2 = (char)i_Data[m_iIndexes[12] + 2];
				m_caDataChunkID_3 = (char)i_Data[m_iIndexes[12] + 3];
				iOffset = m_iIndexes[13];
				while( (m_caDataChunkID_0 != 'd') || (m_caDataChunkID_1 != 'a') || (m_caDataChunkID_2 != 't') || (m_caDataChunkID_3 != 'a') )
				{
					iOffset += (int)BitConverter.ToUInt32(i_Data, iOffset);
					iOffset += 4;
					m_caDataChunkID_0 = (char)i_Data[iOffset];
					m_caDataChunkID_1 = (char)i_Data[iOffset + 1];
					m_caDataChunkID_2 = (char)i_Data[iOffset + 2];
					m_caDataChunkID_3 = (char)i_Data[iOffset + 3];
					iOffset += 4;
				}
				m_dwDataChunkSize = BitConverter.ToUInt32(i_Data, iOffset);
			}

			return(bRet);
		}

		/// <summary>
		/// Check validity of data read in.
		/// </summary>
		/// <returns></returns>
		public bool Valid()
		{
			bool	bRet = true;

			// FIX - Find a way to compare arrays of chars that don't just compare references.
			/*if(!Array.Equals(m_caGroupID, "RIFF"))
			{
				bRet = false;
			}
			else if(!Array.Equals(m_caRiffType, "WAVE"))
			{
				bRet = false;
			}
			else if(!Array.Equals(m_caFormatChunkID, "fmt "))
			{
				bRet = false;
			}
			else if(!Array.Equals(m_caDataChunkID, "data"))
			{
				bRet = false;
			}
			else */if(m_wFormatTag != (UInt16)WaveFormat.MULAW)
			{
					bRet = false;
			}

			return(bRet);
		}
	} // class WavHeader
}
