using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using System.Reflection;
using TasteItApi.authentication;
using TasteItApi.Graph.Admin.Repositories;
using TasteItApi.Graph.Admin.Services;
using TasteItApi.Graph.Configuration;
using TasteItApi.Graph.Infrastructure;
using TasteItApi.Graph.Repositories;
using TasteItApi.Graph.Services;
using TasteItApi.Graph.Testing.Repositories;
using TasteItApi.Graph.Testing.Services;

namespace TasteItApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problem = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation failed"
                    };
                    return new BadRequestObjectResult(problem);
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TasteIt", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
                c.DocInclusionPredicate((_, apiDesc) =>
                {
                    if (_env.IsDevelopment())
                    {
                        return true;
                    }

                    var path = apiDesc.RelativePath ?? string.Empty;
                    return !path.StartsWith("test", StringComparison.OrdinalIgnoreCase);
                });
            });

            services.AddCors();

            services.AddSingleton(FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("firebase-config.json")
            }));
            services.AddFirebaseAuthentication();

            var neo4jOptions = Configuration.GetSection("Neo4j").Get<Neo4jOptions>() ?? new Neo4jOptions();
            services.Configure<Neo4jOptions>(Configuration.GetSection("Neo4j"));
            if (string.IsNullOrWhiteSpace(neo4jOptions.Password))
            {
                throw new InvalidOperationException("Neo4j password is not configured. Set Neo4j__Password in environment variables or user secrets.");
            }

            services.AddSingleton<IDriver>(_ =>
                GraphDatabase.Driver(
                    neo4jOptions.Uri,
                    AuthTokens.Basic(neo4jOptions.Username, neo4jOptions.Password),
                    builder =>
                    {
                        builder.WithConnectionTimeout(TimeSpan.FromSeconds(neo4jOptions.ConnectionTimeoutSeconds));
                        builder.WithMaxConnectionPoolSize(neo4jOptions.MaxConnectionPoolSize);
                    }));

            services.AddScoped<IRecipeGraphRepository, RecipeGraphRepository>();
            services.AddScoped<IRecipeGraphService, RecipeGraphService>();
            services.AddScoped<IAdminGraphRepository, AdminGraphRepository>();
            services.AddScoped<IAdminGraphService, AdminGraphService>();

            var enableTestEndpoints = Configuration.GetValue<bool>("Features:EnableTestGraphEndpoints");
            if (_env.IsDevelopment() || enableTestEndpoints)
            {
                services.AddScoped<ITestGraphRepository, TestGraphRepository>();
                services.AddScoped<ITestGraphService, TestGraphService>();
            }

            services.AddHostedService<Neo4jSchemaInitializationService>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("profile", policy => policy.RequireClaim("profile"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    var problem = new ProblemDetails
                    {
                        Title = "Unexpected error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = env.IsDevelopment() ? feature?.Error.ToString() : "Internal server error"
                    };

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(problem);
                });
            });

            app.Use(async (context, next) =>
            {
                if (!env.IsDevelopment() &&
                    context.Request.Path.Value?.StartsWith("/test", StringComparison.OrdinalIgnoreCase) == true)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TasteIt v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
