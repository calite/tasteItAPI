using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using TasteItApi.authentication;
using TasteItApi.Graph.Configuration;
using TasteItApi.Graph.Repositories;
using TasteItApi.Graph.Services;

namespace TasteItApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TasteIt", Version = "v1" });
            });

            services.AddCors();

            services.AddSingleton(FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("firebase-config.json")
            }));
            services.AddFirebaseAuthentication();

            var neo4jOptions = Configuration.GetSection("Neo4j").Get<Neo4jOptions>() ?? new Neo4jOptions();
            services.Configure<Neo4jOptions>(Configuration.GetSection("Neo4j"));

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("profile", policy => policy.RequireClaim("profile"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

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
