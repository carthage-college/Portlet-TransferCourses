use ICS_NET
GO

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_TRANSFERCOURSES_HEADTEXT_LABEL' AND Language_Code='En')
 	INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
	VALUES ('CUS_TRANSFERCOURSES_HEADTEXT_LABEL', 'En','Header Text',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_TRANSFERCOURSES_HEADTEXT_DESC' AND Language_Code='En')
 	INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
	VALUES ('CUS_TRANSFERCOURSES_HEADTEXT_DESC', 'En','Text displayed at the top of the portlet page. May include HTML.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_TRANSFERCOURSES_FOOTTEXT_LABEL' AND Language_Code='En')
 	INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
	VALUES ('CUS_TRANSFERCOURSES_FOOTTEXT_LABEL', 'En','Footer Text',null); 

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_TRANSFERCOURSES_FOOTTEXT_DESC' AND Language_Code='En')
 	INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
	VALUES ('CUS_TRANSFERCOURSES_FOOTTEXT_DESC', 'En','Text displayed at the top of the portlet page. May include HTML.',null);

