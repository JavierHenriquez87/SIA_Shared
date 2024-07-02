using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Au_auditores_asignados
    {
        public string? CODIGO_USUARIO { get; set; }
        public int? NUMERO_MDP { get; set; }
        public int? NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int? ANIO_AI { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }

        [ForeignKey("CODIGO_USUARIO")]
        public Mg_usuarios mg_usuarios { get; set; }
    }
}
