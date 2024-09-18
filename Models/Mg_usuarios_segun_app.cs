using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_usuarios_segun_app
    {
        public string? CODIGO_APLICACION { get; set; }
        public string? CODIGO_USUARIO { get; set; }
        public string? NOMBRE { get; set; }
        public int? CODIGO_ROL { get; set; }
        public int? CODIGO_CARGO { get; set; }
        public string? CLAVE_ACCESO { get; set; }
        public string? EMAIL { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        //[Column(TypeName = "CLOB")]
        public string? FIRMA { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        [NotMapped]
        public bool SELECTED { get; set; }
    }
}
