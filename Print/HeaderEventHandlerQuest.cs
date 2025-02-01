using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

public class HeaderEventHandlerQuest
{
    private readonly string _id;

    public HeaderEventHandlerQuest(string id = "")
    {
        _id = id;
    }

    public void ComposeHeader(IContainer container)
    {
        // Definir el ancho de la página
        var pageWidth = PageSizes.Letter.Width;

        // Crear una tabla para el header
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(30); 
                columns.RelativeColumn(0); // 100% del ancho
            });

            // Agregar el logo del banco y el texto superpuesto
            table.Cell().Element(ComposeLogoAndText);
        });
    }

    private void ComposeLogoAndText(IContainer container)
    {
        // Usar un Stack para superponer la imagen y el texto
        container.Stack(stack =>
        {
            // Agregar la imagen
            stack.Item().Image("wwwroot/assets/images/pdf-headers.png", ImageScaling.FitArea);

            // Agregar el texto superpuesto
            stack.Item().PaddingRight(40).Text(text =>
            {
                text.Span(_id)
                    .FontColor("#80BD9F") // Color del texto (código hexadecimal)
                    .FontSize(11) // Tamaño de la fuente
                    .FontFamily("Arial") // Fuente
                    .Bold();
                text.AlignRight();

            });
        });
    }
}