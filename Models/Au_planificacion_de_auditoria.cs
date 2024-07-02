using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace SIA.Models
{
    public class Au_planificacion_de_auditoria
    {
        public int NUMERO_MDP { get; set; }
        public int NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public string? CODIGO_MEMORANDUM { get; set; }
        public string? OBJETIVO { get; set; }
        public string? RECURSOS { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_SOLICITUD_APROBACION { get; set; }
        public string? SOLICITADO_POR { get; set; }
        public DateTime? FECHA_APROBACION { get; set; }
        public string? APROBADO_POR { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_ACTUALIZACION { get; set; }
        public string? ACTUALIZADO_POR { get; set; }
        public int? ANIO_MDP { get; set; }
        public string? TEXTO_TIPO_AUDITORIA { get; set; }
        public string? TEXTO_EQUIPO_TRABAJO { get; set; }
        public string? TEXTO_TIEMPO_AUDITORIA { get; set; }
    }
}
