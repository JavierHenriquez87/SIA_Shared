using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_opciones
    {
        public string CODIGO_APLICACION { get; set; }
        public int CODIGO_MENU { get; set; }
        public int CODIGO_OPCION { get; set; }
        public string? NOMBRE { get; set; }
        public string? ESTADO { get; set; }
        public string? URL { get; set; }
        public int? ORDEN { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        [ForeignKey("CODIGO_MENU")]
        public Mg_menus mg_Menus { get; set; }

    }
}
