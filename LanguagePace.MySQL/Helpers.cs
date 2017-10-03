using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguagePace.MySQL
{
    /// <summary>
    ///     Class containing helper functions for entities.
    /// </summary>
    public class Helpers
    {
        /// <summary>
        ///     Returns list of hydrated objects from a dictionary list. Used for mapping
        ///     from the database returns to entity objects.
        /// </summary>
        /// <typeparam name="T">Type of object you want hydrated</typeparam>
        /// <param name="dictionary">List of dictionary items that make up the data
        ///     being hydrated</param>
        /// <returns></returns>
        public static List<T> Hydrate<T>(List<Dictionary<string, string>> dictionary)
        {
            var objs = new List<T>();

            foreach (var item in dictionary)
            {
                T obj = (T)Activator.CreateInstance(typeof(T));
                foreach(var pi in typeof(T).GetProperties())
                {
                    string value;
                    if (item.TryGetValue(pi.Name, out value))
                    {
                        pi.SetValue(obj, value);
                    }
                }
                // Adding object to the list
                objs.Add(obj);
            }
            return objs;
        }
    }
}
