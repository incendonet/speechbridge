INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP2:0000', 'IppbxType', 'IP-PBX Type', 'ShoreTel', 'The type of IP-PBX that SpeechBridge will connect to.', '1');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0005', 'Type', 'Server Type', '[none]', 'The type of directory to import users from.  Setting this to [none] disables directory syncing.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Collaboration:0000', 'Type', 'Server Type', '[none]', 'Choose the particular collaboration server type you are using, or [none] of you are not using one.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Collaboration:0001', 'Address', 'Server Address', '', 'The fully qualified domain name of the collaboration server.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0000', 'AudioRecording', 'Audio Recording', '0', 'Turn caller-side audio recording of calls on or off.  IMPORTANT NOTICE:  By turning this on, you acknowledge that you are complying with all local, state, and federal laws.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0000', 'OperatorExtension', 'Operator Extension', '0', 'The extension to reach the operator.  This is the extension callers are transferred to when they press 0, and if the system is unable to understand their request.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0001', 'IntroBargein', 'Speech Attendant greeting barge-in', '1', 'When barge-in is enabled, callers can start speaking before the greeting prompt finishes playing.  Hint: Enable this if you are having problems with echo-cancellation, which is common with analog PSTN trunks.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0002', 'CollabCommands', 'Email & Calendar commands in Speech Attendant', '1', 'When disabled, the email and calendar commands are not available from the Speech Attendants main menu.  You can create a DID Mapping to those applications to route callers directly to them, if desired.', '0');

UPDATE tblConfigParams SET sValue='Microsoft Active Directory' WHERE sComponent='LDAP:0005' AND EXISTS (SELECT uParamID FROM tblConfigParams WHERE sComponent='LDAP:0003' AND sValue<>'');
UPDATE tblConfigParams SET sValue=(select sValue from tblConfigParams WHERE sComponent LIKE 'LDAP:%' AND sName='WebdavServer') WHERE sComponent LIKE 'Collaboration:%' AND sName='Address';

CREATE TABLE tblDIDMap
(
    uEntryID            SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16),
    sVoicexmlUrl        varchar(256)
);

INSERT INTO tblDIDMap (sDID, sVoicexmlUrl) Values ('DEFAULT', 'http://localhost/VoiceDocStore/AAMain.vxml.xml');
