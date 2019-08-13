-- For all releases prior to 4.1.1;

-- Language codes for sLanguageCode as specified by RFC 5646 (see http://www.ietf.org/rfc/rfc5646.txt);
CREATE TABLE tblLanguages
(
    sLanguageName       varchar(128) UNIQUE NOT NULL DEFAULT '',
    sLanguageCode       varchar(32) NOT NULL DEFAULT ''
);

INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('2', 'SpeechBridge', '4.1.1.243', 'SpeechBridge updated to 4.1.1');
