using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    partial class LuaStateTest
    {

        [Fact]
        public void TestNewLuaDebug()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                using (var db = L.NewLuaDebug())
                {
                    Assert.NotNull(db);
                    Assert.Null(db.Name);
                    Assert.Null(db.What);
                    Assert.Null(db.NameWhat);
                    Assert.Null(db.ShortSource);
                    Assert.Null(db.Source);
                    Assert.Equal(-1, db.CurrentLine);
                    Assert.Equal(LuaHookEvent.HookCall, db.Event);
                    Assert.Equal(-1, db.LineDefined);
                    Assert.Equal(-1, db.LastLineDefined);
                    Assert.Equal(false, db.IsTailCall);
                    Assert.Equal(false, db.IsVarArg);
                    Assert.Equal(0, db.NParams);
                    Assert.Equal(0, db.NUps);
                }
            }
        }

        [Fact]
        public void TestLuaGetStack()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        Assert.True(l.LuaGetStack(0, ar));

                        Assert.Null(ar.Name);
                        Assert.Null(ar.What);
                        Assert.Null(ar.NameWhat);
                        Assert.Null(ar.ShortSource);
                        Assert.Null(ar.Source);
                        Assert.Equal(-1, ar.CurrentLine);
                        Assert.NotEqual(LuaHookEvent.HookCall, ar.Event);
                        Assert.Equal(-1, ar.LineDefined);
                        Assert.Equal(-1, ar.LastLineDefined);
                        Assert.Equal(false, ar.IsTailCall);
                        Assert.Equal(false, ar.IsVarArg);
                        Assert.Equal(0, ar.NParams);
                        Assert.Equal(0, ar.NUps);

                        Assert.False(l.LuaGetStack(1, ar));

                        // TODO Add test from lua code
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                Assert.Throws<ArgumentNullException>(() => L.LuaGetStack(0, null));

                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaGetStack(0, mDebug.Object));
            }
        }

        [Fact]
        public void TestLuaGetInfo()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        Assert.True(l.LuaGetStack(0, ar));
                        Assert.True(l.LuaGetInfo(LuaGetInfoWhat.AllFills, ar));

                        Assert.Null(ar.Name);
                        Assert.Equal("C", ar.What);
                        Assert.Equal("", ar.NameWhat);
                        Assert.Equal("[C]", ar.ShortSource);
                        Assert.Equal("=[C]", ar.Source);
                        Assert.Equal(-1, ar.CurrentLine);
                        //Assert.NotEqual(LuaHookEvent.HookCall, ar.Event);
                        Assert.Equal(-1, ar.LineDefined);
                        Assert.Equal(-1, ar.LastLineDefined);
                        Assert.Equal(false, ar.IsTailCall);
                        Assert.Equal(true, ar.IsVarArg);
                        Assert.Equal(0, ar.NParams);
                        Assert.Equal(0, ar.NUps);

                        Assert.Equal(0, l.LuaGetTop());
                        Assert.True(l.LuaGetInfo(LuaGetInfoWhat.AllFills | LuaGetInfoWhat.FromTopOfStack | LuaGetInfoWhat.PushFunction | LuaGetInfoWhat.PushLines, ar));
                        Assert.Equal(1, l.LuaGetTop());
                        Assert.Equal(LuaType.Nil, l.LuaType(-1));

                        // TODO Add test from Lua code
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                Assert.Throws<ArgumentNullException>(() => L.LuaGetInfo(LuaGetInfoWhat.AllFills, null));

                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaGetInfo(LuaGetInfoWhat.AllFills, mDebug.Object));
            }
        }

        [Fact]
        public void TestLuaGetLocal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        L.LuaGetStack(0, ar);
                        Assert.Equal(null, l.LuaGetLocal(ar, 1));
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                // TODO Add test from Lua code

                // TODO Uncomment when the functions are implemented
                //                L.LuaDoString(@"
                //function test(a,b)
                // local c = a + b
                // print('test ', a, b, c)
                //end
                //");
                //// Local names
                //L.LuaGetGlobal("test");
                //Assert.Equal(1, L.LuaGetTop());
                //Assert.Equal(null, L.LuaGetLocal(null, 0));
                //Assert.Equal("a", L.LuaGetLocal(null, 1));
                //Assert.Equal("b", L.LuaGetLocal(null, 2));
                //Assert.Equal(null, L.LuaGetLocal(null, 3));
                //Assert.Equal(null, L.LuaGetLocal(null, 4));
                //Assert.Equal(1, L.LuaGetTop());

                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaGetLocal(mDebug.Object, 0));
            }
        }

        [Fact]
        public void TestLuaSetLocal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction(l => {
                    using (var ar = l.NewLuaDebug())
                    {
                        L.LuaGetStack(0, ar);
                        L.LuaPushNumber(123);
                        Assert.Equal("(*temporary)", l.LuaSetLocal(ar, 1));
                    }
                    return 0;
                });
                L.LuaPCall(0, 0, 0);

                // TODO Add test from lua code


                Assert.Throws<ArgumentNullException>(() => L.LuaSetLocal(null, 0));
                var mDebug = new Mock<ILuaDebug>();
                Assert.Throws<ArgumentException>(() => L.LuaSetLocal(mDebug.Object, 0));

            }
        }

        //[Fact]
        //public void TestGetUpvalue()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        L.PushNumber(23);
        //        L.PushFunction(l => {
        //            Assert.Equal(null, l.GetUpvalue(0, 0));
        //            return 0;
        //        });
        //        L.PCall(0, 0, 0);
        //    }
        //}

        //[Fact]
        //public void TestSetUpvalue()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        L.PushNumber(23);
        //        L.PushFunction(l => {
        //            Assert.Equal(null, l.SetUpvalue(0, 0));
        //            return 0;
        //        });
        //        L.PCall(0, 0, 0);
        //    }
        //}

        //[Fact]
        //public void TestUpvalueId()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        L.PushNumber(23);
        //        L.PushFunction(l => {
        //            Assert.Equal(0, l.UpvalueId(0, 0));
        //            return 0;
        //        });
        //        L.PCall(0, 0, 0);
        //    }
        //}

        //[Fact]
        //public void TestUpvalueJoin()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        L.PushNumber(23);
        //        L.PushFunction(l => {
        //            //TODO Do test
        //            //l.UpvalueJoin(0, 0, 0, 0);
        //            return 0;
        //        });
        //        L.PCall(0, 0, 0);
        //    }
        //}

        //[Fact]
        //public void TestGetSetHook()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        LuaHook hook = (ILuaState l, ILuaDebug ar) => {
        //        };

        //        L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, 1);
        //        Assert.Same(hook, L.GetHook());
        //        Assert.Equal(LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, L.GetHookMask());
        //        Assert.Equal(1, L.GetHookCount());

        //        L.SetHook(hook, LuaHookMask.None, 1);
        //        Assert.Same(null, L.GetHook());
        //        Assert.Equal(LuaHookMask.None, L.GetHookMask());
        //        Assert.Equal(1, L.GetHookCount());

        //    }
        //}

    }
}
