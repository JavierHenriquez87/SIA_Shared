using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SIA.Print
{
    public class CartaIngreso
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersPDF _helpersPDF;
        private readonly FooterEventHandlerQuest _footerEventHandler;

        public CartaIngreso(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
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
                    page.Header().Element(headerEventHandler.ComposeHeader);

                    // Agregar el contenido
                    page.Content().Element(container => ComposeContent(container, id));

                    // Agregar el footer
                    page.Footer().Element(_footerEventHandler.ComposeFooter);
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

            string fechaInicioVisita = dataAI.FECHA_INICIO_VISITA?.ToString("dd MMMM yyyy");
            string fechaFinVisita = dataAI.FECHA_FIN_VISITA?.ToString("dd MMMM 'del año' yyyy");

            string fechaInicioRevision = dataAI.PERIODO_INICIO_REVISION?.ToString("dd MMMM yyyy");
            string fechaFinRevision = dataAI.PERIODO_FIN_REVISION?.ToString("dd MMMM 'del año' yyyy");

            container.Padding(40).Column(column =>
            {
                column.Item().Text(text =>
                {
                    text.Span(DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"))
                        .FontSize(14)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Saludo inicial
                column.Item().PaddingTop(12).Text(text =>
                {
                    text.Span("Señor:")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Nombre del jefe de agencia
                column.Item().Text(text =>
                {
                    text.Span("JERSON ELY VELASQUEZ PEREZ")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                // Cargo del jefe de agencia
                column.Item().Text(text =>
                {
                    text.Span("Jefe de Agencia")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Nombre de la fundación
                column.Item().Text(text =>
                {
                    text.Span("Fundación Microfinanciera Hermandad de Honduras, OPDF")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Saludo formal
                column.Item().PaddingTop(16).Text(text =>
                {
                    text.Span("Estimado Jefe de Agencia")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Primer párrafo
                column.Item().PaddingTop(14).Text(text =>
                {
                    text.Span("Hemos sido asignados a la revisión periódica de todas sus transacciones realizadas en su Agencia, en el periodo comprendido de ")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");


                    text.Span(fechaInicioVisita + " a " + fechaFinVisita)
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();

                    text.Span(".")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Segundo párrafo
                column.Item().PaddingTop(14).Text(text =>
                {
                    text.Span("Con mandato de la Junta de Vigilancia nos solicitan que auditemos todas las transacciones que comprenden la cartera de préstamos, cartera de ahorros, movimientos diarios de caja de ventanilla, caja de reserva, inventario de activos fijos y libros de su Agencia correspondientes al periodo comprendido de ")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");

                    text.Span(fechaInicioRevision + " a " + fechaFinRevision)
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();

                    text.Span(".")
                        .FontSize(13)
                        .FontColor(_helpersPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });
            });
        }

        public string ColorCafe()
        {
            return "#8B4513";
        }
    }
}