using DataVidsThumbnails;
using DataVidsThumbnails.Services.Abstract;
using DataVidsThumbnails.Services.Concrete;
using FFMpegCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllersWithViews();

//Video Processing Setup
GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./ffmpeg/bin", TemporaryFilesFolder = "/tmp" });

//install microsoft.extensions.configuration
ConfigurationManager configuration = builder.Configuration;
services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

services.AddScoped<IImageService, ImageService>();
services.AddScoped<ISetupService, SetupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
