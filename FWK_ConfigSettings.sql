use ICS_NET
GO

IF NOT EXISTS(SELECT [Key] FROM FWK_ConfigSettings WHERE Category='C_Database' AND [Key]='ODBCConnectionStringCXANSI')
 	INSERT INTO FWK_ConfigSettings (Category, [Key], Value, DefaultValue)
	VALUES ('C_Database', 'ODBCConnectionStringCXANSI', 'DSN=CXANSICART;', 'DSN=CXANSICART;');

IF NOT EXISTS(SELECT [Key] FROM FWK_ConfigSettings WHERE Category='C_ERP' AND [Key]='SchoolIDNumber')
 	INSERT INTO FWK_ConfigSettings (Category, [Key], Value, DefaultValue)
	VALUES ('C_ERP', 'SchoolIDNumber', '40737', '40737');

IF NOT EXISTS(SELECT [Key] FROM FWK_ConfigSettings WHERE Category='C_TrnsfrCrsesCART' AND [Key]='CourseListColumnLabels')
 	INSERT INTO FWK_ConfigSettings (Category, [Key], Value, DefaultValue)
	VALUES ('C_TrnsfrCrsesCART', 'CourseListColumnLabels', 'Transfer Course, Transfer Title, CART Course, CART Title, Years, Notes', 'Transfer Course, Transfer Title, CART Course, CART Title, Years, Notes');



