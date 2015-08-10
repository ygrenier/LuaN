using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class UserDataIndexTest
    {

        [Fact]
        public void TestCreate()
        {
            var index = new UserDataIndex();
            Assert.Equal(0, index.Count);
        }

        [Fact]
        public void TestAdd()
        {
            var index = new UserDataIndex();
            var ud = index.Add(this);
            Assert.Equal(new IntPtr(-1), ud.Pointer);
            Assert.Same(this, ud.Data);
            Assert.Equal(1, index.Count);

            Assert.Same(ud, index.Add(this));

            Assert.Null(index.Add(null));

            var nud = new LuaNativeUserData(new IntPtr(12345), 123);
            ud = index.Add(nud);
            Assert.Equal(new IntPtr(12345), ud.Pointer);
            Assert.Equal(2, index.Count);
        }

        [Fact]
        public void TestFindData()
        {
            var index = new UserDataIndex();

            var nud = new LuaNativeUserData(new IntPtr(12345), 123);

            Assert.Null(index.FindData(null));
            Assert.Null(index.FindData(this));
            Assert.Null(index.FindData(nud));

            var ud = index.Add(this);
            Assert.Same(ud, index.FindData(this));

            ud = index.Add(nud);
            Assert.Same(ud, index.FindData(nud));
        }

        [Fact]
        public void TestFindPtr()
        {
            var index = new UserDataIndex();

            var nud = new LuaNativeUserData(new IntPtr(12345), 123);

            Assert.Null(index.FindPointer(IntPtr.Zero));
            Assert.Null(index.FindPointer(new IntPtr(10)));
            Assert.Null(index.FindPointer(new IntPtr(-10)));

            var ud = index.Add(this);
            Assert.Same(ud, index.FindPointer(ud.Pointer));

            ud = index.Add(nud);
            Assert.Same(ud, index.FindPointer(new IntPtr(12345)));
        }

        [Fact]
        public void TestReset()
        {
            var index = new UserDataIndex();

            var nud = new LuaNativeUserData(new IntPtr(12345), 123);

            index.Add(this);
            index.Add(nud);

            index.Reset();

            Assert.Equal(0, index.Count);
            Assert.Equal(new UserDataIndex.UserDataRef[0], index.GetUserDatas().ToArray());

        }

        [Fact]
        public void TestRemove()
        {
            var index = new UserDataIndex();

            var nud = new LuaNativeUserData(new IntPtr(12345), 123);

            var ud1 = index.Add(this);
            var ud2 = index.Add(nud);

            index.Remove(IntPtr.Zero);
            index.Remove(new IntPtr(99));
            index.Remove(new IntPtr(12345));

            Assert.Equal(1, index.Count);
            Assert.Equal(new UserDataIndex.UserDataRef[] { ud1 }, index.GetUserDatas().ToArray());

        }

        [Fact]
        public void TestGetUserDatas()
        {
            var index = new UserDataIndex();
            Assert.Equal(new UserDataIndex.UserDataRef[0], index.GetUserDatas().ToArray());

            var nud = new LuaNativeUserData(new IntPtr(12345), 123);

            var ud1 = index.Add(this);
            var ud2 = index.Add(nud);

            Assert.Equal(new UserDataIndex.UserDataRef[] { ud1, ud2 }, index.GetUserDatas().ToArray());

        }

    }
}
