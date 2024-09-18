using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_hallazgos_detalles
    {
        public int CODIGO_HALLAZGO { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? TIPO { get; set; }

        public Mg_Hallazgos Hallazgo { get; set; }
    }
}
