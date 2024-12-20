using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIA.Models
{
    public class ResultadoAuditoria
    {
        public int? CODIGO_CUESTIONARIO { get; set; }
        public string DESCRIPCION { get; set; }

        public List<Porcentaje_SubSecciones> listPorcentajeSubSecciones { get; set; }
    }
}
