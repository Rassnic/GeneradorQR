<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="QRgenerate.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
       <div>
            <label for="txtQR">Texto para el QR:</label>
            <asp:TextBox ID="txtQR" runat="server"></asp:TextBox><br /><br />
            <asp:Button ID="btnGenerateQR" runat="server" Text="Generar QR" OnClick="btnGenerateQR_Click" /><br /><br />
            <asp:Image ID="imgQR" runat="server" Height="200" Width="200"/><br /><br />
            <asp:Label ID="lblMessage" runat="server" Text="Aquí se generará su QR." /><br /><br />
            <asp:Button ID="btnDownloadPDF" runat="server" Text="Descargar PDF" OnClick="btnDownloadPDF_Click" />
        </div>
        <div style="text-align: center;">
    <asp:Label  runat="server"  Font-Size="55px" Font-Bold="true" >Mis Reservaciones</asp:Label>
    </div>
    <div id="ConfigDivencabezado" >            
            <a href="/Formularios/Inicio.aspx" style="" >Regresar</a>
            <br />
            <br />
            <br />
            <br />
            <asp:Label ID="lblsolicitudcodigo" runat="server" AutoPostBack="true" Text="Ingrese su código de Consultora: "></asp:Label>
            <br />            
            <asp:TextBox ID="txtCodicoCon" CssClass="text" runat="server" AutoPostBack="true" OnTextChanged="txtCodicoCon_TextChanged" placeholder="Código Consultora"></asp:TextBox>            
            <br />
            <asp:Label ID="lblNombre" runat="server" Text="" Font-Size="Small"></asp:Label>
            <br />
            <asp:Label ID="lblCodigo" runat="server" Text="" Font-Size="Small"></asp:Label>           
            <br />           
            <br />
            <br />
        </div>
        <div id="ConfigDivGrid" >
            <asp:GridView GridLines="None" CssClass="table table-condensed table-responsive table-hover" 
                Width="100%" ID="dgvReservados" runat="server" AutoGenerateColumns="False" Enabled="true" Visible="true" 
                OnRowCommand="dgvReservados_RowCommand">
    
                <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="White" />
                <Columns>                    
                    <asp:BoundField DataField="id_lugar_reserva" HeaderText="<center>ID</center>" SortExpression="id_lugar_reserva" HtmlEncode="false"/>
                    <asp:BoundField DataField="lugar" HeaderText="<center>Lugar Reservado</center>" SortExpression="lugar" HtmlEncode="false"/>
                    <asp:BoundField DataField="codigo_con" HeaderText="<center>Codigo</center>" SortExpression="codigo_con" HtmlEncode="false"/>
                    <asp:BoundField DataField="fecha" HeaderText="<center>Fecha</center>" SortExpression="fecha" HtmlEncode="false" Visible="false"/>
                    <asp:BoundField DataField="codigo_reserva" HeaderText="<center>Codigo Reserva</center>" SortExpression="codigo_reserva" HtmlEncode="false" ReadOnly="false" />  
                    <asp:BoundField DataField="nombre_reserva" HeaderText="<center>Nombre Reserva</center>" SortExpression="nombre_reserva" HtmlEncode="false" ReadOnly="false" /> 
                    
                    <asp:TemplateField HeaderText="<center>PDF</center>">
                        <ItemTemplate>
                            <asp:Button ID="btnPDF" runat="server" Text="Descargar PDF" CommandName="GenerarPDF"
                                CommandArgument='<%# Eval("id_lugar_reserva") + "|" + Eval("lugar") + "|" + Eval("codigo_con") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<center>Correo</center>">
                        <ItemTemplate>
                            <asp:Button ID="btnCorreo" runat="server" Text="Enviar Correo" CommandName="EnviarCorreo"
                                CommandArgument='<%# Eval("id_lugar_reserva") + "|" + Eval("lugar") + "|" + Eval("codigo_con") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
            </asp:GridView>
        </div>
        <br />
        <br />
        <h1>Lectura QR</h1>
        <br />
        <br />     
        <asp:TextBox ID="txtCodigo" runat="server" AutoPostBack="true" OnTextChanged="txtCodigo_TextChanged" placeholder="Ingrese Codigo"></asp:TextBox><br />
        <br />
        <asp:Label ID="lblCodigoCompro" runat="server" Text="Codigo Compra"></asp:Label><br />
        <asp:TextBox ID="txtCodigoCompro" runat="server" placeholder="Ingrese Codigo"></asp:TextBox><br />
        <asp:TextBox ID="txtNombreCompro" runat="server"></asp:TextBox>
        <br />
        <br />
        <br />
        <asp:Label ID="Label1" runat="server" Text="Codigo Reservado"></asp:Label><br />
        <asp:TextBox ID="txtCodigoReservado" runat="server" placeholder="Ingrese Codigo"></asp:TextBox><br />
        <asp:TextBox ID="txtLugar" runat="server"></asp:TextBox><br />
        <asp:TextBox ID="txtNombrereservado" runat="server"></asp:TextBox><br />
        <asp:Label ID="lblValidaIngreso" runat="server" Text=""></asp:Label>
        <br />
        <br />
        <br />
    </form>
</body>
</html>