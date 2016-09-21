using System;
using System.Collections.Generic;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    /// <summary>
    /// A repository of virtual objects
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public static unsafe class Repository<TKey> {
        private static Dictionary<TKey, IntPtr> _map = new Dictionary<TKey, IntPtr>();

        public static void Add(TKey k, SmartPointer data) {
        }

        public static VirtualObject Get(TKey k, SmartPointer data) {
            return null;
        }
    }
}