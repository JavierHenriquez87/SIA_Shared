using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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
                                .FirstOrDefaultAsync(u => u.TIPO_CARTA == 2 && u.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == dataAI.ANIO_AI);

            string fechaInicioVisita = dataAI.FECHA_INICIO_VISITA?.ToString("dd MMMM yyyy");
            string fechaFinVisita = dataAI.FECHA_FIN_VISITA?.ToString("dd MMMM 'del año' yyyy");

            container.PaddingLeft(40).PaddingRight(40).PaddingTop(20).Column(column =>
            {
                column.Item().Text(text =>
                {
                    text.Span(DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"))
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Nombre 
                column.Item().PaddingTop(20).Text(text =>
                {
                    text.Span("AUDITORÍA INTERNA")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                // Cargo del jefe de agencia
                column.Item().Text(text =>
                {
                    text.Span("Fundación Microfinanciera Hermandad de Honduras, OPDF")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Nombre de la fundación
                column.Item().Text(text =>
                {
                    text.Span("San Marcos, Ocotepeque")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                });

                // Primer párrafo
                column.Item().PaddingTop(20).PaddingBottom(14).Text(text =>
                {
                    text.Span("Esta carta de manifestaciones se proporciona en relación con su auditoría interna detodas las transacciones de cartera de préstamos, cartera de ahorros, movimientosdiarios de caja de ventanilla, caja de reserva, activos fijos, y libros de su Agencia,correspondientes al periodo comprendido de ")
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

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                var converter = new HtmlToPdfConverter();
                converter.AddHtmlContent(column, carta.TEXTO_CARTA ?? "", _helpersQuestPDF);

                column.Item().PaddingTop(20).Row(row =>
                {
                    // Segunda firma
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Width(100).AlignCenter().PaddingLeft(30).Image("wwwroot/Archivos/Usuarios/Firmas_Usuarios/firmaSergio.png", ImageScaling.FitWidth);

                        col.Item().Text("Jerson Ely Velasquez Perez")
                            .FontSize(13)
                            .FontFamily("Arial")
                            .FontColor(_helpersQuestPDF.ColorGrisHtml());

                        col.Item().Text("Jefe de Agencia")
                            .FontSize(13)
                            .FontFamily("Arial Bold")
                            .FontColor(_helpersQuestPDF.ColorGrisHtml());
                    });
                });
            });
        }

    }
}