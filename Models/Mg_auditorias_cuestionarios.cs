using System.ComponentModel.DataAnnotations;

namespace SIA.Models
{
    public class Mg_auditorias_cuestionarios
    {
        [Key]
        public int CODIGO_AUDITORIA_CUESTIONARIO { get; set; }
        public string? CODIGO_AUD_CUEST { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public string? APROBADO_POR { get; set; }
        public DateTime? FECHA_APROBACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public int? NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int? ANIO { get; set; }
        public int? CODIGO_CUESTIONARIO { get; set; }
        public int? CORRELATIVO_CUESTIONARIO { get; set; }
    }
}
