using System;

namespace xServiceManager.Module
{
    public class ItemStatistics
    {
        public Object Type;
        public string TypeName;
        public int Count;
        public ItemStatistics(Object o, string s, int c)
        {
            Type = o;
            TypeName = s;
            Count = c;
        }
    }
}
