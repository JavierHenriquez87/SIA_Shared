using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_preguntas_cuestionario
    {
        [Key]
        public int CODIGO_PREGUNTA { get; set; }
        public string? DESCRIPCION { get; set; }
        public int CODIGO_SUB_SECCION { get; set; }
        public string? ESTADO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        public int? CODIGO_CUESTIONARIO { get; set; }

        // Propiedad de navegación a Mg_secciones
        public Mg_sub_secciones Sub_secciones { get; set; }
    }
}
