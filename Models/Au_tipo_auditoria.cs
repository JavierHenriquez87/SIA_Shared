using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Au_tipo_auditoria
    {
        [Key]
        public int? CODIGO_TIPO_AUDITORIA { get; set; }
        public string? NOMBRE_TIPO_AUDITORIA { get; set; }
        public string? CODIGO_ESTADO { get; set; }
    }
}
