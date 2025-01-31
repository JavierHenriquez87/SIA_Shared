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

namespace SIA.Print
{
    public class CartaDeIngreso
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersPDF _helpersPDF;

        public CartaDeIngreso(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
        }

        public async Task<byte[]> CreateCartaDeIngreso(string id)
        {
            //Obtenemos informacion de la planificacion de la auditoria
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                                .FirstOrDefaultAsync(u => u.CODIGO_AUDITORIA == id);

            var carta = await _context.MG_CARTAS
                                .FirstOrDefaultAsync(u => u.TIPO_CARTA == 1 && u.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == dataAI.ANIO_AI);

            string fechaInicioVisita = dataAI.FECHA_INICIO_VISITA?.ToString("dd MMMM yyyy");
            string fechaFinVisita = dataAI.FECHA_FIN_VISITA?.ToString("dd MMMM 'del año' yyyy");

            string fechaInicioRevision = dataAI.PERIODO_INICIO_REVISION?.ToString("dd MMMM yyyy");
            string fechaFinRevisiona = dataAI.PERIODO_FIN_REVISION?.ToString("dd MMMM 'del año' yyyy");

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
                                _helpersPDF.CreateTextFormat("Señor:", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(12)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("JERSON ELY VELASQUEZ PEREZ", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                )));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Jefe de Agencia", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                )));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Fundación Microfinanciera Hermandad de Honduras, OPDF", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                )));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Estimado Jefe de Agencia", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(16)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Hemos sido asignados a la revisión periódica de todas sus transacciones realizadas en su Agencia, en el periodo comprendido de " + fechaInicioVisita + " a " + fechaFinVisita + ".", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Con mandato de la Junta de Vigilancia nos solicitan que auditemos todas las transacciones que comprenden la cartera de préstamos, cartera de ahorros, movimientos diarios de caja de ventanilla, caja de reserva, inventario de activos fijos y libros de su Agencia correspondientes al periodo comprendido de " + fechaInicioRevision + " a " + fechaFinRevisiona + ".", bfArial, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(14)));

                    document.Add(section1);

                    var elements = HtmlConverter.ConvertToElements(carta.TEXTO_CARTA);

                    foreach (IElement element in elements)
                    {
                        if (element is Paragraph paragraph)
                        {
                            Paragraph styledParagraph = new Paragraph();

                            foreach (IElement childElement in paragraph.GetChildren())
                            {
                                if (childElement is Text text)
                                {
                                    var existe = text.GetProperty<string>(95) == "bold";

                                    if (existe)
                                    {
                                        text.SetFont(bfArialBd);
                                    }
                                    else
                                    {
                                        text.SetFont(bfArial);
                                    }

                                    styledParagraph.Add(text.SetFontSize(13).SetFontColor(_helpersPDF.ColorGris()));
                                }
                                else if (childElement is LineSeparator lineSeparator)
                                {
                                    styledParagraph.Add(lineSeparator);
                                }
                            }

                            document.Add(styledParagraph);
                        }
                        else if (element is List list)
                        {
                            foreach (ListItem item in list.GetChildren())
                            {
                                item.SetFont(bfArial).SetFontSize(13).SetFontColor(_helpersPDF.ColorGris());

                                foreach (IElement listItemChild in item.GetChildren())
                                {
                                    if (listItemChild is Text text)
                                    {
                                        var existe = text.GetProperty<string>(95) == "bold";

                                        if (existe)
                                        {
                                            text.SetFont(bfArialBd).SetFontSize(13).SetFontColor(_helpersPDF.ColorGris());
                                        }
                                        else
                                        {
                                            text.SetFont(bfArial).SetFontSize(13).SetFontColor(_helpersPDF.ColorGris());
                                        }
                                    }
                                    else if (listItemChild is Paragraph || listItemChild is List || listItemChild is Div)
                                    {
                                        if (listItemChild is Paragraph paragraph2)
                                        {
                                            paragraph2.SetMarginTop(8);
                                            Paragraph styledParagraph = new Paragraph();

                                            foreach (IElement childElement in paragraph2.GetChildren())
                                            {
                                                if (childElement is Text text2)
                                                {
                                                    var existe = text2.GetProperty<string>(95) == "bold";

                                                    if (existe)
                                                    {
                                                        text2.SetFont(bfArialBd);
                                                    }
                                                    else
                                                    {
                                                        text2.SetFont(bfArial);
                                                    }

                                                    styledParagraph.Add(text2.SetFontSize(13).SetFontColor(_helpersPDF.ColorGris()));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            document.Add(list);
                        }
                        else if (element is IBlockElement blockElement)
                        {
                            document.Add(blockElement);
                        }
                        else
                        {
                            // Manejar casos no esperados
                            throw new InvalidOperationException($"Elemento no compatible: {element.GetType().Name}");
                        }
                    }

                    var sectionFirma = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    Image firma1 = new Image(ImageDataFactory.Create("wwwroot/Archivos/Usuarios/Firmas_Usuarios/firmaSergio.png"))
                    .SetWidth(130)
                    .SetHeight(120)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    Image firma2 = new Image(ImageDataFactory.Create("wwwroot/Archivos/Usuarios/Firmas_Usuarios/firmaSergio.png"))
                    .SetWidth(125)
                    .SetHeight(120)
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetOpacity(0);

                    var cellFirma1 = new Cell()
                        .Add(firma1)
                        .Add(new Paragraph("Lic. Sergio Antonio Melgar").SetFont(bfArial).SetFontSize(13))
                        .Add(new Paragraph("Auditor Interno").SetFont(bfArialBd).SetFontSize(13))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER).SetFontColor(_helpersPDF.ColorGris());

                    var cellFirma2 = new Cell()
                        .Add(firma2)
                        .Add(new Paragraph("Jerson Ely Velasquez Perez").SetFont(bfArial).SetFontSize(13).SetFontColor(_helpersPDF.ColorGris()))
                        .Add(new Paragraph("Jefe de Agencia").SetFont(bfArialBd).SetFontSize(13))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER).SetFontColor(_helpersPDF.ColorGris());

                    sectionFirma.AddCell(cellFirma1);
                    sectionFirma.AddCell(cellFirma2);

                    document.Add(sectionFirma);

                    document.Close();

                    byte[] byteInfo = workStream.ToArray();
                    return byteInfo;
                }
            }
        }
        
    }

}
