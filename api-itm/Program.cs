using api_itm;
using api_itm.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO.Packaging;

namespace api_itm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var builder = Host.CreateApplicationBuilder();

            builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql("Host=193.231.20.42;Port=5432;Database=direc_restore;Username=usrru;Password=R3sur$e")
             .LogTo(Console.WriteLine) // <--- Log queries to output
             .EnableSensitiveDataLogging()         // includes parameter values
             .EnableDetailedErrors()               // includes full context
  );


            builder.Services.AddScoped<LoginForm>();

            var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();
            Application.Run(loginForm);
        }


        public static class SessionState
        {
            public static TokenStore Tokens { get; set; } = new TokenStore();
        }

    }
}
