using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class HeaderEventHandlerQuest
{
    public void ComposeHeader(IContainer container)
    {
        // Definir el ancho de la página
        var pageWidth = PageSizes.Letter.Width;

        // Crear una tabla para el header
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(80); // 80% del ancho
                columns.RelativeColumn(20); // 20% del ancho
            });

            // Agregar el logo del banco
            table.Cell().Element(ComposeLogo);

            // Espacio vacío para la segunda columna (opcional)
            table.Cell().Element(ComposeEmptySpace);
        });
    }

    private void ComposeLogo(IContainer container)
    {
        // Cargar la imagen del logo
        container.Image("wwwroot/assets/images/pdf-footer.png", ImageScaling.FitArea)
                 .Width(620) // Ancho de la imagen
                 .Height(120); // Alto de la imagen
    }

    private void ComposeEmptySpace(IContainer container)
    {
        // Espacio vacío (opcional)
        container.Background(Colors.Transparent);
    }
}