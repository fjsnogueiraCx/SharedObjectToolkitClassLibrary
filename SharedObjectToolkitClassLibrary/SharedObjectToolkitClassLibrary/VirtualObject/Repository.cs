using System;
using System.Collections.Generic;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    /// <summary>
    /// A repository of virtual objects
    /// 
    /// COMPRESSION
    /// 
    /// Quand un block n'a plus d'objet qui pointe dessus, alors on le compresse en LZ4. Cela pourrait être un comportement
    /// spécifié pour un type particulier.
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public static unsafe class Repository<TKey> where TKey:struct, IComparable, IComparable<TKey>, IEquatable<TKey> {
        private static Dictionary<TKey, IntPtr> _map = new Dictionary<TKey, IntPtr>();

        public static void Add(VirtualObject<TKey> obj) {
        }

        public static void Update(VirtualObject<TKey> obj) {
        }

        public static void Remove(VirtualObject<TKey> obj) {
        }

        public static VirtualObject<TKey> Get(TKey k, SmartPointer data) {
            return null;
        }
    }
}