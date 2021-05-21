using Automind.Autoload.Dados.Modulos.PUCOMEX;

namespace Autoload.PUCOMEX.Client
{
    public interface IPUCOMEXClient
    {
        string ComunicarRecepcoesNFE(RecepcoesNFE recepcoesNfeXML);

        string ComunicarEntregaDocumentoCarga(EntregasDocumentoCarga entregasDocumentoCarga);
    }
}
