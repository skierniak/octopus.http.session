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
    public static class GrapewineExtensions
    {
        /// <summary>
        /// Search session for current request. Returns session, null or fresh session.
        /// first searches cookies then GET and POST variables.  
        /// </summary>
        /// <param name="context">Current http context.</param>
        /// <param name="autoCreate">When a session does not exist - If is set to true method returns a new session, if is set to false method returns null.</param>
        /// <returns></returns>
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
                if (!string.IsNullOrEmpty(sid) && !string.IsNullOrWhiteSpace(sid)) session = mgr.GetSession(sid);
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
            if (!context.Response.ResponseSent)
            {
                var cookie = new System.Net.Cookie(mgr.DefaultSessionId, session.Sid);
                context.Response.Cookies.Add(cookie);
                return session;
            }
            // else headers are sent - throw?
            return null;            
        }
    }
}
