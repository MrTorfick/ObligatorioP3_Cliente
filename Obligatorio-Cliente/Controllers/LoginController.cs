using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Obligatorio_Cliente.Models;
using System.Net.Http.Headers;
using System.Text;

namespace Obligatorio_Cliente.Controllers
{
    public class LoginController : Controller
    {
        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5155/api/Login/login";

        public LoginController()
        {
        }



        // GET: UsuarioController/Create
        public ActionResult Create(string mensaje)
        {

            ViewBag.Mensaje = mensaje;
            if (HttpContext.Session.GetString("LogueadoRol") == "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        // POST: UsuarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioModel usuario, string Administrador)
        {
            try
            {

                if (Administrador != null)
                {
                    usuario.EsAdmin = true;
                }
                else
                {
                    usuario.EsAdmin = false;
                }

                AltaUsuario(usuario);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Create), new { mensaje = e.Message });
            }
        }



        public IActionResult Login(string mensaje)
        {
            if (HttpContext.Session.GetString("usuario") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Mensaje = mensaje;
            return View();

        }

        [HttpPost]
        public IActionResult Login(UsuarioModel user)
        {

            try
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
                    if (usuario.EsAdmin)
                    {
                        HttpContext.Session.SetString("LogueadoRol", "admin");
                    }
                    else
                    {
                        HttpContext.Session.SetString("LogueadoRol", "default");
                    }
                }
                else
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    string mensaje = response.Result.ToString();
                    return RedirectToAction("Login", new { mensaje = $"La solicitud no fue exitosa: {codigoDeError}, {mensaje} " });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", new { mensaje = ex.Message });

            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("usuario") != null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }



        private UsuarioModel AltaUsuario(UsuarioModel user)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "Usuario");
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Post, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
            solicitud.Headers.Add("Rol", HttpContext.Session.GetString("LogueadoRol"));
            string json = JsonConvert.SerializeObject(user);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            solicitud.Content = contenido;

            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                UsuarioModel aux = JsonConvert.DeserializeObject<UsuarioModel>(response.Result);
                return aux;
            }
            else
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
            }
        }
    }
}
