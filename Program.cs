// <<<<<<< delivery
// using Microsoft.AspNetCore.Authentication.Cookies;
// =======
// using Foodie.Repositories;
// >>>>>>> master

namespace Foodie
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

// <<<<<<< delivery
//             // Add session services
//             builder.Services.AddDistributedMemoryCache(); // Use in-memory cache for session storage
//             builder.Services.AddSession(options =>
//             {
//                 options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout duration
//                 options.Cookie.HttpOnly = true; // Ensures cookies are accessible only by the server
//                 options.Cookie.IsEssential = true; // Ensures session cookies are essential even if user doesn't accept other cookies
//             });
//             builder.Services.AddHttpContextAccessor();
//             // Add authentication services
//             builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//                 .AddCookie(options =>
//                 {
//                     options.LoginPath = "/deliverysignup/login";  // Redirect to login page if unauthorized
//                     options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Session timeout
//                 });
// =======
//             builder.Services.AddScoped<IcustomerRepository, customerRepository>();
//             builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

//             builder.Services.AddSession();
// >>>>>>> master

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

            // Add session middleware
            app.UseSession();

            app.UseRouting();

// <<<<<<< delivery
//             app.UseAuthentication(); // Ensure authentication middleware comes before authorization
// =======
//             app.UseSession();

// >>>>>>> master
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Locality}/{action=Locality}/{id?}");

            app.MapControllers();

            app.Run();
        }
    }
}
