using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Ag_auditorias
    {
        [Key]
        public int? NUMERO_AUDITORIA { get; set; }
        public string? CODIGO_AUDITORIA { get; set; }
        public string? NUMERO_REGISTRO { get; set; }
        public string? USUARIO_ASIGNADO { get; set; }
        public int? CODIGO_AGENCIA { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public DateTime? PERIODO_INICIO { get; set; }
        public DateTime? PERIODO_FINAL { get; set; }
        public string? AUDITOR_INTERNO { get; set; }
        public string? JEFE_AGENCIA { get; set; }
        public string? MEMORANDUM { get; set; }
        public string? CONCEPTO { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_INICIO_AUDITORIA { get; set; }
        public DateTime? FECHA_FIN_AUDITORIA { get; set; }
        public DateTime? FECHA_CONTESTACION { get; set; }
        public string? URL_EXPEDIENTE { get; set; }
    }
}
