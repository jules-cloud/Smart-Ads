

using VilleInteligente.Connector;
using VilleInteligente.Models;

class Program
{
    // UPDATE logylinegestlic.t_shareservice_project SET logylinegestlic.t_shareservice_project.society_identifier = CONCAT('gestlic_', logylinegestlic.t_shareservice_project.society_id)
    // UPDATE logylinegestlic.t_shareservice_society_cloud_storage SET logylinegestlic.t_shareservice_society_cloud_storage.society_identifier = CONCAT('gestlic_', logylinegestlic.t_shareservice_society_cloud_storage.id_society)

    static void Main(string[] args)
    {
        var test = AzureConnector.GetAddsConnector();
        test.GetContainer(AzureConnector._CONTAINER_NAME_);
        byte[] data = { 0, 6, 5, 5, 58, 2, 8, 52 };
        test.CreateData(AzureConnector._CONTAINER_NAME_, new BlobData(data, "testdeblob"));
    }
}