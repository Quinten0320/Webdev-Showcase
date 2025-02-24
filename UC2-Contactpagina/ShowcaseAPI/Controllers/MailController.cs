using Microsoft.AspNetCore.Mvc;
using Showcase_Contactpagina.Models;
using System.Net;
using System.Net.Mail;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowcaseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        // POST api/<MailController>
        [HttpPost]
        public ActionResult Post([FromBody] Contactform form)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("55375c476ffc0c", "5156d655c46e18"),
                EnableSsl = true
            };
            client.Send("from@example.com", "s1182530@student.windesheim.nl", $"{form.Subject}",
                $" voornaam: {form.FirstName} achternaam: {form.LastName} email: {form.Email} telefoonnummer: {form.Phone} bericht: {form.Message}");
            System.Console.WriteLine("Sent");

            return Ok();
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok("ping");
        }
    }
}
