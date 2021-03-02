using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
                        await context.Response.WriteAsync("Authenticated Hello World!");
                    });

                    endpoints.MapGet("/anonymous", async context =>
                    {
                        await context.Response.WriteAsync("Anonymous Hello World!");
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
            if (Context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                
                var message = "======================================================" + Environment.NewLine +
                    "IAllowAnonymous confirmed!" + Environment.NewLine +
                    "======================================================";

                Logger.LogInformation(message);
            }

            throw new NotImplementedException();

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
