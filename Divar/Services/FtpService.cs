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

        public List<string> GetFtpImageList()
        {
            var request = (FtpWebRequest)WebRequest.Create(_ftpPath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            var fileList = new List<string>();

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    while (!reader.EndOfStream)
                    {
                        fileList.Add(reader.ReadLine());
                    }
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                Console.WriteLine($"Error: {response.StatusDescription}");
            }

            return fileList;
        }



        public bool UploadImageToFtp(IFormFile file)
        {
            var uploadUrl = _ftpPath + file.FileName;
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

        public bool DeleteImageFromFtp(string fileName)
        {
            var deleteUrl = _ftpPath + fileName;
            var request = (FtpWebRequest)WebRequest.Create(deleteUrl);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UsePassive = true;
            request.KeepAlive = false;
            request.EnableSsl = false;

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Delete File Complete, status {response.StatusDescription}");
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
    }
}
