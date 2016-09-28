using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedObjectToolkitClassLibrary.VirtualObject {
    public struct VOId : IULongConvertible, IComparable, IComparable<VOId>, IEquatable<VOId> {
        public ulong Id;
        public ulong AsULong() {
            return Id;
        }

        public int CompareTo(object obj) {
            if (obj is VOId)
                return Id.CompareTo(((VOId)obj).Id);
            return 0;
        }

        public int CompareTo(VOId other) {
            return Id.CompareTo(((VOId)other).Id);
        }

        public bool Equals(VOId other) {
            return Id == other.Id;
        }

        public VOId(ulong id) {
            Id = id;
        }

        public static implicit operator VOId(ulong d) {
            return new VOId(d);
        }

        public static implicit operator ulong (VOId id) {
            return id.Id;
        }
    }
}
