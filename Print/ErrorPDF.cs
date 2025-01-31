using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;

namespace SIA.Print
{
    public class ErrorPDF
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersPDF _helpersPDF;
        private readonly HeaderEventHandlerQuest _headerEventHandler;
        private readonly FooterEventHandlerQuest _footerEventHandler;

        public ErrorPDF(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
            _headerEventHandler = new HeaderEventHandlerQuest();
            _footerEventHandler = new FooterEventHandlerQuest();
        }

        public async Task<byte[]> CreateErrorPDF()
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    //page.MarginTop(100);    // Margen superior
                    //page.MarginRight(40);  // Margen derecho
                    //page.MarginBottom(80); // Margen inferior
                    //page.MarginLeft(40);   // Margen izquierdo

                    // Agregar el header
                    page.Header().Element(_headerEventHandler.ComposeHeader);

                    // Agregar el contenido
                    page.Content().Element(ComposeContent);

                    // Agregar el footer
                    page.Footer().Element(_footerEventHandler.ComposeFooter);
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private void ComposeContent(IContainer container)
        {
            container.Padding(20).Column(column =>
            {
                column.Spacing(4);

                column.Item().Text(text =>
                {
                    text.Span("NO HEMOS ENCONTRADO LA INFORMACION SOLICITADA, POR FAVOR HACER LA BUSQUEDA EN LA BASE DE DATOS O SOLICITAR INFORMACION A UN SUPERIOR.")
                         .FontColor(_helpersPDF.ColorCafeHtml())
                         .FontSize(12)
                         .FontFamily("Arial");
                });
            });
        }

    }
}