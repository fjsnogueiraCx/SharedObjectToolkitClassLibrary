using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using SharedObjectToolkitClassLibrary.Memory.BlockBasedAllocator;
using SharedObjectToolkitClassLibrary.VirtualObject.Recorder;

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
    public unsafe class VirtualObjectRepository<TKey> where TKey:struct, IComparable, IComparable<TKey>, IEquatable<TKey>, IULongConvertible {
        private static ULongToPtrByteTree _map = new ULongToPtrByteTree();

        public void Add(VirtualObject<TKey> obj) {
            MemoryAllocator.IncrementReference(obj.Data);
            _map.Add(obj.Id.AsULong(),obj.Data);
        }

        public void Update(VirtualObject<TKey> obj) {
            _map.Add(obj.Id.AsULong(), obj.Data);
        }

        public void Remove(VirtualObject<TKey> obj) {
            _map.Remove(obj.Id.AsULong(), obj.Data);
        }

        public VirtualObject<TKey> Get(TKey k) {
            var p = _map.Get(k.AsULong());
            return (VirtualObject < TKey > )VirtualObjectFactory.Rebirth(p); ;
        }

        /*public IEnumerable<VirtualObject<TKey>> AllOfType() {
            foreach (var k in _map.All) {
                yield return k;
            }
        }*/
    }
}