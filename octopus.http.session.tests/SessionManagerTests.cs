using NUnit.Framework;
using System.Threading;

namespace octopus.http.session.tests
{
    [TestFixture]
    public class SessionManagerTests
    {
        [Test]
        public void GetSessionManagerTest()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            Assert.NotNull(mgr);
            Assert.AreSame(SessionManager.GetSessionManager(), mgr);
        }

        [Test]
        public void CountTest()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            Assert.AreEqual(mgr.Count, 0);
            mgr.GetSession();
            Assert.AreEqual(mgr.Count, 1);
            mgr.Clear();
            Assert.AreEqual(mgr.Count, 0);
        }

        [Test]
        public void ClearTest()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            mgr.Clear();
            Assert.AreEqual(0, mgr.Count);
            for (int i = 0; i < 50; i++)
            {
                mgr.GetSession();
            }
            Assert.AreEqual(50, mgr.Count);
            mgr.Clear();
            Assert.AreEqual(0, mgr.Count);
        }
        
        
        [Test]
        public void SessionCleanIntervalTest_0()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            mgr.SessionCleanInterval = 5;
            Assert.AreEqual(mgr.SessionCleanInterval, 5);
        }

        [Test]
        public void SessionCleanIntervalTest_1()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            mgr.Clear();
            Assert.AreEqual(mgr.Count, 0);
            mgr.SessionCleanInterval = 1;
            ISession session = mgr.GetSession();
            session.TTL = 0;
            Thread.Sleep(1500);
            Assert.AreEqual(mgr.Count, 0);
        }

        [Test]
        public void DefaultSessionTTLTest()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            ISession s1 = mgr.GetSession();
            mgr.DefaultSessionTTL = s1.TTL + 20;
            ISession s2 = mgr.GetSession();
            Assert.AreEqual(s1.TTL + 20, s2.TTL);
            Assert.AreEqual(s2.TTL, mgr.GetSession().TTL);
            mgr.DefaultSessionTTL = s1.TTL;
            Assert.AreEqual(mgr.DefaultSessionTTL, s1.TTL);
        }

        [Test]
        public void GetSessionTest_0()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            var s = mgr.GetSession();
            Assert.True(s is ISession);
        }

        [Test]
        public void GetSessionTest_1()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            var s = mgr.GetSession();
            var s2 = mgr.GetSession(s.Sid);
            Assert.True(object.ReferenceEquals(s,s2));
        }

        [Test]
        public void RemoveSessionTest_0()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            mgr.Clear();
            Assert.AreEqual(mgr.Count, 0);
            var s = mgr.GetSession();
            Assert.AreEqual(mgr.Count, 1);
            mgr.RemoveSession(s.Sid);
            Assert.AreEqual(mgr.Count,0);
        }

        [Test]
        public void RemoveSessionTest_1()
        {
            SessionManager mgr = SessionManager.GetSessionManager();
            mgr.Clear();
            Assert.AreEqual(mgr.Count, 0);
            var s = mgr.GetSession();
            Assert.AreEqual(mgr.Count, 1);
            mgr.RemoveSession(s);
            Assert.AreEqual(mgr.Count, 0);
        }
    }
}
