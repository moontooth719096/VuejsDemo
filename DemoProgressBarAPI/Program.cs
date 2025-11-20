using DemoProgressBarAPI.Hubs;
using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Middlewares;
using DemoProgressBarAPI.Models;
using DemoProgressBarAPI.Services;
using DemoProgressBarAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())  // 設定基礎目錄
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // 主設定檔
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true) // 環境設定檔
    .AddEnvironmentVariables(); // 允許環境變數覆蓋設定 // Ensure environment variables are loaded first

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
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

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<LoggingService>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddSingleton<IYoutubeListDownloadService, YoutubeClientVerDownloadService>();


var config = builder.Configuration; // Use the builder.Configuration directly
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
            var hubPattern = new Regex(@"^/\w+Hub$", RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(accessToken) && hubPattern.IsMatch(path))
            {
                //context.HttpContext.Request.Headers.Add("Authorization", $"Bearer {accessToken}");
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
    //hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(2);
    hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
}).AddJsonProtocol(options => {
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserLevel99", policy =>
        policy.Requirements.Add(new UserLevelRequirement(99)));
});

builder.Services.AddSingleton<IAuthorizationHandler, UserLevelHandler>();
builder.Services.AddHttpClient<LinePayService>();
builder.Services.AddSingleton(new DiscordService(config["Discord:BotID"],config["Discord:Token"]));

var app = builder.Build();

// Ensure required directories exist
var directoriesToCreate = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), "YoutubeDonloadZIP")
};

foreach (var dir in directoriesToCreate)
{
    if (!Directory.Exists(dir))
    {
        Directory.CreateDirectory(dir);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{ 
    app.UseSwagger();
    app.UseSwaggerUI();
}
// 判斷是否為 Linux
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    app.UseHttpsRedirection();
}
app.UseCors(builder =>
        builder
        .WithOrigins("https://localhost:7145", "https://demoprogressbar.moon719096service.uk", "http://127.0.0.1:5174", "http://localhost:5173", "http://localhost:10001")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "YoutubeDonloadZIP")),
    RequestPath = "/YoutubeDonloadZIP"
});

app.MapControllers();

app.MapHub<ChatHub>("/ChatHub");
app.MapHub<YoutubeDownloadProgressHub>("/YoutubeDownloadProgressHub");

app.Run();
