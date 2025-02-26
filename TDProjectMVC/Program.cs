﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Models;
using TDProjectMVC.Models.MoMo;
using TDProjectMVC.Services.Mail;
using TDProjectMVC.Services.Map;
using TDProjectMVC.Services.Momo;
using TDProjectMVC.Services.PayPal;
using TDProjectMVC.Services.VnPay;

public class Program 
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<Hshop2023Context>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("HShop"));
        });
        builder.Services.AddTransient<IMailSender, MailSender>();   
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IMomoService, MomoService>();
        builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromDays(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddLogging(configure => {
            configure.AddConsole();
            configure.AddDebug();
        });
        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        builder.Services.AddAuthentication(options =>
        {
            // Set default schemes
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/Khachhang/DangNhap";
            options.AccessDeniedPath = "/AccessDenied";
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
            options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
        });

        builder.Services.AddSingleton<IVnPayService, VnPayService>();
        builder.Services.AddSingleton<IPayPalService, PayPalService>();
        builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("Vnpay"));
        builder.Services.AddHttpClient<OpenStreetMapService>();

        //builder.Services.AddSingleton(x => new PaypalClient(
        //      builder.Configuration["PaypalOptions:AppId"],
        //      builder.Configuration["PaypalOptions:AppSecret"],
        //      builder.Configuration["PaypalOptions:Mode"]
        //));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}