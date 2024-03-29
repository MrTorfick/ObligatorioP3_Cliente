﻿using Microsoft.AspNetCore.Http;
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
        private IWebHostEnvironment _environment;

        public EcosistemaMarinoController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }



        public ActionResult Index(string mensaje)
        {
            try
            {
                ViewBag.Mensaje = mensaje;
                IEnumerable<EcosistemaMarinoModel> ecosistemaMarinos = GetEcosistemaMarinos();
                ViewBag.Logueado = HttpContext.Session.GetString("usuario");
                return View(ecosistemaMarinos);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Create), new { mensaje = "No se pudo guardar la imagen" });
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
        public ActionResult Create(string mensaje)
        {

            if (HttpContext.Session.GetString("usuario") != null)
            {
                ViewBag.Mensaje = mensaje;

                HttpClient clientePaises = new HttpClient();
                clientePaises.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                Uri uriPaises = new Uri(url + "/" + "Pais");
                HttpRequestMessage solicitudPaises = new HttpRequestMessage(HttpMethod.Get, uriPaises);
                solicitudPaises.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
                    Task<string> response = respuestaPaises.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuestaPaises.Result.StatusCode;
                    string aux = response.Result.ToString();
                    return RedirectToAction(nameof(Create), new { mensaje = $"Error: {codigoDeError}, {aux}" });

                }
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                Uri uriEstadosConservacion = new Uri(url + "/" + "EstadoConservacion");
                HttpRequestMessage solicitudEstadosConservacion = new HttpRequestMessage(HttpMethod.Get, uriEstadosConservacion);
                solicitudEstadosConservacion.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
                    Task<string> response = respuestaPaises.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuestaPaises.Result.StatusCode;
                    string aux = response.Result.ToString();
                    return RedirectToAction(nameof(Create), new { mensaje = $"Error: {codigoDeError}, {aux}" });

                }
                Uri uriAmenazas = new Uri(url + "/" + "Amenaza");
                HttpRequestMessage solicitudAmenazas = new HttpRequestMessage(HttpMethod.Get, uriAmenazas);
                solicitudAmenazas.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
                    Task<string> response = respuestaPaises.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuestaPaises.Result.StatusCode;
                    string aux = response.Result.ToString();
                    return RedirectToAction(nameof(Create), new { mensaje = $"Error: {codigoDeError}, {aux}" });

                }

            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
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
                    UpdateEcosistema(ecosistemaMarino);
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
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    string mensaje = response.Result.ToString();
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
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
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                Uri uri = new Uri(url + "/" + "EcosistemaMarino");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Put, uri);
                solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    string mensaje = response.Result.ToString();
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
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
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "EcosistemaMarino");
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Post, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
            else
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
            }
            return null;

        }
        private AmenazaModel ObtenerAmenazaPorId(int id)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "Amenaza" + "/" + id);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
            else
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
            }
            return null;
        }

        private PaisModel ObtenerPaisPorISO(string ISO)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "Pais" + "/" + ISO);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
            else
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
            }
            return null;
        }


        private EstadoConservacionModel ObtenerEstadoConservacionPorId(int id)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "EstadoConservacion" + "/" + id);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
            else
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
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
            if (HttpContext.Session.GetString("usuario") != null)
            {

                EcosistemaMarinoModel ecosistemaMarino = obtenerEcosistemaMarinoPorId(id);
                if (ecosistemaMarino != null && ecosistemaMarino.EspeciesHabitan != null)
                {
                    if (ecosistemaMarino.EspeciesHabitan.Count > 0)
                    {
                        return RedirectToAction(nameof(Index), new { mensaje = "No se puede eliminar el ecosistema marino porque tiene especies que lo habitan" });
                    }
                }
                return View(ecosistemaMarino);

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }


        }

        // POST: EcosistemaMarinoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                BorrarEcosistema(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { mensaje = ex.Message });
            }
        }



        private void BorrarEcosistema(int id)
        {
            try
            {
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                Uri uri = new Uri(url + "/" + "EcosistemaMarino" + "/" + id);
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Delete, uri);
                solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
                Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    response.Wait();
                }
                else
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    string mensaje = response.Result.ToString();
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }



        private EcosistemaMarinoModel obtenerEcosistemaMarinoPorId(int id)
        {
            try
            {
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                Uri uri = new Uri(url + "/" + "EcosistemaMarino" + "/" + id);
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
                solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
                Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    response.Wait();
                    EcosistemaMarinoModel ecosistemaMarino = JsonConvert.DeserializeObject<EcosistemaMarinoModel>(response.Result);
                    return ecosistemaMarino;
                }
                else
                {
                    Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    string mensaje = response.Result.ToString();
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
