namespace xServiceManager.Module
{
    public class PropertyChange
    {
        public Change WhatChanged;
        public ChangeType TypeOfChange;
        public string Name;
        public object OldValue;
        public object NewValue;
        public PropertyChange() {; }
        public PropertyChange(Change type, ChangeType operation, string name, object oldval, object newval)
        {
            WhatChanged = type;
            TypeOfChange = operation;
            Name = name;
            OldValue = oldval;
            NewValue = newval;
        }
    }
}
