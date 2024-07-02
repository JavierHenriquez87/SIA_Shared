using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_menus
    {
        [Key]
        public int? CODIGO_MENU { get; set; }
        public string? NOMBRE { get; set; }
        public string? ESTADO { get; set; }
        public string? POSEE_OPCIONES { get; set; }
        public string? ICONO { get; set; }
        public string? URL { get; set; }
        public int? ORDEN { get; set; }

        public ICollection<Mg_sub_menus> Mg_sub_menus { get; set; }
    }
}
