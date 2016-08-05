using System;
using System.Text;
using System.Web;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;
using Jenzabar.Portal.Framework.Web.UI.Controls.MetaControls;
using Jenzabar.Portal.Framework.Web.UI.Controls.MetaControls.Attributes;
using Jenzabar.Portal.Framework.Security.Authorization;

namespace CUS.ICS.TransferCourses
{
    #region Settings
    [TextAreaMetaControl(
         0, "HeadText", "CUS_TRANSFERCOURSES_HEADTEXT_LABEL",
         "CUS_TRANSFERCOURSES_HEADTEXT_DESC",
         false, @"<NameValueDataSources><NameValueDataSource Name='CUS_TRANSFERCOURSES_HEADTEXT_LABEL' Value='' /></NameValueDataSources>",
         NameValueDataSourceType.Static, NameValueType.PortletSetting, "MetaControl", 75, 5, 3000, true, LengthMaximum = 3000)]

    [TextAreaMetaControl(
         1, "FootText", "CUS_TRANSFERCOURSES_FOOTTEXT_LABEL",
         "CUS_TRANSFERCOURSES_FOOTTEXT_DESC",
         false, @"<NameValueDataSources><NameValueDataSource Name='CUS_TRANSFERCOURSES_FOOTTEXT_LABEL' Value='' /></NameValueDataSources>",
         NameValueDataSourceType.Static, NameValueType.PortletSetting, "MetaControl", 75, 5, 3000, true, LengthMaximum = 3000)]

    #region Carthage Custom Settings
    //[PortletOperation("CanAdminPortlet", "Can Administer Portlet", "Whether a user can administer this portlet", PortletOperationScope.Global)]
    #endregion
    #endregion

    /// <summary>
    /// Displays a list of transfer courses accepted from another school. The school is selected using
    /// a drop-down list with all the States in which transferring courses are on record, plus a drop-down
    /// list of the schools in a given state. Once a State and School are chosen, the courses are retrieved from
    /// the ERP database and shown in a grid. An Export to Excel link is also shown, with results actually formatted
    /// in HTML but named .xls.
    /// </summary>
    public class TransferCourses : PortletBase
    {
        public TransferCourses()
        {
        }

        protected override PortletViewBase GetCurrentScreen()
        {
            PortletViewBase screen = null;
            switch (this.CurrentPortletScreenName)
            {
                case "Admin":
                    screen = this.LoadPortletView("ICS/Portlet.TransferCoursesCART/Admin.ascx");
                    break;
                default:
                    screen = this.LoadPortletView("ICS/Portlet.TransferCoursesCART/Transfer.ascx");
                    break;
            }
            return screen;
        }
    }
}
