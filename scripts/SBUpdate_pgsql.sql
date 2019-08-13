-- ---------------------------------------------------------------------------------------------------------------------;
-- Copyright © Incendonet 2016                                                                                          ;
--                                                                                                                      ;
-- For all releases prior to 6.4.1                                                                                      ;
-- ---------------------------------------------------------------------------------------------------------------------;
-- ---------------------------------------------------------------------------------------------------------------------;


-- ---------------------------------------------------------------------------------------------------------------------;
-- ---------------------------------------------------------------------------------------------------------------------;

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0009', 'DtmfDialingRestricted', 'DTMF Dialing Restricted', '1', 'If enabled DTMF dialing is limited to numbers in the User Directory.', '1');

INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('7', 'SpeechBridge', '6.4.1.16172', 'SpeechBridge updated to 6.4.1');
