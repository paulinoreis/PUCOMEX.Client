using System.Configuration;
using System.IO;
using System.Reflection;

namespace Autoload.PUCOMEX.Client.Utils
{
    public class Config
    {
        private static Configuration i_Config;

        private Config()
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            string l_FolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            configFileMap.ExeConfigFilename = Path.Combine(l_FolderPath, "WsApiPUCOMEX.config");

            // Get the mapped configuration file.
            i_Config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
        }

        public static string LerConfiguracao(string l_Chave)
        {
            if (i_Config == null)
            {
                new Config();
            }

            return i_Config.AppSettings.Settings[l_Chave].Value;
        }
    }
}
