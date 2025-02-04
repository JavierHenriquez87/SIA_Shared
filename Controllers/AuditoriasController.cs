using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIA.Context;
using SIA.Models;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using SIA.Print;

namespace SIA.Controllers
{
    public class AuditoriasController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuditoriasController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;
        }

        //********************************************************************************
        //MENU PROGRAMAR AUDITORIA INTEGRALES
        //********************************************************************************

        /// <summary>
        /// Pagina principal para programar auditorias integrales
        /// </summary>
        /// <returns></returns>
        public IActionResult ProgramarAuditoria()
        {
            return View();
        }


        /// <summary>
        /// Pagina principal con listado de auditorias integrales
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias")]
        public IActionResult Auditorias()
        {
            return View();
        }


        /// <summary>
        /// Obtener las auditorias Integrales
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetAuditoriasIntegrales()
        {
            var estadoRequest = Request.Form["estado"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault().ToUpper();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault().ToUpper();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            List<Au_auditorias_integrales> data = new List<Au_auditorias_integrales>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue))
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderBy(e => e.FECHA_CREACION)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue))
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS_INTEGRALES
                    .CountAsync(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderBy(e => e.FECHA_CREACION)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS_INTEGRALES
                    .CountAsync();
            }

            // Recorre cada auditoría y realizamos las actualizaciones necesarias
            foreach (var dataAI in data)
            {
                dataAI.CANTIDAD_AUD_ESPEC = await _context.AU_AUDITORIAS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == dataAI.NUMERO_AUDITORIA_INTEGRAL)
                    .Where(e => e.ANIO_AE == dataAI.ANIO_AI)
                    .Where(e => e.CODIGO_AUDITORIA != "Pendiente Confirmación")
                    .CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }


        /// <summary>
        /// Guardar nueva Auditoria Integral
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Guardar_Auditoria_Integral([FromBody] Au_auditorias_integrales DataAI)
        {
            int nuevoIdAuditoria = 0;

            try
            {
                if (DataAI == null) return new JsonResult("error");

                //Obtenemos el codigo del siguiente registro segun el anio actual
                int maxNumeroAuditoriaIntegral = await _context.AU_AUDITORIAS_INTEGRALES
                    .Where(a => a.ANIO_AI == DateTime.Now.Year)
                    .MaxAsync(a => (int?)a.NUMERO_AUDITORIA_INTEGRAL) ?? 0;

                // Incrementar el valor máximo en 1
                nuevoIdAuditoria = maxNumeroAuditoriaIntegral + 1;

                //Obtenemos informacion complementaria del universo auditable
                var universos = await _context.MG_UNIVERSO_AUDITABLE
                                    .Where(u => u.CODIGO_UNIVERSO_AUDITABLE == DataAI.CODIGO_UNIVERSO_AUDITABLE)
                                    .FirstOrDefaultAsync();

                DataAI.NUMERO_AUDITORIA_INTEGRAL = nuevoIdAuditoria;
                DataAI.CODIGO_AUDITORIA = DataAI.CODIGO_UNIVERSO_AUDITABLE + "-" + nuevoIdAuditoria + "-" + DateTime.Now.Year;
                DataAI.NOMBRE_AUDITORIA = universos?.NOMBRE;
                DataAI.CREADO_POR = HttpContext.Session.GetString("user");
                DataAI.FECHA_CREACION = DateTime.Now;
                DataAI.CODIGO_ESTADO = 1;
                DataAI.HABILITADA = "N";
                DataAI.MDP_EMITIDO = "N";
                DataAI.PDT_EMITIDO = "N";
                DataAI.CI_RECIBIDO = "N";
                DataAI.IP_ENVIADO = "N";
                DataAI.IP_RECIBIDO = "N";
                DataAI.CS_RECIBIDA = "N";
                DataAI.IF_EMITIDO = "N";
                DataAI.IF_RECIBIDO = "N";
                DataAI.PA_EMITIDO = "N";
                DataAI.PA_RECIBIDO = "N";
                DataAI.AUDITORIA_ANULADA = "N";
                DataAI.ANIO_AI = DateTime.Now.Year;

                _context.Add(DataAI);

                //Guardamos el rol editado
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("num_auditoria_integral", nuevoIdAuditoria);
                HttpContext.Session.SetInt32("anio_auditoria_integral", DateTime.Now.Year);
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult(nuevoIdAuditoria);
        }



        //********************************************************************************
        //PAGINA DE AUDITORIA INDIVIDUAL (ESPECIFICA)
        //********************************************************************************

        /// <summary>
        /// Mostrar Pagina de Auditoria Individual (Especificas)
        /// </summary>
        /// <param name="cod"></param>
        /// <returns></returns>
        [Route("Auditorias/ProgramarAuditoria/AuditoriaIndividual")]
        public async Task<IActionResult> AuditoriaIndividual([FromQuery] bool first = false)
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //obtenemos el numero de auditorias especificas de la integral
            int cantidad = await _context.AU_AUDITORIAS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AE == anio)
                    .CountAsync();

            //Obtenemos informacion de la auditoria integral
            var AuditoriaIntegral = await _context.AU_AUDITORIAS_INTEGRALES
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                                .Where(e => e.ANIO_AI == anio)
                                .FirstOrDefaultAsync();

            //Obtenemos informacion complementaria del universo auditable
            var universos = await _context.MG_UNIVERSO_AUDITABLE
                                .Where(u => u.CODIGO_UNIVERSO_AUDITABLE == AuditoriaIntegral.CODIGO_UNIVERSO_AUDITABLE)
                                .FirstOrDefaultAsync();

            //Guardamos en variable de sesion el titulo de la auditoria para usarla en otras ventanas (tipo AGE102-3-2024 | AUDITORÍA AGENCIA OCOTEPEQUE)
            HttpContext.Session.SetString("titulo_auditoria", AuditoriaIntegral?.CODIGO_AUDITORIA + " | AUDITORÍA " + universos?.NOMBRE.ToUpper());

            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.CODIGO_AUDITORIA = AuditoriaIntegral?.CODIGO_AUDITORIA;
            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.UNIVERSO_AUDITABLE = AuditoriaIntegral?.CODIGO_UNIVERSO_AUDITABLE + "-" + universos?.NOMBRE;
            ViewBag.CODIGO_UNIVERSO_AUDITABLE = AuditoriaIntegral?.CODIGO_UNIVERSO_AUDITABLE;

            if (cantidad > 0 || first == true)
            {
                ViewBag.pTitle = "PROGRAMAR AUDITORÍA ESPECIFICAS";
                ViewBag.audEspecifica = true;
                ViewBag.sinAudEspecifica = false;
            }
            else
            {
                ViewBag.pTitle = "DETALLE DE LA AUDITORÍA";
                ViewBag.audEspecifica = false;
                ViewBag.sinAudEspecifica = true;
            }

            return View();
        }


        /// <summary>
        /// Obtener las auditorias especificas de una auditoria Integral
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetAuditoriasEspecificas(int num_auditoria_integral, int anio_auditoria_integral)
        {
            var estadoRequest = Request.Form["estado"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault().ToUpper();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault().ToUpper();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            List<Au_auditorias> data = new List<Au_auditorias>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_AUDITORIAS
                        .Include(e => e.mg_tipos_de_auditorias)
                        .OrderByDescending(e => e.NUMERO_AUDITORIA)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue))
                        .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                        .Where(e => e.ANIO_AE == anio_auditoria_integral)
                        .Skip(skip).Take(pageSize).ToListAsync();
                }
                else
                {
                    data = await _context.AU_AUDITORIAS
                        .Include(e => e.mg_tipos_de_auditorias)
                        .OrderBy(e => e.NUMERO_AUDITORIA)
                        .Where(e => e.CODIGO_AUDITORIA.Contains(searchValue))
                        .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                        .Where(e => e.ANIO_AE == anio_auditoria_integral)
                        .Skip(skip).Take(pageSize).ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                    .Where(e => e.ANIO_AE == anio_auditoria_integral)
                    .CountAsync(x => x.CODIGO_AUDITORIA.Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_AUDITORIAS
                        .Include(e => e.mg_tipos_de_auditorias)
                        .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                        .Where(e => e.ANIO_AE == anio_auditoria_integral)
                        .OrderByDescending(e => e.NUMERO_AUDITORIA)
                        .Skip(skip).Take(pageSize).ToListAsync();
                }
                else
                {
                    data = await _context.AU_AUDITORIAS
                        .Include(e => e.mg_tipos_de_auditorias)
                        .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                        .Where(e => e.ANIO_AE == anio_auditoria_integral)
                        .OrderBy(e => e.NUMERO_AUDITORIA)
                        .Skip(skip).Take(pageSize).ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                    .Where(e => e.ANIO_AE == anio_auditoria_integral)
                    .CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }


        /// <summary>
        /// Guardar Auditoria Individual (Especifica)
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GuardarAuditoriaEspecifica(int num_auditoria_integral, int anio_auditoria_integral, string codigo_universo_auditable, string tipo_auditoria, string selectedText, string encargado_auditoria)
        {
            int nuevoIdAuditoria = 0;

            try
            {
                //Verificamos si ya existe en la auditoria integral una auditoria especifica segun el tipo_auditoria
                int existeAudEsp = await _context.AU_AUDITORIAS
                    .Where(a => a.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                    .Where(a => a.ANIO_AE == anio_auditoria_integral)
                    .Where(a => a.CODIGO_TIPO_AUDITORIA == tipo_auditoria)
                    .CountAsync();

                if (existeAudEsp > 0)
                {
                    return new JsonResult("existe");
                }
                else
                {
                    //Obtenemos el codigo del siguiente registro
                    int maxNumeroAuditoriaEspecifica = await _context.AU_AUDITORIAS
                        .MaxAsync(a => (int?)a.NUMERO_AUDITORIA) ?? 0;

                    // Incrementar el valor máximo en 1
                    nuevoIdAuditoria = maxNumeroAuditoriaEspecifica + 1;

                    //Obtenemos informacion complementaria del universo auditable
                    var universos = await _context.MG_UNIVERSO_AUDITABLE
                                        .Where(u => u.CODIGO_UNIVERSO_AUDITABLE == codigo_universo_auditable)
                                        .FirstOrDefaultAsync();

                    Au_auditorias au_Auditorias = new();
                    au_Auditorias.NUMERO_AUDITORIA_INTEGRAL = num_auditoria_integral;
                    au_Auditorias.NUMERO_AUDITORIA = nuevoIdAuditoria;
                    au_Auditorias.CODIGO_TIPO_AUDITORIA = tipo_auditoria;
                    au_Auditorias.CODIGO_AUDITORIA = "Pendiente Confirmación";
                    au_Auditorias.NOMBRE_AUDITORIA = selectedText + " " + universos?.NOMBRE;
                    au_Auditorias.FECHA_CREACION = DateTime.Now;
                    au_Auditorias.CREADO_POR = HttpContext.Session.GetString("user");
                    au_Auditorias.ENCARGADO_AUDITORIA = encargado_auditoria;
                    au_Auditorias.ANIO_AE = anio_auditoria_integral;

                    _context.Add(au_Auditorias);

                    //Guardamos el rol editado
                    await _context.SaveChangesAsync();

                    return new JsonResult(nuevoIdAuditoria);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }


        /// <summary>
        /// Borrar Auditoria Individual (Especifica)
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BorrarAuditoriaEspecifica(int num_audit_integral, int numero_auditoria)
        {
            try
            {
                int exito = await _context.AU_AUDITORIAS
                        .Where(x => x.NUMERO_AUDITORIA_INTEGRAL == num_audit_integral)
                        .Where(x => x.NUMERO_AUDITORIA == numero_auditoria)
                        .ExecuteDeleteAsync();

                if (exito > 0)
                {
                    return new JsonResult("success");
                }
                else
                {
                    return new JsonResult("error");
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }


        /// <summary>
        /// Guardar Auditoria Individual (Especifica)
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ConfirmarAuditoriasEspecifica(int num_auditoria_integral, int anio_auditoria_integral)
        {
            try
            {
                var nuevoPDT = 0;

                //Obtenemos data de la auditoria integral
                var auditIntegral = await _context.AU_AUDITORIAS_INTEGRALES
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                                    .Where(u => u.ANIO_AI == anio_auditoria_integral)
                                    .FirstOrDefaultAsync();

                //Confirmamos todas las auditorias especificas que no han sido confirmadas
                var auditEspec = await _context.AU_AUDITORIAS
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                                    .Where(u => u.ANIO_AE == anio_auditoria_integral)
                                    .Where(u => u.CODIGO_AUDITORIA == "Pendiente Confirmación")
                                    .ToListAsync();

                if (auditEspec.Count == 0)
                {
                    return new JsonResult("Sin Confirmaciones");
                }

                // Recorre cada auditoría y realizamos las actualizaciones necesarias
                foreach (var auditoria in auditEspec)
                {
                    auditoria.CODIGO_AUDITORIA = auditIntegral.CODIGO_UNIVERSO_AUDITABLE + "-" + auditoria.CODIGO_TIPO_AUDITORIA + "-" + num_auditoria_integral + "-" + anio_auditoria_integral;

                    // Guarda los cambios en la base de datos una vez
                    int result = await _context.SaveChangesAsync();


                    if (result > 0)
                    {
                        //Obtenemos el codigo del siguiente registro segun el anio actual
                        int maxNumeroPlanAuditGeneral = await _context.AU_PLANES_DE_TRABAJO
                        .MaxAsync(a => (int?)a.NUMERO_PDT) ?? 0;

                        // Incrementar el valor máximo en 1
                        nuevoPDT = maxNumeroPlanAuditGeneral + 1;

                        //Obtenemos el codigo del siguiente registro segun el anio actual
                        int maxNumeroPlanAudit = await _context.AU_PLANES_DE_TRABAJO
                            .MaxAsync(a => (int?)a.NUMERO_PDT) ?? 0;

                        // Incrementar el valor máximo en 1
                        nuevoPDT = maxNumeroPlanAudit + 1;

                        Au_Planes_De_Trabajo au_planes_de_trabajo = new();
                        au_planes_de_trabajo.NUMERO_PDT = nuevoPDT;
                        au_planes_de_trabajo.NUMERO_AUDITORIA_INTEGRAL = num_auditoria_integral;
                        au_planes_de_trabajo.ANIO_AUDITORIA = anio_auditoria_integral;
                        au_planes_de_trabajo.NUMERO_AUDITORIA = auditoria.NUMERO_AUDITORIA;
                        au_planes_de_trabajo.CODIGO_TIPO_AUDITORIA = auditoria.CODIGO_TIPO_AUDITORIA;
                        au_planes_de_trabajo.CODIGO_PDT = "PDT-" + auditoria.CODIGO_AUDITORIA;
                        au_planes_de_trabajo.CODIGO_ESTADO = 1;
                        au_planes_de_trabajo.FECHA_CREACION = DateTime.Now;
                        au_planes_de_trabajo.CREADO_POR = HttpContext.Session.GetString("user");

                        _context.Add(au_planes_de_trabajo);

                        //Guardamos el rol editado
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Success");
        }


        /// <summary>
        /// Actualizamos la variable de sesion cada vez que el usuario accede a una nueva Auditoria Integral
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AsigAudIntegSession")]
        public async Task<IActionResult> AsigAudIntegSession(int num_audit_integral, int anio_audit_integral)
        {
            // Obtenemos informacion de la auditoria
            var AuditoriaIntegral = await _context.AU_AUDITORIAS_INTEGRALES
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == num_audit_integral)
                                .Where(u => u.ANIO_AI == anio_audit_integral)
                                .FirstOrDefaultAsync();

            //Obtenemos informacion complementaria del universo auditable
            var universos = await _context.MG_UNIVERSO_AUDITABLE
                                .Where(u => u.CODIGO_UNIVERSO_AUDITABLE == AuditoriaIntegral.CODIGO_UNIVERSO_AUDITABLE)
                                .FirstOrDefaultAsync();

            HttpContext.Session.SetInt32("num_auditoria_integral", num_audit_integral);
            HttpContext.Session.SetInt32("anio_auditoria_integral", anio_audit_integral);
            HttpContext.Session.SetString("titulo_auditoria", AuditoriaIntegral?.CODIGO_AUDITORIA + " | AUDITORÍA " + universos?.NOMBRE.ToUpper());

            return new JsonResult("success");
        }


        /// <summary>
        /// Guardar Auditoria Individual (Especifica)
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditarEncargadoAuditoriaEspec(int num_auditoria_integral, int num_audit, string encargado_auditoria)
        {
            try
            {
                //Obtenemos data de la auditoria integral
                var auditEspec = await _context.AU_AUDITORIAS
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == num_auditoria_integral)
                                    .Where(u => u.NUMERO_AUDITORIA == num_audit)
                                    .FirstOrDefaultAsync();

                auditEspec.ENCARGADO_AUDITORIA = encargado_auditoria;

                await _context.SaveChangesAsync();

                return new JsonResult(auditEspec);
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }



        //********************************************************************************
        // PAGINA DETALLE DE AUDITORIA
        //********************************************************************************

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/DetalleAuditoria")]
        public async Task<IActionResult> DetalleAuditoria()
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //obtenemos el numero de auditorias especificas de la integral
            int cantidad = await _context.AU_AUDITORIAS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AE == anio)
                    .CountAsync();

            //Obtenemos data del memorandum de planificacion
            var dataAuditoria = await _context.AU_AUDITORIAS_INTEGRALES
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                                .Where(e => e.ANIO_AI == anio)
                                .FirstOrDefaultAsync();

            var MemoExiste = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                                    .Where(e => e.ANIO_MDP == anio)
                                    .FirstOrDefaultAsync();

            ViewBag.CANTIDAD_AUDITORIAS = cantidad;
            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.ESTADO_AUDITORIA = dataAuditoria.CODIGO_ESTADO;
            ViewBag.MEMO_EXISTE = MemoExiste;
            ViewBag.ESTADO_MDP = MemoExiste?.CODIGO_ESTADO ?? 0;
            ViewBag.CODIGO_AUDITORIA = dataAuditoria.CODIGO_AUDITORIA;


            return View();
        }


        /// <summary>
        /// Validamos si el memoramdun de planificacion ya existe
        /// </summary>
        /// <param name="num_audit_integral"></param>
        /// <returns></returns>
        public async Task<IActionResult> validarMemorandumExiste(int num_audit_integral, int anio_audit_integral)
        {
            try
            {
                // Obtenemos informacion de la auditoria
                var existeMemorandum = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == num_audit_integral)
                                    .Where(u => u.ANIO_MDP == anio_audit_integral)
                                    .CountAsync();

                if (existeMemorandum == 0)
                {
                    return new JsonResult(false);
                }
                else
                {
                    return new JsonResult(true);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Anular una auditoria
        /// </summary>
        /// <param name="num_audit_integral"></param>
        /// <returns></returns>
        public async Task<IActionResult> AnularAuditoria(int num_audit_integral, int anio_audit_integral)
        {
            try
            {
                // Obtenemos informacion de la auditoria
                var dataAuditoria = await _context.AU_AUDITORIAS_INTEGRALES
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == num_audit_integral)
                                    .Where(u => u.ANIO_AI == anio_audit_integral)
                                    .FirstOrDefaultAsync();

                dataAuditoria.CODIGO_ESTADO = 14;
                dataAuditoria.AUDITORIA_ANULADA = "S";
                dataAuditoria.FECHA_ANULACION = DateTime.Now;
                dataAuditoria.ANULADA_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }



        //********************************************************************************
        // PAGINA PLANIFICACION DE AUDITORIA
        //********************************************************************************  

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/ProgramarAuditoria/AuditoriaIndividual/PlanificacionAuditoria")]
        public async Task<IActionResult> PlanificacionAuditoria()
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //obtenemos el numero de auditorias especificas de la integral
            var AuditoriaI = await _context.AU_AUDITORIAS_INTEGRALES
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .FirstOrDefaultAsync();
            //Obtenemos informacion complementaria del universo auditable
            var universos = await _context.MG_UNIVERSO_AUDITABLE
                                .Where(u => u.CODIGO_UNIVERSO_AUDITABLE == AuditoriaI.CODIGO_UNIVERSO_AUDITABLE)
                                .FirstOrDefaultAsync();

            int currentYear = DateTime.Now.Year;
            //Obtenemos el codigo del siguiente registro
            int maxNumeroMDP = await _context.AU_PLANIFICACION_DE_AUDITORIA
                .Where(a => a.ANIO_MDP == currentYear)
                .MaxAsync(a => (int?)a.NUMERO_MDP) ?? 0;
            // Incrementar el valor máximo en 1
            var nuevoMDP = maxNumeroMDP + 1;

            /********************************************/
            //Obtenemos las auditorias especificas agregadas
            var dataAuditoriasEspecificas = await _context.AU_AUDITORIAS
            .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
            .Where(e => e.ANIO_AE == anio)
            .Where(u => u.CODIGO_AUDITORIA != "Pendiente Confirmación")
            .OrderBy(e => e.NUMERO_AUDITORIA)
            .ToListAsync();

            var auditoriasText = "";

            // Crear y agregar entidades para cada auditor en el array
            foreach (var auditor in dataAuditoriasEspecificas)
            {
                auditoriasText = auditoriasText + "<li> " + auditor.CODIGO_AUDITORIA + " " + auditor.NOMBRE_AUDITORIA + " \n";
            }
            /********************************************/

            DateTime? fechaInicioVisita = AuditoriaI.FECHA_INICIO_VISITA;
            string fechaInicio = fechaInicioVisita?.ToString("dd/MM/yyyy");
            DateTime? fechaFinVisita = AuditoriaI.FECHA_FIN_VISITA;
            string fechaFin = fechaFinVisita?.ToString("dd/MM/yyyy");

            ViewBag.CODIGO_AUDITORIA = AuditoriaI.CODIGO_AUDITORIA;
            ViewBag.UNIVERSO_AUDITABLE = universos.CODIGO_UNIVERSO_AUDITABLE + " - " + universos.NOMBRE;
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.CODIGO_MEMORANDUM_PLANIFICACION = "MDP-UAI-" + nuevoMDP + "-" + currentYear;
            ViewBag.LIST_AUDITORIAS_ESP = auditoriasText;
            ViewBag.TIEMPOS_DE_LA_AUDITORIA = " " + fechaInicio + " al " + fechaFin;

            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> solicitarAprobacionMP(string text_tipo_auditoria, string objetivo_auditoria, string equipo_auditores, string recursos, string text_equipo_auditores, string text_tiempo_auditoria)
        {
            try
            {
                //Obtenemos el codigo del siguiente registro
                int maxNumeroMDP = await _context.AU_PLANIFICACION_DE_AUDITORIA
                    .MaxAsync(a => (int?)a.NUMERO_MDP) ?? 0;

                // Incrementar el valor máximo en 1
                var nuevoMDP = maxNumeroMDP + 1;

                Au_planificacion_de_auditoria au_planificacion_auditoria = new();
                au_planificacion_auditoria.NUMERO_MDP = nuevoMDP;
                au_planificacion_auditoria.NUMERO_AUDITORIA_INTEGRAL = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                au_planificacion_auditoria.CODIGO_MEMORANDUM = "MDP-UAI-" + (int)HttpContext.Session.GetInt32("num_auditoria_integral") + "-" + (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
                au_planificacion_auditoria.OBJETIVO = objetivo_auditoria;
                au_planificacion_auditoria.RECURSOS = recursos;
                au_planificacion_auditoria.CODIGO_ESTADO = 1;
                au_planificacion_auditoria.FECHA_CREACION = DateTime.Now;
                au_planificacion_auditoria.CREADO_POR = HttpContext.Session.GetString("user");
                au_planificacion_auditoria.ANIO_MDP = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
                au_planificacion_auditoria.TEXTO_TIPO_AUDITORIA = text_tipo_auditoria;
                au_planificacion_auditoria.TEXTO_EQUIPO_TRABAJO = text_equipo_auditores;
                au_planificacion_auditoria.TEXTO_TIEMPO_AUDITORIA = text_tiempo_auditoria;

                _context.Add(au_planificacion_auditoria);

                //Guardamos la data del memorandum de planificacion
                int result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    /*******/
                    //Obtenemos data de la auditoria integral
                    var dataAudInt = await _context.AU_AUDITORIAS_INTEGRALES
                                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == (int)HttpContext.Session.GetInt32("num_auditoria_integral"))
                                                .Where(u => u.ANIO_AI == (int)HttpContext.Session.GetInt32("anio_auditoria_integral"))
                                                .FirstOrDefaultAsync();

                    dataAudInt!.CODIGO_ESTADO = 2;
                    dataAudInt!.MDP_EMITIDO = "S";
                    dataAudInt!.MDP_FECHA_EMISION = DateTime.Now;
                    dataAudInt!.MDP_EMITIDO_POR = HttpContext.Session.GetString("user");
                    dataAudInt.FECHA_MODIFICACION = DateTime.Now;
                    dataAudInt.MODIFICADO_POR = HttpContext.Session.GetString("user");

                    // Guarda los cambios en la base de datos
                    await _context.SaveChangesAsync();
                    /********/

                    // Convertir el string equipo_auditores en un array
                    string[] auditoresArray = equipo_auditores.Split(',');

                    // Crear y agregar entidades para cada auditor en el array
                    foreach (string auditor in auditoresArray)
                    {
                        var asignarAuditores = new Au_auditores_asignados
                        {
                            CODIGO_USUARIO = auditor,
                            NUMERO_MDP = nuevoMDP,
                            NUMERO_AUDITORIA_INTEGRAL = (int)HttpContext.Session.GetInt32("num_auditoria_integral"),
                            ANIO_AI = (int)HttpContext.Session.GetInt32("anio_auditoria_integral"),
                            FECHA_CREACION = DateTime.Now,
                            CREADO_POR = HttpContext.Session.GetString("user")
                        };

                        _context.Add(asignarAuditores);

                        await _context.SaveChangesAsync();
                    }
                }

                return new JsonResult("Ok");
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> editarMP(string textAudit, string codigo_memo)
        {
            try
            {
                var editAudit = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.CODIGO_MEMORANDUM == codigo_memo)
                                    .FirstOrDefaultAsync();

                editAudit.TEXTO_TIPO_AUDITORIA = textAudit;

                await _context.SaveChangesAsync();

                return new JsonResult("Ok");
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }

        /// <summary>
        /// Editar objetivos, equipo de trabajo y recursos del memorandum de planificacion
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> editarObjEqRecursosMP(string objetivo_auditoria, string equipo_trabajo, string equipo_auditores, string recursos, int numeromdp, string tiempo_edit, string fecha_inicio, string fecha_fin)
        {
            try
            {
                int numAuditInt = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                int anioAuditInt = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

                // Obtenemos informacion de la Planificacion
                var dataAuditoria = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == numAuditInt)
                                    .Where(u => u.ANIO_MDP == anioAuditInt)
                                    .FirstOrDefaultAsync();

                dataAuditoria.OBJETIVO = objetivo_auditoria;
                dataAuditoria.RECURSOS = recursos;
                dataAuditoria.TEXTO_EQUIPO_TRABAJO = equipo_trabajo;
                dataAuditoria.TEXTO_TIEMPO_AUDITORIA = tiempo_edit;
                dataAuditoria.FECHA_ACTUALIZACION = DateTime.Now;
                dataAuditoria.ACTUALIZADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                var save = await _context.SaveChangesAsync();

                if (save > 0)
                {
                    // Obtenemos informacion de la auditoria Integral para modificar la fecha si se realizaron cambios
                    var dataPlanificacion = await _context.AU_AUDITORIAS_INTEGRALES
                                        .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == numAuditInt)
                                        .Where(u => u.ANIO_AI == anioAuditInt)
                                        .FirstOrDefaultAsync();

                    dataPlanificacion.FECHA_INICIO_VISITA = DateTime.Parse(fecha_inicio);
                    dataPlanificacion.FECHA_FIN_VISITA = DateTime.Parse(fecha_fin);

                    // Guarda los cambios en la base de datos
                    save = await _context.SaveChangesAsync();

                    /********/
                    //Borramos los auditores para volverlos a agregar los nuevos
                    int exito = await _context.AU_AUDITORES_ASIGNADOS
                        .Where(x => x.NUMERO_MDP == numeromdp)
                        .Where(x => x.NUMERO_AUDITORIA_INTEGRAL == numAuditInt)
                        .Where(x => x.ANIO_AI == anioAuditInt)
                        .ExecuteDeleteAsync();

                    if (exito > 0)
                    {
                        // Convertir el string equipo_auditores en un array
                        string[] auditoresArray = equipo_auditores.Split(',');

                        // Crear y agregar entidades para cada auditor en el array
                        foreach (string auditor in auditoresArray)
                        {
                            var asignarAuditores = new Au_auditores_asignados
                            {
                                CODIGO_USUARIO = auditor,
                                NUMERO_MDP = numeromdp,
                                NUMERO_AUDITORIA_INTEGRAL = numAuditInt,
                                ANIO_AI = anioAuditInt,
                                FECHA_CREACION = DateTime.Now,
                                CREADO_POR = HttpContext.Session.GetString("user")
                            };

                            _context.Add(asignarAuditores);

                            await _context.SaveChangesAsync();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }

        //********************************************************************************
        // PAGINA DETALLE DE LA AUDITORIA
        //********************************************************************************  

        /// <summary>
        /// Pagina principal del detalle de la auditoria
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/DetalleAuditoria/DetalleDocAuditoria")]
        public async Task<IActionResult> DetalleDocAuditoria()
        {
            string estado = null;
            string cssStatus = null;

            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //Obtenemos informacion de la planificacion de la auditoria
            var dataMDP = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                                .Where(u => u.ANIO_MDP == anio)
                                .FirstOrDefaultAsync();

            var dataAuditoriasEspecificas = await _context.AU_AUDITORIAS
                        .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                        .Where(e => e.ANIO_AE == anio)
                        .Where(u => u.CODIGO_AUDITORIA != "Pendiente Confirmación")
                        .OrderBy(e => e.NUMERO_AUDITORIA)
                        .ToListAsync();


            var auditoriasText = "";

            // Crear y agregar entidades para cada auditor en el array
            foreach (var auditor in dataAuditoriasEspecificas)
            {
                auditoriasText = auditoriasText + "<li> " + auditor.CODIGO_AUDITORIA + " " + auditor.NOMBRE_AUDITORIA + " \n";
            }

            //Obtenemos informacion de la auditoria integral
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                                .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                                .Where(u => u.ANIO_AI == anio)
                                .FirstOrDefaultAsync();

            var listAuditores = await _context.AU_AUDITORES_ASIGNADOS
                .Include(x => x.mg_usuarios)
                .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(e => e.ANIO_AI == anio)
                .OrderBy(e => e.CODIGO_USUARIO)
                .ToListAsync();

            var auditoresText = "";

            // Crear y agregar entidades para cada auditor en el array
            foreach (var auditores in listAuditores)
            {
                auditoresText = auditoresText + auditores.mg_usuarios.NOMBRE_USUARIO + ", ";
            }

            //Obtenemos el listado de comentarios
            var listComentarios = await _context.AU_COMENTARIOS_MDP
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .OrderByDescending(e => e.FECHA_CREACION)
                    .ToListAsync();

            DateTime? fechaInicioVisita = dataAI.FECHA_INICIO_VISITA;
            string fechaInicio = fechaInicioVisita?.ToString("dd/MM/yyyy");
            DateTime? fechaFinVisita = dataAI.FECHA_FIN_VISITA;
            string fechaFin = fechaFinVisita?.ToString("dd/MM/yyyy");

            //Evaluamos el estado de la Auditoria
            if (dataMDP.CODIGO_ESTADO == 1)
            {
                estado = "Creado";
                cssStatus = "create";
            }
            else if (dataMDP.CODIGO_ESTADO == 2)
            {
                estado = "Pendiente Aprobación";
                cssStatus = "pending";
            }
            else if (dataMDP.CODIGO_ESTADO == 4)
            {
                estado = "Con Observaciones";
                cssStatus = "verification";
            }
            else
            {
                estado = "Aprobado";
                cssStatus = "approved";
            }

            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.ESTADO = estado;
            ViewBag.ESTADOAI = dataAI.CODIGO_ESTADO;
            ViewBag.STATUS = cssStatus;
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.CODIGO_MEMORANDUM_PLANIFICACION = dataMDP.CODIGO_MEMORANDUM;
            ViewBag.FECHA_APROBACION = dataMDP.FECHA_APROBACION?.ToString() ?? "";
            ViewBag.TIPOS_DE_AUDITORIAS_REALIZAR = dataMDP.TEXTO_TIPO_AUDITORIA + " \n\n" + auditoriasText;
            ViewBag.TEXTO_AUDITORIAS_EDIT = dataMDP.TEXTO_TIPO_AUDITORIA;
            ViewBag.OBJETIVO_AUDITORIA = dataMDP.OBJETIVO;
            ViewBag.RECURSOS_AUDITORIA = dataMDP.RECURSOS;
            ViewBag.TEXTO_AUDITORES_AUDITORIA = dataMDP.TEXTO_EQUIPO_TRABAJO;
            ViewBag.NUMERO_MDP = dataMDP.NUMERO_MDP;
            ViewBag.AUDITORES_AUDITORIA = auditoresText;
            ViewBag.TEXTO_TIEMPO_AUDITORIA = dataMDP.TEXTO_TIEMPO_AUDITORIA;
            ViewBag.TIEMPO_AUDITORIA_INICIO = DateTime.Parse(fechaInicio.ToString()).ToString("yyyy-MM-dd");
            ViewBag.TIEMPO_AUDITORIA_FIN = DateTime.Parse(fechaFin.ToString()).ToString("yyyy-MM-dd");
            ViewBag.COMENTARIOS = listComentarios;
            ViewBag.ROL_CODE = HttpContext.Session.GetString("rolCode");

            return View();
        }


        /// <summary>
        /// Metodo para obtener los comentarios del memorandum de planificacion
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> obtenerComentariosMDP(int num_audit_integral)
        {
            try
            {
                var listComentarios = await _context.AU_COMENTARIOS_MDP
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_audit_integral)
                    .OrderBy(e => e.FECHA_CREACION)
                    .ToListAsync();

                return new JsonResult(listComentarios);
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

        }


        /// <summary>
        /// Guardar nueva Auditoria Integral
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Guardar_ComentarioMDP([FromBody] Au_comentarios_mdp DataAI)
        {
            int nuevoIdAuditoria = 0;
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            try
            {
                if (DataAI == null) return new JsonResult("error");

                //Obtenemos el codigo del siguiente registro
                int maxNumero = await _context.AU_COMENTARIOS_MDP
                    .MaxAsync(a => (int?)a.CODIGO_COMENTARIO) ?? 0;

                // Incrementar el valor máximo en 1
                var nuevoId = maxNumero + 1;

                DataAI.CODIGO_COMENTARIO = nuevoId;
                DataAI.NUMERO_AUDITORIA_INTEGRAL = cod;
                DataAI.CODIGO_USUARIO = HttpContext.Session.GetString("user");
                DataAI.NOMBRE_USUARIO = HttpContext.Session.GetString("userName");
                DataAI.FECHA_CREACION = DateTime.Now;
                DataAI.CREADO_POR = HttpContext.Session.GetString("user");
                DataAI.ANIO_AI = anio;

                _context.Add(DataAI);


                //Guardamos el comentario
                await _context.SaveChangesAsync();


                //Obtenemos el listado de comentarios
                var listComentarios = await _context.AU_COMENTARIOS_MDP
                        .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                        .Where(e => e.ANIO_AI == anio)
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .ToListAsync();

                ViewBag.COMENTARIOS = listComentarios;

                //Enviamos los comentarios actualizados a la vista parcial
                return PartialView("partialView/_ComentariosMDP");
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }


        /// <summary>
        /// Solicitar aprobacion de memorandum de planificacion
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SolicitarAprobacionMDP(string codigo_memorandum)
        {
            try
            {
                //Obtenemos data del memorandum de planificacion
                var datamemorandum = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.CODIGO_MEMORANDUM == codigo_memorandum)
                                    .FirstOrDefaultAsync();

                datamemorandum!.CODIGO_ESTADO = 2;
                datamemorandum.FECHA_SOLICITUD_APROBACION = DateTime.Now;
                datamemorandum.SOLICITADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                /*******/
                //Obtenemos data de la auditoria integral
                var dataAudInt = await _context.AU_AUDITORIAS_INTEGRALES
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == (int)HttpContext.Session.GetInt32("num_auditoria_integral"))
                                    .Where(u => u.ANIO_AI == (int)HttpContext.Session.GetInt32("anio_auditoria_integral"))
                                    .FirstOrDefaultAsync();

                dataAudInt!.CODIGO_ESTADO = 3;
                dataAudInt.FECHA_MODIFICACION = DateTime.Now;
                dataAudInt.MODIFICADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
                /********/

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }

        /// <summary>
        /// Dar por aprobada un memorandum de planificacion
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AprobarMDP(string codigo_memorandum)
        {
            try
            {
                //Obtenemos data del memorandum de planificacion
                var datamemorandum = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.CODIGO_MEMORANDUM == codigo_memorandum)
                                    .FirstOrDefaultAsync();

                datamemorandum!.CODIGO_ESTADO = 3;
                datamemorandum.FECHA_APROBACION = DateTime.Now;
                datamemorandum.APROBADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                /*******/
                //Obtenemos data de la auditoria integral
                var dataAudInt = await _context.AU_AUDITORIAS_INTEGRALES
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == (int)HttpContext.Session.GetInt32("num_auditoria_integral"))
                                    .Where(u => u.ANIO_AI == (int)HttpContext.Session.GetInt32("anio_auditoria_integral"))
                                    .FirstOrDefaultAsync();

                dataAudInt!.CODIGO_ESTADO = 4;
                dataAudInt.FECHA_MODIFICACION = DateTime.Now;
                dataAudInt.MODIFICADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
                /********/

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }


        /// <summary>
        /// Regresar un memorandum de planificacion
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegresarMDP(string codigo_memorandum)
        {
            try
            {
                //Obtenemos data del memorandum de planificacion
                var datamemorandum = await _context.AU_PLANIFICACION_DE_AUDITORIA
                                    .Where(u => u.CODIGO_MEMORANDUM == codigo_memorandum)
                                    .FirstOrDefaultAsync();

                datamemorandum!.CODIGO_ESTADO = 4;
                datamemorandum.FECHA_ACTUALIZACION = DateTime.Now;
                datamemorandum.ACTUALIZADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                /*******/
                //Obtenemos data de la auditoria integral
                var dataAudInt = await _context.AU_AUDITORIAS_INTEGRALES
                                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == (int)HttpContext.Session.GetInt32("num_auditoria_integral"))
                                    .Where(u => u.ANIO_AI == (int)HttpContext.Session.GetInt32("anio_auditoria_integral"))
                                    .FirstOrDefaultAsync();

                dataAudInt!.CODIGO_ESTADO = 3;
                dataAudInt.FECHA_MODIFICACION = DateTime.Now;
                dataAudInt.MODIFICADO_POR = HttpContext.Session.GetString("user");

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
                /********/
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }


        [HttpGet]
        public async Task<ActionResult<List<Mg_usuarios>>> GetAuditoresAsignados(int num_audit_integral, int anio_audit_integral)
        {
            try
            {
                // Obtener todos los usuarios
                var todosLosUsuarios = await _context.MG_USUARIOS
                    .Where(i => i.ESTADO == 1)
                    .Where(i => i.CODIGO_ROL == "AA")
                    .OrderBy(i => i.NOMBRE_USUARIO)
                    .ToListAsync();

                // Obtener los auditores asignados para la auditoría específica
                var auditoresAsignados = await _context.AU_AUDITORES_ASIGNADOS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == num_audit_integral)
                    .Where(e => e.ANIO_AI == anio_audit_integral)
                    .Select(e => e.CODIGO_USUARIO)
                    .ToListAsync();

                // Combinar los resultados
                todosLosUsuarios.ForEach(usuario =>
                {
                    usuario.SELECTED = auditoresAsignados.Contains(usuario.CODIGO_USUARIO);
                });

                if (todosLosUsuarios == null)
                {
                    return NotFound();
                }

                return todosLosUsuarios;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        //********************************************************************************
        // CUESTIONARIOS DE TRABAJO
        //********************************************************************************

        /// <summary>
        /// Metodo para mostrar los cuestionarios de trabajo que se han agregado a la auditoria
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/CuestionariosAuditoria")]
        public async Task<IActionResult> CuestionariosAuditoria()
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //obtenemos el numero de auditorias especificas de la integral
            var cuestionarios = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO == anio)
                    .ToListAsync();


            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.AUDITORIAS_CUESTIONARIOS = cuestionarios;

            return View();
        }


        /// <summary>
        /// Metodo para obtener los cuestionarios autorizados para ser agregados a la auditoria
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerCuestionarios(int num_auditoria_integral, int anio_auditoria_integral)
        {
            var estadoRequest = Request.Form["estado"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault().ToUpper();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault().ToUpper();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            List<Au_cuestionarios> data = new List<Au_cuestionarios>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_CUESTIONARIOS
                        .OrderByDescending(e => e.NOMBRE_CUESTIONARIO)
                        .Where(e => e.NOMBRE_CUESTIONARIO.ToUpper().Contains(searchValue))
                        .Where(e => e.ESTADO == 3)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.AU_CUESTIONARIOS
                        .OrderBy(e => e.NOMBRE_CUESTIONARIO)
                        .Where(e => e.NOMBRE_CUESTIONARIO.Contains(searchValue))
                        .Where(e => e.ESTADO == 3)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_CUESTIONARIOS
                    .Where(e => e.ESTADO == 3)
                    .CountAsync(x => x.NOMBRE_CUESTIONARIO.Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_CUESTIONARIOS
                        .Where(e => e.ESTADO == 3)
                        .OrderByDescending(e => e.NOMBRE_CUESTIONARIO)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.AU_CUESTIONARIOS
                        .Where(e => e.ESTADO == 3)
                        .OrderBy(e => e.NOMBRE_CUESTIONARIO)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_CUESTIONARIOS
                    .Where(e => e.ESTADO == 3)
                    .CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);

        }


        /// <summary>
        /// Guardar un cuestionario a la auditoria
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Guardar_Audit_Cuestionario([FromBody] Mg_auditorias_cuestionarios DataCuest)
        {
            try
            {
                if (DataCuest == null) return new JsonResult("error");

                //Obtenemos el codigo del siguiente registro
                int maxNumeroRegistro = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .MaxAsync(a => (int?)a.CODIGO_AUDITORIA_CUESTIONARIO) ?? 0;
                // Incrementar el valor máximo en 1
                var nuevoIdRegistro = maxNumeroRegistro + 1;



                //Obtenemos el codigo del siguiente correlativo del cuestionario
                int maxNumero = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == DataCuest.NUMERO_AUDITORIA_INTEGRAL)
                    .Where(e => e.ANIO == DataCuest.ANIO)
                    .MaxAsync(a => (int?)a.CORRELATIVO_CUESTIONARIO) ?? 0;
                // Incrementar el valor máximo en 1
                var nuevoId = maxNumero + 1;



                //Obtenemos informacion complementaria del universo auditable
                var auditoria = await _context.AU_AUDITORIAS_INTEGRALES
                                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == DataCuest.NUMERO_AUDITORIA_INTEGRAL)
                                    .Where(e => e.ANIO_AI == DataCuest.ANIO)
                                    .FirstOrDefaultAsync();

                DataCuest.CODIGO_AUDITORIA_CUESTIONARIO = nuevoIdRegistro;
                DataCuest.CODIGO_AUD_CUEST = "CDT-" + auditoria.CODIGO_UNIVERSO_AUDITABLE + "-" + DataCuest.CODIGO_AUD_CUEST + "-" + nuevoId + "-" + DataCuest.ANIO;
                DataCuest.CODIGO_ESTADO = 1;
                DataCuest.CREADO_POR = HttpContext.Session.GetString("user");
                DataCuest.FECHA_CREACION = DateTime.Now;
                DataCuest.CORRELATIVO_CUESTIONARIO = nuevoId;

                _context.Add(DataCuest);


                //Guardamos el cuestionario
                int resp = await _context.SaveChangesAsync();

                //Si se guardo el cuestionario, guardamos las preguntas
                if (resp > 0)
                {
                    //Obtenemos informacion complementaria del universo auditable
                    var preguntas = await _context.MG_PREGUNTAS_CUESTIONARIO
                                        .Where(e => e.CODIGO_CUESTIONARIO == DataCuest.CODIGO_CUESTIONARIO)
                                        .ToListAsync();

                    foreach (var item in preguntas)
                    {
                        //Obtenemos el codigo del siguiente registro
                        int maxNumeroPreg = await _context.MG_RESPUESTAS_CUESTIONARIO
                            .MaxAsync(a => (int?)a.CODIGO_RESPUESTA) ?? 0;
                        // Incrementar el valor máximo en 1
                        var IdPregunta = maxNumeroPreg + 1;

                        Mg_respuestas_cuestionario RespCuest = new();
                        RespCuest.CODIGO_RESPUESTA = IdPregunta;
                        RespCuest.CODIGO_PREGUNTA = item.CODIGO_PREGUNTA;
                        RespCuest.CUMPLE = 0;
                        RespCuest.NO_CUMPLE = 0;
                        RespCuest.CUMPLE_PARCIALMENTE = 0;
                        RespCuest.NO_APLICA = 0;
                        RespCuest.OBSERVACIONES = null;
                        RespCuest.CALIFICACIONES = null;
                        RespCuest.PUNTAJE = 0;
                        RespCuest.CREADO_POR = HttpContext.Session.GetString("user");
                        RespCuest.FECHA_CREACION = DateTime.Now;
                        RespCuest.CODIGO_AUDITORIA_CUESTIONARIO = nuevoIdRegistro;

                        _context.Add(RespCuest);

                        //Guardamos el cuestionario
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }


        /// <summary>
        /// Metodo para obtener un cuestionarios autorizado y ver sus preguntas
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerPreguntasCuestionario(int codigo_cuestionario)
        {
            try
            {
                List<string> datacuestionario = new List<string>();

                var data = await _context.AU_CUESTIONARIOS
                    .Where(e => e.CODIGO_CUESTIONARIO == codigo_cuestionario)
                    .FirstOrDefaultAsync();

                List<Mg_secciones> secciones = await _context.MG_SECCIONES
                        .Include(x => x.sub_secciones.Where(x => x.CODIGO_CUESTIONARIO == data.CODIGO_CUESTIONARIO))
                        .ThenInclude(x => x.Preguntas_Cuestionarios)
                        .ToListAsync();

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true,
                };

                return new JsonResult(secciones, options);

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }



        }


        /// <summary>
        /// Mostramos el cuestionario de preguntas
        /// </summary>
        /// <returns></returns>
        [HttpGet("Auditorias/CuestionariosAuditoria/CuestionarioTrabajo")]
        public async Task<IActionResult> CuestionarioTrabajo()
        {
            string codigoCuestionario = DecodeBase64(Request.Query["dc"]);

            int codigoCuest = int.Parse(codigoCuestionario);
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //Obtenemos los auditores asignados a la auditoria
            var Auditores = await _context.AU_AUDITORES_ASIGNADOS
                    .Include(e => e.mg_usuarios)
                    .OrderBy(e => e.mg_usuarios.NOMBRE_USUARIO)
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .ToListAsync();

            //Obtenemos la data del cuestionario de la auditoria
            var data = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .Where(e => e.CODIGO_AUDITORIA_CUESTIONARIO == codigoCuest)
                    .FirstOrDefaultAsync();

            //Obtenemos informacion de la auditoria integral a la que pertenece el cuestionario
            var dataAI = await _context.AU_AUDITORIAS_INTEGRALES
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == data.NUMERO_AUDITORIA_INTEGRAL)
                    .Where(e => e.ANIO_AI == data.ANIO)
                    .FirstOrDefaultAsync();

            //Obtenemos las preguntas del cuestionario
            List<Mg_secciones> secciones = await _context.MG_SECCIONES
                        .Include(x => x.sub_secciones.Where(x => x.CODIGO_CUESTIONARIO == data.CODIGO_CUESTIONARIO))
                        .ThenInclude(x => x.Preguntas_Cuestionarios)
                        .ToListAsync();

            //Obtenemos las respuestas del cuestionario
            var respuestasData = await _context.MG_RESPUESTAS_CUESTIONARIO
                            .Where(e => e.CODIGO_AUDITORIA_CUESTIONARIO == codigoCuest)
                            .ToListAsync();

            foreach (var item in secciones)
            {
                foreach (var item2 in item.sub_secciones)
                {
                    var totalPuntos = 0;
                    var numPreguntas = 0;
                    foreach (var item3 in item2.Preguntas_Cuestionarios)
                    {
                        var respuesta = respuestasData.FirstOrDefault(r => r.CODIGO_PREGUNTA == item3.CODIGO_PREGUNTA);

                        item3.RESPUESTA_PREGUNTA = respuesta;

                        if (respuesta?.CUMPLE == 1)
                        {
                            totalPuntos += 100;
                            numPreguntas++;
                        }
                        else if (respuesta?.CUMPLE_PARCIALMENTE == 1)
                        {
                            totalPuntos += 50;
                            numPreguntas++;
                        }
                        else if (respuesta?.NO_CUMPLE == 1)
                        {
                            totalPuntos += 0;
                            numPreguntas++;
                        }
                        else if (respuesta?.NO_APLICA == 1)
                        {
                            totalPuntos += 0;
                        }
                        else
                        {
                            totalPuntos += 0;
                        }
                    }
                    // Calcular el promedio
                    item2.PORCENTAJE = numPreguntas > 0 ? (double)totalPuntos / numPreguntas : 0;
                }
            }

            ViewBag.DATA_CUESTIONARIO = secciones;
            ViewBag.CODIGO_CUEST = codigoCuest;
            ViewBag.ESTADO_CUESTIONARIO = data.CODIGO_ESTADO;
            ViewBag.AGENCIA = dataAI.NOMBRE_AUDITORIA;
            ViewBag.FECHA_CUESTIONARIO = data.FECHA_CUESTIONARIO != null ? DateTime.Parse(data.FECHA_CUESTIONARIO.ToString()).ToString("yyyy-MM-dd") : null;
            ViewBag.AUDITOR_ASIGNADO = data.AUDITOR_ASIGNADO;
            ViewBag.RESPONSABLE = data.RESPONSABLE;
            ViewBag.REVISADO_POR = data.REVISADO_POR;
            ViewBag.AUDITORES_ASIG = Auditores;


            return View();
        }


        /// <summary>
        /// Metodo para obtener las respuestas de un cuestionario agregado a la auditoria
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerRespuestasCuestionario(int codigo_cuestionario)
        {
            try
            {
                List<Mg_secciones> secciones = await _context.MG_SECCIONES
                        .Include(x => x.sub_secciones)
                        .ThenInclude(x => x.Preguntas_Cuestionarios)
                        .ToListAsync();

                var respuestasData = await _context.MG_RESPUESTAS_CUESTIONARIO
                                .Where(e => e.CODIGO_AUDITORIA_CUESTIONARIO == codigo_cuestionario)
                                .ToListAsync();

                foreach (var item in secciones)
                {
                    foreach (var item2 in item.sub_secciones)
                    {
                        foreach (var item3 in item2.Preguntas_Cuestionarios)
                        {
                            var respuesta = respuestasData.FirstOrDefault(r => r.CODIGO_PREGUNTA == item3.CODIGO_PREGUNTA);

                            item3.RESPUESTA_PREGUNTA = respuesta;
                        }
                    }
                }

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true,
                };

                return new JsonResult(secciones, options);

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }



        }


        /// <summary>
        /// Editar un cuestionario a la auditoria
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit_Cuestionario([FromBody] List<Mg_respuestas_cuestionario> DataCuest)
        {
            try
            {
                if (DataCuest == null) return new JsonResult("error");

                foreach (var item in DataCuest)
                {
                    var dataPregunta = await _context.MG_RESPUESTAS_CUESTIONARIO
                                        .Where(u => u.CODIGO_RESPUESTA == item.CODIGO_RESPUESTA)
                                        .FirstOrDefaultAsync();

                    dataPregunta.CUMPLE = item.CUMPLE;
                    dataPregunta.NO_CUMPLE = item.NO_CUMPLE;
                    dataPregunta.CUMPLE_PARCIALMENTE = item.CUMPLE_PARCIALMENTE;
                    dataPregunta.NO_APLICA = item.NO_APLICA;
                    dataPregunta.OBSERVACIONES = item.OBSERVACIONES;
                    dataPregunta.CALIFICACIONES = item.CALIFICACIONES;
                    dataPregunta.PUNTAJE = item.PUNTAJE;
                    dataPregunta.FECHA_MODIFICACION = DateTime.Now;
                    dataPregunta.MODIFICADO_POR = HttpContext.Session.GetString("user");

                    // Guarda los cambios en la base de datos
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }



        /// <summary>
        /// Editar datos un cuestionario a la auditoria
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit_Audi_Cuest([FromBody] Mg_auditorias_cuestionarios DataAuditCuest)
        {
            try
            {
                if (DataAuditCuest == null) return new JsonResult("error");

                var data = await _context.MG_AUDITORIAS_CUESTIONARIOS
                                    .Where(u => u.CODIGO_AUDITORIA_CUESTIONARIO == DataAuditCuest.CODIGO_AUDITORIA_CUESTIONARIO)
                                    .FirstOrDefaultAsync();

                data.FECHA_CUESTIONARIO = DataAuditCuest.FECHA_CUESTIONARIO;
                data.AUDITOR_ASIGNADO = DataAuditCuest.AUDITOR_ASIGNADO;
                data.RESPONSABLE = DataAuditCuest.RESPONSABLE;
                data.REVISADO_POR = DataAuditCuest.REVISADO_POR;

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }

        /// <summary>
        /// Borrar un cuestionario
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BorrarCuestionario(int codigo_cuestionario)
        {
            try
            {
                int exito = await _context.MG_AUDITORIAS_CUESTIONARIOS
                        .Where(x => x.CODIGO_AUDITORIA_CUESTIONARIO == codigo_cuestionario)
                        .ExecuteDeleteAsync();

                if (exito > 0)
                {
                    return new JsonResult("success");
                }
                else
                {
                    return new JsonResult("error");
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }



        //********************************************************************************
        // PROGRAMAS DE TRABAJO
        //********************************************************************************

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/ProgramasDeTrabajo")]
        public async Task<IActionResult> ProgramasTrabajoAuditoria()
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            //obtenemos el numero de auditorias especificas de la integral
            var planesTrab = await _context.AU_PLANES_DE_TRABAJO
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AUDITORIA == anio)
                    .ToListAsync();


            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.AUDITORIAS_PLANESDETRABAJO = planesTrab;

            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/ProgramaTrabajo")]
        public async Task<IActionResult> ProgramaTrabajo()
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

            string numPdtEncoded = DecodeBase64(Request.Query["pdt"]);
            int numeroPDT = int.Parse(numPdtEncoded);

            var data = await _context.AU_PLANES_DE_TRABAJO
                    .Where(e => e.NUMERO_PDT == numeroPDT)
                    .FirstOrDefaultAsync();

            var dataAct = await _context.MG_ACTIVIDADES
                            .Where(e => e.CODIGO_ESTADO == "A")
                            .OrderBy(e => e.NOMBRE_ACTIVIDAD)
                            .ToListAsync();

            ViewBag.NUMERO_PDT = data.NUMERO_PDT;
            ViewBag.NUMERO_AUDITORIA = data.NUMERO_AUDITORIA;
            ViewBag.CODIGO_PDT = data.CODIGO_PDT;
            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.DATA_ACTIVIDADES = dataAct;
            ViewBag.ROL_CODE = HttpContext.Session.GetString("rolCode");

            return View();
        }

        /// <summary>
        /// Guardar actividades asignadas a un usuario
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Guardar_Actividades_asignadas([FromBody] dynamic data)
        {
            try
            {
                int numeroPdt = Int32.Parse(data.GetProperty("numeropdt").GetString());
                int numeroAi = Int32.Parse(data.GetProperty("numeroai").GetString());
                int anioAi = Int32.Parse(data.GetProperty("anioai").GetString());
                int numeroAuditoria = Int32.Parse(data.GetProperty("numero_auditoria").GetString());
                string codigoUsuarioAsignado = data.GetProperty("codigo_usuario_asignado").GetString();
                // Obtener la cadena 'actividades' y dividirla en un array
                var actividadesString = data.GetProperty("actividades").GetString();

                var actividadesArray = actividadesString.Split(','); // Divide la cadena en un array de cadenas

                // Obtén todas las actividades existentes para la combinación dada
                var actividadesExistentesAuditor = await _context.AU_DETALLE_PLAN_DE_TRABAJO
                    .Where(a => a.NUMERO_PDT == numeroPdt &&
                        a.NUMERO_AUDITORIA_INTEGRAL == numeroAi &&
                        a.ANIO_AI == anioAi &&
                        a.NUMERO_AUDITORIA == numeroAuditoria &&
                        a.CODIGO_USUARIO_ASIGNADO == codigoUsuarioAsignado)
                    .Select(a => a.CODIGO_ACTIVIDAD)
                    .ToListAsync();

                foreach (var actividad in actividadesArray)
                {
                    if (!actividadesExistentesAuditor.Contains(Int32.Parse(actividad)))
                    {
                        Au_detalle_plan_de_trabajo detallePDT = new();
                        detallePDT.CODIGO_ACTIVIDAD = Int32.Parse(actividad);
                        detallePDT.NUMERO_PDT = numeroPdt;
                        detallePDT.NUMERO_AUDITORIA_INTEGRAL = numeroAi;
                        detallePDT.ANIO_AI = anioAi;
                        detallePDT.NUMERO_AUDITORIA = numeroAuditoria;
                        detallePDT.CODIGO_ESTADO = 0;
                        detallePDT.CODIGO_USUARIO_ASIGNADO = codigoUsuarioAsignado;
                        detallePDT.FECHA_CREACION = DateTime.Now;
                        detallePDT.CREADO_POR = HttpContext.Session.GetString("user");

                        _context.Add(detallePDT);

                        //Guardamos las actividades
                        await _context.SaveChangesAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }

        /// <summary>
        /// Eliminar actividad asignada a un usuario
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Eliminar_Actividad_asignada(int codigoActividad, int numeroPdt, string codigoUsuarioAsignado)
        {
            try
            {
                // Obtén el registro completo basado en los criterios
                var actividad = await _context.AU_DETALLE_PLAN_DE_TRABAJO
                    .Where(a => a.NUMERO_PDT == numeroPdt &&
                                a.CODIGO_ACTIVIDAD == codigoActividad &&
                                a.CODIGO_USUARIO_ASIGNADO == codigoUsuarioAsignado)
                    .FirstOrDefaultAsync();

                _context.AU_DETALLE_PLAN_DE_TRABAJO.Remove(actividad);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }

        /// <summary>
        /// Metodo para obtener las actividades asignadas
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ActividadesAsignadas(int numero_pdt, string cod_auditor = "T")
        {
            dynamic data = new List<dynamic>();
            var estadoRequest = Request.Form["estado"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault().ToUpper();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault().ToUpper();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var userLogeado = HttpContext.Session.GetString("user");

            var query = _context.AU_DETALLE_PLAN_DE_TRABAJO
                .Where(e => e.NUMERO_PDT == numero_pdt);

            var recordsT = _context.AU_DETALLE_PLAN_DE_TRABAJO
                .Where(e => e.NUMERO_PDT == numero_pdt);

            // Agrega el filtro condicional por cod_auditor solo si no es "T"
            if (cod_auditor != "T")
            {
                query = query.Where(e => e.CODIGO_USUARIO_ASIGNADO == cod_auditor);
                recordsT = recordsT.Where(e => e.CODIGO_USUARIO_ASIGNADO == cod_auditor);
            }

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {

                    query = query.Where(e => e.mg_actividades.NOMBRE_ACTIVIDAD.ToUpper().Contains(searchValue))
                    .OrderByDescending(e => e.CODIGO_USUARIO_ASIGNADO == userLogeado) // Ordena para que el usuario logueado esté primero
                    .ThenByDescending(e => e.CODIGO_USUARIO_ASIGNADO) // Luego ordena ascendentemente por el código de usuario asignado
                        .ThenByDescending(e => e.mg_actividades.NOMBRE_ACTIVIDAD) // Finalmente, ordena ascendentemente por el nombre de la actividad
                    .Skip(skip)
                    .Take(pageSize);

                    var result = query.Select(e => new
                    {
                        e.CODIGO_ACTIVIDAD,
                        e.NUMERO_PDT,
                        e.NUMERO_AUDITORIA_INTEGRAL,
                        e.ANIO_AI,
                        e.NUMERO_AUDITORIA,
                        e.CODIGO_ESTADO,
                        e.CODIGO_USUARIO_ASIGNADO,
                        e.FECHA_CREACION,
                        e.CREADO_POR,
                        e.FECHA_ACTUALIZACION,
                        e.ACTUALIZADO_POR,
                        NOMBRE_ACTIVIDAD = e.mg_actividades != null ? e.mg_actividades.NOMBRE_ACTIVIDAD : "", // Verificar null para mg_actividades
                        DESCRIPCION = e.mg_actividades != null ? e.mg_actividades.DESCRIPCION : "", // Verificar null para mg_actividades
                        NOMBRE_USUARIO = e.mg_usuarios != null ? e.mg_usuarios.NOMBRE_USUARIO : "" // Verificar null para mg_usuarios
                    });

                    data = await result.ToListAsync();
                }
                else
                {
                    query = query.Where(e => e.mg_actividades.NOMBRE_ACTIVIDAD.Contains(searchValue))
                    .OrderBy(e => e.CODIGO_USUARIO_ASIGNADO == userLogeado) // Primero ordena para que el usuario logueado esté primero
                    .ThenBy(e => e.CODIGO_USUARIO_ASIGNADO) // Luego ordena ascendentemente por el código de usuario asignado
                    .ThenBy(e => e.mg_actividades.NOMBRE_ACTIVIDAD) // Finalmente, ordena ascendentemente por el nombre de la actividad                       
                    .Skip(skip)
                    .Take(pageSize);

                    var result = query.Select(e => new
                    {
                        e.CODIGO_ACTIVIDAD,
                        e.NUMERO_PDT,
                        e.NUMERO_AUDITORIA_INTEGRAL,
                        e.ANIO_AI,
                        e.NUMERO_AUDITORIA,
                        e.CODIGO_ESTADO,
                        e.CODIGO_USUARIO_ASIGNADO,
                        e.FECHA_CREACION,
                        e.CREADO_POR,
                        e.FECHA_ACTUALIZACION,
                        e.ACTUALIZADO_POR,
                        NOMBRE_ACTIVIDAD = e.mg_actividades != null ? e.mg_actividades.NOMBRE_ACTIVIDAD : "", // Verificar null para mg_actividades
                        DESCRIPCION = e.mg_actividades != null ? e.mg_actividades.DESCRIPCION : "", // Verificar null para mg_actividades
                        NOMBRE_USUARIO = e.mg_usuarios != null ? e.mg_usuarios.NOMBRE_USUARIO : "" // Verificar null para mg_usuarios
                    });

                    data = await result.ToListAsync();
                }

                recordsTotal = await recordsT.CountAsync(x => x.mg_actividades.NOMBRE_ACTIVIDAD.Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {

                    query = query.OrderByDescending(e => e.CODIGO_USUARIO_ASIGNADO == userLogeado) // Primero ordena para que el usuario logueado esté primero
                       .ThenByDescending(e => e.CODIGO_USUARIO_ASIGNADO) // Luego ordena ascendentemente por el código de usuario asignado
                        .ThenByDescending(e => e.mg_actividades.NOMBRE_ACTIVIDAD) // Finalmente, ordena ascendentemente por el nombre de la actividad
                        .Skip(skip)
                        .Take(pageSize);

                    var result = query.Select(e => new
                    {
                        e.CODIGO_ACTIVIDAD,
                        e.NUMERO_PDT,
                        e.NUMERO_AUDITORIA_INTEGRAL,
                        e.ANIO_AI,
                        e.NUMERO_AUDITORIA,
                        e.CODIGO_ESTADO,
                        e.CODIGO_USUARIO_ASIGNADO,
                        e.FECHA_CREACION,
                        e.CREADO_POR,
                        e.FECHA_ACTUALIZACION,
                        e.ACTUALIZADO_POR,
                        NOMBRE_ACTIVIDAD = e.mg_actividades != null ? e.mg_actividades.NOMBRE_ACTIVIDAD : "", // Verificar null para mg_actividades
                        DESCRIPCION = e.mg_actividades != null ? e.mg_actividades.DESCRIPCION : "", // Verificar null para mg_actividades
                        NOMBRE_USUARIO = e.mg_usuarios != null ? e.mg_usuarios.NOMBRE_USUARIO : "" // Verificar null para mg_usuarios
                    });

                    data = await result.ToListAsync();
                }
                else
                {
                    query = query.OrderBy(e => e.CODIGO_USUARIO_ASIGNADO == userLogeado) // Primero ordena para que el usuario logueado esté primero
                        .ThenBy(e => e.CODIGO_USUARIO_ASIGNADO) // Luego ordena ascendentemente por el código de usuario asignado
                        .ThenBy(e => e.mg_actividades.NOMBRE_ACTIVIDAD) // Finalmente, ordena ascendentemente por el nombre de la actividad
                        .Skip(skip)
                        .Take(pageSize);

                    var result = query.Select(e => new
                    {
                        e.CODIGO_ACTIVIDAD,
                        e.NUMERO_PDT,
                        e.NUMERO_AUDITORIA_INTEGRAL,
                        e.ANIO_AI,
                        e.NUMERO_AUDITORIA,
                        e.CODIGO_ESTADO,
                        e.CODIGO_USUARIO_ASIGNADO,
                        e.FECHA_CREACION,
                        e.CREADO_POR,
                        e.FECHA_ACTUALIZACION,
                        e.ACTUALIZADO_POR,
                        NOMBRE_ACTIVIDAD = e.mg_actividades != null ? e.mg_actividades.NOMBRE_ACTIVIDAD : "", // Verificar null para mg_actividades
                        DESCRIPCION = e.mg_actividades != null ? e.mg_actividades.DESCRIPCION : "", // Verificar null para mg_actividades
                        NOMBRE_USUARIO = e.mg_usuarios != null ? e.mg_usuarios.NOMBRE_USUARIO : "" // Verificar null para mg_usuarios
                    });

                    data = await result.ToListAsync();
                }

                recordsTotal = await recordsT.CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);

        }


        //********************************************************************************
        // HALLAZGOS
        //********************************************************************************

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaResultados")]
        public async Task<IActionResult> AuditoriaResultados(string ca, string pdt, string us)
        {
            //Obtenemos la informacion de la url y la decodificamos
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
            string codigoActividad = DecodeBase64(ca);
            string numeroPdt = DecodeBase64(pdt);
            string codigoUsuarioAsignado = DecodeBase64(us);
            int codigoActividadInt = int.Parse(codigoActividad);
            int numeroPdtInt = int.Parse(numeroPdt);

            HttpContext.Session.SetInt32("codigoActividadInt", codigoActividadInt);
            HttpContext.Session.SetInt32("numeroPdt", numeroPdtInt);
            HttpContext.Session.SetString("codigoUsuarioAsignado", codigoUsuarioAsignado);

            var detalles_actividad = await _context.MG_ACTIVIDADES
                .Where(a => a.au_detalle_plan_trabajo
                .Any(d => d.CODIGO_ACTIVIDAD == codigoActividadInt &&
                d.NUMERO_PDT == numeroPdtInt &&
                d.NUMERO_AUDITORIA_INTEGRAL == cod &&
                d.ANIO_AI == anio))
                .Include(a => a.au_detalle_plan_trabajo
                .Where(d => d.NUMERO_PDT == numeroPdtInt &&
                    d.NUMERO_AUDITORIA_INTEGRAL == cod &&
                    d.ANIO_AI == anio))
                .ToListAsync();

            // Ejemplo de uso
            if (detalles_actividad != null)
            {
                foreach (var item in detalles_actividad)
                {
                    var nombreActividad = item.NOMBRE_ACTIVIDAD;
                    var codActividad = item.CODIGO_ACTIVIDAD;
                    var detalles = item.au_detalle_plan_trabajo;

                    // Puedes pasar esta estructura a tu ViewBag o modelo de vista
                    ViewBag.DETALLES_ACTIVIDAD = new
                    {
                        Nombre_actividad = nombreActividad,
                        Codigo_actividad = codActividad,
                        Detalles = detalles
                    };
                }
            }
            else
            {
                // Manejo de caso cuando no se encuentra la actividad
                ViewBag.DETALLES_ACTIVIDAD = null;
            }


            var hallazgos = await _context.MG_HALLAZGOS
                .Where(d => d.CODIGO_ACTIVIDAD == codigoActividadInt)
                .Where(d => d.NUMERO_PDT == numeroPdtInt)
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .Include(d => d.Detalles)
                .Include(d => d.OrientacionCalificacion.Where(s => s.ESTADO == "A"))
                .Include(d => d.Documentos)
                .ToListAsync();

            var hallazgoDetalles = await _context.MG_HALLAZGOS_DETALLES
                .Where(d => d.CODIGO_HALLAZGO == codigoActividadInt)
                .ToListAsync();


            string queryParams = "?ca=" + Uri.EscapeDataString(ca) + "&pdt=" + Uri.EscapeDataString(pdt) + "&us=" + Uri.EscapeDataString(us);
            HttpContext.Session.SetString("params_base64_hallazgos", queryParams);
            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.HALLAZGOS = hallazgos;

            return View();
        }


        /// <summary>
        /// Pantalla para agregar Hallazgos a una actividad
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaResultados/AuditoriaHallazgo")]
        public async Task<IActionResult> AuditoriaHallazgo(int id = 0)
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
            int codigoActividadInt = (int)HttpContext.Session.GetInt32("codigoActividadInt");
            int numeroPdtInt = (int)HttpContext.Session.GetInt32("numeroPdt");
            string codigoUsuarioAsignado = HttpContext.Session.GetString("codigoUsuarioAsignado");
            string params_base64_hallazgos = HttpContext.Session.GetString("params_base64_hallazgos");

            var detalles_actividad = await _context.MG_ACTIVIDADES
                .Where(a => a.au_detalle_plan_trabajo
                .Any(d => d.CODIGO_ACTIVIDAD == codigoActividadInt &&
                d.NUMERO_PDT == numeroPdtInt &&
                d.NUMERO_AUDITORIA_INTEGRAL == cod &&
                d.ANIO_AI == anio))
                .Include(a => a.au_detalle_plan_trabajo
                .Where(d => d.NUMERO_PDT == numeroPdtInt &&
                    d.NUMERO_AUDITORIA_INTEGRAL == cod &&
                    d.ANIO_AI == anio))
                .ToListAsync();


            var objetivosinternos = await _context.MG_OBJETIVOS_INTERNOS
                .Where(a => a.ESTADO == "A")
                .ToListAsync();

            // Ejemplo de uso
            if (detalles_actividad != null)
            {
                foreach (var item in detalles_actividad)
                {
                    var nombreActividad = item.NOMBRE_ACTIVIDAD;
                    var codActividad = item.CODIGO_ACTIVIDAD;
                    var detalles = item.au_detalle_plan_trabajo;

                    // Puedes pasar esta estructura a tu ViewBag o modelo de vista
                    ViewBag.DETALLES_ACTIVIDAD = new
                    {
                        Nombre_actividad = nombreActividad,
                        Codigo_actividad = codActividad,
                        Detalles = detalles
                    };
                }
            }
            else
            {
                // Manejo de caso cuando no se encuentra la actividad
                ViewBag.DETALLES_ACTIVIDAD = null;
            }

            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.PARAMS_BASE64 = params_base64_hallazgos;
            ViewBag.OBJETIVOSINTERNOS = objetivosinternos;

            if (id > 0)
            {
                var hallazgo = await _context.MG_HALLAZGOS
                               .Where(d => d.CODIGO_HALLAZGO == id)
                               .Include(d => d.Detalles.OrderBy(o => o.TIPO))
                               .Include(d => d.OrientacionCalificacion.Where(o => o.ESTADO == "A"))
                               .Include(d => d.Documentos)
                               .FirstOrDefaultAsync();
                ViewBag.DOCUMENTOS = hallazgo.Documentos;

                ViewBag.HALLAZGO = hallazgo;
            }

            return View();
        }

        /// <summary>
        /// Cargar información de orientaciones de calificaciones
        /// </summary>
        /// <param name="calificacion"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> OrientacionCalificacion(int calificacion)
        {
            try
            {
                var orientaciones = await _context.MG_ORIENTACION_CALIFICACION.Where(x => x.NIVEL_RIESGO == calificacion && x.ESTADO == "A").Select(x => x.ORIENTACION).ToListAsync();

                return Json(new { success = true, data = orientaciones });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Metodo para decodificar los datos codificados de la URL
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string DecodeBase64(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Guardar Hallazgo
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GuardarHallazgos(IFormCollection formularioData)
        {
            int nuevoIdHallazgo = 0;

            try
            {
                int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
                int numPDT = (int)HttpContext.Session.GetInt32("numeroPdt");
                int codigoActividadInt = (int)HttpContext.Session.GetInt32("codigoActividadInt");

                //Obtenemos el codigo del siguiente registro segun el anio actual
                int maxNumeroHallazgo = await _context.MG_HALLAZGOS
                    .MaxAsync(a => (int?)a.CODIGO_HALLAZGO) ?? 0;

                // Incrementar el valor máximo en 1
                nuevoIdHallazgo = maxNumeroHallazgo + 1;

                int nivel_riesgo = 1;
                int vm = 0;
                int mi = 0;
                double desviacion = 0;
                // Obtener calificación
                string calificacionStr = formularioData["calificacion"];
                int? calificacion = string.IsNullOrEmpty(calificacionStr) ? (int?)null : int.Parse(calificacionStr);

                if (calificacion == 1)
                {
                    string valorMuestraStr = formularioData["valor_muestra"];
                    vm = string.IsNullOrEmpty(valorMuestraStr) ? 0 : int.Parse(valorMuestraStr);
                    string muestraInconsistenteStr = formularioData["muestra_inconsistente"];
                    mi = string.IsNullOrEmpty(muestraInconsistenteStr) ? 0 : int.Parse(muestraInconsistenteStr);

                    if (vm == 0)
                    {
                        nivel_riesgo = 1;
                    }
                    else
                    {
                        desviacion = Math.Round(((double)mi / vm) * 100, 2); // Redondea a 2 decimales

                        if (desviacion <= 5.1)
                        {
                            nivel_riesgo = 1;
                        }
                        else if (desviacion > 5.1 && desviacion <= 9.99)
                        {
                            nivel_riesgo = 2;
                        }
                        else
                        {
                            nivel_riesgo = 3;
                        }
                    }
                }
                else
                {
                    string nivelRiesgoStr = formularioData["nivel_riesgo"];
                    nivel_riesgo = string.IsNullOrEmpty(nivelRiesgoStr) ? 1 : int.Parse(nivelRiesgoStr);
                }


                Mg_Hallazgos Hallazgo = new();
                Hallazgo.CODIGO_HALLAZGO = nuevoIdHallazgo;
                Hallazgo.HALLAZGO = formularioData["hallazgo"];
                Hallazgo.CALIFICACION = calificacion;
                Hallazgo.VALOR_MUESTRA = vm;
                Hallazgo.MUESTRA_INCONSISTENTE = mi;
                Hallazgo.DESVIACION_MUESTRA = desviacion;
                Hallazgo.NIVEL_RIESGO = nivel_riesgo;
                Hallazgo.CONDICION = formularioData["condicion"];
                Hallazgo.CRITERIO = formularioData["criterio"];
                Hallazgo.CODIGO_ACTIVIDAD = codigoActividadInt;
                Hallazgo.NUMERO_PDT = numPDT;
                Hallazgo.NUMERO_AUDITORIA_INTEGRAL = cod;
                Hallazgo.ANIO_AI = anio;
                Hallazgo.NUMERO_AUDITORIA = 1;
                Hallazgo.FECHA_CREACION = DateTime.Now;
                Hallazgo.CREADO_POR = HttpContext.Session.GetString("user");

                _context.Add(Hallazgo);


                // Crear la lista de detalles
                List<Mg_hallazgos_detalles> detalles = new List<Mg_hallazgos_detalles>();


                //**************************************************************************************

                // Obtener el JSON que representa el array 'causageneral'
                string causageneralJson = formularioData["causageneral"];

                // Deserializar el JSON a una lista de diccionarios
                var causageneral = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(causageneralJson);

                foreach (var causa in causageneral)
                {
                    var causasObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = nuevoIdHallazgo,
                        DESCRIPCION = causa["causa"], // Acceder al campo 'causa'
                        TIPO = causa["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(causasObj);
                }

                //**************************************************************************************
                // Obtener el JSON que representa el array 'recomendaciones'
                string recomendacionesJson = formularioData["recomendaciones"];

                // Deserializar el JSON a una lista de diccionarios
                var recomendaciones = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(recomendacionesJson);

                foreach (var recom in recomendaciones)
                {
                    var recomendacionObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = nuevoIdHallazgo,
                        DESCRIPCION = recom["recomendaciones"], // Acceder al campo 'recomendaciones'
                        TIPO = recom["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(recomendacionObj);
                }

                //**************************************************************************************
                // Obtener el JSON que representa el array 'efecto'
                string efectoJson = formularioData["efecto"];

                // Deserializar el JSON a una lista de diccionarios
                var efecto = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(efectoJson);

                foreach (var efect in efecto)
                {
                    var efectoObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = nuevoIdHallazgo,
                        DESCRIPCION = efect["efecto"], // Acceder al campo 'efecto'
                        TIPO = efect["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(efectoObj);
                }


                //**************************************************************************************
                // Obtener el JSON que representa el array 'efecto'
                string comentariosJson = formularioData["comentarios"];

                // Deserializar el JSON a una lista de diccionarios
                var comentarios = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(comentariosJson);

                foreach (var coment in comentarios)
                {
                    var comentariosObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = nuevoIdHallazgo,
                        DESCRIPCION = coment["comentarios"], // Acceder al campo 'comentarios'
                        TIPO = coment["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(comentariosObj);
                }

                _context.AddRange(detalles);

                // Guardamos todos los cambios
                await _context.SaveChangesAsync();


                //**************************************************************************************
                // Guardar archivos adjuntos si se han enviado
                var archivosAdjuntos = formularioData.Files;
                var directorio = Path.Combine("wwwroot", "Archivos", "Auditorias", "Documentos");

                // Verificar si el directorio existe, si no, crearlo
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                List<Mg_hallazgos_documentos> listadoDocumentos = new List<Mg_hallazgos_documentos>();

                foreach (var archivo in archivosAdjuntos)
                {
                    if (archivo.Length > 0)
                    {
                        int mayorCodigoHallazgo = await _context.MG_HALLAZGOS_DOCUMENTOS
                      .MaxAsync(o => (int?)o.CODIGO_HALLAZGO_DOCUMENTO) ?? 0;

                        // Generar un número aleatorio de 5 dígitos
                        var random = new Random();
                        var randomNumber = random.Next(10000, 99999);
                        //Nombre del archivo que con el que se guarda
                        var FileName = randomNumber + "_" + archivo.FileName;

                        var filePath = Path.Combine(directorio, FileName);
                        long fileSizeInKB = archivo.Length / 1024;

                        var documento = new Mg_hallazgos_documentos
                        {
                            CODIGO_HALLAZGO_DOCUMENTO = mayorCodigoHallazgo + 1,
                            NOMBRE_DOCUMENTO = FileName,
                            CODIGO_HALLAZGO = nuevoIdHallazgo,
                            PESO = fileSizeInKB + " KB",
                            FECHA_CREACION = DateTime.Now,
                            CREADO_POR = HttpContext.Session.GetString("user")
                        };

                        //listadoDocumentos.Add(documento);
                        _context.Add(documento);

                        //Guardamos el rol editado
                        await _context.SaveChangesAsync();

                        try
                        {
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await archivo.CopyToAsync(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al guardar el archivo: {ex.Message}"); // Registro de error
                        }
                    }
                }

                _context.MG_HALLAZGOS_DOCUMENTOS.AddRange(listadoDocumentos);



            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }


        /// <summary>
        /// Editar un hallazgo
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditarHallazgo(IFormCollection formularioData)
        {
            try
            {
                int codigo_hallazgo = int.Parse(formularioData["codigo_hallazgo"].ToString());
                int nivel_riesgo = 1;
                int vm = 0;
                int mi = 0;
                double desviacion = 0;
                // Obtener calificación
                string calificacionStr = formularioData["calificacion"];
                int? calificacion = string.IsNullOrEmpty(calificacionStr) ? (int?)null : int.Parse(calificacionStr);

                if (calificacion == 1)
                {
                    string valorMuestraStr = formularioData["valor_muestra"];
                    vm = string.IsNullOrEmpty(valorMuestraStr) ? 0 : int.Parse(valorMuestraStr);
                    string muestraInconsistenteStr = formularioData["muestra_inconsistente"];
                    mi = string.IsNullOrEmpty(muestraInconsistenteStr) ? 0 : int.Parse(muestraInconsistenteStr);

                    if (vm == 0)
                    {
                        nivel_riesgo = 1;
                    }
                    else
                    {
                        desviacion = Math.Round(((double)mi / vm) * 100, 2); // Redondea a 2 decimales

                        if (desviacion <= 5.1)
                        {
                            nivel_riesgo = 1;
                        }
                        else if (desviacion > 5.1 && desviacion <= 9.99)
                        {
                            nivel_riesgo = 2;
                        }
                        else
                        {
                            nivel_riesgo = 3;
                        }
                    }
                }
                else
                {
                    string nivelRiesgoStr = formularioData["nivel_riesgo"];
                    nivel_riesgo = string.IsNullOrEmpty(nivelRiesgoStr) ? 1 : int.Parse(nivelRiesgoStr);
                }


                var Hallazgo = await _context.MG_HALLAZGOS
                                    .Where(u => u.CODIGO_HALLAZGO == codigo_hallazgo)
                                    .FirstOrDefaultAsync();

                _context.MG_HALLAZGOS.Remove(Hallazgo);
                _context.SaveChanges();

                Hallazgo.HALLAZGO = formularioData["hallazgo"];
                Hallazgo.CALIFICACION = calificacion;
                Hallazgo.VALOR_MUESTRA = vm;
                Hallazgo.MUESTRA_INCONSISTENTE = mi;
                Hallazgo.DESVIACION_MUESTRA = desviacion;
                Hallazgo.NIVEL_RIESGO = nivel_riesgo;
                Hallazgo.CONDICION = formularioData["condicion"];
                Hallazgo.CRITERIO = formularioData["criterio"];
                Hallazgo.FECHA_MODIFICACION = DateTime.Now;
                Hallazgo.MODIFICADO_POR = HttpContext.Session.GetString("user");

                _context.Add(Hallazgo);

                var detallesAEliminar = await _context.MG_HALLAZGOS_DETALLES
                                      .Where(d => d.CODIGO_HALLAZGO == codigo_hallazgo)
                                      .ToListAsync();


                //Guardamos cambios en la tabla de Hallazgos
                if (detallesAEliminar.Any())
                {
                    _context.MG_HALLAZGOS_DETALLES.RemoveRange(detallesAEliminar);
                }

                // Guardamos los cambios en la base de datos
                await _context.SaveChangesAsync();

                // Crear la lista de detalles
                List<Mg_hallazgos_detalles> detalles = new List<Mg_hallazgos_detalles>();

                //**************************************************************************************

                // Obtener el JSON que representa el array 'causageneral'
                string causageneralJson = formularioData["causageneral"];

                // Deserializar el JSON a una lista de diccionarios
                var causageneral = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(causageneralJson);

                foreach (var causa in causageneral)
                {
                    var causasObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = codigo_hallazgo,
                        DESCRIPCION = causa["causa"], // Acceder al campo 'causa'
                        TIPO = causa["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(causasObj);
                }

                //**************************************************************************************
                // Obtener el JSON que representa el array 'recomendaciones'
                string recomendacionesJson = formularioData["recomendaciones"];

                // Deserializar el JSON a una lista de diccionarios
                var recomendaciones = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(recomendacionesJson);

                foreach (var recom in recomendaciones)
                {
                    var recomendacionObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = codigo_hallazgo,
                        DESCRIPCION = recom["recomendaciones"], // Acceder al campo 'recomendaciones'
                        TIPO = recom["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(recomendacionObj);
                }

                //**************************************************************************************
                // Obtener el JSON que representa el array 'efecto'
                string efectoJson = formularioData["efecto"];

                // Deserializar el JSON a una lista de diccionarios
                var efecto = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(efectoJson);

                foreach (var efect in efecto)
                {
                    var efectoObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = codigo_hallazgo,
                        DESCRIPCION = efect["efecto"], // Acceder al campo 'efecto'
                        TIPO = efect["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(efectoObj);
                }


                //**************************************************************************************
                // Obtener el JSON que representa el array 'efecto'
                string comentariosJson = formularioData["comentarios"];

                // Deserializar el JSON a una lista de diccionarios
                var comentarios = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(comentariosJson);

                foreach (var coment in comentarios)
                {
                    var comentariosObj = new Mg_hallazgos_detalles
                    {
                        CODIGO_HALLAZGO = codigo_hallazgo,
                        DESCRIPCION = coment["comentarios"], // Acceder al campo 'comentarios'
                        TIPO = coment["id"]            // Acceder al campo 'id'
                    };
                    detalles.Add(comentariosObj);
                }

                _context.AddRange(detalles);

                // Guardamos todos los cambios
                await _context.SaveChangesAsync();


                //**************************************************************************************
                // Guardar archivos adjuntos si se han enviado
                var archivosAdjuntos = formularioData.Files;
                var directorio = Path.Combine("wwwroot", "Archivos", "Auditorias", "Documentos");

                // Verificar si el directorio existe, si no, crearlo
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                List<Mg_hallazgos_documentos> listadoDocumentos = new List<Mg_hallazgos_documentos>();

                foreach (var archivo in archivosAdjuntos)
                {
                    if (archivo.Length > 0)
                    {
                        int mayorCodigoHallazgo = await _context.MG_HALLAZGOS_DOCUMENTOS
                      .MaxAsync(o => (int?)o.CODIGO_HALLAZGO_DOCUMENTO) ?? 0;

                        // Generar un número aleatorio de 5 dígitos
                        var random = new Random();
                        var randomNumber = random.Next(10000, 99999);
                        //Nombre del archivo que con el que se guarda
                        var FileName = randomNumber + "_" + archivo.FileName;

                        var filePath = Path.Combine(directorio, FileName);
                        long fileSizeInKB = archivo.Length / 1024;

                        var documento = new Mg_hallazgos_documentos
                        {
                            CODIGO_HALLAZGO_DOCUMENTO = mayorCodigoHallazgo + 1,
                            NOMBRE_DOCUMENTO = FileName,
                            CODIGO_HALLAZGO = codigo_hallazgo,
                            PESO = fileSizeInKB + " KB",
                            FECHA_CREACION = DateTime.Now,
                            CREADO_POR = HttpContext.Session.GetString("user")
                        };

                        //listadoDocumentos.Add(documento);
                        _context.Add(documento);

                        //Guardamos el rol editado
                        await _context.SaveChangesAsync();

                        try
                        {
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await archivo.CopyToAsync(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al guardar el archivo: {ex.Message}"); // Registro de error
                        }
                    }
                }

                _context.MG_HALLAZGOS_DOCUMENTOS.AddRange(listadoDocumentos);



            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
        }


        /// <summary>
        /// Eliminar un documento de un Hallazgo
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> EliminarDocumento(int codigo)
        {
            try
            {
                // Lógica para eliminar el documento de la base de datos usando el código proporcionado
                var documento = await _context.MG_HALLAZGOS_DOCUMENTOS.FindAsync(codigo);
                if (documento != null)
                {
                    _context.MG_HALLAZGOS_DOCUMENTOS.Remove(documento);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Documento no encontrado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }






        //********************************************************************************
        // CARTA DE INGRESO Y SALIDA
        //********************************************************************************



        /// <summary>
        /// Mostrar la carta de ingreso
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaCartaIngreso")]
        public async Task<IActionResult> AuditoriaCartaIngresoAsync(string id)
        {
            ViewBag.id = id;

            MemoryStream workStream = new MemoryStream();
            byte[] pdfGenerateMemoryStream;
            ErrorPDF errorPDF = new ErrorPDF(_context, _config, _contextAccessor);

            CartaIngreso cartaDeIngreso = new CartaIngreso(_context, _config, _contextAccessor);
            pdfGenerateMemoryStream = await cartaDeIngreso.CreateCartaIngresoPDF(id);

            if (pdfGenerateMemoryStream.Length == 0)
            {
                errorPDF = new ErrorPDF(_context, _config, _contextAccessor);
                pdfGenerateMemoryStream = await errorPDF.CreateErrorPDF();
            }

            var documentoBase64 = Convert.ToBase64String(pdfGenerateMemoryStream);
            TempData["Base64PDF"] = documentoBase64;

            return View();
        }

        /// <summary>
        /// Modificar fecha de inicio y fin de la auditoria integral
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ModificarFechaIntegral(string id, DateTime inicio, DateTime fin, string tipo)
        {
            try
            {
                var auditoria = await _context.AU_AUDITORIAS_INTEGRALES
                                    .FirstOrDefaultAsync(a => a.CODIGO_AUDITORIA == id);

                // Modificar las fechas de inicio y fin por tipo de fecha
                if (tipo == "visita")
                {
                    auditoria.FECHA_INICIO_VISITA = inicio;
                    auditoria.FECHA_FIN_VISITA = fin;
                }
                else
                {
                    auditoria.PERIODO_INICIO_REVISION = inicio;
                    auditoria.PERIODO_FIN_REVISION = fin;
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Modificar fecha de inicio y fin de la auditoria integral
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ModificarTexto(string id, string texto, int tipo)
        {
            try
            {
                var auditoria = await _context.AU_AUDITORIAS_INTEGRALES
                                    .FirstOrDefaultAsync(a => a.CODIGO_AUDITORIA == id);

                var carta = await _context.MG_CARTAS
                                    .FirstOrDefaultAsync(u => u.TIPO_CARTA == tipo && u.NUMERO_AUDITORIA_INTEGRAL == auditoria.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == auditoria.ANIO_AI);

                carta.TEXTO_CARTA = texto;

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obtenemos el texto para mostrarlo en el editor
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ObtenerTextoCarta(string id, int tipo)
        {
            try
            {
                var auditoria = await _context.AU_AUDITORIAS_INTEGRALES
                                    .FirstOrDefaultAsync(a => a.CODIGO_AUDITORIA == id);

                var carta = await _context.MG_CARTAS
                                    .FirstOrDefaultAsync(u => u.TIPO_CARTA == tipo && u.NUMERO_AUDITORIA_INTEGRAL == auditoria.NUMERO_AUDITORIA_INTEGRAL && u.ANIO_AI == auditoria.ANIO_AI);

                return Json(new { success = true, message = carta.TEXTO_CARTA });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mostrar la carta de salida
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaCartaSalida")]
        public async Task<IActionResult> AuditoriaCartaSalida(string id)
        {
            ViewBag.id = id;

            MemoryStream workStream = new MemoryStream();
            byte[] pdfGenerateMemoryStream;
            ErrorPDF errorPDF = new ErrorPDF(_context, _config, _contextAccessor);

            CartaSalida cartaDeSalida = new CartaSalida(_context, _config, _contextAccessor);
            pdfGenerateMemoryStream = await cartaDeSalida.CreateCartaSalidaPDF(id);

            if (pdfGenerateMemoryStream.Length == 0)
            {
                errorPDF = new ErrorPDF(_context, _config, _contextAccessor);
                pdfGenerateMemoryStream = await errorPDF.CreateErrorPDF();
            }

            var documentoBase64 = Convert.ToBase64String(pdfGenerateMemoryStream);
            TempData["Base64PDF"] = documentoBase64;

            return View();
        }



        //********************************************************************************
        // INFORME PRELIMINAR
        //********************************************************************************


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaResultados/AuditoriaResultadosInforme")]
        public async Task<IActionResult> AuditoriaResultadosInformeAsync()
        {
            int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
            int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

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
            var dataCuestionarios = await _context.MG_AUDITORIAS_CUESTIONARIOS
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO == anio)
                    .Where(e => e.CODIGO_ESTADO == 3)
                    .ToListAsync();

            //List<Mg_secciones> secciones = new List<Mg_secciones>();

            //// Iteramos sobre los cuestionarios
            //foreach (var item in data)
            //{
            //    secciones = await _context.MG_SECCIONES
            //        .Include(x => x.sub_secciones
            //        .Where(sub => sub.CODIGO_CUESTIONARIO == item.CODIGO_CUESTIONARIO))
            //        .ThenInclude(x => x.Preguntas_Cuestionarios)
            //        .ToListAsync();
            //}

            var seccInformesPreli = await _context.MG_SECC_INF_PRELI
                .Where(d => d.NUMERO_AUDITORIA_INTEGRAL == cod)
                .Where(d => d.ANIO_AI == anio)
                .OrderBy(d => d.CODIGO_SEC_INF)
                .ToListAsync();

            List<ResultadoAuditoria> resultadosAuditorias = new List<ResultadoAuditoria>();

            foreach (var dataCuestionario in dataCuestionarios)
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

                resultadosAuditorias.Add(new ResultadoAuditoria {
                    CODIGO_CUESTIONARIO = cuestionario.CODIGO_CUESTIONARIO,
                    DESCRIPCION = cuestionario.NOMBRE_CUESTIONARIO,
                    listPorcentajeSubSecciones = porcentaje
                });
            }

            ViewBag.TITULO_AUDITORIA = HttpContext.Session.GetString("titulo_auditoria");
            ViewBag.NUMERO_AUDITORIA_INTEGRAL = cod;
            ViewBag.ANIO_AUDITORIA_INTEGRAL = anio;
            ViewBag.AU_TXT_INFOR_PRELIM = Infor;
            ViewBag.HALLAZGOS = hallazgosAllData;
            ViewBag.HALLAZGOS_ANTERIORES = hallazgosAnteriores;
            ViewBag.SECC_INF_PRELI = seccInformesPreli;
            ViewBag.RESULTADOS_AUDITORIAS = resultadosAuditorias;

            return View();
        }

        /// <summary>
        /// Modificar el texto de las secciones
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ModificarTextoResultadoInforme(string texto, string titulo)
        {
            try
            {
                int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

                var registro = await _context.MG_SECC_INF_PRELI
                                    .FirstOrDefaultAsync(u => u.NUMERO_AUDITORIA_INTEGRAL == cod && u.ANIO_AI == anio && u.TITULO == titulo.ToUpper());

                if (registro != null)
                {
                    registro.TEXTO_SECCION = texto;
                }
                else
                {
                    int maxNumeroRegistro = await _context.MG_SECC_INF_PRELI
                    .MaxAsync(a => (int?)a.CODIGO_SEC_INF) ?? 0;

                    registro = new Mg_secc_inf_preli
                    {
                        CODIGO_SEC_INF = maxNumeroRegistro + 1,
                        NUMERO_AUDITORIA_INTEGRAL = cod,
                        ANIO_AI = anio,
                        TITULO = titulo.ToUpper(),
                        TEXTO_SECCION = texto,
                        MOSTRAR_SECCION = 1
                    };

                    _context.MG_SECC_INF_PRELI.Add(registro);
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar la seccion
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> EliminarSeccion(int codigoSecInf)
        {
            try
            {
                int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

                var registro = await _context.MG_SECC_INF_PRELI
                                    .FirstOrDefaultAsync(u => u.NUMERO_AUDITORIA_INTEGRAL == cod && u.ANIO_AI == anio && u.CODIGO_SEC_INF == codigoSecInf);

                if (registro != null)
                {
                    // Eliminar el registro
                    _context.MG_SECC_INF_PRELI.Remove(registro);

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "La sección no fue encontrada." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Modificar fecha de inicio y fin de la auditoria integral
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ModificarTextoAuditoriaRI(int id, string texto, string tipo)
        {
            try
            {
                if (id == 0)
                {
                    int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                    int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
                    int maxNumeroRegistro = await _context.AU_TXT_INFOR_PRELIM
                    .MaxAsync(a => (int?)a.CODIGO_TXT_INF_PREL) ?? 0;

                    var AuditoriaIntegral = await _context.AU_AUDITORIAS_INTEGRALES
                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .FirstOrDefaultAsync();

                    var nuevoRegistro = new Au_txt_infor_prelim
                    {
                        CODIGO_TXT_INF_PREL = maxNumeroRegistro + 1,
                        CODIGO_AUDITORIA = AuditoriaIntegral.CODIGO_AUDITORIA,
                        NUMERO_AUDITORIA_INTEGRAL = cod,
                        ANIO_AI = anio,
                        MOTIVO_INFORME = tipo == "motivoContent" ? texto : null,
                        OBJETIVO = tipo == "objetivoContent" ? texto : null,
                        ALCANCE = tipo == "alcanceContent" ? texto : null,
                        TEXTO_CONCLUSION_GENERAL = tipo == "conclusionContent" ? texto : null,
                        PROCEDIMIENTOS_AUDITORIA = tipo == "procedimientoAuditoriaContent" ? texto : null,
                        MOSTRAR_CONCLUSION_GENERAL = 0
                    };

                    _context.AU_TXT_INFOR_PRELIM.Add(nuevoRegistro);
                }
                else
                {
                    var infor = await _context.AU_TXT_INFOR_PRELIM
                                   .FirstOrDefaultAsync(a => a.CODIGO_TXT_INF_PREL == id);

                    if (infor != null)
                    {
                        switch (tipo)
                        {
                            case "motivoContent":
                                infor.MOTIVO_INFORME = texto;
                                break;
                            case "objetivoContent":
                                infor.OBJETIVO = texto;
                                break;
                            case "alcanceContent":
                                infor.ALCANCE = texto;
                                break;
                            case "conclusionContent":
                                infor.TEXTO_CONCLUSION_GENERAL = texto;
                                break;
                            case "procedimientoAuditoriaContent":
                                infor.PROCEDIMIENTOS_AUDITORIA = texto;
                                break;
                            default:

                                break;
                        }
                    }
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Modificar fecha de inicio y fin de la auditoria integral
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ActivarConclusionAuditoriaRI(int id, bool estado)
        {
            try
            {
                if (id == 0)
                {
                    int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                    int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");
                    int maxNumeroRegistro = await _context.AU_TXT_INFOR_PRELIM
                    .MaxAsync(a => (int?)a.CODIGO_TXT_INF_PREL) ?? 0;

                    var AuditoriaIntegral = await _context.AU_AUDITORIAS_INTEGRALES
                    .Where(u => u.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .FirstOrDefaultAsync();

                    var nuevoRegistro = new Au_txt_infor_prelim
                    {
                        NUMERO_AUDITORIA_INTEGRAL = cod,
                        CODIGO_AUDITORIA = AuditoriaIntegral.CODIGO_AUDITORIA,
                        ANIO_AI = anio,
                        CODIGO_TXT_INF_PREL = maxNumeroRegistro + 1,
                        MOSTRAR_CONCLUSION_GENERAL = estado ? 1 : 0,
                    };

                    _context.AU_TXT_INFOR_PRELIM.Add(nuevoRegistro);
                }
                else
                {
                    var infor = await _context.AU_TXT_INFOR_PRELIM
                                   .FirstOrDefaultAsync(a => a.CODIGO_TXT_INF_PREL == id);

                    if (infor != null)
                    {
                        infor.MOSTRAR_CONCLUSION_GENERAL = estado ? 1 : 0;
                    }
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GuardarComentario(List<IFormFile> archivos, string comentario, int codigo)
        {
            // Verificar si no se han enviado archivos o si no se ha agregado comentario
            if (archivos == null || archivos.Count == 0)
            {
                return Json(new { success = false, message = "No se han agregado documentos." });
            }
            else if (comentario == null || comentario == "")
            {
                return Json(new { success = false, message = "No se ha agregado comentario." });
            }

            try
            {
                int maxNumeroRegistro = await _context.MG_COMENT_AUDITADO
                .MaxAsync(a => (int?)a.CODIGO_COMENT_AUDITADO) ?? 0;

                string estado = "";

                var comentarioAuditado = await _context.MG_COMENT_AUDITADO
                .Where(u => u.CODIGO_HALLAZGO == codigo)
                .FirstOrDefaultAsync();
                 
                if (comentarioAuditado == null)
                {
                    var nuevoRegistro = new Mg_coment_auditado
                    {
                        CODIGO_COMENT_AUDITADO = maxNumeroRegistro + 1,
                        CODIGO_HALLAZGO = codigo,
                        COMENTARIO = comentario,
                        FECHA_CREACION = DateTime.Now,
                        CREADO_POR = HttpContext.Session.GetString("user")
                    };

                    _context.MG_COMENT_AUDITADO.Add(nuevoRegistro);
                    estado = "Comentario y archivos guardados correctamente";
                }
                else
                {
                    comentarioAuditado.COMENTARIO = comentario;
                    comentarioAuditado.FECHA_MODIFICACION = DateTime.Now;
                    comentarioAuditado.MODIFICADO_POR = HttpContext.Session.GetString("user");
                    estado = "Comentario y archivos editados correctamente";
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                var directorio = Path.Combine("wwwroot", "Archivos", "AuditoriasInforme", "Documentos");

                // Verificar si el directorio existe, si no, crearlo
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                // Procesar cada archivo recibido
                foreach (var archivo in archivos)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        int maxNumeroDocumentos = (await _context.MG_DOCS_AUDITADO
                        .MaxAsync(a => (int?)a.CODIGO_DOC_AUDITADO) ?? 0) + 1;
                        
                        string peso = "";
                        //Nombre del archivo que con el que se guarda
                        var FileName = "AU" + maxNumeroDocumentos + "_" + archivo.FileName.Replace(" ", "_");

                        var filePath = Path.Combine(directorio, FileName);

                        if (archivo.Length >= 1024 * 1024)
                        {
                            peso = Math.Round((double)archivo.Length / (1024 * 1024), 2) + "MB";
    }
                        else
                        {
                            peso = Math.Round((double)archivo.Length / 1024, 2) + "KB";
                        }

                        var nuevoRegistro = new Mg_docs_auditado
                        {
                            CODIGO_DOC_AUDITADO = maxNumeroDocumentos,
                            CODIGO_COMENT_AUDITADO = codigo > 0 ? codigo : maxNumeroRegistro + 1,
                            NOMBRE_DOCUMENTO = FileName,
                            PESO = peso,
                            FECHA_CREACION = DateTime.Now,
                            CREADO_POR = HttpContext.Session.GetString("user")
                        };

                        _context.MG_DOCS_AUDITADO.Add(nuevoRegistro);

                        // Guardar los cambios en la base de datos
                        await _context.SaveChangesAsync();

                        // Guardamos el Archivo
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await archivo.CopyToAsync(stream);
                        }
                    }
                }

                return Json(new { success = true, message = estado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Eliminar un documento de un Hallazgo de resultado de informe
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> EliminarDocumentoInforme(int codigo)
        {
            try
            {
                // Lógica para eliminar el documento de la base de datos usando el código proporcionado
                var documento = await _context.MG_DOCS_AUDITADO.FindAsync(codigo);
                if (documento != null)
                {
                    // Obtener la ruta del archivo
                    var directorio = Path.Combine("wwwroot", "Archivos", "AuditoriasInforme", "Documentos");
                    var filePath = Path.Combine(directorio, documento.NOMBRE_DOCUMENTO);

                    // Eliminar el archivo físico si existe
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    _context.MG_DOCS_AUDITADO.Remove(documento);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Documento no encontrado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //********************************************************************************
        // PAGES NEWS
        //********************************************************************************

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ProgramarAuditoriaPost()
        {

            return Json("Ok");
        }

        /// <summary>
        /// Mostrar la matriz de hallazgos 
        /// </summary>
        /// <returns></returns>
        [Route("/MatrizHallazgos")]
        public async Task<IActionResult> MatrizHallazgos()
        {
            return View();
        }

        /// <summary>
        /// Obtener la matriz de hallazgo
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetMatrizHallazgos()
        {
            var estadoRequest = Request.Form["estado"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault().ToUpper();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault().ToUpper();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            List<Au_auditorias_integrales> info = new List<Au_auditorias_integrales>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    info = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue))
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(h => h.listado_planes_trabajo)
                        .ThenInclude(h => h.listado_detalles_plan_trabajo)
                        .ThenInclude(h => h.listado_hallazgos)
                        .Include(u => u.universo_auditable)
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(ai => new Au_auditorias_integrales
                        {
                            NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                            CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                            ANIO_AI = ai.ANIO_AI,
                            IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                            NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                            CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                            listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                            {
                                CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                {
                                    listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                    {
                                        CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                        listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                        {
                                            CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                            CONDICION = h.CONDICION,
                                            NIVEL_RIESGO = h.NIVEL_RIESGO,
                                            listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                            {
                                                CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                DESCRIPCION = d.DESCRIPCION,
                                                TIPO = d.TIPO
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                        })
                        .ToListAsync();
                }
                else
                {
                    info = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderBy(e => e.FECHA_CREACION)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue))
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(h => h.listado_planes_trabajo)
                        .ThenInclude(h => h.listado_detalles_plan_trabajo)
                        .ThenInclude(h => h.listado_hallazgos)
                        .Include(u => u.universo_auditable)
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(ai => new Au_auditorias_integrales
                        {
                            NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                            CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                            ANIO_AI = ai.ANIO_AI,
                            IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                            NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                            CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                            listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                            {
                                CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                {
                                    listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                    {
                                        CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                        listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                        {
                                            CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                            CONDICION = h.CONDICION,
                                            NIVEL_RIESGO = h.NIVEL_RIESGO,
                                            listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                            {
                                                CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                DESCRIPCION = d.DESCRIPCION,
                                                TIPO = d.TIPO
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                        })
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS_INTEGRALES
                    .CountAsync(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    info = await _context.AU_AUDITORIAS_INTEGRALES
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(h => h.listado_planes_trabajo)
                        .ThenInclude(h => h.listado_detalles_plan_trabajo)
                        .ThenInclude(h => h.listado_hallazgos)
                        .Include(u => u.universo_auditable)
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(ai => new Au_auditorias_integrales
                        {
                            NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                            CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                            ANIO_AI = ai.ANIO_AI,
                            IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                            NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                            CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                            listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                            {
                                CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                {
                                    listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                    {
                                        CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                        listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                        {
                                            CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                            CONDICION = h.CONDICION,
                                            NIVEL_RIESGO = h.NIVEL_RIESGO,
                                            listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                            {
                                                CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                DESCRIPCION = d.DESCRIPCION,
                                                TIPO = d.TIPO
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                        })
                        .ToListAsync();
                }
                else
                {
                    info = await _context.AU_AUDITORIAS_INTEGRALES
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                        .ThenInclude(h => h.listado_planes_trabajo)
                        .ThenInclude(h => h.listado_detalles_plan_trabajo)
                        .ThenInclude(h => h.listado_hallazgos)
                        .Include(u => u.universo_auditable)
                        .OrderBy(e => e.FECHA_CREACION)
                        .Skip(skip)
                        .Take(pageSize)
                         .Select(ai => new Au_auditorias_integrales
                         {
                             NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                             CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                             ANIO_AI = ai.ANIO_AI,
                             IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                             NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                             CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                             listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                             {
                                 CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                 NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                 TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                 listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                 {
                                     listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                     {
                                         CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                         listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                         {
                                             CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                             CONDICION = h.CONDICION,
                                             NIVEL_RIESGO = h.NIVEL_RIESGO,
                                             listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                             {
                                                 CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                 DESCRIPCION = d.DESCRIPCION,
                                                 TIPO = d.TIPO
                                             }).ToList()
                                         }).ToList()
                                     }).ToList()
                                 }).ToList()
                             }).ToList(),
                         })
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS_INTEGRALES
                    .CountAsync();
            }

            List<MatrizHallazgosResumen> data = new List<MatrizHallazgosResumen>();

            foreach (var AuIntegral in info)
            {
                foreach (var auditoria in AuIntegral.listado_auditorias)
                {
                    foreach (var planTrabajo in auditoria.listado_planes_trabajo)
                    {
                        foreach (var detallePlan in planTrabajo.listado_detalles_plan_trabajo)
                        {
                            foreach (var hallazgo in detallePlan.listado_hallazgos)
                            {
                                string recomendaciones = "";
                                foreach (var detalles in hallazgo.listado_detalles)
                                {
                                    if (detalles.TIPO.ToLower().Contains("recomendacion"))
                                    {
                                        recomendaciones = recomendaciones == "" ? detalles.DESCRIPCION : recomendaciones + ", " + detalles.DESCRIPCION;
                                    }
                                }

                                data.Add(new MatrizHallazgosResumen
                                {
                                    CODIGO_HALLAZGO = AuIntegral.CODIGO_AUDITORIA + "-"+ AuIntegral.NUMERO_AUDITORIA_INTEGRAL,
                                    CONDICION = hallazgo.CONDICION,
                                    RECOMENDACION = recomendaciones,
                                    ANEXOS = "FALTA...",
                                    ACCIONES_PREV_CORR = "FALTA...",
                                    EVIDENCIA = "FALTA..."
                                });
                            }
                        }
                    }
                }
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }

        /// <summary>
        /// Obtener la matriz de hallazgo completa
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetMatrizHallazgosCompleto()
        {
            var estadoRequest = Request.Form["estado"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault().ToUpper();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault().ToUpper();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            List<Au_auditorias_integrales> listMatriz = new List<Au_auditorias_integrales>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    listMatriz = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue))
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(h => h.listado_hallazgos)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(u => u.mg_usuarios)
                        .Include(u => u.universo_auditable)
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(ai => new Au_auditorias_integrales
                        {
                            NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                            CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                            ANIO_AI = ai.ANIO_AI,
                            IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                            NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                            CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                            listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                            {
                                CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                NUMERO_AUDITORIA = a.NUMERO_AUDITORIA,
                                NUMERO_AUDITORIA_INTEGRAL = a.NUMERO_AUDITORIA_INTEGRAL,
                                ANIO_AE = a.ANIO_AE,
                                NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                {
                                    NUMERO_PDT = p.NUMERO_PDT,
                                    NUMERO_AUDITORIA = p.NUMERO_AUDITORIA,
                                    NUMERO_AUDITORIA_INTEGRAL = p.NUMERO_AUDITORIA_INTEGRAL,
                                    ANIO_AUDITORIA = p.ANIO_AUDITORIA,
                                    listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                    {
                                        CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                        NUMERO_PDT = d.NUMERO_PDT,
                                        NUMERO_AUDITORIA = d.NUMERO_AUDITORIA,
                                        NUMERO_AUDITORIA_INTEGRAL = d.NUMERO_AUDITORIA_INTEGRAL,
                                        ANIO_AI = d.ANIO_AI,
                                        CODIGO_USUARIO_ASIGNADO = d.CODIGO_USUARIO_ASIGNADO,
                                        NOMBRE_USUARIO_ASIGNADO = d.mg_usuarios.NOMBRE_USUARIO,
                                        listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                        {
                                            CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                            CONDICION = h.CONDICION,
                                            NIVEL_RIESGO = h.NIVEL_RIESGO,
                                            NUMERO_PDT = h.NUMERO_PDT,
                                            listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                            {
                                                CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                DESCRIPCION = d.DESCRIPCION,
                                                TIPO = d.TIPO
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                        })
                        .ToListAsync();
                }
                else
                {
                    listMatriz = await _context.AU_AUDITORIAS_INTEGRALES
                        .OrderBy(e => e.FECHA_CREACION)
                        .Where(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue))
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(h => h.listado_hallazgos)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(u => u.mg_usuarios)
                        .Include(u => u.universo_auditable)
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(ai => new Au_auditorias_integrales
                        {
                            NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                            CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                            ANIO_AI = ai.ANIO_AI,
                            IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                            NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                            CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                            listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                            {
                                CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                NUMERO_AUDITORIA = a.NUMERO_AUDITORIA,
                                NUMERO_AUDITORIA_INTEGRAL = a.NUMERO_AUDITORIA_INTEGRAL,
                                ANIO_AE = a.ANIO_AE,
                                NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                {
                                    NUMERO_PDT = p.NUMERO_PDT,
                                    NUMERO_AUDITORIA = p.NUMERO_AUDITORIA,
                                    NUMERO_AUDITORIA_INTEGRAL = p.NUMERO_AUDITORIA_INTEGRAL,
                                    ANIO_AUDITORIA = p.ANIO_AUDITORIA,
                                    listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                    {
                                        CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                        NUMERO_PDT = d.NUMERO_PDT,
                                        NUMERO_AUDITORIA = d.NUMERO_AUDITORIA,
                                        NUMERO_AUDITORIA_INTEGRAL = d.NUMERO_AUDITORIA_INTEGRAL,
                                        ANIO_AI = d.ANIO_AI,
                                        CODIGO_USUARIO_ASIGNADO = d.CODIGO_USUARIO_ASIGNADO,
                                        NOMBRE_USUARIO_ASIGNADO = d.mg_usuarios.NOMBRE_USUARIO,
                                        listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                        {
                                            CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                            CONDICION = h.CONDICION,
                                            NIVEL_RIESGO = h.NIVEL_RIESGO,
                                            NUMERO_PDT = h.NUMERO_PDT,
                                            listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                            {
                                                CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                DESCRIPCION = d.DESCRIPCION,
                                                TIPO = d.TIPO
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                        })
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS_INTEGRALES
                    .CountAsync(e => e.CODIGO_AUDITORIA.ToUpper().Contains(searchValue) || e.NOMBRE_AUDITORIA.ToUpper().Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    listMatriz = await _context.AU_AUDITORIAS_INTEGRALES
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(h => h.listado_hallazgos)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(u => u.mg_usuarios)
                        .Include(u => u.universo_auditable)
                        .OrderByDescending(e => e.FECHA_CREACION)
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(ai => new Au_auditorias_integrales
                        {
                            NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                            CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                            ANIO_AI = ai.ANIO_AI,
                            IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                            NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                            CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                            listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                            {
                                CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                NUMERO_AUDITORIA = a.NUMERO_AUDITORIA,
                                NUMERO_AUDITORIA_INTEGRAL = a.NUMERO_AUDITORIA_INTEGRAL,
                                ANIO_AE = a.ANIO_AE,
                                NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                {
                                    NUMERO_PDT = p.NUMERO_PDT,
                                    NUMERO_AUDITORIA = p.NUMERO_AUDITORIA,
                                    NUMERO_AUDITORIA_INTEGRAL = p.NUMERO_AUDITORIA_INTEGRAL,
                                    ANIO_AUDITORIA = p.ANIO_AUDITORIA,
                                    listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                    {
                                        CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                        NUMERO_PDT = d.NUMERO_PDT,
                                        NUMERO_AUDITORIA = d.NUMERO_AUDITORIA,
                                        NUMERO_AUDITORIA_INTEGRAL = d.NUMERO_AUDITORIA_INTEGRAL,
                                        ANIO_AI = d.ANIO_AI,
                                        CODIGO_USUARIO_ASIGNADO = d.CODIGO_USUARIO_ASIGNADO,
                                        NOMBRE_USUARIO_ASIGNADO = d.mg_usuarios.NOMBRE_USUARIO,
                                        listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                        {
                                            CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                            CONDICION = h.CONDICION,
                                            NIVEL_RIESGO = h.NIVEL_RIESGO,
                                            NUMERO_PDT = h.NUMERO_PDT,
                                            listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                            {
                                                CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                DESCRIPCION = d.DESCRIPCION,
                                                TIPO = d.TIPO
                                            }).ToList()
                                        }).ToList()
                                    }).ToList()
                                }).ToList()
                            }).ToList(),
                        })
                        .ToListAsync();
                }
                else
                {
                    listMatriz = await _context.AU_AUDITORIAS_INTEGRALES
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(a => a.mg_tipos_de_auditorias)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(h => h.listado_hallazgos)
                        .Include(d => d.listado_auditorias)
                            .ThenInclude(h => h.listado_planes_trabajo)
                                .ThenInclude(h => h.listado_detalles_plan_trabajo)
                                    .ThenInclude(u => u.mg_usuarios)
                        .Include(u => u.universo_auditable)
                        .OrderBy(e => e.FECHA_CREACION)
                        .Skip(skip)
                        .Take(pageSize)
                         .Select(ai => new Au_auditorias_integrales
                         {
                             NUMERO_AUDITORIA_INTEGRAL = ai.NUMERO_AUDITORIA_INTEGRAL,
                             CODIGO_AUDITORIA = ai.CODIGO_AUDITORIA,
                             ANIO_AI = ai.ANIO_AI,
                             IF_FECHA_EMITIDO = ai.IF_FECHA_EMITIDO,
                             NOMBRE_UNIVERSO_AUDITABLE = ai.universo_auditable.NOMBRE,
                             CODIGO_UNIVERSO_AUDITABLE = ai.CODIGO_UNIVERSO_AUDITABLE,
                             listado_auditorias = ai.listado_auditorias.Select(a => new Au_auditorias
                             {
                                 CODIGO_AUDITORIA = a.CODIGO_AUDITORIA,
                                 NUMERO_AUDITORIA = a.NUMERO_AUDITORIA,
                                 NUMERO_AUDITORIA_INTEGRAL = a.NUMERO_AUDITORIA_INTEGRAL,
                                 ANIO_AE = a.ANIO_AE,
                                 NOMBRE_AUDITORIA = a.NOMBRE_AUDITORIA,
                                 TIPO_AUDITORIA = a.mg_tipos_de_auditorias.DESCRIPCION,
                                 listado_planes_trabajo = a.listado_planes_trabajo.Select(p => new Au_Planes_De_Trabajo
                                 {
                                     NUMERO_PDT = p.NUMERO_PDT,
                                     NUMERO_AUDITORIA = p.NUMERO_AUDITORIA,
                                     NUMERO_AUDITORIA_INTEGRAL = p.NUMERO_AUDITORIA_INTEGRAL,
                                     ANIO_AUDITORIA = p.ANIO_AUDITORIA,
                                     listado_detalles_plan_trabajo = p.listado_detalles_plan_trabajo.Select(d => new Au_detalle_plan_de_trabajo
                                     {
                                         CODIGO_ACTIVIDAD = d.CODIGO_ACTIVIDAD,
                                         NUMERO_PDT = d.NUMERO_PDT,
                                         NUMERO_AUDITORIA = d.NUMERO_AUDITORIA,
                                         NUMERO_AUDITORIA_INTEGRAL = d.NUMERO_AUDITORIA_INTEGRAL,
                                         ANIO_AI = d.ANIO_AI,
                                         CODIGO_USUARIO_ASIGNADO = d.CODIGO_USUARIO_ASIGNADO,
                                         NOMBRE_USUARIO_ASIGNADO = d.mg_usuarios.NOMBRE_USUARIO,
                                         listado_hallazgos = d.listado_hallazgos.Select(h => new Mg_Hallazgos
                                         {
                                             CODIGO_HALLAZGO = h.CODIGO_HALLAZGO,
                                             CONDICION = h.CONDICION,
                                             NIVEL_RIESGO = h.NIVEL_RIESGO,
                                             NUMERO_PDT = h.NUMERO_PDT,
                                             listado_detalles = h.Detalles.Select(d => new Mg_hallazgos_detalles
                                             {
                                                 CODIGO_HALLAZGO = d.CODIGO_HALLAZGO,
                                                 DESCRIPCION = d.DESCRIPCION,
                                                 TIPO = d.TIPO
                                             }).ToList()
                                         }).ToList()
                                     }).ToList()
                                 }).ToList()
                             }).ToList(),
                         })
                        .ToListAsync();
                }

                recordsTotal = await _context.AU_AUDITORIAS_INTEGRALES
                    .CountAsync();
            }

            List<MatrizHallazgos> data = new List<MatrizHallazgos>();

            foreach(var AuIntegral in listMatriz)
            {
                foreach(var auditoria in AuIntegral.listado_auditorias)
                {
                    foreach (var planTrabajo in auditoria.listado_planes_trabajo)
                    {
                        foreach (var detallePlan in planTrabajo.listado_detalles_plan_trabajo)
                        {
                            foreach (var hallazgo in detallePlan.listado_hallazgos)
                            {
                                data.Add(new MatrizHallazgos
                                {
                                    CORRELATIVO = AuIntegral.CODIGO_AUDITORIA,
                                    TIPO_AUDITORIA = auditoria.TIPO_AUDITORIA,
                                    NUMERO_INFORME = AuIntegral.NUMERO_AUDITORIA_INTEGRAL,
                                    FECHA_EMISION_INF_FINAL = AuIntegral.IF_FECHA_EMITIDO,
                                    UNIDAD_AUDITORIA = AuIntegral.NOMBRE_UNIVERSO_AUDITABLE,
                                    RESPONSABLE_UNI_AUDITORIA = "Falta..",
                                    CODIGO_HALLAZGO = AuIntegral.CODIGO_UNIVERSO_AUDITABLE + "-" + hallazgo.CODIGO_HALLAZGO,
                                    DESCRIPCION = "Falta..",
                                    NIVEL_RIESGO = hallazgo.NIVEL_RIESGO,
                                    PROCESO = "Falta..",
                                    OBJETIVO_CONTROL_INTERNO = "Falta..",
                                    OBJETIVO_ESTRATEGICO = "Falta..",
                                    ACCIONES_PREV_CORRE = "Falta..",
                                    FECHA_SOLUCION = null,
                                    FECHA_SOLUCIONO = null,
                                    DIAS_ATRAZO = 0,
                                    RESPONSABLE = detallePlan.NOMBRE_USUARIO_ASIGNADO,
                                    EVIDENCIA = "Falta..",
                                    UNIDAD_APOYO = "Falta..",
                                    ESTATUS = "Falta..",
                                    CODIGO_ACTIVIDAD = detallePlan.CODIGO_ACTIVIDAD,
                                    CODIGO_USUARIO_ASIGNADO = detallePlan.CODIGO_USUARIO_ASIGNADO,
                                    NUMERO_PDT = hallazgo.NUMERO_PDT,
                                    NUMERO_AUDITORIA_INTEGRAL = AuIntegral.NUMERO_AUDITORIA_INTEGRAL,
                                    ANIO_AI = AuIntegral.ANIO_AI
                                });
                            }
                        }
                    }
                }
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }
        
        /// <summary>
        /// Guardar las sesiones de la auditoria
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ActualizarVariablesSession(int numero_auditoria_integral, int anio_ai)
        {
            HttpContext.Session.SetInt32("num_auditoria_integral", numero_auditoria_integral);
            HttpContext.Session.SetInt32("anio_auditoria_integral", anio_ai);

            return new JsonResult("Ok");
        }
    }
}
