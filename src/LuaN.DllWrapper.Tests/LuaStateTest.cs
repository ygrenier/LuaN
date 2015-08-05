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

        void PushTestValues(LuaState L, LuaCFunction f = null, Object userData = null)
        {
            f = f ?? (l => 0);
            L.LuaPushNil();
            L.LuaPushNumber(123.45);
            L.LuaPushInteger(987);
            L.LuaPushString("Test");
            L.LuaPushString("5.6");
            L.LuaPushString("5D");
            L.LuaPushString("5z");
            L.LuaPushBoolean(true);
            L.LuaPushCClosure(f, 0);
            // TODO Uncomment when this methods will be implemented
            //L.LuaPushLightUserData(DateTime.Now);
            //L.LuaPushGlobalTable();
            //L.LuaPushThread();
        }

        [Fact]
        public void TestLuaIsNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();
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
            }
        }

        [Fact]
        public void TestLuaIsString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();
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
            }
        }

        [Fact]
        public void TestLuaIsCFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();
                // TODO Add test with Lua script function
                Assert.False(L.LuaIsCFunction(1));
                Assert.False(L.LuaIsCFunction(2));
                Assert.False(L.LuaIsCFunction(3));
                Assert.False(L.LuaIsCFunction(4));
                Assert.False(L.LuaIsCFunction(5));
                Assert.False(L.LuaIsCFunction(6));
                Assert.False(L.LuaIsCFunction(7));
                Assert.False(L.LuaIsCFunction(8));
                Assert.True(L.LuaIsCFunction(9));
                Assert.False(L.LuaIsCFunction(10));
                Assert.False(L.LuaIsCFunction(11));
                Assert.False(L.LuaIsCFunction(12));
            }
        }

        [Fact]
        public void TestLuaIsInteger()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();
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
            }
        }

        [Fact]
        public void TestLuaIsUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();
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
                Assert.False(L.LuaIsUserData(11));
                Assert.False(L.LuaIsUserData(12));
            }
        }

        [Fact]
        public void TestLuaType()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();
                Assert.Equal(LuaType.Nil, L.LuaType(1));
                Assert.Equal(LuaType.Number, L.LuaType(2));
                Assert.Equal(LuaType.Number, L.LuaType(3));
                Assert.Equal(LuaType.String, L.LuaType(4));
                Assert.Equal(LuaType.String, L.LuaType(5));
                Assert.Equal(LuaType.String, L.LuaType(6));
                Assert.Equal(LuaType.String, L.LuaType(7));
                Assert.Equal(LuaType.Boolean, L.LuaType(8));
                Assert.Equal(LuaType.Function, L.LuaType(9));
                Assert.Equal(LuaType.None, L.LuaType(10));
                Assert.Equal(LuaType.None, L.LuaType(11));
                Assert.Equal(LuaType.None, L.LuaType(12));
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
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();

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
            }
        }

        [Fact]
        public void TestLuaToInteger_LuaToIntegerX()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();

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
            }
        }

        [Fact]
        public void TestLuaToBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.LuaPushNil();
                L.LuaPushNumber(123.45);
                L.LuaPushInteger(987);
                L.LuaPushString("Test");
                L.LuaPushString("5.6");
                L.LuaPushString("5D");
                L.LuaPushString("5z");
                L.LuaPushBoolean(true);
                L.LuaPushCClosure(l => 0, 0);
                // TODO Uncomment when this methods will be implemented
                //L.LuaPushLightUserData(DateTime.Now);
                //L.LuaPushGlobalTable();

                Assert.Equal(false, L.LuaToBoolean(1));
                Assert.Equal(true, L.LuaToBoolean(2));
                Assert.Equal(true, L.LuaToBoolean(3));
                Assert.Equal(true, L.LuaToBoolean(4));
                Assert.Equal(true, L.LuaToBoolean(5));
                Assert.Equal(true, L.LuaToBoolean(6));
                Assert.Equal(true, L.LuaToBoolean(7));
                Assert.Equal(true, L.LuaToBoolean(8));
                Assert.Equal(true, L.LuaToBoolean(9));
                Assert.Equal(false, L.LuaToBoolean(10));
                Assert.Equal(false, L.LuaToBoolean(11));
                Assert.Equal(false, L.LuaToBoolean(12));
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
                Assert.Equal(0u, L.LuaRawLen(11));
                Assert.Equal(0u, L.LuaRawLen(12));
            }
        }

        [Fact]
        public void TestToCFunction()
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
                Assert.Same(f, L.LuaToCFunction(9));
                Assert.Equal(null, L.LuaToCFunction(10));
                Assert.Equal(null, L.LuaToCFunction(11));
                Assert.Equal(null, L.LuaToCFunction(12));
            }
        }

        [Fact]
        public void TestToUserData()
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
                //Assert.Equal(dt, L.LuaToUserData(10));
                Assert.Equal(null, L.LuaToUserData(10));
                Assert.Equal(null, L.LuaToUserData(11));
                Assert.Equal(null, L.LuaToUserData(12));

                // TODO uncomment when LuaPushLightUserData() is implemented
                //L.LuaSetTop(0);
                //L.LuaPushLightUserData(this);
                //L.LuaPushLightUserData(dt);
                //L.LuaPushLightUserData(null);
                //L.LuaPushLightUserData(this);
                //Assert.Same(this, L.LuaToUserData(1));
                //Assert.Equal(dt, L.LuaToUserData(2));
                //Assert.Equal(null, L.LuaToUserData(3));
                //Assert.Same(this, L.LuaToUserData(4));

            }
        }

        [Fact]
        public void TestToThread()
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
                //Assert.Same(L, L.LuaToThread(11));
                Assert.Equal(null, L.LuaToThread(11));
                Assert.Equal(null, L.LuaToThread(12));
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
