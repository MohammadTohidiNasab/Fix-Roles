using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Divar.Services
{
    public class FtpService
    {
        private readonly string _ftpUsername = "Ftp_User";
        private readonly string _ftpPassword = "12345";
        private readonly string _ftpPath = @"ftp://127.0.0.1/";
        private readonly string _localPath = @"wwwroot/uploads/";

        public bool UploadImageToFtp(IFormFile file, int advertisementId)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";
            var uploadUrl = directoryPath + file.FileName;

            CreateDirectoryIfNotExists(directoryPath);

            var request = (FtpWebRequest)WebRequest.Create(uploadUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var requestStream = request.GetRequestStream())
                {
                    file.CopyTo(requestStream);
                }

                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                }

                return true;
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
                return false;
            }
        }

        public async Task<string> UploadImageToFtpAsync(IFormFile file, int advertisementId, int imageNumber)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";
            var fileName = $"image{imageNumber}_{file.FileName}";
            var uploadUrl = directoryPath + fileName;

            await CreateDirectoryIfNotExistsAsync(directoryPath);

            var request = (FtpWebRequest)WebRequest.Create(uploadUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var requestStream = await request.GetRequestStreamAsync())
                using (var fileStream = file.OpenReadStream())
                {
                    await fileStream.CopyToAsync(requestStream);
                }

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                }

                return uploadUrl;
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
                return null;
            }
        }

        private void CreateDirectoryIfNotExists(string directoryPath)
        {
            var request = (FtpWebRequest)WebRequest.Create(directoryPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Directory created, status {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    Console.WriteLine($"Error in creating directory: {response.StatusDescription}");
                }
            }
        }

        private async Task CreateDirectoryIfNotExistsAsync(string directoryPath)
        {
            var request = (FtpWebRequest)WebRequest.Create(directoryPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    Console.WriteLine($"Directory created, status {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    Console.WriteLine($"Error in creating directory: {response.StatusDescription}");
                }
            }
        }

        public (string ImageUrl1, string ImageUrl2, string ImageUrl3) DownloadImages(int advertisementId)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";
            var localDirectoryPath = _localPath + $"advertisement_{advertisementId}/";

            if (!Directory.Exists(localDirectoryPath))
            {
                Directory.CreateDirectory(localDirectoryPath);
            }

            var imageUrls = new List<string>();

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                request.UsePassive = true;
                request.KeepAlive = false;
                request.EnableSsl = false;

                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    while (!reader.EndOfStream)
                    {
                        var imageName = reader.ReadLine();
                        if (!string.IsNullOrEmpty(imageName))
                        {
                            var remoteFileUrl = directoryPath + imageName;
                            var localFileUrl = localDirectoryPath + imageName;
                            DownloadImage(remoteFileUrl, localFileUrl);
                            imageUrls.Add("/uploads/" + $"advertisement_{advertisementId}/" + imageName);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
            }

            string imageUrl1 = imageUrls.Count > 0 ? imageUrls[0] : null;
            string imageUrl2 = imageUrls.Count > 1 ? imageUrls[1] : null;
            string imageUrl3 = imageUrls.Count > 2 ? imageUrls[2] : null;

            return (imageUrl1, imageUrl2, imageUrl3);
        }

        private void DownloadImage(string remoteFileUrl, string localFileUrl)
        {
            var request = (FtpWebRequest)WebRequest.Create(remoteFileUrl);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var fileStream = new FileStream(localFileUrl, FileMode.Create))
                {
                    responseStream.CopyTo(fileStream);
                }

                Console.WriteLine($"Downloaded File Complete, saved to {localFileUrl}");
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
            }
        }

        public bool DeleteFtpDirectory(int advertisementId)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                request.UsePassive = true;
                request.KeepAlive = false;
                request.EnableSsl = false;

                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    while (!reader.EndOfStream)
                    {
                        var fileName = reader.ReadLine();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            DeleteImage(directoryPath + fileName);
                        }
                    }
                }

                var deleteDirRequest = (FtpWebRequest)WebRequest.Create(directoryPath);
                deleteDirRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                deleteDirRequest.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                deleteDirRequest.UsePassive = true;
                deleteDirRequest.KeepAlive = false;
                deleteDirRequest.EnableSsl = false;

                using (var deleteDirResponse = (FtpWebResponse)deleteDirRequest.GetResponse())
                {
                    Console.WriteLine($"Deleted Directory: {directoryPath}, status {deleteDirResponse.StatusDescription}");
                }

                return true;
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
                return false;
            }
        }

        public void DeleteImage(string imageUrl)
        {
            var request = (FtpWebRequest)WebRequest.Create(imageUrl);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Deleted Image: {imageUrl}, status {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            var request = (FtpWebRequest)WebRequest.Create(imageUrl);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    Console.WriteLine($"Deleted Image: {imageUrl}, status {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
            }
        }

        public async Task DeleteAllImagesAsync(int advertisementId)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

                List<string> filesToDelete = new List<string>();

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string fileName;
                    while ((fileName = await reader.ReadLineAsync()) != null)
                    {
                        filesToDelete.Add(fileName);
                    }
                }

                foreach (var fileName in filesToDelete)
                {
                    await DeleteImageAsync(directoryPath + fileName);
                }

                Console.WriteLine($"All images deleted for advertisement {advertisementId}");
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error deleting images: {response.StatusDescription}");
            }
        }

        public async Task ClearLocalCacheAsync(int advertisementId)
        {
            var localDirectoryPath = Path.Combine(_localPath, $"advertisement_{advertisementId}");
            if (Directory.Exists(localDirectoryPath))
            {
                Directory.Delete(localDirectoryPath, true);
            }
            await Task.CompletedTask; // To make the method async
        }

        public string GetCacheBustedImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return null;
            }
            return $"{imageUrl}?t={DateTime.Now.Ticks}";
        }
    }
}