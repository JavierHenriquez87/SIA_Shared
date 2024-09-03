using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_menus
    {
        public string CODIGO_APLICACION { get; set; }
        public int CODIGO_MENU { get; set; }
        public string? NOMBRE { get; set; }
        public string? ESTADO { get; set; }
        public string? POSEE_OPCIONES { get; set; }
        public string? ICONO { get; set; }
        public string? URL { get; set; }
        public int? ORDEN { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        public ICollection<Mg_opciones> Mg_sub_menus { get; set; }
    }
}
