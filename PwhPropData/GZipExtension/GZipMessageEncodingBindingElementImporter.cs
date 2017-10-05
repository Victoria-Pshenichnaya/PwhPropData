using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Xml;
using PwhPropData.Core.Common;

namespace PwhPropData.Core.GZipExtension
{
    /// <summary>
    /// This class is used to create the custom encoder (GZipMessageEncoder)
    /// </summary>
    public class GZipMessageEncodingBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            Guard.NotNull(importer, "importer");
            Guard.NotNull(context, "context");

            ICollection<XmlElement> assertions = context.GetBindingAssertions();
            foreach (XmlElement assertion in assertions)
            {
                if ((assertion.NamespaceURI == GZipMessageEncodingPolicyConstants.GZipEncodingNamespace) &&
                    (assertion.LocalName == GZipMessageEncodingPolicyConstants.GZipEncodingName)
                    )
                {
                    assertions.Remove(assertion);
                    context.BindingElements.Add(new GZipMessageEncodingBindingElement());
                    break;
                }
            }
        }
    }
}