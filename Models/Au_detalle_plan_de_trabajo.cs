using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Au_detalle_plan_de_trabajo
    {
        public int CODIGO_ACTIVIDAD { get; set; }
        public int NUMERO_PDT { get; set; }
        public int NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int ANIO_AI { get; set; }
        public int NUMERO_AUDITORIA { get; set; }
        public int CODIGO_ESTADO { get; set; }
        public string CODIGO_USUARIO_ASIGNADO { get; set; }        
        public DateTime FECHA_CREACION { get; set; }
        public string CREADO_POR { get; set; }
        public DateTime? FECHA_ACTUALIZACION { get; set; }
        public string? ACTUALIZADO_POR { get; set; }


        [ForeignKey("CODIGO_ACTIVIDAD")]
        public Mg_actividades mg_actividades { get; set; }

        [ForeignKey("CODIGO_USUARIO_ASIGNADO")]
        public Mg_usuarios mg_usuarios { get; set; }

    }
}
