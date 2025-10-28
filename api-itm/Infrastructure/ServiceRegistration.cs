using api_itm.Infrastructure.Sessions;
using api_itm.UserControler.Contracts;
using api_itm.UserControler.Contracts.Cessation___Reactivation;
using api_itm.UserControler.Contracts.Operations;
using api_itm.UserControler.Contracts.Suspended;
using api_itm.UserControler.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registers EF Core + all WinForms/controls/services used by the app.
        /// </summary>
        public static IServiceCollection AddApiItmServices(
            this IServiceCollection services,
            string connectionString,
            bool devLogging = true)
        {
            // --- DbContext (scoped) — used where you want direct injection of AppDbContext
            services.AddDbContext<AppDbContext>(o =>
            {
                o.UseNpgsql(connectionString);
                if (devLogging)
                {
                    o.LogTo(Console.WriteLine)
                     .EnableSensitiveDataLogging()
                     .EnableDetailedErrors();
                }
            });

            // --- DbContextFactory — preferred for long-lived UI (forms/controls)
            services.AddDbContextFactory<AppDbContext>(o =>
            {
                o.UseNpgsql(connectionString);
                if (devLogging)
                {
                    o.LogTo(Console.WriteLine)
                     .EnableSensitiveDataLogging()
                     .EnableDetailedErrors();
                }
            });

            // --- App/session services
            services.AddSingleton<ISessionContext, SessionContext>();

            // --- Forms (scoped so you resolve them inside a scope)
            services.AddScoped<LoginForm>();
            services.AddScoped<MainForm>();

            // --- UserControls (transient is usually best)
            services.AddTransient<ControlerAddEmployeeView>();
            services.AddTransient<ControlerModifyEmployeeView>();
            services.AddTransient<ControlerDeleteEmployeeView>();//ControlerDeleteEmployeeView
            services.AddTransient<ControlerrCorrectionContractsView>();//ControlerrCorrectionContractsView
            services.AddTransient<ControlerCorrectionEmployeeView>();//ControlerCorrectionEmployeeView
            services.AddTransient<ControlerAddContractsView>();
            services.AddTransient<ControlerModificationContractsView>();
            services.AddTransient<ControlerTerminationContractsView>();
                //Suspendari Contracte
            services.AddTransient<ControlerSuspendedContractsView>();
            services.AddTransient<ControlerCorrectionSuspendedContractsView>();
            services.AddTransient<ControlerModificationSuspendedContractsView>();
            services.AddTransient<ControlerCancelSuspendedContractsView>(); 
            services.AddTransient<ControlerStopedSuspendedContractsView>();
            services.AddTransient<ControlerCorrectionStopedSuspendedContractsView>();


            return services;
        }
    }
}