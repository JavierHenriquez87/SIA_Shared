using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIA.Models;
using SIA.Context;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using SIA.Helpers;
using SIA.Services;

namespace SIA.Controllers
{
    public class AccesoController : LoginHelper
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly RabbitMQService _rabbitService;

        public AccesoController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;

            // Inicializar RabbitMQService con los datos de configuración
            _rabbitService = new RabbitMQService(
                hostname: _config["RabbitMQ:HostName"],
                username: _config["RabbitMQ:UserName"],
                password: _config["RabbitMQ:Password"]
            );

            // Iniciar la escucha al crear el controlador
            AddUpdateUserRabbit();
            DisableUserRabbit();
            UpdateNameUserRabbit();
            UnlinkedUserRabbit();
        }

        /// <summary>
        /// Metodo inicial cuando el usuario quiere ingresar al sistema
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Login(string? hash)
        {
            var hashSession = HttpContext.Session.GetString("hash");

            if ((hash == "" || hash == null) && hashSession == null)
            {
                return Redirect(Url.Action("Logout", "Acceso"));
            }

            hash = hashSession != null ? hashSession : hash;

            await LoginInitialAsync(hash, _config);

            string user = HttpContext.Session.GetString("user");
            string userName = HttpContext.Session.GetString("userName");


            try
            {
                //Validamos si existe un usuario con las credenciales enviadas
                Mg_usuarios_segun_app userRolApp = await _context.MG_USUARIOS_SEGUN_APP
                                    .Where(u => u.CODIGO_USUARIO == user && u.CODIGO_APLICACION == "SIA").FirstOrDefaultAsync();

                int? codigoRol = 2;

                if (userRolApp == null)
                {
                    var nuevoUsuario = new Mg_usuarios_segun_app
                    {
                        CODIGO_APLICACION = "SIA",
                        CODIGO_USUARIO = user,
                        NOMBRE = userName,
                        CODIGO_ROL = 2,
                        CODIGO_ESTADO = 1,
                        FECHA_CREACION = DateTime.Now,
                        CREADO_POR = user
                    };

                    // Agregar el nuevo registro al contexto
                    _context.MG_USUARIOS_SEGUN_APP.Add(nuevoUsuario);

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();
                }
                else
                {
                    codigoRol = userRolApp.CODIGO_ROL;
                    HttpContext.Session.SetString("rolCode", userRolApp.CODIGO_ROL.ToString());
                }

                //Obtenemos los menus y submenus a los que el usuario tiene acceso segun el rol
                List<Mg_menus_segun_rol> menu = await _context.MG_MENUS_SEGUN_ROL
                    .Where(x => x.CODIGO_ROL == codigoRol && x.CODIGO_APLICACION == "SIA")
                    .Include(x => x.Menu)
                    .ThenInclude(m => m.Mg_submenu)
                    .OrderBy(e => e.Menu.ORDEN)
                    .ToListAsync();
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true,
                };

                //Guardamos en una variable de sesion el menu y submenus a los que el usuario tiene acceso, mientras este logueado
                HttpContext.Session.SetString("menu", JsonSerializer.Serialize(menu, options));
            }
            catch (Exception ex)
            {
                return Redirect(Url.Action("Logout", "Acceso"));
            }

            return RedirectToAction("Index", "Dashboard");

            //return View();
        }

        /// <summary>
        /// Validación de usuario y clave para ingresar al sistema
        /// </summary>
        /// <param name="User_Login"></param>
        /// <param name="Password_Login"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ValidarLogin(string User_Login, string Password_Login)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Password_Login);
                // Calcular el hash SHA-256
                byte[] hashBytes = SHA256.HashData(bytes);
                // Convertir el hash a una cadena base64
                //string hashString = Password_Login;
                string hashString = Convert.ToBase64String(hashBytes);

                //Validamos si existe un usuario con las credenciales enviadas
                var userRolApp = await _context.MG_USUARIOS_SEGUN_APP
                                    .Where(user => user.CODIGO_USUARIO == User_Login)
                                    //.Where(user => user.CLAVE_ACCESO == hashString)
                                    .Where(user => user.CODIGO_ESTADO == 1)
                                    .FirstOrDefaultAsync();

                if (userRolApp == null)
                {
                    return RedirectToAction("Login", "Acceso");
                }
                else
                {
                    //Creamos variables de sesion con informacion del usuario que acaba de loguearse
                    HttpContext.Session.SetString("authenticated", "true");
                    HttpContext.Session.SetString("user", userRolApp.CODIGO_USUARIO);
                    HttpContext.Session.SetString("userName", userRolApp.NOMBRE);
                    HttpContext.Session.SetString("agencyCode", "" /*userRolApp.CODIGO_AGENCIA.ToString()*/);
                    HttpContext.Session.SetString("rolCode", userRolApp.CODIGO_ROL.ToString());
                    HttpContext.Session.SetString("app", "SIA");

                    //Obtenemos los menus y submenus a los que el usuario tiene acceso segun el rol
                    List<Mg_menus_segun_rol> menu = await _context.MG_MENUS_SEGUN_ROL
                        .Where(x => x.CODIGO_ROL == userRolApp.CODIGO_ROL && x.CODIGO_APLICACION == "SIA")
                        .Include(x => x.Menu)
                        .ThenInclude(m => m.Mg_submenu)
                        .OrderBy(e => e.Menu.ORDEN)
                        .ToListAsync();
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true,
                    };

                    //Guardamos en una variable de sesion el menu y submenus a los que el usuario tiene acceso, mientras este logueado
                    HttpContext.Session.SetString("menu", JsonSerializer.Serialize(menu, options));

                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return RedirectToAction("Login", "Acceso");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// LIMPIAMOS LA SESION
        /// </summary>
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();//Limpiar la sesión
            return Redirect(_config.GetSection("ApiURLs")["LoginURL"]);
        }

        public IActionResult AddUpdateUserRabbit()
        {
            string queueName = "hdh.sso.users.access.added." + _config["AppsNumber:AppNumberDev"];

            // Consumir mensajes y procesarlos
            _rabbitService.ConsumeMessages(queueName, (message) =>
            {
                try
                {
                    // Deserializar el mensaje en un objeto dinámico
                    using var jsonDoc = JsonDocument.Parse(message);
                    var root = jsonDoc.RootElement;

                    // Acceder a las propiedades del JSON
                    string userCode = root.GetProperty("userCode").GetString();
                    string name = root.GetProperty("name").GetString();
                    string agencyName = root.GetProperty("agency").GetProperty("name").GetString();
                    int agencyId = root.GetProperty("agency").GetProperty("id").GetInt32();
                    string id = root.GetProperty("id").GetString();

                    // Verificar si el usuario ya existe en la base de datos
                    var existingUser = _context.MG_USUARIOS_SEGUN_APP
                        .FirstOrDefault(u => u.CODIGO_APLICACION == "SIA" && u.ID == id);

                    if (existingUser != null)
                    {
                        // Si el usuario ya existe, actualizar el estado
                        existingUser.NOMBRE = name;
                        existingUser.CODIGO_ESTADO = 1;
                    }
                    else
                    {
                        // Si el usuario no existe, crear un nuevo registro
                        var newUser = new Mg_usuarios_segun_app
                        {
                            CODIGO_APLICACION = "SIA",
                            CODIGO_USUARIO = userCode,
                            NOMBRE = name,
                            CODIGO_ROL = 2,
                            CODIGO_ESTADO = 1,
                            FECHA_CREACION = DateTime.Now,
                            CREADO_POR = "System"
                        };

                        // Agregar el nuevo registro al contexto
                        _context.MG_USUARIOS_SEGUN_APP.Add(newUser);
                    }

                    // Guardar los cambios en la base de datos
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            });

            return Content("Escuchando mensajes en la cola RabbitMQ...");
        }

        public IActionResult DisableUserRabbit()
        {
            string queueName = "hdh.sso.users.access.removed." + _config["AppsNumber:AppNumberDev"];

            // Consumir mensajes y procesarlos
            _rabbitService.ConsumeMessages(queueName, (message) =>
            {
                try
                {
                    // Deserializar el mensaje en un objeto dinámico
                    using var jsonDoc = JsonDocument.Parse(message);
                    var root = jsonDoc.RootElement;

                    // Acceder a las propiedades del JSON
                    string name = root.GetProperty("name").GetString();
                    string id = root.GetProperty("id").GetString();

                    // Verificar si el usuario ya existe en la base de datos
                    var existingUser = _context.MG_USUARIOS_SEGUN_APP
                        .FirstOrDefault(u => u.CODIGO_APLICACION == "SIA" && u.ID == id);

                    if (existingUser != null)
                    {
                        // Si el usuario ya existe, actualizar el estado
                        existingUser.NOMBRE = name;
                        existingUser.CODIGO_ESTADO = 0;
                    }

                    // Guardar los cambios en la base de datos
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            });

            return Content("Escuchando mensajes en la cola RabbitMQ...");
        }

        public IActionResult UpdateNameUserRabbit()
        {
            string queueName = "hdh.sso.users.access.updated." + _config["AppsNumber:AppNumberDev"];

            // Consumir mensajes y procesarlos
            _rabbitService.ConsumeMessages(queueName, (message) =>
            {
                try
                {
                    // Deserializar el mensaje en un objeto dinámico
                    using var jsonDoc = JsonDocument.Parse(message);
                    var root = jsonDoc.RootElement;

                    // Acceder a las propiedades del JSON
                    string name = root.GetProperty("name").GetString();
                    string id = root.GetProperty("id").GetString();

                    // Verificar si el usuario ya existe en la base de datos
                    var existingUser = _context.MG_USUARIOS_SEGUN_APP
                        .FirstOrDefault(u => u.CODIGO_APLICACION == "SIA" && u.ID == id);

                    if (existingUser != null)
                    {
                        // Si el usuario ya existe, actualizar el estado
                        existingUser.NOMBRE = name;
                    }

                    // Guardar los cambios en la base de datos
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            });

            return Content("Escuchando mensajes en la cola RabbitMQ...");
        }


        public IActionResult UnlinkedUserRabbit()
        {
            string queueName = "hdh.sso.users.access.unlinked." + _config["AppsNumber:AppNumberDev"];

            // Consumir mensajes y procesarlos
            _rabbitService.ConsumeMessages(queueName, (message) =>
            {
                try
                {
                    // Deserializar el mensaje en un objeto dinámico
                    using var jsonDoc = JsonDocument.Parse(message);
                    var root = jsonDoc.RootElement;

                    // Acceder a las propiedades del JSON
                    string id = root.GetProperty("id").GetString();

                    // Verificar si el usuario ya existe en la base de datos
                    var existingUser = _context.MG_USUARIOS_SEGUN_APP
                        .FirstOrDefault(u => u.CODIGO_APLICACION == "SIA" && u.ID == id);

                    if (existingUser != null)
                    {
                        // Si el usuario ya existe, actualizar el estado
                        existingUser.CODIGO_ESTADO = 0;
                    }

                    // Guardar los cambios en la base de datos
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            });

            return Content("Escuchando mensajes en la cola RabbitMQ...");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rabbitService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}