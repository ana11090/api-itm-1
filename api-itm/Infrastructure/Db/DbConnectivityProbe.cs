using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Db
{
    public static class DbConnectivityProbe
    {
        public static void Run(DbContext db)
        {
            try
            {
                var cx = db.Database.GetDbConnection();

                // Masked connection details for logs
                var b = new NpgsqlConnectionStringBuilder(cx.ConnectionString) { Password = "***" };
                Debug.WriteLine($"[DB:conn] Host={b.Host};Port={b.Port};Db={b.Database};User={b.Username};SSL={b.SslMode};SearchPath={b.SearchPath}");

                cx.Open();
                using (var cmd = cx.CreateCommand())
                {
                    cmd.CommandText = @"select version(),
                                           current_database(),
                                           current_user,
                                           current_setting('search_path'),
                                           inet_server_addr()::text,
                                           inet_server_port();";
                    using var r = cmd.ExecuteReader();
                    r.Read();
                    Debug.WriteLine($"[DB:server] {r.GetString(0)}");
                    Debug.WriteLine($"[DB:ctx] db={r.GetString(1)} user={r.GetString(2)} sp={r.GetString(3)} addr={r.GetString(4)} port={r.GetInt32(5)}");
                }
                cx.Close();
                Debug.WriteLine("[DB:ok] Connection test passed.");
            }
            catch (PostgresException ex)
            {
                Debug.WriteLine($"[DB:FAIL/PG] SQLSTATE={ex.SqlState} | {ex.MessageText}");
                MessageBox.Show($"DB connection failed.\nSQLSTATE: {ex.SqlState}\n{ex.Message}", "Database Error");
                throw;
            }
            catch (NpgsqlException ex)
            {
                Debug.WriteLine($"[DB:FAIL/NPGSQL] {ex.Message}");
                MessageBox.Show($"DB connection failed.\n{ex.Message}", "Database Error");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB:FAIL/GENERIC] {ex}");
                MessageBox.Show(ex.ToString(), "Database Error");
                throw;
            }
        }
    }
}