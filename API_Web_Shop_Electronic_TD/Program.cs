using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Repository;
using API_Web_Shop_Electronic_TD.Services;
using API_Web_Shop_Electronic_TD.Services.Momo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using API_Web_Shop_Electronic_TD.Services.Map;
using API_Web_Shop_Electronic_TD.Services.ChatBot;

var builder = WebApplication.CreateBuilder(args);
var JWTSetting = builder.Configuration.GetSection("JWTSetting");

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
		   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
		   .AddEnvironmentVariables();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<OpenStreetMapService>();

builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = @"JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
}); 
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Hshop2023Context>(options => {
	options.UseSqlServer(builder.Configuration.GetConnectionString("HShop"));
});
builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IHangHoaRepository, HangHoaRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangsRepository>();
builder.Services.AddScoped<ILoaiSpRepository, LoaiSpRepository>();
builder.Services.AddScoped<IHangSp, HangSpRepository>();
builder.Services.AddScoped<IHoaDonRepository, HoaDonRepository>();
builder.Services.AddScoped<ITrangThaiHd, TrangThaiHdRepository>();
builder.Services.AddScoped<IDanhMucRepository, DanhMucRepository>();
builder.Services.AddScoped<IDanhGiaSp, DanhGiaSpRepository>();
builder.Services.AddScoped<ICtHoaDon, ChiTietHoaDonRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IWishListRepository, WishListRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IThongKeRepository, ThongKeRepository>();
builder.Services.AddScoped<ICheckOutRepository, CheckOutRepository>();
builder.Services.AddSingleton<IVnPayService, VnPayService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();


builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));

builder.Services.AddHttpClient<IMomoService, MomoService>(client =>
{
	client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddHttpClient();

builder.Services.Configure<OpenAISettings>(
	builder.Configuration.GetSection("OpenAI"));

builder.Services.AddSingleton<OpenAIService>();

builder.Services.Configure<DialogflowSettings>(builder.Configuration.GetSection("Dialogflow"));
builder.Services.AddSingleton<DialogflowService>();


builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.SaveToken = true;
	options.RequireHttpsMetadata = false;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero,
		ValidIssuer = JWTSetting["validIssuer"],
		ValidAudience = JWTSetting["validAudience"],
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSetting.GetSection("securityKey").Value!))
		{

		}
	};
});

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
app.UseCors(options =>
{
	options.AllowAnyHeader();
	options.AllowAnyMethod();
	options.AllowAnyOrigin();
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
//builder.Services.AddSession(options =>
//{
//	options.IdleTimeout = TimeSpan.FromDays(30);
//	options.Cookie.HttpOnly = true;
//	options.Cookie.IsEssential = true;
//});

// Add CORS configuration

//app.UseSession();