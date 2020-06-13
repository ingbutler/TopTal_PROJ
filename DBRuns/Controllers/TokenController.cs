using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DBRuns.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DBRuns.Controllers
{

    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        // POST api/Token
        [HttpPost]
        public IActionResult GetToken([FromBody] TokenRequest tokenRequest)
        {
            //TokenRequest è una nostra classe contenente le proprietà Username e Password
            //Avvisiamo il client se non ha fornito tali valori
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            //Lo avvisiamo anche se non ha fornito credenziali valide
            if (!VerifyCredentials(tokenRequest.Username, tokenRequest.Password))
            {
                return Unauthorized();
            }

            //Ok, l'utente ha fornito credenziali valide, creiamogli una ClaimsIdentity
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            //Aggiungiamo uno o più claim relativi all'utente loggato
            identity.AddClaim(new Claim(ClaimTypes.Name, tokenRequest.Username));
            //Incapsuliamo l'identità in una ClaimsPrincipal l'associamo alla richiesta corrente
            HttpContext.User = new ClaimsPrincipal(identity);

            //Non è necessario creare il token qui, lo possiamo creare da un middleware
            return NoContent();
        }

        private bool VerifyCredentials(string username, string password)
        {
            //TODO: Modificare questa implementazione, che è puramente dimostrativa
            return username == "Admin" && password == "Password";
        }
    }

}
