using Microsoft.AspNetCore.Http;
using System.Text.Json; // 1. Using System.Text.Json

namespace RoleplayersGuild.Site.Utils
{
    /// <summary>
    /// Provides extension methods for the ISession interface to allow storing
    /// and retrieving complex objects. This replaces the old SharedSessionObjects class.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Stores a complex object in the session by serializing it to JSON.
        /// </summary>
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            // 2. Replaced JsonConvert.SerializeObject with JsonSerializer.Serialize
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Retrieves a complex object from the session by deserializing it from JSON.
        /// </summary>
        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            // 3. Replaced JsonConvert.DeserializeObject with JsonSerializer.Deserialize
            return value is null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}