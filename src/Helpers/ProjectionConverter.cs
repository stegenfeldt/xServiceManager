using System;
using System.Management.Automation;

namespace xServiceManager.Module
{
    public class ProjectionConverter : PSTypeConverter
    {
        // Currently, this converter is only targeted at/ EnterpriseManagementObjectProjection, 
        // (via the SMLets.Types.ps1xml file) but could easily be extended

        public override bool CanConvertFrom(object source, Type destination)
        {
            PSObject o = source as PSObject;
            return CanConvertFrom(o, destination);
        }
        public override bool CanConvertFrom(PSObject source, Type destination)
        {
            if (source.Properties["__base"] != null)
            {
                return true;
            }
            return false;
        }
        public override object ConvertFrom(Object source, Type destination, IFormatProvider p, bool ignoreCase)
        {
            PSObject o = source as PSObject;
            if (o == null) { throw new InvalidCastException("Conversion failed"); }
            if (this.CanConvertFrom(o, destination))
            {
                try
                {
                    return o.Properties["__base"].Value;
                }
                catch
                {
                    throw new InvalidCastException("Conversion failed");
                }
            }
            throw new InvalidCastException("Conversion failed");
        }
        public override object ConvertFrom(PSObject source, Type destination, IFormatProvider p, bool ignoreCase)
        {
            if (source == null) { throw new InvalidCastException("Conversion failed"); }
            if (this.CanConvertFrom(source, destination))
            {
                try
                {
                    return source.Properties["__base"].Value;
                }
                catch
                {
                    throw new InvalidCastException("Conversion failed");
                }
            }
            throw new InvalidCastException("Conversion failed");
        }
        public override bool CanConvertTo(object source, Type destination)
        {
            PSObject o = source as PSObject;
            return CanConvertTo(o, destination);
        }
        public override bool CanConvertTo(PSObject value, Type destination)
        {
            return false;
        }
        public override object ConvertTo(object value, Type destination, IFormatProvider p, bool ignoreCase)
        {
            throw new InvalidCastException("Conversion failed");
        }
        public override object ConvertTo(PSObject value, Type destination, IFormatProvider p, bool ignoreCase)
        {
            throw new InvalidCastException("Conversion failed");
        }
    }
}
