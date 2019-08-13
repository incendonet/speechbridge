DROP TABLE tblDirectory;
DROP TABLE tblConfigParams;
DROP TABLE tblUserParams;
DROP TABLE tblSysActivity;

CREATE TABLE tblDirectory
(
    uUserID             SERIAL UNIQUE PRIMARY KEY,
    iFeatures1          int DEFAULT 0,
    sGroupName          varchar(64),
    
    sLName              varchar(32),
    sFName              varchar(32),
    sMName              varchar(32),
    sAltPronunciations  varchar(512),
    sWavPath            varchar(261),
    
    sExt                varchar(32),
    sUsername           varchar(64),
    sDomain             varchar(64),
    sEmail              varchar(128),
    
    abPasscode          bytea,
    abPassword          bytea,
    abIV                bytea
);

CREATE INDEX tbldirectory_uuserid_index ON tblDirectory (uUserID);

CREATE TABLE tblConfigParams
(
    uParamID        SERIAL UNIQUE PRIMARY KEY,
    sGroupName      varchar(64),
    sServerIP       varchar(16),
    sComponent      varchar(64),
    sName           varchar(32),
    sValue          varchar(256),
    sLabel          varchar(64),
    sHint           varchar(256),
    bAdvanced       bit
);

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0000', 'SipProxy', 'SIP Proxy Server', '', 'IP address of the SIP Proxy Server', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0001', 'SipFirstExt', 'First SIP extension', '2100', 'First extension of the group that SpeechBridge will use.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0002', 'SipPassword', 'SIP password', '', 'SIP password.  Note: This will be the same for all extensions.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0003', 'SipNumExt', 'Number of extensions', '4', 'Number of extensions group.  Note: Setting this higher than the number of ports you have licensed will have no effect.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0004', 'FirstLocalSipPort', 'First Local SIP IP port', '5062', 'IP port for the SpeechBridge User Agent to use', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0005', 'DisplayNamePrefix', 'Display Name prefix', 'IncendonetSpeechBridge_', 'The display names of SpeechBridge User Agents will begin with this string.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0006', 'SipTransport', 'SIP transport (UDP or TCP)', 'UDP', '', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0007', 'SipRegistrarIp', 'SIP Registrar Server (if different than proxy)', '', 'If the SIP Registrar Server is on the same server as the SIP Proxy Server, leave this field blank.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0008', 'SipRegistrationExpiration', 'SIP registration expiration (in seconds)', '900', '', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0009', 'RtpPortMin', 'Min RTP port', '10000', 'Low end of range of IP ports for RTP to use.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0010', 'RtpPortMax', 'Max RTP port', '10999', 'High end of range of IP ports for RTP to use.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0011', 'NatIpAddr', 'NAT IP address', '', 'Public IP where RTP should be routed.  Leave blank if used on a private network.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0012', 'AudioMgrIp', 'AudioMgr IP address', '', 'Address of the AudioMgr to use.  This value should only be changed in distributed configurations.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0013', 'AudioMgrSendPortFirst', 'First AudioMgr send port number', '1780', 'Port for send audio data socket.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0014', 'AudioMgrRecvPortFirst', 'First AudioMgr receive port number', '1781', 'Port for receive audio data socket.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0015', 'LogLevel', 'Log level', 'LOG_INFO', 'Choose from LOG_ERR, LOG_INFO, LOG_DEBUG, or LOG_DEBUG_STACK', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0016', 'LogFilenamePrefix', 'Log filename prefix', '/opt/speechbridge/logs/AudioRtr_', 'The name of the SpeechBridge User Agent log file will be preceded by this string.', '1');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'SIP:0017', 'SaveAudio', 'Save audio (0 or 1)', '1', '0 for no, 1 for yes', '1');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0000', 'WebdavServer', 'E-Mail Server', 'YourExchangeSrv.YourDomain.local', 'The fully qualified domain name of your email server.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0001', 'LdapServer', 'Directory Server', 'YourADSSrv.YourDomain.local', 'The name or IP address of your enterprise directory server.  This must be the primary controller if there is more than one.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0002', 'Domain', 'Domain', 'YourDomain.local', 'The name of the domain that the directory server belongs to, ie. "sales.mydomain.local".', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0003', 'UserName', 'Username', 'AnyUser', 'This is a user with access to browse the directory.', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'LDAP:0004', 'Password', 'Password', '', 'The password for the domain user.  This value is only used to browse the directory for importing and auto-sync.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'VoiceXML:0000', 'VoicexmlDirty', 'VoiceXML Dirty', 'false', 'When dirty, the VoiceXML does not match the data in the database, and needs regenerating', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'Licensing:0000', 'EULAAccepted', 'EULA Accepted', 'false', 'Web admin will be prompted if they have not yet accepted the EULA.', '0');

INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'GreetingPrompts', 'UseAfterhours', 'Use Afterhour Greeting Prompts', 'false', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0000', 'MonStart', '9:00 AM', '0900', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0001', 'MonEnd', '5:00 PM', '1700', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0002', 'TueStart', '9:00 AM', '0900', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0003', 'TueEnd', '5:00 PM', '1700', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0004', 'WedStart', '9:00 AM', '0900', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0005', 'WedEnd', '5:00 PM', '1700', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0006', 'ThuStart', '9:00 AM', '0900', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0007', 'ThuEnd', '5:00 PM', '1700', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0008', 'FriStart', '9:00 AM', '0900', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0009', 'FriEnd', '5:00 PM', '1700', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0010', 'SatStart', 'Closed', '0', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0011', 'SatEnd', 'Closed', '0', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0012', 'SunStart', 'Closed', '0', '', '0');
INSERT INTO tblConfigParams (sGroupName, sServerIP, sComponent, sName, sLabel, sValue, sHint, bAdvanced) Values ('HQ_0', '', 'BusinessHours:0013', 'SunEnd', 'Closed', '0', '', '0');

CREATE TABLE tblUserParams
(
    uParamKey       SERIAL UNIQUE PRIMARY KEY,
    uUserID         varchar(64),
    iParamType      integer DEFAULT 0,
    sParamName      varchar(32),
    sParamValue     varchar(128)
);

CREATE TABLE tblSysActivity
(
    uEventID        SERIAL UNIQUE PRIMARY KEY,
    sSessionId      varchar(96),
    tsEventTime     timestamp DEFAULT NOW(),
    iEventType      integer DEFAULT 0,
    iEventSeverity  smallint CHECK (iEventSeverity >= 0 AND iEventSeverity <= 5),
    sEventName      varchar(32),
    sEventLabel     varchar(64),
    sEventValue     varchar(256)
);
