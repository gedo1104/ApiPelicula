using ApiPeliculas.Data;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using ApiPeliculas.PeliculasMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ApiPeliculas.Modelos;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);


//configurar la cadana de conexion
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql"));
});

//soporte para autentificacion conn .net identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//cache
var apiVersioningBuilder = builder.Services.AddApiVersioning( opcion =>
{
    opcion.AssumeDefaultVersionWhenUnspecified = true;
    opcion.DefaultApiVersion = new ApiVersion(1, 0);
    opcion.ReportApiVersions = true;
    //opcion.ApiVersionReader = ApiVersionReader.Combine(
    //    new QueryStringApiVersionReader("api-verison") //?api-version=1.0
        
    //);
});

apiVersioningBuilder.AddApiExplorer(
        opciones =>
        {
            opciones.GroupNameFormat = "'v'VVV";
            opciones.SubstituteApiVersionInUrl = true;
            
        }
    );

//Agregar los repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

//agregar el automapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

//configuracion autentificacion
var key = builder.Configuration.GetValue<string>("ApiSettings:token");

//soporte para versionamiento
builder.Services.AddApiVersioning();


builder.Services.AddAuthentication(
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(
            x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

// Add services to the container.
//y agregar cache global
builder.Services.AddControllers(options =>
{
    //cache global
    options.CacheProfiles.Add("Default20seconds", new CacheProfile() { Duration=20});
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = 
        "Autentificacion JWT con el esquema bearer. \r\n\r\n" +
        "Ingresa la palabra 'Bearer' seguida de un [espacio] y despues su token en campo de abajo \r\n\r\n " +
        "Ejemplo: \"Bearer sfsdfsdf\"", 
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
    options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1.0",
            Title = "Peliculas Api V1",
            Description = "api de peliculas",
            TermsOfService = new Uri("https://github.com/gedo1104"),
            Contact = new OpenApiContact
            {
                Name = "Gedo",
                Url = new Uri("https://github.com/gedo1104")
            },
            License = new OpenApiLicense
            {
                Name = "License",
                Url = new Uri("https://github.com/gedo1104")
            }
        }

    );
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "Peliculas Api V2",
        Description = "api de peliculas",
        TermsOfService = new Uri("https://github.com/gedo1104"),
        Contact = new OpenApiContact
        {
            Name = "Gedo",
            Url = new Uri("https://github.com/gedo1104")
        },
        License = new OpenApiLicense
        {
            Name = "License",
            Url = new Uri("https://github.com/gedo1104")
        }
    }

    );

});

//config cords
//se pueden habilitar 1 dominio como n dominios separados por coma
//si usamos el * es para todos los dominios o podemos usar uno por defecto http://localhost:443

builder.Services.AddCors(p => p.AddPolicy("PolicyCors", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader(); 
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiPeliculasV1");
        opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "ApiPeliculasV2");

    });
}
//soporte para archivos estaticos como img
app.UseStaticFiles();


app.UseHttpsRedirection();

//uso de cors
app.UseCors("PolicyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
