using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_objetivos_internos
    {
        [Key]
        public int CODIGO_OBJINT { get; set; }
        public string DESCRIPCION { get; set; }
        public string ESTADO { get; set; }
    }
}
