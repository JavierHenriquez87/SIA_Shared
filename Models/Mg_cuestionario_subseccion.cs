using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_cuestionario_subseccion
    {
        public string NOMBRE_CUESTIONARIO { get; set; }
        public List<Mg_sub_secciones> SUB_SECCIONES { get; set; }
    }
}
