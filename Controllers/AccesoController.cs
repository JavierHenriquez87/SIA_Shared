﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIA.Models;
using SIA.Context;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SIA.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;

        public AccesoController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;
        }

        /// <summary>
        /// Metodo inicial cuando el usuario quiere ingresar al sistema
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            return View();
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
                string hashString = Password_Login; //Convert.ToBase64String(hashBytes);
                //Validamos si existe un usuario con las credenciales enviadas
                var userRolApp = await _context.MG_USUARIOS
                                    .Where(user => user.CODIGO_USUARIO == User_Login)
                                    .Where(user => user.CLAVE_USUARIO == hashString)
                                    .Where(user => user.ESTADO == 1)
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
                    HttpContext.Session.SetString("userName", userRolApp.NOMBRE_USUARIO);
                    HttpContext.Session.SetString("agencyCode", userRolApp.CODIGO_AGENCIA.ToString());
                    HttpContext.Session.SetString("rolCode", userRolApp.CODIGO_ROL);

                    //Obtenemos los menus y submenus a los que el usuario tiene acceso segun el rol
                    List<Mg_menus_segun_rol> menu = await _context.MG_MENUS_SEGUN_ROL
                        .Where(x => x.CODIGO_ROL == userRolApp.CODIGO_ROL)
                        .Include(x => x.mg_Menus)
                        .ThenInclude(m => m.Mg_sub_menus)
                        .OrderBy(e => e.mg_Menus.ORDEN)
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
        public void Logout()
        {
            HttpContext.Session.Clear();//Limpiar la sesión
            HttpContext.Session.SetString("authenticated", "false");
        }
    }
}