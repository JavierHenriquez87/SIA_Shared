using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_catalogo_de_auditorias
    {
        [Key]
        public string? CODIGO_AUDITORIA { get; set; }
        public string? DESCRIPCION { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string? USUARIO_MODIFICA { get; set; }
        public string? TIPO_AUDITORIA { get; set; }
        public string? NOMBRE_INFORME_PRELIMINAR { get; set; }
        public string? NOMBRE_INFORME_FINAL { get; set; }
        public string? NOMBRE_PLAN_ACCION { get; set; }
    }
}
