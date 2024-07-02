using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_sub_menus
    {
        [Key]
        public int? CODIGO_SUB_MENU { get; set; }
        public string? NOMBRE { get; set; }
        public string? ESTADO { get; set; }
        public string? URL { get; set; }
        public int? CODIGO_MENU { get; set; }
        public int? ORDEN { get; set; }

        [ForeignKey("CODIGO_MENU")]
        public Mg_menus mg_Menus { get; set; }

    }
}
