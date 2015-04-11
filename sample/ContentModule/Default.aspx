<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="/css/bootstrap.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="row">
                <div class="col-lg-12">
                    <div class="form-horizontal panel">
                        <div class="form-group">
                            <label class="col-lg-2 control-label">Enter Anything : </label>
                            <div class="col-sm-3">
                                <asp:TextBox ID="txtTest" CssClass="form-control" runat="server" />
                                <asp:RequiredFieldValidator ID="rfvTest" ControlToValidate="txtTest" ValidationGroup="Test" runat="server" Display="None" SetFocusOnError="true" ErrorMessage="Enter something" />
                            </div>
                            <div class="col-sm-3">
                                <asp:Button ID="btnSubmit" Text="Submit" CssClass="btn btn-primary" runat="server" OnClick="btnSubmit_Click" ValidationGroup="Test" />
                                <asp:ValidationSummary ID="vs1" runat="server" ValidationGroup="Test" ShowMessageBox="true" ShowSummary="false" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
