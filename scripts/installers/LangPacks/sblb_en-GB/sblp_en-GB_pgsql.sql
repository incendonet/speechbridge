INSERT INTO tblLanguages (sLanguageName, sLanguageCode) VALUES ('English - Great Britain', 'en-GB');

INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '0', 'zero');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '0', 'null');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '0', 'nil');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '1', 'one');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '2', 'two');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '3', 'three');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '4', 'four');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '5', 'five');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '6', 'six');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '7', 'seven');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '8', 'eight');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '9', 'nine');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '*', 'star');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '*', 'asterisk');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '#', 'hash');
INSERT INTO tblDtmfKeyToSpokenEquivalent(sLanguageCode, sDtmfKey, sSpokenEquivalent) VALUES ('en-GB', '#', 'pound');

INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('5', 'SpeechBridge LanguagePack en-GB', '6.4.1.16172', 'SpeechBridge Language Pack for English - Great Britain, version 6.4.1.16172');
INSERT INTO tblTTSLanguageCodeMapping (sRequestedLanguageCode, sMappedLanguageCode) VALUES ('en-GB', 'en-US');
