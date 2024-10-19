using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_cartas
    {
        [Key]
        public int CODIGO_CARTA { get; set; }
        [Column(TypeName = "CLOB")]
        public string TEXTO_CARTA { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public int? NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int? ANIO_AI { get; set; }
        public int? TIPO_CARTA { get; set; }
    }
}
