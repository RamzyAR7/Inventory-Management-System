using AutoMapper; // Add this using directive at the top of the file
using Microsoft.EntityFrameworkCore;
using IMS.Application.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using IMS.Infrastructure.Context;
using IMS.Infrastructure.UnitOfWork;
using IMS.Application.Services;
using IMS.Application.Mapping;
using IMS.Application.Services.Implementation;
using IMS.Application.Interfaces;
using IMS.Application.SharedServices.Interface;
using IMS.Application.SharedServices.Impelimentation;
using IMS.Application.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContextFactory<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//main service
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IDeliveryManService, DeliveryManService>();

// helper services
builder.Services.AddScoped<IWhoIsUserLoginService, WhoIsUserloginService>();
builder.Services.AddScoped<IProductHelperService, ProductHelperService>();
builder.Services.AddScoped<IOrderHelperService, OrderHelperService>();
builder.Services.AddScoped<IShipmentHelperService, ShipmentHelperService>();
builder.Services.AddScoped<IDashboardUpdateNotifier, DashboardUpdateNotifier>();


// Global services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHttpContextAccessor();

// Add SignalR
builder.Services.AddSignalR();


// register Authentication and Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });
builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<TransactionService>>());


var app = builder.Build();
app.MapHub<DashboardHub>("/dashboardHub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}"); 

app.Run();
