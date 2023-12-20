using DemoProgressBarAPI.Hubs;
using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header (Example: 'Bearer asdfasdfasdf')",
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
                 }
             },
             new string[] {}
     }
});
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

var config = builder.Configuration;
builder.Services.AddAuthentication(options =>
{
    //// This forces challenge results to be handled by Google OpenID Handler, so there's no
    //// need to add an AccountController that emits challenges for Login.
    //o.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    //// This forces forbid results to be handled by Google OpenID Handler, which checks if
    //// extra scopes are required and does automatic incremental auth.
    //o.DefaultForbidScheme = GoogleDefaults.AuthenticationScheme;
    //// Default scheme that will handle everything else.
    //// Once a user is authenticated, the OAuth2 token info is stored in cookies.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
//    .AddCookie()
    .AddGoogle(googleOptions =>
{
    googleOptions.ClientId = config["GoogleAuth:ClientID"];
    googleOptions.ClientSecret = config["GoogleAuth:SecretKey"];
});
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(builder =>
//    {
//        builder
//            .WithOrigins()
//            .AllowAnyMethod()
//            .AllowAnyHeader();
//    });
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(builder =>
        builder
        .WithOrigins("https://localhost:7145", "https://localhost:32768", "https://localhost:44318", "https://demoprogressbar.moon719096service.uk")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DemoHub>("/DownloadHub");
app.MapHub<ChatHub>("/ChatHub");

app.Run();
