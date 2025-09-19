using api_itm.Data;
using api_itm.Infrastructure;
using api_itm.Infrastructure.Db;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using api_itm.UserControler.Contracts;
using api_itm.UserControler.Contracts.Cessation___Reactivation;
using api_itm.UserControler.Contracts.Operations;
using api_itm.UserControler.Contracts.Suspended;
using api_itm.UserControler.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql; // <-- ADDED
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

            // Single canonical connection string. Force schema so unqualified names hit public.*
            // var conn = "Host=193.231.20.42;Port=5432;Database=direc;Username=usrru;Password=R3sur$e;Search Path=ru"; //REAL
            var conn = "Host=193.231.20.42;Port=5432;Database=direc_restore;Username=usrru;Password=R3sur$e;Search Path=ru"; // TEST

            // === DI registrations moved to Infrastructure/ServiceRegistration (your "interface" folder)
            // Keep logging on while developing; flip to false for quieter logs.
            builder.Services.AddApiItmServices(conn, devLogging: true);

            App = builder.Build();

            using (var scope = App.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;

                // === Resolve DbContext
                var db = sp.GetRequiredService<AppDbContext>();

                // call function services
                ProbeDb(db);

                // === Ensure tables BEFORE anything queries them (still sync)
                DbIdRagesEmployeesSetup.EnsureAsync(db).GetAwaiter().GetResult();
                DbIdRagesContractsSetup.EnsureAsync(db).GetAwaiter().GetResult();
                DbIdRagesEmployeesModificationsSetup.EnsureAsync(db).GetAwaiter().GetResult();
                DbIdRagesContractsModificationsSetup.EnsureAsync(db).GetAwaiter().GetResult();

                // Quick visibility check (sync, no await)
                var cx = db.Database.GetDbConnection();
                cx.Open();
                using (var cmd = cx.CreateCommand())
                {
                    cmd.CommandText = "select current_setting('search_path'), current_schema(), to_regclass('public.idsreges_salariat')::text;";
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        Debug.WriteLine($"[DB] search_path={r.GetString(0)} | current_schema={r.GetString(1)} | public.idsreges_salariat={(r.IsDBNull(2) ? "NULL" : r.GetString(2))}");
                    }
                }
                cx.Close();

                var session = sp.GetRequiredService<ISessionContext>();
                var loginForm = sp.GetRequiredService<LoginForm>();
                loginForm.Init(session);

                Application.Run(loginForm);
            }
        }

        // === Minimal, self-contained DB check (shows a message and exits if it fails)
        private static void ProbeDb(AppDbContext db)
        {
            try
            {
                var cx = db.Database.GetDbConnection();
                cx.Open();
                using (var cmd = cx.CreateCommand())
                {
                    cmd.CommandText = @"
                        select version(),
                               current_database(),
                               current_user,
                               current_setting('search_path'),
                               inet_server_addr()::text,
                               inet_server_port();";
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        Debug.WriteLine($"[DB:OK] {r.GetString(0)}");
                        Debug.WriteLine($"[DB:CTX] db={r.GetString(1)} | user={r.GetString(2)} | sp={r.GetString(3)} | addr={r.GetString(4)}:{r.GetInt32(5)}");
                    }
                }
                cx.Close();
            }
            catch (PostgresException ex)
            {
                MessageBox.Show($"Database connection failed.\nSQLSTATE: {ex.SqlState}\n{ex.MessageText}", "Database Error");
                Environment.Exit(1);
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Database connection failed.\n{ex.Message}", "Database Error");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Database Error");
                Environment.Exit(1);
            }
        }

        public static class SessionState
        {
            public static TokenStore Tokens { get; set; } = new TokenStore();
        }
    }
}
