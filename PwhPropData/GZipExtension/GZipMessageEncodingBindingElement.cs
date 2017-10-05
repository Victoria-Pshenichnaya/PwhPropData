using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using PwhPropData.Core.Common;

namespace PwhPropData.Core.GZipExtension
{
    /// <summary>
    /// This is the binding element that, when plugged into a custom binding,
    /// will enable the GZip encoder
    /// </summary>
    public sealed class GZipMessageEncodingBindingElement
        : MessageEncodingBindingElement //BindingElement
          , IPolicyExportExtension
    {
        public bool EnableCompression;

        //We will use an inner binding element to store information required for the inner encoder
        MessageEncodingBindingElement _innerBindingElement;

        //By default, use the default text encoder as the inner encoder
        public GZipMessageEncodingBindingElement()
            : this(GZipMessageEncoderFactory.GetTextMessageEncodingBindingElement(), false, "") { }

        public GZipMessageEncodingBindingElement(bool enableCompression)
            : this(GZipMessageEncoderFactory.GetTextMessageEncodingBindingElement(), enableCompression, "")
        {
            EnableCompression = enableCompression;
        }

        public GZipMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement, bool enableCompression, string sContentType)
        {
            EnableCompression = enableCompression;
            if (!String.IsNullOrEmpty(sContentType))
            {
                if (sContentType.Equals("application/soap+xml; charset=utf-8"))
                    messageEncoderBindingElement.MessageVersion = MessageVersion.Soap12;
                else if (sContentType.Equals("application/soap+msbin1"))
                    messageEncoderBindingElement.MessageVersion = MessageVersion.Soap12WSAddressing10;
                else
                    messageEncoderBindingElement.MessageVersion = MessageVersion.Soap11;
            }
            _innerBindingElement = messageEncoderBindingElement;
        }

        public MessageEncodingBindingElement InnerMessageEncodingBindingElement
        {
            get { return _innerBindingElement; }
            set { _innerBindingElement = value; }
        }

        //Main entry point into the encoder binding element. Called by WCF to get the factory that will create the
        //message encoder
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new GZipMessageEncoderFactory(_innerBindingElement.CreateMessageEncoderFactory(), EnableCompression);
        }

		public XmlDictionaryReaderQuotas ReaderQuotas
		{
			get
			{
				BinaryMessageEncodingBindingElement el1 = this._innerBindingElement as BinaryMessageEncodingBindingElement;
				if (el1 != null) return el1.ReaderQuotas;
				TextMessageEncodingBindingElement el2 = this._innerBindingElement as TextMessageEncodingBindingElement;
				if (el2 != null) return el2.ReaderQuotas;
				return null;
			}
		} 

        public override MessageVersion MessageVersion
        {
            get { return _innerBindingElement.MessageVersion; }
            set { _innerBindingElement.MessageVersion = value; }
        }

        public override BindingElement Clone()
        {
            return new GZipMessageEncodingBindingElement(this._innerBindingElement, EnableCompression, "");
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
                return _innerBindingElement.GetProperty<T>(context);
            
            return base.GetProperty<T>(context);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            Guard.NotNull(context, "context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            Guard.NotNull(context, "context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            Guard.NotNull(context, "context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            Guard.NotNull(policyContext, "policyContext");

            var document = new XmlDocument();
            policyContext.GetBindingAssertions().Add(document.CreateElement(
                GZipMessageEncodingPolicyConstants.GZipEncodingPrefix,
                GZipMessageEncodingPolicyConstants.GZipEncodingName,
                GZipMessageEncodingPolicyConstants.GZipEncodingNamespace));
        }
    }
}