using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;

namespace RoleplayersGuild.Site.Services.DataServices
{
    public abstract class BaseDataService : IBaseDataService
    {
        private readonly string _connectionString;

        protected BaseDataService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        protected IDbConnection GetConnection() => new NpgsqlConnection(_connectionString);

        public async Task<T> GetScalarAsync<T>(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return (await connection.ExecuteScalarAsync<T>(sql, parameters))!;
        }

        public async Task<T?> GetRecordAsync<T>(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> GetRecordsAsync<T>(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync(sql, parameters);
        }
    }
}
