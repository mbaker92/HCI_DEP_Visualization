<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DEPVisualization._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row">
        <h1>DEP Visualization</h1>
    </div>
    
    <!--  TODO: Add input boxes -->
    <asp:Label runat="server" ID="ExampleLabel"></asp:Label>

    <asp:button name="SubmitButton" Text="Submit" runat="server" onclick="SubmitButtonOnClick"></asp:button>
    
    <!--  TODO: Add charts (use nuget to get DotNetHighCharts  -->

</asp:Content>
