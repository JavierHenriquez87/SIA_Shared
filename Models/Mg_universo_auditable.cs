using System.ComponentModel.DataAnnotations;

namespace SIA.Models
{
    public class Mg_universo_auditable
    {
        [Key]
        public string CODIGO_UNIVERSO_AUDITABLE { get; set; }
        public string? NOMBRE { get; set; }
        public string? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        public Au_auditorias_integrales AuditoriaIntegral { get; set; }
    }
}
