using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VilleInteligente.Models;

namespace VilleInteligente.Connector
{
    public class AzureConnector
    {
        private const string _CONNECTION_STRING_LC3DRESOURCE_ = "DefaultEndpointsProtocol=https;AccountName=smartadds;" +
            "AccountKey=rEUWt7ie8qcTz8RFgLGaDhmEq4bBKhoFzTv1FCm6w43vVa2P/1mi80kr0rCsoWXFLrt5+4WgIl3d+AStXQDfKw==;EndpointSuffix=core.windows.net";
        public const string _CONTAINER_NAME_ = "smartadds-videos";
        private BlobServiceClient _blobServiceClient;
        private string _connexionString;

        public static AzureConnector GetAddsConnector() => new AzureConnector(_CONNECTION_STRING_LC3DRESOURCE_);

        public AzureConnector() : this(_CONNECTION_STRING_LC3DRESOURCE_)
        {

        }

        public AzureConnector(string connectionString)
        {
            _connexionString = connectionString;
            _blobServiceClient = new BlobServiceClient(_connexionString);
            ;
        }

        /// <summary>
        /// Creer un conteneur Azure par son nom
        /// </summary>
        /// <param name="containerName"></param>
        public bool CreateContainer(string containerName)
        {

            bool res;
            try
            {
                res = true;
                BlobContainerClient cont = _blobServiceClient.CreateBlobContainer(containerName);
            }
            catch
            {
                res = false;
            }
            return res;
        }

        /// <summary>
        /// Créer un conteneur Azure par le biais d'une instance Conteneur en envoyant les data qu'il contient
        /// </summary>
        /// <param name="cont"></param>
        public bool CreateContainer(Container cont)
        {
            bool res;
            try
            {
                //Création du conteneur par son nom
                _blobServiceClient.CreateBlobContainer(cont.ContainerName);

                //Envoi de la liste de donnée dans le conteneur azure crée
                if (cont.ContainerData != null)
                {
                    foreach (BlobData d in cont.ContainerData)
                    {
                        CreateData(cont.ContainerName, d);
                    }
                }
                res = true;
            }
            catch
            {
                res = false;
            }
            return res;
        }

        /// <summary>
        /// Créer un Blob Azure avec le nom du conteneur associé, le nom de la data et le contenu de la data
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="dataName"></param>
        /// <param name="data"></param>
        public bool CreateData(string containerName, string dataName, byte[] data)
        {
            if (!ContainerExist(containerName))
            {
                if (!CreateContainer(new Container(containerName)))
                {
                    return false;
                }
            }
            return !(UploadData(containerName, dataName, data) is null);
        }

        private BlobClient UploadData(string containerName, string dataName, byte[] data)
        {
            try
            {
                BlobClient blobData = new BlobClient(_connexionString, containerName, dataName);
                IDictionary<string, string> metadata = new Dictionary<string, string>();
                Stream s = new MemoryStream();
                s.Write(data, 0, data.Length);
                s.Position = 0;
                using (s)
                {
                    blobData.Upload(s, true);
                }
                return blobData;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Créer un Blob Azure avec le nom du conteneur associé, le nom de la data et le contenu de la data
        /// retourne l'URI safe
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="dataName"></param>
        /// <param name="data"></param>
        public string CreateTemporaryData(byte[] data, string containerName, string dataName, string fileNameWithExtension, int availableTimeInMinute)
        {
            string res;
            if (!ContainerExist(containerName))
            {
                if (!CreateContainer(new Container(containerName)))
                {
                    return null;
                }
            }
            //return false;
            try
            {
                BlobClient blobData = UploadData(containerName, dataName, data);
                return GetTemporaryURL(blobData, fileNameWithExtension, availableTimeInMinute);
            }
            catch
            {
                res = null;
            }
            return res;
        }

        /// <summary>
        /// Créer un Blob Azure à partir d'une instance Data
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="data"></param>
        public bool CreateData(string containerName, BlobData data)
        {
            if (data == null || data.DataContent == null || data.DataContent.Length == 0)
                return false;
            return CreateData(containerName, data.DataName, data.DataContent);
        }

        public bool DeleteContainer(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName).DeleteIfExists();
        }

        public bool DeleteData(string containerName, string dataName)
        {
            bool res;

            try
            {
                var data = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(dataName);
                if (!DataExist(containerName, dataName))
                    return false;

                data.Delete();
                res = true;
            }
            catch
            {
                res = false;
            }
            return res;

        }

        /// <summary>
        /// Récupère les datas (byte[]) d'un conteneur donné
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public List<byte[]> GetContainer(string containerName)
        {
            List<byte[]> listData = new List<byte[]>();

            if (ContainerExist(containerName))
            {
                foreach (BlobItem data in _blobServiceClient.GetBlobContainerClient(containerName).GetBlobs())
                {
                    listData.Add(GetData(containerName, data.Name));
                }

            }

            if (listData.Count == 0)
                return null;

            return listData;
        }

        /// <summary>
        /// récupère une data par son nom et le nom du conteneur associé
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="dataName"></param>
        /// <returns></returns>
        public byte[] GetData(string containerName, string dataName)
        {
            byte[] dataContent;
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(dataName))
                return null;

            BlobContainerClient blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
            if (!blobContainer.Exists())
                return null;

            BlobClient client = blobContainer.GetBlobClient(dataName);
            if (!client.Exists())
                return null;

            client.GetProperties();

            BlobDownloadInfo download = client.Download();

            using (BinaryReader br = new BinaryReader(download.Content))
            {
                dataContent = br.ReadBytes((int)download.ContentLength);
            }

            return dataContent;
        }

        /// <summary>
        /// Renvoi la liste de nom de tous les conteneurs Azure
        /// </summary>
        /// <returns></returns>
        public List<string> ListAllContainer()
        {
            List<string> listName = new List<string>();
            foreach (BlobContainerItem item in _blobServiceClient.GetBlobContainers())
            {
                listName.Add(item.Name);
            }
            return listName;
        }

        /// <summary>
        /// Renvoi la liste de nom de toutes les data d'un conteneur Azure
        /// </summary>
        /// <param name="idProject"></param>
        /// <returns></returns>

        /// Nom de la fonction modifié parce qu'on ne devrait pas faire de spécificité sur la publication projet aussi haut (_CONTAINER_NAME_)
        /// -> on casse la compilation pour forcer à changer et repasser sur un truc propre
        public List<string> ListAllDatas(string idProject)
        {
            List<string> listData = new List<string>();

            foreach (BlobItem data in _blobServiceClient.GetBlobContainerClient(_CONTAINER_NAME_).GetBlobs())
            {
                if (data.Name.Contains(idProject))
                    listData.Add(data.Name);
            }

            if (listData.Count == 0)
                return null;

            return listData;
        }

        //liste tout les blobs dans contenant le path indiqué. 
        //le path doit être écrit avec des slach normaux "/".
        public List<string> ListAllData(string containerName, string path)
        {
            List<string> listData = new List<string>();
            var test = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobs();
            foreach (BlobItem data in test)
            {
                if (data.Name.Contains(path))
                    listData.Add(data.Name);
            }

            return listData;
        }

        /// Nom de la fonction modifié parce qu'on ne devrait pas faire de spécificité sur la publication projet aussi haut (_CONTAINER_NAME_)
        /// -> on casse la compilation pour forcer à changer et repasser sur un truc propre
        public bool DeleteAllDatasProject(string idproject)
        {
            bool ret = true;
            try
            {
                List<string> listBlob = ListAllDatas(idproject);
                if (listBlob != null)
                {
                    foreach (string blobName in listBlob)
                    {
                        if (!DeleteData(_CONTAINER_NAME_, blobName))
                            return false;
                    }
                }


            }
            catch
            {
                return false;
            }
            return ret;

        }

        public bool ContainerExist(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                return false;
            try
            {
                BlobContainerClient blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
                blobContainer.CreateIfNotExists();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool DataExist(string containerName, string dataName)
        {
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(dataName))
                return false;

            return _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(dataName).Exists();

        }

        /// <summary>
        /// crée une url vers le blob passé en paramétre avec (SAS) shared access signature pour un temps limiter.
        /// </summary>
        /// <param name="blobClient">le blob</param>
        /// <param name="fileNameWithExtension">nom du fichier au téléchargement du blob</param>
        /// <param name="availableTimeInMinute">durée de validité du lien</param>
        /// <returns></returns>
        private string GetTemporaryURL(BlobClient blobClient, string fileNameWithExtension, int availableTimeInMinute)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(availableTimeInMinute),
                ContentDisposition = "attachment; filename=" + fileNameWithExtension,
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).AbsoluteUri;
        }

        public string GetTemporaryURL(string containerName, string dataName, string fileNameWithExtension, int availableTimeInMinute)
        {
            try
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = container.GetBlobClient(dataName);
                return GetTemporaryURL(blobClient, fileNameWithExtension, availableTimeInMinute);
            }
            catch
            {
                return null;
            }
        }

        public string GetTemporaryAccessContainer(string containerName, DateTime dateLimit)
        {
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);
            Uri sas = container.GenerateSasUri(BlobContainerSasPermissions.All, dateLimit);
            return sas.Query;
        }

        public DateTimeOffset GetDataLastModifiedDate(string containerName, string dataName)
        {
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = container.GetBlobClient(dataName);
            return blobClient.GetProperties().Value.LastModified;
        }
    }
}
