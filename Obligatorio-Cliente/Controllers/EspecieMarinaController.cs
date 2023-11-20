using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Obligatorio_Cliente.Models;
using System.Data;

namespace Obligatorio_Cliente.Controllers
{
    public class EspecieMarinaController : Controller
    {

        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5155/api";
        private IWebHostEnvironment _environment;
        // GET: EspecieMarinaController
        public ActionResult Index(string mensaje)
        {
            try
            {
                return View(GetEspeciesMarinas());

            }
            catch (Exception ex)
            {

                ViewBag.mensaje = ex.Message;
                return View();
            }
        }

        // GET: EspecieMarinaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EspecieMarinaController/Create
        public ActionResult Create()
        {
            try
            {
                if (HttpContext.Session.GetString("LogueadoNombre") != null)
                {
                    IEnumerable<EcosistemaMarinoModel> ecosistemasMarinos = GetEcosistemaMarinos();
                    if (ecosistemasMarinos.Count() > 0)
                    {
                        ViewBag.EcosistemasMarinos = ecosistemasMarinos;
                    }
                    else
                    {
                        ViewBag.EcosistemasMarinos = null;
                    }
                    ViewBag.Amenazas = GetAmenazas();
                    ViewBag.EstadosConservacion = GetEstadosConservaciones();
                    return View();

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }
        }

        // POST: EspecieMarinaController/Create
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

        // GET: EspecieMarinaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EspecieMarinaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: EspecieMarinaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EspecieMarinaController/Delete/5
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



        private IEnumerable<EcosistemaMarinoModel> GetEcosistemaMarinos()
        {
            try
            {
                Uri uri = new Uri(url + "/" + "EcosistemaMarino");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
                Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<EcosistemaMarinoModel> ecosistemaMarinos = JsonConvert.DeserializeObject<IEnumerable<EcosistemaMarinoModel>>(response.Result);
                    return ecosistemaMarinos;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private IEnumerable<AmenazaModel> GetAmenazas()
        {
            try
            {
                Uri uriAmenazas = new Uri(url + "/" + "Amenaza");
                HttpRequestMessage solicitudAmenazas = new HttpRequestMessage(HttpMethod.Get, uriAmenazas);
                Task<HttpResponseMessage> respuestaAmenazas = cliente.SendAsync(solicitudAmenazas);
                respuestaAmenazas.Wait();
                if (respuestaAmenazas.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuestaAmenazas.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<AmenazaModel> amenazaModels = JsonConvert.DeserializeObject<IEnumerable<AmenazaModel>>(response.Result);
                    return amenazaModels;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        private IEnumerable<EstadoConservacionModel> GetEstadosConservaciones()
        {
            try
            {
                Uri uriEstadosConservacion = new Uri(url + "/" + "EstadoConservacion");
                HttpRequestMessage solicitudEstadosConservacion = new HttpRequestMessage(HttpMethod.Get, uriEstadosConservacion);
                Task<HttpResponseMessage> respuestaEstadosConservacion = cliente.SendAsync(solicitudEstadosConservacion);
                respuestaEstadosConservacion.Wait();
                if (respuestaEstadosConservacion.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuestaEstadosConservacion.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<EstadoConservacionModel> estadoConservacionModels = JsonConvert.DeserializeObject<IEnumerable<EstadoConservacionModel>>(response.Result);
                    return estadoConservacionModels;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }


        private IEnumerable<EspecieMarinaModel> GetEspeciesMarinas()
        {
            try
            {
                Uri uri = new Uri(url + "/" + "EspecieMarina");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
                Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<EspecieMarinaModel> especieMarinas = JsonConvert.DeserializeObject<IEnumerable<EspecieMarinaModel>>(response.Result);
                    return especieMarinas;
                }
                else
                {
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError} {respuesta.Result.StatusCode}");

                }

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
