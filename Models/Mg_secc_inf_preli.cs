using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_secc_inf_preli
    {
        [Key]
        public int CODIGO_SEC_INF { get; set; }
        public string TITULO { get; set; }
        [Column(TypeName = "CLOB")]
        public string TEXTO_SECCION { get; set; }
        public int MOSTRAR_SECCION { get; set; }
        public int NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int ANIO_AI { get; set; }
    }
}
