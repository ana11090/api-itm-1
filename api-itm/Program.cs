using api_itm.Data;
using api_itm.Infrastructure;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using api_itm.UserControler.Contracts;
using api_itm.UserControler.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace api_itm
{
    internal static class Program
    {
        public static IHost App { get; private set; }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var builder = Host.CreateApplicationBuilder();

            // Configure EF Core with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql("Host=193.231.20.42;Port=5432;Database=direc_restore;Username=usrru;Password=R3sur$e")
                       .LogTo(Console.WriteLine)
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
            );

            // DI registrations
            builder.Services.AddSingleton<ISessionContext, SessionContext>();
            builder.Services.AddScoped<LoginForm>();
            builder.Services.AddScoped<MainForm>();
            builder.Services.AddScoped<ControlerEmployeeView>();
            builder.Services.AddScoped<ControlerAddContractsView>();

            // Build the host AFTER all services are registered
            App = builder.Build();

            // Create a scope to resolve scoped services
            using (var serviceScope = App.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                // 1) Ensure DB objects exist (no migrations)
                var db = services.GetRequiredService<AppDbContext>();
                DbIdRagesSetup.EnsureAsync(db).GetAwaiter().GetResult();

                // 2) Resolve session + login form
                var session = services.GetRequiredService<ISessionContext>();
                var loginForm = services.GetRequiredService<LoginForm>();
                loginForm.Init(session);

                Debug.WriteLine("=== Session just created ===");
                Debug.WriteLine($"SessionId: {session.SessionId}");

                // 3) Start WinForms app
                Application.Run(loginForm);
            }
        }

        public static class SessionState
        {
            public static TokenStore Tokens { get; set; } = new TokenStore();
        }
    }
}
