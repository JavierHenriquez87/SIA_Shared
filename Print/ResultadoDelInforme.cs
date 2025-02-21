using iText.Html2pdf;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIA.Context;
using SIA.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SIA.Print
{
    public class ResultadoDelInforme
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;
        private readonly HelpersQuestPDF _helpersQuestPDF;
        private readonly FooterEventHandlerQuest _footerEventHandler;
        private int cod;
        private int anio;

        public ResultadoDelInforme(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = HttpContextAccessor.HttpContext.Session.GetString("user");
            _helpersQuestPDF = new HelpersQuestPDF(context, config, HttpContextAccessor);
            _footerEventHandler = new FooterEventHandlerQuest();
            cod = (int)HttpContextAccessor.HttpContext.Session.GetInt32("num_auditoria_integral");
            anio = (int)HttpContextAccessor.HttpContext.Session.GetInt32("anio_auditoria_integral");
        }

        public async Task<byte[]> CreateResultadoDelInformePDF()
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);

                    // Crear una instancia de HeaderEventHandlerQuest con el id
                    var headerEventHandler = new HeaderEventHandlerQuest();

                    // Agregar el header
                    page.Header().Element(headerEventHandler.ComposeHeader);

                    // Agregar el contenido
                    page.Content().Element(container => ComposeContent(container));

                    // Agregar el footer
                    page.Footer().Element(_footerEventHandler.ComposeFooter);
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private async void ComposeContent(IContainer container)
        {
            var converter = new HtmlToPdfConverter();

            //obtenemos el numero de auditorias informe
            var Infor = await _context.AU_TXT_INFOR_PRELIM
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .FirstOrDefaultAsync();


            var hallazgosAnteriores = await _context.MG_HALLAZGOS
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .Include(h => h.comentarioAuditado)
                .ThenInclude(ca => ca.Mg_docs_auditado)
                .ToListAsync();

            var hallazgosAllData = await _context.MG_HALLAZGOS
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .Include(d => d.Detalles)
                .Include(d => d.OrientacionCalificacion.Where(s => s.ESTADO == "A"))
                .Include(d => d.Documentos)
                .ToListAsync();


            //Obtenemos el o los cuestionarios agregados a la auditoria
            var data = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO == anio)
                    .Where(e => e.CODIGO_ESTADO == 3)
                    .ToListAsync();

            List<Mg_secciones> secciones = new List<Mg_secciones>();

            // Iteramos sobre los cuestionarios
            foreach (var item in data)
            {
                secciones = await _context.MG_SECCIONES
                    .Include(x => x.sub_secciones
                    .Where(sub => sub.CODIGO_CUESTIONARIO == item.CODIGO_CUESTIONARIO))
                    .ThenInclude(x => x.Preguntas_Cuestionarios)
                    .ToListAsync();
            }

            var seccInformesPreli = await _context.MG_SECC_INF_PRELI
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .OrderBy(d => d.CODIGO_SEC_INF)
                .ToListAsync();

            List<ResultadoAuditoria> resultadosAuditorias = new List<ResultadoAuditoria>();

            foreach (var dataCuestionario in data)
            {
                var cuestionario = await _context.AU_CUESTIONARIOS.FirstOrDefaultAsync(d => d.CODIGO_CUESTIONARIO == dataCuestionario.CODIGO_CUESTIONARIO);

                var query = @"
                    SELECT 
                        s.descripcion_seccion AS Seccion,
                        ss.descripcion AS SubSeccion,
                        ROUND(
                            (
                                -- Suma ponderada de los porcentajes
                                SUM(CASE WHEN r.cumple = 1 THEN 1 ELSE 0 END) * 100 + 
                                SUM(CASE WHEN r.cumple_parcialmente = 1 THEN 0.5 ELSE 0 END) * 100
                            ) / 
                            NULLIF(
                                -- Total de preguntas que no son 'No Aplica'
                                COUNT(pc.codigo_pregunta) - SUM(CASE WHEN r.no_aplica = 1 THEN 1 ELSE 0 END), 
                                0
                            ), 
                            2
                        ) AS PorcentajeCumplimiento
                    FROM 
                        mg_auditorias_cuestionarios a
                    INNER JOIN 
                        mg_cuestionario_secciones cs ON a.codigo_cuestionario = cs.codigo_cuestionario
                    INNER JOIN 
                        mg_secciones s ON cs.codigo_seccion = s.codigo_seccion
                    INNER JOIN 
                        mg_sub_secciones ss ON ss.codigo_seccion = s.codigo_seccion 
                                            AND ss.codigo_cuestionario = cs.codigo_cuestionario
                    INNER JOIN 
                        mg_preguntas_cuestionario pc ON pc.codigo_sub_seccion = ss.codigo_sub_seccion
                                                        AND pc.codigo_cuestionario = cs.codigo_cuestionario
                    INNER JOIN 
                        mg_respuestas_cuestionario r ON r.codigo_pregunta = pc.codigo_pregunta 
                                                        AND r.codigo_auditoria_cuestionario = a.codigo_auditoria_cuestionario
                    WHERE 
                        a.numero_auditoria_integral = {0}
                        AND a.anio = {1}
                        AND a.codigo_cuestionario = {2}
                    GROUP BY 
                        s.descripcion_seccion, ss.descripcion
                    ORDER BY 
                        s.descripcion_seccion, ss.descripcion;
                ";

                var porcentaje = await _context.Porcentaje_SubSecciones
                    .FromSqlRaw(query, cod, anio, dataCuestionario.CODIGO_CUESTIONARIO)
                    .ToListAsync();

                resultadosAuditorias.Add(new ResultadoAuditoria
                {
                    CODIGO_CUESTIONARIO = cuestionario.CODIGO_CUESTIONARIO,
                    DESCRIPCION = cuestionario.NOMBRE_CUESTIONARIO,
                    listPorcentajeSubSecciones = porcentaje
                });
            }

            container.PaddingLeft(40).PaddingRight(40).PaddingTop(20).Column(column =>
            {
                column.Item().AlignCenter().PaddingBottom(5).Text(text =>
                {
                    text.Span("INFORME PRELIMINAR AUDITORÍA INTEGRAL AGENCIA SAN MARCOS")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                // Agregar una línea verde en la parte inferior
                column.Item().Height(1).Background("#007A3D"); // Puedes ajustar el alto para hacerlo más grueso si es necesario

                column.Item().PaddingTop(20).Text("Señores(as)")
                    .FontSize(11)
                    .FontFamily("Arial")
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(5).Text("ELVIN NOEL ORELLANA")
                    .FontFamily("Arial")
                    .FontSize(12)
                    .Bold()
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(5).Text("Jefe de Agencia")
                    .FontSize(11)
                    .FontFamily("Arial")
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(18).Text("CC.")
                    .FontSize(11)
                    .FontFamily("Arial")
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(5).Text("Coordinador de Zona I")
                    .FontSize(11)
                    .FontFamily("Arial")
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(18).Text("Fundación Microfinanciera Hermandad de Honduras, OPDF")
                    .FontSize(11)
                    .FontFamily("Arial")
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(5).Text("San Marcos, " + DateTime.Now.ToString("dd 'de' MMMM 'del' yyyy"))
                    .FontSize(11)
                    .FontFamily("Arial")
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(20).PaddingBottom(5).Text("MOTIVO DEL INFORME")
                    .FontFamily("Arial")
                    .FontSize(12)
                    .Bold()
                    .FontColor(_helpersQuestPDF.ColorGrisHtml());

                var motivo = "";
                if (Infor != null && Infor.MOTIVO_INFORME != null)
                {
                    motivo = Infor.MOTIVO_INFORME;
                }
                else
                {
                    motivo = "<p>Como Auditores Internos de la Fundación Microfinanciera Hermandad de Honduras y dando cumplimiento al Plan Anual de Trabajo de Auditoría Interna para el año 2024, nos permitimos presentar el informe correspondiente a la Auditoría Agencia Santa Barbara.</p>";
                }

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                var converter = new HtmlToPdfConverter();
                converter.AddHtmlContent(column, motivo, _helpersQuestPDF, 11);

                column.Item().AlignCenter().PaddingBottom(12).Text(text =>
                {
                    text.Span("I. INTRODUCCIÓN")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                column.Item().PaddingBottom(5).Text("1. Objetivo")
                   .FontFamily("Arial")
                   .FontSize(12)
                   .Bold()
                   .FontColor(_helpersQuestPDF.ColorGrisHtml());

                var objetivo = "";
                if (Infor != null && Infor.OBJETIVO != null)
                {
                    objetivo = Infor.OBJETIVO;
                }
                else
                {
                    objetivo = "<p>Emitir una opinión independiente sobre el cumplimiento razonable en la Agencia respecto a las disposiciones contenidas en las normativas emitidas por el ente regulador y los manuales y procesos establecidos dentro de la Institución relacionada al control y calidad de las operaciones.</p>";
                }

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                converter.AddHtmlContent(column, objetivo, _helpersQuestPDF, 11);

                column.Item().PaddingBottom(5).Text("2. Alcance")
                   .FontFamily("Arial")
                   .FontSize(12)
                   .Bold()
                   .FontColor(_helpersQuestPDF.ColorGrisHtml());

                var alcance = "";
                if (Infor != null && Infor.ALCANCE != null)
                {
                    alcance = Infor.ALCANCE;
                }
                else
                {
                    alcance = "<p>Nuestra revisión se realizó de conformidad con los manuales y políticas de la Fundación Micro Financiera HDH-OPDF haciendo su aplicación en todas las áreas de la Agencia que se encuentran en ejecución.</p><p><br></p><p>La Gerencia General es responsable de mantener los sistemas de control interno efectivos, la responsabilidad del Área de Auditoría Interna se limita a la revisión de los registros contenidos en los sistemas de la Fundación respecto a las operaciones efectuadas en su Oficina y la documentación de respaldo proporcionada por el personal y a la exposición de los resultados alcanzados basados en la revisión de dicha información.</p><p><br></p><p>De acuerdo a lo anterior, nuestro trabajo fue desarrollado basados en análisis sobre una muestra de las transacciones de 4 meses, por lo cual pueden existir desviaciones en el proceso auditado que no fueron identificadas por las técnicas de auditoría aplicadas en base a la NIA 530 Muestreo de Auditoría, esta norma permite al Auditor utilizar un enfoque de muestreo no estadístico, basado en las características del universo que se somete a prueba.</p><p><br></p><p>En tal sentido el alcance de la revisión corresponde al periodo comprendido entre el 01 de enero 2024 al 30 de abril del 2024, donde se verificó el cumplimiento con los manuales y políticas de la institución</p>";
                }

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                converter.AddHtmlContent(column, alcance, _helpersQuestPDF, 11);

                column.Item().PaddingBottom(5).Text("3. Resultados Generales")
                   .FontFamily("Arial")
                   .FontSize(12)
                   .Bold()
                   .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(10).PaddingBottom(12).Table(table =>
                {
                    // Configuración de las columnas (20%, 50%, 30%)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(20);
                        columns.RelativeColumn(50);
                        columns.RelativeColumn(30);
                    });

                    // Fila de encabezados
                    table.Cell().Row(1).Column(1).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("ID")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                        .FontFamily("Arial Bold");

                    table.Cell().Row(1).Column(2).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Hallazgos")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                        .FontFamily("Arial Bold");

                    table.Cell().Row(1).Column(3).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Prioridad")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                        .FontFamily("Arial Bold");

                    // Validación de datos
                    if (hallazgosAllData == null || hallazgosAllData.Count == 0)
                    {
                        // Celda única cuando no hay datos
                        table.Cell().Row(2).ColumnSpan(3).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Sin hallazgos encontrados")
                            .FontSize(10)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .FontFamily("Arial");
                    }
                    else
                    {
                        // Agregar datos dinámicamente
                        foreach (var HALLAZGO in hallazgosAllData)
                        {
                            var riesgo = HALLAZGO.NIVEL_RIESGO == 1 ? "BAJO" : HALLAZGO.NIVEL_RIESGO == 2 ? "MEDIO" : "ALTO";
                            var colorEstado = HALLAZGO.NIVEL_RIESGO == 1
                                ? _helpersQuestPDF.ColorBajoTablaHtml()
                                : HALLAZGO.NIVEL_RIESGO == 2
                                    ? _helpersQuestPDF.ColorMedioTablaHtml()
                                    : _helpersQuestPDF.ColorAltoTablaHtml();

                            // Columna 1: ID
                            table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).AlignCenter().Padding(4).AlignMiddle().Text(HALLAZGO.CODIGO_HALLAZGO.ToString())
                                .FontSize(11)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");

                            // Columna 2: Hallazgos
                            table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).PaddingLeft(6).AlignMiddle().Text(HALLAZGO.HALLAZGO)
                                .FontSize(11)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");

                            // Columna 3: Prioridad
                            table.Cell().Background(colorEstado).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text(riesgo)
                                .FontSize(9)
                                .Bold()
                                .FontColor(_helpersQuestPDF.ColorBlancoHtml())
                                .FontFamily("Arial Bold");
                        }
                    }
                });

                column.Item().PaddingBottom(5).Text("4. Seguimientos a Informes Anteriores")
                   .FontFamily("Arial")
                   .FontSize(12)
                   .Bold()
                   .FontColor(_helpersQuestPDF.ColorGrisHtml());

                column.Item().PaddingTop(10).PaddingBottom(12).Table(table =>
                {
                    // Configuración de las columnas (20%, 50%, 30%)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(20);
                        columns.RelativeColumn(50);
                        columns.RelativeColumn(30);
                    });

                    // Fila de encabezados
                    table.Cell().Row(1).Column(1).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("ID")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                        .FontFamily("Arial Bold");

                    table.Cell().Row(1).Column(2).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Hallazgos")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                        .FontFamily("Arial Bold");

                    table.Cell().Row(1).Column(3).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Prioridad")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                        .FontFamily("Arial Bold");

                    // Validación de datos
                    if (hallazgosAnteriores == null || hallazgosAnteriores.Count == 0)
                    {
                        // Celda única cuando no hay datos
                        table.Cell().Row(2).ColumnSpan(3).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Sin hallazgos encontrados")
                            .FontSize(10)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .FontFamily("Arial");
                    }
                    else
                    {
                        // Agregar datos dinámicamente
                        foreach (var HALLAZGO in hallazgosAnteriores)
                        {
                            var riesgo = HALLAZGO.NIVEL_RIESGO == 1 ? "BAJO" : HALLAZGO.NIVEL_RIESGO == 2 ? "MEDIO" : "ALTO";
                            var colorEstado = HALLAZGO.NIVEL_RIESGO == 1
                                ? _helpersQuestPDF.ColorBajoTablaHtml()
                                : HALLAZGO.NIVEL_RIESGO == 2
                                    ? _helpersQuestPDF.ColorMedioTablaHtml()
                                    : _helpersQuestPDF.ColorAltoTablaHtml();

                            // Columna 1: ID
                            table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).AlignCenter().Padding(4).AlignMiddle().Text(HALLAZGO.CODIGO_HALLAZGO.ToString())
                                .FontSize(11)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");

                            // Columna 2: Hallazgos
                            table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).PaddingLeft(6).AlignMiddle().Text(HALLAZGO.HALLAZGO)
                                .FontSize(11)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");

                            // Columna 3: Prioridad
                            table.Cell().Background(colorEstado).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text(riesgo)
                                .FontSize(9)
                                .Bold()
                                .FontColor(_helpersQuestPDF.ColorBlancoHtml())
                                .FontFamily("Arial Bold");
                        }
                    }
                });

                column.Item().PaddingBottom(5).Text("5. Conclusión General")
                   .FontFamily("Arial")
                   .FontSize(12)
                   .Bold()
                   .FontColor(_helpersQuestPDF.ColorGrisHtml());

                var conclusion = "";
                if (Infor != null && Infor.TEXTO_CONCLUSION_GENERAL != null)
                {
                    conclusion = Infor.TEXTO_CONCLUSION_GENERAL;
                }
                else
                {
                    conclusion = "<p>Sin Conclusión.</p>";
                }

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                converter.AddHtmlContent(column, conclusion, _helpersQuestPDF, 11);

                column.Item().AlignCenter().PaddingBottom(12).Text(text =>
                {
                    text.Span("II. PROCEDIMIENTOS DE LA AUDITORÍA")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                var procedimientoAuditoria = "";
                if (Infor != null && Infor.PROCEDIMIENTOS_AUDITORIA != null)
                {
                    procedimientoAuditoria = Infor.PROCEDIMIENTOS_AUDITORIA;
                }
                else
                {
                    procedimientoAuditoria = "<div><p>Los procedimientos y técnicas utilizadas para la realización de este trabajo,</p></div><div><p>fueron los siguientes:</p></div>";
                }

                // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                converter.AddHtmlContent(column, procedimientoAuditoria, _helpersQuestPDF, 11);

                column.Item().AlignCenter().PaddingTop(12).Text(text =>
                {
                    text.Span("III. RESULTADOS DE LA AUDITORÍA")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                if (resultadosAuditorias != null && resultadosAuditorias.Count > 0)
                {
                    foreach (var resultadoAuditoria in resultadosAuditorias)
                    {
                        column.Item().PaddingTop(12).PaddingBottom(12).PaddingLeft(80).PaddingRight(80).Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).AlignCenter().AlignMiddle().Table(table =>
                        {
                            // Encabezado de la tabla
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(150);
                                columns.RelativeColumn(1);
                            });

                            table.Cell().ColumnSpan(2).Background(_helpersQuestPDF.ColorVerdeFondoTablaHtml()).AlignCenter().Padding(5).Text("RESUMEN DE CONTROL - AGENCIA")
                                .FontColor(Colors.White)
                                .FontFamily("Arial")
                                .FontSize(11)
                                .Bold();

                            table.Cell().ColumnSpan(2).Background(_helpersQuestPDF.ColorVerdeFondoTablaHtml()).AlignCenter().Padding(5).Text(resultadoAuditoria.DESCRIPCION.ToUpper())
                                .FontColor(Colors.White)
                                .FontFamily("Arial")
                                .FontSize(11)
                                .Bold();

                            int conteo = 1;
                            string seccion = "";
                            decimal porcentaje = 0;

                            foreach (var porcentajeSubSeccion in resultadoAuditoria.listPorcentajeSubSecciones)
                            {
                                if (seccion != porcentajeSubSeccion.Seccion)
                                {
                                    seccion = porcentajeSubSeccion.Seccion;

                                    if (conteo == 1)
                                    {
                                        // Título de nueva sección
                                        table.Cell().Background(_helpersQuestPDF.ColorSubTituloFondoTablaHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text(seccion)
                                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                            .FontFamily("Arial")
                                            .FontSize(10)
                                            .Bold();

                                        table.Cell().Background(_helpersQuestPDF.ColorSubTituloFondoTablaHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text("PUNTAJE GRADO DE CUMPLIMIENTO")
                                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                            .FontFamily("Arial")
                                            .FontSize(10)
                                            .Bold();
                                    }
                                    else
                                    {
                                        // Título de nueva sección
                                        table.Cell().Background(_helpersQuestPDF.ColorSubTituloFondoTablaHtml()).BorderTop(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text(seccion)
                                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                            .FontFamily("Arial")
                                            .FontSize(10)
                                            .Bold();

                                        table.Cell().Background(_helpersQuestPDF.ColorSubTituloFondoTablaHtml()).BorderTop(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text("")
                                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                            .FontFamily("Arial")
                                            .FontSize(10)
                                            .Bold();

                                    }
                                }


                                // Celda de SubSección
                                table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text($"{conteo}. {porcentajeSubSeccion.SubSeccion.ToUpper()}")
                                    .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                    .FontFamily("Arial")
                                    .FontSize(10);

                                // Celda de Porcentaje
                                table.Cell().Background(GetColorForPercentage((double)porcentajeSubSeccion.PorcentajeCumplimiento))
                                    .Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml())
                                    .Padding(5).AlignCenter()
                                    .Text($"{porcentajeSubSeccion.PorcentajeCumplimiento}%")
                                    .FontColor((double)porcentajeSubSeccion.PorcentajeCumplimiento <= 50.90 ? Colors.White : _helpersQuestPDF.ColorGrisHtml())
                                    .Bold()
                                    .FontFamily("Arial")
                                    .FontSize(11);

                                conteo++;
                                porcentaje += porcentajeSubSeccion.PorcentajeCumplimiento;
                            }

                            // Cálculo del porcentaje total
                            double porcentajeTotal = (porcentaje != 0 ? Math.Round((double)porcentaje / (conteo - 1), 2) : 0);

                            // Fila del puntaje total
                            table.Cell().ColumnSpan(1).Background(_helpersQuestPDF.ColorVerdeLimonFondoTablaHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text("Puntaje de la Agencia...")
                                .FontColor(Colors.White)
                                .FontFamily("Arial")
                                .FontSize(11)
                                .Bold();

                            table.Cell().Background(GetColorForPercentage(porcentajeTotal)).Border(1).BorderColor(_helpersQuestPDF.ColorVerdeHtml()).Padding(5).AlignCenter().Text($"{porcentajeTotal}%")
                                .FontColor(porcentajeTotal <= 50.90 ? Colors.White : _helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial")
                                .FontSize(11);
                        });
                    }
                }

                // Agregar condición para verificar si hallazgos existen
                if (hallazgosAllData == null || hallazgosAllData.Count == 0)
                {
                    column.Item().PaddingTop(10).Text("Sin hallazgos encontrados")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .AlignCenter();
                }
                else
                {
                    foreach (var HALLAZGO in hallazgosAllData)
                    {
                        var calif = HALLAZGO.NIVEL_RIESGO == 1 ? " - BAJO" : HALLAZGO.NIVEL_RIESGO == 2 ? " - MEDIO" : HALLAZGO.NIVEL_RIESGO == 3 ? " - ALTO" : "";
                        var colorEstado = HALLAZGO.NIVEL_RIESGO == 1 ? _helpersQuestPDF.ColorBajoTablaHtml() : HALLAZGO.NIVEL_RIESGO == 2 ? _helpersQuestPDF.ColorMedioTablaHtml() : _helpersQuestPDF.ColorAltoTablaHtml();

                        column.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span(HALLAZGO.HALLAZGO.ToUpper())
                                .FontSize(12)
                                .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                                .FontFamily("Arial")
                                .Bold();
                            text.Span(calif)
                                .FontSize(12)
                                .FontColor(colorEstado)
                                .Italic();
                        });

                        if (HALLAZGO.CALIFICACION == 1)
                        {
                            column.Item().PaddingTop(10).PaddingBottom(12).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(25);
                                    columns.RelativeColumn(25);
                                    columns.RelativeColumn(25);
                                    columns.RelativeColumn(25);
                                });

                                // Fila de encabezados
                                table.Cell().Row(1).Column(1).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Valor de Muestra")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                                    .FontFamily("Arial Bold");

                                table.Cell().Row(1).Column(2).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Muestra con Hallazgos")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                                    .FontFamily("Arial Bold");

                                table.Cell().Row(1).Column(3).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Desviación de Muestra")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                                    .FontFamily("Arial Bold");

                                table.Cell().Row(1).Column(4).Background(_helpersQuestPDF.ColorGrisClaroHtml()).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text("Nivel de Riesgo")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(_helpersQuestPDF.ColorGrisOscuroHtml())
                                    .FontFamily("Arial Bold");

                                table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).AlignCenter().Padding(4).AlignMiddle().Text(HALLAZGO.VALOR_MUESTRA.ToString())
                                    .FontSize(11)
                                    .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                    .FontFamily("Arial");

                                table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).AlignCenter().Padding(4).AlignMiddle().Text(HALLAZGO.MUESTRA_INCONSISTENTE.ToString())
                                    .FontSize(11)
                                    .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                    .FontFamily("Arial");

                                table.Cell().Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).AlignCenter().Padding(4).AlignMiddle().Text(HALLAZGO.DESVIACION_MUESTRA + " %")
                                    .FontSize(11)
                                    .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                    .FontFamily("Arial");

                                table.Cell().Background(colorEstado).Border(1).BorderColor(_helpersQuestPDF.ColorGrisBordeTablaHtml()).Padding(4).AlignCenter().AlignMiddle().Text(HALLAZGO.NIVEL_RIESGO == 1 ? "BAJO" : HALLAZGO.NIVEL_RIESGO == 2 ? "MEDIO" : HALLAZGO.NIVEL_RIESGO == 3 ? "ALTO" : "")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(_helpersQuestPDF.ColorBlancoHtml())
                                    .BackgroundColor(colorEstado)
                                    .FontFamily("Arial Bold");
                            });

                        }

                        column.Item().PaddingTop(10).Text(HALLAZGO.CONDICION)
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text("Criterio: ")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text(HALLAZGO.CRITERIO)
                            .FontSize(10)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text("Efectos: ")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");

                        foreach (var CAUSAS in HALLAZGO.Detalles)
                        {
                            if (CAUSAS.TIPO.Contains("efecto"))
                            {
                                column.Item().PaddingTop(5).Text("• " + CAUSAS.DESCRIPCION)
                                .FontSize(10)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");
                            }
                        }

                        column.Item().PaddingTop(10).Text("Causas: ")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");
                        foreach (var CAUSAS in HALLAZGO.Detalles)
                        {
                            if (CAUSAS.TIPO.Contains("causa"))
                            {
                                column.Item().PaddingTop(5).Text("• " + CAUSAS.DESCRIPCION)
                                .FontSize(10)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");
                            }
                        }

                        column.Item().PaddingTop(10).Text("Proceso Asociado")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text("Proceso de Consulta en Central de Riesgo - FALTA")
                            .FontSize(10)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text("Acciones Requeridas: ")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");
                        foreach (var CAUSAS in HALLAZGO.Detalles)
                        {
                            if (CAUSAS.TIPO.Contains("accion_requerida - FALTA"))
                            {
                                column.Item().PaddingTop(5).Text("• " + CAUSAS.DESCRIPCION)
                                .FontSize(10)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");
                            }
                        }

                        column.Item().PaddingTop(10).Text("Contestación")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text("Contestación.......  - FALTA")
                            .FontSize(10)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).Text("Comentario del Auditado")
                            .FontSize(11)
                            .FontColor(_helpersQuestPDF.ColorGrisHtml())
                            .Bold()
                            .FontFamily("Arial");

                        column.Item().PaddingTop(10).PaddingBottom(10).Text("Comentario.......  - FALTA")
                                .FontSize(10)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial");
                    }
                }

                column.Item().AlignCenter().PaddingBottom(12).Text(text =>
                {
                    text.Span("IV. CONCLUSIONES")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                var conclusionesFinales = seccInformesPreli.FirstOrDefault(x => x.TITULO.Trim().Equals("IV. CONCLUSIONES", StringComparison.OrdinalIgnoreCase));

                if (conclusionesFinales != null)
                {
                    // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                    converter.AddHtmlContent(column, conclusionesFinales.TEXTO_SECCION, _helpersQuestPDF, 11);
                }
                else
                {
                    // Si no hay conclusiones, mostrar mensaje de "Sin conclusiones agregadas"
                    column.Item().PaddingTop(10).Text("Sin conclusiones agregadas.......")
                        .FontSize(10)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                }

                column.Item().AlignCenter().PaddingBottom(12).Text(text =>
                {
                    text.Span("V. RECOMENDACIONES DE LA AUDITORÍA")
                        .FontSize(13)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial")
                        .Bold();
                });

                var RecomendacionesFinales = seccInformesPreli.FirstOrDefault(x => x.TITULO.Trim().Equals("V. RECOMENDACIONES DE LA AUDITORÍA", StringComparison.OrdinalIgnoreCase));
                if (RecomendacionesFinales != null)
                {
                    // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                    converter.AddHtmlContent(column, RecomendacionesFinales.TEXTO_SECCION, _helpersQuestPDF, 11);
                }
                else
                {
                    // Si no hay conclusiones, mostrar mensaje de "Sin conclusiones agregadas"
                    column.Item().PaddingTop(10).Text("Sin recomendaciones agregadas.......")
                        .FontSize(10)
                        .FontColor(_helpersQuestPDF.ColorGrisHtml())
                        .FontFamily("Arial");
                }


                if (seccInformesPreli != null && seccInformesPreli.Count > 1)
                {
                    foreach (var secc in seccInformesPreli)
                    {
                        if (secc.TITULO == "IV. CONCLUSIONES" || secc.TITULO == "V. RECOMENDACIONES DE LA AUDITORÍA")
                        {
                            continue;
                        }

                        column.Item().AlignCenter().PaddingBottom(12).Text(text =>
                        {
                            text.Span("VI. COMENTARIOS DEL AUDITADO")
                                .FontSize(13)
                                .FontColor(_helpersQuestPDF.ColorGrisHtml())
                                .FontFamily("Arial")
                                .Bold();
                        });


                        // AGREGAMOS EL TEXTO QUE ES HTML AL PDF
                        converter.AddHtmlContent(column, RecomendacionesFinales.TEXTO_SECCION, _helpersQuestPDF, 11);
                    }
                }
            });
        }

        private string GetColorForPercentage(double porcentaje)
        {
            if (porcentaje <= 50.90)
                return _helpersQuestPDF.ColorRojoPorcentajeTablaHtml();
            if (porcentaje <= 65.90)
                return _helpersQuestPDF.ColorAnaranjadoPorcentajeTablaHtml();
            if (porcentaje <= 85.90)
                return _helpersQuestPDF.ColorAmarilloPorcentajeTablaHtml();
            return _helpersQuestPDF.ColorVerdePorcentajeTablaHtml();
        }
    }
}