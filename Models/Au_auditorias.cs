using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Au_auditorias
    {
        public int? NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int? NUMERO_AUDITORIA { get; set; }
        public string? CODIGO_TIPO_AUDITORIA { get; set; }
        public string? CODIGO_AUDITORIA { get; set; }
        public string? NOMBRE_AUDITORIA { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        public string? ENCARGADO_AUDITORIA { get; set; }
        public int? ANIO_AE { get; set; }


        [ForeignKey("CODIGO_TIPO_AUDITORIA")]
        public Mg_tipos_de_auditorias mg_tipos_de_auditorias { get; set; }
    }
}
