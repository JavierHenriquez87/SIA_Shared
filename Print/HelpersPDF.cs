﻿using iText.Layout;
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

namespace SIA.Print
{
    public class HelpersPDF
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;

        public HelpersPDF(AppDbContext context, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = httpContextAccessor.HttpContext.Session.GetString("user");
        }

        public Color ColorAnaranjado()
        {
            Color color = new DeviceRgb(239, 140, 74);
            return color;
        }

        public Color ColorAzul()
        {
            Color color = new DeviceRgb(68, 114, 196);
            return color;
        }

        public Color ColorCafe()
        {
            Color color = new DeviceRgb(131, 60, 11);
            return color;
        }

        public Color ColorVerde()
        {
            Color color = new DeviceRgb(0, 122, 61);
            return color;
        }

        public Text CreateTextFormat(string texto, PdfFont font, int size)
        {
            Text textParagraph;
            textParagraph = new Text(texto ?? "").SetFont(font).SetFontSize(size);
            return textParagraph;
        }

        public Cell CreateTableCellNoBorder(int row = 1, int col = 1)
        {
            Cell someCell;
            someCell = new Cell(row, col)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingTop(0)
                .SetPaddingBottom(-1)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
            return someCell;
        }
    }

    public class TextFooterEventHandler : IEventHandler
    {
        protected Document doc;
        private string _user;

        public TextFooterEventHandler(Document doc)
        {
            this.doc = doc;
        }

        public void HandleEvent(Event currentEvent)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
            Rectangle pageSize = docEvent.GetPage().GetPageSize();
            float width = pageSize.GetWidth() - doc.GetRightMargin() - doc.GetLeftMargin();
            PdfPage page = docEvent.GetPage();
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

            float coordX2 = ((pageSize.GetLeft() + doc.GetLeftMargin())
                            + (pageSize.GetRight() - doc.GetRightMargin()) + 575) / 2;
            float footerY = doc.GetBottomMargin() + 42;
            Rectangle rect = new Rectangle(coordX2, footerY);

            PdfFont bfTimesBold = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

            //IMAGEN DE LOGO DE BANCO
            Image logo = new Image(ImageDataFactory.Create("wwwroot/assets/images/pdf-footer.png"));
            logo.ScaleAbsolute(620, 120);

            Cell cell = new Cell();

            var tableFooter = new Table(UnitValue.CreatePercentArray(new float[] { 80, 20 })).SetWidth(UnitValue.CreatePercentValue(100));

            //LOGO DEL BANCO
            cell = new Cell(2, 1).Add(logo);
            cell.SetBorder(Border.NO_BORDER);
            tableFooter.AddCell(cell);

            tableFooter.SetVerticalAlignment(VerticalAlignment.BOTTOM);
            tableFooter.SetMarginLeft(-2);
            tableFooter.SetBorder(Border.NO_BORDER);
            new Canvas(canvas, rect).Add(tableFooter).Close();
        }
    }

    public class HeaderEventHandler : IEventHandler
    {
        protected Document doc;
        private string _id;

        public HeaderEventHandler(Document doc, string id)
        {
            this.doc = doc;
            _id = id;
        }

        public void HandleEvent(Event currentEvent)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
            Rectangle pageSize = docEvent.GetPage().GetPageSize();
            PdfPage page = docEvent.GetPage();
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

            float coordX2 = ((pageSize.GetLeft() + doc.GetLeftMargin())
                            + (pageSize.GetRight() - doc.GetRightMargin()) + 575) / 2;
            float headerY = pageSize.GetTop() - doc.GetTopMargin() + 85;

            Rectangle rect = new Rectangle(coordX2, headerY);

            PdfFont bfArialBd = PdfFontFactory.CreateFont("c:/windows/fonts/arialbd.ttf", PdfEncodings.IDENTITY_H);

            //IMAGEN DE LOGO DE BANCO
            Image logo = new Image(ImageDataFactory.Create("wwwroot/assets/images/pdf-headers.png"));
            logo.ScaleAbsolute(620,82);

            Cell cell = new Cell();

            var tableHeader = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).SetWidth(UnitValue.CreatePercentValue(100));

            // Crear un contenedor para la imagen y el texto
            Div imageTextContainer = new Div();

            // Texto superpuesto
            Paragraph textOverlay = new Paragraph(_id)
                .SetFont(bfArialBd)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingLeft(340)
                .SetMarginTop(-45)
                .SetFontColor(new DeviceRgb(128, 189, 159));

            // Añadir la imagen y el texto al contenedor
            imageTextContainer.Add(logo);
            imageTextContainer.Add(textOverlay);

            // Añadir el contenedor a la celda de la tabla
            tableHeader.AddCell(new Cell().Add(imageTextContainer).SetBorder(Border.NO_BORDER));


            tableHeader.SetVerticalAlignment(VerticalAlignment.BOTTOM);
            tableHeader.SetMarginLeft(-2);
            tableHeader.SetMarginTop(-10);
            tableHeader.SetBorder(Border.NO_BORDER);
            new Canvas(canvas, rect).Add(tableHeader).Close();
        }
    }
}