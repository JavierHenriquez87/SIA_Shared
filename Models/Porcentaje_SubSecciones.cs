using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Porcentaje_SubSecciones
    {
        [Key]
        public string SubSeccion { get; set; }
        public string Seccion { get; set; }
        public decimal PorcentajeCumplimiento { get; set; }
    }
}
