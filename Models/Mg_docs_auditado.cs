using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_docs_auditado
    {
        [Key]
        public int CODIGO_DOC_AUDITADO { get; set; }
        public int CODIGO_COMENT_AUDITADO { get; set; }
        public string? NOMBRE_DOCUMENTO { get; set; }
        public string? PESO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
    }
}
