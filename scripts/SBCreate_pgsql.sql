DROP TABLE tblUsersInGroups;
DROP TABLE tblDirectory;
DROP TABLE tblConfigParams;
DROP TABLE tblUserParams;
DROP TABLE tblSysActivity;
DROP TABLE tblDIDToPromptMapping;
DROP TABLE tblDIDToAfterHoursPromptMapping;
DROP TABLE tblDIDToHolidaysPromptMapping;
DROP TABLE tblDIDPromptSetting;
DROP TABLE tblDIDBusinessHours;
DROP TABLE tblLanguages;
DROP TABLE tblComponentUpdates;
DROP TABLE tblDIDMap;
DROP TABLE tblSBtoADPropertyMapping;
DROP TABLE tblAvailableSBtoADPropertyMappings;
DROP TABLE tblSBDirProperties;
DROP TABLE tblADDirProperties;
DROP TABLE tblGroups;
DROP TABLE tblTTSLanguageCodeMapping;

DROP TABLE tblMenuCommandsMap;
DROP TABLE tblMenus;
DROP TABLE tblCommands;
DROP TABLE tblDtmfKeyToSpokenEquivalent;


CREATE TABLE tblDirectory
(
    uUserID             SERIAL UNIQUE PRIMARY KEY,
    iFeatures1          int DEFAULT 0,
    
    sLName              text,
    sFName              text,
    sMName              text,
    sAltPronunciations  text,
    sWavPath            text,
    
    sExt                text,
    sUsername           text,
    sDomain             text,
    sEmail              text,
    
    abPasscode          bytea,
    abPassword          bytea,
    abIV                bytea
);

CREATE INDEX tbldirectory_uuserid_index ON tblDirectory (uUserID);

CREATE TABLE tblConfigParams
(
    uParamID        SERIAL UNIQUE PRIMARY KEY,
    sGroupName      text,
    sServerIP       text,
    sComponent      text,
    sName           text,
    sValue          text,
    sLabel          text,
    sHint           text,
    bAdvanced       bit
);

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0000', 'SipProxy', 'SpeechBridge SIP Proxy', '', 'IP address of the SpeechBridge SIP Proxy Server.  (This is usually the IP address of the SpeechBridge server.)  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0001', 'SipFirstExt', 'First SpeechBridge SIP UA', '2100', 'The first SIP User Agent of the internal group that SpeechBridge will use.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0002', 'SipPassword', 'SIP password', '', 'SIP password.  Note: This will be the same for all extensions.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0003', 'SipNumExt', 'Number of extensions', '2', 'Number of simultaneous calls.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0004', 'FirstLocalSipPort', 'First Local SIP IP port', '5062', 'IP port for the SpeechBridge User Agent to use.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0005', 'DisplayNamePrefix', 'Display Name prefix', 'IncendonetSpeechBridge_', 'The display names of SpeechBridge User Agents will begin with this string.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0006', 'SipTransport', 'SIP transport (UDP or TCP)', 'UDP', 'Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0007', 'SipRegistrarIp', 'SIP Registrar Server (if different than proxy)', '', 'If the SIP Registrar Server is on the same server as the SIP Proxy Server, leave this field blank.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0008', 'SipRegistrationExpiration', 'SIP registration expiration (in seconds)', '900', 'Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0009', 'RtpPortMin', 'Min RTP port', '10000', 'Low end of range of IP ports for RTP to use.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0010', 'RtpPortMax', 'Max RTP port', '10999', 'High end of range of IP ports for RTP to use.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0011', 'NatIpAddr', 'NAT IP address', '', 'Public IP where RTP should be routed.  Leave blank if used on a private network.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0012', 'AudioMgrIp', 'AudioMgr IP address', '', 'Address of the AudioMgr to use.  This value should only be changed in distributed configurations.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0013', 'AudioMgrSendPortFirst', 'First AudioMgr send port number', '1780', 'Port for send audio data socket.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0014', 'AudioMgrRecvPortFirst', 'First AudioMgr receive port number', '1781', 'Port for receive audio data socket.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0015', 'LogLevel', 'Log level', 'LOG_INFO', 'Choose from LOG_ERR, LOG_INFO, LOG_DEBUG, or LOG_DEBUG_STACK.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0016', 'LogFilenamePrefix', 'Log filename prefix', '/opt/speechbridge/logs/AudioRtr_', 'The name of the SpeechBridge User Agent log file will be preceded by this string.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0017', 'SaveAudio', 'Save audio (0 or 1)', '1', '0 for no, 1 for yes  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP2:0000', 'IppbxType', 'IP-PBX Type', 'ShoreTel', 'The type of IP-PBX that SpeechBridge will connect to.  Note: After changing this value, you will need to click the Save button and then the Reset Speech Runtime for the change to take effect.', '1');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP3:0000', 'MasterIp', 'Master IP', '', 'IP address of Master.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP3:0001', 'SlaveIp', 'Slave IP', '', 'IP address of the Slave.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP3:0002', 'VirtualIp', 'Virtual IP', '', 'IP address used by SIP switch to communicate with SpeechBridge.', '1');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0000', 'WebdavServer', 'E-Mail Server', 'YourExchangeSrv.YourDomain.local', 'The fully qualified domain name of your email server.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0001', 'LdapServer', 'Directory Server', 'YourADSSrv.YourDomain.local', 'The name or IP address of your enterprise directory server.  This must be the primary controller if there is more than one.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0002', 'Domain', 'Domain', 'YourDomain.local', 'The name of the domain that the directory server belongs to, ie. "sales.mydomain.local".', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0003', 'UserName', 'Username', 'AnyUser', 'This is a user with access to browse the directory.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0004', 'Password', 'Password', '', 'The password for the domain user.  This value is only used to browse the directory for importing and auto-sync.  If this value has already been set, it will appear empty here.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0005', 'Type', 'Server Type', '[none]', 'The type of directory to import users from.  Setting this to [none] disables directory syncing.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Collaboration:0000', 'Type', 'Server Type', '[none]', 'Choose the particular collaboration server type you are using, or [none] if you are not using one.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page for the change to take effect.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Collaboration:0001', 'Address', 'Server Address', '', 'The fully qualified domain name of the collaboration server.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page for the change to take effect.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0000', 'AudioRecording', 'Audio Recording', '0', 'Turn caller-side audio recording of calls on or off.  IMPORTANT NOTICE:  By turning this on, you acknowledge that you are complying with all local, state, and federal laws.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0001', 'EDNoiseLevel', 'Speech Energy Detector Level', '1200', 'The level used by the speech energy detector to determine when someone is speaking, between 0 and 32000.  Lower values make the ED more sensitive.  Default is 1200.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0002', 'EDNoiseLevelInternal', 'Energy Detector Level (Internal)', '1600', 'The level used on internal calls by the speech energy detector to determine when someone is speaking, between 0 and 32000.  Lower values make the ED more sensitive.  Default is 1600.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0003', 'CallTimeout', 'Call Timeout (minutes)', '20', 'The time in minutes after which an incoming call is terminated regardless of activity.  Default is 20 minutes.  Note: After changing this value, you will need to click the Reset Speech Runtime button above for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:global:0004', 'RecognitionCutoff', 'Recognition Cutoff (%)', '50', 'The recognition probability below which the utterance is rejected.  Default is 50%.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0000', 'OperatorExtension', 'Operator Extension', '0', 'This is the extension callers are transferred to when they press 0, and if the system is unable to understand their request.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0001', 'IntroBargein', 'Speech Attendant greeting barge-in', '1', 'When enabled, callers can start speaking before the greeting finishes playing.  Disable this if you are having speech recognition problems on analog trunks.  After changing this value click the "Generate VoiceXML..." button on the User Directory page.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0002', 'CollabCommands', 'Email & Calendar commands in Speech Attendant', '0', 'When enabled, callers can enter the email and calendar applications by speech commands from the main menu.  After changing this value click the "Generate VoiceXML..." button on the User Directory page.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0003', 'MaxRetriesBeforeOperator', 'Max retries before transferring to operator', '3', 'If the caller is not understood after this many tries, transfer them to the defined operator extension.  Note: After changing this value, you will need to click the "Generate VoiceXML..." button on the User Directory page for the change to take effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0004', 'AlternateNumbersEditing', 'Alternate Numbers Editing by User', '0', 'If enabled users are allowed to edit their own Alternate Numbers.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0005', 'AliasEditing', 'Alias Editing by User', '0', 'If enabled users are allowed to edit their own Aliases and Alternate Pronunciations.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0006', 'MoreOptions', 'More Options', '0', 'When enabled allows ''More Options'' to be recognized and transfers to ''More Options'' dialog.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0007', 'ConfirmationCutoff', 'Confirmation Cutoff (%)', '90', 'The recognition probability above which the caller will not be asked to confirm what they said.  Default is 90%.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0008', 'DIDTruncationLength', 'DID Truncation Length', '15', 'Truncate the external phone number dialed to a corresponding internal extension for use in the speech attendant (e.g. a value of 4 would mean that for an Extension of 7605551212 in the User Directory, the call would be transferred to x1212).', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Apps:SpeechAttendant:0009', 'DtmfDialingRestricted', 'DTMF Dialing Restricted', '1', 'If enabled DTMF dialing is limited to numbers in the User Directory.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'VoiceXML:0000', 'VoicexmlDirty', 'VoiceXML Dirty', 'false', 'When dirty, the VoiceXML does not match the data in the database, and needs regenerating', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Licensing:0000', 'EULAAccepted', 'EULA Accepted', 'false', 'Web admin will be prompted if they have not yet accepted the EULA.', '0');


CREATE TABLE tblUserParams
(
    uParamKey       SERIAL UNIQUE PRIMARY KEY,
    uUserID         text,
    iParamType      integer DEFAULT 0,
    sParamName      text,
    sParamValue     text
);

CREATE INDEX tbluserparams_uuserid_index ON tblUserParams (uUserID);


CREATE TABLE tblSysActivity
(
    uEventID        SERIAL UNIQUE PRIMARY KEY,
    sSessionId      text,
    tsEventTime     timestamp DEFAULT NOW(),
    iEventType      integer DEFAULT 0,
    iEventSeverity  smallint CHECK (iEventSeverity >= 0 AND iEventSeverity <= 5),
    sEventName      text,
    sEventLabel     text,
    sEventValue     text
);

-- IMPORTANT: Do NOT make any changes to tblDIDMap as it will break APS (see Bryan's e-mail dated 9/16/15).;
CREATE TABLE tblDIDMap
(
    uEntryID            SERIAL UNIQUE PRIMARY KEY,
    sDID                text UNIQUE,
    sVoicexmlUrl        text
);

INSERT INTO tblDIDMap (sDID, sVoicexmlUrl) Values ('DEFAULT', 'AAMain_en-US.vxml.xml');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDToPromptMapping;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			The extension called.;
  -- sPromptFile:	Currently (11/08/12) needs to be fully qualified path and name.;
  -- sPromptText:	Text to be TTSd in case the prompt file is not specified or does not exist.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDToPromptMapping
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                text UNIQUE NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sPromptFile         text NOT NULL DEFAULT '',
    sPromptText         text NOT NULL DEFAULT ''
);

INSERT INTO tblDIDToPromptMapping (sDID, sPromptFile, sPromptText) VALUES ('DEFAULT', '/opt/speechbridge/VoiceDocStore/Prompts/DID/DEFAULT/ThankYouForCallingPleaseSayTheName.wav', 'Thank you for calling. Please say the name of the person or department you wish to reach.');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDToAfterHoursPromptMapping;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			The extension called.;
  -- sPromptFile:	Currently (11/08/12) needs to be fully qualified path and name.;
  -- sPromptText:	Text to be TTSd in case the prompt file is not specified or does not exist.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDToAfterHoursPromptMapping
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                text UNIQUE NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sPromptFile         text NOT NULL DEFAULT '',
    sPromptText         text NOT NULL DEFAULT ''
);

INSERT INTO tblDIDToAfterHoursPromptMapping (sDID) VALUES ('DEFAULT');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDToHolidaysPromptMapping;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			The extension called.;
  -- sDate:			;
  -- sPromptFile:	Currently (11/08/12) needs to be fully qualified path and name.;
  -- sPromptText:	Text to be TTSd in case the prompt file is not specified or does not exist.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDToHolidaysPromptMapping
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                text NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sDate               text NOT NULL,
    sPromptFile         text NOT NULL DEFAULT '',
    sPromptText         text NOT NULL DEFAULT ''
);

INSERT INTO tblDIDToHolidaysPromptMapping(sDID, sDate) VALUES ('DEFAULT', '01/01/0001');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDPromptSetting;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			The extension called.;
  -- sPromptFile:	After Hours prompts disabled by default.;
  -- sPromptText:	Holiday prompts disabled by default.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDPromptSetting
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                text UNIQUE NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    bAfterHoursEnabled  boolean NOT NULL DEFAULT 'false',
    bHolidaysEnabled    boolean NOT NULL DEFAULT 'false'
);

INSERT INTO tblDIDPromptSetting (sDID) VALUES ('DEFAULT');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDBusinessHours;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			The extension called.;
  -- sWeekDay:		;
  -- sStartTime:	;
  -- sEndTime:		;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDBusinessHours
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                text NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sWeekday            text NOT NULL,
    sStartTime          text NOT NULL,
    sEndTime            text NOT NULL
);

INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Monday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Tuesday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Wednesday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Thursday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Friday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Saturday', '0', '0');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Sunday', '0', '0');

-- ---------------------------------------------------------------------------------------------------------------------;
-- Language codes as specified by RFC 5646 (see http://www.ietf.org/rfc/rfc5646.txt)
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblLanguages
(
    sLanguageName       text UNIQUE NOT NULL DEFAULT '',
    sLanguageCode       text NOT NULL DEFAULT ''
);


CREATE TABLE tblSBDirProperties
(
    iSBPropertyId       integer UNIQUE PRIMARY KEY NOT NULL,
    sSBPropertyName     text UNIQUE NOT NULL
);

-- IMPORTANT: These names have to match the values in SBLdapConn.LdapDirectory (case sensitive).;

INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (1, 'FirstName');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (2, 'LastName');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (3, 'MiddleName');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (4, 'Extension');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (5, 'Email');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (6, 'Username');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (7, 'MobileNumber');
INSERT INTO tblSBDirProperties (iSBPropertyId, sSBPropertyName) VALUES (8, 'PagerNumber');


CREATE TABLE tblADDirProperties
(
    iADPropertyId       integer UNIQUE PRIMARY KEY NOT NULL,
    sADPropertyName     text UNIQUE NOT NULL
);

INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (1, 'givenName');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (2, 'sn');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (3, 'initials');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (4, 'mail');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (5, 'sAMAccountName');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (6, 'telephoneNumber');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (7, 'mobile');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (8, 'pager');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (9, 'homePhone');
INSERT INTO tblADDirProperties (iADPropertyId, sADPropertyName) VALUES (10, 'ipPhone');


CREATE TABLE tblAvailableSBtoADPropertyMappings
(
    iSBPropertyId       integer REFERENCES tblSBDirProperties,
    iADPropertyId       integer REFERENCES tblADDirProperties
);

INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (1, 1);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (1, 2);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (1, 3);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (2, 1);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (2, 2);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (2, 3);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (3, 1);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (3, 2);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (3, 3);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (4, 6);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (4, 7);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (4, 8);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (4, 9);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (4, 10);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (5, 4);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (6, 5);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (7, 6);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (7, 7);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (7, 8);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (7, 9);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (7, 10);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (8, 6);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (8, 7);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (8, 8);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (8, 9);
INSERT INTO tblAvailableSBtoADPropertyMappings (iSBPropertyId, iADPropertyId) VALUES (8, 10);


CREATE TABLE tblSBtoADPropertyMapping
(
    iSBPropertyId       integer UNIQUE REFERENCES tblSBDirProperties,
    iADPropertyId       integer UNIQUE REFERENCES tblADDirProperties
);

INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (1, 1);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (2, 2);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (3, 3);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (4, 6);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (5, 4);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (6, 5);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (7, 7);
INSERT INTO tblSBtoADPropertyMapping (iSBPropertyId, iADPropertyId) VALUES (8, 8);


CREATE TABLE tblGroups
(
    uGroupID             SERIAL UNIQUE PRIMARY KEY,
    sGroupName           text UNIQUE NOT NULL
);

INSERT INTO tblGroups (sGroupName) VALUES ('Hidden');


CREATE TABLE tblUsersInGroups
(
    uGroupID             integer REFERENCES tblGroups (uGroupID) ON DELETE CASCADE,
    uUserID              integer REFERENCES tblDirectory (uUserID) ON DELETE CASCADE
);


CREATE TABLE tblTTSLanguageCodeMapping
(
    sRequestedLanguageCode       text UNIQUE NOT NULL,
    sMappedLanguageCode          text NOT NULL
);


CREATE TABLE tblComponentUpdates
(
    dtApplied          timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sDatabaseVersion   text NOT NULL DEFAULT '',
    sSoftwareModule    text NOT NULL DEFAULT '',
    sSoftwareVersion   text NOT NULL DEFAULT '',
    sComments          text NOT NULL DEFAULT ''
);

-- ---------------------------------------------------------------------------------------------------------------------;
-- Menu Editor ("Dialog Designer light") tables;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblMenus
(
    iMenuID                         SERIAL UNIQUE PRIMARY KEY,
    sMenuName                       text UNIQUE NOT NULL DEFAULT '',
    sLanguageCode                   text NOT NULL DEFAULT '',
    sInclude                        text NOT NULL DEFAULT '',
    sGrammarUrl                     text NOT NULL DEFAULT '',
    sVariables                      text NOT NULL DEFAULT '',
    iMinConfScore                   integer NOT NULL DEFAULT 50,
    iHighConfScore                  integer NOT NULL DEFAULT 85,
    bDtmfCanBeSpoken                boolean NOT NULL DEFAULT '0',
    bConfirmationEnabled            boolean NOT NULL DEFAULT '0',
    bEnabled                        bit NOT NULL DEFAULT '1'
);

CREATE INDEX tblmenus_imenuid_index ON tblMenus (iMenuID);

CREATE TABLE tblCommands
(
    iCommandID                      SERIAL UNIQUE PRIMARY KEY,
    sCommandName                    text NOT NULL DEFAULT '',
    iCommandType                    integer NOT NULL DEFAULT 0,
    iOperationType                  integer NOT NULL DEFAULT 0,
    sCommandOption                  text NOT NULL DEFAULT '',
    sConfirmationText               text NOT NULL DEFAULT '',
    sConfirmationWavUrl             text NOT NULL DEFAULT '',
    sResponse                       text NOT NULL DEFAULT ''
);

CREATE INDEX tblcommands_icommandid_index ON tblCommands (iCommandID);

CREATE TABLE tblMenuCommandsMap
(
    iMCMapID                        SERIAL UNIQUE PRIMARY KEY,
    iMenuID                         integer    REFERENCES tblMenus        ON DELETE CASCADE,
    iCommandID                      integer    REFERENCES tblCommands     ON DELETE CASCADE		
);

CREATE INDEX tblmenucommandsmap_imenuid_index ON tblMenuCommandsMap (iMenuID);

CREATE TABLE tblDtmfKeyToSpokenEquivalent
(
    sLanguageCode                   text NOT NULL DEFAULT '',
    sDtmfKey                        char(1) NOT NULL,
    sSpokenEquivalent               text NOT NULL
);

INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('7', 'SpeechBridge', '6.4.1.16172', 'Fresh 6.4.1 install.');
