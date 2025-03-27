using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;
using SIA.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Newtonsoft.Json;
using System.Globalization;

namespace SIA.Print
{
    public class CartaSalida
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersQuestPDF _helpersQuestPDF;
        private readonly FooterEventHandlerQuest _footerEventHandler;

        public CartaSalida(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersQuestPDF = new HelpersQuestPDF(context, config, HttpContextAccessor);
            _footerEventHandler = new FooterEventHandlerQuest();
        }

        public async Task<byte[]> CreateCartaSalidaPDF(string id)
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
                                .FirstOrDefaultAsync(u => u.TIPO_CARTA == 2 && u.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == dataAI.ANIO_AI);

            if (carta == null)
            {

                int maxNumeroRegistro = await _context.MG_CARTAS
                    .MaxAsync(a => (int?)a.CODIGO_CARTA) ?? 0;

                Mg_cartas nuevaCarta = new();
                nuevaCarta.CODIGO_CARTA = maxNumeroRegistro + 1;
                nuevaCarta.TEXTO_CARTA = "<p data-pm-slice=\"0 0 []\">Para efectos de expresar una conformidad sobre el cumplimiento de controles, internos, Resoluciones, Procesos, reglamentos internos y manuales operativos.</p>\r\n<p>Confirmo que he cumplido con mi responsabilidad, tal como se establecen en el memorándum de solicitud y en la carta de entrada, con respecto a la presentación de toda la información misma que es transparente, confiable, oportuna, libre de errores de incorrección material y fraudes.</p>\r\n<p><strong>Les hemos proporcionado</strong></p>\r\n<ol>\r\n<li>Acceso a toda la información que tengo conocimiento que sea relevante en la preparación del informe, tal como registros, documentación y otro material;</li>\r\n<li>Información adicional que solicitaron para los fines de la auditoría; y</li>\r\n<li>Acceso ilimitado al personal de la Unidad de Auditoría Interna en la realización de su trabajo, a manera de obtener las evidencias sobre las inconsistencias encontradas.</li>\r\n<li>Les manifiesto que la información proporcionada para la realización de su trabajo, está libre de incorrecciones materiales debida a fraudes de la que tenemos conocimiento y que afecta a la Agencia e implica a los empleados que desempeñan funciones significativas en el control interno.</li>\r\n<li>Le he revelado toda la información relativa a denuncias de fraude o a indicios defraude que afectan a los estados financieros de la Institución, comunicada por empleados, antiguos empleados y clientes.</li>\r\n<li>Le he revelado todos los casos conocidos de incumplimiento o sospecha de incumplimiento de las disposiciones legales y reglamentarias cuyos efectos deberían considerarse para preparar los estados financieros.</li>\r\n</ol>\r\n<p>Le he revelado la identidad de las partes vinculadas con los Empleados y todas las relaciones y transacciones con partes vinculadas de las que tengo conocimiento.</p>";
                nuevaCarta.FECHA_CREACION = DateTime.Now;
                nuevaCarta.CREADO_POR = "PROVISIONAL";
                nuevaCarta.NUMERO_AUDITORIA_INTEGRAL = dataAI.NUMERO_AUDITORIA_INTEGRAL;
                nuevaCarta.ANIO_AI = dataAI.ANIO_AI;
                nuevaCarta.TIPO_CARTA = 2;


                _context.Add(nuevaCarta);

                //Guardamos el rol editado
                await _context.SaveChangesAsync();

                carta = await _context.MG_CARTAS
                                .FirstOrDefaultAsync(u => u.TIPO_CARTA == 2 && u.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == dataAI.ANIO_AI);
            }

            string fechaInicioVisita = dataAI.FECHA_INICIO_VISITA?.ToString("dd MMMM yyyy");
            string fechaFinVisita = dataAI.FECHA_FIN_VISITA?.ToString("dd MMMM 'del año' yyyy");

            string ownerNameAuditado = null;
            string jobPositionAuditado = null;
            string formattedDateAuditado = null;

            var firmaAuditado = await _context.MG_FIRMAS_CARTAS
                .FirstOrDefaultAsync(u => u.CODIGO_CARTA == dataAI.CODIGO_AUDITORIA && u.TIPO_CARTA == 2);

            if (firmaAuditado != null)
            {
                dynamic firmaData = JsonConvert.DeserializeObject<dynamic>(firmaAuditado.FIRMA);

                ownerNameAuditado = firmaData.ownerName;
                jobPositionAuditado = firmaData.jobPositionName;
                string generationDateStr = firmaData.generationDate;

                // Convertir la fecha correctamente manejando la zona horaria
                DateTimeOffset generationDateOffset = DateTimeOffset.Parse(generationDateStr, CultureInfo.InvariantCulture);

                // Convertir a DateTime en la hora local del sistema
                DateTime generationDate = generationDateOffset.DateTime;

                // Formatear la fecha al estilo requerido
                formattedDateAuditado = generationDate.ToString("dd / MM / yyyy  HH:mm:ss");
            }

            container.PaddingLeft(40).PaddingRight(40).PaddingTop(20).Column(column =>
            {
                // Agregar la imagen centrada con un ancho de 4.26 cm
                column.Item().AlignCenter().Width(4.26f * 28.35f).Image("wwwroot/assets/images/logoNew.png", ImageScaling.FitWidth);

                // Agregar el texto alineado a la derecha
                column.Item().PaddingRight(40).Text(text =>
                {
                    text.Span(id)
                        .FontColor("#80BD9F")
                        .FontSize(11)
                        .FontFamily("Arial")
                        .Bold();
                    text.AlignRight();
                });

                column.Item().Text(text =>
                {
                    text.Span(DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"))
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial");
                });

                // Nombre 
                column.Item().PaddingTop(20).Text(text =>
                {
                    text.Span("AUDITORÍA INTERNA")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial")
                        .Bold();
                });

                // Cargo del jefe de agencia
                column.Item().Text(text =>
                {
                    text.Span("Fundación Microfinanciera Hermandad de Honduras, OPDF")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial");
                });

                // Nombre de la fundación
                column.Item().Text(text =>
                {
                    text.Span("San Marcos, Ocotepeque")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial");
                });

                // Primer párrafo
                column.Item().PaddingTop(20).PaddingBottom(14).Text(text =>
                {
                    text.Span("Esta carta de manifestaciones se proporciona en relación con su auditoría interna detodas las transacciones de cartera de préstamos, cartera de ahorros, movimientosdiarios de caja de ventanilla, caja de reserva, activos fijos, y libros de su Agencia,correspondientes al periodo comprendido de ")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial");


                    text.Span(fechaInicioVisita + " a " + fechaFinVisita)
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial")
                        .Bold();

                    text.Span(".")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                        .FontFamily("Arial");
                });

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                var converter = new HtmlToPdfConverter();
                converter.AddHtmlContent(column, carta.TEXTO_CARTA ?? "", _helpersQuestPDF);

                column.Item().PaddingTop(20).Row(row =>
                {
                    // Segunda firma
                    if (firmaAuditado != null)
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Row(rowImg =>
                            {
                                rowImg.ConstantItem(60).Image("wwwroot/assets/images/logoNew.png", ImageScaling.FitWidth);

                                rowImg.RelativeItem().Column(textCol =>
                                {
                                    textCol.Item().Text("Firmado digitalmente por:")
                                        .FontSize(10)
                                        .FontFamily("Arial")
                                        .FontColor(Colors.Grey.Darken1);

                                    textCol.Item().Text(ownerNameAuditado)
                                        .FontSize(13)
                                        .FontFamily("Arial Bold")
                                        .FontColor(Colors.Green.Darken2); // Ajusta el color según la imagen

                                    textCol.Item().Text(jobPositionAuditado)
                                        .FontSize(12)
                                        .FontFamily("Arial")
                                        .FontColor(Colors.Grey.Darken1);

                                    textCol.Item().Text(formattedDateAuditado)
                                        .FontSize(10)
                                        .FontFamily("Arial")
                                        .FontColor(Colors.Grey.Darken2);
                                });
                            });
                        });
                    }
                });
            });
        }

    }
}