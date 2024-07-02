using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Kernel.Events;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.IO.Font.Constants;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.IO.Image;
using iText.Kernel.Geom;
using SIA.Context;
using SIA.Print;
using Microsoft.AspNetCore.Http;

namespace SIA.Print
{
    public class ErrorPDF
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersPDF _helpersPDF;

        public ErrorPDF(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersPDF = new HelpersPDF(context, config, HttpContextAccessor);
        }

        public async Task<byte[]> createErrorPDF()
        {
            MemoryStream workStream = new MemoryStream();

            using (var pdfWriter = new PdfWriter(workStream))
            {
                pdfWriter.SetCloseStream(false);
                using (PdfDocument pdfDoc = new PdfDocument(pdfWriter))
                using (Document document = new Document(pdfDoc, PageSize.LETTER))
                {
                    document.SetMargins(105, 40, 80, 40);

                    PdfFont bfArial = PdfFontFactory.CreateFont("c:/windows/fonts/Arial.ttf", PdfEncodings.IDENTITY_H);
                    PdfFont bfArialBd = PdfFontFactory.CreateFont("c:/windows/fonts/arialbd.ttf", PdfEncodings.IDENTITY_H);

                    //CELDAS PARA UTILIZAR EN LAS TABLAS
                    Cell cell = new Cell();

                    string id = "";

                    pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, new HeaderEventHandler(document, id));

                    //AGREGAMOS EL FOOTER DE LAS PAGINAS
                    pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, new TextFooterEventHandler(document));

                    //GENERAMOS UN ESPACIO PARA SER UTILIZADO DONDE SEA NECESARIO
                    Paragraph sectionSpacing = new Paragraph(" ").SetFontSize(4);

                    document.Add(sectionSpacing);

                    var sectionI_1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));
                    cell = _helpersPDF.CreateTableCellNoBorder();

                    sectionI_1.AddCell(
                        cell.Add(
                            new Paragraph(
                                _helpersPDF.CreateTextFormat("NO HEMOS ENCONTRADO LA INFORMACION SOLICITADA, POR FAVOR HACER LA BUSQUEDA EN LA BASE DE DATOS O SOLICITAR INFORMACION A UN SUPERIOR.", bfArial, 12)
                                .SetFontColor(_helpersPDF.ColorCafe())
                                )
                            )
                        );

                    document.Add(sectionI_1);

                    document.Close();

                    byte[] byteInfo = workStream.ToArray();
                    return byteInfo;
                }
            }
        }
        
    }

}
