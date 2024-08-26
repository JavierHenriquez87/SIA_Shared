using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_sub_secciones
    {
        [Key]
        public int CODIGO_SUB_SECCION { get; set; }
        public string? DESCRIPCION { get; set; }
        public int CODIGO_SECCION { get; set; }

        // Propiedad de navegación a Mg_secciones
        public Mg_secciones Seccion { get; set; }

        // Propiedad de navegación para la relación uno a muchos con Mg_sub_secciones
        public ICollection<Mg_preguntas_cuestionario> Preguntas_Cuestionarios { get; set; }

        public Mg_sub_secciones()
        {
            // Inicializar la colección para evitar null references
            Preguntas_Cuestionarios = new List<Mg_preguntas_cuestionario>();
        }

        // Propiedad que no estará en la base de datos
        [NotMapped]
        public double PORCENTAJE { get; set; }
    }
}
