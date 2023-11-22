using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System;
using Obligatorio_Cliente.Models;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.Metrics;
using System.Security.Policy;

namespace Obligatorio_Cliente.Controllers
{
    public class EcosistemaMarinoController : Controller
    {
        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5155/api";
        private string urlPaises = "https://restcountries.com/v3.1/all";
        private bool PaisesCargados = true;
        private IWebHostEnvironment _environment;

        public EcosistemaMarinoController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        private bool CargarPaises()
        {
            if (!PaisesCargados)
            {
                Uri uriPaises = new Uri(urlPaises);
                HttpRequestMessage solicitudPaises = new HttpRequestMessage(HttpMethod.Get, uriPaises);
                Task<HttpResponseMessage> respuestaPaises = cliente.SendAsync(solicitudPaises);
                respuestaPaises.Wait();

                if (respuestaPaises.Result.IsSuccessStatusCode)
                {
                    Task<string> responsePaises = respuestaPaises.Result.Content.ReadAsStringAsync();
                    responsePaises.Wait();
                    IEnumerable<PaisModel> paises = JsonConvert.DeserializeObject<IEnumerable<PaisModel>>(responsePaises.Result);

                    //Paises post
                    Uri uri2 = new Uri(url + "/" + "Pais");
                    HttpRequestMessage solicitud2 = new HttpRequestMessage(HttpMethod.Post, uri2);

                    string json = JsonConvert.SerializeObject(paises);
                    Console.WriteLine(json);
                    HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
                    solicitud2.Content = contenido;

                    Task<HttpResponseMessage> respuesta2 = cliente.SendAsync(solicitud2);
                    respuesta2.Wait();

                    if (respuesta2.Result.IsSuccessStatusCode)
                    {
                        Task<string> response2 = respuesta2.Result.Content.ReadAsStringAsync();
                        response2.Wait();
                    }
                    else
                    {
                        return false;
                    }
                    PaisesCargados = true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        

        public ActionResult Index()
        {
            try
            {
                if (!CargarPaises())
                {
                    return RedirectToAction("Error");
                }
                else
                {

                    //cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                    //cliente.DefaultRequestHeaders.Add("Accept", "application/json"); ?
                    IEnumerable<EcosistemaMarinoModel> ecosistemaMarinos = GetEcosistemaMarinos();
                    return View(ecosistemaMarinos);

                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                //TODO
                return RedirectToAction("Error");
            }
        }

        public ActionResult Error()
        {
            return View();
        }

        // GET: EcosistemaMarinoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EcosistemaMarinoController/Create
        public ActionResult Create()
        {
            CargarPaises();
            if (!CargarPaises())
            {
                return RedirectToAction("Error");
            }
            else
            {

                HttpClient clientePaises = new HttpClient();
                Uri uriPaises = new Uri(url + "/" + "Pais");
                HttpRequestMessage solicitudPaises = new HttpRequestMessage(HttpMethod.Get, uriPaises);
                Task<HttpResponseMessage> respuestaPaises = clientePaises.SendAsync(solicitudPaises);
                respuestaPaises.Wait();

                if (respuestaPaises.Result.IsSuccessStatusCode)
                {
                    Task<string> responsePaises = respuestaPaises.Result.Content.ReadAsStringAsync();
                    responsePaises.Wait();
                    IEnumerable<PaisModel> paises = JsonConvert.DeserializeObject<IEnumerable<PaisModel>>(responsePaises.Result);
                    ViewBag.Paises = paises;
                }
                else
                {
                    return RedirectToAction("Error");
                }
                Uri uriEstadosConservacion = new Uri(url + "/" + "EstadoConservacion");
                HttpRequestMessage solicitudEstadosConservacion = new HttpRequestMessage(HttpMethod.Get, uriEstadosConservacion);
                Task<HttpResponseMessage> respuestaEstadosConservacion = cliente.SendAsync(solicitudEstadosConservacion);
                respuestaEstadosConservacion.Wait();
                if (respuestaEstadosConservacion.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuestaEstadosConservacion.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<EstadoConservacionModel> estadoConservacionModels = JsonConvert.DeserializeObject<IEnumerable<EstadoConservacionModel>>(response.Result);
                    ViewBag.EstadosConservacion = estadoConservacionModels;
                }
                else
                {
                    return RedirectToAction("Error");

                }
                Uri uriAmenazas = new Uri(url + "/" + "Amenaza");
                HttpRequestMessage solicitudAmenazas = new HttpRequestMessage(HttpMethod.Get, uriAmenazas);
                Task<HttpResponseMessage> respuestaAmenazas = cliente.SendAsync(solicitudAmenazas);
                respuestaAmenazas.Wait();
                if (respuestaAmenazas.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuestaAmenazas.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    IEnumerable<AmenazaModel> amenazaModels = JsonConvert.DeserializeObject<IEnumerable<AmenazaModel>>(response.Result);
                    ViewBag.Amenazas = amenazaModels;
                }
                else
                {
                    return RedirectToAction("Error");

                }
            }

            //ViewBag.Mensaje = mensaje;
            return View();
        }

        // POST: EcosistemaMarinoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EcosistemaMarinoModel ecosistemasMarinos, string Longitud, string Latitud, List<IFormFile> imagen, int SelectedOptionEstado, List<int> SelectedOptionsAmenazas, string PaisSeleccionado)
        {
            try
            {

                if (ecosistemasMarinos == null || imagen.Count == 0 || SelectedOptionEstado == 0 || PaisSeleccionado == null || Latitud == null || Longitud == null)
                    return RedirectToAction(nameof(Error));

                string LongitudTipo = "Longitud";
                string LatitudTipo = "Latitud";

                string grados_Latitud = ecosistemasMarinos.GradosMinutosSegundos(Latitud, LatitudTipo);
                string grados_Longitud = ecosistemasMarinos.GradosMinutosSegundos(Longitud, LongitudTipo);

                ecosistemasMarinos.Coordenadas = new CoordenadasModel(grados_Longitud, grados_Latitud);
                ecosistemasMarinos.EstadoConservacionId = this.ObtenerEstadoConservacionPorId(SelectedOptionEstado).Id;
                ecosistemasMarinos.Amenazas = new List<AmenazasAsociadasModel>();
                foreach (var item in SelectedOptionsAmenazas)
                {
                    AmenazaModel amenaza = this.ObtenerAmenazaPorId(item);

                    if (amenaza != null)
                    {
                        AmenazasAsociadasModel amenazasAsociadas = new AmenazasAsociadasModel();
                        amenazasAsociadas.AmenazaId = amenaza.Id;
                        ecosistemasMarinos.Amenazas.Add(amenazasAsociadas);
                    }
                }
                ecosistemasMarinos.PaisId = ObtenerPaisPorISO(PaisSeleccionado).id;
                EcosistemaMarinoModel ecosistemaMarino = AltaEcosistema(ecosistemasMarinos);

                if (GuardarImagen(imagen, ecosistemaMarino))
                {
                    if (UpdateEcosistema(ecosistemaMarino))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction(nameof(Error));
                    }
                }
                else
                {
                    return RedirectToAction(nameof(Error));
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Create), new
                {
                    mensaje = ex.Message
                });
            }
        }



        /*
         public ActionResult Create(EcosistemaMarinoModel ecosistemasMarinos, string Longitud, string Latitud, List<IFormFile> imagen, int SelectedOptionEstado, List<int> SelectedOptionsAmenazas, int PaisSeleccionado)
        {
            try
            {

                if (ecosistemasMarinos == null || imagen.Count == 0 || SelectedOptionEstado == 0 || PaisSeleccionado == 0 || Latitud == null || Longitud == null)

                    return RedirectToAction(nameof(Create), new { mensaje = "Debe ingresar todos los datos" });

                string LongitudTipo = "Longitud";
                string LatitudTipo = "Latitud";

                //string grados_Latitud = ecosistemasMarinos.GradosMinutosSegundos(Latitud, LatitudTipo);
                //string grados_Longitud = ecosistemasMarinos.GradosMinutosSegundos(Longitud, LongitudTipo);

                //ecosistemasMarinos.Coordenadas = new CoordenadasModel(grados_Longitud, grados_Latitud);

                //ecosistemasMarinos.EstadoConservacionId = this.obtenerEstadoConservacionPorIdUC.ObtenerEstadoConservacionPorId(SelectedOptionEstado).Id;
                ecosistemasMarinos.Amenazas = new List<AmenazasAsociadas>();
                foreach (var item in SelectedOptionsAmenazas)
                {
                    Amenaza amenaza = this.obtenerAmenazasPorIdUC.ObtenerAmenazaPorId(item);

                    if (amenaza != null)
                    {
                        AmenazasAsociadas amenazasAsociadas = new AmenazasAsociadas();
                        amenazasAsociadas.AmenazaId = amenaza.Id;
                        ecosistemasMarinos.Amenazas.Add(amenazasAsociadas);
                    }
                }
                ecosistemasMarinos.PaisId = obtenerPaisPorIdUC.ObtenerPaisPorId(PaisSeleccionado).PaisId;
                addEcosistemaMarinoUC.AddEcosistemaMarino(ecosistemasMarinos, HttpContext.Session.GetString("LogueadoNombre"));
                if (GuardarImagen(imagen, ecosistemasMarinos))
                {
                    updateEcosistemaMarinoUC.UpdateEcosistemaMarino(ecosistemasMarinos, HttpContext.Session.GetString("LogueadoNombre"));
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(Create), new { mensaje = "No se pudo guardar la imagen" });
                }


            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Create), new { mensaje = ex.Message });
            }
        }
         
         
         */
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
        private bool UpdateEcosistema(EcosistemaMarinoModel ecosistema)
        {
            try
            {
                Uri uri = new Uri(url + "/" + "EcosistemaMarino");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Put, uri);

                string json = JsonConvert.SerializeObject(ecosistema);
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
                    return false;
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        private EcosistemaMarinoModel AltaEcosistema(EcosistemaMarinoModel ecosistema)
        {
            ecosistema.EspeciesHabitan = new List<EspecieMarinaModel>();
            Uri uri = new Uri(url + "/" + "EcosistemaMarino");
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Post, uri);

            string json = JsonConvert.SerializeObject(ecosistema);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            solicitud.Content = contenido;

            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                EcosistemaMarinoModel ecosistemaMarino = JsonConvert.DeserializeObject<EcosistemaMarinoModel>(response.Result);
                return ecosistemaMarino;
            }
            return null;

        }
        private AmenazaModel ObtenerAmenazaPorId(int id)
        {

            Uri uri = new Uri(url + "/" + "Amenaza" + "/" + id);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            string json = JsonConvert.SerializeObject(id);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                AmenazaModel amenaza = JsonConvert.DeserializeObject<AmenazaModel>(response.Result);
                return amenaza;
            }
            return null;
        }

        private PaisModel ObtenerPaisPorISO(string ISO)
        {
            Uri uri = new Uri(url + "/" + "Pais" + "/" + ISO);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            string json = JsonConvert.SerializeObject(ISO);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                PaisModel pais = JsonConvert.DeserializeObject<PaisModel>(response.Result);
                return pais;
            }
            return null;
        }


        private EstadoConservacionModel ObtenerEstadoConservacionPorId(int id)
        {
            Uri uri = new Uri(url + "/" + "EstadoConservacion" + "/" + id);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            string json = JsonConvert.SerializeObject(id);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                EstadoConservacionModel estadoConservacion = JsonConvert.DeserializeObject<EstadoConservacionModel>(response.Result);
                return estadoConservacion;
            }
            return null;
        }


        private bool GuardarImagen(List<IFormFile> imagen, EcosistemaMarinoModel em)
        {
            if (imagen == null || em == null) return false;
            // SUBIR LA IMAGEN
            //ruta física de wwwroot
            string rutaFisicaWwwRoot = _environment.WebRootPath;
            int num = 0;
            foreach (var item in imagen)
            {
                string tipoImagen;
                if (item.ContentType.Contains("png"))
                {
                    tipoImagen = ".png";
                }
                else if (item.ContentType.Contains("jpeg"))
                {
                    tipoImagen = ".jpeg";
                }
                else
                {
                    tipoImagen = ".jpg";
                }

                string numString = num.ToString("D3");
                string nombreImagen = item.FileName;
                nombreImagen = em.Id + "_" + numString + tipoImagen;
                num++;
                //ruta donde se guardan las fotos de las personas
                string rutaFisicaFoto = Path.Combine
                (rutaFisicaWwwRoot, "images", "ecosistema", nombreImagen);
                //FileStream permite manejar archivos
                try
                {
                    //el método using libera los recursos del objeto FileStream al finalizar
                    using (FileStream f = new FileStream(rutaFisicaFoto, FileMode.Create))
                    {
                        //Para archivos grandes o varios archivos usar la versión
                        //asincrónica de CopyTo. Sería: await imagen.CopyToAsync (f);
                        item.CopyTo(f);
                    }
                    //GUARDAR EL NOMBRE DE LA IMAGEN SUBIDA EN EL OBJETO
                    ImagenModel imagenEnviar = new ImagenModel(nombreImagen);
                    em.Imagen.Add(imagenEnviar);

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;

        }

        // GET: EcosistemaMarinoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EcosistemaMarinoController/Edit/5
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

        // GET: EcosistemaMarinoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EcosistemaMarinoController/Delete/5
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
    }
}
