using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_firmas_cartas
    {
        [Key]
        public int? CODIGO_FIRMA_CARTA { get; set; }
        public string? CODIGO_CARTA { get; set; }
        public string? FIRMA { get; set; }
        public int? TIPO_CARTA { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string? MODIFICADO_FOR { get; set; }
    }
}
