﻿using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.AspNetCore.Mvc;
using SIA.Context;
using SIA.Print;
using System.Drawing;

namespace SIA.Controllers
{
    public class PrintController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;
        private ErrorPDF errorPDF;
        private MemorandumPlanificacion memorandumPlanificacion;

        public PrintController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexAsync(string doc, string id)
        {
            MemoryStream workStream = new MemoryStream();
            byte[] pdfGenerateMemoryStream;
            switch (doc)
            {
                case "memorandumPlanificacion":
                    memorandumPlanificacion = new MemorandumPlanificacion(_context, _config, _contextAccessor);
                    pdfGenerateMemoryStream = await memorandumPlanificacion.CreateMemorandumPlanificacion(id);
                    break;

                default:
                    errorPDF = new ErrorPDF(_context, _config, _contextAccessor);
                    pdfGenerateMemoryStream = await errorPDF.createErrorPDF();
                    break;
            }

            if (pdfGenerateMemoryStream.Length == 0)
            {
                errorPDF = new ErrorPDF(_context, _config, _contextAccessor);
                pdfGenerateMemoryStream = await errorPDF.createErrorPDF();
            }

            var documentoBase64 = Convert.ToBase64String(pdfGenerateMemoryStream);
            TempData["Base64PDF"] = documentoBase64;

            return View();
        }
    }
}