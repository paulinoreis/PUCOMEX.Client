using Autoload.PUCOMEX.Client.Entities;
using Autoload.PUCOMEX.Client.Exceptions;
using Autoload.PUCOMEX.Client.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Autoload.PUCOMEX.Client.Helpers
{
    public class AuthClient
    {
        private static volatile AuthClient instance = null;
        private static readonly AuthClient syncRoot = new AuthClient();
        private DateTime HoraExpiracaoToken;
        private string Token;

        private readonly string CERTFILE;
        private readonly string PASSWORD;
        private readonly string ROOTPATH;
        private readonly int ValidadeTokenMinutos;
        private readonly string Certificado_Digital;


        // Instância de HTTPClient para comunicação com o serviço
        public HttpClient HttpClient { get; set; }


        /// <summary>
        /// Singleton
        /// </summary>
        public static AuthClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new AuthClient();
                    }
                }

                return instance;
            }
        }

        /// <param name="Environment">Dependência para leitura de dados do ambiente da aplicação</param>
        private AuthClient()
        {
            this.CERTFILE = Util.ObterParametro<string>("PUCOMEX.CERTFILE");
            this.PASSWORD = Util.ObterParametro<string>("PUCOMEX.PASSWORD");
            this.ROOTPATH = Util.ObterParametro<string>("PUCOMEX.ROOTPATH");
            this.ValidadeTokenMinutos = int.Parse(Util.ObterParametro<long>("PUCOMEX.Validade.Sessao.Token").ToString());
            this.Certificado_Digital = Util.ObterParametro<string>("PUCOMEX.CNPJ.CertificadoDigital");
        }


        public string GetToken()
        {
            return this.Token;
        }

        /// <summary>
        /// Cria novo objeto HttpClient através do baseAddress e do handler padrão para conexão
        /// </summary>
        /// <param name="baseAddress">URL do enedereço base para conexão com o serviço REST</param>
        /// <returns>Objeto HttpClient</returns>
        public void CreateHttpClient(string baseAddress)
        {
            try
            {
                HttpClientHandler handler = this.GetHttpClientHandler();
                HttpClient client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMinutes(1),
                    BaseAddress = new Uri(baseAddress),
                };

                client.DefaultRequestHeaders.Add("Role-Type", "DEPOSIT");

                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                this.HttpClient = client;
            }
            catch (Exception ex)
            {
                ApplicationException aEx = new ApplicationException(string.Format("Erro ao estabelecer comunicação com o serviço: \'{0}\'.", ex.Message));
                GlobalException.ReturnException(aEx);
            }
        }

        /// <summary>
        /// Configura o handler para o HttpClient, usando certificado e protocolos SSL
        /// </summary>
        /// <returns>Objeto HttpClientHandler para configurar conexão http</returns>
        private HttpClientHandler GetHttpClientHandler()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                HttpClientHandler handler = new HttpClientHandler
                {
                    MaxConnectionsPerServer = 9999,
                    //SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
                };

                /// Obtendo o certificado em um caminho físico na maquina/rede
                X509Certificate2 certificate = Certificate.ObterCertificado(ROOTPATH, CERTFILE, PASSWORD);

                /// Obtendo o certificado registrado no repositório local do usuário no SO
                //X509Certificate2 certificate = Certificate.ObterCertificado(Certificado_Digital);

                _ = handler.ClientCertificates.Add(certificate);
                handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, errors) =>
                {
                    //if (Config.LerConfiguracao("AmbDEV").Equals("1"))
                    return true; // No ambiente de Desenvolvimento, trust all certificates
                    //return errors == SslPolicyErrors.None; // Compliant: trust only some certificates
                };

                return handler;
            }
            catch (Exception ex)
            {
                ApplicationException aEx = new ApplicationException(ex.Message);
                GlobalException.ReturnException(aEx);

                return null;
            }
        }

        /// <summary>
        /// Envia uma requisição POST para o endpoint formado por baseAddress e url
        /// </summary>
        /// <param name="url">Action usada para criar resource no serviço</param>
        /// <returns>Resultado da requisição</returns>
        public async Task<JRetornoAutenticacao> Post(string url)
        {
            JRetornoAutenticacao retorno = null;
            try
            {
                HttpResponseMessage response = null;

                if (this.HoraExpiracaoToken <= DateTime.Now || string.IsNullOrWhiteSpace(this.Token))
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

                    response = this.HttpClient.SendAsync(request).Result;

                    if ((int)response.StatusCode != 200)
                    {
                        ApplicationException aEx = new ApplicationException(response.ReasonPhrase);
                        GlobalException.ReturnException(aEx);
                    }

                    this.HoraExpiracaoToken = DateTime.Now.AddMinutes(this.ValidadeTokenMinutos);

                    string operationLocation = response.Headers.GetValues("Set-Token").FirstOrDefault();

                    retorno = new JRetornoAutenticacao
                    {
                        Set_Token = response.Headers.GetValues("Set-Token").FirstOrDefault(),
                        X_CSRF_Expiration = response.Headers.GetValues("X-CSRF-Expiration").FirstOrDefault(),
                        X_CSRF_Token = response.Headers.GetValues("X-CSRF-Token").FirstOrDefault()
                    };
                }

                return retorno;
            }
            catch (Exception ex)
            {
                ApplicationException aEx = new ApplicationException(string.Format("Erro ao enviar requisição para serviço: \'{0}\'.", ex.Message));
                GlobalException.ReturnException(aEx);

                return null;
            }
        }
    }
}
