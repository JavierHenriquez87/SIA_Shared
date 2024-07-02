using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Kernel.Events;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.IO.Font.Constants;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.IO.Image;
using iText.Kernel.Geom;
using SIA.Context;
using SIA.Print;
using Microsoft.AspNetCore.Http;

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

        public async Task<byte[]> CreateMemorandumPlanificacion()
        {
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
                    string id = "MDP-UAI-00-2024";
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
                                _helpersPDF.CreateTextFormat("San Marcos, Ocotepeque, 31 de mayo del 2024", bfArialBd, 13)
                                .SetFontColor(_helpersPDF.ColorAzul())
                                )));

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
                                _helpersPDF.CreateTextFormat("Auditoria de cumplimiento sobre Riesgo de Liquidez", bfArial, 12)))).SetMarginBottom(10);

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

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section3.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("revisar la efectividad y la eficiencia de los controles internos y los procedimientos de gestión del riesgo de liquidez, revisando que se alineen con las normativas vigentes establecidas por la Comisión Nacional de Bancos y Seguros (CNBS) y que la entidad esté preparada para manejar situaciones de estrés de liquidez de manera adecuada.", bfArial, 12)))).SetMarginBottom(10);

                    document.Add(section3);

                    // OTRA LINEA
                    var section4 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 2, 90 })).SetWidth(UnitValue.CreatePercentValue(100));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("1.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Revisar la Estructura Organizacional:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1,2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Verificar que la entidad tenga una estructura organizacional clara y bien definida para la gestión del riesgo de liquidez, con roles y responsabilidades adecuadamente asignados a los miembros del Comité de Activos y Pasivos (CAPA) y del Comité de Crisis.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("2.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Evaluar las Políticas de Gestión del Riesgo de Liquidez:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Asegurar que existan políticas y procedimientos documentados y aprobados por la Junta Directiva o el Consejo de Administración, que cubran todos los aspectos relevantes de la gestión del riesgo de liquidez.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("3.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Revisar la Definición y Cumplimiento de Límites Internos:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Evaluar la adecuación de los límites internos establecidos para la exposición al riesgo de liquidez y verificar el cumplimiento de estos límites mediante el análisis de informes y datos históricos.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("4.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Analizar los Escenarios de Estrés:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Revisar los escenarios de estrés desarrollados por la entidad para evaluar su capacidad de respuesta ante crisis de liquidez, asegurando que sean realistas y representativos de posibles situaciones de estrés.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("5.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Verificar los Indicadores de Alerta Temprana:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Identificar y evaluar la implementación y el monitoreo de indicadores de alerta temprana utilizados para detectar posibles problemas de liquidez de manera oportuna.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("6.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Evaluar los Planes de Contingencia:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Asegurar que existan planes de contingencia bien documentados y actualizados para gestionar situaciones de crisis de liquidez y evaluar la preparación del personal para implementar dichos planes.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("7.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Revisar los Sistemas de Información:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Evaluar la efectividad y confiabilidad de los sistemas de información utilizados para la gestión y el reporte del riesgo de liquidez, asegurando que proporcionen datos precisos y oportunos.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("8.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Asegurar la Documentación y Comunicación Efectiva:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Verificar que la documentación y los procedimientos de gestión del riesgo de liquidez estén adecuadamente documentados y que exista una comunicación efectiva entre las diferentes áreas involucradas.", bfArial, 12)).SetMarginBottom(10)));

                    // OTRA LINEA
                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("9.", bfArial, 13)).SetMarginLeft(14)));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Cumplir con las Normas Regulatoria:", bfArialBd, 13))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section4.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section4.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Evaluar el cumplimiento de las normas y regulaciones establecidas por la CNBS y otros organismos regulatorios, asegurando que la entidad cumpla con los requerimientos de reporte y divulgación de información sobre el riesgo de liquidez.", bfArial, 12)).SetMarginBottom(10)));

                    document.Add(section4);

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
                                _helpersPDF.CreateTextFormat("La auditoría será ejecuta en dos etapas por el siguiente personal asignado:", bfArial, 12))));

                    document.Add(section5);

                    section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 6, 90 })).SetWidth(UnitValue.CreatePercentValue(100));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section5.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Aldany Josué Ramírez (asistente de Auditoria)", bfArial, 12)).SetMarginBottom(10)));

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

                    section5 = new Table(UnitValue.CreatePercentArray(new float[] { 8, 6, 90 })).SetWidth(UnitValue.CreatePercentValue(100));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section5.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Se solicitará información al área de Finanzas", bfArial, 12))));

                    cell = _helpersPDF.CreateTableCellNoBorder(1, 2);
                    section5.AddCell(cell.Add(new Paragraph(_helpersPDF.CreateTextFormat("•", bfArialBd, 14))
                        .SetTextAlignment(TextAlignment.RIGHT).SetMarginRight(8).SetPadding(0)).SetVerticalAlignment(VerticalAlignment.TOP));

                    cell = _helpersPDF.CreateTableCellNoBorder();
                    section5.AddCell(cell.Add(new Paragraph(
                                _helpersPDF.CreateTextFormat("Se utilizará computadora asignada para realizar la auditoria", bfArial, 12))));

                    document.Add(section5);

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
                                _helpersPDF.CreateTextFormat("Se estima que la primera etapa dure 1 mes desde la entrega de la información solicitada", bfArial, 12))));

                    document.Add(section5);

                    document.Close();

                    byte[] byteInfo = workStream.ToArray();
                    return byteInfo;
                }
            }
        }
        
    }

}
