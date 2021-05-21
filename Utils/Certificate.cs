using Autoload.PUCOMEX.Client.Exceptions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Autoload.PUCOMEX.Client.Utils
{
    public class Certificate
    {
        /// <summary>
        /// Retorna objeto do certificado para autenticação no serviço
        /// Obtendo o certificado em um caminho físico na maquina/rede
        /// </summary>
        /// <param name="rootPath">Caminho do diretório raiz do projeto</param>
        /// <param name="filename">Nome do certificado a ser lido</param>
        /// <param name="password">Senha do certificado para extração</param>
        /// <returns>Objeto X509Certificate2 contento o certificado</returns>
        public static X509Certificate2 ObterCertificado(string rootPath, string filename, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(rootPath))
                {
                    BusinessException bEx = new BusinessException("Caminho do certificado não informado.");
                    GlobalException.ReturnException(bEx);
                }

                string filePath = Path.Combine(rootPath, filename);
                if (!File.Exists(filePath))
                {
                    BusinessException aEx = new BusinessException(string.Format("Arquivo do certificado inexistente: \'{0}\'.", filename));
                    GlobalException.ReturnException(aEx);
                }

                X509Certificate2 certificate = new X509Certificate2(filePath, password, X509KeyStorageFlags.MachineKeySet);

                return certificate;
            }
            catch (Exception ex)
            {
                ApplicationException bEx = new ApplicationException("Erro ao extrair certificado: " + ex.Message);
                GlobalException.ReturnException(bEx);

                return null;
            }
        }

        /// <summary>
        /// Retorna objeto do certificado para autenticação no serviço
        /// Obtendo o certificado registrado no repositório local do usuário do SO
        /// </summary>
        /// <returns>Objeto X509Certificate2 contento o certificado</returns>
        public static X509Certificate2 ObterCertificado(string CertificadoDigitalECPF)
        {

            X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                foreach (X509Certificate2 x509 in fcollection)
                {
                    try
                    {
                        if (x509.FriendlyName.IndexOf(CertificadoDigitalECPF) != -1)
                        {
                            //Console.WriteLine(x509.FriendlyName);
                            return x509;
                        }
                        //Console.WriteLine(x509.FriendlyName);
                    }
                    catch (CryptographicException)
                    {
                        CryptographicException cEx = new CryptographicException("As informações não puderam ser obtidas deste certificado.");
                        GlobalException.ReturnException(cEx);
                    }
                }
                BusinessException bEx = new BusinessException("Certificado não localizado.");
                GlobalException.ReturnException(bEx);

                return null;
            }
            catch (Exception ex)
            {
                ApplicationException aEx = new ApplicationException(string.Format("Erro ao extrair certificado: \'{0}\'.", ex.Message));
                GlobalException.ReturnException(aEx);

                return null;
            }
            finally
            {
                store.Close();
            }
        }

    }
}
