using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIA.Models
{
    public class Mg_actividades
    {
        [Key]
        public int CODIGO_ACTIVIDAD { get; set; }
        public string CODIGO_PROGRAMA_AUDITORIA { get; set; }
        public string? NOMBRE_ACTIVIDAD { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_ADICIONA { get; set; }
        public string? USUARIO_ADICIONA { get; set; }
        public DateTime? FECHA_MODIFICA { get; set; }
        public string? USUARIO_MODIFICA { get; set; }
        public string? CODIGO_TIPO_AUDITORIA { get; set; }

        // Propiedad de navegación para la relación uno a muchos con au_detalle_plan_trabajo
        public ICollection<Au_detalle_plan_de_trabajo> au_detalle_plan_trabajo { get; set; }

        public Mg_actividades()
        {
            // Inicializar la colección para evitar null references
            au_detalle_plan_trabajo = new List<Au_detalle_plan_de_trabajo>();
        }
    }
}
