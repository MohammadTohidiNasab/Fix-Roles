using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Divar.Services
{
    public class FtpService
    {
        private readonly string _ftpUsername = "Ftp_User";
        private readonly string _ftpPassword = "12345";
        private readonly string _ftpPath = @"ftp://127.0.0.1/";
        private readonly string _localPath = @"wwwroot/uploads/";

        // اپلود عکس
        public bool UploadImageToFtp(IFormFile file, int advertisementId)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";
            var uploadUrl = directoryPath + file.FileName;

            // Create directory if not exists
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
                Console.WriteLine($"خطا: {response.StatusDescription}");
                return false;
            }
        }

        // ایجاد دایرکتوری در صورت عدم وجود
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
                    Console.WriteLine($"خطا در ایجاد دایرکتوری: {response.StatusDescription}");
                }
            }
        }

        // دانلود تصاویر از سرور FTP و ذخیره آن‌ها به صورت لوکال
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

        // دانلود فایل از FTP
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






        // حذف پوشه و فایل‌های داخل آن از سرور FTP
        public bool DeleteFtpDirectory(int advertisementId)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";

            try
            {
                // حذف تمامی فایل‌های داخل پوشه
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
                            var deleteFileRequest = (FtpWebRequest)WebRequest.Create(directoryPath + fileName);
                            deleteFileRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                            deleteFileRequest.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                            deleteFileRequest.UsePassive = true;
                            deleteFileRequest.KeepAlive = false;
                            deleteFileRequest.EnableSsl = false;

                            using (var deleteFileResponse = (FtpWebResponse)deleteFileRequest.GetResponse())
                            {
                                Console.WriteLine($"Deleted File: {fileName}, status {deleteFileResponse.StatusDescription}");
                            }
                        }
                    }
                }

                // حذف پوشه
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







        // ویرایش تصاویر در سرور FTP
        public void EditImages(int advertisementId, IFormFile newImageFile1, IFormFile newImageFile2, IFormFile newImageFile3, string oldImageUrl1, string oldImageUrl2, string oldImageUrl3)
        {
            var directoryPath = _ftpPath + $"advertisement_{advertisementId}/";

            // آپلود و جایگزینی تصاویر جدید
            if (newImageFile1 != null && newImageFile1.Length > 0)
            {
                if (!string.IsNullOrEmpty(oldImageUrl1))
                {
                    DeleteImage(oldImageUrl1);
                }
                UploadImageToFtp(newImageFile1, advertisementId);
            }

            if (newImageFile2 != null && newImageFile2.Length > 0)
            {
                if (!string.IsNullOrEmpty(oldImageUrl2))
                {
                    DeleteImage(oldImageUrl2);
                }
                UploadImageToFtp(newImageFile2, advertisementId);
            }

            if (newImageFile3 != null && newImageFile3.Length > 0)
            {
                if (!string.IsNullOrEmpty(oldImageUrl3))
                {
                    DeleteImage(oldImageUrl3);
                }
                UploadImageToFtp(newImageFile3, advertisementId);
            }
        }

        // حذف تصویر از سرور FTP
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

    }
}


