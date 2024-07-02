using System.ComponentModel.DataAnnotations;

namespace SIA.Models
{
    public class Mg_agencias
    {
        [Key]
        public int CODIGO_AGENCIA { get; set; }
        public int? CODIGO_EMPRESA { get; set; }
        public string NOMBRE_AGENCIA { get; set; }
        public string? JEFE_AGENCIA { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string USUARIO_MODIFICA { get; set; }
        public string? DIRECCION { get; set; }
        public string ALIAS { get; set; }
        public string? CODIGO_CNBS { get; set; }
        public string TIPO_OFICINA { get; set; }
        public string? AGENCIA { get; set; }
    }
}
