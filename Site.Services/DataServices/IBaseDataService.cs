using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public interface IBaseDataService
    {
        Task<T> GetScalarAsync<T>(string sql, object? parameters = null);
        Task<T?> GetRecordAsync<T>(string sql, object? parameters = null);
        Task<IEnumerable<T>> GetRecordsAsync<T>(string sql, object? parameters = null);
        Task<int> ExecuteAsync(string sql, object? parameters = null);
    }
}
