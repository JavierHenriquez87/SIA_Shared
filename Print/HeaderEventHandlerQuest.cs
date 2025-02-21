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
        container.Stack(stack =>
        {
            // Agregar la imagen centrada con un ancho de 4.26 cm
            stack.Item().AlignCenter().Width(4.26f * 28.35f).Image("wwwroot/assets/images/logoNew.png", ImageScaling.FitWidth);

            // Agregar el texto alineado a la derecha
            stack.Item().PaddingRight(40).Text(text =>
            {
                text.Span(_id)
                    .FontColor("#80BD9F")
                    .FontSize(11)
                    .FontFamily("Arial")
                    .Bold();
                text.AlignRight();
            });
        });
    }





}