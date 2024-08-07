using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIA.Context;
using SIA.Models;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;

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
                    await _context.SaveChangesAsync();
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
            ViewBag.FECHA_APROBACION = dataMDP.FECHA_APROBACION?.ToString() ?? "-";
            ViewBag.TIPOS_DE_AUDITORIAS_REALIZAR = dataMDP.TEXTO_TIPO_AUDITORIA + " \n\n" + auditoriasText;
            ViewBag.TEXTO_AUDITORIAS_EDIT = dataMDP.TEXTO_TIPO_AUDITORIA;
            ViewBag.OBJETIVO_AUDITORIA = dataMDP.OBJETIVO;
            ViewBag.RECURSOS_AUDITORIA = dataMDP.RECURSOS;
            ViewBag.AUDITORES_AUDITORIA = dataMDP.TEXTO_EQUIPO_TRABAJO + " " + auditoresText;
            ViewBag.TIEMPO_AUDITORIA = dataMDP.TEXTO_TIEMPO_AUDITORIA + " " + fechaInicio + " al " + fechaFin;
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
                await _context.SaveChangesAsync();
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
        [Route("Auditorias/ProgramaTrabajo")]
        public IActionResult ProgramaTrabajo()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaResultados")]
        public IActionResult AuditoriaResultados()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("Auditorias/AuditoriaResultados/AuditoriaHallazgo")]
        public IActionResult AuditoriaHallazgo()
        {
            return View();
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
