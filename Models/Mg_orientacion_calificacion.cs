using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_orientacion_calificacion
    {
        [Key]
        public int ORIENTACION_CALIFICACION { get; set; }
        public string ORIENTACION { get; set; }
        public int NIVEL_RIESGO { get; set; }
        public string ESTADO { get; set; }

        public Mg_Hallazgos Hallazgo { get; set; }
    }
}
