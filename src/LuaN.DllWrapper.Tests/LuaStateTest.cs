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
        public void TestGetTop()
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
        public void TestSetTop()
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
        public void TestPushValue()
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

    }
}
