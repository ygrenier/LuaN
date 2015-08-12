using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaUserDataTest
    {
        [Fact]
        public void TestCreate()
        {
            LuaValue v;
            using (var state = new LuaState())
            {
                v = new LuaUserData(state, 123, true);
                Assert.Same(state, v.State);
                Assert.Equal(123, v.Reference);
                v.Dispose();
            }
        }

        [Fact]
        public void TestCall()
        {
            var state = new LuaState();
            Lua l;
            ILuaUserData ud;
            using (l = new Lua(state))
            {
                l.Push(this);

                using (ud = l.ToUserData(1))
                {
                    var ex = Assert.Throws<LuaException>(() => ud.Call(true, null, 1234, "Test"));
                    Assert.Equal("attempt to call a userdata value", ex.Message);

                    l.State.LuaNewTable();
                    using(var mt = l.ToTable(-1))
                    {
                        mt["__call"] = (LuaCFunction)(s=>
                        {
                            Assert.Equal(5, s.LuaGetTop());
                            Assert.Same(this, s.LuaToUserData(1));
                            Assert.Equal(true, s.LuaToBoolean(2));
                            Assert.Equal(true, s.LuaIsNil(3));
                            Assert.Equal(1234, s.LuaToNumber(4));
                            Assert.Equal("Test", s.LuaToString(5));

                            s.LuaPushNil();
                            s.LuaPushBoolean(true);
                            s.LuaPushNumber(123.45);
                            s.LuaPushString("Test");
                            return 4;
                        });
                    }
                    l.State.LuaSetMetatable(1);

                    Assert.Equal(new Object[] { null, true, 123.45, "Test" }, ud.Call(true, null, 1234, "Test"));
                }
            }
        }

        [Fact]
        public void TestCallTyped()
        {
            var state = new LuaState();
            Lua l;
            ILuaUserData ud;
            using (l = new Lua(state))
            {
                l.Push(this);

                using (ud = l.ToUserData(1))
                {
                    var ex = Assert.Throws<LuaException>(() => ud.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) }));
                    Assert.Equal("attempt to call a userdata value", ex.Message);

                    l.State.LuaNewTable();
                    using (var mt = l.ToTable(-1))
                    {
                        mt["__call"] = (LuaCFunction)(s =>
                        {
                            Assert.Equal(5, s.LuaGetTop());
                            Assert.Same(this, s.LuaToUserData(1));
                            Assert.Equal(true, s.LuaToBoolean(2));
                            Assert.Equal(true, s.LuaIsNil(3));
                            Assert.Equal(1234, s.LuaToNumber(4));
                            Assert.Equal("Test", s.LuaToString(5));

                            s.LuaPushNil();
                            s.LuaPushBoolean(true);
                            s.LuaPushNumber(123.45);
                            s.LuaPushString("Test");
                            return 4;
                        });
                    }
                    l.State.LuaSetMetatable(1);

                    Assert.Equal(
                        new Object[] { null, "True", 123 }, 
                        ud.Call(new object[] { true, null, 1234, "Test" }, new Type[] { typeof(String), typeof(String), typeof(int) })
                        );
                }
            }
        }

        [Fact]
        public void TestAccesByField()
        {
            var state = new LuaState();
            Lua l;
            ILuaUserData ud;
            using (l = new Lua(state))
            {
                l.Push(this);

                using (ud = l.ToUserData(1))
                {
                    var ex = Assert.Throws<LuaException>(() => Assert.Null(ud["field1"]));
                    Assert.Equal("attempt to index a userdata value", ex.Message);

                    ex = Assert.Throws<LuaException>(() => ud["field1"] = 1234);
                    Assert.Equal("attempt to index a userdata value", ex.Message);

                    l.State.LuaNewTable();
                    using(var mt = l.ToTable(-1))
                    {
                        l.State.LuaNewTable();
                        using (var idx = l.ToTable(-1))
                        {
                            idx["field1"] = 9876;
                            mt["__index"] = idx;
                            mt["__newindex"] = idx;
                        }
                        l.State.LuaPop(1);
                    }
                    l.State.LuaSetMetatable(1);

                    Assert.Equal(9876d, ud["field1"]);
                    ud["field1"] = 1234d;
                    Assert.Equal(1234d, ud["field1"]);
                }
            }
        }

        [Fact]
        public void TestAccesByObject()
        {
            var state = new LuaState();
            Lua l;
            ILuaUserData ud;
            using (l = new Lua(state))
            {
                l.Push(this);

                using (ud = l.ToUserData(1))
                {
                    double key = 98.76;

                    var ex = Assert.Throws<LuaException>(() => Assert.Null(ud[key]));
                    Assert.Equal("attempt to index a userdata value", ex.Message);

                    ex = Assert.Throws<LuaException>(() => ud[key] = 1234);
                    Assert.Equal("attempt to index a userdata value", ex.Message);

                    l.State.LuaNewTable();
                    using (var mt = l.ToTable(-1))
                    {
                        l.State.LuaNewTable();
                        using (var idx = l.ToTable(-1))
                        {
                            idx[key] = 9876;
                            mt["__index"] = idx;
                            mt["__newindex"] = idx;
                        }
                        l.State.LuaPop(1);
                    }
                    l.State.LuaSetMetatable(1);

                    Assert.Equal(9876d, ud[key]);
                    ud[key] = 1234d;
                    Assert.Equal(1234d, ud[key]);
                }
            }
        }

    }
}
