using Autoload.PUCOMEX.Client.Entities;
using Autoload.PUCOMEX.Client.Exceptions;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Autoload.PUCOMEX.Client.Helpers
{
    public class RestClient : IDisposable
    {
        // Instância de HTTPClient para comunicação com o serviço
        public HttpClient HttpClient { get; set; }

        // Endereço base usado para consumo do serviço REST
        public string BaseAddress { get; set; }

        // Token de Segurança JWT
        public JRetornoAutenticacao Token { get; set; }

        private string EdiaType { get; set; }

        /// <param name="Environment">Dependência para leitura de dados do ambiente da aplicação</param>
        public RestClient(string baseAddress, JRetornoAutenticacao token, string ediaType = "application/json")
        {
            this.BaseAddress = baseAddress;
            this.Token = token;
            this.EdiaType = ediaType;
            this.HttpClient = this.CreateHttpClient();
        }

        /// <summary>
        /// Cria novo objeto HttpClient através do baseAddress e do handler padrão para conexão
        /// </summary>
        /// <param name="baseAddress">URL do enedereço base para conexão com o serviço REST</param>
        /// <param name="token">Toke da sessão - DEVE SER OBTIDO DO Web services de Autenticação</param>
        /// <returns>Objeto HttpClient</returns>
        public HttpClient CreateHttpClient()
        {
            try
            {
                HttpClient client = new HttpClient()
                {
                    Timeout = TimeSpan.FromMinutes(20),
                    BaseAddress = new Uri(this.BaseAddress),
                };

                /// Toke da sessão - DEVE SER OBTIDO DO Web services de Autenticação
                client.DefaultRequestHeaders.Add("Authorization", this.Token.Set_Token);
                client.DefaultRequestHeaders.Add("X-CSRF-Token", this.Token.X_CSRF_Token);


                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue(this.EdiaType));

                return client;
            }
            catch (Exception ex)
            {
                ApplicationException aEx = new ApplicationException(string.Format("Erro ao estabelecer comunicação com o serviço: \'{0}\'.", ex.Message));
                GlobalException.ReturnException(aEx);

                return null;
            }
        }

        /// <summary>
        /// Envia uma requisição POST para o endpoint formado por baseAddress e url, usando 
        /// como request body o parâmetro jsonData
        /// </summary>
        /// <param name="url">Action usada para criar resource no serviço</param>
        /// <param name="jsonData">Objeto usado para requisição</param>
        /// <returns>Resultado da requisição</returns>
        public async Task<HttpResponseMessage> Post(string url, string jsonData, string ediaType = "application/json")
        {
            try
            {
                HttpResponseMessage response = null;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, ediaType),
                };

                response = this.HttpClient.SendAsync(request).Result;

                return response;
            }
            catch (Exception ex)
            {
                ApplicationException aEx = new ApplicationException(string.Format("Erro ao enviar requisição para serviço: \'{0}\'.", ex.Message));
                GlobalException.ReturnException(aEx);

                return null;
            }
        }

        /// <summary>
        /// Libera os recursos de uso do client Http
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.HttpClient.Dispose();
        }
    }
}
