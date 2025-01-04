using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_coment_auditado
    {
        [Key]
        public int CODIGO_COMENT_AUDITADO { get; set; }
        public int CODIGO_HALLAZGO { get; set; }
        public string? COMENTARIO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        public virtual ICollection<Mg_docs_auditado> Mg_docs_auditado { get; set; }
    }
}
