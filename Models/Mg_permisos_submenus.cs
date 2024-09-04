using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace SIA.Models
{
    public class Mg_permisos_submenus
    {
        public string CODIGO_APLICACION { get; set; }
        public int CODIGO_MENU { get; set; }
        public int CODIGO_OPCION { get; set; }
        public string CODIGO_ROL { get; set; }
        public int? LECTURA { get; set; }
        public int? AUTORIZAR { get; set; }
        public int? ELIMINAR { get; set; }
        public int? CREAR { get; set; }
        public int? MODIFICAR { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string? USUARIO_MODIFICA { get; set; }

        //[ForeignKey("CODIGO_APLICACION, CODIGO_ROL")]
        //public virtual Mg_roles roles { get; set; }

        [ForeignKey("CODIGO_APLICACION, CODIGO_MENU, CODIGO_OPCION")]
        public virtual Mg_opciones Submenus { get; set; }
    }
}
