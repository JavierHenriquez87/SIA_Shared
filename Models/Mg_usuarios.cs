using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_usuarios
    {
        [Key]
        public string? CODIGO_USUARIO { get; set; }
        public int? CODIGO_EMPRESA { get; set; }
        public int? CODIGO_AGENCIA { get; set; }
        public string? CODIGO_ROL { get; set; }
        public string? CODIGO_CARGO { get; set; }
        public int? ESTADO { get; set; }
        public string? NUMERO_IDENTIDAD { get; set; }
        public string? NOMBRE_USUARIO { get; set; }
        public string? CLAVE_USUARIO { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string? USUARIO_MODIFICA { get; set; }
        public string? EMAIL { get; set; }
        //[Column(TypeName = "CLOB")]
        public string? FIRMA { get; set; }
        public string? USUARIO { get; set; }
        public int? CODIGO_RESPONSABLE { get; set; }

        [ForeignKey("CODIGO_AGENCIA")]
        public Mg_agencias mg_agencias { get; set; }
    }
}
