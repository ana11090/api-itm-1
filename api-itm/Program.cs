using api_itm.Data;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using api_itm.UserControler.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

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

            // Register the in-memory session as a singleton
            builder.Services.AddSingleton<ISessionContext, SessionContext>();

            // Register Forms and UserControls
            builder.Services.AddScoped<LoginForm>();
            builder.Services.AddScoped<MainForm>();
            builder.Services.AddScoped<ControlerEmployeeView>();

            // 🔧 Build the host AFTER all services are registered
            App = builder.Build();

            using var scope = App.Services.CreateScope();

            // Resolve LoginForm
            var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();

            // Init session
            var session = scope.ServiceProvider.GetRequiredService<ISessionContext>();
            loginForm.Init(session);

            Debug.WriteLine("=== Session just created ===");
            Debug.WriteLine($"SessionId: {session.SessionId}");

            // Start app
            Application.Run(loginForm);
        }

        public static class SessionState
        {
            public static TokenStore Tokens { get; set; } = new TokenStore();
        }
    }
}
