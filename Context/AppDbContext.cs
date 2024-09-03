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
            modelBuilder.Entity<Mg_menus>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_MENU });
            modelBuilder.Entity<Mg_opciones>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_MENU, e.CODIGO_OPCION });
            modelBuilder.Entity<Mg_menus_segun_rol>().HasKey(e => new { e.CODIGO_APLICACION, e.CODIGO_MENU, e.CODIGO_ROL, });
            modelBuilder.Entity<Mg_permisos_submenus>().HasKey(e => new { e.CODIGO_ROL, e.CODIGO_SUB_MENU });
            modelBuilder.Entity<Au_auditorias_integrales>().HasKey(e => new { e.NUMERO_AUDITORIA_INTEGRAL, e.ANIO_AI });
            modelBuilder.Entity<Au_auditorias>().HasKey(e => new { e.NUMERO_AUDITORIA_INTEGRAL, e.NUMERO_AUDITORIA });
            modelBuilder.Entity<Au_planificacion_de_auditoria>().HasKey(e => new { e.NUMERO_MDP, e.NUMERO_AUDITORIA_INTEGRAL });
            modelBuilder.Entity<Au_auditores_asignados>().HasKey(e => new { e.CODIGO_USUARIO, e.NUMERO_MDP, e.NUMERO_AUDITORIA_INTEGRAL });
            modelBuilder.Entity<Au_Planes_De_Trabajo>().HasKey(e => new { e.NUMERO_PDT, e.NUMERO_AUDITORIA_INTEGRAL, e.NUMERO_AUDITORIA });
            modelBuilder.Entity<Au_detalle_plan_de_trabajo>().HasKey(e => new { e.CODIGO_ACTIVIDAD, e.NUMERO_PDT, e.NUMERO_AUDITORIA_INTEGRAL, e.NUMERO_AUDITORIA, e.ANIO_AI, e.CODIGO_USUARIO_ASIGNADO });

            modelBuilder.Entity<Mg_secciones>()
            .HasMany(s => s.sub_secciones)
            .WithOne(sub => sub.Seccion)
            .HasForeignKey(sub => sub.CODIGO_SECCION);

            modelBuilder.Entity<Mg_sub_secciones>()
            .HasMany(s => s.Preguntas_Cuestionarios)
            .WithOne(sub => sub.Sub_secciones)
            .HasForeignKey(sub => sub.CODIGO_SUB_SECCION);
        }

        public virtual DbSet<Mg_usuarios> MG_USUARIOS { get; set; }
        public virtual DbSet<Mg_agencias> MG_AGENCIAS { get; set; }
        public virtual DbSet<Mg_roles_del_sistema> MG_ROLES_DEL_SISTEMA { get; set; }
        public virtual DbSet<Mg_cargos> MG_CARGOS { get; set; }
        public virtual DbSet<Mg_menus> MG_MENUS { get; set; }
        public virtual DbSet<Mg_opciones> MG_OPCIONES { get; set; }
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
        
    }
}
