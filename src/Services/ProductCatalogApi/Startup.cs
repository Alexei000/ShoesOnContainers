using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalogApi.Data;
using Swashbuckle.AspNetCore.Swagger;

namespace ProductCatalogApi
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
            services.Configure<CatalogSettings>(Configuration);

            string server = Configuration["DatabaseServer"];
            string database = Configuration["DatabaseName"];
            string user = Configuration["DatabaseUser"];
            string password = Configuration["DatabasePassword"];
            string connectionString = $"Server={server};Database={database};User={user};Password={password};";

            services.AddDbContext<CatalogContext>(opt =>
                // opt.UseSqlServer(Configuration["ConnectionString"])
                opt.UseSqlServer(connectionString)
            );

            services.AddControllers();

            //TODO: check why it does not work
            //services.AddSwaggerGen(c =>
            //{
            //    c.DescribeAllEnumsAsStrings();

            //    c.SwaggerDoc("v1", new Info { Title = "Put title here", Description = "DotNet Core Api 3 - with swagger" });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseSwagger()
            //    .UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint($"/swagger/v1/swagger.json", "ProductCatalogApi V1");
            //    });
        }
    }
}
