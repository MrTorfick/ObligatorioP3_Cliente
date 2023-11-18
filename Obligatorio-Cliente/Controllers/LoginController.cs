using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Obligatorio_Cliente.Models;
using System.Text;

namespace Obligatorio_Cliente.Controllers
{
    public class LoginController : Controller
    {
        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5085/api/Login/login";

        public LoginController()
        {
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UsuarioModel user)
        {
            Uri uri = new Uri(url);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Post, uri);

            string json = JsonConvert.SerializeObject(user);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            solicitud.Content = contenido;
            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                UsuarioModel usuario = JsonConvert.DeserializeObject<UsuarioModel>(response.Result);
                HttpContext.Session.SetString("token", usuario.token);
                HttpContext.Session.SetString("usuario", usuario.nombre);
                return RedirectToAction("Index", "Equipo");
            }
            return RedirectToAction("Login");
        }
    }
}
