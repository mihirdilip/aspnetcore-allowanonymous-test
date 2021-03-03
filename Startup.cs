using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<ApiKeyOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder("ApiKey").RequireAuthenticatedUser().Build();
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/auth", async context =>
                    {
                        await context.Response.WriteAsync($"Authenticated as {context.User.Identity.Name} (is-authenticated: {context.User.Identity.IsAuthenticated})!");
                    });

                    endpoints.MapGet("/anonymous", async context =>
                    {
                        await context.Response.WriteAsync($"Anonymous as {context.User.Identity.Name} (is-authenticated: {context.User.Identity.IsAuthenticated})!");
                    }).WithMetadata(new AllowAnonymousAttribute());

                    endpoints.MapControllers();
                });
        }
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyOptions>
    {
        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                Logger.LogInformation($"================== IAllowAnonymous confirmed on {endpoint.DisplayName}! ==================");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "TestUser")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));

            //throw new NotImplementedException();

            //try
            //{
            //    throw new NotImplementedException();
            //}
            //catch (Exception e)
            //{
            //    Logger.LogError(e, e.Message);
            //    return Task.FromResult(AuthenticateResult.NoResult());
            //}
        }
    }

    public class ApiKeyOptions : AuthenticationSchemeOptions
    {
    }
}
