using marcu5yolo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace marcu5yolo.Helpers
{
    public static class StorageHelper
    {
        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
        public static async Task<MessageModel> UploadFileToStorage(Stream fileStream1, Stream fileStream2, string fileName1, string fileName2, AzureStorageConfig _storageConfig)
        {
            string guid_this = Guid.NewGuid().ToString();
            fileName1 = guid_this + fileName1;
            fileName2 = "1" + guid_this + fileName2;
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob1 = container.GetBlockBlobReference(fileName1);
            string url1 = blockBlob1.Uri.AbsoluteUri;
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(fileName2);
            string url2 = blockBlob2.Uri.AbsoluteUri;
            // Upload the file
            
            try
            {
                await blockBlob1.UploadFromStreamAsync(fileStream1);
                await blockBlob2.UploadFromStreamAsync(fileStream2);
                return new MessageModel() {uri1 = url1,uri2 = url2, intendedClient = "flask", guid = guid_this, filename1 = fileName1, filename2 = fileName2};
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public static async Task<bool> PushToQueue(string json, AzureStorageConfig _storageConfig)
        {
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue messageQueue = queueClient.GetQueueReference("jsonqueue");
            CloudQueueMessage message = new CloudQueueMessage(json);
            try
            {
                await messageQueue.AddMessageAsync(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
