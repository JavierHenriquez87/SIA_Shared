using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class FooterEventHandlerQuest
{
    public void ComposeFooter(IContainer container, bool validate = false)
    {
        // Definir el ancho de la página
        var pageWidth = PageSizes.Letter.Width;

        // Crear una tabla para el header
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(30); // 80% del ancho
                columns.RelativeColumn(0); // 20% del ancho
            });

            // Agregar el logo del banco
            if(!validate)
            {
                table.Cell().Element(ComposeLogo);
            }
            else // Agregar solo espacio
            {
                table.Cell().Height(60);
            }
            // Espacio vacío para la segunda columna (opcional)
            table.Cell().Element(ComposeEmptySpace);
        });
    }

    private void ComposeLogo(IContainer container)
    {
        // Cargar la imagen del logo
        container.Image("wwwroot/assets/images/pdf-footer.png", ImageScaling.FitWidth); // Alto de la imagen
    }

    private void ComposeEmptySpace(IContainer container)
    {
        // Espacio vacío (opcional)
        container.Background(Colors.Transparent);
    }
}