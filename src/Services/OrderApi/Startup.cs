using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using ShoesOnContainers.Services.OrderApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MySql.Data.MySqlClient;
using System.Threading;
using Swashbuckle.AspNetCore.Swagger;
using ShoesOnContainers.Services.OrderApi.Infrastructure.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using ShoesOnContainers.Services.OrderApi;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OrderApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(
                options => options.Filters.Add(typeof(HttpGlobalExceptionFilter))
            );


            services.Configure<OrderSettings>(Configuration);


            ConfigureAuthService(services);

            // WaitForDBInit(_connectionString);

            var hostname = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "mssqlserver";
            var password = Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "MyProduct!123";
            var database = Environment.GetEnvironmentVariable("DATABASE") ?? "OrdersDb";

            var connectionString = $"Server={hostname};Database={database};User ID=sa;Password={password};";


            services.AddDbContext<OrdersContext>(options =>
            {
                options.UseMySql(connectionString,
                     mySqlOptionsAction: sqlOptions =>
                     {
                         sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: new List<int>());
                     });
            });

            //IServiceCollection serviceCollection = services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            //    {
            //        Title = "Ordering HTTP API",
            //        Version = "v1",
            //        Description = "The Ordering Service HTTP API"
            //    });

            //    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            //    {
            //        Type = SecuritySchemeType.OAuth2,
            //        Flows = new OpenApiOAuthFlows
            //        {
            //            Implicit = new OpenApiOAuthFlow
            //            {
            //                AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrl")}/connect/authorize"),
            //                TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrl")}/connect/token"),
            //                Scopes = new Dictionary<string, string>()
            //                {
            //                    { "order", "Order Api" }
            //                }
            //            }
            //        }
            //    });

            //    options.OperationFilter<AuthorizeCheckOperationFilter>();
            //});

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                return Bus.Factory.CreateUsingRabbitMq(rmq =>
                {
                    rmq.Host(new Uri("rabbitmq://rabbitmq"), "/", host =>
                    {
                        host.Username("guest");
                        host.Password("guest");
                    });
                    rmq.ExchangeType = ExchangeType.Fanout;
                });
            })
            .As<IBusControl>()
            .As<IBus>()
            .As<IPublishEndpoint>()
            .SingleInstance();
        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var identityUrl = Configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = "order";

            });
        }
        //private void WaitForDBInit(string connectionString)
        //{
        //    var connection = new MySqlConnection(connectionString);
        //    int retries = 1;
        //    while (retries < 7)
        //    {
        //        try
        //        {
        //            Console.WriteLine("Connecting to db. Trial: {0}", retries);
        //            connection.Open();
        //            connection.Close();
        //            break;
        //        }
        //        catch (MySqlException)
        //        {
        //            Thread.Sleep((int)Math.Pow(2, retries) * 1000);
        //            retries++;
        //        }
        //    }
        //}


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            app.UseStaticFiles();
            app.UseCors("CorsPolicy");

            app.UseRouting();
            // app.UseAuthorization();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseSwagger()
            //    .UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Basket.API V1");
            //        c.OAuthConfigObject = new OAuthConfigObject
            //        {
            //            AppName = "Ordering Swagger UI",
            //            ClientId = "orderswaggerui"
            //        };
            //    });


            var root = app.ApplicationServices.GetAutofacRoot();
            var bus = root.Resolve<IBusControl>();
            bus.Start(TimeSpan.FromSeconds(30));
            bus.StartAsync();

            lifetime.ApplicationStopping.Register(() => bus.Stop());
        }
    }
}
