using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Authorization;


namespace AuthApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // BuildWebHost(args).Run();
            
            // Initialize the database
            var host = BuildWebHost(args);
            
            using (var scope = host.Services.CreateScope())
            {
                
                var services = scope.ServiceProvider;
                
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    
                    // requires using Microsoft.EntityFrameworkCore;
                    context.Database.Migrate();


                    // requires using Microsoft.Extensions.Configuration
                    var config = host.Services.GetRequiredService<IConfiguration>();
                    var testUserPw = config["SeedUserPW"];

                    // Requires using RazorPagesMovie.Models;
                    SeedData.Initialize(services, testUserPw).Wait();
                    
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
                
                
            }

            // critical aspect in order to start the operation
            host.Run();
            

        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
