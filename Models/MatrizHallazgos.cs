using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIA.Models
{
    public class MatrizHallazgos
    {
        public string CORRELATIVO { get; set; }
        public string TIPO_AUDITORIA { get; set; }
        public int NUMERO_INFORME { get; set; }
        public DateTime? FECHA_EMISION_INF_FINAL { get; set; }
        public string UNIDAD_AUDITORIA { get; set; }
        public string RESPONSABLE_UNI_AUDITORIA { get; set; }
        public string CODIGO_HALLAZGO { get; set; }
        public string DESCRIPCION { get; set; }
        public int? NIVEL_RIESGO { get; set; }
        public string PROCESO { get; set; }
        public string OBJETIVO_CONTROL_INTERNO { get; set; }
        public string OBJETIVO_ESTRATEGICO { get; set; }
        public string ACCIONES_PREV_CORRE { get; set; }
        public DateTime? FECHA_SOLUCION { get; set; }
        public DateTime? FECHA_SOLUCIONO { get; set; }
        public int DIAS_ATRAZO { get; set; }
        public string RESPONSABLE { get; set; }
        public string EVIDENCIA { get; set; }
        public string UNIDAD_APOYO { get; set; }
        public string ESTATUS { get; set; }
    }
}
