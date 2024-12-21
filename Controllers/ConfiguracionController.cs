using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIA.Models;
using System.Text;
using SIA.Context;
using System.Text.Json.Serialization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SIA.Controllers
{
    public class ConfiguracionController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;

        public ConfiguracionController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;
        }

        //********************************************************************************
        // ACTIVIDADES
        //********************************************************************************

        public IActionResult Actividades()
        {
            return View();
        }

        /// <summary>
        /// Obtener listado de actividades segun el tipo de auditoria
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetActividades()
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
            var tipoAuditoria = Request.Form["tipoAuditoria"].FirstOrDefault().ToUpper();

            List<Mg_actividades> data = new();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.MG_ACTIVIDADES
                        .OrderByDescending(e => e.CODIGO_ESTADO)
                        .OrderByDescending(e => e.NOMBRE_ACTIVIDAD)
                        .Where(e => e.NOMBRE_ACTIVIDAD.ToUpper().Contains(searchValue))
                        .Where(e => e.CODIGO_TIPO_AUDITORIA == tipoAuditoria)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.MG_ACTIVIDADES
                        .OrderBy(e => e.CODIGO_ESTADO)
                        .OrderBy(e => e.NOMBRE_ACTIVIDAD)
                        .Where(e => e.NOMBRE_ACTIVIDAD.Contains(searchValue))
                        .Where(e => e.CODIGO_TIPO_AUDITORIA == tipoAuditoria)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.MG_ACTIVIDADES
                    .Where(e => e.CODIGO_TIPO_AUDITORIA == tipoAuditoria)
                    .CountAsync(x => x.NOMBRE_ACTIVIDAD
                    .Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.MG_ACTIVIDADES
                        .OrderByDescending(e => e.CODIGO_ESTADO)
                        .OrderByDescending(e => e.NOMBRE_ACTIVIDAD)
                        .Where(e => e.CODIGO_TIPO_AUDITORIA == tipoAuditoria)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.MG_ACTIVIDADES
                        .OrderBy(e => e.CODIGO_ESTADO)
                        .OrderBy(e => e.NOMBRE_ACTIVIDAD)
                        .Where(e => e.CODIGO_TIPO_AUDITORIA == tipoAuditoria)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.MG_ACTIVIDADES
                    .Where(e => e.CODIGO_TIPO_AUDITORIA == tipoAuditoria)
                    .CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }

        /// <summary>
        /// Guardar una nueva actividad
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Guardar_Actividad([FromBody] Mg_actividades userData)
        {
            int nuevoIdActividad = 0;

            if (userData == null) return new JsonResult("error");

            //Obtenemos el codigo del siguiente registro segun el anio actual
            int maxNumeroActividad = await _context.MG_ACTIVIDADES
                .MaxAsync(a => (int?)a.CODIGO_ACTIVIDAD) ?? 0;

            // Incrementar el valor máximo en 1
            nuevoIdActividad = maxNumeroActividad + 1;

            userData.CODIGO_ACTIVIDAD = nuevoIdActividad;
            userData.CODIGO_PROGRAMA_AUDITORIA = "AUAG";
            userData.CODIGO_ESTADO = "A";
            userData.USUARIO_ADICIONA = HttpContext.Session.GetString("user");
            userData.FECHA_ADICIONA = DateTime.Now;

            _context.Add(userData);

            try
            {
                //Guardamos la nueva actividad
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }

        /// <summary>
        /// Obtener informacion de la actividad a editar
        /// </summary>
        /// <param name="codigoActividad"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Informacion_Actividad(string codigoActividad)
        {
            try
            {
                int codigo = Int32.Parse(codigoActividad);

                var actividad = await _context.MG_ACTIVIDADES.FirstOrDefaultAsync(x => x.CODIGO_ACTIVIDAD == codigo);

                if (actividad == null) return new JsonResult("error");

                return Json(actividad);
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }

        /// <summary>
        /// Editar la informacion de la actividad
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Editar_Actividad([FromBody] Mg_actividades actData)
        {
            var actividad = await _context.MG_ACTIVIDADES
                .FirstOrDefaultAsync(x => x.CODIGO_ACTIVIDAD == actData.CODIGO_ACTIVIDAD);

            if (actividad == null) return new JsonResult("error");

            // Agregar campos adicionales de userData a la entidad usuario
            actividad.NOMBRE_ACTIVIDAD = actData.NOMBRE_ACTIVIDAD;
            actividad.DESCRIPCION = actData.DESCRIPCION;
            actividad.CODIGO_TIPO_AUDITORIA = actData.CODIGO_TIPO_AUDITORIA;
            actividad.USUARIO_MODIFICA = HttpContext.Session.GetString("user");
            actividad.FECHA_MODIFICA = DateTime.Now;

            _context.Entry(actividad).CurrentValues.SetValues(actividad);

            try
            {
                //Guardamos la actividad editada
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }

        /// <summary>
        /// Editar la informacion de la actividad
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Inactivar_Actividad(string codigoActividad)
        {
            var actividad = await _context.MG_ACTIVIDADES
                .FirstOrDefaultAsync(x => x.CODIGO_ACTIVIDAD == Int32.Parse(codigoActividad));

            if (actividad == null) return new JsonResult("error");

            string estado = "A";

            if (actividad.CODIGO_ESTADO == "A")
            {
                estado = "I";
            }

            actividad.CODIGO_ESTADO = estado;
            actividad.USUARIO_MODIFICA = HttpContext.Session.GetString("user");
            actividad.FECHA_MODIFICA = DateTime.Now;

            _context.Entry(actividad).CurrentValues.SetValues(actividad);

            try
            {
                //Guardamos la actividad editada
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }



        //********************************************************************************
        // CUESTIONARIOS
        //********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Cuestionarios()
        {
            return View();
        }

        /// <summary>
        /// Obtener los cuestionarios
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetCuestionarios()
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
                        .OrderByDescending(e => e.CODIGO_CUESTIONARIO)
                        .Where(e => e.NOMBRE_CUESTIONARIO.ToUpper().Contains(searchValue))
                        .Skip(skip).Take(pageSize).ToListAsync();
                }
                else
                {
                    data = await _context.AU_CUESTIONARIOS
                        .OrderBy(e => e.CODIGO_CUESTIONARIO)
                        .Where(e => e.NOMBRE_CUESTIONARIO.ToUpper().Contains(searchValue))
                        .Skip(skip).Take(pageSize).ToListAsync();
                }

                recordsTotal = await _context.AU_CUESTIONARIOS.CountAsync(x => x.NOMBRE_CUESTIONARIO.Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.AU_CUESTIONARIOS
                        .OrderByDescending(e => e.CODIGO_CUESTIONARIO)
                        .Skip(skip).Take(pageSize).ToListAsync();
                }
                else
                {
                    data = await _context.AU_CUESTIONARIOS
                        .OrderBy(e => e.CODIGO_CUESTIONARIO)
                        .Skip(skip).Take(pageSize).ToListAsync();
                }

                recordsTotal = await _context.AU_CUESTIONARIOS.CountAsync();
            }

            var cuestionariosConRespuestas = new List<object>();

            foreach (var cuestionario in data)
            {
                // Obtener las preguntas del cuestionario actual
                var preguntasDelCuestionario = await _context.MG_PREGUNTAS_CUESTIONARIO
                    .Where(p => p.CODIGO_CUESTIONARIO == cuestionario.CODIGO_CUESTIONARIO)
                    .Select(p => p.CODIGO_PREGUNTA)
                    .ToListAsync();

                // Verificar si existen respuestas para las preguntas del cuestionario
                var existeRespuesta = await _context.MG_RESPUESTAS_CUESTIONARIO
                    .AnyAsync(r => preguntasDelCuestionario.Contains(r.CODIGO_PREGUNTA));

                cuestionariosConRespuestas.Add(new
                {
                    cuestionario.CODIGO_CUESTIONARIO,
                    cuestionario.NOMBRE_CUESTIONARIO,
                    cuestionario.TIPO_CUESTIONARIO,
                    cuestionario.FECHA_CREACION,
                    cuestionario.CREADO_POR,
                    cuestionario.FECHA_MODIFICACION,
                    cuestionario.MODIFICADO_POR,
                    cuestionario.ESTADO,
                    cuestionario.CODIGO_TIPO_AUDITORIA,
                    EXISTE_RESPUESTAS = existeRespuesta
                });
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data = cuestionariosConRespuestas };
            return Ok(jsonData);
        }

        /// <summary>
        /// Editar un cuestionario
        /// </summary>
        /// <param name="idCuestionario"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Editar_Cuestionario(int idCuestionario)
        {
            try
            {
                if (idCuestionario == null)
                {
                    return NotFound();
                }

                //Obtenemos todos los submenus para mostrarlos en el modal de agregar nuevo rol
                Au_cuestionarios menu = await _context.AU_CUESTIONARIOS
                    .FirstOrDefaultAsync(x => x.CODIGO_CUESTIONARIO == idCuestionario);

                var result = new
                {
                    message = "success",
                    menu = menu
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Metodo para obtener un cuestionarios autorizado y ver sus preguntas
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerPreguntasCuestionario(int codigo = 0)
        {
            try
            {
                List<Mg_secciones> secciones = await _context.MG_SECCIONES.ToListAsync();

                if (codigo > 0)
                {
                    var cuestionarioConSecciones = await _context.MG_CUESTIONARIO_SECCIONES
                                                        .Where(cs => cs.CODIGO_CUESTIONARIO == codigo)
                                                        .ToListAsync();

                    List<Mg_secciones> seccionesConCuestionario = new List<Mg_secciones>();

                    foreach (var cuestionarioSeccion in cuestionarioConSecciones)
                    {
                        var subSecciones = await _context.MG_SUB_SECCIONES.Where(s => s.CODIGO_SECCION == cuestionarioSeccion.CODIGO_SECCION && s.CODIGO_CUESTIONARIO == codigo).ToListAsync();

                        foreach (var subseccion in subSecciones)
                        {
                            var preguntas = await _context.MG_PREGUNTAS_CUESTIONARIO.Where(s => s.CODIGO_SUB_SECCION == subseccion.CODIGO_SUB_SECCION && s.CODIGO_CUESTIONARIO == codigo).ToListAsync();

                            subseccion.Preguntas_Cuestionarios = preguntas;
                        }

                        secciones.FirstOrDefault(a => a.CODIGO_SECCION == cuestionarioSeccion.CODIGO_SECCION).sub_secciones = subSecciones;
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
        /// Verificar si ya existe el nombre de un cuestionario
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> VerificarNombreCuestionario(string? nombre)
        {
            int cuestionarioExiste = await _context.AU_CUESTIONARIOS.CountAsync(x => x.NOMBRE_CUESTIONARIO.ToUpper() == nombre.ToUpper());

            if (cuestionarioExiste >= 1)
            {
                return new JsonResult("true");
            }

            return new JsonResult("false");
        }

        /// <summary>
        /// Metodo para obtener un las secciones
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerSecciones()
        {
            try
            {
                List<Mg_secciones> secciones = await _context.MG_SECCIONES
                .Select(s => new Mg_secciones
                {
                    CODIGO_SECCION = s.CODIGO_SECCION,
                    DESCRIPCION_SECCION = s.DESCRIPCION_SECCION,
                    // Agregar cualquier otra propiedad de la sección que necesites
                    EXISTE_SUB_SECCION = _context.MG_SUB_SECCIONES.Any(sub => sub.CODIGO_SECCION == s.CODIGO_SECCION)
                })
                .OrderBy(c => c.CODIGO_SECCION)
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
        /// Metodo para agregar secciones
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AgregarSeccion(string? nombre)
        {
            try
            {
                //Obtenemos el codigo del siguiente registro segun el anio actual
                int maxCodigoSeccion = await _context.MG_SECCIONES
                    .MaxAsync(a => (int?)a.CODIGO_SECCION) ?? 0;

                // Crear una nueva entidad de la sección
                var nuevaSeccion = new Mg_secciones
                {
                    CODIGO_SECCION = maxCodigoSeccion + 1,
                    DESCRIPCION_SECCION = nombre,
                };

                // Guardar la nueva sección en la base de datos
                _context.MG_SECCIONES.Add(nuevaSeccion);
                _context.SaveChanges();

                // Retornar una respuesta de éxito
                return Json(new { success = true, message = "Sección agregada correctamente." });
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                return Json(new { success = false, message = "Ocurrió un error al agregar la sección: " + ex.Message });
            }
        }

        /// <summary>
        /// Metodo para eliminar secciones
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EliminarSeccion(int codigo)
        {
            try
            {
                var seccion = _context.MG_SECCIONES.FirstOrDefault(s => s.CODIGO_SECCION == codigo);

                if (seccion == null)
                {
                    // Si no se encuentra la sección, retornar un error
                    return Json(new { success = false, message = "No se encontró la sección con el código especificado." });
                }

                // Eliminar la entidad de la sección
                _context.MG_SECCIONES.Remove(seccion);
                _context.SaveChanges();

                // Retornar una respuesta de éxito
                return Json(new { success = true, message = "Sección eliminada correctamente." });
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                return Json(new { success = false, message = "Ocurrió un error al eliminar la sección: " + ex.Message });
            }
        }

        /// <summary>
        /// Guardar un cuestionario
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GuardarCuestionarioAsync([FromBody] Cuestionario_subseccion cuestionario)
        {
            int codigoCuestionario = 0;

            try
            {
                if (cuestionario.CODIGO_CUESTIONARIO == 0)
                {
                    codigoCuestionario = _context.AU_CUESTIONARIOS.Any() ? _context.AU_CUESTIONARIOS.Max(s => s.CODIGO_CUESTIONARIO) : 0;

                    codigoCuestionario = codigoCuestionario + 1;

                    // Crear una nueva entidad de la sección
                    var nuevoCuestionario = new Au_cuestionarios
                    {
                        CODIGO_CUESTIONARIO = codigoCuestionario,
                        NOMBRE_CUESTIONARIO = cuestionario.NOMBRE_CUESTIONARIO,
                        CREADO_POR = HttpContext.Session.GetString("user"),
                        FECHA_CREACION = DateTime.Now,
                        ESTADO = 1
                    };

                    _context.AU_CUESTIONARIOS.Add(nuevoCuestionario);
                }
                else
                {
                    codigoCuestionario = cuestionario.CODIGO_CUESTIONARIO;

                    // Eliminar preguntas relacionadas
                    var preguntasAEliminar = _context.MG_PREGUNTAS_CUESTIONARIO
                        .Where(p => p.CODIGO_CUESTIONARIO == codigoCuestionario).ToList();
                    if (preguntasAEliminar.Any())
                    {
                        _context.MG_PREGUNTAS_CUESTIONARIO.RemoveRange(preguntasAEliminar);
                    }

                    // Eliminar subsecciones relacionadas
                    var subSeccionesAEliminar = _context.MG_SUB_SECCIONES
                        .Where(s => s.CODIGO_CUESTIONARIO == codigoCuestionario).ToList();
                    if (subSeccionesAEliminar.Any())
                    {
                        _context.MG_SUB_SECCIONES.RemoveRange(subSeccionesAEliminar);
                    }

                    // Eliminar secciones relacionadas
                    var seccionesAEliminar = _context.MG_CUESTIONARIO_SECCIONES
                        .Where(s => s.CODIGO_CUESTIONARIO == codigoCuestionario).ToList();
                    if (seccionesAEliminar.Any())
                    {
                        _context.MG_CUESTIONARIO_SECCIONES.RemoveRange(seccionesAEliminar);
                    }
                    var cuestionarioExistente = await _context.AU_CUESTIONARIOS.FirstOrDefaultAsync(c => c.CODIGO_CUESTIONARIO == codigoCuestionario);

                    // Actualizar las propiedades del cuestionario existente
                    cuestionarioExistente.NOMBRE_CUESTIONARIO = cuestionario.NOMBRE_CUESTIONARIO;
                    cuestionarioExistente.MODIFICADO_POR = HttpContext.Session.GetString("user");
                    cuestionarioExistente.FECHA_MODIFICACION = DateTime.Now;

                    // Guardar los cambios después de eliminar
                    _context.SaveChanges();
                }

                var secciones = cuestionario.SUB_SECCIONES
                                .GroupBy(c => c.CODIGO_SECCION)
                                .Select(group => new Mg_cuestionario_secciones
                                {
                                    CODIGO_SECCION = group.Key,
                                    CODIGO_CUESTIONARIO = codigoCuestionario,
                                })
                                .ToList();

                _context.MG_CUESTIONARIO_SECCIONES.AddRange(secciones);

                foreach (var subSeccion in cuestionario.SUB_SECCIONES)
                {
                    int maxSubSeccion = _context.MG_SUB_SECCIONES.Any() ? _context.MG_SUB_SECCIONES.Max(s => s.CODIGO_SUB_SECCION) : 0;

                    var nuevaSubSeccion = new Mg_sub_secciones
                    {
                        CODIGO_SUB_SECCION = maxSubSeccion + 1,
                        DESCRIPCION = subSeccion.DESCRIPCION,
                        CODIGO_SECCION = subSeccion.CODIGO_SECCION,
                        CODIGO_CUESTIONARIO = codigoCuestionario
                    };

                    _context.MG_SUB_SECCIONES.Add(nuevaSubSeccion);

                    _context.SaveChanges();

                    foreach (var pregunta in subSeccion.Preguntas_Cuestionarios)
                    {
                        int maxPregunta = _context.MG_PREGUNTAS_CUESTIONARIO.Any() ? _context.MG_PREGUNTAS_CUESTIONARIO.Max(s => s.CODIGO_PREGUNTA) : 0;

                        var nuevaPregunta = new Mg_preguntas_cuestionario
                        {
                            CODIGO_PREGUNTA = maxPregunta + 1,
                            CODIGO_SUB_SECCION = maxSubSeccion + 1,
                            DESCRIPCION = pregunta.DESCRIPCION,
                            CODIGO_CUESTIONARIO = codigoCuestionario,
                            CREADO_POR = HttpContext.Session.GetString("user"),
                            FECHA_CREACION = DateTime.Now,
                        };

                        _context.MG_PREGUNTAS_CUESTIONARIO.Add(nuevaPregunta);

                        _context.SaveChanges();
                    }
                }

                // Retornar una respuesta de éxito
                return Json(new { success = true, message = "Cuestionario agregado con exito." });
            }
            catch (Exception ex)
            {
                if (codigoCuestionario > 0)
                {
                    // Eliminar preguntas relacionadas
                    var preguntasAEliminar = _context.MG_PREGUNTAS_CUESTIONARIO
                        .Where(p => p.CODIGO_CUESTIONARIO == codigoCuestionario).ToList();
                    if (preguntasAEliminar.Any())
                    {
                        _context.MG_PREGUNTAS_CUESTIONARIO.RemoveRange(preguntasAEliminar);
                    }

                    // Eliminar subsecciones relacionadas
                    var subSeccionesAEliminar = _context.MG_SUB_SECCIONES
                        .Where(s => s.CODIGO_CUESTIONARIO == codigoCuestionario).ToList();
                    if (subSeccionesAEliminar.Any())
                    {
                        _context.MG_SUB_SECCIONES.RemoveRange(subSeccionesAEliminar);
                    }

                    // Eliminar secciones relacionadas
                    var seccionesAEliminar = _context.MG_CUESTIONARIO_SECCIONES
                        .Where(s => s.CODIGO_CUESTIONARIO == codigoCuestionario).ToList();
                    if (seccionesAEliminar.Any())
                    {
                        _context.MG_CUESTIONARIO_SECCIONES.RemoveRange(seccionesAEliminar);
                    }

                    // Eliminar el cuestionario
                    var cuestionarioAEliminar = _context.AU_CUESTIONARIOS
                        .Where(c => c.CODIGO_CUESTIONARIO == codigoCuestionario).FirstOrDefault();
                    if (cuestionarioAEliminar != null)
                    {
                        _context.AU_CUESTIONARIOS.Remove(cuestionarioAEliminar);
                    }

                    // Guardar los cambios después de eliminar
                    _context.SaveChanges();
                }

                return Json(new { success = false, message = "Ocurrió un error al agregar el cuestionario: " + ex.Message });
            }
        }

        /// <summary>
        /// eliminar un cuestionario
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EliminarCuestionario(int codigo)
        {
            try
            {
                // Eliminar preguntas relacionadas
                var preguntasAEliminar = _context.MG_PREGUNTAS_CUESTIONARIO
                    .Where(p => p.CODIGO_CUESTIONARIO == codigo).ToList();
                _context.MG_PREGUNTAS_CUESTIONARIO.RemoveRange(preguntasAEliminar);

                // Guardar los cambios
                _context.SaveChanges();

                // Eliminar subsecciones relacionadas
                var subSeccionesAEliminar = _context.MG_SUB_SECCIONES
                    .Where(s => s.CODIGO_CUESTIONARIO == codigo).ToList();
                _context.MG_SUB_SECCIONES.RemoveRange(subSeccionesAEliminar);

                // Guardar los cambios
                _context.SaveChanges();

                // Eliminar secciones relacionadas
                var seccionesAEliminar = _context.MG_CUESTIONARIO_SECCIONES
                    .Where(s => s.CODIGO_CUESTIONARIO == codigo).ToList();
                _context.MG_CUESTIONARIO_SECCIONES.RemoveRange(seccionesAEliminar);

                // Guardar los cambios
                _context.SaveChanges();

                // Eliminar el cuestionario
                var cuestionarioAEliminar = _context.AU_CUESTIONARIOS
                    .FirstOrDefault(c => c.CODIGO_CUESTIONARIO == codigo);
                _context.AU_CUESTIONARIOS.Remove(cuestionarioAEliminar);

                // Guardar los cambios
                _context.SaveChanges();

                return Json(new { success = true, message = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ocurrió un error: " + ex.Message });
            }
        }



        //********************************************************************************
        // AGENCIAS
        //********************************************************************************
        /// <summary>
        /// Mantenimiento a Agencias
        /// </summary>
        /// <returns></returns>
        public IActionResult Agencias()
        {
            return View();
        }

        /// <summary>
        /// Obtener listado de las agencias
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetAgencias()
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

            List<Mg_agencias> data = new List<Mg_agencias>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.MG_AGENCIAS
                        .Where(e => e.CODIGO_AGENCIA != 0)
                        .Where(e => e.NOMBRE_AGENCIA.ToUpper().Contains(searchValue) || e.JEFE_AGENCIA.ToUpper().Contains(searchValue))
                        .OrderBy(e => e.CODIGO_AGENCIA)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.MG_AGENCIAS
                        .Where(e => e.CODIGO_AGENCIA != 0)
                        .Where(e => e.NOMBRE_AGENCIA.ToUpper().Contains(searchValue) || e.JEFE_AGENCIA.ToUpper().Contains(searchValue))
                        .OrderByDescending(e => e.CODIGO_AGENCIA)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.MG_AGENCIAS
                    .CountAsync(e => e.NOMBRE_AGENCIA.ToUpper().Contains(searchValue) || e.JEFE_AGENCIA.ToUpper().Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.MG_AGENCIAS
                        .Where(e => e.CODIGO_AGENCIA != 0)
                        .OrderBy(e => e.CODIGO_AGENCIA)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    data = await _context.MG_AGENCIAS
                        .Where(e => e.CODIGO_AGENCIA != 0)
                        .OrderByDescending(e => e.CODIGO_AGENCIA)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }

                recordsTotal = await _context.MG_AGENCIAS
                    .CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }

        /// Modificar Jefe Agencia
        /// </summary>
        /// <param name="nombreJefe"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ModificarJefeAgencia(int codigo, string nombreJefe)
        {
            try
            {
                var agencia = await _context.MG_AGENCIAS.FirstOrDefaultAsync(x => x.CODIGO_AGENCIA == codigo);
                if (agencia != null)
                {
                    agencia.JEFE_AGENCIA = nombreJefe;
                    agencia.FECHA_MODIFICA = DateTime.Now;
                    agencia.USUARIO_MODIFICA = HttpContext.Session.GetString("user");
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Error" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}