// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using SBConfigStor;
using SBTTS;


namespace SBTTSTests
{
	[TestFixture]
	public class TtsLanguageCodeMappingTest
	{
		private class TestTtsLanguageMappingDAL : ITtsLanguageCodeMappingDAL
		{
			public TestTtsLanguageMappingDAL()
			{
			}

			public Dictionary<string, string> GetMapping()
			{
				Dictionary<string, string> languageCodeMapping = new Dictionary<string, string>();

				languageCodeMapping.Add("en-AU", "en-GB");

				return languageCodeMapping;
			}
		}

		[Test]
		public void TestThatEmptyMappingReturnPassedInLanguageCode()
		{
			string sLanguageCode = "en-US";

			TtsLanguageCodeMapping ttsLanguageCodeMapping = new TtsLanguageCodeMapping(null);

			Assert.That(ttsLanguageCodeMapping.GetMappedLanguageCodeFor(sLanguageCode), Is.EqualTo(sLanguageCode));
		}

		[Test]
		public void TestThatUnmappedLanguageCodeReturnsPassedInLanguageCode()
		{
			string sLanguageCode = "de-DE";

			TtsLanguageCodeMapping ttsLanguageCodeMapping = new TtsLanguageCodeMapping(new TestTtsLanguageMappingDAL());

			Assert.That(ttsLanguageCodeMapping.GetMappedLanguageCodeFor(sLanguageCode), Is.EqualTo(sLanguageCode));
		}

		[Test]
		public void TestThatLanguageCodeIsRemapped()
		{
			string sOriginalLanguageCode = "en-AU";
			string sExpectedLanguageCode = "en-GB";

			TtsLanguageCodeMapping ttsLanguageCodeMapping = new TtsLanguageCodeMapping(new TestTtsLanguageMappingDAL());

			Assert.That(ttsLanguageCodeMapping.GetMappedLanguageCodeFor(sOriginalLanguageCode), Is.EqualTo(sExpectedLanguageCode));
		}
	}
}
