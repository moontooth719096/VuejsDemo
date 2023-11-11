using DemoProgressBarAPI.Hubs;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

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
        .WithOrigins("https://localhost:44318")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
app.UseAuthorization();

app.MapControllers();
app.MapHub<DemoHub>("/DownloadHub");
app.MapHub<ChatHub>("/ChatHub");

app.Run();
