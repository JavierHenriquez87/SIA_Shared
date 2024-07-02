using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Au_comentarios_mdp
    {
        [Key]
        public int? CODIGO_COMENTARIO { get; set; }
        public int? NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public string? CODIGO_USUARIO { get; set; }
        public string? NOMBRE_USUARIO { get; set; }
        public string? NOTA { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public int? ANIO_AI { get; set; }

        [ForeignKey("CODIGO_USUARIO")]
        public Mg_usuarios mg_usuarios { get; set; }
    }
}
