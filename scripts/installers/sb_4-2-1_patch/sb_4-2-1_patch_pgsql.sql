-- ---------------------------------------------------------------------------------------------------------------------;
-- Copyright 2013 Incendonet Inc.                                                                                       ;
--                                                                                                                      ;
-- This SQL script will update a "fresh" 4.2.1.13007 installation to make sure all necessary tables exist and the       ;
-- defaults are initialized.                                                                                            ;
-- ---------------------------------------------------------------------------------------------------------------------;
-- ---------------------------------------------------------------------------------------------------------------------;

-- ---------------------------------------------------------------------------------------------------------------------;
-- Make sure tblLanguages exists.  (It should already be there, but failed in a previous install.);
-- Language codes as specified by RFC 5646 (see http://www.ietf.org/rfc/rfc5646.txt);
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblLanguages
(
    sLanguageName       varchar(128) UNIQUE NOT NULL DEFAULT '',
    sLanguageCode       varchar(32) NOT NULL DEFAULT ''
);

-- ---------------------------------------------------------------------------------------------------------------------;
-- Add 'unique' constraint to sDID in tblDIDMap.  (It should already be there.);
-- ---------------------------------------------------------------------------------------------------------------------;
ALTER TABLE tblDIDMap ADD CONSTRAINT tbldidmap_sdid_key UNIQUE (sDID);

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDToPromptMapping;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			Sized to be able to hold a valid E.164 number.;
  -- sPromptFile:	Currently (11/08/12) needs to be fully qualified path and name.;
  -- sPromptText:	Text to be TTSd in case the prompt file is not specified or does not exist.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDToPromptMapping
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16) UNIQUE NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sPromptFile         varchar(261) NOT NULL DEFAULT '',
    sPromptText         text NOT NULL DEFAULT ''
);

INSERT INTO tblDIDToPromptMapping (sDID, sPromptFile, sPromptText) VALUES ('DEFAULT', '/opt/speechbridge/VoiceDocStore/Prompts/DID/DEFAULT/ThankYouForCallingPleaseSayTheName.wav', 'Thank you for calling. Please say the name of the person or department you wish to reach.');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDToAfterHoursPromptMapping;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			Sized to be able to hold a valid E.164 number.;
  -- sPromptFile:	Currently (11/08/12) needs to be fully qualified path and name.;
  -- sPromptText:	Text to be TTSd in case the prompt file is not specified or does not exist.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDToAfterHoursPromptMapping
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16) UNIQUE NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sPromptFile         varchar(261) NOT NULL DEFAULT '',
    sPromptText         text NOT NULL DEFAULT ''
);

INSERT INTO tblDIDToAfterHoursPromptMapping (sDID) VALUES ('DEFAULT');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDToHolidaysPromptMapping;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			Sized to be able to hold a valid E.164 number.;
  -- sDate:			;
  -- sPromptFile:	Currently (11/08/12) needs to be fully qualified path and name.;
  -- sPromptText:	Text to be TTSd in case the prompt file is not specified or does not exist.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDToHolidaysPromptMapping
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16) NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sDate               varchar(10) NOT NULL,
    sPromptFile         varchar(261) NOT NULL DEFAULT '',
    sPromptText         text NOT NULL DEFAULT ''
);

INSERT INTO tblDIDToHolidaysPromptMapping(sDID, sDate) VALUES ('DEFAULT', '01/01/0001');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDPromptSetting;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			Sized to be able to hold a valid E.164 number.;
  -- sPromptFile:	After Hours prompts disabled by default.;
  -- sPromptText:	Holiday prompts disabled by default.;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDPromptSetting
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16) UNIQUE NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    bAfterHoursEnabled  boolean NOT NULL DEFAULT 'false',
    bHolidaysEnabled    boolean NOT NULL DEFAULT 'false'
);

INSERT INTO tblDIDPromptSetting (sDID) VALUES ('DEFAULT');

-- ---------------------------------------------------------------------------------------------------------------------;
-- tblDIDBusinessHours;
  -- uID:			The only reason to have this field to to allow easier manual manipulation of the table content;
  -- sDID:			Sized to be able to hold a valid E.164 number.;
  -- sWeekDay:		;
  -- sStartTime:	;
  -- sEndTime:		;
-- ---------------------------------------------------------------------------------------------------------------------;
CREATE TABLE tblDIDBusinessHours
(
    uID                 SERIAL UNIQUE PRIMARY KEY,
    sDID                varchar(16) NOT NULL REFERENCES tblDIDMap (sDID) ON DELETE CASCADE,
    sWeekday            varchar(9) NOT NULL,
    sStartTime          varchar(4) NOT NULL,
    sEndTime            varchar(4) NOT NULL
);

INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Monday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Tuesday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Wednesday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Thursday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Friday', '0900', '1700');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Saturday', '0', '0');
INSERT INTO tblDIDBusinessHours (sDID, sWeekDay, sStartTime, sEndTime) VALUES ('DEFAULT', 'Sunday', '0', '0');


--INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('1', 'SpeechBridge', '4.0.1.157', 'Fresh 4.0.1 install.');
--INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('2', 'SpeechBridge', '4.1.1.221', 'Fresh 4.1.1 install.');
--INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('3', 'SpeechBridge', '4.2.1.13007', 'Fresh 4.2.1 install.');
INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('3', 'SpeechBridge', '4.2.1.13007', 'Patched 4.2.1 install.');
