using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaNet.LuaLib.Tests
{
    public partial class LuaStateTest
    {
        [Fact]
        public void TestCreateAndDispose()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(503.0, L.Version());
            }
            Assert.Throws<ObjectDisposedException>(() => L.Version());
        }

        [Fact]
        public void TestAtPanic()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Throws<ArgumentNullException>(() => L.AtPanic(null));

                // Default panic
                L.PushCClosure(l => {
                    L.PushString("Test panic");
                    L.Error();
                    return 0;
                }, 0);
                Exception ex = Assert.Throws<LuaAtPanicException>(() => L.Call(0, 0));
                Assert.Equal("Test panic", ex.Message);

                // user panic
                L.PushCClosure(l => {
                    L.PushString("Test panic");
                    L.Error();
                    return 0;
                }, 0);
                var old = L.AtPanic(l => {
                    throw new ApplicationException(String.Format("Panic !! => {0}", l.ToString(-1)));
                });
                ex = Assert.Throws<ApplicationException>(() => L.Call(0, 0));
                Assert.Equal("Panic !! => Test panic", ex.Message);

                // Restore
                L.AtPanic(old);

                // Restore the default
                Assert.True(L.RestoreOriginalAtPanic());
                Assert.False(L.RestoreOriginalAtPanic());
            }
        }

        [Fact]
        public void TestAbsIndex()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(1)
                 .PushNumber(2)
                 .PushNumber(3);

                Assert.Equal(4, L.AbsIndex(0));
                Assert.Equal(1, L.AbsIndex(1));
                Assert.Equal(2, L.AbsIndex(2));
                Assert.Equal(3, L.AbsIndex(3));
                Assert.Equal(4, L.AbsIndex(4));
                Assert.Equal(3, L.AbsIndex(-1));
                Assert.Equal(2, L.AbsIndex(-2));
                Assert.Equal(1, L.AbsIndex(-3));
                Assert.Equal(0, L.AbsIndex(-4));

            }
        }

        [Fact]
        public void TestGetTop()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                Assert.Equal(1, L.GetTop());
                L.PushNumber(1);
                Assert.Equal(2, L.GetTop());

                L.Pop(1);
                Assert.Equal(1, L.GetTop());
                L.Pop(1);
                Assert.Equal(0, L.GetTop());
            }
        }

        [Fact]
        public void TestSetTop()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushNumber(1);
                L.PushNumber(1);
                L.PushNumber(1);
                Assert.Equal(4, L.GetTop());

                L.SetTop(-2);
                Assert.Equal(3, L.GetTop());
                L.SetTop(2);
                Assert.Equal(2, L.GetTop());
            }
        }

        [Fact]
        public void TestPushValue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushString("Test");
                L.PushNumber(2);
                L.PushString("Text");
                Assert.Equal(4, L.GetTop());

                L.PushValue(-1).PushValue(2);
                Assert.Equal(6, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal("Test", L.ToString(2));
                Assert.Equal(2, L.ToNumber(3));
                Assert.Equal("Text", L.ToString(4));
                Assert.Equal("Text", L.ToString(5));
                Assert.Equal("Test", L.ToString(6));

            }
        }

        [Fact]
        public void TestRotate()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushString("Test");
                L.PushNumber(2);
                L.PushString("Text");
                L.PushNumber(3);
                L.PushString("Toto");
                L.PushNumber(4);
                Assert.Equal(7, L.GetTop());

                L.Rotate(2, 1);
                Assert.Equal(7, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal(4, L.ToNumber(2));
                Assert.Equal("Test", L.ToString(3));
                Assert.Equal(2, L.ToNumber(4));
                Assert.Equal("Text", L.ToString(5));
                Assert.Equal(3, L.ToNumber(6));
                Assert.Equal("Toto", L.ToString(7));

                L.Rotate(2, 2);
                Assert.Equal(7, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal(3, L.ToNumber(2));
                Assert.Equal("Toto", L.ToString(3));
                Assert.Equal(4, L.ToNumber(4));
                Assert.Equal("Test", L.ToString(5));
                Assert.Equal(2, L.ToNumber(6));
                Assert.Equal("Text", L.ToString(7));

                L.Rotate(2, -3);
                Assert.Equal(7, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal("Test", L.ToString(2));
                Assert.Equal(2, L.ToNumber(3));
                Assert.Equal("Text", L.ToString(4));
                Assert.Equal(3, L.ToNumber(5));
                Assert.Equal("Toto", L.ToString(6));
                Assert.Equal(4, L.ToNumber(7));

            }
        }

        [Fact]
        public void TestInsert()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushString("Test");
                L.PushNumber(2);
                L.PushString("Text");
                L.PushNumber(3);
                L.PushString("Toto");
                L.PushNumber(4);
                Assert.Equal(7, L.GetTop());

                L.Insert(2);
                Assert.Equal(7, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal(4, L.ToNumber(2));
                Assert.Equal("Test", L.ToString(3));
                Assert.Equal(2, L.ToNumber(4));
                Assert.Equal("Text", L.ToString(5));
                Assert.Equal(3, L.ToNumber(6));
                Assert.Equal("Toto", L.ToString(7));
            }
        }

        [Fact]
        public void TestRemove()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushString("Test");
                L.PushNumber(2);
                L.PushString("Text");
                L.PushNumber(3);
                L.PushString("Toto");
                L.PushNumber(4);
                Assert.Equal(7, L.GetTop());

                L.Remove(2);
                Assert.Equal(6, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal(2, L.ToNumber(2));
                Assert.Equal("Text", L.ToString(3));
                Assert.Equal(3, L.ToNumber(4));
                Assert.Equal("Toto", L.ToString(5));
                Assert.Equal(4, L.ToNumber(6));
            }
        }

        [Fact]
        public void TestReplace()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushString("Test");
                L.PushNumber(2);
                L.PushString("Text");
                L.PushNumber(3);
                L.PushString("Toto");
                L.PushNumber(4);
                Assert.Equal(7, L.GetTop());

                L.Replace(2);
                Assert.Equal(6, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal(4, L.ToNumber(2));
                Assert.Equal(2, L.ToNumber(3));
                Assert.Equal("Text", L.ToString(4));
                Assert.Equal(3, L.ToNumber(5));
                Assert.Equal("Toto", L.ToString(6));
            }
        }

        [Fact]
        public void TestCopy()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                L.PushNumber(1);
                L.PushString("Test");
                L.PushNumber(2);
                L.PushString("Text");
                L.PushNumber(3);
                L.PushString("Toto");
                L.PushNumber(4);
                Assert.Equal(7, L.GetTop());


                L.Copy(3, 6);
                Assert.Equal(7, L.GetTop());

                Assert.Equal(1, L.ToNumber(1));
                Assert.Equal("Test", L.ToString(2));
                Assert.Equal(2, L.ToNumber(3));
                Assert.Equal("Text", L.ToString(4));
                Assert.Equal(3, L.ToNumber(5));
                Assert.Equal(2, L.ToNumber(6));
                Assert.Equal(4, L.ToNumber(7));

            }
        }

        [Fact]
        public void TestCheckStack()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetTop());
                Assert.Equal(true, L.CheckStack(2));
                Assert.Equal(0, L.GetTop());
                Assert.Equal(true, L.CheckStack(35));
                Assert.Equal(false, L.CheckStack(1000000));
            }
        }

        [Fact]
        public void TestXMove()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushString("#1 Line 1");
                L.PushString("#1 Line 2");
                L.PushString("#1 Line 3");
                Assert.Equal(3, L.GetTop());

                using (LuaState L2 = new LuaState())
                {
                    L2.PushString("#2 Line 1");
                    L2.PushString("#2 Line 2");
                    L2.PushString("#2 Line 3");
                    Assert.Equal(3, L2.GetTop());

                    Assert.Same(L, L.XMove(L2, 2));

                    Assert.Equal(1, L.GetTop());
                    Assert.Equal(5, L2.GetTop());

                    Assert.Equal("#1 Line 1", L.ToString(1));
                    Assert.Equal(null, L.ToString(2));

                    Assert.Equal("#2 Line 1", L2.ToString(1));
                    Assert.Equal("#2 Line 2", L2.ToString(2));
                    Assert.Equal("#2 Line 3", L2.ToString(3));
                    Assert.Equal("#1 Line 2", L2.ToString(4));
                    Assert.Equal("#1 Line 3", L2.ToString(5));

                }

                Assert.Throws<ArgumentNullException>(() => L.XMove(null, 1));

                var mockLs = new Mock<ILuaState>();
                Assert.Throws<InvalidOperationException>(() => L.XMove(mockLs.Object, 1));

            }
        }

        [Fact]
        public void TestIsNil()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.True(L.IsNil(1));
                Assert.False(L.IsNil(2));
                Assert.False(L.IsNil(3));
                Assert.False(L.IsNil(4));
                Assert.False(L.IsNil(5));
                Assert.False(L.IsNil(6));
                Assert.False(L.IsNil(7));
                Assert.False(L.IsNil(8));
                Assert.False(L.IsNil(9));
                Assert.False(L.IsNil(10));
                Assert.False(L.IsNil(11));
                Assert.False(L.IsNil(12));
            }
        }

        [Fact]
        public void TestIsNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsNumber(1));
                Assert.True(L.IsNumber(2));
                Assert.True(L.IsNumber(3));
                Assert.False(L.IsNumber(4));
                Assert.True(L.IsNumber(5));
                Assert.False(L.IsNumber(6));
                Assert.False(L.IsNumber(7));
                Assert.False(L.IsNumber(8));
                Assert.False(L.IsNumber(9));
                Assert.False(L.IsNumber(10));
                Assert.False(L.IsNumber(11));
                Assert.False(L.IsNumber(12));
            }
        }

        [Fact]
        public void TestIsBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsBoolean(1));
                Assert.False(L.IsBoolean(2));
                Assert.False(L.IsBoolean(3));
                Assert.False(L.IsBoolean(4));
                Assert.False(L.IsBoolean(5));
                Assert.False(L.IsBoolean(6));
                Assert.False(L.IsBoolean(7));
                Assert.True(L.IsBoolean(8));
                Assert.False(L.IsBoolean(9));
                Assert.False(L.IsBoolean(10));
                Assert.False(L.IsBoolean(11));
                Assert.False(L.IsBoolean(12));
            }
        }

        [Fact]
        public void TestIsString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsString(1));
                Assert.True(L.IsString(2));
                Assert.True(L.IsString(3));
                Assert.True(L.IsString(4));
                Assert.True(L.IsString(5));
                Assert.True(L.IsString(6));
                Assert.True(L.IsString(7));
                Assert.False(L.IsString(8));
                Assert.False(L.IsString(9));
                Assert.False(L.IsString(10));
                Assert.False(L.IsString(11));
                Assert.False(L.IsString(12));
            }
        }

        [Fact]
        public void TestIsCFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                // TODO Add test with Lua script function
                Assert.False(L.IsCFunction(1));
                Assert.False(L.IsCFunction(2));
                Assert.False(L.IsCFunction(3));
                Assert.False(L.IsCFunction(4));
                Assert.False(L.IsCFunction(5));
                Assert.False(L.IsCFunction(6));
                Assert.False(L.IsCFunction(7));
                Assert.False(L.IsCFunction(8));
                Assert.True(L.IsCFunction(9));
                Assert.False(L.IsCFunction(10));
                Assert.False(L.IsCFunction(11));
                Assert.False(L.IsCFunction(12));
            }
        }

        [Fact]
        public void TestIsFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                // TODO Add test with Lua script function
                Assert.False(L.IsFunction(1));
                Assert.False(L.IsFunction(2));
                Assert.False(L.IsFunction(3));
                Assert.False(L.IsFunction(4));
                Assert.False(L.IsFunction(5));
                Assert.False(L.IsFunction(6));
                Assert.False(L.IsFunction(7));
                Assert.False(L.IsFunction(8));
                Assert.True(L.IsFunction(9));
                Assert.False(L.IsFunction(10));
                Assert.False(L.IsFunction(11));
                Assert.False(L.IsFunction(12));
            }
        }

        [Fact]
        public void TestIsInteger()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsInteger(1));
                Assert.False(L.IsInteger(2));
                Assert.True(L.IsInteger(3));
                Assert.False(L.IsInteger(4));
                Assert.False(L.IsInteger(5));
                Assert.False(L.IsInteger(6));
                Assert.False(L.IsInteger(7));
                Assert.False(L.IsInteger(8));
                Assert.False(L.IsInteger(9));
                Assert.False(L.IsInteger(10));
                Assert.False(L.IsInteger(11));
                Assert.False(L.IsInteger(12));
            }
        }

        [Fact]
        public void TestIsUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsUserData(1));
                Assert.False(L.IsUserData(2));
                Assert.False(L.IsUserData(3));
                Assert.False(L.IsUserData(4));
                Assert.False(L.IsUserData(5));
                Assert.False(L.IsUserData(6));
                Assert.False(L.IsUserData(7));
                Assert.False(L.IsUserData(8));
                Assert.False(L.IsUserData(9));
                Assert.False(L.IsUserData(10));
                Assert.False(L.IsUserData(11));
                Assert.False(L.IsUserData(12));
            }
        }

        [Fact]
        public void TestIsLightUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsLightUserData(1));
                Assert.False(L.IsLightUserData(2));
                Assert.False(L.IsLightUserData(3));
                Assert.False(L.IsLightUserData(4));
                Assert.False(L.IsLightUserData(5));
                Assert.False(L.IsLightUserData(6));
                Assert.False(L.IsLightUserData(7));
                Assert.False(L.IsLightUserData(8));
                Assert.False(L.IsLightUserData(9));
                Assert.False(L.IsLightUserData(10));
                Assert.False(L.IsLightUserData(11));
                Assert.False(L.IsLightUserData(12));
            }
        }

        [Fact]
        public void TestIsTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .NewTable()
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsTable(1));
                Assert.False(L.IsTable(2));
                Assert.False(L.IsTable(3));
                Assert.False(L.IsTable(4));
                Assert.False(L.IsTable(5));
                Assert.False(L.IsTable(6));
                Assert.False(L.IsTable(7));
                Assert.False(L.IsTable(8));
                Assert.False(L.IsTable(9));
                Assert.True(L.IsTable(10));
                Assert.True(L.IsTable(11));
                Assert.False(L.IsTable(12));
            }
        }

        [Fact]
        public void TestIsThread()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .NewTable()
                    .PushGlobalTable()
                    ;
                L.PushThread();
                Assert.False(L.IsThread(1));
                Assert.False(L.IsThread(2));
                Assert.False(L.IsThread(3));
                Assert.False(L.IsThread(4));
                Assert.False(L.IsThread(5));
                Assert.False(L.IsThread(6));
                Assert.False(L.IsThread(7));
                Assert.False(L.IsThread(8));
                Assert.False(L.IsThread(9));
                Assert.False(L.IsThread(10));
                Assert.False(L.IsThread(11));
                Assert.True(L.IsThread(12));
            }
        }

        [Fact]
        public void TestIsNone()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .NewTable()
                    .PushGlobalTable()
                    ;
                Assert.False(L.IsNone(1));
                Assert.False(L.IsNone(2));
                Assert.False(L.IsNone(3));
                Assert.False(L.IsNone(4));
                Assert.False(L.IsNone(5));
                Assert.False(L.IsNone(6));
                Assert.False(L.IsNone(7));
                Assert.False(L.IsNone(8));
                Assert.False(L.IsNone(9));
                Assert.False(L.IsNone(10));
                Assert.False(L.IsNone(11));
                Assert.True(L.IsNone(12));
            }
        }

        [Fact]
        public void TestIsNoneOrNil()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .NewTable()
                    .PushGlobalTable()
                    ;
                Assert.True(L.IsNoneOrNil(1));
                Assert.False(L.IsNoneOrNil(2));
                Assert.False(L.IsNoneOrNil(3));
                Assert.False(L.IsNoneOrNil(4));
                Assert.False(L.IsNoneOrNil(5));
                Assert.False(L.IsNoneOrNil(6));
                Assert.False(L.IsNoneOrNil(7));
                Assert.False(L.IsNoneOrNil(8));
                Assert.False(L.IsNoneOrNil(9));
                Assert.False(L.IsNoneOrNil(10));
                Assert.False(L.IsNoneOrNil(11));
                Assert.True(L.IsNoneOrNil(12));
            }
        }

        [Fact]
        public void TestType()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.Equal(LuaType.Nil, L.Type(1));
                Assert.Equal(LuaType.Number, L.Type(2));
                Assert.Equal(LuaType.Number, L.Type(3));
                Assert.Equal(LuaType.String, L.Type(4));
                Assert.Equal(LuaType.String, L.Type(5));
                Assert.Equal(LuaType.String, L.Type(6));
                Assert.Equal(LuaType.String, L.Type(7));
                Assert.Equal(LuaType.Boolean, L.Type(8));
                Assert.Equal(LuaType.Function, L.Type(9));
                Assert.Equal(LuaType.Table, L.Type(10));
                Assert.Equal(LuaType.None, L.Type(11));
                Assert.Equal(LuaType.None, L.Type(12));
            }
        }

        [Fact]
        public void TestTypeName()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.Equal("nil", L.TypeName(L.Type(1)));
                Assert.Equal("number", L.TypeName(L.Type(2)));
                Assert.Equal("number", L.TypeName(L.Type(3)));
                Assert.Equal("string", L.TypeName(L.Type(4)));
                Assert.Equal("string", L.TypeName(L.Type(5)));
                Assert.Equal("string", L.TypeName(L.Type(6)));
                Assert.Equal("string", L.TypeName(L.Type(7)));
                Assert.Equal("boolean", L.TypeName(L.Type(8)));
                Assert.Equal("function", L.TypeName(L.Type(9)));
                Assert.Equal("table", L.TypeName(L.Type(10)));
                Assert.Equal("no value", L.TypeName(L.Type(11)));
                Assert.Equal("no value", L.TypeName(L.Type(12)));
            }
        }

        [Fact]
        public void TestToNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;

                bool isnum;
                Assert.Equal(0.0, L.ToNumber(1));
                Assert.Equal(0.0, L.ToNumber(1, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(123.45, L.ToNumber(2));
                Assert.Equal(123.45, L.ToNumber(2, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(987.0, L.ToNumber(3));
                Assert.Equal(987.0, L.ToNumber(3, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(0.0, L.ToNumber(4));
                Assert.Equal(0.0, L.ToNumber(4, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(5.6, L.ToNumber(5));
                Assert.Equal(5.6, L.ToNumber(5, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(0.0, L.ToNumber(6));
                Assert.Equal(0.0, L.ToNumber(7));
                Assert.Equal(0.0, L.ToNumber(8));
                Assert.Equal(0.0, L.ToNumber(9));
                Assert.Equal(0.0, L.ToNumber(10));
                Assert.Equal(0.0, L.ToNumber(11));
                Assert.Equal(0.0, L.ToNumber(12));
            }
        }

        [Fact]
        public void TestToInteger()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;

                bool isnum;
                Assert.Equal(0, L.ToInteger(1));
                Assert.Equal(0, L.ToInteger(1, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(0, L.ToInteger(2));
                Assert.Equal(0, L.ToInteger(2, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(987, L.ToInteger(3));
                Assert.Equal(987, L.ToInteger(3, out isnum));
                Assert.Equal(true, isnum);

                Assert.Equal(0, L.ToInteger(4));
                Assert.Equal(0, L.ToInteger(4, out isnum));
                Assert.Equal(false, isnum);

                Assert.Equal(0, L.ToInteger(5));
                Assert.Equal(0, L.ToInteger(6));
                Assert.Equal(0, L.ToInteger(7));
                Assert.Equal(0, L.ToInteger(8));
                Assert.Equal(0, L.ToInteger(9));
                Assert.Equal(0, L.ToInteger(10));
                Assert.Equal(0, L.ToInteger(11));
                Assert.Equal(0, L.ToInteger(12));
            }
        }

        [Fact]
        public void TestToBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.Equal(false, L.ToBoolean(1));
                Assert.Equal(true, L.ToBoolean(2));
                Assert.Equal(true, L.ToBoolean(3));
                Assert.Equal(true, L.ToBoolean(4));
                Assert.Equal(true, L.ToBoolean(5));
                Assert.Equal(true, L.ToBoolean(6));
                Assert.Equal(true, L.ToBoolean(7));
                Assert.Equal(true, L.ToBoolean(8));
                Assert.Equal(true, L.ToBoolean(9));
                Assert.Equal(true, L.ToBoolean(10));
                Assert.Equal(false, L.ToBoolean(11));
                Assert.Equal(false, L.ToBoolean(12));
            }
        }

        [Fact]
        public void TestRawLen()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(l => 0, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.Equal(0u, L.RawLen(1));
                Assert.Equal(0u, L.RawLen(2));
                Assert.Equal(0u, L.RawLen(3));
                Assert.Equal(4u, L.RawLen(4));
                Assert.Equal(3u, L.RawLen(5));
                Assert.Equal(2u, L.RawLen(6));
                Assert.Equal(2u, L.RawLen(7));
                Assert.Equal(0u, L.RawLen(8));
                Assert.Equal(0u, L.RawLen(9));
                Assert.Equal(0u, L.RawLen(10));
                Assert.Equal(0u, L.RawLen(11));
                Assert.Equal(0u, L.RawLen(12));
            }
        }

        [Fact]
        public void TestToCFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaFunction f = l => 0;
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(f, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                Assert.Equal(null, L.ToCFunction(1));
                Assert.Equal(null, L.ToCFunction(2));
                Assert.Equal(null, L.ToCFunction(3));
                Assert.Equal(null, L.ToCFunction(4));
                Assert.Equal(null, L.ToCFunction(5));
                Assert.Equal(null, L.ToCFunction(6));
                Assert.Equal(null, L.ToCFunction(7));
                Assert.Equal(null, L.ToCFunction(8));
                Assert.Same(f, L.ToCFunction(9));
                Assert.Equal(null, L.ToCFunction(10));
                Assert.Equal(null, L.ToCFunction(11));
                Assert.Equal(null, L.ToCFunction(12));
            }
        }

        [Fact]
        public void TestToUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                DateTime dt = DateTime.Now;
                LuaFunction f = l => 0;
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(f, 0)
                    .PushLightUserData(dt)
                    .PushGlobalTable()
                    ;
                Assert.Equal(null, L.ToUserData(1));
                Assert.Equal(null, L.ToUserData(2));
                Assert.Equal(null, L.ToUserData(3));
                Assert.Equal(null, L.ToUserData(4));
                Assert.Equal(null, L.ToUserData(5));
                Assert.Equal(null, L.ToUserData(6));
                Assert.Equal(null, L.ToUserData(7));
                Assert.Equal(null, L.ToUserData(8));
                Assert.Equal(null, L.ToUserData(9));
                Assert.Equal(dt, L.ToUserData(10));
                Assert.Equal(null, L.ToUserData(11));
                Assert.Equal(null, L.ToUserData(12));
            }
        }

        [Fact]
        public void TestToThread()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaFunction f = l => 0;
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(f, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                L.PushThread();
                Assert.Equal(null, L.ToThread(1));
                Assert.Equal(null, L.ToThread(2));
                Assert.Equal(null, L.ToThread(3));
                Assert.Equal(null, L.ToThread(4));
                Assert.Equal(null, L.ToThread(5));
                Assert.Equal(null, L.ToThread(6));
                Assert.Equal(null, L.ToThread(7));
                Assert.Equal(null, L.ToThread(8));
                Assert.Equal(null, L.ToThread(9));
                Assert.Equal(null, L.ToThread(10));
                Assert.Same(L, L.ToThread(11));
                Assert.Equal(null, L.ToThread(12));
            }
        }

        [Fact]
        public void TestToString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaFunction f = l => 0;
                L.PushNil()
                    .PushNumber(123.45)
                    .PushInteger(987)
                    .PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    .PushBoolean(true)
                    .PushCClosure(f, 0)
                    //.PushLightUserData(DateTime.Now)
                    .PushGlobalTable()
                    ;
                L.PushThread();
                Assert.Equal(null, L.ToString(1));
                Assert.Equal("123.45", L.ToString(2));
                Assert.Equal("987", L.ToString(3));
                Assert.Equal("Test", L.ToString(4));
                Assert.Equal("5.6", L.ToString(5));
                Assert.Equal("5D", L.ToString(6));
                Assert.Equal("5z", L.ToString(7));
                Assert.Equal(null, L.ToString(8));
                Assert.Equal(null, L.ToString(9));
                Assert.Equal(null, L.ToString(10));
                Assert.Equal(null, L.ToString(11));
                Assert.Equal(null, L.ToString(12));
            }
        }

        [Fact]
        public void TestPushNil()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNil();
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Nil, L.Type(-1));
            }
        }

        [Fact]
        public void TestPushNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(1234);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Number, L.Type(-1));
                Assert.Equal(1234, L.ToNumber(-1));
            }
        }

        [Fact]
        public void TestPushInteger()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushInteger(1234);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Number, L.Type(-1));
                Assert.Equal(1234, L.ToInteger(-1));
            }
        }

        [Fact]
        public void TestPushString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushString("Text");
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.String, L.Type(-1));
            }
        }

        [Fact]
        public void TestPushFString()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushFString("-s-d-", 1234);
                L.PushFString("-%s-", "Text");
                L.PushFString("-%f-", 123.45);
                L.PushFString("-%d-", 987);
                L.PushFString("-%s-%s-", "Str", "Text");
                L.PushFString("-%s-%f-", "Str", 123.45);
                L.PushFString("-%s-%d-", "Str", 987);
                L.PushFString("-%f-%s-", 11.22, "Text");
                L.PushFString("-%f-%f-", 11.22, 123.45);
                L.PushFString("-%f-%d-", 11.22, 987);
                L.PushFString("-%d-%s-", 9988, "Text");
                L.PushFString("-%d-%f-", 9988, 123.45);
                L.PushFString("-%d-%d-", 9988, 987);
                Assert.Equal(13, L.GetTop());
                Assert.Equal(LuaType.String, L.Type(-1));
                Assert.Equal("-s-d-", L.ToString(1));
                Assert.Equal("-Text-", L.ToString(2));
                Assert.Equal("-123.45-", L.ToString(3));
                Assert.Equal("-987-", L.ToString(4));
                Assert.Equal("-Str-Text-", L.ToString(5));
                Assert.Equal("-Str-123.45-", L.ToString(6));
                Assert.Equal("-Str-987-", L.ToString(7));
                Assert.Equal("-11.22-Text-", L.ToString(8));
                Assert.Equal("-11.22-123.45-", L.ToString(9));
                Assert.Equal("-11.22-987-", L.ToString(10));
                Assert.Equal("-9988-Text-", L.ToString(11));
                Assert.Equal("-9988-123.45-", L.ToString(12));
                Assert.Equal("-9988-987-", L.ToString(13));
            }
        }

        [Fact]
        public void TestPushCClosure()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaFunction f = l => 0;
                L.PushCClosure(f, 0);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Function, L.Type(-1));
                Assert.Same(f, L.ToCFunction(-1));
            }
        }

        [Fact]
        public void TestPushBoolean()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushBoolean(true);
                L.PushBoolean(false);
                Assert.Equal(2, L.GetTop());
                Assert.Equal(LuaType.Boolean, L.Type(-1));
                Assert.Equal(LuaType.Boolean, L.Type(-2));
                Assert.Equal(false, L.ToBoolean(-1));
                Assert.Equal(true, L.ToBoolean(-2));
            }
        }

        [Fact]
        public void TestPushLightUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushLightUserData(DateTime.Now);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.LightUserData, L.Type(-1));
            }
        }

        [Fact]
        public void TestPushThread()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushThread();
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Thread, L.Type(-1));
            }
        }

        [Fact]
        public void TestSetGetGlobal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushString("Value A");
                L.SetGlobal("a");
                Assert.Equal(0, L.GetTop());

                L.GetGlobal("a");
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.String, L.Type(-1));

                L.GetGlobal("b");
                Assert.Equal(2, L.GetTop());
                Assert.Equal(LuaType.Nil, L.Type(-1));

            }
        }

        [Fact]
        public void TestSetGetTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());

                L.PushInteger(2).PushString("Value");
                L.SetTable(1);
                Assert.Equal(1, L.GetTop());

                L.PushInteger(1);
                L.GetTable(1);
                Assert.Equal(2, L.GetTop());
                Assert.Equal(true, L.IsNil(-1));

                L.PushInteger(2);
                L.GetTable(1);
                Assert.Equal(3, L.GetTop());
                Assert.Equal(true, L.IsString(-1));
                Assert.Equal("Value", L.ToString(-1));
            }
        }

        [Fact]
        public void TestSetGetField()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());

                L.PushString("Value");
                L.SetField(1, "a");
                Assert.Equal(1, L.GetTop());

                L.GetField(1, "b");
                Assert.Equal(2, L.GetTop());
                Assert.Equal(true, L.IsNil(-1));

                L.GetField(1, "a");
                Assert.Equal(3, L.GetTop());
                Assert.Equal(true, L.IsString(-1));
                Assert.Equal("Value", L.ToString(-1));
            }
        }

        [Fact]
        public void TestSetGetI()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());

                L.PushString("Value");
                L.SetI(1, 2);
                Assert.Equal(1, L.GetTop());

                L.GetI(1, 1);
                Assert.Equal(2, L.GetTop());
                Assert.Equal(true, L.IsNil(-1));

                L.GetI(1, 2);
                Assert.Equal(3, L.GetTop());
                Assert.Equal(true, L.IsString(-1));
                Assert.Equal("Value", L.ToString(-1));
            }
        }

        [Fact]
        public void TestRawSetGet()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());

                L.PushInteger(2).PushString("Value");
                L.RawSet(1);
                Assert.Equal(1, L.GetTop());

                L.PushInteger(1);
                L.RawGet(1);
                Assert.Equal(2, L.GetTop());
                Assert.Equal(true, L.IsNil(-1));

                L.PushInteger(2);
                L.RawGet(1);
                Assert.Equal(3, L.GetTop());
                Assert.Equal(true, L.IsString(-1));
                Assert.Equal("Value", L.ToString(-1));
            }
        }

        [Fact]
        public void TestRawSetGetI()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());

                L.PushString("Value");
                L.RawSetI(1, 2);
                Assert.Equal(1, L.GetTop());

                L.RawGetI(1, 1);
                Assert.Equal(2, L.GetTop());
                Assert.Equal(true, L.IsNil(-1));

                L.RawGetI(1, 2);
                Assert.Equal(3, L.GetTop());
                Assert.Equal(true, L.IsString(-1));
                Assert.Equal("Value", L.ToString(-1));
            }
        }

        [Fact]
        public void TestRawSetGetP()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());

                L.PushString("Value");
                L.RawSetP(1, new IntPtr(2));
                Assert.Equal(1, L.GetTop());

                L.RawGetP(1, new IntPtr(1));
                Assert.Equal(2, L.GetTop());
                Assert.Equal(true, L.IsNil(-1));

                L.RawGetP(1, new IntPtr(2));
                Assert.Equal(3, L.GetTop());
                Assert.Equal(true, L.IsString(-1));
                Assert.Equal("Value", L.ToString(-1));
            }
        }

        [Fact]
        public void TestCreateTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.CreateTable(0, 0);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Table, L.Type(-1));
                L.Len(-1);
                Assert.Equal(0, L.ToInteger(-1));

                L.CreateTable(3, 7);
                Assert.Equal(3, L.GetTop());
                Assert.Equal(LuaType.Table, L.Type(-1));
                L.Len(-1);
                Assert.Equal(0, L.ToInteger(-1));
            }
        }

        [Fact]
        public void TestNewUserData()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                var ptr = L.NewUserData(12);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.UserData, L.Type(-1));
            }
        }

        [Fact]
        public void TestGetSetMetatable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                // Create the working table
                L.NewTable();

                // Get the metatable
                Assert.False(L.GetMetatable(1));

                // Create and set the metatable
                L.NewTable();
                L.SetMetatable(1);

                // Get the metatable
                Assert.True(L.GetMetatable(1));
                Assert.Equal(2, L.GetTop());
            }
        }

        [Fact]
        public void TestGetSetUserValue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.NewTable();
                Assert.Equal(15, (int)L.GetUserValue(1));

                L.PushString("UserValue");
                L.SetUserValue(1);
                Assert.Equal(2, L.GetTop());

                Assert.Equal(LuaType.String, L.GetUserValue(1));

                L.SetTop(0);
                var ud = L.NewUserData(12);
                Assert.Equal(LuaType.Nil, L.GetUserValue(1));

                L.PushString("UserValue");
                L.SetUserValue(1);
                Assert.Equal(2, L.GetTop());

                Assert.Equal(LuaType.String, L.GetUserValue(1));

            }
        }

        [Fact]
        public void TestArith()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(12.34)
                    .PushNumber(98.76);
                L.Arith(LuaArithOperator.Add);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(111.1, L.ToNumber(-1), 2);
            }
        }

        [Fact]
        public void TestRawEqual()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(12.34)
                    .PushNumber(98.76)
                    .PushNumber(12.34);
                Assert.False(L.RawEqual(1, 2));
                Assert.False(L.RawEqual(2, 3));
                Assert.True(L.RawEqual(1, 3));
            }
        }

        [Fact]
        public void TestCompare()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(12.34)
                    .PushNumber(98.76)
                    .PushNumber(12.34);
                Assert.Equal(false, L.Compare(1, 2, LuaRelOperator.EQ));
                Assert.Equal(false, L.Compare(2, 3, LuaRelOperator.EQ));
                Assert.Equal(true, L.Compare(1, 3, LuaRelOperator.EQ));
                Assert.Equal(true, L.Compare(1, 2, LuaRelOperator.LE));
                Assert.Equal(false, L.Compare(2, 3, LuaRelOperator.LE));
                Assert.Equal(true, L.Compare(1, 3, LuaRelOperator.LE));
                Assert.Equal(true, L.Compare(1, 2, LuaRelOperator.LT));
                Assert.Equal(false, L.Compare(2, 3, LuaRelOperator.LT));
                Assert.Equal(false, L.Compare(1, 3, LuaRelOperator.LT));
            }
        }

        [Fact]
        public void TestCall()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushFunction(l => {
                    var a = L.ToNumber(1);
                    var b = L.ToNumber(2);
                    L.PushNumber(a * b);
                    return 1;
                });
                L.PushNumber(12);
                L.PushNumber(4);
                L.Call(2, 1);
                Assert.Equal(1, L.GetTop());
                Assert.Equal(48, L.ToNumber(-1));

                L.DoString(@"
function testA(a,b)
return a-b
end
function testB(a,b)
DoAnError(a,b)
end
");
                Assert.Equal(1, L.GetTop());
                L.GetGlobal("testA");
                L.PushNumber(12);
                L.PushNumber(4);
                L.Call(2, 1);
                Assert.Equal(2, L.GetTop());
                Assert.Equal(8, L.ToNumber(-1));

                L.GetGlobal("testB");
                L.PushNumber(12);
                L.PushNumber(4);
                var ex = Assert.Throws<LuaAtPanicException>(() => L.Call(2, 1));
                Assert.Equal("[string \"\r...\"]:6: attempt to call a nil value (global 'DoAnError')", ex.Message);
            }
        }

        [Fact]
        public void TestCallK()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushFunction(l => {
                    var a = L.ToNumber(1);
                    var b = L.ToNumber(2);
                    L.PushNumber(a * b);
                    return 1;
                });
                L.PushNumber(12);
                L.PushNumber(4);
                Assert.Throws<NotImplementedException>(() => L.CallK(2, 1, 0, null));
            }
        }

        [Fact]
        public void TestPCall()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushFunction(l => {
                    var a = L.ToNumber(1);
                    var b = L.ToNumber(2);
                    L.PushNumber(a * b);
                    return 1;
                });
                L.PushNumber(12);
                L.PushNumber(4);
                Assert.Equal(LuaStatus.OK, L.PCall(2, 1, 0));
                Assert.Equal(1, L.GetTop());
                Assert.Equal(48, L.ToNumber(-1));

                L.DoString(@"
function testA(a,b)
return a-b
end
function testB(a,b)
DoAnError(a,b)
end
");
                Assert.Equal(1, L.GetTop());
                L.GetGlobal("testA");
                L.PushNumber(12);
                L.PushNumber(4);
                Assert.Equal(LuaStatus.OK, L.PCall(2, 1, 0));
                Assert.Equal(2, L.GetTop());
                Assert.Equal(8, L.ToNumber(-1));

                L.GetGlobal("testB");
                L.PushNumber(12);
                L.PushNumber(4);
                Assert.Equal(LuaStatus.ErrRun, L.PCall(2, 1, 0));
                Assert.Equal("[string \"\r...\"]:6: attempt to call a nil value (global 'DoAnError')", L.ToString(-1));
            }
        }

        [Fact]
        public void TestPCallK()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushFunction(l => {
                    var a = L.ToNumber(1);
                    var b = L.ToNumber(2);
                    L.PushNumber(a * b);
                    return 1;
                });
                L.PushNumber(12);
                L.PushNumber(4);
                Assert.Throws<NotImplementedException>(() => L.PCallK(2, 1, 0, 0, null));
            }
        }

        [Fact]
        public void TestLoad()
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
                var st = L.Load((l, ud) => {
                    Byte[] res = null;
                    if (curr < chunkBytes.Length)
                    {
                        int c = Math.Min(7, chunkBytes.Length - curr);
                        res = new Byte[c];
                        Array.Copy(chunkBytes, curr, res, 0, c);
                        curr += c;
                    }
                    return res;
                }, null, "main", null);
                Assert.Equal(LuaStatus.OK, st);
                st = L.PCall(0, 1, 0);
                Assert.Equal(LuaStatus.OK, st);
                Assert.Equal("Lua rocks!!", L.ToString(-1));
            }
        }

        [Fact]
        public void TestDump()
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
                var st = L.Load(loadChunk, null, "main", null);
                Assert.Equal(LuaStatus.OK, st);

                List<Byte[]> dump = new List<byte[]>();
                var r = L.Dump((l, b, ud) => {
                    dump.Add(b);
                    return 0;
                }, null, 0);
                Assert.Equal(0, r);
                Byte[] dumpBytes = dump.SelectMany(d => d).ToArray();
                Assert.Equal(199, dumpBytes.Length);

                // Remove the function
                L.Pop(1);
                Assert.Equal(0, L.GetTop());

                // Reload chunk compiled
                chunkBytes = dumpBytes;
                curr = 0;
                st = L.Load(loadChunk, null, "main", "b");
                Assert.Equal(LuaStatus.OK, st);
                st = L.PCall(0, 1, 0);
                Assert.Equal(LuaStatus.OK, st);
                Assert.Equal("Lua rocks!!", L.ToString(-1));

            }
        }

        [Fact]
        public void TestYieldK()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Throws<NotImplementedException>(() => L.YieldK(0, 0, null));
            }
        }

        [Fact]
        public void TestYield()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                var ex = Assert.Throws<LuaAtPanicException>(() => L.Yield(0));
                Assert.Equal("attempt to yield from outside a coroutine", ex.Message);
            }
        }

        [Fact]
        public void TestResume()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(LuaStatus.ErrRun, L.Resume(null, 0));

                Assert.Equal(LuaStatus.ErrRun, L.Resume(L, 0));
            }
        }

        [Fact]
        public void TestStatus()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(LuaStatus.OK, L.Status());
            }
        }

        [Fact]
        public void TestIsYieldable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(false, L.IsYieldable());
            }
        }

        [Fact]
        public void TestGC()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(2, L.GC(LuaGcFunction.Count, 0));
            }
        }

        [Fact]
        public void TestError()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushString("Error message");
                var ex = Assert.Throws<LuaAtPanicException>(() => L.Error());
                Assert.Equal("Error message", ex.Message);
            }
        }

        [Fact]
        public void TestNext()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.NewTable();
                var t = L.GetTop();
                L.PushString("Value A");
                L.SetField(t, "a");
                L.PushString("Value 2");
                L.SetI(t, 2);
                Assert.Equal(1, L.GetTop());

                int count = 0;
                bool foundA = false;
                // First Key
                L.PushNil();
                while (L.Next(t))
                {
                    count++;
                    // => 'key' at index -2 , 'value' as index -1
                    if (LuaType.String == L.Type(-2))
                    {
                        Assert.False(foundA);
                        Assert.Equal(LuaType.String, L.Type(-2));
                        Assert.Equal("a", L.ToString(-2));
                        Assert.Equal(LuaType.String, L.Type(-1));
                        Assert.Equal("Value A", L.ToString(-1));
                        foundA = true;
                    }
                    else
                    {
                        // Second key
                        Assert.Equal(LuaType.Number, L.Type(-2));
                        Assert.Equal(2, L.ToInteger(-2));
                        Assert.Equal(LuaType.String, L.Type(-1));
                        Assert.Equal("Value 2", L.ToString(-1));
                    }
                    // Remove 'value' and keep the 'key' on the stack for the next key
                    L.Pop(1);
                }
                Assert.Equal(2, count);
                Assert.True(foundA);
                // No more key
                // The stack is cleaned
                Assert.Equal(1, L.GetTop());

            }
        }

        [Fact]
        public void TestConcat()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushString("One");
                L.PushNumber(2);
                L.PushString("Three");
                L.PushNumber(4);
                L.Concat(3);
                Assert.Equal("2.0Three4.0", L.ToString(-1));
            }
        }

        [Fact]
        public void TestLen()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushString("Test")
                    .PushString("5.6")
                    .PushString("5D")
                    .PushString("5z")
                    ;
                L.Len(1);
                Assert.Equal(4, L.ToNumber(-1));
                L.Len(2);
                Assert.Equal(3, L.ToNumber(-1));
                L.Len(3);
                Assert.Equal(2, L.ToNumber(-1));
                L.Len(4);
                Assert.Equal(2, L.ToNumber(-1));
            }
        }

        [Fact]
        public void TestStringToNumber()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(4, L.StringToNumber("5.6"));
                Assert.Equal(1, L.GetTop());
                Assert.Equal(5.6, L.ToNumber(-1));

                Assert.Equal(2, L.StringToNumber("5"));
                Assert.Equal(2, L.GetTop());
                Assert.Equal(5.0, L.ToNumber(-1));

                Assert.Equal(0, L.StringToNumber("5D"));
                Assert.Equal(2, L.GetTop());

                Assert.Equal(0, L.StringToNumber("Test"));
                Assert.Equal(2, L.GetTop());

            }
        }

        [Fact]
        public void TestPop()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(1);
                L.PushString("Text");
                L.PushNumber(2);
                L.PushString("Text");
                Assert.Equal(4, L.GetTop());
                L.Pop(2);
                Assert.Equal(2, L.GetTop());
                L.Pop(1);
                Assert.Equal(1, L.GetTop());
                L.Pop(2);
                Assert.Equal(-1, L.GetTop());
            }
        }

        [Fact]
        public void TestNewTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.NewTable();
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Table, L.Type(-1));
            }
        }

        [Fact]
        public void TestRegister()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.GetGlobal("a");
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Nil, L.Type(-1));

                L.Register("a", (l) => {
                    l.PushNumber(12.34);
                    return 0;
                });

                L.GetGlobal("a");
                Assert.Equal(2, L.GetTop());
                Assert.Equal(LuaType.Function, L.Type(-1));
            }
        }

        [Fact]
        public void TestPushFunction()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushFunction((l) => {
                    l.PushNumber(12.34);
                    return 0;
                });
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.Function, L.Type(-1));

                Assert.Throws<ArgumentNullException>(() => L.PushFunction(null));
            }
        }

        [Fact]
        public void TestPushLiteral()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushLiteral("Test");
                Assert.Equal(1, L.GetTop());
                Assert.Equal(LuaType.String, L.Type(-1));
            }
        }

        [Fact]
        public void TestPushGlobalTable()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushGlobalTable();
                Assert.Equal(LuaType.Table, L.Type(-1));
            }
        }

        [Fact]
        public void TestPushUnsigned()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushUnsigned(23);
                Assert.Equal(23, L.ToInteger(-1));
            }
        }

        [Fact]
        public void TestToUnsigned()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushUnsigned(23);
                Assert.Equal(23u, L.ToUnsigned(-1));
                bool isnum;
                Assert.Equal(23u, L.ToUnsigned(-1, out isnum));
                Assert.True(isnum);
            }
        }

        [Fact]
        public void TestGetStack()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    var ar = new LuaDebug();
                    Assert.Throws<NotImplementedException>(() => l.GetStack(0, ar));
                    //Assert.Equal(0, l.GetStack(0, ar));
                    //Assert.Equal(0, ar.currentline);
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestGetInfo()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    var ar = new LuaDebug();
                    Assert.Throws<NotImplementedException>(() => l.GetInfo("f", ar));
                    //Assert.Equal(0, l.GetStack(0, ar));
                    //Assert.Equal(0, ar.currentline);
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestGetLocal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    var ar = new LuaDebug();
                    Assert.Throws<NotImplementedException>(() => l.GetLocal(ar, 0));
                    //Assert.Equal(0, l.GetStack(0, ar));
                    //Assert.Equal(0, ar.currentline);
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestSetLocal()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    var ar = new LuaDebug();
                    Assert.Throws<NotImplementedException>(() => l.SetLocal(ar, 0));
                    //Assert.Equal(0, l.GetStack(0, ar));
                    //Assert.Equal(0, ar.currentline);
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestGetUpvalue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    Assert.Equal(null, l.GetUpvalue(0, 0));
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestSetUpvalue()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    Assert.Equal(null, l.SetUpvalue(0, 0));
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestUpvalueId()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    Assert.Equal(0, l.UpvalueId(0, 0));
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestUpvalueJoin()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                L.PushNumber(23);
                L.PushFunction(l => {
                    //TODO Do test
                    //l.UpvalueJoin(0, 0, 0, 0);
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestSetHook()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                LuaHook hook = (ILuaState l, LuaDebug ar) => {

                };
                Assert.Throws<NotImplementedException>(() => L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, 1));
                L.PushNumber(23);
                L.PushFunction(l => {
                    return 0;
                });
                L.PCall(0, 0, 0);
            }
        }

        [Fact]
        public void TestGetHook()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Throws<NotImplementedException>(() => Assert.Null(L.GetHook()));
                LuaHook hook = (ILuaState l, LuaDebug ar) => {

                };
                Assert.Throws<NotImplementedException>(() => L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, 1));
                Assert.Throws<NotImplementedException>(() => Assert.Null(L.GetHook()));
            }
        }

        [Fact]
        public void TestGetHookMask()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(LuaHookMask.None, L.GetHookMask());
                LuaHook hook = (ILuaState l, LuaDebug ar) => {
                };
                Assert.Throws<NotImplementedException>(() => L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, 1));
                Assert.Equal(LuaHookMask.None, L.GetHookMask());
            }
        }

        [Fact]
        public void TestGetHookCount()
        {
            LuaState L = null;
            using (L = new LuaState())
            {
                Assert.Equal(0, L.GetHookCount());
                LuaHook hook = (ILuaState l, LuaDebug ar) => {
                };
                Assert.Throws<NotImplementedException>(() => L.SetHook(hook, LuaHookMask.MaskCall | LuaHookMask.MaskLine | LuaHookMask.MaskCount, 1));
                Assert.Equal(0, L.GetHookCount());
            }
        }

    }
}
