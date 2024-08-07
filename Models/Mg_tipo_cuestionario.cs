using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_tipo_cuestionario
    {
        [Key]
        public int CODIGO_TIPO_CUESTIONARIO { get; set; }
        public string? NOMBRE_TIPO_CUESTIONARIO { get; set; }
    }
}
