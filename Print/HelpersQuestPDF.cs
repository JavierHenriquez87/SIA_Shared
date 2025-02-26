using SIA.Context;

namespace SIA.Print
{
    public class HelpersQuestPDF
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private string _user;

        public HelpersQuestPDF(AppDbContext context, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _config = config;
            _user = httpContextAccessor.HttpContext.Session.GetString("user");
        }
        public string ColorAnaranjadoHtml() => "#EF8C4A"; // RGB(239, 140, 74)
        public string ColorAzulHtml() => "#4472C4"; // RGB(68, 114, 196)
        public string ColorCafeHtml() => "#833C0B"; // RGB(131, 60, 11)
        public string ColorVerdeHtml() => "#007A3D"; // RGB(0, 122, 61)
        public string ColorGrisHtml() => "#494F57"; // RGB(73, 79, 87)
        public string ColorVerdeFondoTablaHtml() => "#14AD4F"; // RGB(20, 173, 79)
        public string ColorSubTituloFondoTablaHtml() => "#DBEEF5"; // RGB(219, 238, 245)
        public string ColorVerdeLimonFondoTablaHtml() => "#92D14F"; // RGB(146, 209, 79)
        public string ColorVerdeClaroFondoTablaHtml() => "#02AD53"; // RGB(2, 173, 83)
        public string ColorGrisBordeTablaHtml() => "#DDDDDD"; // RGB(221, 221, 221)
        public string ColorVerdeBordeTablaHtml() => "#29965F"; // RGB(41, 150, 95)
        public string ColorVerdePorcentajeTablaHtml() => "#02AE50"; // RGB(2, 174, 80)
        public string ColorAmarilloPorcentajeTablaHtml() => "#FEFE01"; // RGB(254, 254, 1)
        public string ColorAnaranjadoPorcentajeTablaHtml() => "#FFBF00"; // RGB(255, 191, 0)
        public string ColorRojoPorcentajeTablaHtml() => "#FC0002"; // RGB(252, 0, 2)
        public string ColorGrisClaroHtml() => "#F3F3F3"; // RGB(243, 243, 243)
        public string ColorGrisOscuroHtml() => "#34383D"; // RGB(52, 56, 61)
        public string ColorBajoTablaHtml() => "#17B26A"; // RGB(23, 178, 106)
        public string ColorMedioTablaHtml() => "#F79009"; // RGB(247, 144, 9)
        public string ColorAltoTablaHtml() => "#F04438"; // RGB(240, 68, 56)
        public string ColorBlancoHtml() => "#FFFFFF"; // RGB(255, 255, 255)

        //COLORES DEL DOCUMENTO
        public string ColorSeccionesPrincipales() => "#007A3D"; // Verde
        public string ColorPrioridadAlto() => "#FF0000"; // Rojo
        public string ColorPrioridadMedio() => "#FC9804"; // Naranja
        public string ColorCriterioEfectoCausa() => "#2F5496"; // Azul
        public string ColorComentarioAuditadoTitulo() => "#007A3D"; // Verde
        public string ColorComentarioAuditadoContenido() => "#0070C0"; // Azul
        public string ColorNegroPrincipal() => "#000000"; // Negro
        public string ColorPrioridadBajo() => "#00B050"; // Verde
        public string ColorCafeTabla() => "#7A5100"; // Café
        public string ColorGrisTabla() => "#F2F2F2"; // Gris
        public string ColorAmarilloTabla() => "#FFFF00"; // Amarillo
    }

}