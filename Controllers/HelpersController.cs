using Microsoft.AspNetCore.Mvc;
using SIA.Models;
using SIA.Context;
using SIA.Helpers;
using Microsoft.EntityFrameworkCore;

namespace SIA.Controllers
{
    public class HelpersController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HelperQueries _helperQueries;
        private int? _codAgenciaLogueada = 0;

        public HelpersController(AppDbContext context, IConfiguration config, IHttpContextAccessor HttpContextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = HttpContextAccessor;
            _codAgenciaLogueada = HelperQueries.CodigoAgenciaLogueado(_contextAccessor);
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_agencias>>> GetAgencias()
        {
            try
            {
                var listadoAgencias = await _context.MG_AGENCIAS
                    .Where(i => (i.NOMBRE_AGENCIA.Contains("AGENCIA") || i.NOMBRE_AGENCIA.Contains("OFICINA PRINCIPAL")) && i.NOMBRE_AGENCIA != "AGENCIA OPD" && i.NOMBRE_AGENCIA != "AGENCIA COOPERATIVA" && (_codAgenciaLogueada > 0 ? i.CODIGO_AGENCIA == _codAgenciaLogueada : i.CODIGO_AGENCIA > 0))
                    .Select(i => new Mg_agencias
                    {
                        CODIGO_AGENCIA = i.CODIGO_AGENCIA,
                        NOMBRE_AGENCIA = i.NOMBRE_AGENCIA
                    })
                    .OrderBy(i => i.CODIGO_AGENCIA)
                    .ToListAsync();

                if (listadoAgencias == null)
                {
                    return NotFound();
                }

                return listadoAgencias;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_roles>>> GetRoles()
        {
            try
            {
                var listadoRoles = await _context.MG_ROLES.Where(a => a.CODIGO_APLICACION == "SIA").ToListAsync();

                if (listadoRoles == null)
                {
                    return NotFound();
                }

                return listadoRoles;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<Mg_usuarios>> GetAuditorInterno()
        {
            try
            {
                var auditorInterno = await _context.MG_USUARIOS.FirstOrDefaultAsync(X => X.CODIGO_CARGO == "1");

                if (auditorInterno == null)
                {
                    return NotFound();
                }

                return auditorInterno;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_cargos>>> GetCargos()
        {
            try
            {
                var listadoCargos = await _context.MG_CARGOS
                    .ToListAsync();

                if (listadoCargos == null)
                {
                    return NotFound();
                }

                return listadoCargos;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<int>> GetConteoAuditorias()
        {
            try
            {
                var conteoAuditorias = await _context.AG_AUDITORIAS.CountAsync();

                return conteoAuditorias;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_catalogo_de_auditorias>>> GetCatalogoAuditorias()
        {
            try
            {
                var catalogoAuditorias = await _context.MG_CATALOGO_DE_AUDITORIAS
                    .Where(X => X.TIPO_AUDITORIA == "AG")
                    .Select(i => new Mg_catalogo_de_auditorias
                    {
                        CODIGO_AUDITORIA = i.CODIGO_AUDITORIA,
                        DESCRIPCION = i.DESCRIPCION
                    })
                    .OrderBy(i => i.CODIGO_AUDITORIA).ToListAsync();

                if (catalogoAuditorias == null)
                {
                    return NotFound();
                }

                return catalogoAuditorias;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_usuarios>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.MG_USUARIOS
                    .Where(X => X.CODIGO_CARGO != "0")
                    .Select(i => new Mg_usuarios
                    {
                        CODIGO_USUARIO = i.CODIGO_USUARIO,
                        NOMBRE_USUARIO = i.NOMBRE_USUARIO
                    })
                    .OrderBy(i => i.CODIGO_USUARIO).ToListAsync();

                if (usuarios == null)
                {
                    return NotFound();
                }

                return usuarios;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_universo_auditable>>> GetUniversoAuditable()
        {
            try
            {
                var universos = await _context.MG_UNIVERSO_AUDITABLE
                    .Where(X => X.CODIGO_ESTADO == "A")
                    .Select(i => new Mg_universo_auditable
                    {
                        CODIGO_UNIVERSO_AUDITABLE = i.CODIGO_UNIVERSO_AUDITABLE,
                        NOMBRE = i.NOMBRE
                    })
                    .OrderBy(i => i.NOMBRE).ToListAsync();

                if (universos == null)
                {
                    return NotFound();
                }

                return universos;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_tipos_de_auditorias>>> GetTiposAuditoria()
        {
            try
            {
                var tiposAuditorias = await _context.MG_TIPOS_DE_AUDITORIAS
                    .Where(i => i.CODIGO_ESTADO == "A")
                    .OrderBy(i => i.DESCRIPCION)
                    .ToListAsync();

                if (tiposAuditorias == null)
                {
                    return NotFound();
                }

                return tiposAuditorias;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Mg_usuarios_segun_app>>> GetAuditores()
        {
            try
            {
                var Auditores = await _context.MG_USUARIOS_SEGUN_APP
                    .Where(i => i.CODIGO_ESTADO == 1)
                    .Where(i => i.CODIGO_ROL == 2)
                    .Where(i => i.CODIGO_APLICACION == HttpContext.Session.GetString("app"))
                    .OrderBy(i => i.NOMBRE)
                    .ToListAsync();

                if (Auditores == null)
                {
                    return NotFound();
                }

                return Auditores;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Au_tipo_auditoria>>> GetProgramadasNoProgramadas()
        {
            try
            {
                var TiposAuditorias = await _context.AU_TIPO_AUDITORIA
                    .Where(i => i.CODIGO_ESTADO == "A")
                    .OrderBy(i => i.NOMBRE_TIPO_AUDITORIA)
                    .ToListAsync();

                if (TiposAuditorias == null)
                {
                    return NotFound();
                }

                return TiposAuditorias;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Au_auditores_asignados>>> GetAuditoresAsignados()
        {
            try
            {
                int cod = (int)HttpContext.Session.GetInt32("num_auditoria_integral");
                int anio = (int)HttpContext.Session.GetInt32("anio_auditoria_integral");

                var Auditores = await _context.AU_AUDITORES_ASIGNADOS
                    .Include(e => e.mg_usuarios)
                    .Where(e => e.NUMERO_AUDITORIA_INTEGRAL == cod)
                    .Where(e => e.ANIO_AI == anio)
                    .OrderBy(e => e.mg_usuarios.NOMBRE_USUARIO)
                    .ToListAsync();

                if (Auditores == null)
                {
                    return NotFound();
                }

                return Auditores;
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}