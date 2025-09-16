using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure
{
    public static class DbFactoryExtensions
    {
        // One-liner for queries that return a value
        public static async Task<TResult> WithDb<TResult>(
            this IDbContextFactory<AppDbContext> factory,
            Func<AppDbContext, Task<TResult>> work)
        {
            await using var db = await factory.CreateDbContextAsync();
            return await work(db);
        }

        // One-liner for commands that don't return a value
        public static async Task WithDb(
            this IDbContextFactory<AppDbContext> factory,
            Func<AppDbContext, Task> work)
        {
            await using var db = await factory.CreateDbContextAsync();
            await work(db);
        }

        // If you ever prefer sync creation (no await/using noise)
        public static AppDbContext NewDb(this IDbContextFactory<AppDbContext> factory)
            => factory.CreateDbContext();
    }
}
