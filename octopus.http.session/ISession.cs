using System;

namespace octopus.http.session
{
    /// <summary>
    /// Public session interface.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Returns session id.
        /// </summary>
        String Sid { get; }

        /// <summary>
        /// Returns session expiration date time.
        /// </summary>
        DateTime Expiration { get; }
        
        /// <summary>
        /// Returns session time to live in seconds.
        /// </summary>
        int TTL { get; set; }

        /// <summary>
        /// Session data indexer.
        /// </summary>
        /// <param name="valueName">Key. Value name.</param>
        /// <returns>Object or null.</returns>
        object this[string valueName] { get; set; }

        /// <summary>
        /// Returns value as given type.
        /// </summary>
        /// <typeparam name="T">Cast type.</typeparam>
        /// <param name="valueName">Key. Value name.</param>
        /// <returns>Data type or null.</returns>
        T ValueGetAs<T>(string valueName);

        /// <summary>
        /// Checks for the key exists.
        /// </summary>
        /// <param name="valueName">Key. Value name.</param>
        /// <returns>true or false</returns>
        bool ValueHas(string valueName);

        /// <summary>
        /// Removes value from session data.
        /// </summary>
        /// <param name="valueName">Key. Value name.</param>
        void ValueRemove(string valueName);
        
        /// <summary>
        /// Clears all session data.
        /// </summary>
        void Clear();

        /// <summary>
        /// Total session values.
        /// </summary>
        /// <returns>Values count.</returns>
        int Count { get; }
    }
}