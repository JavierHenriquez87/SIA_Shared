﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIA.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SIA.Context;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace SIA.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;

        public UsuariosController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;
        }

        // GET: Usuarios
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetUsuarios()
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

            List<Mg_usuarios_segun_app> data = new List<Mg_usuarios_segun_app>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await (from usuario in _context.MG_USUARIOS_SEGUN_APP
                                      join rol in _context.MG_ROLES
                                          on new { usuario.CODIGO_ROL, usuario.CODIGO_APLICACION }
                                          equals new { rol.CODIGO_ROL, rol.CODIGO_APLICACION }
                                      where usuario.NOMBRE.ToUpper().Contains(searchValue)
                                          && rol.CODIGO_APLICACION == "SIA"
                                      orderby usuario.NOMBRE descending
                                      select new Mg_usuarios_segun_app
                                      {
                                          CODIGO_USUARIO = usuario.CODIGO_USUARIO,
                                          NOMBRE = usuario.NOMBRE,
                                          CODIGO_ROL = usuario.CODIGO_ROL,
                                          NOMBRE_ROL = rol.NOMBRE,
                                          CODIGO_ESTADO = usuario.CODIGO_ESTADO
                                      })
                                     .Skip(skip)
                                     .Take(pageSize)
                                     .ToListAsync();
                }
                else
                {
                    data = await (from usuario in _context.MG_USUARIOS_SEGUN_APP
                                  join rol in _context.MG_ROLES
                                      on new { usuario.CODIGO_ROL, usuario.CODIGO_APLICACION }
                                      equals new { rol.CODIGO_ROL, rol.CODIGO_APLICACION }
                                  where usuario.NOMBRE.ToUpper().Contains(searchValue)
                                      && rol.CODIGO_APLICACION == "SIA"
                                  orderby usuario.NOMBRE ascending
                                  select new Mg_usuarios_segun_app
                                  {
                                      CODIGO_USUARIO = usuario.CODIGO_USUARIO,
                                      NOMBRE = usuario.NOMBRE,
                                      CODIGO_ROL = usuario.CODIGO_ROL,
                                      NOMBRE_ROL = rol.NOMBRE,
                                      CODIGO_ESTADO = usuario.CODIGO_ESTADO
                                  })
                                .Skip(skip)
                                .Take(pageSize)
                                .ToListAsync();
                }

                recordsTotal = await _context.MG_USUARIOS_SEGUN_APP.CountAsync(x => x.NOMBRE.Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await (from usuario in _context.MG_USUARIOS_SEGUN_APP
                                  join rol in _context.MG_ROLES
                                      on new { usuario.CODIGO_ROL, usuario.CODIGO_APLICACION }
                                      equals new { rol.CODIGO_ROL, rol.CODIGO_APLICACION }
                                  where rol.CODIGO_APLICACION == "SIA"
                                  orderby usuario.NOMBRE descending
                                  select new Mg_usuarios_segun_app
                                  {
                                      CODIGO_USUARIO = usuario.CODIGO_USUARIO,
                                      NOMBRE = usuario.NOMBRE,
                                      CODIGO_ROL = usuario.CODIGO_ROL,
                                      NOMBRE_ROL = rol.NOMBRE,
                                      CODIGO_ESTADO = usuario.CODIGO_ESTADO
                                  })
                                .Skip(skip)
                                .Take(pageSize)
                                .ToListAsync();
                }
                else
                {

                    data = await (from usuario in _context.MG_USUARIOS_SEGUN_APP
                                  join rol in _context.MG_ROLES
                                      on new { usuario.CODIGO_ROL, usuario.CODIGO_APLICACION }
                                      equals new { rol.CODIGO_ROL, rol.CODIGO_APLICACION }
                                  where rol.CODIGO_APLICACION == "SIA"
                                  orderby usuario.NOMBRE ascending
                                  select new Mg_usuarios_segun_app
                                  {
                                      CODIGO_USUARIO = usuario.CODIGO_USUARIO,
                                      NOMBRE = usuario.NOMBRE,
                                      CODIGO_ROL = usuario.CODIGO_ROL,
                                      NOMBRE_ROL = rol.NOMBRE,
                                      CODIGO_ESTADO = usuario.CODIGO_ESTADO
                                  })
                                .Skip(skip)
                                .Take(pageSize)
                                .ToListAsync();
                }

                recordsTotal = await _context.MG_USUARIOS_SEGUN_APP.CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }

        [HttpPost]
        public async Task<IActionResult> Informacion_Usuario(string idUsuario)
        {
            try
            {
                var usuario = await _context.MG_USUARIOS_SEGUN_APP.FirstOrDefaultAsync(x => x.CODIGO_USUARIO == idUsuario);

                if (usuario == null) return new JsonResult("error");

                return Json(usuario);
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Guardar_Usuario([FromBody] Mg_usuarios_segun_app userData)
        {

            if (userData == null) return new JsonResult("error");

            //userData.CODIGO_EMPRESA = 1;
            userData.CREADO_POR = HttpContext.Session.GetString("user");
            userData.FECHA_CREACION = DateTime.Now;

            //byte[] bytes = Encoding.UTF8.GetBytes(userData.CLAVE_USUARIO);

            // Calcular el hash SHA-256
            //byte[] hashBytes = SHA256.HashData(bytes);

            // Convertir el hash a una cadena base64
            //string hashString = Convert.ToBase64String(hashBytes);

            //userData.CLAVE_USUARIO = hashString;

            _context.Add(userData);

            try
            {
                //Guardamos el rol editado
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }

        [HttpPost]
        public async Task<IActionResult> Editar_Usuario([FromBody] Mg_usuarios_segun_app userData)
        {
            var usuario = await _context.MG_USUARIOS_SEGUN_APP.FirstOrDefaultAsync(x => x.CODIGO_USUARIO == userData.CODIGO_USUARIO && x.CODIGO_APLICACION == "SIA");

            if (usuario == null) return new JsonResult("error");

            //if (!string.IsNullOrEmpty(userData.CLAVE_USUARIO))
            //{
            //    byte[] bytes = Encoding.UTF8.GetBytes(userData.CLAVE_USUARIO);

            //    // Calcular el hash SHA-256
            //    byte[] hashBytes = SHA256.HashData(bytes);

            //    // Convertir el hash a una cadena base64
            //    string hashString = Convert.ToBase64String(hashBytes);

            //    usuario.CLAVE_USUARIO = hashString;
            //}

            // Agregar campos adicionales de userData a la entidad usuario
            //usuario.CODIGO_AGENCIA = userData.CODIGO_AGENCIA;
            usuario.CODIGO_ROL = userData.CODIGO_ROL;
            //usuario.CODIGO_CARGO = userData.CODIGO_CARGO;
            //usuario.ESTADO = userData.ESTADO;
            //usuario.NUMERO_IDENTIDAD = userData.NUMERO_IDENTIDAD;
            //usuario.NOMBRE_USUARIO = userData.NOMBRE_USUARIO;
            //usuario.EMAIL = userData.EMAIL;
            usuario.MODIFICADO_POR = HttpContext.Session.GetString("user");
            usuario.FECHA_MODIFICACION = DateTime.Now;

            _context.Entry(usuario).CurrentValues.SetValues(usuario);

            try
            {
                //Guardamos el rol editado
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("success");
        }

        [HttpPost]
        public async Task<ActionResult> GuardarFirmaUsuario(IFormFile firma, string codigoUsuarioFirma)
        {
            try
            {
                // Ruta completa del archivo, incluyendo el nombre deseado
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", "firmas_usuarios");
                var filePath = Path.Combine(folderPath, $"{codigoUsuarioFirma}.jpg");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await firma.CopyToAsync(stream);
                }

                var usuario = await _context.MG_USUARIOS.FirstOrDefaultAsync(x => x.CODIGO_USUARIO == codigoUsuarioFirma);
                usuario.FIRMA = "/assets/images/firmas_usuarios/" + codigoUsuarioFirma + ".jpg";
                usuario.USUARIO_MODIFICA = HttpContext.Session.GetString("user");
                usuario.FECHA_MODIFICA = DateTime.Now;
                _context.Entry(usuario).CurrentValues.SetValues(usuario);

                await _context.SaveChangesAsync();

                return new JsonResult("success");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new JsonResult(ex.Message);
            }
        }

        // GET: Roles
        public IActionResult Roles()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetRoles()
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

            List<Mg_roles> data = new List<Mg_roles>();

            if (!string.IsNullOrEmpty(searchValue) && searchValue.Count() >= 3)
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.MG_ROLES
                        .OrderByDescending(e => e.CODIGO_ROL)
                        .Where(e => e.NOMBRE.ToUpper().Contains(searchValue) && e.CODIGO_APLICACION == "SIA")
                        .Skip(skip).Take(pageSize).ToListAsync();
                }
                else
                {
                    data = await _context.MG_ROLES
                        .OrderBy(e => e.CODIGO_ROL)
                        .Where(e => e.NOMBRE.ToUpper().Contains(searchValue) && e.CODIGO_APLICACION == "SIA")
                        .Skip(skip).Take(pageSize).ToListAsync();
                }

                recordsTotal = await _context.MG_ROLES_DEL_SISTEMA.CountAsync(x => x.NOMBRE_ROL.Contains(searchValue));
            }
            else
            {
                if (sortColumnDirection.Equals("asc"))
                {
                    data = await _context.MG_ROLES
                        .OrderByDescending(e => e.CODIGO_ROL)
                        .Where(e => e.CODIGO_APLICACION == "SIA")
                        .Skip(skip).Take(pageSize).ToListAsync();
                }
                else
                {
                    data = await _context.MG_ROLES
                        .OrderBy(e => e.CODIGO_ROL)
                        .Where(e => e.CODIGO_APLICACION == "SIA")
                        .Skip(skip).Take(pageSize).ToListAsync();
                }

                recordsTotal = await _context.MG_ROLES.CountAsync();
            }

            var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
            return Ok(jsonData);
        }

        [HttpPost]
        public async Task<IActionResult> Editar_Rol(int idRol)
        {
            try
            {
                if (idRol == null)
                {
                    return NotFound();
                }

                //Obtenemos todos los submenus para mostrarlos en el modal de agregar nuevo rol
                List<Mg_menus_segun_rol> menu = await _context.MG_MENUS_SEGUN_ROL
                    .OrderBy(e => e.CODIGO_ROL)
                    .Where(x => x.CODIGO_ROL == idRol && x.CODIGO_APLICACION == "SIA")
                    .ToListAsync();

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

        [HttpPost]
        public async Task<IActionResult> VerificarNombreRolMenu(string? NOMBRE)
        {
            int nombreRol = await _context.MG_ROLES.CountAsync(x => x.NOMBRE.ToUpper() == NOMBRE.ToUpper());

            if (nombreRol >= 1)
            {
                return new JsonResult("nombreRol");
            }

            return new JsonResult("false");
        }

        [HttpPost]
        public async Task<IActionResult> VerificarNombreRol(string? NOMBRE_ROL)
        {
            int nombreRol = await _context.MG_ROLES_DEL_SISTEMA.CountAsync(x => x.NOMBRE_ROL.ToUpper() == NOMBRE_ROL.ToUpper());

            if (nombreRol >= 1)
            {
                return new JsonResult("nombreRol");
            }

            return new JsonResult("false");
        }

        [HttpPost]
        public async Task<IActionResult> GuardarMenuRol(string NOMBRE_ROL, string ACCESOS)
        {
            int maxRoles = await _context.MG_ROLES
                    .MaxAsync(a => a.CODIGO_ROL) ?? 0;

            Mg_roles rol = new()
            {
                CODIGO_APLICACION = "SIA",
                CODIGO_ROL = maxRoles + 1,
                CODIGO_ESTADO = 1,
                NOMBRE = NOMBRE_ROL.ToUpper(),
                CREADO_POR = HttpContext.Session.GetString("user"),
                FECHA_CREACION = DateTime.Now
            };

            _context.Add(rol);

            try
            {
                //Guardamos el rol
                await _context.SaveChangesAsync();

                var lstMenus = JsonConvert.DeserializeObject<Mg_menus_segun_rol[]>(ACCESOS);

                foreach (var item in lstMenus)
                {
                    Mg_menus_segun_rol menusRol = new()
                    {
                        CODIGO_ROL = maxRoles + 1,
                        CODIGO_MENU = item.CODIGO_MENU,
                        CODIGO_APLICACION = "SIA",
                        CODIGO_ESTADO = 1,
                        CREADO_POR = HttpContext.Session.GetString("user"),
                        FECHA_CREACION = DateTime.Now,
                    };
                    _context.Add(menusRol);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _context.Remove(rol);

                await _context.MG_MENUS_SEGUN_ROL
                        .Where(x => x.CODIGO_ROL == maxRoles + 1)
                        .ExecuteDeleteAsync();

                await _context.SaveChangesAsync();

                Console.WriteLine(ex.Message);
                return new JsonResult("error");
            }

            return new JsonResult("agregado");
        }

        [HttpPost]
        public async Task<IActionResult> GuardarRolEditado(string NOMBRE_ROL, string ACCESOS, int CODIGO_ROL)
        {
            var rol = await _context.MG_ROLES.FirstOrDefaultAsync(x => x.CODIGO_ROL == CODIGO_ROL);
            rol.NOMBRE = NOMBRE_ROL.ToUpper();
            rol.MODIFICADO_POR = HttpContext.Session.GetString("user");
            rol.FECHA_MODIFICACION = DateTime.Now;

            _context.Entry(rol).CurrentValues.SetValues(rol);
            try
            {
                //Guardamos el rol editado
                await _context.SaveChangesAsync();

                //Eliminamos los roles
                await _context.MG_MENUS_SEGUN_ROL
                        .Where(x => x.CODIGO_ROL == CODIGO_ROL)
                        .ExecuteDeleteAsync();

                var lstMenus = JsonConvert.DeserializeObject<Mg_menus_segun_rol[]>(ACCESOS);

                foreach (var item in lstMenus)
                {
                    Mg_menus_segun_rol menusRol = new()
                    {
                        CODIGO_ROL = CODIGO_ROL,
                        CODIGO_MENU = item.CODIGO_MENU,
                        CODIGO_APLICACION = "SIA",
                        CODIGO_ESTADO = 1,
                        CREADO_POR = HttpContext.Session.GetString("user"),
                        FECHA_CREACION = DateTime.Now,
                    };
                    _context.Add(menusRol);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("agregado");
        }

        [HttpPost]
        public async Task<IActionResult> VerificarCodigoYNombreRol(string? CODIGO_ROL, string? NOMBRE_ROL)
        {
            //int codigoRol = await _context.MG_ROLES_DEL_SISTEMA.CountAsync(x => x.CODIGO_ROL.ToUpper() == CODIGO_ROL.ToUpper());
            int nombreRol = await _context.MG_ROLES_DEL_SISTEMA.CountAsync(x => x.NOMBRE_ROL.ToUpper() == NOMBRE_ROL.ToUpper());

            //if (codigoRol >= 1)
            //{
            //    return new JsonResult("codigoRol");
            //}
            //else 
            if (nombreRol >= 1)
            {
                return new JsonResult("nombreRol");
            }

            return new JsonResult("false");
        }

        [HttpPost]
        public async Task<IActionResult> Guardar_Rol_Nuevo(string NOMBRE_ROL, string ACCESOS, int CODIGO_ROL)
        {
            int maxRoles = await _context.MG_ROLES
                    .MaxAsync(a => a.CODIGO_ROL) ?? 0;

            Mg_roles rol = new()
            {
                CODIGO_APLICACION = "SIA",
                CODIGO_ROL = maxRoles + 1,
                CODIGO_ESTADO = 1,
                NOMBRE = NOMBRE_ROL.ToUpper(),
                CREADO_POR = HttpContext.Session.GetString("user"),
                FECHA_CREACION = DateTime.Now
            };

            _context.Add(rol);

            try
            {
                //Guardamos el rol
                await _context.SaveChangesAsync();

                var lstSubmenus = JsonConvert.DeserializeObject<Mg_permisos_submenus[]>(ACCESOS);

                foreach (var item in lstSubmenus)
                {
                    Mg_permisos_submenus submenus = new()
                    {
                        CODIGO_ROL = maxRoles + 1,
                        CODIGO_OPCION = item.CODIGO_SUB_MENU,
                        CODIGO_APLICACION = "SIA",
                        USUARIO_ADICIONA = HttpContext.Session.GetString("user"),
                        FECHA_ADICIONA = DateTime.Now,
                        LECTURA = item.LECTURA,
                        CREAR = item.CREAR,
                        MODIFICAR = item.MODIFICAR,
                        AUTORIZAR = item.AUTORIZAR,
                        ELIMINAR = item.ELIMINAR
                    };
                    _context.Add(submenus);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _context.Remove(rol);

                await _context.MG_PERMISOS_SUBMENUS
                        .Where(x => x.CODIGO_ROL == maxRoles + 1)
                        .ExecuteDeleteAsync();

                await _context.SaveChangesAsync();

                Console.WriteLine(ex.Message);
                return new JsonResult("error");
            }

            return new JsonResult("agregado");
        }

        [HttpPost]
        public async Task<IActionResult> Guardar_Rol_Editado(string NOMBRE_ROL, string ACCESOS, int CODIGO_ROL)
        {
            var rol = await _context.MG_ROLES.FirstOrDefaultAsync(x => x.CODIGO_ROL == CODIGO_ROL);
            rol.NOMBRE = NOMBRE_ROL.ToUpper();
            rol.CREADO_POR = HttpContext.Session.GetString("user");
            rol.FECHA_CREACION = DateTime.Now;

            _context.Entry(rol).CurrentValues.SetValues(rol);
            try
            {
                //Guardamos el rol editado
                await _context.SaveChangesAsync();

                var rolSubmenu = await _context.MG_PERMISOS_SUBMENUS.AsNoTracking().FirstOrDefaultAsync(x => x.CODIGO_ROL == CODIGO_ROL);

                //Eliminamos los roles
                await _context.MG_PERMISOS_SUBMENUS
                        .Where(x => x.CODIGO_ROL == CODIGO_ROL)
                        .ExecuteDeleteAsync();

                var lstSubmenus = JsonConvert.DeserializeObject<Mg_permisos_submenus[]>(ACCESOS);

                foreach (var item in lstSubmenus)
                {

                    Mg_permisos_submenus submenus = new()
                    {
                        CODIGO_ROL = CODIGO_ROL,
                        CODIGO_OPCION = item.CODIGO_OPCION,
                        CODIGO_APLICACION = "SIA",
                        USUARIO_ADICIONA = rolSubmenu == null ? HttpContext.Session.GetString("user") : rolSubmenu.USUARIO_ADICIONA,
                        FECHA_ADICIONA = rolSubmenu == null ? DateTime.Now : rolSubmenu.FECHA_ADICIONA,
                        USUARIO_MODIFICA = HttpContext.Session.GetString("user"),
                        FECHA_MODIFICA = DateTime.Now,
                        LECTURA = item.LECTURA,
                        CREAR = item.CREAR,
                        MODIFICAR = item.MODIFICAR,
                        AUTORIZAR = item.AUTORIZAR,
                        ELIMINAR = item.ELIMINAR
                    };
                    _context.Add(submenus);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("error");
            }

            return new JsonResult("agregado");
        }
    }
}