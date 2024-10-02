using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Kernel.Events;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using SIA.Context;
using Microsoft.EntityFrameworkCore;
using iText.Html2pdf;
using System.IO;
using System.Xml;
using System.Drawing;
using Image = iText.Layout.Element.Image;
using iText.IO.Image;
using System.Diagnostics;

namespace SIA.Print
{
    public class CartaDeSalida
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersPDF _helpersPDF;

        public CartaDeSalida(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
        }

        public async Task<byte[]> CreateCartaDeSalida(string id)
        {
            //Obtenemos informacion de la planificacion de la auditoria
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                                .FirstOrDefaultAsync(u => u.CODIGO_AUDITORIA == id);

            string fechaInicioVisita = dataAI.FECHA_INICIO_VISITA?.ToString("dd MMMM yyyy");
            string fechaFinVisita = dataAI.FECHA_FIN_VISITA?.ToString("dd MMMM 'del año' yyyy");

            MemoryStream workStream = new MemoryStream();

            using (var pdfWriter = new PdfWriter(workStream))
            {
                pdfWriter.SetCloseStream(false);
                using (PdfDocument pdfDoc = new PdfDocument(pdfWriter))
                using (Document document = new Document(pdfDoc, PageSize.LETTER))
                {
                    document.SetMargins(105, 50, 80, 50);

                    PdfFont bfArial = PdfFontFactory.CreateFont("c:/windows/fonts/Arial.ttf", PdfEncodings.IDENTITY_H);
                    PdfFont bfArialBd = PdfFontFactory.CreateFont("c:/windows/fonts/arialbd.ttf", PdfEncodings.IDENTITY_H);

                    //CELDAS PARA UTILIZAR EN LAS TABLAS
                    Cell cell = new Cell();

                    pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, new HeaderEventHandler(document, id));

                    //AGREGAMOS EL FOOTER DE LAS PAGINAS
                    pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, new TextFooterEventHandler(document));

                    //GENERAMOS UN ESPACIO PARA SER UTILIZADO DONDE SEA NECESARIO
                    Paragraph sectionSpacing = new Paragraph(" ").SetFontSize(4);

                    var sectionTitle = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    document.Add(sectionSpacing);
                    document.Add(sectionSpacing);
                    document.Add(sectionSpacing);

                    var section1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat(DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"), bfArial, 14).SetFontColor(_helpersPDF.ColorGris()))));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("AUDITORÍA INTERNA", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(16)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Fundación Microfinanciera Hermandad de Honduras, OPDF", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                )));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat(" San Marcos, Ocotepeque", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                )));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Estimado Jefe de Agencia", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(16)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Esta carta de manifestaciones se proporciona en relación con su auditoría interna de todas las transacciones de cartera de préstamos, cartera de ahorros, movimientos diarios de caja de ventanilla, caja de reserva, activos fijos, y libros de su Agencia, correspondientes al periodo comprendido de " + fechaInicioVisita + " a " + fechaFinVisita + ".", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Para efectos de expresar una conformidad sobre el cumplimiento de controles, internos, Resoluciones, Procesos, reglamentos internos y manuales operativos.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Confirmo que he cumplido con mi responsabilidad, tal como se establecen en el memorándum de solicitud y en la carta de entrada, con respecto a la presentación de toda la información misma que es transparente, confiable, oportuna, libre de errores de incorrección material y fraudes.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    document.Add(section1);

                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    section1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Les hemos proporcionado", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(2)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("a. Acceso a toda la información que tengo conocimiento que sea relevante en la preparación del informe, tal como registros, documentación y otro material;", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("b. Información adicional que solicitaron para los fines de la auditoría;", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("c. Acceso ilimitado al personal de la Unidad de Auditoría Interna en la realización de su trabajo, a manera de obtener las evidencias sobre las inconsistencias encontradas.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("d. Les manifiesto que la información proporcionada para la realización de su trabajo, está libre de incorrecciones materiales debida a fraudes de la que tenemos conocimiento y que afecta a la Agencia e implica a los empleados que desempeñan funciones significativas en el control interno.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("e. Le he revelado toda la información relativa a denuncias de fraude o a indicios de fraude que afectan a los estados financieros de la Institución, comunicada por empleados, antiguos empleados y clientes.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("f. Le he revelado todos los casos conocidos de incumplimiento o sospecha de incumplimiento de las disposiciones legales y reglamentarias cuyos efectos deberían considerarse para preparar los estados financieros.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat(" Le he revelado la identidad de las partes vinculadas con los Empleados y todas las relaciones y transacciones con partes vinculadas de las que tengo conocimiento.", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    document.Add(section1);

                    var sectionFirma = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    Image firma1 = new Image(ImageDataFactory.Create("wwwroot/Archivos/Usuarios/Firmas_Usuarios/firmaSergio.png"))
                    .SetWidth(100)
                    .SetHeight(90)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetOpacity(0);

                    var cellFirma1 = new Cell()
                        .Add(firma1)
                        .Add(new Paragraph("Jerson Ely Velasquez Perez").SetFont(bfArial).SetFontSize(13).SetFontColor(_helpersPDF.ColorGris()))
                        .Add(new Paragraph("Jefe de Agencia").SetFont(bfArialBd).SetFontSize(13))
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetBorder(Border.NO_BORDER).SetFontColor(_helpersPDF.ColorGris());

                    sectionFirma.AddCell(cellFirma1);

                    document.Add(sectionFirma);

                    document.Close();

                    byte[] byteInfo = workStream.ToArray();
                    return byteInfo;
                }
            }
        }
        
    }

}
