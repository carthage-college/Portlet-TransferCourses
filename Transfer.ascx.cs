using System;
using System.Text;
using System.Xml;
using System.Data;
using CUS.OdbcConnectionClass3;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using Jenzabar.Common.ApplicationBlocks.Data; //SqlHelper
using Jenzabar.ICS;
using Jenzabar.ICS.Web.Portlets.Common; // PortletUtilities
using Jenzabar.Portal.Framework; //PortalUser
using Jenzabar.Portal.Framework.Configuration; // Settings
using Jenzabar.Portal.Framework.Web.UI;
//EBS - Added for CAR custom
using System.Collections;
//END

using System.Linq;
//using Jenzabar.Portal.Framework.Security.Authorization;

namespace CUS.ICS.TransferCourses
{
	/// <summary>
	///		Default_View is the only view and does all the work.
	/// </summary>
	public class Transfer : PortletViewBase
	{
        protected System.Web.UI.WebControls.Literal litHeadText;
        protected System.Web.UI.WebControls.Literal litFootText;
        protected System.Web.UI.WebControls.Label lblState;
        protected System.Web.UI.WebControls.Label lblSchool;
        protected System.Web.UI.WebControls.DropDownList ddlState;
        protected System.Web.UI.WebControls.DropDownList ddlSchool;
        protected System.Web.UI.WebControls.Button btnStateGo;
        protected System.Web.UI.WebControls.Button btnSchoolGo;
        protected System.Web.UI.WebControls.HyperLink lnkExport;
        protected System.Web.UI.WebControls.Panel pnlResults;
        protected System.Web.UI.WebControls.Panel pnlSchool;
        protected System.Web.UI.WebControls.DataGrid grdCourseList;

        #region Carthage custom controls
        protected bool canAdmin = PortalUser.Current.IsSiteAdmin || PortalUser.Current.GetGroupMembership().Where(gm => gm.DisplayName == "Staff").Count() > 0;
        #endregion

        protected string strKey;
		private CUS.OdbcConnectionClass3.OdbcConnectionClass3 odbcConn;

		private void Page_Load(object sender, System.EventArgs e)
		{
            if (PortletUtilities.GetSettingValue(this.ParentPortlet, "HeadText") != null)
            {
                litHeadText.Text = PortletUtilities.GetSettingValue(this.ParentPortlet, "HeadText");
            }
            if (PortletUtilities.GetSettingValue(this.ParentPortlet, "FootText") != null)
            {
                litFootText.Text = PortletUtilities.GetSettingValue(this.ParentPortlet, "FootText");
            }

            //Try and connect
			try
            {
                odbcConn = new CUS.OdbcConnectionClass3.OdbcConnectionClass3(Settings.GetConfigValue("C_Database", "ODBCConnectionStringCXANSI"));
            }
            catch
			{
                if (PortalUser.Current.IsSiteAdmin)
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Unable to locate usable connection string. Site admin only note: Insert a functioning CX connection string into FWK_ConfigSettings.Value where Category=C_Database and [Key]=ODBCConnectionStringCXANSI");
                else
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Unable to locate usable connection string. Contact portal administrator.");

                //ddlState.Visible = false;
                //pnlSchool.Visible = false;
                //pnlResults.Visible = false;
                ddlState.Visible = pnlSchool.Visible = pnlResults.Visible = false;
                return;
			}

			try
			{
                odbcConn.ConnectionTest();
			}
            catch
			{
                if (PortalUser.Current.IsSiteAdmin)
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Database connection test failed. Site admin only note: FWK_ConfigSettings C_Database ODBCConnectionStringCXANSI = " + Settings.GetConfigValue("C_Database", "ODBCConnectionStringCXANSI"));
                else
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Database connection test failed. Contact portal administrator. ");

                //ddlState.Visible = false;
                //pnlSchool.Visible = false;
                //pnlResults.Visible = false;
                ddlState.Visible = pnlSchool.Visible = pnlResults.Visible = false;
                return;
			}

            if (IsFirstLoad)
            {
                ddlStateLoad();
                pnlSchool.Enabled = false;
                ddlSchool.Items.Add(new ListItem("First select a State above", ""));
                pnlResults.Visible = false;
            }
            else
            {
                this.ParentPortlet.State = PortletState.Maximized;
            }

            if (!pnlSchool.Enabled && ddlState.SelectedIndex > 0)
            {
                ddlSchoolLoad(ddlState.SelectedValue);
                pnlSchool.Enabled = true;
            }
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
            ddlState.SelectedIndexChanged += new EventHandler(this.ddlState_SelectedIndexChanged);
            ddlSchool.SelectedIndexChanged += new EventHandler(this.ddlSchool_SelectedIndexChanged);
            //
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion


        private void ddlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlState.SelectedIndex > 0)
                ddlSchoolLoad(ddlState.SelectedValue);
            pnlResults.Visible = false;
        }

        private void ddlSchool_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ddlSchool.SelectedIndex > 0)
                {
                    bool blGood = grdCourseListLoad(Convert.ToInt32(ddlSchool.SelectedValue));
                    pnlResults.Visible = blGood;
                }
                else
                {
                    pnlResults.Visible = false;
                }
            }
            catch (Exception err)
            {
                if (PortalUser.Current.IsSiteAdmin)
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Couldn't load the courses for the selected School. Portal administrator only message: " + err.StackTrace + " school id: " + Convert.ToInt32(ddlSchool.SelectedValue)); // + ex.ToString());
                else
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Couldn't load the courses for the selected School. " + err.Message + err.StackTrace);
            }
        }
        
        private void ddlStateLoad()
        {

            //string strQueryText = "SELECT distinct st_table.txt, st_table.st " +
            //    "FROM id_rec, noncateq_rec, st_table " +
            //    "WHERE id_rec.id = noncateq_rec.sch_id " +
            //    "AND st_table.st = id_rec.st " +
            //    "AND st_table.st != ' ' " +
            //    "ORDER BY txt";
            string strQueryText = @"
                SELECT
	                TRIM(st_table.txt) AS txt, TRIM(st_table.st) AS st
                FROM
	                id_rec	INNER JOIN	noncateq_rec	ON	id_rec.id	=	noncateq_rec.sch_id
			                INNER JOIN	st_table		ON	id_rec.st	=	st_table.st
                WHERE
	                st_table.st != ' '
                GROUP BY
	                st_table.txt, st_table.st
                ORDER BY
	                txt
            ";
            Exception ex = new Exception();
            DataTable dtState = odbcConn.ConnectToERP(strQueryText, ref ex);
            if (ex != null)
            {
                if (PortalUser.Current.IsSiteAdmin)
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "State dropdown query failed. Portal administrator only message: " + ex.StackTrace + " sql: " + strQueryText); // + ex.ToString());
                else
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "State dropdown query failed. Contact portal administrator. "); // + ex.ToString());
                return;
            }

            if (dtState != null && dtState.Rows.Count > 0)
            {
                //Populate data grid
                ddlState.Items.Add(new ListItem("Please select a State", ""));
                ddlState.AppendDataBoundItems = true;
                ddlState.DataSource = dtState;
                ddlState.DataTextField = "txt";
                ddlState.DataValueField = "st";
                ddlState.DataBind();
            }
        }

        private void ddlSchoolLoad(string strState)
        {
            if (strState.Length > 4)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, "State value passed to school dropdown is invalid. Contact portal administrator.");
                return;
            }
            else
            {
                //string strQueryText = "SELECT distinct id_rec.id, " +
                //    "TRIM(id_rec.fullname)||' ('||TRIM(id_rec.city)||', '||id_rec.st||')' txt " +
                //    "FROM id_rec, noncateq_rec " +
                //    "WHERE id_rec.id = noncateq_rec.sch_id " + 
                //    "AND id_rec.st = '" + strState + "' " +
                //    "AND id_rec.id != " + Settings.GetConfigValue("C_ERP", "SchoolIDNumber") + " " +
                //    "ORDER BY txt";
                string strQueryText = String.Format(@"
                    SELECT
	                    id_rec.id, TRIM(id_rec.fullname)||' ('||TRIM(id_rec.city)||', '||id_rec.st||')' AS txt
                    FROM
	                    id_rec	INNER JOIN	noncateq_rec	ON	id_rec.id	=	noncateq_rec.sch_id
                    WHERE
	                    id_rec.st = '{0}'
                    AND
                        id_rec.id != {1}
                    GROUP BY
	                    id, txt
                    ORDER BY
	                    txt
                ", strState, Settings.GetConfigValue("C_ERP", "SchoolIDNumber"));
                System.Exception ex = new Exception();
                DataTable dtSchool = odbcConn.ConnectToERP(strQueryText, ref ex);
                if (ex != null)
                {
                    if (PortalUser.Current.IsSiteAdmin)
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, "School dropdown query failed. Portal administrator only message: " + ex.StackTrace + " sql: " + strQueryText); 
                    else
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, "School dropdown query failed. Contact portal administrator. ");
                return;
                }

                if (dtSchool != null && dtSchool.Rows.Count > 0)
                {
                    //Populate data grid
                    ddlSchool.Items.Clear();
                    ddlSchool.Items.Add(new ListItem("Please select a School", ""));
                    ddlSchool.AppendDataBoundItems = true;
                    ddlSchool.DataSource = dtSchool;
                    ddlSchool.DataTextField = "txt";
                    ddlSchool.DataValueField = "id";
                    ddlSchool.DataBind();
                    pnlSchool.Visible = true;
                }
                else
                {
                    pnlSchool.Visible = false;
                }
            }
        }

        protected void grdCourseList_Delete(object sender, DataGridCommandEventArgs e)
        {
            //this.ParentPortlet.ShowFeedback(String.Format("DELETE CMD Index: {0}; DataItem: {1}; PK: {2}", e.Item.ItemIndex, e.Item.DataItem.ToString(), grdCourseList.DataKeys[e.Item.ItemIndex]));
            try
            {
                string sqlInvalidate = String.Format("UPDATE noncateq_rec SET notes = 'INVALID' WHERE noncateq_no = {0}", grdCourseList.DataKeys[e.Item.ItemIndex].ToString());
                this.ParentPortlet.ShowFeedback(String.Format("DELETE CMD Index: {0};", sqlInvalidate));
                Exception exInvalidate = null;
                try
                {
                    //odbcConn.ConnectToERP(sqlInvalidate, ref exInvalidate);
                    //if (exInvalidate != null) { throw exInvalidate; }
                    pnlResults.Visible = grdCourseListLoad(Convert.ToInt32(ddlSchool.SelectedValue));
                }
                catch (Exception ei)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Error while invalidating:<br /><p>{0}</p><pre>{1}</pre>", ei.Message, ei.StackTrace));
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(String.Format("Error in ondelete:<br /><p>{0}</p><pre>{1}</pre>", ex.Message, ex.StackTrace));
            }
        }

        private bool grdCourseListLoad(int SchoolID)
        {
            string strQueryText = String.Format(@"
                SELECT
	                MAX(n.noncateq_no) AS noncateq_no, n.crs_no AS ext_crs_no, n.title AS ext_title, n.equiv_crs_no int_crs_no,trim(c.title1)||' '||trim(c.title2) int_title, MAX(beg_yr) AS beg_yr, n.notes
                FROM
	                noncateq_rec	n	INNER JOIN	crs_rec		c	ON	n.equiv_crs_no	=	c.crs_no
													                AND	n.equiv_cat		=	c.cat
						                INNER JOIN	cat_table	ct  ON	n.equiv_cat		=	ct.cat
                WHERE
	                sch_id =  {0}
                AND
	                LOWER(n.notes)	<>	'invalid'
                GROUP BY
	                ext_crs_no, ext_title, int_crs_no, int_title, n.notes
                ORDER BY
	                ext_crs_no, ext_title, beg_yr
            ", SchoolID);
            
            Exception ex = new Exception();

            try
            {
                DataTable dtCourses = odbcConn.ConnectToERP(strQueryText, ref ex);
                if (ex != null)
                {
                    if (PortalUser.Current.IsSiteAdmin)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Course List query failed. Portal administrator only message: " + ex.StackTrace + " sql: " + strQueryText); // + ex.ToString());
                    }
                    else
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Course List query failed. Contact portal administrator. " + ex.Message + strQueryText);
                    }
                    throw ex;
                }

                if (dtCourses != null && dtCourses.Rows.Count > 0)
                {
                    //If the user cannot admin the portlet, suppress display of the control column
                    grdCourseList.Columns[grdCourseList.Columns.Count - 1].Visible = !canAdmin;

                    // load the grid
                    grdCourseList.DataSource = dtCourses;
                    grdCourseList.DataBind();

                    // Set up the Export data and link
                    strKey = Guid.NewGuid().ToString();
                    lnkExport.ToolTip = "Export this list of courses as an Excel spreadsheet file.";
                    lnkExport.NavigateUrl = "Export_Data.aspx?key=" + strKey;
                    lnkExport.Visible = true;
                    System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                    System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
                    grdCourseList.RenderControl(htmlWrite);
                    htmlWrite.Flush();
                    HttpContext.Current.Session["tcfilename+" + strKey] = Regex.Replace(ddlSchool.SelectedItem.Text, @"\W", "");//remove non-alphanumeric characters from filename
                    HttpContext.Current.Session["tchtml+" + strKey] = stringWrite.ToString();

                    return true;
                }
                else
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "No records returned");
                    return false;
                }
            }
            catch(Exception ee)
            {
                return false;
            }
        }

        private bool grdCourseListLoadOriginal(int SchoolID)
        {
            //string strQueryText = "select distinct n.crs_no ext_crs_no, n.title ext_title, " + 
            //                        "n.equiv_crs_no int_crs_no, " + 
            //                        "trim(c.title1)||' '||trim(c.title2) int_title, " +
            //                        "beg_yr " +
            //                        //"case when n.end_yr = 0 " +
            //                        //" then n.beg_yr||'-' " + 
            //                        //" else n.beg_yr||'-'||n.end_yr " +
            //                        //"end year_span, " +
            //                        //"n.notes " +
            //                        "from noncateq_rec n, crs_rec c, cat_table ct " + 
            //                        "where sch_id = " + SchoolID + " " +
            //                        "and c.crs_no = n.equiv_crs_no and c.cat = n.equiv_cat " + 
            //                        "and n.equiv_cat = ct.cat order by ext_crs_no, ext_title, beg_yr";
            string strQueryText = String.Format(@"
                SELECT
	                n.crs_no AS ext_crs_no, n.title AS ext_title, n.equiv_crs_no int_crs_no,trim(c.title1)||' '||trim(c.title2) int_title, MAX(beg_yr) AS beg_yr, n.notes
                FROM
	                noncateq_rec	n	INNER JOIN	crs_rec		c	ON	n.equiv_crs_no	=	c.crs_no
													                AND	n.equiv_cat		=	c.cat
						                INNER JOIN	cat_table	ct  ON	n.equiv_cat		=	ct.cat
                WHERE
	                sch_id =  {0}
                AND
	                LOWER(n.notes)	<>	'invalid'
                GROUP BY
	                ext_crs_no, ext_title, int_crs_no, int_title, n.notes
                ORDER BY
	                ext_crs_no, ext_title, beg_yr
            ", SchoolID);
            
            Exception ex = new Exception();
            DataTable dtCourses = odbcConn.ConnectToERP(strQueryText, ref ex);
            if (ex != null)
            {
                if (PortalUser.Current.IsSiteAdmin)
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Course List query failed. Portal administrator only message: " + ex.StackTrace + " sql: " + strQueryText); // + ex.ToString());
                else
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Course List query failed. Contact portal administrator. " + ex.Message + strQueryText);
                return false;
            }

            if (dtCourses != null && dtCourses.Rows.Count > 0)
            {
                // set column headings
                string[] strColumnLabel = Settings.GetConfigValue("C_TrnsfrCrsesCART","CourseListColumnLabels").Split(',');
                for (int i = 0; i < dtCourses.Columns.Count; i++)
                    dtCourses.Columns[i].ColumnName = strColumnLabel[i].Trim();

                ArrayList arDups = new ArrayList();
                //This puts all of the elements into an ArrayList
                foreach(DataRow drC in dtCourses.Rows)
                {
                    arDups.Add(drC["Transfer Course"].ToString().Trim() + ":" + drC["Carthage Course"].ToString().Trim());
                }

                /*
                 * update FWK_ConfigSettings
                 * set Value = 'Transfer Course, Transfer Title, Carthage Course, Carthage Title, Recent Years',
                 * DefaultValue = 'Transfer Course, Transfer Title, Carthage Course, Carthage Title, Recent Years'
                 * where Category='C_TrnsfrCrsesCART' AND [Key]='CourseListColumnLabels'
                 */


                ArrayList arNewDups = new ArrayList();
                //This puts all of the ones that are duplicates based on "Transfer Course" and "Carthage Course" into a new ArrayList
                for(int i = 0; i < arDups.Count; i++)
                {
                    if(i < arDups.Count - 1)
                    {
                        if(arDups[i].ToString().Trim() == arDups[i+1].ToString().Trim())
                        {
                            arNewDups.Add(arDups[i].ToString());
                        }
                    }
                    
                }

                //This removes any duplicates of duplicates and puts into a new ArrayList
                for(int j = 0; j < arNewDups.Count; j++)
                {
                    string value =  arNewDups[j].ToString().Trim();
                    for(int k = j +1; k < arNewDups.Count; k++)
                    {
                        if(value == arNewDups[k].ToString().Trim())
                        {
                            arNewDups[k] = "remove:remove";
                        }
                    }
                }



                string tranCurrentRow = "";
                string cartCurrentRow = "";
                int count = 0;
                ArrayList arDup = new ArrayList();
                ArrayList arDup2 = new ArrayList();
                foreach (DataRow dr in dtCourses.Rows)
                {
                    if (tranCurrentRow != dr["Transfer Course"].ToString().Trim())
                    {
                        if (count > 0)
                        {
                            arDup.Add(tranCurrentRow + ":" + cartCurrentRow + ":" + count.ToString());
                        }
                        tranCurrentRow = dr["Transfer Course"].ToString().Trim();
                        cartCurrentRow = dr["Carthage Course"].ToString().Trim();

                        count = 0;
                    }
                    else
                    {
                        if (cartCurrentRow == dr["Carthage Course"].ToString().Trim())
                        {
                            count++;
                        }
                    }
                }

                


                DataTable dtNewTable = new DataTable();
                dtNewTable.Columns.Add("Transfer Course");
                dtNewTable.Columns.Add("Transfer Title");
                dtNewTable.Columns.Add("Carthage Course");
                dtNewTable.Columns.Add("Carthage Title");
                //dtNewTable.Columns.Add("Carthage Course");
                dtNewTable.Columns.Add("Recent Years");
                DataRow drNewRow = null;
                
                ArrayList arDupsToProcess = new ArrayList();
                for(int i = 0; i < arNewDups.Count; i++)
                {
                    int countDup = 0;
                    for(int j = 0; j < dtCourses.Rows.Count; j++)
                    {
                        DataRow drNew = dtCourses.Rows[j];
                        if (arNewDups[i].ToString().Trim() == drNew["Transfer Course"].ToString().Trim() + ":" + drNew["Carthage Course"].ToString().Trim())
                        {
                            countDup++;
                        }
                        else
                        {
                            if(countDup > 0)
                            {
                                arDupsToProcess.Add(arNewDups[i].ToString().Trim() + ":" + countDup.ToString());
                            }
                            countDup = 0;
                        }
                    }
                }


                //For testing
                //string outPut = "";
                //for (int i = 0; i < arDupsToProcess.Count; i++)
                //{
                //    outPut += arDupsToProcess[i].ToString() + "||";
                //}
                //this.ParentPortlet.ShowFeedback("HERE=" + outPut);


                string tranAlreadyProcessed = "";
                string cartAlreadyProcessed = "";


                //BIOL 100:BIO 1010:2
                //ENG 180:GEL 9999:3
                //POLS 101:POL 2400:2
                //POLS 122:POL 2400:2
                //PSY 100:PYC 1500:2
                //THEA 110:THR 1150:2

                bool match = false;
                foreach (DataRow drNew in dtCourses.Rows)
                {
                    for(int i = 0; i < arDupsToProcess.Count; i++)
                    {
                        string[] split = arDupsToProcess[i].ToString().Trim().Split(':');
                        string transferCourse = split[0];
                        string cartCourse = split[1];
                        string years = split[2];

                        if (tranAlreadyProcessed != drNew["Transfer Course"].ToString().Trim() && cartAlreadyProcessed != drNew["Carthage Course"].ToString().Trim())
                        {
                            match = false;

                            if (transferCourse == drNew["Transfer Course"].ToString().Trim() && cartCourse == drNew["Carthage Course"].ToString().Trim())
                            {
                                match = true;

                                drNewRow = dtNewTable.NewRow();
                                drNewRow["Transfer Course"] = drNew["Transfer Course"].ToString();
                                drNewRow["Transfer Title"] = drNew["Transfer Title"].ToString();
                                drNewRow["Carthage Course"] = drNew["Carthage Course"].ToString();
                                drNewRow["Carthage Title"] = drNew["Carthage Title"].ToString();
                                //drNewRow["Carthage Course"] = drNew["CART Course"].ToString().Trim() + " - " + drNew["CART Title"].ToString().Trim();

                                int startYear = Int32.Parse(drNew["Recent Years"].ToString());
                                int addYear = Int32.Parse(years) - 1;
                                int endYear = startYear + addYear;
                                drNewRow["Recent Years"] = startYear.ToString() + "-" + endYear.ToString();

                                dtNewTable.Rows.Add(drNewRow);

                                tranAlreadyProcessed = drNew["Transfer Course"].ToString().Trim();
                                cartAlreadyProcessed = drNew["Carthage Course"].ToString().Trim();
                            }
                        }
                        
                    }

                    //If here then no match was found
                    if (!match)
                    {
                        drNewRow = dtNewTable.NewRow();
                        drNewRow["Transfer Course"] = drNew["Transfer Course"].ToString();
                        drNewRow["Transfer Title"] = drNew["Transfer Title"].ToString();
                        drNewRow["Carthage Course"] = drNew["Carthage Course"].ToString();
                        drNewRow["Carthage Title"] = drNew["Carthage Title"].ToString();
                        //drNewRow["Carthage Course"] = drNew["CART Course"].ToString().Trim() + " - " + drNew["CART Title"].ToString().Trim();
                        drNewRow["Recent Years"] = drNew["Recent Years"].ToString();

                        dtNewTable.Rows.Add(drNewRow);
                    }
                }


                // load the grid
                //grdCourseList.DataSource = dtCourses;
                grdCourseList.DataSource = dtNewTable;
                grdCourseList.DataBind();

                // Set up the Export data and link
                Guid key;
                key = Guid.NewGuid();
                strKey = key.ToString();
                lnkExport.ToolTip = "Export this list of courses as an Excel spreadsheet file.";
                lnkExport.NavigateUrl = "Export_Data.aspx?key=" + strKey;
                lnkExport.Visible = true;
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
                grdCourseList.RenderControl(htmlWrite);
                htmlWrite.Flush();
                HttpContext.Current.Session["tcfilename+" + strKey] = Regex.Replace(ddlSchool.SelectedItem.Text, @"\W", "");//remove non-alphanumeric characters from filename
                HttpContext.Current.Session["tchtml+" + strKey] = stringWrite.ToString();

                return true;
            }
            else
            {
                return false;
            }
        }
	}
}