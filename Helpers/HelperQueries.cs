namespace SIA.Helpers
{
    public class HelperQueries
    {
        // RETORNAMOS EL CODIGO DE LA AGENCIA QUE SE ENCUENTRA LOGUEADO 
        public static int CodigoAgenciaLogueado(IHttpContextAccessor contextAccessor)
        {
            var agencyCode = contextAccessor.HttpContext.Session.GetInt32("agencyCode");

            agencyCode = agencyCode == 100 || agencyCode == null ? 0 : agencyCode;

            return agencyCode ?? 0;
        }
    }
}
