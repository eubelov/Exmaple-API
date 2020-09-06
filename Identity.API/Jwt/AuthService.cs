using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Identity.API.Configuration;
using Identity.API.Models;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Identity.API.Jwt
{
    public sealed class AuthService
    {
        private readonly IHttpClientFactory clientFactory;

        private readonly Auth0Options auth0Options;

        public AuthService(IHttpClientFactory clientFactory, IOptions<Auth0Options> options)
        {
            this.clientFactory = clientFactory;
            this.auth0Options = options.Value;
        }

        public async Task<(TokenModel Token, HttpStatusCode Code)> Authorize(LoginModel loginModel)
        {
            using var client = this.clientFactory.CreateClient("auth0");
            var result = await client.PostAsync(
                             string.Empty,
                             new FormUrlEncodedContent(
                                 new Dictionary<string, string>
                                 {
                                     ["client_id"] = this.auth0Options.ClientId,
                                     ["client_secret"] = this.auth0Options.ClientSecret,
                                     ["audience"] = "https://sample.api.dev.com",
                                     ["username"] = loginModel.Login,
                                     ["password"] = loginModel.Password,
                                     ["grant_type"] = "password",
                                     ["scope"] = "openid profile email",
                                     ["response_type"] = "code"
                                 }));

            var responseString = await result.Content.ReadAsStringAsync();

            return (JsonConvert.DeserializeObject<TokenModel>(responseString), result.StatusCode);
        }
    }
}