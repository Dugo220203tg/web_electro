using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Models;
using TDProjectMVC.Services;
using TDProjectMVC.Services.Mail;

public class Program // Đã thay đổi từ mặc định thành công khai
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<Hshop2023Context>(options => {
            options.UseSqlServer(builder.Configuration.GetConnectionString("HShop"));
        });
        builder.Services.AddTransient<IMailSender, MailSender>();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(120);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/Khachhang/DangNhap";
            options.AccessDeniedPath = "/AccessDenied";
        });
        //builder.Services.AddSingleton(x => new PaypalClient(
        //      builder.Configuration["PaypalOptions:AppId"],
        //      builder.Configuration["PaypalOptions:AppSecret"],
        //      builder.Configuration["PaypalOptions:Mode"]
        //));

        builder.Services.AddSingleton<IVnPayService, VnPayService>();
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
