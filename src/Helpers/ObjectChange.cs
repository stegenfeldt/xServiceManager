using System;
using System.Collections.Generic;

namespace xServiceManager.Module
{
    public class ObjectChange
    {
        public List<PropertyChange> Changes;
        public DateTime LastModified;
        public string UserName;
        public string Connector;
        public ObjectChange()
        {
            Changes = new List<PropertyChange>();
        }
    }
}
