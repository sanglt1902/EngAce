﻿using Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("ClientId");
    options.ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
    options.Scope.Add("https://www.googleapis.com/auth/cloud-platform");
    options.Scope.Add("https://www.googleapis.com/auth/generative-language.retriever");
    options.SaveTokens = true;
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = context =>
        {
            context.Identity.AddClaim(new Claim("access_token", context.AccessToken));

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
HttpContextHelper.Configure(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>());
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EngAce APIs Documentation",
        Version = "v1.0.0",
        Description = "Developed by Phan Xuan Quang."
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authentication",
        Type = SecuritySchemeType.ApiKey,
        Description = "The API Key or the Access Token to access Gemini services"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    c.UseAllOfToExtendReferenceSchemas();

    c.MapType<ProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } });
    c.MapType<ValidationProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ValidationProblemDetails" } });
    c.MapType<SerializableError>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "SerializableError" } });

    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
    options.EnableForHttps = true;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend APIs");
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseResponseCompression();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();

app.Run();