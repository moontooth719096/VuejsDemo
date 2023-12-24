using DemoProgressBarAPI.Hubs;
using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models;
using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTsettings"));
builder.Services.AddSignalR();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddSingleton<IYoutubeListDownloadService, YoutubeClientVerDownloadService>();

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
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = config["JWTsettings:ValidIssuer"],
        ValidAudience = config["JWTsettings:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["JWTsettings:Secret"])),
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR 會將 Token 以參數名稱 access_token 的方式放在 URL 查詢參數裡
            var accessToken = context.Request.Query["access_token"];

            // 連線網址為 Hubs 相關路徑才檢查
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
//    .AddCookie()
//    .AddGoogle(googleOptions =>
//{
//    googleOptions.ClientId = config["GoogleAuth:ClientID"];
//    googleOptions.ClientSecret = config["GoogleAuth:SecretKey"];
//});
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
app.MapHub<YoutubeDownloadProgressHub>("/youtubeDownloadProgressHub");

app.Run();
