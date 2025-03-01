using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using SIA.Models;
using System.Runtime.InteropServices;

namespace SIA.Helpers
{
    public class LoginHelper : Controller
    {
        public LoginHelper()
        {
        }

        //Evaluamos el hash, guardamos registro y la session
        public async Task LoginInitialAsync(string hash, IConfiguration _config)
        {
            var authenticatedSession = HttpContext.Session.GetString("Authenticated");
            if (hash == "" || hash == null)
            {
                if (authenticatedSession == null || authenticatedSession == "false")
                {
                    Redirect(Url.Action("Logout", "Acceso"));
                }
            }
            else
            {
                var resultApi = await ApiResponseAsync(hash, _config);
                if(resultApi.User != null)
                {
                    HttpContext.Session.SetString("Authenticated", "true");
                    HttpContext.Session.SetString("user", resultApi.User);
                    HttpContext.Session.SetString("userName", resultApi.UserName);
                    HttpContext.Session.SetString("agencyCode", resultApi.AgencyCode.ToString() ?? "");
                    HttpContext.Session.SetString("agencyName", resultApi.AgencyName);
                    HttpContext.Session.SetString("rolCode", "2");
                    HttpContext.Session.SetString("app", "SIA");
                    HttpContext.Session.SetString("hash", hash);
                    HttpContext.Session.SetString("ID", resultApi.Id);
                }
                else
                {
                    Redirect(Url.Action("Logout", "Acceso"));
                }
            }
        }

        //Recibimos el valor que viene de la API
        public async Task<UserInfoApi> ApiResponseAsync(string hash, IConfiguration _config)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                int appId = int.Parse(_config.GetSection("AppsNumber")["AppNumber"]);

                object login = new { hash, appId };
                var json = JsonConvert.SerializeObject(login);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var url = _config.GetSection("ApiURLs")["ApiURLLogin"];
                using var client = new HttpClient(clientHandler);

                var response = await client.PostAsync(url, data);

                var result = await response.Content.ReadAsStringAsync();
                var resultEnd = JsonConvert.DeserializeObject<UserInfoApi>(result);

                return resultEnd;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
