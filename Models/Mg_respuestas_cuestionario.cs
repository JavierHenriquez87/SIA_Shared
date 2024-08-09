using System.ComponentModel.DataAnnotations;

namespace SIA.Models
{
    public class Mg_respuestas_cuestionario
    {
        [Key]
        public int CODIGO_RESPUESTA { get; set; }
        public int CODIGO_PREGUNTA { get; set; }
        public int? CUMPLE { get; set; }
        public int? NO_CUMPLE { get; set; }
        public int? CUMPLE_PARCIALMENTE { get; set; }
        public int? NO_APLICA { get; set; }
        public string? OBSERVACIONES { get; set; }
        public string? CALIFICACIONES { get; set; }
        public double? PUNTAJE { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        public int CODIGO_AUDITORIA_CUESTIONARIO { get; set; }

    }
}
