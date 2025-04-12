using Foodie.Repositories;


namespace Foodie
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            builder.Services.AddScoped<IcustomerRepository, customerRepository>();
            builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

            builder.Services.AddSession();


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


            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Locality}/{action=Locality}/{id?}");

            app.MapControllers();

            app.Run();
        }
    }
}
