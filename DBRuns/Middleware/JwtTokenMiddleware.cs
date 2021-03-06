﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace DBRuns.Middleware
{

    public class JwtTokenMiddleware
    {

        private readonly RequestDelegate next;
        public JwtTokenMiddleware(RequestDelegate next)
        {
            this.next = next;
        }



        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() => {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity.IsAuthenticated)
                {
                    context.Response.Headers.Add("X-Token", CreateTokenForIdentity(identity));
                }
                return Task.CompletedTask;
            });
            await next.Invoke(context);
        }



        private StringValues CreateTokenForIdentity(ClaimsIdentity identity)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MiaChiaveSegreta"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: "Issuer",
                                             audience: "Audience",
                                             claims: identity.Claims,
                                             expires: DateTime.Now.AddMinutes(20),
                                             signingCredentials: credentials
                                             );
            var tokenHandler = new JwtSecurityTokenHandler();
            var serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }

    }
}