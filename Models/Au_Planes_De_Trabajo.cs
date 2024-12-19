namespace SIA.Models
{
    public class Au_Planes_De_Trabajo
    {
        public int? NUMERO_PDT { get; set; }
        public int? NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int? NUMERO_AUDITORIA { get; set; }
        public string? CODIGO_TIPO_AUDITORIA { get; set; }
        public string? CODIGO_PDT { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_SOLICITUD_APROBACION { get; set; }
        public string? SOLICITADO_POR { get; set; }
        public DateTime? FECHA_APROBACION { get; set; }
        public string? APROBADO_POR { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }        
        public DateTime? FECHA_ACTUALIZACION { get; set; }
        public string? ACTUALIZADO_POR { get; set; }
        public int ANIO_AUDITORIA { get; set; }

        public Au_auditorias auditoria { get; set; }

        public ICollection<Au_detalle_plan_de_trabajo> listado_detalles_plan_trabajo { get; set; }
    }
}
