<%@ Control Language="c#" AutoEventWireup="false" CodeBehind="Transfer.ascx.cs" Inherits="CUS.ICS.TransferCourses.Transfer" %>
<div class="pContent">
	<div class="pSection">
	   <div  class="hint">
           <asp:Literal id="litHeadText" runat="server" />
	   </div>
        <table border="0" cellpadding="4">
            <tr>
                <th align="left">
                    <asp:Label ID="lblState" runat="server" Text="State:" />
                </th>
                <td>
                    <asp:DropDownList ID="ddlState" runat="server" AutoPostBack="True" />
                </td>
            </tr>
   	    <asp:panel id="pnlSchool" Runat="server">
            <tr>
                <th align="left">
                    <asp:Label ID="lblSchool" runat="server" Text="School:" />
                </th>
                <td>
                    <asp:DropDownList ID="ddlSchool" runat="server" AutoPostBack="True" />
                </td>
            </tr>
        </asp:panel>
        </table>
	    <asp:panel id="pnlResults" runat="server">
			<p><asp:HyperLink id="lnkExport" runat="server" Target="_blank">Export to Excel</asp:HyperLink></p>
            <asp:DataGrid id="grdCourseList" runat="server" AlternatingItemStyle-BackColor="#EFEFEF" CellPadding="4" HeaderStyle-BackColor="LightGray" HeaderStyle-Font-Bold="true"
                OnDeleteCommand="grdCourseList_Delete" DataKeyField="noncateq_no" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundColumn DataField="ext_crs_no" HeaderText="Transfer Course" />
                    <asp:BoundColumn DataField="ext_title" HeaderText="Transfer Title" />
                    <asp:BoundColumn DataField="int_crs_no" HeaderText="Carthage Course" />
                    <asp:BoundColumn DataField="int_title" HeaderText="Carthage Title" />
                    <asp:BoundColumn DataField="beg_yr" HeaderText="Years" />
                    <asp:ButtonColumn CommandName="Delete" ButtonType="PushButton" Text="Delete" HeaderText="Delete" ItemStyle-HorizontalAlign="Center" />
                </Columns>
            </asp:DataGrid>
            <p><asp:Literal id="litFootText" runat="server"></asp:Literal></p>
	    </asp:panel>
	</div>
</div>