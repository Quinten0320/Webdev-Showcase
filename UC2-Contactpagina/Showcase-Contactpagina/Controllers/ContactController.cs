using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Showcase_Contactpagina.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Showcase_Contactpagina.Controllers
{
    public class ContactController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _recaptchaSecret;

        public ContactController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7278");

            _recaptchaSecret = configuration["Recaptcha:SecretKey"];
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(Contactform form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "De ingevulde velden voldoen niet aan de gestelde voorwaarden";
                return View();
            }

            // recaptcha uit formulier
            var recaptchaResponse = Request.Form["g-recaptcha-response"];

            // valideer recaptcha
            bool isRecaptchaValid = await ValidateRecaptcha(recaptchaResponse);
            if (!isRecaptchaValid)
            {
                TempData["Message"] = "reCAPTCHA validatie mislukt. Probeer opnieuw.";
                return View();
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(form, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // post request naar showcaseapi
            HttpResponseMessage response = await _httpClient.PostAsync("/api/mail", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Er is iets misgegaan";
                return View();
            }

            TempData["Message"] = "Het contactformulier is verstuurd";
            return View();
        }

        // valideer recaptcha
        private async Task<bool> ValidateRecaptcha(string recaptchaResponse)
        {
            using (var client = new HttpClient())
            {
                var googleUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaSecret}&response={recaptchaResponse}";

                var response = await client.GetStringAsync(googleUrl);
                var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(response);

                return recaptchaResult.Success;
            }
        }
    }

    
public class RecaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public string ChallengeTs { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }
    }
}
