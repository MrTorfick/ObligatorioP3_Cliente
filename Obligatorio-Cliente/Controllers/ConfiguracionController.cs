using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Obligatorio_Cliente.Models;
using System.Text;

namespace Obligatorio_Cliente.Controllers
{
    public class ConfiguracionController : Controller
    {

        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5155/api";

        // GET: ConfiguracionController
        public ActionResult Index()
        {
            try
            {
                return View(GetConfiguraciones());

            }
            catch (Exception)
            {
                //TODO
                throw;
            }
        }

        // GET: ConfiguracionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ConfiguracionController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ConfiguracionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ConfiguracionController/Edit/5
        public ActionResult Edit(string NombreAtributo)
        {
            try
            {
                if (NombreAtributo != null)
                {
                    return View(ObtenerConfiguracionPorNombre(NombreAtributo));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                //TODO
                throw;
            }

        }

        // POST: ConfiguracionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ConfiguracionModel config)
        {
            try
            {
                UpdateConfiguracion(config);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                //TODO
                return View();
            }
        }

        // GET: ConfiguracionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ConfiguracionController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        private IEnumerable<ConfiguracionModel> GetConfiguraciones()
        {
            try
            {
                Uri uri = new Uri(url + "/" + "Configuracion");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
                Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<ConfiguracionModel> configuraciones = JsonConvert.DeserializeObject<IEnumerable<ConfiguracionModel>>(response.Result);
                    return configuraciones;
                }
                else
                {
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError} {respuesta.Result.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private ConfiguracionModel ObtenerConfiguracionPorNombre(string NombreAtributo)
        {
            Uri uri = new Uri(url + "/" + "Configuracion" + "/" + NombreAtributo);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            string json = JsonConvert.SerializeObject(NombreAtributo);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                ConfiguracionModel config = JsonConvert.DeserializeObject<ConfiguracionModel>(response.Result);
                return config;
            }
            else
            {
                int codigoDeError = (int)respuesta.Result.StatusCode;
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError} {respuesta.Result.StatusCode}");
            }
            return null;
        }

        private bool UpdateConfiguracion(ConfiguracionModel config)
        {
            Uri uri = new Uri(url + "/" + "Configuracion");
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Put, uri);
            string json = JsonConvert.SerializeObject(config);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            solicitud.Content = contenido;
            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                return true;
            }
            else
            {
                int codigoDeError = (int)respuesta.Result.StatusCode;
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError} {respuesta.Result.StatusCode}");


            }

        }


    }
}
