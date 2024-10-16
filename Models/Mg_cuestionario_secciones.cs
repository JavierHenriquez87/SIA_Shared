using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_cuestionario_secciones
    {
        [Key]
        public int CODIGO_CUESTIONARIO { get; set; }

        public int? CODIGO_SECCION { get; set; }

        [NotMapped]
        public List<Mg_secciones> Secciones { get; set; }
    }
}
