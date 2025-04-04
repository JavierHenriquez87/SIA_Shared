﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIA.Models
{
    public class Mg_menus_segun_rol
    {
        public string? CODIGO_APLICACION { get; set; }
        public int? CODIGO_MENU { get; set; }
        public int? CODIGO_ROL { get; set; }
        public int? CODIGO_ESTADO { get; set; }
        public DateTime? FECHA_CREACION { get; set; }
        public string? CREADO_POR { get; set; }
        public DateTime? FECHA_MODIFICACION { get; set; }
        public string? MODIFICADO_POR { get; set; }

        public virtual Mg_menus Menu { get; set; }
    }
}
