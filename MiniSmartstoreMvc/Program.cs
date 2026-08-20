using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniSmartstoreMvc.Data;
using MiniSmartstoreMvc.Extensions;
using MiniSmartstoreMvc.Models;
using MiniSmartstoreMvc.Services;

var builder = WebApplication.CreateBuilder(args);


var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found."
    );


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


// ===== LƯU Ý: ĐĂNG KÝ CÁC PHƯƠNG THỨC ĐĂNG NHẬP NGOÀI =====
builder.Services.AddExternalLoginProviders(
    builder.Configuration
);
// ===== KẾT THÚC ĐĂNG KÝ CÁC PHƯƠNG THỨC ĐĂNG NHẬP NGOÀI =====


builder.Services.AddControllersWithViews();


builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(60);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});


// ===== LƯU Ý: ĐĂNG KÝ DỊCH VỤ GỬI EMAIL =====
builder.Services.AddScoped<EmailSender>();
// ===== KẾT THÚC ĐĂNG KÝ DỊCH VỤ GỬI EMAIL =====


builder.Services.AddScoped<ProductRuleService>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services =
        scope.ServiceProvider;


    await DbSeeder.SeedRolesAndAdminAsync(
        services
    );


    var productRuleService =
        services.GetRequiredService<ProductRuleService>();


    await productRuleService.ApplyActiveRulesAsync();
}


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler(
        "/Home/Error"
    );

    app.UseHsts();
}


app.UseHttpsRedirection();


app.UseStaticFiles();


app.UseRouting();


app.UseSession();


app.UseAuthentication();


app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);


app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);


app.MapRazorPages();


app.Run();