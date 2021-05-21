using Newtonsoft.Json;

namespace Autoload.PUCOMEX.Client.Entities
{
    public class JRetornoAutenticacao
    {
        [JsonProperty("Set-Token")]
        public string Set_Token { get; set; }

        [JsonProperty("X-CSRF-Token")]
        public string X_CSRF_Token { get; set; }

        [JsonProperty("X-CSRF-Expiration")]
        public string X_CSRF_Expiration { get; set; }
    }
}
