using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedObjectToolkitClassLibrary;
using SharedObjectToolkitClassLibrary.Memory;
using SharedObjectToolkitClassLibrary.VirtualObject;

namespace SharedObjectToolkitClassLibraryUnitTests.Memory.VirtualObject {
    [TestClass]
    public unsafe class MemoryHelperUnitTests {
        [TestMethod]
        public void Move() {
            byte[] buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            
            fixed (byte* src = &buffer[0])
            {
                MemoryHelper.Move(src, 3, 6, 10);
            }
        }

        [TestMethod]
        public void MoveReverse() {
            byte[] buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

            fixed (byte* src = &buffer[0])
            {
                MemoryHelper.Move(src, 6, 3, 10);
            }
        }
    }
}
