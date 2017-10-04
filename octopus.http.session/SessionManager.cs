using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace octopus.http.session
{
    /// <summary>
    /// Sessions manager. Manages the sessions.
    /// </summary>
    public class SessionManager
    {
        private ConcurrentDictionary<string, Session> sessions;
        private Timer sessionCleanTimer;
        private int sessionCleanInterval;
        private int defaultSessionTTL = 3600;
        private bool cleanTaskLock = false;

        private static SessionManager _instance;

        static SessionManager()
        {
            _instance = new SessionManager();
        }
    
        private SessionManager()
        {
            sessions = new ConcurrentDictionary<string, Session>();
            SessionCleanInterval = 1800;
        }

        /// <summary>
        /// Returns the instance of the session manager.
        /// </summary>
        /// <returns>SessionManager</returns>
        public static SessionManager GetSessionManager()
        {
            return _instance;
        }

        /// <summary>
        /// Expired sessions clean interval in seconds. Default 1800s.
        /// </summary>
        public int SessionCleanInterval
        {
            get => sessionCleanInterval;
            set {
                sessionCleanInterval = value;
                if (sessionCleanTimer != null)
                {
                    sessionCleanTimer.Enabled = false;
                    sessionCleanTimer.Stop();
                    sessionCleanTimer.Elapsed -= CleanTimer_Elapsed;
                    sessionCleanTimer.Dispose();
                }

                sessionCleanTimer = new Timer(sessionCleanInterval*1000);
                sessionCleanTimer.Elapsed += CleanTimer_Elapsed;
                sessionCleanTimer.AutoReset = true;
                sessionCleanTimer.Enabled = true;
                sessionCleanTimer.Start();
            }
        }

        public int DefaultSessionTTL { get => defaultSessionTTL; set => defaultSessionTTL = value; }

        private void CleanTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (cleanTaskLock == true) return;
                cleanTaskLock = true;
                List<Session> toRemove = new List<Session>();
                foreach (KeyValuePair<string, Session> item in sessions)
                {
                    if (item.Value.Expired) toRemove.Add(item.Value);
                }

                foreach (Session session in toRemove)
                {
                    //Slowly removes expired sessions.
                    Task.WaitAll(Task.Run(() => 
                        {
                            sessions.TryRemove(session.Sid, out Session tmp);
                        })
                    );
                }
            }
            catch(Exception ex){ Console.WriteLine($"Session clean timer error: \n message: {ex.Message} \n stacktrace:{ex.StackTrace}"); } //TODO: log 
            finally
            {
                cleanTaskLock = false;
            }
        }

        /// <summary>
        /// Returns session by session ID.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <returns>Session or null.</returns>
        public ISession GetSession(string sessionId)
        {
            if (string.Empty.Equals(sessionId)) return null;
            if (! sessions.ContainsKey(sessionId)) return null;

            Session session = sessions[sessionId];
            
            if (session.Expired) return null;

            session.ExpirationRefresh();
            return session;
        }
                
        /// <summary>
        /// Creates and returns new session.
        /// </summary>
        /// <returns>Session object.</returns>
        public ISession GetSession()
        {
            Session session = new Session();
            session.TTL = DefaultSessionTTL;
            
            Task.WaitAll(Task.Run(() =>
                {
                    sessions.TryAdd(session.Sid, session);                    
                })
            );
            return GetSession(session.Sid);
        }
       
        /// <summary>
        /// Removes session by session object.
        /// </summary>
        /// <param name="session">Session object to remove.</param>
        public void RemoveSession(ISession session)
        {
            if (session == null) return;
            var tmp = session as Session;
            if(tmp != null) tmp.TTL = -1;
            sessions.TryRemove(session.Sid, out Session rm);                    
        }

        /// <summary>
        /// Removes session by session ID.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        public void RemoveSession(string sessionId)
        {   
            RemoveSession(GetSession(sessionId));
        }
        
        /// <summary>
        /// Returns total sessions count.
        /// </summary>
        public int Count { get => sessions.Count; }

        /// <summary>
        /// Clears all sessions.
        /// </summary>
        public void Clear() { sessions.Clear(); }
    }
}
