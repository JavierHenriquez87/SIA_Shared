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
using SIA.Models;

namespace SIA.Print
{
    public class ResultadoInforme
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private int cod;
        private int anio;
        private readonly HelpersPDF _helpersPDF;

        public ResultadoInforme(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            cod = (int)HttpContextAccessor.HttpContext.Session.GetInt32("num_auditoria_integral");
            anio = (int)HttpContextAccessor.HttpContext.Session.GetInt32("anio_auditoria_integral");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
        }

        public async Task<byte[]> CreateResultadoInforme()
        {
            //obtenemos el numero de auditorias informe
            var Infor = await _context.AU_TXT_INFOR_PRELIM
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .FirstOrDefaultAsync();


            var hallazgosAnteriores = await _context.MG_HALLAZGOS
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .ToListAsync();

            var hallazgosAllData = await _context.MG_HALLAZGOS
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .Include(d => d.Detalles)
                .Include(d => d.OrientacionCalificacion.Where(s => s.ESTADO == "A"))
                .Include(d => d.Documentos)
                .ToListAsync();


            //Obtenemos el o los cuestionarios agregados a la auditoria
            var data = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO == anio)
                    .Where(e => e.CODIGO_ESTADO == 3)
                    .ToListAsync();

            List<Mg_secciones> secciones = new List<Mg_secciones>();

            // Iteramos sobre los cuestionarios
            foreach (var item in data)
            {
                secciones = await _context.MG_SECCIONES
                    .Include(x => x.sub_secciones
                    .Where(sub => sub.CODIGO_CUESTIONARIO == item.CODIGO_CUESTIONARIO))
                    .ThenInclude(x => x.Preguntas_Cuestionarios)
                    .ToListAsync();
            }

            var seccInformesPreli = await _context.MG_SECC_INF_PRELI
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .OrderBy(d => d.CODIGO_SEC_INF)
                .ToListAsync();

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

                    pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, new HeaderEventHandler(document, "1"));

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
                                _helpersPDF.CreateTextFormat("INFORME PRELIMINAR AUDITORÍA INTEGRAL AGENCIA SAN MARCOS", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetTextAlignment(TextAlignment.CENTER)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("ELVIN NOEL ORELLANA", bfArialBd, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(12)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Jefe de Agencia", bfArial, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(5)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("CC.", bfArial, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(18)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Coordinador de Zona I", bfArial, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(5)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Fundación Microfinanciera Hermandad de Honduras, OPDF", bfArial, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(18)));

                    section1.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("San Marcos, Ocotepeque, 30 de junio 2024", bfArial, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(5)));

                    document.Add(section1);

                    //Agregarmos el motivo del informe
                    var section2 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section2.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("MOTIVO DEL INFORME", bfArialBd, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(18)));

                    document.Add(section2);

                    var motivo = "";
                    if (Infor != null && Infor.MOTIVO_INFORME != null)
                    {
                        motivo = Infor.MOTIVO_INFORME;
                    }
                    else
                    {
                        motivo = "<p>Como Auditores Internos de la Fundación Microfinanciera Hermandad de Honduras y dando cumplimiento al Plan Anual de Trabajo de Auditoría Interna para el año 2024, nos permitimos presentar el informe correspondiente a la Auditoría Agencia Santa Barbara.</p>";
                    }

                    var elements = HtmlConverter.ConvertToElements(motivo);

                    foreach (IElement element in elements)
                    {
                        if (element is Paragraph paragraph)
                        {
                            Paragraph styledParagraph = CrearParrafoConEstilo(paragraph, bfArial, bfArialBd);
                            document.Add(styledParagraph);
                        }
                        else if (element is List list)
                        {
                            list = EstilizarListItem(list, bfArial, bfArialBd);
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


                    //Agregarmos el motivo del informe
                    var section3 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();


                    section3.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("I. INTRODUCCIÓN", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10)));

                    section3.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("1. Objetivo", bfArialBd, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(10)));

                    document.Add(section3);

                    var objetivo = "";
                    if (Infor != null && Infor.OBJETIVO != null)
                    {
                        objetivo = Infor.OBJETIVO;
                    }
                    else
                    {
                        objetivo = "<p>Emitir una opinión independiente sobre el cumplimiento razonable en la Agencia respecto a las disposiciones contenidas en las normativas emitidas por el ente regulador y los manuales y procesos establecidos dentro de la Institución relacionada al control y calidad de las operaciones.</p>";
                    }

                    elements = HtmlConverter.ConvertToElements(objetivo);

                    foreach (IElement element in elements)
                    {
                        if (element is Paragraph paragraph)
                        {
                            Paragraph styledParagraph = CrearParrafoConEstilo(paragraph, bfArial, bfArialBd);
                            document.Add(styledParagraph);
                        }
                        else if (element is List list)
                        {
                            list = EstilizarListItem(list, bfArial, bfArialBd);
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

                    document.Close();

                    byte[] byteInfo = workStream.ToArray();
                    return byteInfo;
                }
            }
        }

        private Paragraph CrearParrafoConEstilo(Paragraph paragraph, PdfFont bfArial, PdfFont bfArialBd)
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

                    styledParagraph.Add(text.SetFontSize(10).SetFontColor(_helpersPDF.ColorGris()));
                }
                else if (childElement is LineSeparator lineSeparator)
                {
                    styledParagraph.Add(lineSeparator);
                }
            }

            return styledParagraph;
        }

        private List EstilizarListItem(List list, PdfFont bfArial, PdfFont bfArialBd)
        {
            foreach (ListItem item in list.GetChildren())
            {
                item.SetFont(bfArial).SetFontSize(10).SetFontColor(_helpersPDF.ColorGris());

                foreach (IElement listItemChild in item.GetChildren())
                {
                    if (listItemChild is Text text)
                    {
                        var existe = text.GetProperty<string>(95) == "bold";

                        if (existe)
                        {
                            text.SetFont(bfArialBd).SetFontSize(10).SetFontColor(_helpersPDF.ColorGris());
                        }
                        else
                        {
                            text.SetFont(bfArial).SetFontSize(10).SetFontColor(_helpersPDF.ColorGris());
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

                                    styledParagraph.Add(text2.SetFontSize(10).SetFontColor(_helpersPDF.ColorGris()));
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }
    }

}
