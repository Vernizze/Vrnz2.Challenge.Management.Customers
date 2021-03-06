using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Reflection;
using Vrnz2.Challenge.Management.Customers.Infra.Configs;
using Vrnz2.Challenge.Management.Customers.Infra.Factories;
using Vrnz2.Challenge.Management.Customers.Shared.Settings;
using Vrnz2.Challenge.Management.Customers.Shared.Validations;
using GetCustomer = Vrnz2.Challenge.Management.Customers.UseCases.GetCustomer;
using Vrnz2.Infra.CrossCutting.Utils;
using Vrnz2.Challenge.Management.Customers.Shared.Queues;

namespace Vrnz2.Challenge.Management.Customers.WebApi
{
    public class Startup
    {
        #region Constants

        private const string SWAGGER_URL = "/swagger/v1/swagger.json";
        private const string SWAGGER_TITLE = "Vrnz2 Management Clients API";
        private const string SWAGGER_VERSION = "v1";

        #endregion 

        #region Constructors

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        #endregion

        #region Attributes

        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
                .AddFluentValidation();

            services
                .AddSettings(out AppSettings appSettings)
                .AddLogsServiceExtensions()
                .AddAutoMapper(AssembliesFactory.GetAssemblies())
                .AddMediatR(AssembliesFactory.GetAssemblies<ValidationHelper>())
                .AddIServiceColletion()
                .AddValidations()
                .AddScoped<QueueHandler>()
                .AddScoped<ControllerHelper>()
                .AddTransient<GetCustomer.GetCustomer>()
                .AddSwaggerGen(c => 
                {
                    c.SwaggerDoc(SWAGGER_VERSION, new OpenApiInfo { Title = SWAGGER_TITLE, Version = SWAGGER_VERSION });

                    c.CustomSchemaIds(x => x.FullName);

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(FilesAndFolders.AppPath(), xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });
        }

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

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint(SWAGGER_URL, string.Concat(SWAGGER_TITLE, " ", SWAGGER_VERSION)));
        }

        #endregion 
    }
}
