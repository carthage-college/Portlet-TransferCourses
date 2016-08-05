using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Data;
using Jenzabar.Common.ApplicationBlocks.ExceptionManagement;

namespace CUS.ICS.TransferCourses
{
	/// <summary>
	/// Summary description for ExportEvents.
	/// </summary>
	public class Export_Data : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			string strContentType = "text/plain"; // these defaults will be overwritten if we're successful
			string strFilename = "ErrorOutput.txt";
			string strKey = string.Empty;
			string strExt = string.Empty;

			System.IO.MemoryStream mstream = new MemoryStream();
			StreamWriter sw = new StreamWriter(mstream);

			if (Request.QueryString["key"] != null
				&& IsGuidFormat(Request.QueryString["key"].ToString()))
			{
				strKey = Request.QueryString["key"].ToString();
				if (HttpContext.Current.Session["tcfilename+" + strKey] != null 
					&& HttpContext.Current.Session["tchtml+" + strKey] != null)
				{
					sw.WriteLine(HttpContext.Current.Session["tchtml+" + strKey].ToString());
					strFilename = HttpContext.Current.Session["tcfilename+" + strKey].ToString() + ".xls";
                    strContentType = "application/vnd.ms-excel";
				}
				else
				{
					sw.WriteLine("Export failed. Please contact site adminstrator. (cache empty)");
				}
			}
			else
			{
				sw.WriteLine("Export failed. Please contact site adminstrator. (bad key)");
			}
			sw.Flush();
			sw.Close();
			
			byte[] byteArray = mstream.ToArray();

			mstream.Flush();
			mstream.Close();

			Response.Clear();
			Response.AddHeader("Content-Type", strContentType);
			Response.AddHeader("Content-Disposition", "attachment; filename=" + strFilename);
			Response.AddHeader("Content-Length", byteArray.Length.ToString());
			Response.ContentType = "application/octet-stream";
			Response.BinaryWrite(byteArray);
			Response.End();

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
		
		private bool IsGuidFormat(string strTest)
		{
			try
			{
				ObjectIdentifier guidTest = new ObjectIdentifier(new Guid(strTest));
				return true;
			}
			catch
			{
				return false;
			}
		}

	}
}
