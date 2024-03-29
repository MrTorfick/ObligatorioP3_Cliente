﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Obligatorio_Cliente.Models;
using System.Net.Http.Headers;
using System.Text;

namespace Obligatorio_Cliente.Controllers
{
    public class ConfiguracionController : Controller
    {

        private HttpClient cliente = new HttpClient();
        private string url = "http://localhost:5155/api";

        // GET: ConfiguracionController
        public ActionResult Index(string mensaje)
        {
            try
            {
                ViewBag.Mensaje = mensaje;
                return View(GetConfiguraciones());

            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { mensaje = ex.Message });
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
        public ActionResult Edit(string NombreAtributo, string mensaje)
        {
            try
            {
                ViewBag.Mensaje = mensaje;
                if (NombreAtributo != null)
                {
                    return View(ObtenerConfiguracionPorNombre(NombreAtributo));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {

                RedirectToAction(nameof(Index), new { mensaje = ex.Message });
            }
            return View();

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
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), new { mensaje = ex.Message });
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
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                Uri uri = new Uri(url + "/" + "Configuracion");
                HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
                solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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


        private ConfiguracionModel ObtenerConfiguracionPorNombre(string NombreAtributo)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "Configuracion" + "/" + NombreAtributo);
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");
            }
            return null;
        }

        private bool UpdateConfiguracion(ConfiguracionModel config)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
            Uri uri = new Uri(url + "/" + "Configuracion");
            HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Put, uri);
            solicitud.Headers.Add("NombreUsuario", HttpContext.Session.GetString("usuario"));
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
                Task<string> response = respuesta.Result.Content.ReadAsStringAsync();
                int codigoDeError = (int)respuesta.Result.StatusCode;
                string mensaje = response.Result.ToString();
                throw new Exception($"La solicitud no fue exitosa. Error: {codigoDeError}, {mensaje}");


            }

        }


    }
}
