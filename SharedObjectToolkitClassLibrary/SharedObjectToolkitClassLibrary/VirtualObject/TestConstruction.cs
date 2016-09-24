using System;
using SharedObjectToolkitClassLibrary.Memory;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    public unsafe class TestConstruction {

        public struct FixedPart_A {
            public int _age;
        }

        public class MyClasse_A : VirtualObject<ulong> {
            private static object _locker = new object();
            private static TypeDescriptor descriptor_A = null;
            private static int NAME;
            private static int POWERS;

            protected override TypeDescriptor GetDescriptor() {
                if (descriptor_A == null) {
                    lock (_locker) {
                        if (descriptor_A == null) {
                            descriptor_A = new TypeDescriptor(base.GetDescriptor(), sizeof(FixedPart_A));
                            NAME = descriptor_A.AddArray(new ArrayDescriptor<char>());
                            POWERS = descriptor_A.AddArray(new ArrayDescriptor<int>());
                        }
                    }
                }
                return descriptor_A;
            }

            public int A {
                get { return ((FixedPart_A*)Data)->_age; }
                set {
                    NewVersion();
                    ((FixedPart_A*)Data)->_age = value;
                }
            }

            public string Name {
                get {
                    var p = GetArray(NAME);
                    return new string((char*)p.Address, 0, p.Lenght/2);
                }
                set {
                    NewVersion();
                    fixed (char* s = value)
                        SetArray(NAME, (byte*) s, value.Length*2);
                }
            }

            public int[] Powers {
                get {
                    var p = GetArray(POWERS);
                    var r = new int[p.Lenght/4];
                    // -------- Copy
                    fixed (int* s = &r[0])
                        MemoryHelper.Copy(p.Address, (byte*)s, p.Lenght);
                    return r;
                }
                set {
                    NewVersion();
                    fixed (int* s = &value[0])
                        SetArray(POWERS, (byte*)s, value.Length * 4);
                }
            }
        }


        public struct FixedPart_B {
            public FixedPart_A _A;
            public int _b;
        }

        public class MyClasse_B : MyClasse_A {
            private static object _locker = new object();
            private static TypeDescriptor descriptor_B = null;
            private static int CITY;

            protected override TypeDescriptor GetDescriptor() {
                if (descriptor_B == null) {
                    lock (_locker) {
                        if (descriptor_B == null) {
                            descriptor_B = new TypeDescriptor(base.GetDescriptor(), sizeof(FixedPart_B));
                            CITY = descriptor_B.AddArray(new ArrayDescriptor<char>());
                        }
                    }
                }
                return descriptor_B;
            }

            public int B {
                get { return ((FixedPart_B*)_data)->_b; }
                set {
                    NewVersion();
                    ((FixedPart_B*)_data)->_b = value;
                }
            }

            public string City {
                get {
                    var p = GetArray(CITY);
                    return new string((char*)p.Address, 0, p.Lenght/2);
                }
                set {
                    NewVersion();
                    fixed (char* s = value)
                        SetArray(CITY, (byte*)s, value.Length * 2);
                }
            }
        }

        public static void Test() {
            MyClasse_B B = new MyClasse_B();


            B.A = 5;
            B.B = 33;

            B.City = "Lyon";

            if (B.City == "Lyon")
                Console.WriteLine("ok!");

            B.Name = "Gabriel";

            if (B.Name == "Gabriel")
                Console.WriteLine("ok - Gabriel!");
        }
    }
}