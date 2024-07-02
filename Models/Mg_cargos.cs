using System.ComponentModel.DataAnnotations;

namespace SIA.Models
{
    public class Mg_cargos
    {
        [Key]
        public string CODIGO_CARGO { get; set; }
        public string? DESCRIPCION { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string? USUARIO_MODIFICA { get; set; }
    }
}
