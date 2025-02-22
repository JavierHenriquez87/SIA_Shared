using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SIA.Print
{
    public class Memorandum
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersQuestPDF _helpersQuestPDF;
        private readonly FooterEventHandlerQuest _footerEventHandler;

        public Memorandum(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersQuestPDF = new HelpersQuestPDF(context, config, HttpContextAccessor);
            _footerEventHandler = new FooterEventHandlerQuest();
        }

        public async Task<byte[]> CreateMemorandumPDF(string id)
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
            var converter = new HtmlToPdfConverter();

            //Obtenemos informacion de la planificacion de la auditoria
            var dataMDP = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                .FirstOrDefaultAsync(u => u.CODIGO_MEMORANDUM == id);

            var listAuditores = await _context.AU_AUDITORES_ASIGNADOS
                .Include(x => x.mg_usuarios)
                .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == dataMDP.NUMERO_AUDITORIA_INTEGRAL)
                .Where(e => e.ANIO_AI == dataMDP.ANIO_MDP)
                .OrderBy(e => e.CODIGO_USUARIO)
                .ToListAsync();

            //Obtenemos informacion de la auditoria integral
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == dataMDP.NUMERO_AUDITORIA_INTEGRAL)
                                .Where(u => u.ANIO_AI == dataMDP.ANIO_MDP)
                                .FirstOrDefaultAsync();

            DateTime? fechaInicioVisita = dataAI.FECHA_INICIO_VISITA;
            string fechaInicio = fechaInicioVisita?.ToString("dd/MM/yyyy");
            DateTime? fechaFinVisita = dataAI.FECHA_FIN_VISITA;
            string fechaFin = fechaFinVisita?.ToString("dd/MM/yyyy");

            container.PaddingLeft(40).PaddingRight(40).PaddingTop(20).Column(column =>
            {
                // Agregar la imagen centrada con un ancho de 4.26 cm
                column.Item().AlignCenter().Width(4.26f * 28.35f).Image("wwwroot/assets/images/logoNew.png", ImageScaling.FitWidth);

                column.Item().AlignCenter().PaddingBottom(20).Text(text =>
                {
                    text.Span("MEMORÁNDUM DE PLANIFICACIÓN")
                        .FontSize(16)
                        .FontColor(_helpersQuestPDF.ColorAnaranjadoHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                column.Item().PaddingBottom(12).Text(text =>
                {
                    text.Span("San Marcos, Ocotepeque, " + DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"))
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorAzulHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                column.Item().PaddingBottom(10).Text(text =>
                {
                    text.Span("Introducción:")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorCafeHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).PaddingLeft(14).Text(text =>
                    {
                        text.Span("1.")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });

                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span("Tipo de Auditoria:")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });
                });

                // Segunda fila: "Auditoria de [nombre]"
                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).Text("");

                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span("Auditoria de " + dataAI.NOMBRE_AUDITORIA)
                            .FontSize(12)
                            .FontColor(Colors.Black)
                            .FontFamily("Arial");
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).PaddingLeft(14).Text(text =>
                    {
                        text.Span("2.")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });

                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span("Objetivos:")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).Text("");
                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span(dataMDP.OBJETIVO ?? "")
                            .FontSize(12)
                            .FontColor(Colors.Black)
                            .FontFamily("Arial");
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).PaddingLeft(14).Text(text =>
                    {
                        text.Span("3.")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });

                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span("Equipo:")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).Text("");
                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span(dataMDP.TEXTO_EQUIPO_TRABAJO ?? "")
                            .FontSize(12)
                            .FontColor(Colors.Black)
                            .FontFamily("Arial");
                    });
                });

                // Recorrer la lista de auditores
                foreach (var auditores in listAuditores)
                {
                    // Fila para cada auditor
                    column.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem(12).PaddingRight(8).AlignRight().Text(text =>
                        {
                            text.Span("•")
                                .FontSize(14)
                                .FontColor(Colors.Black)
                                .FontFamily("Arial")
                                .Bold();
                        });

                        row.RelativeItem(1).Text("");

                        row.RelativeItem(90).Text(text =>
                        {
                            text.Span(auditores.mg_usuarios.NOMBRE_USUARIO)
                                .FontSize(12)
                                .FontColor(Colors.Black)
                                .FontFamily("Arial");
                        });
                    });
                }


                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).PaddingLeft(14).Text(text =>
                    {
                        text.Span("4.")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });

                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span("Recurso:")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).Text("");
                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span(dataMDP.RECURSOS ?? "")
                            .FontSize(12)
                            .FontColor(Colors.Black)
                            .FontFamily("Arial");
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).PaddingLeft(14).Text(text =>
                    {
                        text.Span("5.")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });

                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span("Tiempo:")
                            .FontSize(13)
                            .FontColor(_helpersQuestPDF.ColorVerdeHtml())
                            .FontFamily("Arial")
                            .Bold();
                    });
                });

                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem(8).Text("");
                    row.RelativeItem(95).Text(text =>
                    {
                        text.Span(dataMDP.TEXTO_TIEMPO_AUDITORIA + " " + fechaInicio + " al " + fechaFin)
                            .FontSize(12)
                            .FontColor(Colors.Black)
                            .FontFamily("Arial");
                    });
                });

            });
        }

    }
}