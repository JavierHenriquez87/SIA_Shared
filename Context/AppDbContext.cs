using Microsoft.EntityFrameworkCore;
using SIA.Models;

namespace SIA.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Mg_usuarios_segun_app>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_USUARIO });
            modelBuilder.Entity<Mg_menus>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_MENU });
            modelBuilder.Entity<Mg_sub_menus>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_MENU, e.CODIGO_SUB_MENU });
            modelBuilder.Entity<Mg_menus_segun_rol>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_MENU, e.CODIGO_ROL });
            modelBuilder.Entity<Mg_permisos_submenus>().HasKey(p => new { p.CODIGO_APLICACION, p.CODIGO_OPCION, p.CODIGO_ROL });
            modelBuilder.Entity<Au_auditorias_integrales>().HasKey(e => new { e.NUMERO_AUDITORIA_INTEGRAL, e.ANIO_AI });
            modelBuilder.Entity<Au_auditorias>().HasKey(e => new { e.NUMERO_AUDITORIA_INTEGRAL, e.NUMERO_AUDITORIA });
            modelBuilder.Entity<Au_planificacion_de_auditoria>().HasKey(e => new { e.NUMERO_MDP, e.NUMERO_AUDITORIA_INTEGRAL });
            modelBuilder.Entity<Au_auditores_asignados>().HasKey(e => new { e.CODIGO_USUARIO, e.NUMERO_MDP, e.NUMERO_AUDITORIA_INTEGRAL });
            modelBuilder.Entity<Au_Planes_De_Trabajo>().HasKey(e => new { e.NUMERO_PDT, e.NUMERO_AUDITORIA_INTEGRAL, e.NUMERO_AUDITORIA });
            modelBuilder.Entity<Au_detalle_plan_de_trabajo>().HasKey(e => new { e.CODIGO_ACTIVIDAD, e.NUMERO_PDT, e.NUMERO_AUDITORIA_INTEGRAL, e.NUMERO_AUDITORIA, e.ANIO_AI, e.CODIGO_USUARIO_ASIGNADO });
            modelBuilder.Entity<Mg_roles>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_ROL });
            modelBuilder.Entity<Mg_hallazgos_detalles>().HasKey(e => new { e.CODIGO_HALLAZGO, e.TIPO });
            modelBuilder.Entity<Mg_cuestionario_secciones>().HasKey(e => new { e.CODIGO_CUESTIONARIO, e.CODIGO_SECCION });
            modelBuilder.Entity<Mg_coment_auditado>().HasKey(e => new { e.CODIGO_COMENT_AUDITADO });

            modelBuilder.Entity<Au_auditorias>()
            .HasOne(h => h.AuditoriaIntegral)
            .WithMany(ai => ai.listado_auditorias)
            .HasForeignKey(h => new { h.NUMERO_AUDITORIA_INTEGRAL, h.ANIO_AE })
            .HasPrincipalKey(ai => new { ai.NUMERO_AUDITORIA_INTEGRAL, ai.ANIO_AI });

            modelBuilder.Entity<Au_Planes_De_Trabajo>()
            .HasOne(h => h.auditoria)
            .WithMany(p => p.listado_planes_trabajo)
            .HasForeignKey(h => new { h.NUMERO_AUDITORIA_INTEGRAL, h.NUMERO_AUDITORIA, h.ANIO_AUDITORIA })
            .HasPrincipalKey(p => new { p.NUMERO_AUDITORIA_INTEGRAL, p.NUMERO_AUDITORIA, p.ANIO_AE });

            modelBuilder.Entity<Au_detalle_plan_de_trabajo>()
            .HasOne(h => h.plan_trabajo)
            .WithMany(d => d.listado_detalles_plan_trabajo)
            .HasForeignKey(h => new { h.NUMERO_AUDITORIA_INTEGRAL, h.NUMERO_PDT, h.NUMERO_AUDITORIA, h.ANIO_AI })
            .HasPrincipalKey(d => new { d.NUMERO_AUDITORIA_INTEGRAL, d.NUMERO_PDT, d.NUMERO_AUDITORIA, d.ANIO_AUDITORIA });

            modelBuilder.Entity<Mg_Hallazgos>()
            .HasOne(h => h.detalle_plan_trabajo)
            .WithMany(ai => ai.listado_hallazgos)
            .HasForeignKey(h => new { h.NUMERO_AUDITORIA_INTEGRAL, h.NUMERO_PDT, h.CODIGO_ACTIVIDAD, h.NUMERO_AUDITORIA, h.ANIO_AI })
            .HasPrincipalKey(d => new { d.NUMERO_AUDITORIA_INTEGRAL, d.NUMERO_PDT, d.CODIGO_ACTIVIDAD, d.NUMERO_AUDITORIA, d.ANIO_AI });

            modelBuilder.Entity<Mg_Hallazgos>()
            .HasMany(h => h.Detalles)
            .WithOne(d => d.Hallazgo)
            .HasForeignKey(d => d.CODIGO_HALLAZGO);

            modelBuilder.Entity<Mg_Hallazgos>()
            .HasMany(h => h.Documentos)
            .WithOne(d => d.Hallazgo)
            .HasForeignKey(d => d.CODIGO_HALLAZGO);

            modelBuilder.Entity<Mg_Hallazgos>()
            .HasMany(h => h.OrientacionCalificacion)
            .WithOne(d => d.Hallazgo)
            .HasForeignKey(d => d.NIVEL_RIESGO)
            .HasPrincipalKey(h => h.NIVEL_RIESGO);

            modelBuilder.Entity<Mg_secciones>()
            .HasMany(s => s.sub_secciones)
            .WithOne(sub => sub.Seccion)
            .HasForeignKey(sub => sub.CODIGO_SECCION );

            modelBuilder.Entity<Mg_sub_secciones>()
            .HasMany(s => s.Preguntas_Cuestionarios)
            .WithOne(sub => sub.Sub_secciones)
            .HasForeignKey(sub => sub.CODIGO_SUB_SECCION);

            modelBuilder.Entity<Mg_menus>()
            .HasMany(m => m.Mg_submenu)
            .WithOne()
            .HasForeignKey(o => new { o.CODIGO_APLICACION, o.CODIGO_MENU });

            modelBuilder.Entity<Mg_menus_segun_rol>()
            .HasOne(m => m.Menu)
            .WithMany()
            .HasForeignKey(m => new { m.CODIGO_APLICACION, m.CODIGO_MENU });

            modelBuilder.Entity<Mg_coment_auditado>()
            .HasMany(m => m.Mg_docs_auditado)
            .WithOne()
            .HasForeignKey(o => new { o.CODIGO_COMENT_AUDITADO });

            modelBuilder.Entity<Mg_Hallazgos>()
            .HasOne(h => h.comentarioAuditado)
            .WithMany()
            .HasForeignKey(h => new { h.CODIGO_HALLAZGO })
            .HasPrincipalKey(d => new { d.CODIGO_HALLAZGO });
        }

        public virtual DbSet<Mg_usuarios_segun_app> MG_USUARIOS_SEGUN_APP { get; set; }
        public virtual DbSet<Mg_usuarios> MG_USUARIOS { get; set; }
        public virtual DbSet<Mg_agencias> MG_AGENCIAS { get; set; }
        public virtual DbSet<Mg_roles_del_sistema> MG_ROLES_DEL_SISTEMA { get; set; }
        public virtual DbSet<Mg_cargos> MG_CARGOS { get; set; }
        public virtual DbSet<Mg_menus> MG_MENUS { get; set; }
        public virtual DbSet<Mg_sub_menus> MG_SUB_MENUS { get; set; }
        public virtual DbSet<Mg_menus_segun_rol> MG_MENUS_SEGUN_ROL { get; set; }
        public virtual DbSet<Mg_permisos_submenus> MG_PERMISOS_SUBMENUS { get; set; }
        public virtual DbSet<Ag_auditorias> AG_AUDITORIAS { get; set; }
        public virtual DbSet<Mg_catalogo_de_auditorias> MG_CATALOGO_DE_AUDITORIAS { get; set; }
        public virtual DbSet<Mg_universo_auditable> MG_UNIVERSO_AUDITABLE { get; set; }
        public virtual DbSet<Au_auditorias_integrales> AU_AUDITORIAS_INTEGRALES { get; set; }
        public virtual DbSet<Mg_tipos_de_auditorias> MG_TIPOS_DE_AUDITORIAS { get; set; }
        public virtual DbSet<Au_auditorias> AU_AUDITORIAS { get; set; }
        public virtual DbSet<Au_planificacion_de_auditoria> AU_PLANIFICACION_DE_AUDITORIA { get; set; }
        public virtual DbSet<Au_auditores_asignados> AU_AUDITORES_ASIGNADOS { get; set; }
        public virtual DbSet<Au_comentarios_mdp> AU_COMENTARIOS_MDP { get; set; }
        public virtual DbSet<Au_Planes_De_Trabajo> AU_PLANES_DE_TRABAJO { get; set; }
        public virtual DbSet<Au_tipo_auditoria> AU_TIPO_AUDITORIA { get; set; }
        public virtual DbSet<Mg_secciones> MG_SECCIONES { get; set; }
        public virtual DbSet<Mg_sub_secciones> MG_SUB_SECCIONES { get; set; }
        public virtual DbSet<Au_cuestionarios> AU_CUESTIONARIOS { get; set; }
        public virtual DbSet<Mg_preguntas_cuestionario> MG_PREGUNTAS_CUESTIONARIO { get; set; }
        public virtual DbSet<Mg_respuestas_cuestionario> MG_RESPUESTAS_CUESTIONARIO { get; set; }
        public virtual DbSet<Mg_tipo_cuestionario> MG_TIPO_CUESTIONARIO { get; set; }
        public virtual DbSet<Mg_auditorias_cuestionarios> MG_AUDITORIAS_CUESTIONARIOS { get; set; }
        public virtual DbSet<Au_detalle_plan_de_trabajo> AU_DETALLE_PLAN_DE_TRABAJO { get; set; }
        public virtual DbSet<Mg_actividades> MG_ACTIVIDADES { get; set; }
        public virtual DbSet<Mg_Hallazgos> MG_HALLAZGOS { get; set; }
        public virtual DbSet<Mg_roles> MG_ROLES { get; set; }
        public virtual DbSet<Mg_hallazgos_detalles> MG_HALLAZGOS_DETALLES { get; set; }
        public virtual DbSet<Mg_hallazgos_documentos> MG_HALLAZGOS_DOCUMENTOS { get; set; }
        public virtual DbSet<Mg_orientacion_calificacion> MG_ORIENTACION_CALIFICACION { get; set; }
        public virtual DbSet<Mg_objetivos_internos> MG_OBJETIVOS_INTERNOS { get; set; }
        public virtual DbSet<Mg_cuestionario_secciones> MG_CUESTIONARIO_SECCIONES { get; set; }
        public virtual DbSet<Mg_cartas> MG_CARTAS { get; set; }
        public virtual DbSet<Au_txt_infor_prelim> AU_TXT_INFOR_PRELIM { get; set; }
        public virtual DbSet<Mg_secc_inf_preli> MG_SECC_INF_PRELI { get; set; }
        public virtual DbSet<Porcentaje_SubSecciones> Porcentaje_SubSecciones { get; set; }
        public virtual DbSet<Mg_docs_auditado> MG_DOCS_AUDITADO { get; set; }
        public virtual DbSet<Mg_coment_auditado> MG_COMENT_AUDITADO { get; set; }
    }
}
