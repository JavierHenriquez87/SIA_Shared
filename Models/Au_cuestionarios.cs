using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Au_cuestionarios
    {
        [Key]
        public int CODIGO_CUESTIONARIO { get; set; }
        public string? NOMBRE_CUESTIONARIO { get; set; }
        public int? TIPO_CUESTIONARIO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        public int? ESTADO { get; set; }
        public string? CODIGO_TIPO_AUDITORIA { get; set; }

        [NotMapped]
        public bool EXISTE_RESPUESTAS { get; set; }
    }
}
