using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

//Interesting web page: https://www.owasp.org/index.php/Session_Management_Cheat_Sheet

namespace octopus.http.session
{
    /// <summary>
    /// In memory session class.
    /// </summary>
    internal class Session : ISession
    {
        private String sid = Guid.NewGuid().ToString("N");
        private int ttl = 3600;
        private bool expired = false;
        private DateTime expiration = DateTime.UtcNow;
        private ConcurrentDictionary<string, object> data = new ConcurrentDictionary<string, object>();

        public string Sid
        {
            get => sid;            
        }
        
        internal bool Expired
        {
            get
            {
                if (! expired)
                {
                    if (DateTime.Compare(Expiration, DateTime.UtcNow) <= 0)
                    {
                        expired = true;
                    }
                }
                return expired;
            }            
        }
        
        public int TTL
        {
            get => ttl;
            set
            {
                ttl = value;
                ExpirationRefresh();
            }
        }

        /// <summary>
        /// Recalculates and sets session expiration date and time from session ttl. 
        /// </summary>
        /// <param name="session">Session object.</param>
        internal void ExpirationRefresh()
        {
            expiration = DateTime.UtcNow.Add(new TimeSpan(0, 0, TTL));
        }

        public DateTime Expiration
        {
            get => expiration;            
        }        
        
        public Object this[string key]
        {
            get
            {
                if (!ValueHas(key)) return null;
                data.TryGetValue(key, out object value);
                return value;
            }
            set
            {
                if (string.IsNullOrEmpty(key)) return;
                //https://msdn.microsoft.com/pl-pl/library/ee378664(v=vs.110).aspx
                data.AddOrUpdate(key, value, (exKey, exValue) => value);
            }
        }

        public void ValueRemove(string key)
        {            
            if (ValueHas(key))
            {
                data.TryRemove(key, out object value);
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        public T ValueGetAs<T>(string key)
        {
            if (ValueHas(key))
            {
                if(data.TryGetValue(key, out object value))
                { 
                    if (value is T) return (T)value;
                }
            }
            return default(T);
        }

        public bool ValueHas(string valueName)
        {
            if (string.IsNullOrEmpty(valueName)) return false;
            return data.ContainsKey(valueName);
        }

        public int Count { get => data.Count; }
    }
}
