using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using octopus.http.session;
using System.Threading;

namespace octopus.http.session.tests
{
    [TestFixture]
    public class SessionTests
    {
        string sid;

        [SetUp]
        public void SetUp()
        {
            ISession s = SessionManager.GetSessionManager().GetSession();
            Console.WriteLine($"SetUp: sid: {s.Sid}. Total sessions: {SessionManager.GetSessionManager().Count}.");
            sid = s.Sid;
            s["int"] = 1;
            s["string"] = "string";
            s["object"] = new object();
            Console.WriteLine($"session ttl:{s.TTL}, exp utc:{s.Expiration}.");
        }
        [Test]
        public void ClearTest()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Assert.IsTrue(3 == s.Count);
            s.Clear();
            Assert.Zero(s.Count);
        }

        [Test]
        public void SessionTTLTest_0()
        {
            ISession s = SessionManager.GetSessionManager().GetSession();
            DateTime d1 = s.Expiration;
            s.TTL = s.TTL - 10;
            DateTime d2 = s.Expiration;
            Assert.False(d1 == d2);
            Assert.True(new DateTime(d1.Ticks - new TimeSpan(0, 0, 10).Ticks) == d2);
            Console.WriteLine($"old ttl:{d1}, new ttl (old - 10s):{new DateTime(d1.Ticks - new TimeSpan(0, 0, 10).Ticks)}");
        }

        [Test]
        public void SessionCount_0()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Console.WriteLine($"SessionCount_0: sid: {s.Sid}. Total sessions: {SessionManager.GetSessionManager().Count}.");
            Assert.NotNull(s);
            Assert.True(s.Count == 3);
            s.Clear();
            Assert.Zero(s.Count);
        }

        [Test]
        public void SessionCount_1()
        {
            Thread.Sleep(100);
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Console.WriteLine($"SessionCount_1: sid: {s.Sid}. Total sessions: {SessionManager.GetSessionManager().Count}.");
            Assert.True(s.Count == 3);
        }

        [Test]
        public void Indexer_0()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            object o1 = s["object"];
            Assert.True(object.ReferenceEquals(o1, s["object"]));
            object o2 = new object();
            s["object"] = o2;
            Assert.True(object.ReferenceEquals(o2, s["object"]));
            Assert.False(object.ReferenceEquals(o2, o1));
        }

        [Test]
        public void Indexer_1()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Assert.AreEqual(1, s["int"]);
            Assert.AreEqual("string", s["string"]);
            s["int"] = 2;
            s["string"] = "string2";
            Assert.AreEqual(2, s["int"]);
            Assert.AreEqual("string2", s["string"]);
        }

        [Test]
        public void Indexer_3()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            String key = "";
            s[key] = 3;
            Assert.IsNull(s[key]);
            Assert.IsNull(s["nokey"]);
        }

        [Test]
        public void GetAsTest_0()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Object o = new object();
            Assert.AreNotSame(s.ValueGetAs<Object>("object"), o);
            s["object"] = o;
            Assert.AreSame(s.ValueGetAs<Object>("object"), o);
        }

        [Test]
        public void GetAsTest_1()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            int i = 1;
            Assert.AreNotSame(s.ValueGetAs<int>("int"), i);
            Assert.AreEqual(s.ValueGetAs<int>("int"), i);
        }

        [Test]
        public void GetAsTest_2()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            String st = "string";
            Assert.AreSame(s.ValueGetAs<string>("string"), st);
            Assert.AreEqual(s.ValueGetAs<string>("string"), st);
        }

        [Test]
        public void GetAsTest_3()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Assert.IsNull(s.ValueGetAs<string>("nokey"));
            Assert.IsNull(s.ValueGetAs<string>("int"));
            Assert.IsNotNull(s.ValueGetAs<object>("int"));
            Assert.IsNotNull(s.ValueGetAs<object>("string"));
        }

        [Test]
        public void ValueHasTest()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Assert.IsTrue(s.ValueHas("int"));
            Assert.IsFalse(s.ValueHas("Int"));
            Assert.IsTrue(s.ValueHas("string"));
            Assert.IsFalse(s.ValueHas("greee"));
            Assert.IsFalse(s.ValueHas(""));
            Assert.IsFalse(s.ValueHas(null));
        }

        [Test]
        public void ValueRemoveTest()
        {
            ISession s = SessionManager.GetSessionManager().GetSession(sid);
            Assert.IsTrue(s.ValueHas("int"));
            s.ValueRemove(null);
            s.ValueRemove("");
            Assert.AreEqual(3, s.Count);
            s.ValueRemove("int");
            Assert.False(s.ValueHas("int"));
            Assert.IsNull(s["int"]);
        }
    }
}
