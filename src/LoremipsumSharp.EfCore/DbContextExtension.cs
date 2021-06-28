using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Linq;

namespace LoremipsumSharp.EfCore
{
    public static class DbContextExtension
    {
        public static async Task<Page<T>> PagingBySqlAsync<T>(this DbContext dbContext, string sql, int pageIndex, int pageSize, string orderBy)
        {
            var db = dbContext.Database.GetDbConnection();
            var countSql = $"select count(1) from ({sql}) t";
            var count = await db.ExecuteScalarAsync<long>(countSql);

            var orderByStr = string.IsNullOrWhiteSpace(orderBy) ? "" : $"order by {orderBy}";
            var pagerSql = $"{sql} {orderByStr} limit {(pageIndex - 1) * pageSize},{pageSize}";
            var items = await db.QueryAsync<T>(pagerSql);

            return new Page<T>() { Items = items.ToList(), TotalCount = count };
        }
    }
}