<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AgmRegistrationReport.aspx.cs" Inherits="BSA.WEB.Reports.Pages.AgmRegistrationReport" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" /> 
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>        
        <%--<rsweb:ReportViewer ID="AgmReportViewer" runat="server" Height="620px" Width="1150px">
        </rsweb:ReportViewer>--%>
        <rsweb:reportviewer runat="server" ID="AgmReportViewer"></rsweb:reportviewer>
    </form>
</body>
</html>

 

