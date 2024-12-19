using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIA.Models
{
    public class MatrizHallazgosResumen
    {
        public string CODIGO_HALLAZGO { get; set; }
        public string CONDICION { get; set; }
        public string RECOMENDACION { get; set; }
        public string ANEXOS { get; set; }
        public string ACCIONES_PREV_CORR { get; set; }
        public string EVIDENCIA { get; set; }
    }
}
