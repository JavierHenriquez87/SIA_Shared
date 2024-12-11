using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Au_auditorias_integrales
    {
        public int NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public string? CODIGO_UNIVERSO_AUDITABLE { get; set; }
        public string? CODIGO_AUDITORIA { get; set; }
        public string? NOMBRE_AUDITORIA { get; set; }
        public DateTime? FECHA_INICIO_VISITA { get; set; }
        public DateTime? FECHA_FIN_VISITA { get; set; }
        public DateTime? PERIODO_INICIO_REVISION { get; set; }
        public DateTime? PERIODO_FIN_REVISION { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }
        public string? HABILITADA { get; set; }
        public string? MDP_EMITIDO { get; set; }
        public DateTime? MDP_FECHA_EMISION { get; set; }
        public string? MDP_EMITIDO_POR { get; set; }
        public string? PDT_EMITIDO { get; set; }
        public DateTime? PDT_FECHA_EMISION { get; set; }
        public string? PDT_EMITIDO_POR { get; set; }
        public string? CI_RECIBIDO { get; set; }
        public DateTime? CI_FECHA_RECIBIDO { get; set; }
        public string? CI_RECIBIDO_POR { get; set; }
        public string? IP_ENVIADO { get; set; }
        public DateTime? IP_FECHA_ENVIADO { get; set; }
        public string? IP_ENVIADO_POR { get; set; }
        public string? IP_RECIBIDO { get; set; }
        public DateTime? IP_FECHA_RECIBIDO { get; set; }
        public string? IP_RECIBIDO_POR { get; set; }
        public string? CS_RECIBIDA { get; set; }
        public DateTime? CS_FECHA_RECIBIDO { get; set; }
        public string? CS_RECIBIDO_POR { get; set; }
        public string? IF_EMITIDO { get; set; }
        public DateTime? IF_FECHA_EMITIDO { get; set; }
        public string? IF_EMITIDO_POR { get; set; }
        public string? IF_RECIBIDO { get; set; }
        public DateTime? IF_FECHA_RECIBIDO { get; set; }
        public string? IF_RECIBIDO_POR { get; set; }
        public string? PA_EMITIDO { get; set; }
        public DateTime? PA_FECHA_EMITIDO { get; set; }
        public string? PA_EMITIDO_POR { get; set; }
        public string? PA_RECIBIDO { get; set; }
        public DateTime? PA_FECHA_RECIBIDO { get; set; }
        public string? PA_RECIBIDO_POR { get; set; }        
        public string? AUDITORIA_ANULADA { get; set; }
        public DateTime? FECHA_ANULACION { get; set; }
        public string? ANULADA_POR { get; set; }
        public int? CODIGO_TIPO_AUDITORIA { get; set; }
        public string? SOLICITADA_POR { get; set; }
        public int ANIO_AI { get; set; }

        /*PROPIEDADES HELPERS*/
        [NotMapped]
        public int? CANTIDAD_AUD_ESPEC { get; set; }

        public List<Mg_Hallazgos> listado_hallazgos { get; set; }
    }
}
