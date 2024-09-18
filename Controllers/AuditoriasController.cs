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
                        .Include(x => x.sub_secciones)
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
                        .Include(x => x.sub_secciones)
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
        public async Task<IActionResult> AuditoriaHallazgo()
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

            return View();
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
        /// Guardar actividades asignadas a un usuario
        /// </summary>
        /// <param name="DataAI"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GuardarHallazgos([FromBody] dynamic formularioData)
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

                var vm = string.IsNullOrEmpty(formularioData.GetProperty("valor_muestra").GetString()) ? (int?)null : int.Parse(formularioData.GetProperty("valor_muestra").GetString());
                var mi = string.IsNullOrEmpty(formularioData.GetProperty("muestra_inconsistente").GetString()) ? (int?)null : int.Parse(formularioData.GetProperty("muestra_inconsistente").GetString());
                int nivel_riesgo = 1;
                var desviacion = 0;

                if (vm == 0)
                {
                    nivel_riesgo = 1;
                }
                else
                {
                    desviacion = (mi / vm) * 100;

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

                Mg_Hallazgos Hallazgo = new();
                Hallazgo.CODIGO_HALLAZGO = nuevoIdHallazgo;
                Hallazgo.HALLAZGO = formularioData.GetProperty("hallazgo").GetString();
                Hallazgo.CALIFICACION = string.IsNullOrEmpty(formularioData.GetProperty("calificacion").GetString()) ? (int?)null : int.Parse(formularioData.GetProperty("calificacion").GetString());
                Hallazgo.VALOR_MUESTRA = vm;
                Hallazgo.MUESTRA_INCONSISTENTE = mi;
                Hallazgo.DESVIACION_MUESTRA = desviacion;
                Hallazgo.NIVEL_RIESGO = nivel_riesgo;
                Hallazgo.CONDICION = formularioData.GetProperty("condicion").GetString();
                Hallazgo.CRITERIO = formularioData.GetProperty("criterio").GetString();
                Hallazgo.CODIGO_ACTIVIDAD = codigoActividadInt;
                Hallazgo.NUMERO_PDT = numPDT;
                Hallazgo.NUMERO_AUDITORIA_INTEGRAL = cod;
                Hallazgo.ANIO_AI = anio;
                Hallazgo.NUMERO_AUDITORIA = 1;
                Hallazgo.FECHA_CREACION = DateTime.Now;
                Hallazgo.CREADO_POR = HttpContext.Session.GetString("user");

                _context.Add(Hallazgo);

                //Guardamos las actividades
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("Ok");
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
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaResultados/AuditoriaResultadosInforme")]
        public IActionResult AuditoriaResultadosInforme()
        {
            return View();
        }
    }
}
