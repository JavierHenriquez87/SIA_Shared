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

namespace SIA.Print
{
    public class MemorandumPlanificacion
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersPDF _helpersPDF;

        public MemorandumPlanificacion(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
        }

        public async Task<byte[]> CreateMemorandumPlanificacion(string id)
        {
            //Obtenemos informacion de la planificacion de la auditoria
            var dataMDP = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                .FirstOrDefaultAsync(u => u.CODIGO_MEMORANDUM == id);

            var listAuditores = await _context.AU_AUDITORES_ASIGNADOS
                .Include(x => x.mg_usuarios)
                .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == dataMDP.NUMERO_MDP)
                .Where(e => e.ANIO_AI == dataMDP.ANIO_MDP)
                .OrderBy(e => e.CODIGO_USUARIO)
                .ToListAsync();

            //Obtenemos informacion de la auditoria integral
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == dataMDP.NUMERO_MDP)
                                .Where(u => u.ANIO_AI == dataMDP.ANIO_MDP)
                                .FirstOrDefaultAsync();

            DateTime? fechaInicioVisita = dataAI.FECHA_INICIO_VISITA;
            string fechaInicio = fechaInicioVisita?.ToString("dd/MM/yyyy");
            DateTime? fechaFinVisita = dataAI.FECHA_FIN_VISITA;
            string fechaFin = fechaFinVisita?.ToString("dd/MM/yyyy");

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
                    
                    sectionTitle.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("MEMORÁNDUM DE PLANIFICACIÓN", bfArialBd, 16)
                                .SetFontColor(_helpersPDF.ColorAnaranjado())
                                ).SetTextAlignment(TextAlignment.CENTER)));
                    document.Add(sectionTitle);

                    document.Add(sectionSpacing);
                    document.Add(sectionSpacing);
                    document.Add(sectionSpacing);

                    var section1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("San Marcos, Ocotepeque, " + DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"), bfArialBd, 13).SetFontColor(_helpersPDF.ColorAzul()))));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Introducción:", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorCafe())
                                ).SetMarginTop(12).SetMarginBottom(10)));

                    document.Add(section1);

                    var section2 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 95 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section2.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("1.", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde())).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section2.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Tipo de Auditoria:", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde()))));
                    
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section2.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section2.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Auditoria de " + dataAI.NOMBRE_AUDITORIA, bfArial, 12)))).SetMarginBottom(10);

                    document.Add(section2);

                    var section3 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 95 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section3.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("2.", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde())).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section3.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Objetivos:", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde()))));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section3.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                    document.Add(section3);

                    IList<IElement> elements = HtmlConverter.ConvertToElements(dataMDP.OBJETIVO);

                    foreach (var element in elements)
                    {
                        if (element is Paragraph paragraph)
                        {
                            paragraph.SetFont(bfArial).SetMarginLeft(40);
                            document.Add(paragraph);
                        }
                        else if (element is IBlockElement blockElement)
                        {
                            Paragraph tempParagraph = new Paragraph().Add(blockElement).SetFont(bfArial).SetMarginLeft(40);
                            document.Add(tempParagraph);
                        }
                    }
                    document.Add(sectionSpacing);

                    // OTRA LINEA
                    var section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 95 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("3.", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde())).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Equipo:", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde()))));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat(dataMDP.TEXTO_EQUIPO_TRABAJO, bfArial, 12))));

                    document.Add(section5);

                    section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 6, 90 })).SetWidth(UnitValue.CreatePercentValue(100));

                    var auditoresText = "";

                    // Crear y agregar entidades para cada auditor en el array
                    foreach (var auditores in listAuditores)
                    {
                        cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                        section5.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                            .SetTextAlignment(TextAlignment.RIGHT).SetMargin(0).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                        cell = _helpersPDF.CreateTableCellNoBorder();
                        section5.AddCell(cell.Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(auditores.mg_usuarios.NOMBRE_USUARIO, bfArial, 12)).SetMarginBottom(10)));
                    }

                    document.Add(section5);

                    // OTRA LINEA
                    section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 95 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("4.", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde())).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Recursos:", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde()))));

                    document.Add(section5);


                    /*elements = HtmlConverter.ConvertToElements(dataMDP.RECURSOS);
                    foreach (var element in elements)
                    {
                        if (element is IBlockElement blockElement)
                        {
                            document.Add(blockElement).SetFont(bfArialBd).SetLeftMargin(14);
                        }
                    }*/
                    // Convertir HTML a elementos iText
                    IList<IElement> elementos = HtmlConverter.ConvertToElements(dataMDP.RECURSOS);

                    foreach (var element in elementos)
                    {
                        if (element is Paragraph paragraph)
                        {
                            // Aplicar la fuente y el margen al párrafo
                            paragraph.SetFont(bfArial).SetMarginLeft(40);
                            document.Add(paragraph);
                        }
                        else if (element is IBlockElement blockElement)
                        {
                            Paragraph tempParagraph = new Paragraph().Add(blockElement).SetFont(bfArial).SetMarginLeft(40);
                            document.Add(tempParagraph);
                        }
                    }
                    //section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 6, 90 })).SetWidth(UnitValue.CreatePercentValue(100));

                    //cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    //section5.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                    //    .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    //cell = _helpersPDF.CreateTableCellNoBorder();
                    //section5.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Se solicitará información al área de Finanzas", bfArial, 12))));

                    //cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    //section5.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                    //    .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    //cell = _helpersPDF.CreateTableCellNoBorder();
                    //section5.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Se utilizará computadora asignada para realizar la auditoria", bfArial, 12))));

                    //document.Add(section5);

                    // OTRA LINEA
                    section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 95 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("5.", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde())).SetMarginLeft(14))).SetMarginTop(10);

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Tiempo:", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorVerde()))));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat(dataMDP.TEXTO_TIEMPO_AUDITORIA + " " + fechaInicio + " al " + fechaFin, bfArial, 12))));

                    document.Add(section5);

                    document.Close();

                    byte[] byteInfo = workStream.ToArray();
                    return byteInfo;
                }
            }
        }
        
    }

}
