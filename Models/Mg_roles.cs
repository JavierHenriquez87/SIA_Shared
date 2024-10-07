using System.ComponentModel.DataAnnotations;

namespace SIA.Models
{
    public class Mg_roles
    {
        public string? CODIGO_APLICACION { get; set; }
        public int? CODIGO_ROL { get; set; }
        public string? NOMBRE { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        public virtual ICollection<Mg_permisos_submenus> PermisosSubmenus { get; set; }
    }
}
