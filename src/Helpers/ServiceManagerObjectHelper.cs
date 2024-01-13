using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace xServiceManager.Module
{
    public sealed class ServiceManagerObjectHelper
    {
        // a helper class  to read a stream and adapt an EMO

        // We handle the retrival of binary types
        // if it's a binary property type, we'll return an array of
        // bytes
        private static Byte[] GetBytes(Stream s)
        {
            // Why SM binary fields don't have a valid length property is a mystery
            if (s == null) { return null; }
            byte[] buffer = new byte[4096];
            Collection<Byte[]> balist = new Collection<Byte[]>();
            int count;
            int totalLength = 0;
            while ((count = s.Read(buffer, 0, 4096)) != 0)
            {
                totalLength += count;
                byte[] tbyte = new byte[count];
                Array.Copy(buffer, tbyte, count);
                balist.Add(tbyte);
            }
            byte[] ReturnBytes = new byte[totalLength];
            int offset = 0;
            foreach (byte[] b in balist)
            {
                b.CopyTo(ReturnBytes, offset);
                offset += b.Length;
            }
            return ReturnBytes;
        }

        public static PSObject AdaptProjection(Cmdlet myCmdlet, EnterpriseManagementObjectProjection p, string projectionName)
        {
            myCmdlet.WriteVerbose("Adapting " + p);
            /*
             * We can't just wrap a type projection because it is Enumerable. This means that we would only see the
             * components of the projection in the output so we have to construct this artificial wrapper. It would be easier if
             * projections weren't Enumerable, which means that PowerShell wouldn't treat a projection as a collection, or if 
             * PowerShell understood that certain collections shouldn't be unspooled, but that's not how PowerShell works.
             * Neither of those two options are available, so we adapt the object and present a PSObject with all the component
             * parts.
             */
            PSObject o = new PSObject();
            o.Members.Add(new PSNoteProperty("__base", p));
            o.Members.Add(new PSScriptMethod("GetAsXml", ScriptBlock.Create("[xml]($this.__base.CreateNavigator().OuterXml)")));
            o.Members.Add(new PSNoteProperty("Object", AdaptManagementObject(myCmdlet, p.Object)));
            // Now promote all the properties on Object
            foreach (EnterpriseManagementSimpleObject so in p.Object.Values)
            {
                try
                {
                    o.Members.Add(new PSNoteProperty(so.Type.Name, so.Value));
                }
                catch
                {
                    myCmdlet.WriteWarning("could not promote: " + so.Type.Name);
                }
            }

            o.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObjectProjection#{0}", projectionName);
            o.TypeNames.Insert(1, "EnterpriseManagementObjectProjection");
            o.Members.Add(new PSNoteProperty("__ProjectionType", projectionName));

            foreach (KeyValuePair<ManagementPackRelationshipEndpoint, IComposableProjection> helper in p)
            {
                // EnterpriseManagementObject myEMO = (EnterpriseManagementObject)helper.Value.Object;
                myCmdlet.WriteVerbose("Adapting related objects: " + helper.Key.Name);
                String myName = helper.Key.Name;
                PSObject adaptedEMO = AdaptManagementObject(myCmdlet, helper.Value.Object);
                // If the MaxCardinality is greater than one, it's definitely a collection
                // so start out that way
                if (helper.Key.MaxCardinality > 1)
                {
                    // OK, this is a collection, so add the critter
                    // This is so much easier in PowerShell
                    if (o.Properties[myName] == null)
                    {
                        o.Members.Add(new PSNoteProperty(myName, new ArrayList()));
                    }
                    ((ArrayList)o.Properties[myName].Value).Add(adaptedEMO);
                }
                else
                {
                    try
                    {
                        o.Members.Add(new PSNoteProperty(helper.Key.Name, adaptedEMO));
                    }
                    catch (ExtendedTypeSystemException e)
                    {
                        myCmdlet.WriteVerbose("Readapting relationship object -> collection :" + e.Message);
                        // We should really only get this exception if we
                        // try to add a create a new property which already exists
                        Object currentPropertyValue = o.Properties[myName].Value;
                        ArrayList newValue = new ArrayList();
                        newValue.Add(currentPropertyValue);
                        newValue.Add(adaptedEMO);
                        o.Properties[myName].Value = newValue;
                        // TODO
                        // If this already exists, it should be converted to a collection
                    }
                }
            }
            return o;
        }

        // Adapt the EnterpriseManagementObject
        // We need to do this because the interest bits of the EMO (from our perspective)
        // are in the values collection, we promote them to NoteProperties so displaying 
        // the contents work better. This should really be done with a Type adapter, but
        // this is ok.
        public static PSObject AdaptManagementObject(Cmdlet myCmdlet, EnterpriseManagementObject managementObject)
        {
            PSObject PromotedObject = new PSObject(managementObject);
            PromotedObject.TypeNames.Insert(1, managementObject.GetType().FullName);
            PromotedObject.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObject#{0}", managementObject.GetLeastDerivedNonAbstractClass().Name);
            // loop through the properties and promote them into the PSObject we're going to return
            foreach (ManagementPackProperty p in managementObject.GetProperties())
            {
                try
                {
                    if (p.SystemType.ToString() == "System.IO.Stream")
                    {
                        if (managementObject[p].Value != null)
                        {
                            PSObject StreamObject = new PSObject(managementObject[p].Value);
                            Byte[] Data = GetBytes(managementObject[p].Value as Stream);
                            StreamObject.Members.Add(new PSNoteProperty("Data", Data));
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, StreamObject));
                        }
                        else
                        {
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, new Byte[0]));
                        }
                    }
                    else
                    {
                        PromotedObject.Members.Add(new PSNoteProperty(p.Name, managementObject[p].Value));
                    }
                }
                catch (ExtendedTypeSystemException ets)
                {
                    myCmdlet.WriteWarning(String.Format("The property '{0}' already exists, skipping.\nException: {1}", p.Name, ets.Message));
                }
                catch (Exception e)
                {
                    myCmdlet.WriteError(new ErrorRecord(e, "Property", ErrorCategory.NotSpecified, p.Name));
                }
            }

            PromotedObject.Members.Add(new PSNoteProperty("__InternalId", managementObject.Id));
            return PromotedObject;

        }

        // This overload is so we can call the adapter from the script layer 
        // where we may not have a cmdlet reference. In the case of errors, we'll 
        // just let it get thrown
        public static PSObject AdaptManagementObject(EnterpriseManagementObject managementObject)
        {
            PSObject PromotedObject = new PSObject(managementObject);
            PromotedObject.TypeNames.Insert(1, managementObject.GetType().FullName);
            PromotedObject.TypeNames[0] = String.Format(CultureInfo.CurrentCulture, "EnterpriseManagementObject#{0}", managementObject.GetLeastDerivedNonAbstractClass().Name);
            // loop through the properties and promote them into the PSObject we're going to return
            foreach (ManagementPackProperty p in managementObject.GetProperties())
            {
                try
                {
                    if (p.SystemType.ToString() == "System.IO.Stream")
                    {
                        if (managementObject[p].Value != null)
                        {
                            PSObject StreamObject = new PSObject(managementObject[p].Value);
                            Byte[] Data = GetBytes(managementObject[p].Value as Stream);
                            StreamObject.Members.Add(new PSNoteProperty("Data", Data));
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, StreamObject));
                        }
                        else
                        {
                            PromotedObject.Members.Add(new PSNoteProperty(p.Name, new Byte[0]));
                        }
                    }
                    else
                    {
                        PromotedObject.Members.Add(new PSNoteProperty(p.Name, managementObject[p].Value));
                    }
                }
                catch (ExtendedTypeSystemException ets)
                {
                    throw (new InvalidOperationException(String.Format("The property '{0}' already exists, skipping.\nException: {1}", p.Name, ets.Message)));
                }
                catch (Exception e)
                {
                    throw (e);
                }
            }
            PromotedObject.Members.Add(new PSNoteProperty("__InternalId", managementObject.Id));
            return PromotedObject;

        }
        private ServiceManagerObjectHelper() {; }
    }
}
