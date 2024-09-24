using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_hallazgos_documentos
    {
        [Key]
        public int CODIGO_HALLAZGO_DOCUMENTO { get; set; }
        public string NOMBRE_DOCUMENTO { get; set; }
        public int CODIGO_HALLAZGO { get; set; }
        public string CREADO_POR { get; set; }
        public DateTime FECHA_CREACION { get; set; }

    }
}
