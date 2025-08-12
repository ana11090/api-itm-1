using api_itm;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace api_itm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var builder = Host.CreateApplicationBuilder();

            // 1️⃣ Configure EF Core with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql("Host=193.231.20.42;Port=5432;Database=direc_restore;Username=usrru;Password=R3sur$e")
                       .LogTo(Console.WriteLine)           // Logs SQL queries
                       .EnableSensitiveDataLogging()       // Show parameters in logs (for dev only)
                       .EnableDetailedErrors()             // More detailed EF errors
            );

            // 2️⃣ Register the in-memory session as a singleton (one per app)
            builder.Services.AddSingleton<ISessionContext, SessionContext>();

            // 3️⃣ Register Forms
            builder.Services.AddScoped<LoginForm>();

            var app = builder.Build();

            using var scope = app.Services.CreateScope();

            // 4️⃣ Resolve LoginForm
            var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();

            // 5️⃣ Give it the session instance
            var session = scope.ServiceProvider.GetRequiredService<ISessionContext>();
            loginForm.Init(session);
             
            Debug.WriteLine("=== Session just created ===");
            Debug.WriteLine($"SessionId: {session.SessionId}"); // will be null at first

            // 6️⃣ Run application
            Application.Run(loginForm);
        }

        // Keep your existing token store for OpenID / API calls
        public static class SessionState
        {
            public static TokenStore Tokens { get; set; } = new TokenStore();
        }
    }
}
