INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0005', 'Type', 'Server Type', '[none]', 'The type of directory to import users from.  Setting this to [none] disables directory syncing.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Collaboration:0000', 'Type', 'Server Type', '[none]', 'Choose the particular collaboration server type you are using, or [none] of you are not using one.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Collaboration:0001', 'Address', 'Server Address', '', 'The fully qualified domain name of the collaboration server.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0000', 'AudioRecording', 'Audio Recording', '0', 'Turns debugging audio recording.  NOTE: By turning this on you are stating that you are in compliance with all applicable laws.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0000', 'OperatorExtension', 'Operator Extension', '0', 'The extension on which the operator/receptionist answers.', '0');

UPDATE tblConfigParams SET sValue='Microsoft Active Directory' WHERE sComponent='LDAP:0005' AND EXISTS (SELECT uParamID FROM tblConfigParams WHERE sComponent='LDAP:0003' AND sValue<>'');

CREATE TABLE tblDIDMap
(
    uEntryID            SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16),
    sVoicexmlUrl        varchar(256)
);

INSERT INTO tblDIDMap (sDID, sVoicexmlUrl) Values ('DEFAULT', 'http://localhost/VoiceDocStore/AAMain.vxml.xml');
