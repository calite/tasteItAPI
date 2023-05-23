using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Neo4jClient;
using TasteItApi.authentication;

namespace TasteItApi
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

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TasteIt", Version = "v1" });
            });

            services.AddCors(); //cors

            //jwt token con firebase
  
            services.AddSingleton(FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase-config.json")
            }));
            services.AddFirebaseAuthentication();
            
            //NEO
            var client = new BoltGraphClient(new Uri("neo4j+s://dc95b24b.databases.neo4j.io"), "neo4j", "sBQ6Fj2oXaFltjizpmTDhyEO9GDiqGM1rG-zelf17kg");
            client.ConnectAsync();
            services.AddSingleton<IGraphClient>(client);

            services.AddAuthorization(opciones => //autorizacion para admin
            {
                opciones.AddPolicy("profile", politica => politica.RequireClaim("profile"));
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); //cors


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
