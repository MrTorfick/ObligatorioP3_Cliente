using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Obligatorio_Cliente.Models;
using System.Data;
using System.Text;

namespace Obligatorio_Cliente.Controllers
{
    public class EspecieMarinaController : Controller
    {

        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5155/api";
        private IWebHostEnvironment _environment;

        public EspecieMarinaController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
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
                //if (HttpContext.Session.GetString("LogueadoNombre") != null)
                //{
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

                // }
                //else
                //{
                //  return RedirectToAction(nameof(Index);
                //}

            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }
        }

        // POST: EspecieMarinaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EspecieMarinaModel especieMarina, List<IFormFile> imagen, List<int> SelectedOptions, int SelectedOptionEstado, List<int> SelectedOptionsAmenazas)
        {
            try
            {
                if (especieMarina == null || imagen.Count == 0 || SelectedOptions.Count == 0 || SelectedOptionEstado == 0 || SelectedOptionsAmenazas.Count == 0)
                {
                    return RedirectToAction(nameof(Create), new { mensaje = "Debe completar todos los campos" });
                }

                especieMarina.EcosistemaMarinos = new List<EcosistemaMarinoModel>();
                foreach (var id in SelectedOptions)
                {
                    EcosistemaMarinoModel ecosistemaMarino = obtenerEcosistemaMarinoPorId(id);
                    if (ecosistemaMarino != null)
                    {
                        especieMarina.EcosistemaMarinos.Add(ecosistemaMarino);
                    }

                }
                especieMarina.Amenazas = new List<AmenazasAsociadasModel>();
                foreach (var item in SelectedOptionsAmenazas)
                {
                    AmenazaModel amenaza = ObtenerAmenazaPorId(item);

                    if (amenaza != null)
                    {
                        AmenazasAsociadasModel amenazasAsociadas = new AmenazasAsociadasModel();
                        amenazasAsociadas.AmenazaId = amenaza.Id;
                        especieMarina.Amenazas.Add(amenazasAsociadas);
                    }
                }

                especieMarina.EstadoConservacionId = SelectedOptionEstado;
                EspecieMarinaModel especieMarinaModel = AltaEspecie(especieMarina);
                if (GuardarImagen(imagen, especieMarinaModel))
                {
                    if (UpdateEspecie(especieMarinaModel))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        //TODO
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    //TODO
                    return RedirectToAction(nameof(Create), new { mensaje = "No se pudo guardar la imagen" });
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Create), new { mensaje = ex.Message });
            }
        }

        public ActionResult AsociarEspecieAEcosistema(string mensaje, int id)
        {
            try
            {
                if (HttpContext.Session.GetString("LogueadoNombre") != null)
                {
                    List<EspecieEcosistemaModel> especieAsociarEcosistema = new List<EspecieEcosistemaModel>();
                    EspecieMarinaModel especieMarina = ObtenerEspeciePorId(id);
                    if (especieMarina != null)
                    {

                        TempData["idEspecie"] = especieMarina.Id;


                        foreach (EcosistemaMarinoModel item in especieMarina.EcosistemaMarinos)
                        {
                            if (item != null)
                            {
                                EspecieEcosistemaModel aux = new EspecieEcosistemaModel();
                                aux.ecosistemasMarinos = item;
                                especieAsociarEcosistema.Add(aux);

                            }

                        }
                        ViewBag.Mensaje = mensaje;
                        return View(especieAsociarEcosistema);

                    }
                    else
                    {
                        return RedirectToAction(nameof(AsociarEspecieAEcosistema), new { mensaje = "No se encontro la especie" });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(AsociarEspecieAEcosistema), new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AsociarEspecieAEcosistema(int EcosistemaSeleccionado)
        {

            try
            {
                int idEspecie = (int)TempData["idEspecie"];
                EspecieMarinaModel especieMarina = ObtenerEspeciePorId(idEspecie);

                IEnumerable<EspecieMarinaModel> especieMarinas = BuscarEspeciesQueHabitanUnEcosistema(EcosistemaSeleccionado);

                foreach (var item in especieMarinas)
                {
                    if (item.Id == idEspecie)
                    {
                        return RedirectToAction(nameof(AsociarEspecieAEcosistema), new { mensaje = "La especie ya se encuentra asociada al ecosistema" });
                    }
                }

                EcosistemaMarinoModel ecosistemaMarino = obtenerEcosistemaMarinoPorId(EcosistemaSeleccionado);

                foreach (var item in ecosistemaMarino.Amenazas)
                {
                    foreach (var item1 in especieMarina.Amenazas)
                    {
                        if (item.AmenazaId == item1.AmenazaId)
                        {
                            return RedirectToAction(nameof(AsociarEspecieAEcosistema), new { mensaje = "La especie no puede habitar el ecosistema porque tiene amenazas en comun" });
                        }
                    }
                }
                //Ecosistema
                EstadoConservacionModel estadoConservacionEcosistema = ObtenerEstadoConservacionPorId((int)ecosistemaMarino.EstadoConservacionId);
                RangosModel rangosEcosistema = new RangosModel();
                rangosEcosistema.Minimo = estadoConservacionEcosistema.Rangos.Minimo;
                rangosEcosistema.Maximo = estadoConservacionEcosistema.Rangos.Maximo;

                //Especie
                EstadoConservacionModel estadoConservacionEspecie = ObtenerEstadoConservacionPorId((int)especieMarina.EstadoConservacionId);
                RangosModel rangosEspecie = new RangosModel();
                rangosEspecie.Minimo = estadoConservacionEspecie.Rangos.Minimo;
                rangosEspecie.Maximo = estadoConservacionEspecie.Rangos.Maximo;

                if (rangosEcosistema.Minimo < rangosEspecie.Minimo)
                {
                    return RedirectToAction(nameof(AsociarEspecieAEcosistema), new { mensaje = "La especie no puede habitar el ecosistema porque el estado de conservacion del ecosistema es menor al de la especie" });
                }


                asociarEspecieEcosistemaUC.AsociarEspecieAEcosistema(idEspecie, EcosistemaSeleccionado, HttpContext.Session.GetString("LogueadoNombre"));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(AsociarEspecieAEcosistema), new { mensaje = ex.Message });
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


        private bool GuardarImagen(List<IFormFile> imagen, EspecieMarinaModel em)
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
                (rutaFisicaWwwRoot, "images", "especie", nombreImagen);
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


        private EspecieMarinaModel AltaEspecie(EspecieMarinaModel especie)
        {
            Uri uri = new Uri(url + "/" + "EspecieMarina");
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Post, uri);

            string json = JsonConvert.SerializeObject(especie);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
            solicitud.Content = contenido;

            Task<HttpResponseMessage> respuesta = cliente.SendAsync(solicitud);
            respuesta.Wait();

            if (respuesta.Result.IsSuccessStatusCode)
            {
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                response.Wait();
                EspecieMarinaModel especieMarina = JsonConvert.DeserializeObject<EspecieMarinaModel>(response.Result);
                return especieMarina;
            }
            return null;

        }

        private EspecieMarinaModel ObtenerEspeciePorId(int id)
        {

            try
            {
                Uri uri = new Uri(url + "/" + "EspecieMarina" + "/" + "Especie" + "/" + id);
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
                    EspecieMarinaModel especie = JsonConvert.DeserializeObject<EspecieMarinaModel>(response.Result);
                    return especie;
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


        private bool UpdateEspecie(EspecieMarinaModel especie)
        {
            try
            {
                Uri uri = new Uri(url + "/" + "EspecieMarina");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Put, uri);

                string json = JsonConvert.SerializeObject(especie);
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


        private EcosistemaMarinoModel obtenerEcosistemaMarinoPorId(int id)
        {
            try
            {
                Uri uri = new Uri(url + "/" + "EcosistemaMarino" + "/" + id);
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
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
                    int codigoDeError = (int)respuesta.Result.StatusCode;
                    throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError} {respuesta.Result.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                throw;
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
            else
            {
                int codigoDeError = (int)respuesta.Result.StatusCode;
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError} {respuesta.Result.StatusCode}");
            }
        }

        private IEnumerable<EspecieMarinaModel> BuscarEspeciesQueHabitanUnEcosistema(int idEcosistema)
        {
            Uri uri = new Uri(url + "/" + "EspecieMarina" + "/" + "EspeciesPorEcosistema" + "/" + idEcosistema);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            string json = JsonConvert.SerializeObject(idEcosistema);
            Console.WriteLine(json);
            HttpContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
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
    }
}
