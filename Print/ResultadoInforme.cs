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
using iText.Kernel.Colors;

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

                    //AGREGAMOS EL MOTIVO DEL INFORME
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
                            throw new InvalidOperationException($"Elemento no compatible: {element.GetType().Name}");
                        }
                    }


                    //AGREGAMOS LA INTRODUCCIÓN Y EL OBJETIVO
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
                            throw new InvalidOperationException($"Elemento no compatible: {element.GetType().Name}");
                        }
                    }

                    //AGREGAMOS EL ALCANCE
                    var section4 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("2. Alcance", bfArialBd, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(10)));

                    document.Add(section4);

                    var alcance = "";
                    if (Infor != null && Infor.ALCANCE != null)
                    {
                        alcance = Infor.ALCANCE;
                    }
                    else
                    {
                        alcance = "<p>Nuestra revisión se realizó de conformidad con los manuales y políticas de la Fundación Micro Financiera HDH-OPDF haciendo su aplicación en todas las áreas de la Agencia que se encuentran en ejecución.</p><p><br></p><p>La Gerencia General es responsable de mantener los sistemas de control interno efectivos, la responsabilidad del Área de Auditoría Interna se limita a la revisión de los registros contenidos en los sistemas de la Fundación respecto a las operaciones efectuadas en su Oficina y la documentación de respaldo proporcionada por el personal y a la exposición de los resultados alcanzados basados en la revisión de dicha información.</p><p><br></p><p>De acuerdo a lo anterior, nuestro trabajo fue desarrollado basados en análisis sobre una muestra de las transacciones de 4 meses, por lo cual pueden existir desviaciones en el proceso auditado que no fueron identificadas por las técnicas de auditoría aplicadas en base a la NIA 530 Muestreo de Auditoría, esta norma permite al Auditor utilizar un enfoque de muestreo no estadístico, basado en las características del universo que se somete a prueba.</p><p><br></p><p>En tal sentido el alcance de la revisión corresponde al periodo comprendido entre el 01 de enero 2024 al 30 de abril del 2024, donde se verificó el cumplimiento con los manuales y políticas de la institución</p>";
                    }

                    elements = HtmlConverter.ConvertToElements(alcance);

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
                            throw new InvalidOperationException($"Elemento no compatible: {element.GetType().Name}");
                        }
                    }

                    //AGREGAMOS EL RESULTADO GENERAL
                    var section5 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("3. Resultados Generales", bfArialBd, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(10)));

                    document.Add(section5);

                    // Crear la tabla de resultados generales
                    var section5_Table = new Table(UnitValue.CreatePercentArray(new float[] { 20, 50, 30 }))
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(10)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(5); 

                    section5_Table.AddCell(new Cell()
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("ID", bfArialBd, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                        .SetBackgroundColor(_helpersPDF.ColorGrisClaro()));

                    section5_Table.AddCell(new Cell()
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("Hallazgos", bfArialBd, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                        .SetBackgroundColor(_helpersPDF.ColorGrisClaro()));

                    section5_Table.AddCell(new Cell()
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("Prioridad", bfArialBd, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                        .SetBackgroundColor(_helpersPDF.ColorGrisClaro()));


                    if (hallazgosAllData == null || hallazgosAllData.Count == 0)
                    {
                        section5_Table.AddCell(new Cell(1, 3)
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("Sin hallazgos encontrados", bfArial, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1)));
                    }
                    else
                    {
                        foreach (var HALLAZGO in hallazgosAllData)
                        {
                            var riesgo = HALLAZGO.NIVEL_RIESGO == 1 ? "BAJO" : HALLAZGO.NIVEL_RIESGO == 2 ? "MEDIO" : "ALTO";

                            iText.Kernel.Colors.Color colorEstado = HALLAZGO.NIVEL_RIESGO == 1 ? _helpersPDF.ColorBajoTabla() : HALLAZGO.NIVEL_RIESGO == 2 ? _helpersPDF.ColorMedioTabla() : _helpersPDF.ColorAltoTabla();

                            section5_Table.AddCell(new Cell()
                                .Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(@HALLAZGO.CODIGO_HALLAZGO.ToString(), bfArial, 10)
                                    .SetFontColor(_helpersPDF.ColorGris())))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1)));

                            section5_Table.AddCell(new Cell()
                                .Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(@HALLAZGO.HALLAZGO, bfArial, 10)
                                    .SetFontColor(_helpersPDF.ColorGris())))
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1)).SetPaddingLeft(6));

                            section5_Table.AddCell(new Cell()
                                .Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(riesgo, bfArialBd, 9)
                                    .SetFontColor(_helpersPDF.ColorBlanco())))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                                .SetBackgroundColor(colorEstado));
                        }
                    }

                    // Agregar la tabla al documento
                    document.Add(section5_Table);

                    //AGREGAMOS EL SEGUIMIENTO A INFORMES ANTERIORES
                    var section6 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    section6.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("4. Seguimientos a Informes Anteriores", bfArialBd, 10)
                                .SetFontColor(_helpersPDF.ColorGris())
                                ).SetMarginTop(18)));

                    document.Add(section6);

                    // Crear la tabla de seguimientos a informes anteriores
                    var section6_Table = new Table(UnitValue.CreatePercentArray(new float[] { 20, 50, 30 }))
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(10)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(5);

                    section6_Table.AddCell(new Cell()
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("ID", bfArialBd, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                        .SetBackgroundColor(_helpersPDF.ColorGrisClaro()));

                    section6_Table.AddCell(new Cell()
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("Hallazgos", bfArialBd, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                        .SetBackgroundColor(_helpersPDF.ColorGrisClaro()));

                    section6_Table.AddCell(new Cell()
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("Prioridad", bfArialBd, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                        .SetBackgroundColor(_helpersPDF.ColorGrisClaro()));


                    if (hallazgosAnteriores == null || hallazgosAnteriores.Count == 0)
                    {
                        section6_Table.AddCell(new Cell(1, 3)
                        .Add(new Paragraph(
                            _helpersPDF.CreateTextFormat("Sin hallazgos encontrados", bfArial, 10)
                            .SetFontColor(_helpersPDF.ColorGris())))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1)));
                    }
                    else
                    {
                        foreach (var HALLAZGO in hallazgosAnteriores)
                        {
                            var riesgo = HALLAZGO.NIVEL_RIESGO == 1 ? "BAJO" : HALLAZGO.NIVEL_RIESGO == 2 ? "MEDIO" : "ALTO";

                            iText.Kernel.Colors.Color colorEstado = HALLAZGO.NIVEL_RIESGO == 1 ? _helpersPDF.ColorBajoTabla() : HALLAZGO.NIVEL_RIESGO == 2 ? _helpersPDF.ColorMedioTabla() : _helpersPDF.ColorAltoTabla();

                            section6_Table.AddCell(new Cell()
                                .Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(@HALLAZGO.CODIGO_HALLAZGO.ToString(), bfArial, 10)
                                    .SetFontColor(_helpersPDF.ColorGris())))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1)));

                            section6_Table.AddCell(new Cell()
                                .Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(@HALLAZGO.HALLAZGO, bfArial, 10)
                                    .SetFontColor(_helpersPDF.ColorGris())))
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1)).SetPaddingLeft(6));

                            section6_Table.AddCell(new Cell()
                                .Add(new Paragraph(
                                    _helpersPDF.CreateTextFormat(riesgo, bfArialBd, 9)
                                    .SetFontColor(_helpersPDF.ColorBlanco())))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(_helpersPDF.ColorGrisBordeTabla(), 1))
                                .SetBackgroundColor(colorEstado));
                        }
                    }

                    // Agregar la tabla al documento
                    document.Add(section6_Table);


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
