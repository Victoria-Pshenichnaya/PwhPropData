using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace PwhPropData.Core.GZipExtension
{
    public class GZipBehavior : IDispatchMessageInspector, IEndpointBehavior, IClientMessageInspector
    {
        #region Member Variables

        public const string GZIP_ENCODING = "gzip";
        /// <summary>
        /// For SOAP 1.1
        /// </summary>
        public const string TEXT_XML_UTF8 = "text/xml; charset=utf-8";
		/// <summary>
		/// For SOAP 1.2
		/// </summary>
		public const string APP_SOAP_XML = "application/soap+xml; charset=utf-8";

		/// <summary>
		/// For SOAP 1.2 Binary
		/// </summary>
		public const string APP_SOAP_MSBINARY = "application/soap+msbin1";


        public string ContentTypeHeader = string.Empty;
        public string ContentEncoding = string.Empty;
        public string AcceptEncoding = string.Empty;

        #endregion Member Variables

        #region Constructor

        public GZipBehavior()
        {
        }

        public GZipBehavior(string contentEncoding, string acceptEncoding, string contentTypeHeader)
        {
            ContentTypeHeader = contentTypeHeader;
            ContentEncoding = contentEncoding;
            AcceptEncoding = acceptEncoding;
        }

        #endregion Constructor

        #region IEndpointBehavior Members

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        #endregion

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //throw new NotImplementedException();
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty property;
            if (request.Properties.Keys.Contains(HttpRequestMessageProperty.Name))
            {
                property = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            }
            else
            {
                property = new HttpRequestMessageProperty();
                request.Properties.Add(HttpRequestMessageProperty.Name, property);
            }

            // Request compression
            if (ContentEncoding == GZIP_ENCODING)
                property.Headers[HttpRequestHeader.ContentEncoding] = GZIP_ENCODING;

            // Response compression
            if (AcceptEncoding == GZIP_ENCODING)
                property.Headers[HttpRequestHeader.AcceptEncoding] = GZIP_ENCODING;

            // Content type
            property.Headers[HttpRequestHeader.ContentType] = string.IsNullOrEmpty(ContentTypeHeader) ? APP_SOAP_XML : ContentTypeHeader;

            return null;
        }

        #endregion

        public static void Apply(ServiceEndpoint endpoint, string contentTypeHeader)
        {
            var binding = endpoint.Binding as CustomBinding;
            if (binding != null)
            {
                if (binding.Elements.OfType<GZipMessageEncodingBindingElement>().Any(e => e.EnableCompression))
                {
					endpoint.Behaviors.Add(new GZipBehavior(GZIP_ENCODING, GZIP_ENCODING, contentTypeHeader));
                }
            }
        }
    }
}