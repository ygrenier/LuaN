using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaStateTest
    {

        [Fact]
        public void TestCreate()
        {
            LuaState L;
            using (L = new LuaState())
            {
                Assert.NotEqual(IntPtr.Zero, L.NativeState);
                Assert.Same(LuaEngine.Current, L.Engine);

                Assert.Equal(-1001000, L.FirstPseudoIndex);
                Assert.Equal(-1, L.MultiReturns);
                Assert.Equal(-1001000, L.RegistryIndex);
                Assert.Equal(20, L.MinStack);

                Assert.Equal(503, L.LuaVersion());

            }
            Assert.Throws<ObjectDisposedException>(() => L.NativeState);
        }

        [Fact]
        public void TestLuaNewThread()
        {
            var engine = new LuaEngine();
            LuaState L, C;
            using (L = (LuaState)engine.NewState())
            {
                C = (LuaState)L.LuaNewThread();

                Assert.NotEqual(IntPtr.Zero, C.NativeState);
                Assert.NotEqual(L.NativeState, C.NativeState);
                Assert.Same(engine, C.Engine);
            }
            Assert.Throws<ObjectDisposedException>(() => L.NativeState);
            Assert.Throws<ObjectDisposedException>(() => C.NativeState);
        }

        [Fact]
        public void TestLuaAtPanic()
        {
            using (var L = new LuaState())
            {
                // Test default atpanic
                var lex = Assert.Throws<LuaException>(() => L.LuaError());
                Assert.Equal("Une exception de type 'LuaN.LuaException' a été levée.", lex.Message);
                L.LuaPushString("Test error");
                lex = Assert.Throws<LuaException>(() => L.LuaError());
                Assert.Equal("Test error", lex.Message);

                // Custom atpanic
                var oldAtPanic = L.LuaAtPanic(state =>
                {
                    throw new ApplicationException("Custom at panic");
                });
                Assert.NotNull(oldAtPanic);
                var aex = Assert.Throws<ApplicationException>(() => L.LuaError());
                Assert.Equal("Custom at panic", aex.Message);

                // Test oldPanic function
                lex = Assert.Throws<LuaException>(() => oldAtPanic(null));
                Assert.Equal("Une exception de type 'LuaN.LuaException' a été levée.", lex.Message);

                // Restore the original old panic
                Assert.True(L.RestoreOriginalAtPanic());
                Assert.False(L.RestoreOriginalAtPanic());
                L.SetDefaultAtPanic();


                Assert.Throws<ArgumentNullException>(() => L.LuaAtPanic(null));

            }
        }

        [Fact]
        public void TestWrapCFunction()
        {
            using (var L = new LuaState())
            {
                LuaDll.lua_CFunction nativeFunction = null;
                LuaCFunction cfunction = null;

                Assert.Null(L.WrapFunction(nativeFunction));
                Assert.Null(L.WrapFunction(cfunction));

                cfunction = s => 0;
                nativeFunction = L.WrapFunction(cfunction);

                Assert.Same(nativeFunction, L.WrapFunction(cfunction));
                Assert.Same(cfunction, L.WrapFunction(nativeFunction));

                nativeFunction = s => 0;
                cfunction = L.WrapFunction(nativeFunction);

                Assert.Same(nativeFunction, L.WrapFunction(cfunction));
                Assert.Same(cfunction, L.WrapFunction(nativeFunction));
            }
        }

        [Fact]
        public void TestFindInstance()
        {
            using (var L = new LuaState())
            {
                LuaState C = (LuaState)L.LuaNewThread();
                IntPtr cPtr = C.NativeState;
                C.Dispose();

                LuaCFunction cfunction = state =>
                {
                    Assert.Same(state, L);
                    return 123;
                };
                LuaDll.lua_CFunction nativeFunc = L.WrapFunction(cfunction);
                Assert.Equal(123, nativeFunc(L.NativeState));

                cfunction = state =>
                {
                    Assert.Equal(cPtr, ((LuaState)state).NativeState);
                    return 321;
                };
                nativeFunc = L.WrapFunction(cfunction);
                Assert.Equal(321, nativeFunc(cPtr));

                Assert.Equal(0, nativeFunc(IntPtr.Zero));

            }
        }

        [Fact]
        public void TestLuaAbsIndex()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushString("a");
                L.LuaPushString("b");
                L.LuaPushString("c");

                Assert.Equal(4, L.LuaAbsIndex(0));
                Assert.Equal(1, L.LuaAbsIndex(1));
                Assert.Equal(2, L.LuaAbsIndex(2));
                Assert.Equal(3, L.LuaAbsIndex(3));
                Assert.Equal(4, L.LuaAbsIndex(4));
                Assert.Equal(3, L.LuaAbsIndex(-1));
                Assert.Equal(2, L.LuaAbsIndex(-2));
                Assert.Equal(1, L.LuaAbsIndex(-3));
                Assert.Equal(0, L.LuaAbsIndex(-4));

                Assert.Equal(-1001000, L.LuaAbsIndex(L.RegistryIndex));
            }
        }

        [Fact]
        public void TestLuaGetTop()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                L.LuaPushNumber(1);
                Assert.Equal(1, L.LuaGetTop());
                L.LuaPushNumber(1);
                Assert.Equal(2, L.LuaGetTop());

                L.LuaPop(1);
                Assert.Equal(1, L.LuaGetTop());
                L.LuaPop(1);
                Assert.Equal(0, L.LuaGetTop());
            }
        }

        [Fact]
        public void TestLuaSetTop()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                L.LuaPushNumber(1);
                L.LuaPushNumber(1);
                L.LuaPushNumber(1);
                L.LuaPushNumber(1);
                Assert.Equal(4, L.LuaGetTop());

                L.LuaSetTop(-2);
                Assert.Equal(3, L.LuaGetTop());
                L.LuaSetTop(2);
                Assert.Equal(2, L.LuaGetTop());
            }
        }

        [Fact]
        public void TestLuaPushValue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                L.LuaPushNumber(1);
                L.LuaPushString("Test");
                L.LuaPushNumber(2);
                L.LuaPushString("Text");
                Assert.Equal(4, L.LuaGetTop());

                L.LuaPushValue(-1);
                L.LuaPushValue(2);
                Assert.Equal(6, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal("Test", L.LuaToString(2));
                Assert.Equal(2, L.LuaToNumber(3));
                Assert.Equal("Text", L.LuaToString(4));
                Assert.Equal("Text", L.LuaToString(5));
                Assert.Equal("Test", L.LuaToString(6));

            }
        }

        [Fact]
        public void TestLuaRotate()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                L.LuaPushNumber(1);
                L.LuaPushString("Test");
                L.LuaPushNumber(2);
                L.LuaPushString("Text");
                L.LuaPushNumber(3);
                L.LuaPushString("Toto");
                L.LuaPushNumber(4);
                Assert.Equal(7, L.LuaGetTop());

                L.LuaRotate(2, 1);
                Assert.Equal(7, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal(4, L.LuaToNumber(2));
                Assert.Equal("Test", L.LuaToString(3));
                Assert.Equal(2, L.LuaToNumber(4));
                Assert.Equal("Text", L.LuaToString(5));
                Assert.Equal(3, L.LuaToNumber(6));
                Assert.Equal("Toto", L.LuaToString(7));

                L.LuaRotate(2, 2);
                Assert.Equal(7, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal(3, L.LuaToNumber(2));
                Assert.Equal("Toto", L.LuaToString(3));
                Assert.Equal(4, L.LuaToNumber(4));
                Assert.Equal("Test", L.LuaToString(5));
                Assert.Equal(2, L.LuaToNumber(6));
                Assert.Equal("Text", L.LuaToString(7));

                L.LuaRotate(2, -3);
                Assert.Equal(7, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal("Test", L.LuaToString(2));
                Assert.Equal(2, L.LuaToNumber(3));
                Assert.Equal("Text", L.LuaToString(4));
                Assert.Equal(3, L.LuaToNumber(5));
                Assert.Equal("Toto", L.LuaToString(6));
                Assert.Equal(4, L.LuaToNumber(7));

            }
        }

        [Fact]
        public void TestLuaCopy()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                L.LuaPushNumber(1);
                L.LuaPushString("Test");
                L.LuaPushNumber(2);
                L.LuaPushString("Text");
                L.LuaPushNumber(3);
                L.LuaPushString("Toto");
                L.LuaPushNumber(4);
                Assert.Equal(7, L.LuaGetTop());


                L.LuaCopy(3, 6);
                Assert.Equal(7, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal("Test", L.LuaToString(2));
                Assert.Equal(2, L.LuaToNumber(3));
                Assert.Equal("Text", L.LuaToString(4));
                Assert.Equal(3, L.LuaToNumber(5));
                Assert.Equal(2, L.LuaToNumber(6));
                Assert.Equal(4, L.LuaToNumber(7));

            }
        }

        [Fact]
        public void TestLuaCheckStack()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.LuaGetTop());
                Assert.Equal(true, L.LuaCheckStack(2));
                Assert.Equal(0, L.LuaGetTop());
                Assert.Equal(true, L.LuaCheckStack(35));
                Assert.Equal(false, L.LuaCheckStack(1000000));
            }
        }

        [Fact]
        public void TestLuaXMove()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushString("#1 Line 1");
                L.LuaPushString("#1 Line 2");
                L.LuaPushString("#1 Line 3");
                Assert.Equal(3, L.LuaGetTop());

                using (LuaState L2 = new LuaState())
                {
                    L2.LuaPushString("#2 Line 1");
                    L2.LuaPushString("#2 Line 2");
                    L2.LuaPushString("#2 Line 3");
                    Assert.Equal(3, L2.LuaGetTop());

                    L.LuaXMove(L2, 2);

                    Assert.Equal(1, L.LuaGetTop());
                    Assert.Equal(5, L2.LuaGetTop());

                    Assert.Equal("#1 Line 1", L.LuaToString(1));
                    Assert.Equal(null, L.LuaToString(2));

                    Assert.Equal("#2 Line 1", L2.LuaToString(1));
                    Assert.Equal("#2 Line 2", L2.LuaToString(2));
                    Assert.Equal("#2 Line 3", L2.LuaToString(3));
                    Assert.Equal("#1 Line 2", L2.LuaToString(4));
                    Assert.Equal("#1 Line 3", L2.LuaToString(5));

                }

                Assert.Throws<ArgumentNullException>(() => L.LuaXMove(null, 1));

                var mockLs = new Mock<ILuaState>();
                var ioex = Assert.Throws<InvalidOperationException>(() => L.LuaXMove(mockLs.Object, 1));
                Assert.Equal("The 'to' state is not a supported state.", ioex.Message);
            }
        }

        //[Fact]
        //public void TestInsert()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        Assert.Equal(0, L.GetTop());
        //        L.PushNumber(1);
        //        L.PushString("Test");
        //        L.PushNumber(2);
        //        L.PushString("Text");
        //        L.PushNumber(3);
        //        L.PushString("Toto");
        //        L.PushNumber(4);
        //        Assert.Equal(7, L.GetTop());

        //        L.Insert(2);
        //        Assert.Equal(7, L.GetTop());

        //        Assert.Equal(1, L.ToNumber(1));
        //        Assert.Equal(4, L.ToNumber(2));
        //        Assert.Equal("Test", L.ToString(3));
        //        Assert.Equal(2, L.ToNumber(4));
        //        Assert.Equal("Text", L.ToString(5));
        //        Assert.Equal(3, L.ToNumber(6));
        //        Assert.Equal("Toto", L.ToString(7));
        //    }
        //}

        //[Fact]
        //public void TestRemove()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        Assert.Equal(0, L.GetTop());
        //        L.PushNumber(1);
        //        L.PushString("Test");
        //        L.PushNumber(2);
        //        L.PushString("Text");
        //        L.PushNumber(3);
        //        L.PushString("Toto");
        //        L.PushNumber(4);
        //        Assert.Equal(7, L.GetTop());

        //        L.Remove(2);
        //        Assert.Equal(6, L.GetTop());

        //        Assert.Equal(1, L.ToNumber(1));
        //        Assert.Equal(2, L.ToNumber(2));
        //        Assert.Equal("Text", L.ToString(3));
        //        Assert.Equal(3, L.ToNumber(4));
        //        Assert.Equal("Toto", L.ToString(5));
        //        Assert.Equal(4, L.ToNumber(6));
        //    }
        //}

        //[Fact]
        //public void TestReplace()
        //{
        //    LuaState L = null;
        //    using (L = new LuaState())
        //    {
        //        Assert.Equal(0, L.GetTop());
        //        L.PushNumber(1);
        //        L.PushString("Test");
        //        L.PushNumber(2);
        //        L.PushString("Text");
        //        L.PushNumber(3);
        //        L.PushString("Toto");
        //        L.PushNumber(4);
        //        Assert.Equal(7, L.GetTop());

        //        L.Replace(2);
        //        Assert.Equal(6, L.GetTop());

        //        Assert.Equal(1, L.ToNumber(1));
        //        Assert.Equal(4, L.ToNumber(2));
        //        Assert.Equal(2, L.ToNumber(3));
        //        Assert.Equal("Text", L.ToString(4));
        //        Assert.Equal(3, L.ToNumber(5));
        //        Assert.Equal("Toto", L.ToString(6));
        //    }
        //}

    }
}
