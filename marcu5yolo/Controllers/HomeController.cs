using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using marcu5yolo.Helpers;
using marcu5yolo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace marcu5yolo.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private IHostingEnvironment _he;
        private AzureStorageConfig _storage_config;
        private AppDbContext _db_context;

        public HomeController(IHostingEnvironment HE, IOptions<AzureStorageConfig> options, AppDbContext dbContext)
        {
            _he = HE;
            _storage_config = options.Value;
            _db_context = dbContext;
        }
        [Route("")]
        public IActionResult Index()
        {
            ViewBag.SelectedNav = "Home";
            return View();
        }

        [Route("[controller]/[action]")]
        public async Task<IActionResult> imageUploadAsync(FormViewModel viewModel)
        {
            var uploadedImage = viewModel.imageUpload;
            var ms = new MemoryStream();
            await uploadedImage.CopyToAsync(ms);
            byte[] imageContents = ms.ToArray();
            //string filepath = null;
            //if (uploadedImage != null && uploadedImage.ContentType.ToLower().StartsWith("image/"))
            //{
            //    //    var root = he.WebRootPath;
            //    //    root = root + "\\SubmittedInitiativeImg";
            //    ////same file name problems
            //    //var filename = Path.Combine(he.WebRootPath, Path.GetFileName(uploadedImage.FileName));
            //    var name = Guid.NewGuid() + Path.GetFileName(uploadedImage.FileName);
            //    var filename = Path.Combine(_he.WebRootPath, name);

            //    uploadedImage.CopyTo(new FileStream(filename, FileMode.Create));
            //    filepath = name;
            //}
            //FileInfo fi = new FileInfo(filepath);
            //string fileName = fi.Name;
            //byte[] fileContents = await System.IO.File.ReadAllBytesAsync(Path.Combine(_he.WebRootPath, filepath));

            Uri webService = new Uri(@"https://marcu5yolo19.azurewebsites.net/prediction/image");
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new ByteArrayContent(imageContents, 0, imageContents.Length), "file", "pic.jpeg" }
            };

            HttpClient client = new HttpClient();
            HttpResponseMessage result = await client.PostAsync(webService.AbsoluteUri, form);
            byte[] imageResponse = await result.Content.ReadAsByteArrayAsync();
            //ViewBag._image = File(result.Result.Content.ReadAsByteArrayAsync().Result, result.Result.GetType().ToString());
            ViewBag.imageUrl = "data:image; base64," + Convert.ToBase64String(imageResponse);
            ViewBag.SelectedNav = "Home";
            
            return View();
        }

        [Route("[controller]/[action]")]
        public IActionResult About()
        {
            ViewBag.SelectedNav = "About";
            return View();
        }

        [Route("[controller]/[action]")]
        public async Task<IActionResult> addToPipelineAsync(FormViewModel viewModel)
        {
           
            IFormFile baseImage = viewModel.imageUpload;
            IFormFile styleImage = viewModel.styleUpload;
            if (StorageHelper.IsImage(baseImage) && StorageHelper.IsImage(styleImage))
            {
                using (Stream stream = baseImage.OpenReadStream())
                {
                    using (Stream stream2 = styleImage.OpenReadStream())
                    {
                        MessageModel message = await StorageHelper.UploadFileToStorage(stream,stream2, baseImage.FileName, styleImage.FileName, _storage_config);
                        string json = JsonConvert.SerializeObject(message);
                        if (await StorageHelper.PushToQueue(json, _storage_config))
                        {
                            _db_context.entry.Add(message);
                            _db_context.SaveChanges();
                            //Uri webService = new Uri(@"https://marcu5yolo19.azurewebsites.net/prediction/image");
                            //HttpClient client = new HttpClient();
                            //await client.GetAsync(webService.AbsoluteUri);
                            return RedirectToAction("FileUploaded", "Home");
                        }
                        else
                            return RedirectToAction("Error", "Home");
                    }
                }
            }
            else
            {
                return RedirectToAction("Error", "Home", "Please try again. THe file wasn't uploaded.");
            }
           
        }

        [Route("[controller]/[action]")]
        public async Task<IActionResult> imageResultAsync()
        {
            StorageCredentials storageCredentials = new StorageCredentials(_storage_config.AccountName, _storage_config.AccountKey);
            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storage_config.ImageContainer);
            CloudBlobContainer d_container = blobClient.GetContainerReference(_storage_config.DetectedContainer);
            List<MessageModel> messageModels = _db_context.entry.ToList();
            List<ImageViewModel> imageViewModels = new List<ImageViewModel>();
            foreach(MessageModel m in messageModels)
            {
                CloudBlockBlob c1 = container.GetBlockBlobReference(m.filename1);
                CloudBlockBlob c2 = container.GetBlockBlobReference(m.filename2);
                CloudBlockBlob c3 = null;
                try
                {
                    c3 = d_container.GetBlockBlobReference(m.guid);
                }
                catch
                {
                    c3 = null;
                }
                await c1.FetchAttributesAsync();
                long fileByteLength = c1.Properties.Length;
                byte[] im1 = new byte[fileByteLength];
                await c1.DownloadToByteArrayAsync(im1,0);
                await c2.FetchAttributesAsync();
                long fileByteLength2 = c2.Properties.Length;
                byte[] im2 = new byte[fileByteLength2];
                await c2.DownloadToByteArrayAsync(im2, 0);
                byte[] im3;
                long fileByteLength3 = 0;
                if (c3 != null)
                {
                    try
                    {
                        await c3.FetchAttributesAsync();
                        fileByteLength3 = c3.Properties.Length;
                    }
                    catch (Exception e)
                    {
                        fileByteLength3 = 0;
                        Exception e1 = e;

                    }



                }

                im3 = new byte[fileByteLength3];
                if(fileByteLength3>0)
                {
                    
                    await c3.DownloadToByteArrayAsync(im3, 0);
                }
                ImageViewModel imageView = new ImageViewModel() { image1 = "data:image; base64," + Convert.ToBase64String(im1), image2 = "data:image; base64,"  + Convert.ToBase64String(im2)};
                if (fileByteLength3 > 0)
                    imageView.result = "data:image; base64,"  + Convert.ToBase64String(im3);
                imageViewModels.Add(imageView);
            }
            ViewBag.images = imageViewModels;
            return View();
        }

        [Route("[controller]/[action]")]
        public IActionResult FileUploaded()
        {
            return View();
        }

        [Route("[controller]/[action]")]
        public IActionResult Error(string errorMessage)
        {
            return View();
        }
    }
}
