using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_tipos_de_auditorias
    {
        [Key]
        public string? CODIGO_TIPO_AUDITORIA { get; set; }
        public string? DESCRIPCION { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public string? CODIGO_ESTADO { get; set; }
    }
}
