using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{

    public partial class LuaStateTest
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
        public void TestWrapKFunction()
        {
            using (var L = new LuaState())
            {
                LuaDll.lua_KFunction nativeKFunction = null;
                LuaKFunction kfunction = null;

                Assert.Null(L.WrapKFunction(kfunction));

                kfunction = (s, u, c) => 0;
                nativeKFunction = L.WrapKFunction(kfunction);

                Assert.Same(nativeKFunction, L.WrapKFunction(kfunction));
            }
        }

        [Fact]
        public void TestWrapReader()
        {
            using (var L = new LuaState())
            {
                LuaDll.lua_Reader nativeReader = null;
                LuaReader reader = null;

                Assert.Null(L.WrapReader(reader));

                reader = (s, b) => null;
                nativeReader = L.WrapReader(reader);

                Assert.Same(nativeReader, L.WrapReader(reader));
            }
        }

        [Fact]
        public void TestWrapWriter()
        {
            using (var L = new LuaState())
            {
                LuaDll.lua_Writer nativeWriter = null;
                LuaWriter writer = null;

                Assert.Null(L.WrapWriter(writer));

                writer = (l, p, u) => 0;
                nativeWriter = L.WrapWriter(writer);

                Assert.Same(nativeWriter, L.WrapWriter(writer));
            }
        }

        [Fact]
        public void TestWrapHook()
        {
            using (var L = new LuaState())
            {
                LuaDll.lua_Hook nativeHook = null;
                LuaHook hook = null;

                Assert.Null(L.WrapHook(nativeHook));
                Assert.Null(L.WrapHook(hook));

                hook = (s,p) => { };
                nativeHook = L.WrapHook(hook);

                Assert.Same(nativeHook, L.WrapHook(hook));
                Assert.Same(hook, L.WrapHook(nativeHook));

                nativeHook = (s, p) => { };
                hook = L.WrapHook(nativeHook);

                Assert.Same(nativeHook, L.WrapHook(hook));
                Assert.Same(hook, L.WrapHook(nativeHook));
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

        void PushTestValues(LuaState L, LuaCFunction f = null, Object userData = null)
        {
            var oldTop = L.LuaGetTop();
            L.DoString(@"
function test1(a,b)
 return a * b
end
");
            L.LuaSetTop(oldTop);

            f = f ?? (l => 0);
            L.LuaPushNil();
            L.LuaPushNumber(123.45);
            L.LuaPushInteger(987);
            L.LuaPushString("Test");
            L.LuaPushString("5.6");
            L.LuaPushString("5D");
            L.LuaPushString("5z");
            L.LuaPushBoolean(true);
            L.LuaGetGlobal("test1");
            L.LuaPushCClosure(f, 0);
            L.LuaNewUserData(20);
            L.LuaPushLightUserData(userData ?? this);
            L.LuaPushGlobalTable();
            L.LuaPushThread();
        }

        [Fact]
        public void TestLuaIsNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsNumber(1));
                Assert.True(L.LuaIsNumber(2));
                Assert.True(L.LuaIsNumber(3));
                Assert.False(L.LuaIsNumber(4));
                Assert.True(L.LuaIsNumber(5));
                Assert.False(L.LuaIsNumber(6));
                Assert.False(L.LuaIsNumber(7));
                Assert.False(L.LuaIsNumber(8));
                Assert.False(L.LuaIsNumber(9));
                Assert.False(L.LuaIsNumber(10));
                Assert.False(L.LuaIsNumber(11));
                Assert.False(L.LuaIsNumber(12));
                Assert.False(L.LuaIsNumber(13));
                Assert.False(L.LuaIsNumber(14));
            }
        }

        [Fact]
        public void TestLuaIsString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsString(1));
                Assert.True(L.LuaIsString(2));
                Assert.True(L.LuaIsString(3));
                Assert.True(L.LuaIsString(4));
                Assert.True(L.LuaIsString(5));
                Assert.True(L.LuaIsString(6));
                Assert.True(L.LuaIsString(7));
                Assert.False(L.LuaIsString(8));
                Assert.False(L.LuaIsString(9));
                Assert.False(L.LuaIsString(10));
                Assert.False(L.LuaIsString(11));
                Assert.False(L.LuaIsString(12));
                Assert.False(L.LuaIsString(13));
                Assert.False(L.LuaIsString(14));
            }
        }

        [Fact]
        public void TestLuaIsCFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsCFunction(1));
                Assert.False(L.LuaIsCFunction(2));
                Assert.False(L.LuaIsCFunction(3));
                Assert.False(L.LuaIsCFunction(4));
                Assert.False(L.LuaIsCFunction(5));
                Assert.False(L.LuaIsCFunction(6));
                Assert.False(L.LuaIsCFunction(7));
                Assert.False(L.LuaIsCFunction(8));
                Assert.False(L.LuaIsCFunction(9));
                Assert.True(L.LuaIsCFunction(10));
                Assert.False(L.LuaIsCFunction(11));
                Assert.False(L.LuaIsCFunction(12));
                Assert.False(L.LuaIsCFunction(13));
                Assert.False(L.LuaIsCFunction(14));
            }
        }

        [Fact]
        public void TestLuaIsInteger()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsInteger(1));
                Assert.False(L.LuaIsInteger(2));
                Assert.True(L.LuaIsInteger(3));
                Assert.False(L.LuaIsInteger(4));
                Assert.False(L.LuaIsInteger(5));
                Assert.False(L.LuaIsInteger(6));
                Assert.False(L.LuaIsInteger(7));
                Assert.False(L.LuaIsInteger(8));
                Assert.False(L.LuaIsInteger(9));
                Assert.False(L.LuaIsInteger(10));
                Assert.False(L.LuaIsInteger(11));
                Assert.False(L.LuaIsInteger(12));
                Assert.False(L.LuaIsInteger(13));
                Assert.False(L.LuaIsInteger(14));
            }
        }

        [Fact]
        public void TestLuaIsUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsUserData(1));
                Assert.False(L.LuaIsUserData(2));
                Assert.False(L.LuaIsUserData(3));
                Assert.False(L.LuaIsUserData(4));
                Assert.False(L.LuaIsUserData(5));
                Assert.False(L.LuaIsUserData(6));
                Assert.False(L.LuaIsUserData(7));
                Assert.False(L.LuaIsUserData(8));
                Assert.False(L.LuaIsUserData(9));
                Assert.False(L.LuaIsUserData(10));
                Assert.True(L.LuaIsUserData(11));
                Assert.True(L.LuaIsUserData(12));
                Assert.False(L.LuaIsUserData(13));
                Assert.False(L.LuaIsUserData(14));
            }
        }

        [Fact]
        public void TestLuaType()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal(LuaType.Nil, L.LuaType(1));
                Assert.Equal(LuaType.Number, L.LuaType(2));
                Assert.Equal(LuaType.Number, L.LuaType(3));
                Assert.Equal(LuaType.String, L.LuaType(4));
                Assert.Equal(LuaType.String, L.LuaType(5));
                Assert.Equal(LuaType.String, L.LuaType(6));
                Assert.Equal(LuaType.String, L.LuaType(7));
                Assert.Equal(LuaType.Boolean, L.LuaType(8));
                Assert.Equal(LuaType.Function, L.LuaType(9));
                Assert.Equal(LuaType.Function, L.LuaType(10));
                Assert.Equal(LuaType.UserData, L.LuaType(11));
                Assert.Equal(LuaType.LightUserData, L.LuaType(12));
                Assert.Equal(LuaType.Table, L.LuaType(13));
                Assert.Equal(LuaType.Thread, L.LuaType(14));
            }
        }

        [Fact]
        public void TestLuaTypeName()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal("no value", L.LuaTypeName(LuaType.None));
                Assert.Equal("nil", L.LuaTypeName(LuaType.Nil));
                Assert.Equal("boolean", L.LuaTypeName(LuaType.Boolean));
                Assert.Equal("userdata", L.LuaTypeName(LuaType.LightUserData));
                Assert.Equal("number", L.LuaTypeName(LuaType.Number));
                Assert.Equal("string", L.LuaTypeName(LuaType.String));
                Assert.Equal("table", L.LuaTypeName(LuaType.Table));
                Assert.Equal("function", L.LuaTypeName(LuaType.Function));
                Assert.Equal("userdata", L.LuaTypeName(LuaType.UserData));
                Assert.Equal("thread", L.LuaTypeName(LuaType.Thread));
            }
        }

        [Fact]
        public void TestLuaToNumber_LuaToNumberX()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                bool isnum;
                Assert.Equal(0.0, L.LuaToNumber(1));
                Assert.Equal(0.0, L.LuaToNumber(1, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(123.45, L.LuaToNumber(2));
                Assert.Equal(123.45, L.LuaToNumberX(2, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(987.0, L.LuaToNumber(3));
                Assert.Equal(987.0, L.LuaToNumber(3, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(0.0, L.LuaToNumber(4));
                Assert.Equal(0.0, L.LuaToNumber(4, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(5.6, L.LuaToNumber(5));
                Assert.Equal(5.6, L.LuaToNumber(5, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(0.0, L.LuaToNumber(6));
                Assert.Equal(0.0, L.LuaToNumber(7));
                Assert.Equal(0.0, L.LuaToNumber(8));
                Assert.Equal(0.0, L.LuaToNumber(9));
                Assert.Equal(0.0, L.LuaToNumber(10));
                Assert.Equal(0.0, L.LuaToNumber(11));
                Assert.Equal(0.0, L.LuaToNumber(12));
                Assert.Equal(0.0, L.LuaToNumber(13));
                Assert.Equal(0.0, L.LuaToNumber(14));
            }
        }

        [Fact]
        public void TestLuaToInteger_LuaToIntegerX()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                bool isnum;
                Assert.Equal(0, L.LuaToInteger(1));
                Assert.Equal(0, L.LuaToInteger(1, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(0, L.LuaToInteger(2));
                Assert.Equal(0, L.LuaToIntegerX(2, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(987, L.LuaToInteger(3));
                Assert.Equal(987, L.LuaToInteger(3, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(0, L.LuaToInteger(4));
                Assert.Equal(0, L.LuaToInteger(4, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(0, L.LuaToInteger(5));
                Assert.Equal(0, L.LuaToInteger(6));
                Assert.Equal(0, L.LuaToInteger(7));
                Assert.Equal(0, L.LuaToInteger(8));
                Assert.Equal(0, L.LuaToInteger(9));
                Assert.Equal(0, L.LuaToInteger(10));
                Assert.Equal(0, L.LuaToInteger(11));
                Assert.Equal(0, L.LuaToInteger(12));
                Assert.Equal(0, L.LuaToInteger(13));
                Assert.Equal(0, L.LuaToInteger(14));
            }
        }

        [Fact]
        public void TestLuaToBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal(false, L.LuaToBoolean(1));
                Assert.Equal(true, L.LuaToBoolean(2));
                Assert.Equal(true, L.LuaToBoolean(3));
                Assert.Equal(true, L.LuaToBoolean(4));
                Assert.Equal(true, L.LuaToBoolean(5));
                Assert.Equal(true, L.LuaToBoolean(6));
                Assert.Equal(true, L.LuaToBoolean(7));
                Assert.Equal(true, L.LuaToBoolean(8));
                Assert.Equal(true, L.LuaToBoolean(9));
                Assert.Equal(true, L.LuaToBoolean(10));
                Assert.Equal(true, L.LuaToBoolean(11));
                Assert.Equal(true, L.LuaToBoolean(12));
                Assert.Equal(true, L.LuaToBoolean(13));
                Assert.Equal(true, L.LuaToBoolean(14));
            }
        }

        [Fact]
        public void TestLuaToString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal(null, L.LuaToString(1));
                Assert.Equal("123.45", L.LuaToString(2));
                Assert.Equal("987", L.LuaToString(3));
                Assert.Equal("Test", L.LuaToString(4));
                Assert.Equal("5.6", L.LuaToString(5));
                Assert.Equal("5D", L.LuaToString(6));
                Assert.Equal("5z", L.LuaToString(7));
                Assert.Equal(null, L.LuaToString(8));
                Assert.Equal(null, L.LuaToString(9));
                Assert.Equal(null, L.LuaToString(10));
                Assert.Equal(null, L.LuaToString(11));
                Assert.Equal(null, L.LuaToString(12));
                Assert.Equal(null, L.LuaToString(13));
                Assert.Equal(null, L.LuaToString(14));
            }
        }

        [Fact]
        public void TestLuaRawLen()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal(0u, L.LuaRawLen(1));
                Assert.Equal(0u, L.LuaRawLen(2));
                Assert.Equal(0u, L.LuaRawLen(3));
                Assert.Equal(4u, L.LuaRawLen(4));
                Assert.Equal(3u, L.LuaRawLen(5));
                Assert.Equal(2u, L.LuaRawLen(6));
                Assert.Equal(2u, L.LuaRawLen(7));
                Assert.Equal(0u, L.LuaRawLen(8));
                Assert.Equal(0u, L.LuaRawLen(9));
                Assert.Equal(0u, L.LuaRawLen(10));
                Assert.Equal(20u, L.LuaRawLen(11));
                Assert.Equal(0u, L.LuaRawLen(12));
                Assert.Equal(0u, L.LuaRawLen(13));
                Assert.Equal(0u, L.LuaRawLen(14));
            }
        }

        [Fact]
        public void TestLuaToCFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaCFunction f = l => 0;
                PushTestValues(L, f);

                Assert.Equal(null, L.LuaToCFunction(1));
                Assert.Equal(null, L.LuaToCFunction(2));
                Assert.Equal(null, L.LuaToCFunction(3));
                Assert.Equal(null, L.LuaToCFunction(4));
                Assert.Equal(null, L.LuaToCFunction(5));
                Assert.Equal(null, L.LuaToCFunction(6));
                Assert.Equal(null, L.LuaToCFunction(7));
                Assert.Equal(null, L.LuaToCFunction(8));
                Assert.Equal(null, L.LuaToCFunction(9));
                Assert.Same(f, L.LuaToCFunction(10));
                Assert.Equal(null, L.LuaToCFunction(11));
                Assert.Equal(null, L.LuaToCFunction(12));
                Assert.Equal(null, L.LuaToCFunction(13));
                Assert.Equal(null, L.LuaToCFunction(14));
            }
        }

        [Fact]
        public void TestLuaToUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                DateTime dt = DateTime.Now;
                LuaCFunction f = l => 0;
                PushTestValues(L, f, dt);
                
                Assert.Equal(null, L.LuaToUserData(1));
                Assert.Equal(null, L.LuaToUserData(2));
                Assert.Equal(null, L.LuaToUserData(3));
                Assert.Equal(null, L.LuaToUserData(4));
                Assert.Equal(null, L.LuaToUserData(5));
                Assert.Equal(null, L.LuaToUserData(6));
                Assert.Equal(null, L.LuaToUserData(7));
                Assert.Equal(null, L.LuaToUserData(8));
                Assert.Equal(null, L.LuaToUserData(9));
                Assert.Equal(null, L.LuaToUserData(10));
                var ud = L.LuaToUserData(11);
                Assert.IsAssignableFrom<ILuaNativeUserData>(ud);
                Assert.Equal(20u, ((ILuaNativeUserData)ud).Size);
                Assert.Equal(dt, L.LuaToUserData(12));
                Assert.Equal(null, L.LuaToUserData(13));
                Assert.Equal(null, L.LuaToUserData(14));

                L.LuaSetTop(0);
                L.LuaPushLightUserData(this);
                L.LuaPushLightUserData(dt);
                L.LuaPushLightUserData(null);
                L.LuaPushLightUserData(this);
                Assert.Same(this, L.LuaToUserData(1));
                Assert.Equal(dt, L.LuaToUserData(2));
                Assert.Equal(null, L.LuaToUserData(3));
                Assert.Same(this, L.LuaToUserData(4));

            }
        }

        [Fact]
        public void TestLuaToThread()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.Equal(null, L.LuaToThread(1));
                Assert.Equal(null, L.LuaToThread(2));
                Assert.Equal(null, L.LuaToThread(3));
                Assert.Equal(null, L.LuaToThread(4));
                Assert.Equal(null, L.LuaToThread(5));
                Assert.Equal(null, L.LuaToThread(6));
                Assert.Equal(null, L.LuaToThread(7));
                Assert.Equal(null, L.LuaToThread(8));
                Assert.Equal(null, L.LuaToThread(9));
                Assert.Equal(null, L.LuaToThread(10));
                Assert.Equal(null, L.LuaToThread(11));
                Assert.Equal(null, L.LuaToThread(12));
                Assert.Equal(null, L.LuaToThread(13));
                Assert.Same(L, L.LuaToThread(14));
            }
        }

        [Fact]
        public void TestLuaPushNil()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Nil, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestLuaPushNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(1234);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Number, L.LuaType(-1));
                Assert.Equal(1234, L.LuaToNumber(-1));
            }
        }

        [Fact]
        public void TestLuaPushInteger()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushInteger(1234);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Number, L.LuaType(-1));
                Assert.Equal(1234, L.LuaToInteger(-1));
            }
        }

        [Fact]
        public void TestLuaPushLString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushLString("Text", 3);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.String, L.LuaType(-1));
                Assert.Equal("Tex", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaPushString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushString("Text");
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.String, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestPushFString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushFString("-s-d-", 1234);
                L.LuaPushFString("-%s-", "Text");
                L.LuaPushFString("-%f-", 123.45);
                L.LuaPushFString("-%d-", 987);
                L.LuaPushFString("-%s-%s-", "Str", "Text");
                L.LuaPushFString("-%s-%f-", "Str", 123.45);
                L.LuaPushFString("-%s-%d-", "Str", 987);
                L.LuaPushFString("-%f-%s-", 11.22, "Text");
                L.LuaPushFString("-%f-%f-", 11.22, 123.45);
                L.LuaPushFString("-%f-%d-", 11.22, 987);
                L.LuaPushFString("-%d-%s-", 9988, "Text");
                L.LuaPushFString("-%d-%f-", 9988, 123.45);
                L.LuaPushFString("-%d-%d-", 9988, 987);
                //Assert.Equal(13, L.LuaGetTop());
                Assert.Equal(LuaType.String, L.LuaType(-1));
                Assert.Equal("-s-d-", L.LuaToString(1));
                Assert.Equal("-Text-", L.LuaToString(2));
                Assert.Equal("-123.45-", L.LuaToString(3));
                Assert.Equal("-987-", L.LuaToString(4));
                Assert.Equal("-Str-Text-", L.LuaToString(5));
                Assert.Equal("-Str-123.45-", L.LuaToString(6));
                Assert.Equal("-Str-987-", L.LuaToString(7));
                Assert.Equal("-11.22-Text-", L.LuaToString(8));
                Assert.Equal("-11.22-123.45-", L.LuaToString(9));
                Assert.Equal("-11.22-987-", L.LuaToString(10));
                Assert.Equal("-9988-Text-", L.LuaToString(11));
                Assert.Equal("-9988-123.45-", L.LuaToString(12));
                Assert.Equal("-9988-987-", L.LuaToString(13));
            }
        }

        [Fact]
        public void TestLuaPushCClosure()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaCFunction f = l => 0;
                L.LuaPushCClosure(f, 0);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Function, L.LuaType(-1));
                Assert.Same(f, L.LuaToCFunction(-1));
            }
        }

        [Fact]
        public void TestLuaPushBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushBoolean(true);
                L.LuaPushBoolean(false);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(LuaType.Boolean, L.LuaType(-1));
                Assert.Equal(LuaType.Boolean, L.LuaType(-2));
                Assert.Equal(false, L.LuaToBoolean(-1));
                Assert.Equal(true, L.LuaToBoolean(-2));
            }
        }

        [Fact]
        public void TestLuaPushLightUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushLightUserData(DateTime.Now);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.LightUserData, L.LuaType(-1));

                var dt = DateTime.Now;
                L.LuaSetTop(0);
                L.LuaPushLightUserData(this);
                L.LuaPushLightUserData(dt);
                L.LuaPushLightUserData(null);
                L.LuaPushLightUserData(this);
                Assert.Same(this, L.LuaToUserData(1));
                Assert.Equal(dt, L.LuaToUserData(2));
                Assert.Equal(null, L.LuaToUserData(3));
                Assert.Same(this, L.LuaToUserData(4));
            }
        }

        [Fact]
        public void TestLuaPushThread()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(true, L.LuaPushThread());
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Thread, L.LuaType(-1));

                using (var C = L.LuaNewThread())
                {
                    Assert.Equal(false, C.LuaPushThread());
                    Assert.Equal(1, C.LuaGetTop());
                    Assert.Equal(LuaType.Thread, C.LuaType(-1));
                }

            }
        }

        [Fact]
        public void TestLuaSetGetGlobal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushString("Value A");
                L.LuaSetGlobal("a");
                Assert.Equal(0, L.LuaGetTop());

                Assert.Equal(LuaType.String, L.LuaGetGlobal("a"));
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.String, L.LuaType(-1));

                Assert.Equal(LuaType.Nil, L.LuaGetGlobal("b"));
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(LuaType.Nil, L.LuaType(-1));

            }
        }

        [Fact]
        public void TestLuaSetGetTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushInteger(2);
                L.LuaPushString("Value");
                L.LuaSetTable(1);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushInteger(1);
                Assert.Equal(LuaType.Nil, L.LuaGetTable(1));
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsNil(-1));

                L.LuaPushInteger(2);
                Assert.Equal(LuaType.String, L.LuaGetTable(1));
                Assert.Equal(3, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsString(-1));
                Assert.Equal("Value", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaSetGetField()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushString("Value");
                L.LuaSetField(1, "a");
                Assert.Equal(1, L.LuaGetTop());

                L.LuaGetField(1, "b");
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsNil(-1));

                L.LuaGetField(1, "a");
                Assert.Equal(3, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsString(-1));
                Assert.Equal("Value", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaSetGetI()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushString("Value");
                L.LuaSetI(1, 2);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaGetI(1, 1);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsNil(-1));

                L.LuaGetI(1, 2);
                Assert.Equal(3, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsString(-1));
                Assert.Equal("Value", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaRawSetGet()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushInteger(2);
                L.LuaPushString("Value");
                L.LuaRawSet(1);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushInteger(1);
                L.LuaRawGet(1);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsNil(-1));

                L.LuaPushInteger(2);
                L.LuaRawGet(1);
                Assert.Equal(3, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsString(-1));
                Assert.Equal("Value", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaRawSetGetI()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushString("Value");
                L.LuaRawSetI(1, 2);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaRawGetI(1, 1);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsNil(-1));

                L.LuaRawGetI(1, 2);
                Assert.Equal(3, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsString(-1));
                Assert.Equal("Value", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaRawSetGetP()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());

                L.LuaPushString("Value");
                L.LuaRawSetP(1, this);
                Assert.Equal(1, L.LuaGetTop());

                DateTime dt = DateTime.Now;

                L.LuaRawGetP(1, dt);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsNil(-1));

                L.LuaRawGetP(1, this);
                Assert.Equal(3, L.LuaGetTop());
                Assert.Equal(true, L.LuaIsString(-1));
                Assert.Equal("Value", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaCreateTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Table, L.LuaType(-1));

                L.LuaCreateTable(3, 7);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestLuaNewUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                var ud = L.LuaNewUserData(12);
                Assert.Equal(12u, ud.Size);
                var bytes = ud.GetRawData();
                Assert.NotNull(bytes);
                Assert.Equal(12, bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = (byte)(i * 2);
                ud.SetRawData(bytes);

                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.UserData, L.LuaType(-1));

                L.LuaPushValue(-1);
                var udt = L.LuaToUserData(-1);
                Assert.Same(ud, udt);

            }
        }

        [Fact]
        public void TestLuaGetSetMetatable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                // Create the working table
                L.LuaCreateTable(0, 0);

                // Get the metatable
                Assert.False(L.LuaGetMetatable(1));

                // Create and set the metatable
                L.LuaCreateTable(0, 0);
                L.LuaSetMetatable(1);

                // Get the metatable
                Assert.True(L.LuaGetMetatable(1));
                Assert.Equal(2, L.LuaGetTop());
            }
        }

        [Fact]
        public void TestLuaGetSetUserValue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0, 0);
                Assert.Equal(15, (int)L.LuaGetUserValue(1));

                L.LuaPushString("UserValue");
                L.LuaSetUserValue(1);
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(LuaType.String, L.LuaGetUserValue(1));

                L.LuaSetTop(0);
                var ud = L.LuaNewUserData(12);
                Assert.Equal(LuaType.Nil, L.LuaGetUserValue(1));

                L.LuaPushString("UserValue");
                L.LuaSetUserValue(1);
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(LuaType.String, L.LuaGetUserValue(1));
                Assert.Equal("UserValue", L.LuaToString(-1));

            }
        }

        [Fact]
        public void TestLuaArith()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(12.34);
                L.LuaPushNumber(98.76);
                L.LuaArith(LuaArithOperator.Add);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(111.1, L.LuaToNumber(-1), 2);
                // TODO More tests with metamethods
            }
        }

        [Fact]
        public void TestLuaRawEqual()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(12.34);
                L.LuaPushNumber(98.76);
                L.LuaPushNumber(12.34);
                Assert.False(L.LuaRawEqual(1, 2));
                Assert.False(L.LuaRawEqual(2, 3));
                Assert.True(L.LuaRawEqual(1, 3));
            }
        }

        [Fact]
        public void TestLuaCompare()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(12.34);
                L.LuaPushNumber(98.76);
                L.LuaPushNumber(12.34);
                Assert.Equal(false, L.LuaCompare(1, 2, LuaRelOperator.EQ));
                Assert.Equal(false, L.LuaCompare(2, 3, LuaRelOperator.EQ));
                Assert.Equal(true, L.LuaCompare(1, 3, LuaRelOperator.EQ));
                Assert.Equal(true, L.LuaCompare(1, 2, LuaRelOperator.LE));
                Assert.Equal(false, L.LuaCompare(2, 3, LuaRelOperator.LE));
                Assert.Equal(true, L.LuaCompare(1, 3, LuaRelOperator.LE));
                Assert.Equal(true, L.LuaCompare(1, 2, LuaRelOperator.LT));
                Assert.Equal(false, L.LuaCompare(2, 3, LuaRelOperator.LT));
                Assert.Equal(false, L.LuaCompare(1, 3, LuaRelOperator.LT));
            }
        }

        [Fact]
        public void TestLuaCall()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCClosure(l => {
                    var a = L.LuaToNumber(1);
                    var b = L.LuaToNumber(2);
                    L.LuaPushNumber(a * b);
                    return 1;
                }, 0);
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                L.LuaCall(2, 1);
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(48, L.LuaToNumber(-1));

                L.DoString(@"
function testA(a,b)
 return a-b
end
function testB(a,b)
 DoAnError(a,b)
end
");
                Assert.Equal(1, L.LuaGetTop());
                L.LuaGetGlobal("testA");
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                L.LuaCall(2, 1);
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(8, L.LuaToNumber(-1));

                L.LuaGetGlobal("testB");
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                var ex = Assert.Throws<LuaException>(() => L.LuaCall(2, 1));
                Assert.Equal("[string \"\r...\"]:6: attempt to call a nil value (global 'DoAnError')", ex.Message);
            }
        }

        [Fact]
        public void TestLuaCallK()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCClosure(l => {
                    var a = L.LuaToNumber(1);
                    var b = L.LuaToNumber(2);
                    L.LuaPushNumber(a * b);
                    return 1;
                }, 0);
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                L.LuaCallK(2, 1, 0, null);
            }
        }

        [Fact]
        public void TestLuaPCall()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCClosure(l => {
                    var a = L.LuaToNumber(1);
                    var b = L.LuaToNumber(2);
                    L.LuaPushNumber(a * b);
                    return 1;
                },0);
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                Assert.Equal(LuaStatus.Ok, L.LuaPCall(2, 1, 0));
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(48, L.LuaToNumber(-1));

                L.DoString(@"
function testA(a,b)
 return a-b
end
function testB(a,b)
 DoAnError(a,b)
end
");
                Assert.Equal(1, L.LuaGetTop());
                L.LuaGetGlobal("testA");
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                Assert.Equal(LuaStatus.Ok, L.LuaPCall(2, 1, 0));
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(8, L.LuaToNumber(-1));

                L.LuaGetGlobal("testB");
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                Assert.Equal(LuaStatus.ErrorRun, L.LuaPCall(2, 1, 0));
                Assert.Equal("[string \"\r...\"]:6: attempt to call a nil value (global 'DoAnError')", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaPCallK()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCClosure(l => {
                    var a = L.LuaToNumber(1);
                    var b = L.LuaToNumber(2);
                    L.LuaPushNumber(a * b);
                    return 1;
                }, 0);
                L.LuaPushNumber(12);
                L.LuaPushNumber(4);
                L.LuaPCallK(2, 1, 0, 0, null);
            }
        }

        [Fact]
        public void TestLuaLoad()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                String chunk = @"
a = 'Lua'
b = ' rocks'
c=a..b
return c..'!!'
";
                Byte[] chunkBytes = Encoding.ASCII.GetBytes(chunk);
                int curr = 0;
                LuaReader reader = (l, ud) =>
                {
                    Byte[] res = null;
                    if (curr < chunkBytes.Length)
                    {
                        int c = Math.Min(7, chunkBytes.Length - curr);
                        res = new Byte[c];
                        Array.Copy(chunkBytes, curr, res, 0, c);
                        curr += c;
                    }
                    return res;
                };
                var st = L.LuaLoad(reader, null, "main", null);
                Assert.Equal(LuaStatus.Ok, st);
                st = L.LuaPCall(0, 1, 0);
                Assert.Equal(LuaStatus.Ok, st);
                Assert.Equal("Lua rocks!!", L.LuaToString(-1));

            }
        }

        [Fact]
        public void TestLuaDump()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                String chunk = @"
a = 'Lua'
b = ' rocks'
c=a..b
return c..'!!'
";
                Byte[] chunkBytes = Encoding.ASCII.GetBytes(chunk);
                int curr = 0;
                LuaReader loadChunk = (l, ud) => {
                    Byte[] res = null;
                    if (curr < chunkBytes.Length)
                    {
                        int c = Math.Min(chunkBytes.Length, chunkBytes.Length - curr);
                        res = new Byte[c];
                        Array.Copy(chunkBytes, curr, res, 0, c);
                        curr += c;
                    }
                    return res;
                };
                var st = L.LuaLoad(loadChunk, null, "main", null);
                Assert.Equal(LuaStatus.Ok, st);

                List<Byte[]> dump = new List<byte[]>();
                LuaWriter fDump = (l, b, ud) => {
                    dump.Add(b);
                    return 0;
                };
                var r = L.LuaDump(fDump, null, false);
                Assert.Equal(0, r);
                Byte[] dumpBytes = dump.SelectMany(d => d).ToArray();
                Assert.Equal(199, dumpBytes.Length);

                // Remove the function
                L.LuaPop(1);
                Assert.Equal(0, L.LuaGetTop());

                // Reload chunk compiled
                chunkBytes = dumpBytes;
                curr = 0;
                st = L.LuaLoad(loadChunk, null, "main", "b");
                Assert.Equal(LuaStatus.Ok, st);
                st = L.LuaPCall(0, 1, 0);
                Assert.Equal(LuaStatus.Ok, st);
                Assert.Equal("Lua rocks!!", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaYieldK()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                List<String> output = new List<string>();
                L.OnPrint += (s, e) => output.Add(e.Text);
                L.LuaOpenLibs();
                LuaKFunction testK = (state, status, ctx) =>
                {
                    Assert.Equal(2, state.LuaGetTop());
                    state.LuaPushValue(1);
                    state.LuaPushValue(2);
                    state.LuaArith(LuaArithOperator.Add);
                    Assert.Equal(3, state.LuaGetTop());
                    return 3;
                };
                L.LuaRegister("test", (state) =>
                {
                    Assert.Equal(2, state.LuaGetTop());
                    state.LuaPushValue(1);
                    state.LuaPushValue(2);
                    state.LuaArith(LuaArithOperator.Add);
                    Assert.Equal(3, state.LuaGetTop());
                    state.LuaYieldK(3, 0, testK);
                    return 0;
                });

                Assert.Equal(LuaStatus.Ok, L.DoString(@"
co = coroutine.create(test)
print('1:', coroutine.resume(co, 1, 2))
print('2:', coroutine.resume(co, 5, 8))
print('3:', coroutine.resume(co, 10, 12))
"));

                Assert.Equal(new String[] {
                    "1:	true	1	2	3",
                    "2:	true	5	8",
                    "3:	false	cannot resume dead coroutine"
                }, output);
            }
        }

        [Fact]
        public void TestLuaResume()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(LuaStatus.ErrorRun, L.LuaResume(null, 0));

                Assert.Equal(LuaStatus.ErrorRun, L.LuaResume(L, 0));
            }
        }

        [Fact]
        public void TestLuaStatus()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(LuaStatus.Ok, L.LuaStatus());
            }
        }

        [Fact]
        public void TestLuaIsYieldable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(false, L.LuaIsYieldable());
            }
        }

        [Fact]
        public void TestLuaYield()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                List<String> output = new List<string>();
                L.OnPrint += (s, e) => output.Add(e.Text);
                L.LuaOpenLibs();
                L.LuaRegister("test", (state) =>
                {
                    Assert.Equal(2, state.LuaGetTop());
                    state.LuaPushValue(1);
                    state.LuaPushValue(2);
                    state.LuaArith(LuaArithOperator.Add);
                    Assert.Equal(3, state.LuaGetTop());
                    state.LuaYield(3);
                    return 0;
                });

                Assert.Equal(LuaStatus.Ok, L.DoString(@"
co = coroutine.create(test)
print('1:', coroutine.resume(co, 1, 2))
print('2:', coroutine.resume(co, 5, 8))
print('3:', coroutine.resume(co, 10, 12))
"));

                Assert.Equal(new String[] {
                    "1:	true	1	2	3",
                    "2:	true	5	8",
                    "3:	false	cannot resume dead coroutine"
                }, output);
            }
        }

        [Fact]
        public void TestLuaGC()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(2, L.LuaGC(LuaGcFunction.Count, 0));
            }
        }

        [Fact]
        public void TestLuaError()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushString("Error message");
                var ex = Assert.Throws<LuaException>(() => L.LuaError());
                Assert.Equal("Error message", ex.Message);
            }
        }

        [Fact]
        public void TestLuaNext()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaCreateTable(0,0);
                var t = L.LuaGetTop();
                L.LuaPushString("Value A");
                L.LuaSetField(t, "a");
                L.LuaPushString("Value 2");
                L.LuaSetI(t, 2);
                Assert.Equal(1, L.LuaGetTop());

                int count = 0;
                bool foundA = false;
                // First Key
                L.LuaPushNil();
                while (L.LuaNext(t))
                {
                    count++;
                    // => 'key' at index -2 , 'value' as index -1
                    if (LuaType.String == L.LuaType(-2))
                    {
                        Assert.False(foundA);
                        Assert.Equal(LuaType.String, L.LuaType(-2));
                        Assert.Equal("a", L.LuaToString(-2));
                        Assert.Equal(LuaType.String, L.LuaType(-1));
                        Assert.Equal("Value A", L.LuaToString(-1));
                        foundA = true;
                    }
                    else
                    {
                        // Second key
                        Assert.Equal(LuaType.Number, L.LuaType(-2));
                        Assert.Equal(2, L.LuaToInteger(-2));
                        Assert.Equal(LuaType.String, L.LuaType(-1));
                        Assert.Equal("Value 2", L.LuaToString(-1));
                    }
                    // Remove 'value' and keep the 'key' on the stack for the next key
                    L.LuaPop(1);
                }
                Assert.Equal(2, count);
                Assert.True(foundA);
                // No more key
                // The stack is cleaned
                Assert.Equal(1, L.LuaGetTop());

            }
        }

        [Fact]
        public void TestLuaConcat()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushString("One");
                L.LuaPushNumber(2);
                L.LuaPushString("Three");
                L.LuaPushNumber(4);
                L.LuaConcat(3);
                Assert.Equal("2.0Three4.0", L.LuaToString(-1));
            }
        }

        [Fact]
        public void TestLuaLen()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Exception ex = Assert.Throws<LuaException>(() => L.LuaLen(1));
                Assert.Equal("attempt to get length of a nil value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLen(2));
                Assert.Equal("attempt to get length of a number value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLen(3));
                Assert.Equal("attempt to get length of a number value", ex.Message);
                L.LuaLen(4);
                Assert.Equal(4u, L.LuaToNumber(-1));
                L.LuaLen(5);
                Assert.Equal(3u, L.LuaToNumber(-1));
                L.LuaLen(6);
                Assert.Equal(2u, L.LuaToNumber(-1));
                L.LuaLen(7);
                Assert.Equal(2u, L.LuaToNumber(-1));
                ex = Assert.Throws<LuaException>(() => L.LuaLen(8));
                Assert.Equal("attempt to get length of a boolean value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLen(9));
                Assert.Equal("attempt to get length of a function value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLen(10));
                Assert.Equal("attempt to get length of a function value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLen(11));
                Assert.Equal("attempt to get length of a userdata value", ex.Message);
                ex = Assert.Throws<LuaException>(() => L.LuaLen(12));
                Assert.Equal("attempt to get length of a userdata value", ex.Message);
                L.LuaLen(13);
                Assert.Equal(0u, L.LuaToNumber(-1));
                ex = Assert.Throws<LuaException>(() => L.LuaLen(14));
                Assert.Equal("attempt to get length of a thread value", ex.Message);

                // Test with metamethod __len
                L.LuaNewTable();
                L.LuaPushCFunction(state =>
                {
                    state.LuaPushNumber(1234);
                    return 1;
                });
                L.LuaSetField(-2, "__len");
                L.LuaSetMetatable(11);
                L.LuaLen(11);
                Assert.Equal(1234u, L.LuaToNumber(-1));

            }

        }

        [Fact]
        public void TestLuaStringToNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(4, L.LuaStringToNumber("5.6"));
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(5.6, L.LuaToNumber(-1));

                Assert.Equal(2, L.LuaStringToNumber("5"));
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(5.0, L.LuaToNumber(-1));

                Assert.Equal(0, L.LuaStringToNumber("5D"));
                Assert.Equal(2, L.LuaGetTop());

                Assert.Equal(0, L.LuaStringToNumber("Test"));
                Assert.Equal(2, L.LuaGetTop());

            }
        }

        [Fact]
        public void TestLuaPop()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNumber(1);
                L.LuaPushString("Text");
                L.LuaPushNumber(2);
                L.LuaPushString("Text");
                Assert.Equal(4, L.LuaGetTop());
                L.LuaPop(2);
                Assert.Equal(2, L.LuaGetTop());
                L.LuaPop(1);
                Assert.Equal(1, L.LuaGetTop());
                L.LuaPop(2);
                Assert.Equal(-1, L.LuaGetTop());
            }
        }

        [Fact]
        public void TestLuaNewTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaNewTable();
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Table, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestLuaRegister()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaGetGlobal("a");
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Nil, L.LuaType(-1));

                L.LuaRegister("a", (l) => {
                    l.LuaPushNumber(12.34);
                    return 0;
                });

                L.LuaGetGlobal("a");
                Assert.Equal(2, L.LuaGetTop());
                Assert.Equal(LuaType.Function, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestLuaPushCFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushCFunction((l) => {
                    l.LuaPushNumber(12.34);
                    return 0;
                });
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.Function, L.LuaType(-1));

                Assert.Throws<ArgumentNullException>(() => L.LuaPushCFunction(null));
            }
        }

        [Fact]
        public void TestLuaIsFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsFunction(1));
                Assert.False(L.LuaIsFunction(2));
                Assert.False(L.LuaIsFunction(3));
                Assert.False(L.LuaIsFunction(4));
                Assert.False(L.LuaIsFunction(5));
                Assert.False(L.LuaIsFunction(6));
                Assert.False(L.LuaIsFunction(7));
                Assert.False(L.LuaIsFunction(8));
                Assert.True(L.LuaIsFunction(9));
                Assert.True(L.LuaIsFunction(10));
                Assert.False(L.LuaIsFunction(11));
                Assert.False(L.LuaIsFunction(12));
                Assert.False(L.LuaIsFunction(13));
                Assert.False(L.LuaIsFunction(14));
            }
        }

        [Fact]
        public void TestLuaIsTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsTable(1));
                Assert.False(L.LuaIsTable(2));
                Assert.False(L.LuaIsTable(3));
                Assert.False(L.LuaIsTable(4));
                Assert.False(L.LuaIsTable(5));
                Assert.False(L.LuaIsTable(6));
                Assert.False(L.LuaIsTable(7));
                Assert.False(L.LuaIsTable(8));
                Assert.False(L.LuaIsTable(9));
                Assert.False(L.LuaIsTable(10));
                Assert.False(L.LuaIsTable(11));
                Assert.False(L.LuaIsTable(12));
                Assert.True(L.LuaIsTable(13));
                Assert.False(L.LuaIsTable(14));
            }
        }

        [Fact]
        public void TestLuaIsLightUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsLightUserData(1));
                Assert.False(L.LuaIsLightUserData(2));
                Assert.False(L.LuaIsLightUserData(3));
                Assert.False(L.LuaIsLightUserData(4));
                Assert.False(L.LuaIsLightUserData(5));
                Assert.False(L.LuaIsLightUserData(6));
                Assert.False(L.LuaIsLightUserData(7));
                Assert.False(L.LuaIsLightUserData(8));
                Assert.False(L.LuaIsLightUserData(9));
                Assert.False(L.LuaIsLightUserData(10));
                Assert.False(L.LuaIsLightUserData(11));
                Assert.True(L.LuaIsLightUserData(12));
                Assert.False(L.LuaIsLightUserData(13));
                Assert.False(L.LuaIsLightUserData(14));
            }
        }

        [Fact]
        public void TestLuaIsNil()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.True(L.LuaIsNil(1));
                Assert.False(L.LuaIsNil(2));
                Assert.False(L.LuaIsNil(3));
                Assert.False(L.LuaIsNil(4));
                Assert.False(L.LuaIsNil(5));
                Assert.False(L.LuaIsNil(6));
                Assert.False(L.LuaIsNil(7));
                Assert.False(L.LuaIsNil(8));
                Assert.False(L.LuaIsNil(9));
                Assert.False(L.LuaIsNil(10));
                Assert.False(L.LuaIsNil(11));
                Assert.False(L.LuaIsNil(12));
                Assert.False(L.LuaIsNil(13));
                Assert.False(L.LuaIsNil(14));
            }
        }

        [Fact]
        public void TestLuaIsBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsBoolean(1));
                Assert.False(L.LuaIsBoolean(2));
                Assert.False(L.LuaIsBoolean(3));
                Assert.False(L.LuaIsBoolean(4));
                Assert.False(L.LuaIsBoolean(5));
                Assert.False(L.LuaIsBoolean(6));
                Assert.False(L.LuaIsBoolean(7));
                Assert.True(L.LuaIsBoolean(8));
                Assert.False(L.LuaIsBoolean(9));
                Assert.False(L.LuaIsBoolean(10));
                Assert.False(L.LuaIsBoolean(11));
                Assert.False(L.LuaIsBoolean(12));
                Assert.False(L.LuaIsBoolean(13));
                Assert.False(L.LuaIsBoolean(14));
            }
        }

        [Fact]
        public void TestLuaIsThread()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsThread(1));
                Assert.False(L.LuaIsThread(2));
                Assert.False(L.LuaIsThread(3));
                Assert.False(L.LuaIsThread(4));
                Assert.False(L.LuaIsThread(5));
                Assert.False(L.LuaIsThread(6));
                Assert.False(L.LuaIsThread(7));
                Assert.False(L.LuaIsThread(8));
                Assert.False(L.LuaIsThread(9));
                Assert.False(L.LuaIsThread(10));
                Assert.False(L.LuaIsThread(11));
                Assert.False(L.LuaIsThread(12));
                Assert.False(L.LuaIsThread(13));
                Assert.True(L.LuaIsThread(14));
            }
        }

        [Fact]
        public void TestLuaIsNone()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.False(L.LuaIsNone(1));
                Assert.False(L.LuaIsNone(2));
                Assert.False(L.LuaIsNone(3));
                Assert.False(L.LuaIsNone(4));
                Assert.False(L.LuaIsNone(5));
                Assert.False(L.LuaIsNone(6));
                Assert.False(L.LuaIsNone(7));
                Assert.False(L.LuaIsNone(8));
                Assert.False(L.LuaIsNone(9));
                Assert.False(L.LuaIsNone(10));
                Assert.False(L.LuaIsNone(11));
                Assert.False(L.LuaIsNone(12));
                Assert.False(L.LuaIsNone(13));
                Assert.False(L.LuaIsNone(14));
                Assert.True(L.LuaIsNone(15));
            }
        }

        [Fact]
        public void TestLuaIsNoneOrNil()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                PushTestValues(L);

                Assert.True(L.LuaIsNoneOrNil(1));
                Assert.False(L.LuaIsNoneOrNil(2));
                Assert.False(L.LuaIsNoneOrNil(3));
                Assert.False(L.LuaIsNoneOrNil(4));
                Assert.False(L.LuaIsNoneOrNil(5));
                Assert.False(L.LuaIsNoneOrNil(6));
                Assert.False(L.LuaIsNoneOrNil(7));
                Assert.False(L.LuaIsNoneOrNil(8));
                Assert.False(L.LuaIsNoneOrNil(9));
                Assert.False(L.LuaIsNoneOrNil(10));
                Assert.False(L.LuaIsNoneOrNil(11));
                Assert.False(L.LuaIsNoneOrNil(12));
                Assert.False(L.LuaIsNoneOrNil(13));
                Assert.False(L.LuaIsNoneOrNil(14));
                Assert.True(L.LuaIsNoneOrNil(15));
            }
        }

        [Fact]
        public void TestLuaPushLiteral()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushLiteral("Test");
                Assert.Equal(1, L.LuaGetTop());
                Assert.Equal(LuaType.String, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestLuaPushGlobalTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushGlobalTable();
                Assert.Equal(LuaType.Table, L.LuaType(-1));
            }
        }

        [Fact]
        public void TestLuaInsert()
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

                L.LuaInsert(2);
                Assert.Equal(7, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal(4, L.LuaToNumber(2));
                Assert.Equal("Test", L.LuaToString(3));
                Assert.Equal(2, L.LuaToNumber(4));
                Assert.Equal("Text", L.LuaToString(5));
                Assert.Equal(3, L.LuaToNumber(6));
                Assert.Equal("Toto", L.LuaToString(7));
            }
        }

        [Fact]
        public void TestLuaRemove()
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

                L.LuaRemove(2);
                Assert.Equal(6, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal(2, L.LuaToNumber(2));
                Assert.Equal("Text", L.LuaToString(3));
                Assert.Equal(3, L.LuaToNumber(4));
                Assert.Equal("Toto", L.LuaToString(5));
                Assert.Equal(4, L.LuaToNumber(6));
            }
        }

        [Fact]
        public void TestLuaReplace()
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

                L.LuaReplace(2);
                Assert.Equal(6, L.LuaGetTop());

                Assert.Equal(1, L.LuaToNumber(1));
                Assert.Equal(4, L.LuaToNumber(2));
                Assert.Equal(2, L.LuaToNumber(3));
                Assert.Equal("Text", L.LuaToString(4));
                Assert.Equal(3, L.LuaToNumber(5));
                Assert.Equal("Toto", L.LuaToString(6));
            }
        }

        [Fact]
        public void TestPushUnsigned()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushUnsigned(23);
                Assert.Equal(23, L.LuaToInteger(-1));
            }
        }

        [Fact]
        public void TestLuaToUnsigned_LuaToUnsignedX()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushUnsigned(23);
                Assert.Equal(23u, L.LuaToUnsigned(-1));
                bool isnum;
                Assert.Equal(23u, L.LuaToUnsignedX(-1, out isnum));
                Assert.True(isnum);
                Assert.Equal(23u, L.LuaToUnsigned(-1, out isnum));
                Assert.True(isnum);
            }
        }

    }
}
