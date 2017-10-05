using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace PwhPropData.Core.GZipExtension
{
    /// <summary>
    /// This class is necessary to be able to plug in the GZip encoder binding element through
    /// a configuration file
    /// </summary>
    public class GZipMessageEncodingElement : BindingElementExtensionElement
    {
        string _sContentType = "";

        [ConfigurationProperty("enableCompression", DefaultValue = "false")]
        public bool EnableCompression
        {
            get { return (bool)base["enableCompression"]; }
            set { base["enableCompression"] = value; }
        }

        //Called by the WCF to discover the type of binding element this config section enables
        public override Type BindingElementType
        {
            get { return typeof(GZipMessageEncodingBindingElement); }
        }

        //The only property we need to configure for our binding element is the type of
        //inner encoder to use. Here, we support text and binary.
        [ConfigurationProperty("innerMessageEncoding", DefaultValue = "textMessageEncoding")]
        public string InnerMessageEncoding
        {
            get { return (string)base["innerMessageEncoding"]; }
            set { base["innerMessageEncoding"] = value; }
        }


        //The only property we need to configure for our binding element is the 
        //SOAP version of the service
        [ConfigurationProperty("messageVersion", DefaultValue = "Soap12")]
        public string SOAPVersion
        {
            get { return (string)base["messageVersion"]; }
            set { base["messageVersion"] = value; }
        }


		[ConfigurationProperty("readerQuotas")]
		public XmlDictionaryReaderQuotasElement ReaderQuotas
		{
			get { return (XmlDictionaryReaderQuotasElement)base["readerQuotas"]; }
		} 

        private void ReadConfiguration()
        {
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo["innerMessageEncoding"].ValueOrigin != PropertyValueOrigin.Default)
            {
                switch (InnerMessageEncoding)
                {
                    case "textMessageEncoding":
                        if (SOAPVersion == "Soap11")
                        {
                            _sContentType = "text/xml; charset=utf-8";
                            //GZipEncoder.GZipMessageEncoderFactory.GZipMessageEncoder.GZipContentType = sContentType;
                        }
                        if (SOAPVersion == "Soap12")
                        {
                            //binding = new GZipMessageEncodingBindingElement();									
                            _sContentType = "application/soap+xml; charset=utf-8";
                            //GZipEncoder.GZipMessageEncoderFactory.GZipMessageEncoder.GZipContentType = sContentType;
                        }
                        break;

                    case "binaryMessageEncoding":
                        _sContentType = "application/soap+msbin1";
                        //GZipEncoder.GZipMessageEncoderFactory.GZipMessageEncoder.GZipContentType = sContentType;
                        break;

                        //case "MtomMessageEncoding":								
                        //break;
                }
            }
        }



        //Called by the WCF to create the binding element
        protected override BindingElement CreateBindingElement()
        {
            GZipMessageEncodingBindingElement bindingElement;

            ReadConfiguration();

            if (_sContentType == "application/soap+msbin1")
            {
                //Custom - sopa 12
                bindingElement = new GZipMessageEncodingBindingElement(new BinaryMessageEncodingBindingElement(),
                                                                       EnableCompression, _sContentType);
            }
            else
            {
                bindingElement = new GZipMessageEncodingBindingElement(GZipMessageEncoderFactory.GetTextMessageEncodingBindingElement(), EnableCompression, _sContentType);
            }

            //ApplyConfiguration(bindingElement);
            if (_sContentType == "application/soap+xml; charset=utf-8" || _sContentType == "")
            {
                bindingElement.InnerMessageEncodingBindingElement.MessageVersion = MessageVersion.Soap12;
                //bindingElement.MessageVersion = MessageVersion.Soap12WSAddressingAugust2004;
            }
            else if (_sContentType == "application/soap+msbin1")
            {
                bindingElement.InnerMessageEncodingBindingElement.MessageVersion = MessageVersion.Soap12WSAddressing10;
            }
            else
            {
                bindingElement.InnerMessageEncodingBindingElement.MessageVersion = MessageVersion.Soap11;
                //bindingElement.MessageVersion = MessageVersion.Soap11WSAddressingAugust2004;
            }
			if (bindingElement.ReaderQuotas != null && bindingElement.ReaderQuotas != null)
			{
				if (ReaderQuotas.MaxNameTableCharCount > 0) bindingElement.ReaderQuotas.MaxNameTableCharCount = ReaderQuotas.MaxNameTableCharCount;
				if (ReaderQuotas.MaxArrayLength > 0) bindingElement.ReaderQuotas.MaxArrayLength = ReaderQuotas.MaxArrayLength;
				if (ReaderQuotas.MaxBytesPerRead > 0) bindingElement.ReaderQuotas.MaxBytesPerRead = ReaderQuotas.MaxBytesPerRead;
				if (ReaderQuotas.MaxDepth > 0) bindingElement.ReaderQuotas.MaxDepth = ReaderQuotas.MaxDepth;
				if (ReaderQuotas.MaxStringContentLength > 0) bindingElement.ReaderQuotas.MaxStringContentLength = ReaderQuotas.MaxStringContentLength;
			}
	        bindingElement.EnableCompression = EnableCompression;
            //bindingElement.InnerMessageEncodingBindingElement = GZipMessageEncoderFactory.GetTextMessageEncodingBindingElement();
            return bindingElement;
        }
    }
}