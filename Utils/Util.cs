using Automind.Autoload.BLL;
using Automind.Autoload.Dados.Entidades.Geral;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Autoload.PUCOMEX.Client.Utils
{
    public class Util
    {
        public static bool RetornoComErro(string sValor)
        {
            bool retorno = false;

            Match m = Regex.Match(sValor, @"\[ER\d{2}?");

            retorno = m.Success;

            return retorno;
        }

        public static T ObterParametro<T>(string a_ParametroID)
        {
            TDSTabelasGeral l_DSTerminal = new TDSTabelasGeral();
            Type itemType = typeof(T);
            object value = null;

            ParametrosConsultaObterParametrosSistema l_ParametrosSistema = new ParametrosConsultaObterParametrosSistema();
            l_ParametrosSistema.ParametroID = a_ParametroID;

            Fachada.Instance.Geral.ObterParametrosSistema(l_DSTerminal, l_ParametrosSistema);


            if (itemType == typeof(string))
            {
                value = l_DSTerminal.Parametros[0].ValorEmString;
            }
            else if (itemType == typeof(long))
            {
                value = l_DSTerminal.Parametros[0].ValorEmLong;
            }
            else
            {
                throw new Exception("Erro ao obter parametros do sistema.");
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string Serializar<T>(object a_MensagemEnvio, string a_NameSpace)
        {
            string text;
            using (StringWriter writer = new Utf8StringWriter())
            {
                var serializer = new XmlSerializer(typeof(T), a_NameSpace);
                serializer.Serialize(writer, a_MensagemEnvio);
                text = writer.ToString();
            }

            return text;
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
