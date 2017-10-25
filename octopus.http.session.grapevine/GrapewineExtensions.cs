using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine.Interfaces.Server;

namespace octopus.http.session.grapevine
{
    /// <summary>
    /// Extension class for grapevine library.
    /// </summary>
    public static class GrapewineSessionExtensions
    {
        /// <summary>
        /// Search session for current request.
        /// First searches cookies then GET and POST variables.  
        /// </summary>
        /// <param name="context">Current http context.</param>
        /// <returns>Session object or null.</returns>
        public static ISession GetSession(this IHttpContext context)
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            ISession session = null;
            
            foreach (System.Net.Cookie item in context.Request.Cookies)
            {
                if (item.Name == mgr.DefaultSessionId && !item.Expired)
                {
                    if(!string.IsNullOrEmpty(item.Value) && !string.IsNullOrWhiteSpace(item.Value)) session = mgr.GetSession(item.Value);
                }
            }

            if (session == null)
            {
                string sid = context.Request.QueryString[mgr.DefaultSessionId];
                ISession tmp = null;
                if (!string.IsNullOrEmpty(sid) && !string.IsNullOrWhiteSpace(sid)) tmp = mgr.GetSession(sid);
                if(context.ValidateClientConnectionHash(tmp))
                {
                    session = tmp;
                }
            }

            return session;           
        }

        /// <summary>
        /// Creates new session and sets session cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ISession CreateSession(this IHttpContext context)
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            ISession session = mgr.GetSession();
            session[clientControlHashKey] = context.GetClientConnectionHash();
            if (!context.Response.ResponseSent)
            {
                var cookie = new System.Net.Cookie(mgr.DefaultSessionId, session.Sid,"/");
                context.Response.Cookies.Add(cookie);
                return session;
            }
            // else headers are sent - throw?
            return null;            
        }

        /// <summary>
        /// Session key for client connection hash.
        /// </summary>
        private static string clientControlHashKey = "EBFCC273-455E-4E95-A345-CC702EBC5020";
        private static System.Security.Cryptography.HashAlgorithm hashAlgorithm = new System.Security.Cryptography.SHA1Managed();

        /// <summary>
        /// Checks whether the connection is from the client that initiated the session.
        /// </summary>
        /// <param name="context">Current context.</param>
        /// <param name="session">Session object.</param>
        /// <returns></returns>
        private static bool ValidateClientConnectionHash(this IHttpContext context, ISession session)
        {
            if (session == null) return false;
            if (!session.ValueHas(clientControlHashKey)) return false;
            var controlString = session.ValueGetAs<string>(clientControlHashKey);
            if (string.IsNullOrEmpty(controlString)) return false;
            return controlString.Equals(context.GetClientConnectionHash());
        }

        /// <summary>
        /// Generates control hash from client ip, hostname and browser info.
        /// </summary>
        /// <param name="context">Current context.</param>
        /// <returns>Hash string.</returns>
        private static string GetClientConnectionHash(this IHttpContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(context.Request.UserHostAddress);
            sb.Append(context.Request.UserHostname);
            sb.Append(context.Request.UserAgent);
            return UTF8Encoding.UTF8.GetString(hashAlgorithm.ComputeHash(UTF8Encoding.UTF8.GetBytes( sb.ToString())));
        }
    }
}
