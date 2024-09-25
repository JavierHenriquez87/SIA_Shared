using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_Hallazgos
    {
        [Key]
        public int CODIGO_HALLAZGO { get; set; }
        public string? HALLAZGO { get; set; }
        public int? CALIFICACION { get; set; }
        public int? VALOR_MUESTRA { get; set; }
        public int? MUESTRA_INCONSISTENTE { get; set; }
        public double? DESVIACION_MUESTRA { get; set; }
        public int? NIVEL_RIESGO { get; set; }
        public string? CONDICION { get; set; }
        public string? CRITERIO { get; set; }
        public int CODIGO_ACTIVIDAD { get; set; }
        public int NUMERO_PDT { get; set; }
        public int NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int ANIO_AI { get; set; }
        public int NUMERO_AUDITORIA { get; set; }
        public DateTime FECHA_CREACION { get; set; }
        public string CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        public ICollection<Mg_hallazgos_detalles> Detalles { get; set; }
        public ICollection<Mg_hallazgos_documentos> Documentos { get; set; }
    }
}
