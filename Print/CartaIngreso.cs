using iTextSharp.tool.xml.html.head;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;
using SIA.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SIA.Print
{
    public class CartaIngreso
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersQuestPDF _helpersQuestPDF;
        private readonly FooterEventHandlerQuest _footerEventHandler;

        public CartaIngreso(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersQuestPDF = new HelpersQuestPDF(context, config, HttpContextAccessor);
            _footerEventHandler = new FooterEventHandlerQuest();
        }

        public async Task<byte[]> CreateCartaIngresoPDF(string id)
        {

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);

                    // Crear una instancia de HeaderEventHandlerQuest con el id
                    var headerEventHandler = new HeaderEventHandlerQuest(id);

                    // Agregar el header
                    page.Header().ShowIf(context => context.PageNumber > 1).Element(headerEventHandler.ComposeHeader);

                    // Agregar el contenido
                    page.Content().Element(container => ComposeContent(container, id));

                    // Agregar el footer
                    page.Footer().Element(container => _footerEventHandler.ComposeFooter(container, false));
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }


        private async void ComposeContent(IContainer container, string id)
        {
            //Obtenemos informacion de la planificacion de la auditoria
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                                .FirstOrDefaultAsync(u => u.CODIGO_AUDITORIA == id);

            var carta = await _context.MG_CARTAS
                                .FirstOrDefaultAsync(u => u.TIPO_CARTA == 1 && u.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == dataAI.ANIO_AI);

            if (carta == null)
            {

                int maxNumeroRegistro = await _context.MG_CARTAS
                    .MaxAsync(a => (int?)a.CODIGO_CARTA) ?? 0;

                Mg_cartas nuevaCarta = new();
                nuevaCarta.CODIGO_CARTA = maxNumeroRegistro + 1;
                nuevaCarta.TEXTO_CARTA = "<p style=\"font-size: 16px\" data-pm-slice=\"1 3 []\">Realizaremos nuestra auditoría con el objetivo de expresar nuestros comentarios, observaciones y recomendaciones sobre las inconsistencias encontradas en dicha revisión.</p><p style=\"font-size: 16px\">Una auditoría conlleva la aplicación de procedimientos para obtener evidencia de auditoría sobre los importes y la información revelada en los estados financieros, comprobando el cumplimiento de los procesos, manuales, reglamentos, normativas y las circulares emitidas por la Administración y/o CNBS.</p><p style=\"font-size: 16px\">Los procedimientos seleccionados dependen del juicio del auditor, incluida la valoración de los riesgos de incorrección material en los estados financieros, debida a fraude o error.</p><p style=\"font-size: 16px\">La Unidad de Auditoría Interna realiza revisiones de forma continua desde la Oficina Principal a través del Sistema Integral de Auditoria y solicitando información a las Agencias vía electrónica, identificado inconsistencias que serán consideradas al momento de emitir el informe de la auditoría realizada a su Agencia.</p><p style=\"font-size: 16px\"></p><p style=\"font-size: 16px\">Debido a las limitaciones inherentes a la auditoría, junto con las limitaciones inherentes al control interno, existe un riesgo inevitable de que puedan no detectarse algunas incorrecciones materiales, aun cuando la auditoría se planifique y ejecute adecuadamente.</p><p style=\"font-size: 16px\"></p><p style=\"font-size: 16px\">Al efectuar nuestras valoraciones del riesgo verificaremos el cumplimiento del control interno implementado por la Administración, identificando algunos riesgos y controles que no se han tomado en cuenta por la Administración, comunicándoles por escrito cualquier inconsistencia significativa en el control interno, relevante para la auditoría que identifiquemos durante la realización de la misma.</p><p style=\"font-size: 16px\"></p><p style=\"font-size: 16px\">Realizaremos la auditoría considerando que la estructura organizativa de la Institución, reconocen y comprenden que el Jefe de Agencia es el responsable de:</p><ol><li style=\"font-size: 16px;\">La preparación, presentación y resguardo de la información de todas las transacciones realizadas en su Agencia</li><li style=\"font-size: 16px;\">Fiel en el cumplimiento de todos los manuales, reglamentos, normativas y circulares emitidas por la Administración.</li><li style=\"font-size: 16px;\">El control interno que la Agencia considere necesario para permitir la preparación de toda la información, misma que debe ser transparente, confiable, oportuna, libre de errores e incorrección material y fraudes.</li></ol><p style=\"font-size: 16px\"><br></p><p style=\"font-size: 16px\" data-pm-slice=\"1 1 []\"><b>Jefe de Agencia debe proporcionarnos</b></p><ol><li style=\"font-size: 16px;\">Acceso a toda la información que tenga conocimiento que sea relevante en la preparación de nuestro informe, tal como registros, documentación y otros datos.</li><li style=\"font-size: 16px;\">Información adicional que podamos solicitar para los fines de la auditoría;</li><li style=\"font-size: 16px;\">Acceso ilimitado al personal de la Unidad de Auditoría Interna en la realización de nuestro trabajo a manera de obtener evidencia de las inconsistencias encontradas.</li></ol><p style=\"font-size: 16px\">Como parte de nuestro proceso de auditoría, solicitaremos por escrito al Jefe de Agencia que la información suministrada sea verdadera y que pueda ser verificada cuando se estime conveniente.</p><p style=\"font-size: 16px\">Esperamos contar con la plena colaboración de sus empleados durante nuestra auditoría, es posible que la estructura y el contenido de nuestro informe tengan que ser modificados en función de las inconsistencias identificadas en nuestra auditoría.</p><p style=\"font-size: 16px\">Le rogamos que firme y devuelva la copia adjunta de esta carta para indicar que conoce y acepta los acuerdos relativos a nuestra auditoría, de todas las transacciones que comprenden cartera de préstamos, cartera de ahorros, movimientos diarios de caja de ventanilla, caja de reserva y libros.</p>";
                nuevaCarta.FECHA_CREACION = DateTime.Now;
                nuevaCarta.CREADO_POR = "PROVISIONAL";
                nuevaCarta.NUMERO_AUDITORIA_INTEGRAL = dataAI.NUMERO_AUDITORIA_INTEGRAL;
                nuevaCarta.ANIO_AI = dataAI.ANIO_AI;
                nuevaCarta.TIPO_CARTA = 1;


                _context.Add(nuevaCarta);

                //Guardamos el rol editado
                await _context.SaveChangesAsync();

                carta = await _context.MG_CARTAS
                                    .FirstOrDefaultAsync(u => u.TIPO_CARTA == 1 && u.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == dataAI.ANIO_AI);
            }

            string fechaInicioVisita = dataAI.FECHA_INICIO_VISITA?.ToString("dd MMMM yyyy");
            string fechaFinVisita = dataAI.FECHA_FIN_VISITA?.ToString("dd MMMM 'del año' yyyy");

            string fechaInicioRevision = dataAI.PERIODO_INICIO_REVISION?.ToString("dd MMMM yyyy");
            string fechaFinRevision = dataAI.PERIODO_FIN_REVISION?.ToString("dd MMMM 'del año' yyyy");

            container.PaddingLeft(40).PaddingRight(40).PaddingTop(20).Column(column =>
            {
                // Agregar la imagen centrada con un ancho de 4.26 cm
                column.Item().AlignCenter().Width(4.26f * 28.35f).Image("wwwroot/assets/images/logoNew.png", ImageScaling.FitWidth);

                // Agregar el texto alineado a la derecha
                column.Item().PaddingRight(40).Text(text =>
                {
                    //text.Span(_id)
                    //    .FontColor("#80BD9F")
                    //    .FontSize(11)
                    //    .FontFamily("Arial")
                    //    .Bold();
                    //text.AlignRight();
                });

                column.Item().Text(text =>
                {
                    text.Span(DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"))
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Saludo inicial
                column.Item().PaddingTop(20).Text(text =>
                {
                    text.Span("Señor:")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Nombre del jefe de agencia
                column.Item().Text(text =>
                {
                    text.Span("JERSON ELY VELASQUEZ PEREZ")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                // Cargo del jefe de agencia
                column.Item().Text(text =>
                {
                    text.Span("Jefe de Agencia")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Nombre de la fundación
                column.Item().Text(text =>
                {
                    text.Span("Fundación Microfinanciera Hermandad de Honduras, OPDF")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Saludo formal
                column.Item().PaddingTop(20).Text(text =>
                {
                    text.Span("Estimado Jefe de Agencia")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Primer párrafo
                column.Item().PaddingTop(14).Text(text =>
                {
                    text.Span("Hemos sido asignados a la revisión periódica de todas sus transacciones realizadas en su Agencia, en el periodo comprendido de ")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");


                    text.Span(fechaInicioVisita + " a " + fechaFinVisita)
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();

                    text.Span(".")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Segundo párrafo
                column.Item().PaddingTop(14).PaddingBottom(14).Text(text =>
                {
                    text.Span("Con mandato de la Junta de Vigilancia nos solicitan que auditemos todas las transacciones que comprenden la cartera de préstamos, cartera de ahorros, movimientos diarios de caja de ventanilla, caja de reserva, inventario de activos fijos y libros de su Agencia correspondientes al periodo comprendido de ")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");

                    text.Span(fechaInicioRevision + " a " + fechaFinRevision)
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();

                    text.Span(".")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                var converter = new HtmlToPdfConverter();
                converter.AddHtmlContent(column, carta.TEXTO_CARTA ?? "", _helpersQuestPDF);

                column.Item().Row(row =>
                {
                    // Primera firma
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Width(100).AlignCenter().PaddingLeft(30).Image("wwwroot/Archivos/Usuarios/Firmas_Usuarios/firmaSergio.png", ImageScaling.FitWidth);

                        col.Item().Text("Lic. Sergio Antonio Melgar")
                            .FontSize(13)
                            .FontFamily("Arial")
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .AlignCenter();

                        col.Item().Text("Auditor Interno")
                            .FontSize(13)
                            .FontFamily("Arial Bold")
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .AlignCenter();
                    });

                    // Segunda firma
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Width(100).AlignCenter().PaddingLeft(30).Image("wwwroot/Archivos/Usuarios/Firmas_Usuarios/firmaSergio.png", ImageScaling.FitWidth);

                        col.Item().Text("Jerson Ely Velasquez Perez")
                            .FontSize(13)
                            .FontFamily("Arial")
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .AlignCenter();

                        col.Item().Text("Jefe de Agencia")
                            .FontSize(13)
                            .FontFamily("Arial Bold")
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .AlignCenter();
                    });
                });
            });
        }

    }
}