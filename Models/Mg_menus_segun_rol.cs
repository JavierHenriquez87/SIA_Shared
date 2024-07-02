using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_menus_segun_rol
    {
        public string? CODIGO_ROL { get; set; }
        public int? CODIGO_MENU { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }

        [ForeignKey("CODIGO_MENU")]
        public Mg_menus mg_Menus { get; set; }

    }
}
