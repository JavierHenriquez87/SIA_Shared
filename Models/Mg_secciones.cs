using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_secciones
    {
        [Key]
        public int CODIGO_SECCION { get; set; } // La clave primaria no necesita ser nullable

        public string? DESCRIPCION_SECCION { get; set; }

        [NotMapped]
        public bool? EXISTE_SUB_SECCION { get; set; }

        // Propiedad de navegación para la relación uno a muchos con Mg_sub_secciones
        public ICollection<Mg_sub_secciones> sub_secciones { get; set; }

        public Mg_secciones()
        {
            // Inicializar la colección para evitar null references
            sub_secciones = new List<Mg_sub_secciones>();
        }
    }
}
