-- ---------------------------------------------------------------------------------------------------------------------;
-- Copyright 2013 Incendonet Inc.                                                                                       ;
--                                                                                                                      ;
-- For all releases prior to 4.3.1;
-- ---------------------------------------------------------------------------------------------------------------------;
-- ---------------------------------------------------------------------------------------------------------------------;

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0004', 'RecognitionCutoff', 'Recognition Cutoff (%)', '50', 'The recognition probability below which the utterance is rejected.  Default is 50%.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0006', 'MoreOptions', 'More Options', '0', 'When enabled allows ''More Options'' to be recognized and transfers to ''More Options'' dialog.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0007', 'ConfirmationCutoff', 'Confirmation Cutoff (%)', '90', 'The recognition probability above which the caller will not be asked to confirm what they said.  Default is 90%.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0008', 'DIDTruncationLength', 'DID Truncation Length', '15', 'Truncate the external phone number dialed to a corresponding internal extension for use in the speech attendant (e.g. a value of 4 would mean that for an Extension of 7605551212 in the User Directory, the call would be transferred to x1212).', '1');
-- ---------------------------------------------------------------------------------------------------------------------;
-- ---------------------------------------------------------------------------------------------------------------------;
INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('4', 'SpeechBridge', '4.3.1.0', 'SpeechBridge updated to 4.3.1');
