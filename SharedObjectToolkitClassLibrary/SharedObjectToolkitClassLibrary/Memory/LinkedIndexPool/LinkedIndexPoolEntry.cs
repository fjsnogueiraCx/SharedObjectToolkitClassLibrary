namespace SharedObjectToolkitClassLibrary.Memory {
    public unsafe struct LinkedIndexPoolEntry {
        public int Previous;
        public int Next;
        public int Queue;
        public int Index;

        public override string ToString() {
            string r = "";
            if (Queue != 0)
                r += "USED :   ";
            else
                r += "free :   ";
            if (Previous != -1)
                r += Previous + "  <-  " + Index;
            else
                r += "        [" + Index;
            if (Next != -1)
                r += "  ->  " + Next;
            else
                r += "]";
            return r;
        }
    }
}