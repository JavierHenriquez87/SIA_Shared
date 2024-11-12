using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Au_txt_infor_prelim
    {
        [Key]
        public int CODIGO_TXT_INF_PREL { get; set; }
        public string CODIGO_AUDITORIA { get; set; }
        public int NUMERO_AUDITORIA_INTEGRAL { get; set; }
        public int ANIO_AI { get; set; }
        public string MOTIVO_INFORME { get; set; }
        public string OBJETIVO { get; set; }
        public string ALCANCE { get; set; }
        public int MOSTRAR_CONCLUSION_GENERAL { get; set; }
        public string TEXTO_CONCLUSION_GENERAL { get; set; }
        public string PROCEDIMIENTOS_AUDITORIA { get; set; }
    }
}
