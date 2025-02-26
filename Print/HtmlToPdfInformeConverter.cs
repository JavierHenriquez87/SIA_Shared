using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SIA.Print
{
    public class HtmlToPdfInformeConverter
    {
        public void AddHtmlContent(ColumnDescriptor column, string htmlContent, HelpersQuestPDF _helpersQuestPDF, int fontSize = 13, float sangria = 0)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    switch (node.Name.ToLower())
                    {
                        case "p":
                            column.Item().AlignLeft().PaddingLeft(sangria).Text(text =>
                            {
                                foreach (var childNode in node.ChildNodes)
                                {
                                    if (childNode.NodeType == HtmlNodeType.Text)
                                    {
                                        // Si es texto plano
                                        text.Span(childNode.InnerText)
                                            .FontSize(fontSize)
                                            .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                                            .FontFamily("Arial");
                                    }
                                    else if (childNode.Name.ToLower() == "b" || childNode.Name.ToLower() == "strong")
                                    {
                                        // Si es un elemento <b> o <strong>
                                        text.Span(childNode.InnerText)
                                            .FontSize(fontSize)
                                            .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                                            .FontFamily("Arial")
                                            .Bold();
                                    }
                                }
                            });
                            column.Item().AlignLeft().PaddingLeft(sangria).PaddingBottom(10);
                            break;

                        case "ul": // Lista no ordenada (viñetas)
                            foreach (var li in node.SelectNodes("li"))
                            {
                                column.Item()
                                    .PaddingLeft(sangria)
                                    .PaddingBottom(10)
                                    .Text(text =>
                                    {
                                        text.Span("• " + li.InnerText)
                                            .FontSize(fontSize)
                                            .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                                            .FontFamily("Arial");
                                    });
                            }
                            break;

                        case "ol": // Lista ordenada (números)
                            int counter = 1;
                            foreach (var li in node.SelectNodes("li"))
                            {
                                column.Item()
                                    .AlignLeft()
                                    .PaddingLeft(sangria)
                                    .PaddingBottom(10)
                                    .Text(text =>
                                    {
                                        text.Span($"{counter}. {li.InnerText}") // Usa números para listas ordenadas
                                            .FontSize(fontSize)
                                            .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                                            .FontFamily("Arial");
                                        counter++;
                                    });
                            }
                            break;

                        case "b":
                        case "strong":
                            column.Item().AlignLeft().PaddingLeft(sangria).Text(text =>
                            {
                                text.Span(node.InnerText)
                                    .FontSize(fontSize)
                                    .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                                    .FontFamily("Arial")
                                    .Bold();
                            });
                            break;

                        default:
                            // Si no es un elemento reconocido, simplemente lo agregamos como texto
                            column.Item().AlignLeft().PaddingLeft(sangria).Text(text =>
                            {
                                text.Span(node.InnerText)
                                    .FontSize(fontSize)
                                    .FontColor(_helpersQuestPDF.ColorNegroPrincipal())
                                    .FontFamily("Arial");
                            });
                            break;
                    }
                }
            }
        }
    }
}