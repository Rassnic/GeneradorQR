using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Web.UI.WebControls;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Windows;
using iText.IO.Image;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace QRgenerate
{
	public partial class WebForm1 : System.Web.UI.Page
	{
        conexion conSAP = new conexion();
        private SqlCommand commandSAP = new SqlCommand();
        protected void Page_Load(object sender, EventArgs e)
		{
            //if (!IsPostBack) // Evitar que se sobrescriba en cada postback
            //{
            //    if (Request.QueryString["valor"] != null)
            //    {
            //        txtCodigo.Text = Request.QueryString["valor"];
            //    }
            //}

        }

        public void GenerarQR(string txtQR)
        {            

            if (string.IsNullOrEmpty(txtQR))
            {
                lblMessage.Text = "Por favor ingrese un texto para generar el QR.";
                return;
            }

            // Generar el código QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQR, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // Convertir la imagen a un bitmap
            Bitmap qrcodeImage = qrCode.GetGraphic(5);

            // Convertir la imagen a un array de bytes para enviarla al navegador
            using (MemoryStream ms = new MemoryStream())
            {
                qrcodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] QRbyte = ms.ToArray();

                // Establecer la imagen generada en el control Image
                imgQR.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(QRbyte);
            }

            string directoryPath = Server.MapPath("~/QRImages"); // Ruta de la carpeta QRImages
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath); // Crear la carpeta si no existe
            }
            
            // Generar un nombre único para el archivo (por ejemplo, usando un GUID)
            //string fileName = "QRCode_" + Guid.NewGuid().ToString() + ".png";
            string fileName = txtQR + ".png";
            string filePath = Path.Combine(directoryPath, fileName);

            // Guardar la imagen en la carpeta
            qrcodeImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            // Establecer la ruta de la imagen generada en el control Image
            imgQR.ImageUrl = "~/QRImages/" + fileName; // Mostrar la imagen guardada en el control Image
        }

        protected void btnDownloadPDF_Click(object sender, EventArgs e)
        {
            string txtQR1 = txtQR.Text;
            if (string.IsNullOrEmpty(txtQR1))
            {
                lblMessage.Text = "Por favor ingrese un texto para generar el QR.";
                return;
            }

            // Crear un archivo PDF
            using (MemoryStream ms = new MemoryStream())
            {
                // Crear un escritor de PDF
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Agregar el texto del Label
                document.Add(new Paragraph("Este es su código QR:"));
                document.Add(new Paragraph(txtQR1));

                // Agregar el código QR al PDF
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQR1, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrcodeImage = qrCode.GetGraphic(5);

                // Convertir la imagen a un byte[] y agregarla al PDF
                using (MemoryStream qrStream = new MemoryStream())
                {
                    qrcodeImage.Save(qrStream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] qrByte = qrStream.ToArray();
                    iText.Layout.Element.Image qrPdfImage = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(qrByte));
                    document.Add(qrPdfImage);
                }

                // Cerrar el documento
                document.Close();

                // Enviar el PDF al navegador para descargarlo
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment;filename="+ txtQR1 + ".pdf");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }

        protected void btnGenerateQR_Click(object sender, EventArgs e)
        {
            string txtQR1 = txtQR.Text;
            GenerarQR(txtQR1);
        }

        protected void txtCodicoCon_TextChanged(object sender, EventArgs e)

        {
            llenarTablaReserva();
        }
       
        private DataTable Reservados()
        {
            try
            {
                DataTable consultas = new DataTable();
                conSAP.connection.Open();
                commandSAP.Connection = conSAP.connection;
                commandSAP.CommandText = //"SELECT * FROM ejemplo.tbl_lugares_reservados Where codigo_con = '"+ 
                                        "USE [BD] SELECT id_lugar_reserva,  concat_ws('', letra, numero,color) as lugar, codigo_con, fecha, codigo_reserva, nombre_reserva, E_Mail " +
                                        "FROM tbl_lugares_reservados T0 " +
                                        "Inner join [Sbo_Masdel].[dbo].OCRD T1 On T0.codigo_reserva = T1.CardCode " +
                                        "Where codigo_con = '" +
                                        txtCodicoCon.Text + "'"
                                        + "and reservado = 1 ORDER BY color";
                consultas.Load(commandSAP.ExecuteReader());
                conSAP.connection.Close();
                return consultas;
            }
            catch (SqlException ex)
            {

            }
            return null;

        }

        private void llenarTablaReserva()
        {
            DataTable dt = new DataTable();
            dt = Reservados();
            dgvReservados.DataSource = dt;
            dgvReservados.DataBind();
            dgvReservados.Visible = true;
        }

        private void GenerarPDF(string codigo, string nombre, string lugar, int descarga)
        {
            string txtQR1 = codigo;
            if (string.IsNullOrEmpty(txtQR1))
            {
                lblMessage.Text = "Por favor ingrese un texto para generar el QR.";
                return;
            }

            string imagePath = "https://marykay.gt/wp-content/uploads/2025/02/Conferencia-de-Carrera-Web.png";

            // Definir ruta donde se guardará el PDF
            string rutaEspecifica = @"C:\Users\Ryuman\source\repos\QRgenerate\QRgenerate\QRgenerate\QRPdf\" + txtQR1 + ".pdf"; // Modifica esta ruta según tu necesidad

            using (MemoryStream ms = new MemoryStream())
            {
                // Crear un escritor de PDF
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Agregar la imagen
                iText.Layout.Element.Image img = new iText.Layout.Element.Image(ImageDataFactory.Create(imagePath))
                    .SetWidth(200)
                    .SetHeight(200)
                    .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                    .SetMarginTop(-50);

                document.Add(img); // Agrega la imagen primero

                string lugares = Regex.Replace(lugar, @"(\d+)([A-Za-z]+)", "$1 $2");                

                //Paragraph contenido = new Paragraph(                    
                //    "Nombre: " + nombre + "\n" + 
                //    "Evento: Conferencia de Carrera 2025\n" +
                //    "Fecha: Jueves 15 de Mayo\n" +
                //    "Hotel Westin Camino Real,  Salón Los Lagos\n" +
                //    "Lugares: " + lugares + "\n" + 
                //    "Este es su código QR:"
                //)
                //.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                //.SetFontSize(12);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                Paragraph contenido = new Paragraph()
                        .Add(new Text("Nombre: ").SetFont(boldFont)) // Texto en negrita
                        .Add(new Text(nombre+"\n").SetFont(normalFont))
                        .Add(new Text("Evento: ").SetFont(boldFont))
                        .Add(new Text("Conferencia de Carrera 2025\n").SetFont(normalFont))
                        .Add(new Text("Fecha: ").SetFont(boldFont))
                        .Add(new Text("Jueves 15 de Mayo, Hotel Westin Camino Real, Salón Los Lagos\n").SetFont(normalFont))
                        .Add(new Text("Lugares: ").SetFont(boldFont))
                        .Add(new Text(lugares+"\n").SetFont(normalFont))
                        .Add(new Text("Este es su código QR: ").SetFont(boldFont));


                // Alinear y agregar al documento
                contenido.SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                         .SetFontSize(12);


                document.Add(contenido);


                // Generar el código QR
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQR1, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrcodeImage = qrCode.GetGraphic(3);

                // Convertir la imagen del QR a byte[] y agregarla al PDF
                using (MemoryStream qrStream = new MemoryStream())
                {
                    qrcodeImage.Save(qrStream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] qrByte = qrStream.ToArray();
                    iText.Layout.Element.Image qrPdfImage = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(qrByte));

                    qrPdfImage.SetHorizontalAlignment((iText.Layout.Properties.HorizontalAlignment?)HorizontalAlignment.Center);

                    document.Add(qrPdfImage);
                }

                // **Centrar el texto del código**
                Paragraph codigoTexto = new Paragraph(txtQR1)
                    .SetTextAlignment((iText.Layout.Properties.TextAlignment?)TextAlignment.Right)
                    .SetFontSize(8);

                document.Add(codigoTexto);


                // Cerrar el documento
                document.Close();

                // **Guardar el PDF en una carpeta específica**
                File.WriteAllBytes(rutaEspecifica, ms.ToArray());

                // Enviar el PDF al navegador para su descarga
                if (descarga == 1) 
                { 
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment;filename=" + txtQR1 + ".pdf");
                Response.BinaryWrite(ms.ToArray());
                Response.End();

                } else { }
            }
        }

        private void GenerarPDF2(string codigo)
        {
            string txtQR1 = codigo;
            if (string.IsNullOrEmpty(txtQR1))
            {
                lblMessage.Text = "Por favor ingrese un texto para generar el QR.";
                return;
            }



            // Definir ruta donde se guardará el PDF
            string rutaEspecifica = @"C:\Users\Ryuman\source\repos\QRgenerate\QRgenerate\QRgenerate\QRPdf\" + txtQR1 + ".pdf"; // Modifica esta ruta según tu necesidad

            using (MemoryStream ms = new MemoryStream())
            {
                // Crear un escritor de PDF
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Agregar el texto del Label
                document.Add(new Paragraph("Nombre:"));
                document.Add(new Paragraph("Evento: Conferencia de Carrera 2025"));
                document.Add(new Paragraph("Fecha:"));
                document.Add(new Paragraph("Lugares:"));
                document.Add(new Paragraph("Este es su código QR:"));                
                document.Add(new Paragraph(txtQR1));

                // Generar el código QR
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQR1, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrcodeImage = qrCode.GetGraphic(5);

                // Convertir la imagen del QR a byte[] y agregarla al PDF
                using (MemoryStream qrStream = new MemoryStream())
                {
                    qrcodeImage.Save(qrStream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] qrByte = qrStream.ToArray();
                    iText.Layout.Element.Image qrPdfImage = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(qrByte));
                    document.Add(qrPdfImage);
                }

                // Cerrar el documento
                document.Close();

                // **Guardar el PDF en una carpeta específica**
                File.WriteAllBytes(rutaEspecifica, ms.ToArray());
              
            }
        }


        protected void dgvReservados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandArgument != null)
            {
                // Dividir los valores usando '|'
                string[] argumentos = e.CommandArgument.ToString().Split('|');

                string idLugarReserva = argumentos[0];
                string lugar = argumentos[1];
                string codigo = argumentos[2];
                string nombre = argumentos[3];
                string codigoR = argumentos[4];
                string correo = argumentos[5];

                if (e.CommandName == "GenerarPDF")
                {
                    GenerarPDF(codigoR + lugar + idLugarReserva, nombre, lugar, 1);
                }
                else if (e.CommandName == "EnviarCorreo")
                {
                    EnviarCorreo(codigoR + lugar + idLugarReserva, nombre, lugar, correo);
                }
            }
        }


        private void EnviarCorreo(string codigo, string nombre, string lugar, string correo)
        {
            GenerarQR(codigo);
            GenerarPDF(codigo, nombre, lugar, 0);
            string lugares = Regex.Replace(lugar, @"(\d+)([A-Za-z]+)", "$1 $2");
            string txtQR1 = codigo;

            try
            {
                // Configurar el cliente SMTP                
                SmtpClient clienteSmtp = new SmtpClient("smtp.office365.com");
                //clienteSmtp.Port = 25;
                clienteSmtp.Port = 587;
                clienteSmtp.Timeout = 10000;
                clienteSmtp.UseDefaultCredentials = false;
                clienteSmtp.DeliveryMethod = SmtpDeliveryMethod.Network;                
                clienteSmtp.Credentials = new NetworkCredential("rudy.yuman@masdel.com.gt", "PASS");
                //clienteSmtp.Credentials = new NetworkCredential("rudy.masdel@gmail.com", "PASS");
                clienteSmtp.EnableSsl = true;

                string rutaArchivoQR = @"C:\Users\Ryuman\source\repos\QRgenerate\QRgenerate\QRgenerate\QRImages\" + codigo + ".png";
                string rutaArchivoPDF = @"C:\Users\Ryuman\source\repos\QRgenerate\QRgenerate\QRgenerate\QRPdf\" + codigo + ".pdf";

                // Crear el mensaje de correo
                MailMessage mensaje = new MailMessage();
                mensaje.From = new MailAddress("rudy.yuman@masdel.com.gt");
                //string destino = correo;
                string destino = "rudy.yuman@masdel.com.gt";
                mensaje.To.Add(destino);
                //Copias Ocultas
                //mensaje.Bcc.Add("rudy.yuman@masdel.com.gt");
                //mensaje.Bcc.Add("rudy.masdel@gmail.com");
                mensaje.Subject = "Envío Codigo QR Conferencia de Carrera 2025";

                mensaje.IsBodyHtml = true;
                string textoEmail = "         " +
                                "<div style='text-align: center;'>" +
                                    "<img style='max-width:1px; height:1px; display: block; margin: 0 auto;' id='Encabezado' border='0' alt='Encabezado' src='cid:Encabezado'><br>" +

                                    "<label><b style='font-size:22px;'>Inversiones Masdel, S.A.</b></label><br><br>" +

                                    "<label><b style='font-size:20px;'>Nombre: </b></label>" +
                                    "<label style='font-size:21px;'>" + nombre + "</label><br>" +

                                    "<label><b style='font-size:20px;'>Evento: </b></label>" +
                                    "<label style='font-size:21px;'>Conferencia de Carrera 2025</label><br>" +

                                    "<label><b style='font-size:20px;'>Fecha: </b></label>" +
                                    "<label style='font-size:21px;'>Jueves 15 de Mayo, Hotel Westin Camino Real, Salón Los Lagos</label><br>" +

                                    "<label><b style='font-size:20px;'>Lugares: </b></label>" +
                                    "<label style='font-size:21px;'>" + lugares + "</label><br>" +

                                    "<p>Estimada Consultora, su código QR es:</p>" +

                                    "<img style='max-width:200px; display: block; margin: 0 auto;' id='QRimage' border='0' alt='QRimage' src='cid:QRimage'><br>" +
                                    "<label style='font-size:18px;'>" + txtQR1 + "</label><br>" +
                                "</div>";
                string textBody = textoEmail;
                AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(textBody, null, MediaTypeNames.Text.Plain);

                string htmlBody = "<html><body style=\"background-color:#white\">" + textBody + "</body></html>";
                // rgb(253, 246, 247) 80074C //obscuro F5DADF //rosa claro
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                
                string QRimagePath = @"C:\Users\Ryuman\source\repos\QRgenerate\QRgenerate\QRgenerate\QRImages\" + codigo + ".png";//aqui la ruta de la imagen
                LinkedResource QRLinkSource = new LinkedResource(QRimagePath);
                QRLinkSource.ContentId = "QRimage";
                htmlView.LinkedResources.Add(QRLinkSource);

                string EncabezadoPath = @"C:\Users\Ryuman\source\repos\QRgenerate\QRgenerate\QRgenerate\QRImages\Conferencia-de-Carrera.png";//aqui la ruta de la imagen
                LinkedResource EncabezadoLinkSource = new LinkedResource(EncabezadoPath);
                EncabezadoLinkSource.ContentId = "Encabezado";
                htmlView.LinkedResources.Add(EncabezadoLinkSource);

                mensaje.AlternateViews.Add(htmlView);
                mensaje.BodyEncoding = UTF8Encoding.UTF8;
                
                mensaje.Body = textoEmail;

                Attachment adjuntoqr = new Attachment(rutaArchivoQR);
                mensaje.Attachments.Add(adjuntoqr);

                Attachment adjuntoPDF = new Attachment(rutaArchivoPDF);
                mensaje.Attachments.Add(adjuntoPDF);
                //Attachment adjuntojson = new Attachment(rutaArchivoJSON);
                //mensaje.Attachments.Add(adjuntojson);

                // Adjuntar archivos al correo
                //foreach (var archivo in archivos)
                //{
                //    Attachment adjunto = new Attachment(archivo);
                //    mensaje.Attachments.Add(adjunto);
                //}

                // Enviar el correo
                clienteSmtp.Send(mensaje);

                // Limpiar los archivos adjuntos después de enviar el correo
                foreach (var archivoAdjunto in mensaje.Attachments)
                {
                    archivoAdjunto.Dispose();
                }

                //enviadas = 1;
                //Response.Write("Correo enviado con archivos adjuntos automáticamente");
            }
            catch (Exception ex)
            {
                               
            }


        }

        protected void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string consultaSql = " USE [BD] "+
                                        "SELECT " +
                                        "Distinct id_lugar_reserva,  " +
                                        "concat_ws(' ', letra, numero,color) as lugar, " +
                                        "codigo_con, " +
                                        "T3.CardName AS Nombre, " +
                                        "codigo_reserva, " +
                                        "nombre_reserva, " +
                                        "T4.GroupName Unidad " +
                                        "FROM " +
                                        "tbl_lugares_reservados AS T1 " +
                                        "INNER JOIN [Sbo_Masdel].[dbo].OINV AS T2 ON T1.codigo_con = T2.CardCode " +
                                        "INNER JOIN [Sbo_Masdel].[dbo].OCRD AS T3 ON T2.CardCode = T3.CardCode  " +
                                        "INNER JOIN [Sbo_Masdel].[dbo].OCRG AS T4 ON T3.GroupCode = T4.GroupCode " + 
                                        "WHERE codigo_reserva =  '" + txtCodigo.Text + "'";

                // Abre la conexión a la base de datos
                conSAP.connection.Open();
                commandSAP.Connection = conSAP.connection;

                using (SqlCommand cmd = new SqlCommand(consultaSql, commandSAP.Connection))
                {
                    // Ejecuta la consulta y obtiene el resultado
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id_lugar_reserva"]);
                        string CodigoReservo = reader["codigo_con"].ToString(); 
                        string NombreReservo = reader["Nombre"].ToString(); 

                        string lugar = reader["lugar"].ToString();
                        string CodigoReserva = reader["codigo_reserva"].ToString();
                        string NombreReserva = reader["nombre_reserva"].ToString();
                        string Unidad = reader["unidad"].ToString();

                        // Asigna el valor del resultado al TextBox
                        txtCodigoCompro.Text = CodigoReservo;
                        txtNombreCompro.Text = NombreReservo;
                        txtUnidad.Text = Unidad;

                        txtLugar.Text = lugar;

                        if (CodigoReserva == "")
                        {
                            txtCodigoReservado.Text = CodigoReservo;
                        } else
                        {
                            txtCodigoReservado.Text = CodigoReserva;
                        }

                        if (NombreReserva == "")
                        {
                            txtNombrereservado.Text = NombreReservo;
                        }
                        else
                        {
                            txtNombrereservado.Text = NombreReserva;
                        }

                        reader.Close();

                        if (chkCoffee.Checked)
                        {
                            if (ValidaCoffee(id) == 1)
                            {
                                lblValidaIngreso.Text = "Codigo ya Se le entrego Coffe Break";
                                lblValidaIngreso.Attributes.Add("style", "background-color:Yellow");
                            }
                            else
                            {
                                lblValidaIngreso.Text = "Se Puede Entregar Coffe Breake";
                                lblValidaIngreso.Attributes.Add("style", "background-color:#34e012");
                            }

                            actualizarCoffee(id);

                            txtUnidad.Text = txtUnidad.Text + " " + recuentoCoffee(CodigoReservo);
                            txtCodigo.Text = "";
                            txtCodigo.Focus();

                        }
                        else
                        {
                            if (Validaingreso(id) == 1)
                            {
                                lblValidaIngreso.Text = "Codigo ya Ingreso";
                                lblValidaIngreso.Attributes.Add("style", "background-color:Yellow");
                            }
                            else
                            {
                                lblValidaIngreso.Text = "Puede Ingresar";
                                lblValidaIngreso.Attributes.Add("style", "background-color:#34e012");
                            }

                            actualizarIngreso(id);

                            txtUnidad.Text = txtUnidad.Text + " " + recuentoIngresos(CodigoReservo);
                            txtCodigo.Text = "";
                            txtCodigo.Focus();
                        }
                    }
                    else
                    {
                        // MostrarAlerta("No se encontraron resultados.");
                        if (chkCoffee.Checked)
                        { lblValidaIngreso.Text = "No puede entregar Coffe Break"; }
                        else { lblValidaIngreso.Text = "No puede dar ingreso"; }

                       
                       lblValidaIngreso.Attributes.Add("style", "background-color:red");
                       txtCodigo.Text = "";                       
                       txtNombreCompro.Text = "";
                       txtCodigoCompro.Text = "";
                       txtCodigoReservado.Text = "";
                       txtNombrereservado.Text = "";
                       txtLugar.Text = "";
                       txtUnidad.Text = "";
                       txtCodigo.Focus();                     
                    }

                    // Cierra el lector de datos                    
                    reader.Close();
                }
            }
            catch
            {

                if (chkCoffee.Checked)
                { lblValidaIngreso.Text = "No puede entregar Coffe Break"; }
                else { lblValidaIngreso.Text = "No puede dar ingreso"; }

                lblValidaIngreso.Attributes.Add("style", "background-color:red");
                txtCodigo.Text = "";
                txtNombreCompro.Text = "";
                txtCodigoCompro.Text = "";
                txtCodigoReservado.Text = "";
                txtNombrereservado.Text = "";
                txtLugar.Text = "";
                txtUnidad.Text = "";
                txtCodigo.Focus();
            }

           conSAP.connection.Close();
        }


        private void actualizarIngreso(int id)
        {

            try
            {
                conSAP.connection.Open();
                commandSAP.Connection = conSAP.connection;
                commandSAP.CommandText = "USE [BD]; Update tbl_lugares_reservados set ingreso = 1 where id_lugar_reserva = " + id;
                commandSAP.ExecuteReader();
                commandSAP.Connection.Close();
            }
            catch (SqlException ex)
            {

            }           

        }

        private void actualizarCoffee(int id)
        {

            try
            {
                conSAP.connection.Open();
                commandSAP.Connection = conSAP.connection;
                commandSAP.CommandText = "USE [BD]; Update tbl_lugares_reservados set coffee = 1 where id_lugar_reserva = " + id;
                commandSAP.ExecuteReader();
                commandSAP.Connection.Close();
            }
            catch (SqlException ex)
            {

            }

        }

        private int Validaingreso(int id)
        {

            try
            {
                int ingreso;                
                commandSAP.CommandText = "USE [BD]; SELECT ISNULL(ingreso, 0) as Ingreso FROM tbl_lugares_reservados  where id_lugar_reserva = " + id;
                ingreso = Convert.ToInt32(commandSAP.ExecuteScalar());
                commandSAP.Connection.Close();
                return ingreso;
            }
            catch (SqlException ex)
            {
                return 0;
            }

        }

        private int ValidaCoffee(int id)
        {

            try
            {
                int ingreso;
                commandSAP.CommandText = "USE [BD]; SELECT ISNULL(Coffee, 0) as Coffee FROM tbl_lugares_reservados  where id_lugar_reserva = " + id;
                ingreso = Convert.ToInt32(commandSAP.ExecuteScalar());
                commandSAP.Connection.Close();
                return ingreso;
            }
            catch (SqlException ex)
            {
                return 0;
            }

        }

        public string recuentoIngresos(string codigo)
        {
            try
            {
                string recuento;
                conSAP.connection.Open();
                commandSAP.Connection = conSAP.connection;
                commandSAP.CommandText = "USE [BD]; Select CONCAT( " +
                    "(select COUNT(id_lugar_reserva) from tbl_lugares_reservados where ingreso = 1 and codigo_con = '"+codigo+"')," +
                    "' / '," +
                    "COUNT(id_lugar_reserva)) AS recuento " +
                    "from tbl_lugares_reservados where codigo_con = '"+codigo+"' ;";
                recuento = commandSAP.ExecuteScalar().ToString();
                commandSAP.Connection.Close();
                return recuento;
            }
            catch (SqlException ex)
            {
                return "0/0";
            }

        }


        public string recuentoCoffee(string codigo)
        {
            try
            {
                string recuento;
                conSAP.connection.Open();
                commandSAP.Connection = conSAP.connection;
                commandSAP.CommandText = "USE [BD]; Select CONCAT( " +
                    "(select COUNT(id_lugar_reserva) from tbl_lugares_reservados where coffee = 1 and codigo_con = '" + codigo + "')," +
                    "' / '," +
                    "COUNT(id_lugar_reserva)) AS recuento " +
                    "from tbl_lugares_reservados where codigo_con = '" + codigo + "' ;";
                recuento = commandSAP.ExecuteScalar().ToString();
                commandSAP.Connection.Close();
                return recuento;
            }
            catch (SqlException ex)
            {
                return "0/0";
            }

        }


    }
}