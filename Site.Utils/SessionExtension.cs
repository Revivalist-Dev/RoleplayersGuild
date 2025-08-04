using Microsoft.AspNetCore.Http;
using Newtonsoft.Json; // You will need the Newtonsoft.Json NuGet package

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
        /// <typeparam name="T">The type of object to store.</typeparam>
        /// <param name="session">The session instance.</param>
        /// <param name="key">The key to store the object under.</param>
        /// <param name="value">The object to store.</param>
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            // Serialize the object to a JSON string and store it in the session.
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Retrieves a complex object from the session by deserializing it from JSON.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="session">The session instance.</param>
        /// <param name="key">The key of the object to retrieve.</param>
        /// <returns>The deserialized object, or the default value for the type (e.g., null) if the key is not found.</returns>
        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            // If the value is null, return the default for the type; otherwise, deserialize it.
            return value is null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}