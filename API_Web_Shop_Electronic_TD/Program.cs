using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using static API_Web_Shop_Electronic_TD.Mappers.KhachHangsMapper;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Hshop2023Context>(options => {
	options.UseSqlServer(builder.Configuration.GetConnectionString("HShop"));
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IHangHoaRepository, HangHoaRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangsRepository>();
builder.Services.AddScoped<ILoaiSp, LoaiSpRepository>();
builder.Services.AddScoped<IHangSp, HangSpRepository>();
builder.Services.AddScoped<IHoaDon, HoaDonRepository>();
builder.Services.AddScoped<ITrangThaiHd, TrangThaiHdRepository>();
builder.Services.AddScoped<IDanhMuc, DanhMucRepository>();
builder.Services.AddScoped<IDanhGiaSp, DanhGiaSpRepository>();
builder.Services.AddScoped<ICtHoaDon, ChiTietHoaDonRepository>();

builder.Services.AddHttpClient();

// Add CORS configuration
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigin",
		builder =>
		{
			builder.WithOrigins("https://localhost:7038")
				   .AllowAnyHeader()
				   .AllowAnyMethod();
		});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseRouting();

app.UseHttpsRedirection();

// Use CORS before Authorization
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();