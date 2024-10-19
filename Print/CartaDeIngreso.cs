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

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Realizaremos nuestra auditoría con el objetivo de expresar nuestros comentarios, observaciones y recomendaciones sobre las inconsistencias encontradas en dicha revisión.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat(" Una auditoría conlleva la aplicación de procedimientos para obtener evidencia de auditoría sobre los importes y la información revelada en los estados financieros, comprobando el cumplimiento de los procesos, manuales, reglamentos, normativas y las circulares emitidas por la Administración y/o CNBS.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Los procedimientos seleccionados dependen del juicio del auditor, incluida la valoración de los riesgos de incorrección material en los estados financieros, debida a fraude o error.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //document.Add(section1);

                    //document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    //section1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    //cell = _helpersPDF.CreateTableCellNoBorder();

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("La Unidad de Auditoría Interna realiza revisiones de forma continua desde la Oficina Principal a través del Sistema Integral de Auditoria y solicitando información a las Agencias vía electrónica, identificado inconsistencias que serán consideradas al momento de emitir el informe de la auditoría realizada a su Agencia.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(2)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Debido a las limitaciones inherentes a la auditoría, junto con las limitaciones inherentes al control interno, existe un riesgo inevitable de que puedan no detectarse algunas incorrecciones materiales, aun cuando la auditoría se planifique y ejecute adecuadamente.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Al efectuar nuestras valoraciones del riesgo verificaremos el cumplimiento del control interno implementado por la Administración, identificando algunos riesgos y controles que no se han tomado en cuenta por la Administración, comunicándoles por escrito cualquier inconsistencia significativa en el control interno, relevante para la auditoría que identifiquemos durante la realización de la misma.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Realizaremos la auditoría considerando que la estructura organizativa de la Institución, reconocen y comprenden que el Jefe de Agencia es el responsable de:", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("a. La preparación, presentación y resguardo de la información de todas las transacciones realizadas en su Agencia", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginLeft(14).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("b. Fiel en el cumplimiento de todos los manuales, reglamentos, normativas y circulares emitidas por la Administración.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginLeft(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("c. El control interno que la Agencia considere necesario para permitir la preparación de toda la información, misma que debe ser transparente, confiable, oportuna, libre de errores e incorrección material y fraudes.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginLeft(14)));

                    //document.Add(section1);

                    //document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    //section1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    //cell = _helpersPDF.CreateTableCellNoBorder();

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Jefe de Agencia debe proporcionarnos", bfArialBd, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(2)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("a. Acceso a toda la información que tenga conocimiento que sea relevante en la preparación de nuestro informe, tal como registros, documentación y otros datos.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("b. Información adicional que podamos solicitar para los fines de la auditoría;", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("c. Acceso ilimitado al personal de la Unidad de Auditoría Interna en la realización de nuestro trabajo a manera de obtener evidencia de las inconsistencias encontradas.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Realizaremos la auditoría considerando que la estructura organizativa de la Institución, reconocen y comprenden que el Jefe de Agencia es el responsable de:", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat("Esperamos contar con la plena colaboración de sus empleados durante nuestra auditoría, es posible que la estructura y el contenido de nuestro informe tengan que ser modificados en función de las inconsistencias identificadas en nuestra auditoría.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14)));

                    //section1.AddCell(cell.Add(new Paragraph(
                    //            _helpersPDF.CreateTextFormat(" Le rogamos que firme y devuelva la copia adjunta de esta carta para indicar que conoce y acepta los acuerdos relativos a nuestra auditoría, de todas las transacciones que comprenden cartera de préstamos, cartera de ahorros, movimientos diarios de caja de ventanilla, caja de reserva y libros.", bfArial, 13)
                    //            .SetFontColor(_helpersPDF.ColorGris())
                    //            ).SetMarginTop(14).SetMarginBottom(28)));

                    //document.Add(section1);

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
