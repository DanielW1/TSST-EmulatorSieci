using System;
using System.Globalization;
using System.Linq;

namespace NetworkingTools
{
    /// <summary>
    /// Klasa odpowiedzialna za stworzenie znacznika czasowego, używanego przy wyświetlaniu logów
    /// </summary>
    public class Timestamp
    {
        /// <summary>
        /// Funkcja służąca do stworzenia znacznika czasowego w momencie wywołania
        /// </summary>
        /// <returns>Zwraca wartość znacznika czasowego</returns>
        public static string generateTimestamp()
        {
            string time = null;
            time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            return time;
        }

    }
}
