-- For 3.1.1 and higher;
--INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0001', 'EDNoiseLevel', 'Speech Energy Detector Level', '1200', 'The level used by the speech energy detector to determine when someone is speaking, between 0 and 32000.  Lower values make the ED more sensitive.  Default is 1200.', '1');
--INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0002', 'EDNoiseLevelInternal', 'Energy Detector Level (Internal)', '1600', 'The level used on internal calls by the speech energy detector to determine when someone is speaking, between 0 and 32000.  Lower values make the ED more sensitive.  Default is 1600.', '1');
--INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0003', 'MaxRetriesBeforeOperator', 'Max retries before transferring to operator', '3', 'If the caller is not understood after this many tries, transfer them to the defined operator extension.', '1');

-- For 3.1.2 and higher;
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0004', 'AlternateNumbersEditing', 'Alternate Numbers Editing by User', '1', 'If enabled users are allowed to edit their own Alternate Numbers.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0005', 'AliasEditing', 'Alias Editing by User', '1', 'If enabled users are allowed to edit their own Aliases and Alternate Pronunciations.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP3:0000', 'MasterIp', 'Master IP', '', 'IP address of Master.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP3:0001', 'SlaveIp', 'Slave IP', '', 'IP address of the Slave.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP3:0002', 'VirtualIp', 'Virtual IP', '', 'IP address used by SIP switch to communicate with SpeechBridge.', '1');

-- For 4.0.1 beta1 and higher;
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0003', 'CallTimeout', 'Call Timeout (minutes)', '20', 'The time in minutes after which an incoming call is terminated regardless of activity.  Default is 20 minutes.  Note: After changing this value, you will need to click the Reset Speech Runtime button above for the change to take effect.', '1');

-- For all releases prior to 4.0.1;
UPDATE tblDIDMap SET sVoicexmlUrl = 'file:///opt/speechbridge/VoiceDocStore/AAMain.vxml.xml' WHERE sVoicexmlUrl = 'http://localhost/VoiceDocStore/AAMain.vxml.xml';
UPDATE tblConfigParams SET sLabel = 'SpeechBridge SIP Proxy', sHint = 'IP address of the SpeechBridge SIP Proxy Server.  (This is usually the IP address of the SpeechBridge server.)' WHERE sComponent = 'SIP:0000';
UPDATE tblConfigParams SET sLabel = 'First SpeechBridge SIP UA', sHint = 'The first SIP User Agent of the internal group that SpeechBridge will use.' WHERE sComponent = 'SIP:0001';

UPDATE tblConfigParams SET sHint = 'Choose the particular collaboration server type you are using, or [none] if you are not using one.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page for the change to take effect.' WHERE sComponent = 'Collaboration:0000';
UPDATE tblConfigParams SET sHint = 'The fully qualified domain name of the collaboration server.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page for the change to take effect.' WHERE sComponent = 'Collaboration:0001';

UPDATE tblConfigParams SET sHint = 'This is the extension callers are transferred to when they press 0, and if the system is unable to understand their request.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page.' WHERE sComponent = 'Apps:SpeechAttendant:0000';
UPDATE tblConfigParams SET sHint = 'When enabled, callers can start speaking before the greeting finishes playing.  Disable this if you are having speech recognition problems on analog trunks.  After changing this value click the "Generate VoiceXML..." button on the User Directory page.' WHERE sComponent = 'Apps:SpeechAttendant:0001';
UPDATE tblConfigParams SET sHint = 'When enabled, callers can enter the email and calendar applications by speech commands from the main menu.  After changing this value click the "Generate VoiceXML..." button on the User Directory page.' WHERE sComponent = 'Apps:SpeechAttendant:0002';
UPDATE tblConfigParams SET sHint = 'If the caller is not understood after this many tries, transfer them to the defined operator extension.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page for the change to take effect.' WHERE sComponent = 'Apps:SpeechAttendant:0003';
UPDATE tblConfigParams SET sHint = 'The level used by the speech energy detector to determine when someone is speaking, between 0 and 32000.  Lower values make the ED more sensitive.  Default is 1200.' WHERE sComponent = 'Apps:global:0001';

CREATE TABLE tblComponentUpdates
(
    dtApplied          timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sDatabaseVersion   varchar(64) NOT NULL DEFAULT '',
    sSoftwareModule    varchar(96) NOT NULL DEFAULT '',
    sSoftwareVersion   varchar(64) NOT NULL DEFAULT '',
    sComments          varchar(256) NOT NULL DEFAULT ''
);
 
INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('1', 'SpeechBridge', '4.0.1.157', 'Fresh 4.0.1 install.');
