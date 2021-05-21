using Autoload.PUCOMEX.Client.Entities;
using Autoload.PUCOMEX.Client.Exceptions;
using Autoload.PUCOMEX.Client.Helpers;
using Automind.Autoload.Dados.Modulos.PUCOMEX;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using Autoload.PUCOMEX.Client.Utils;

namespace Autoload.PUCOMEX.Client
{
    public class PUCOMEXClient : IPUCOMEXClient
    {
        private readonly string URLBase;

        public PUCOMEXClient()
        {
            this.URLBase = Util.ObterParametro<string>("PUCOMEX.URLBase");
        }

        private JRetornoAutenticacao Autenticar()
        {
            /// EndPoint para Autenticação
            string lEndPoint = URLBase + Util.ObterParametro<string>("PUCOMEX.EndPointAutentica");
            AuthClient client = AuthClient.Instance;
            client.CreateHttpClient(lEndPoint);

            return client.Post(lEndPoint).Result;
        }

        public string ComunicarRecepcoesNFE(RecepcoesNFE recepcoesNfeXML)
        {
            /// EndPoint
            string lEndPoint = URLBase + Util.ObterParametro<string>("PUCOMEX.EndPointComunicarRecepcoesNFE");

            RestClient client = new RestClient(lEndPoint, Autenticar(), "application/xml");


            string text = Util.Serializar<RecepcoesNFE>(recepcoesNfeXML, "http://www.pucomex.serpro.gov.br/cct"); //##// Buscar de parametro NameSpace do XML da integração PUCOMEX

            using (client)
            {
                HttpResponseMessage response = client.Post(lEndPoint, text, "application/xml").Result;

                int StatusCode = (int)response.StatusCode;
                if (!StatusCode.Equals(200))
                {
                    BusinessException ex = new BusinessException(response.Content.ReadAsStringAsync().Result);
                    GlobalException.ReturnException(ex);
                }

                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public string ComunicarEntregaDocumentoCarga(EntregasDocumentoCarga entregasDocumentoCarga)
        {
            /// EndPoint
            string lEndPoint = URLBase + Util.ObterParametro<string>("PUCOMEX.EndPointComunicarEntregaDocumentoCarga");

            RestClient client = new RestClient(lEndPoint, Autenticar(), "application/xml");

            string text = Util.Serializar<EntregasDocumentoCarga>(entregasDocumentoCarga, "http://www.pucomex.serpro.gov.br/cct"); //##// Buscar de parametro NameSpace do XML da integração PUCOMEX


            using (client)
            {
                HttpResponseMessage response = client.Post(lEndPoint, text, "application/xml").Result;

                int StatusCode = (int)response.StatusCode;
                if (!StatusCode.Equals(200))
                {
                    BusinessException ex = new BusinessException(response.Content.ReadAsStringAsync().Result);
                    GlobalException.ReturnException(ex);
                }

                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
