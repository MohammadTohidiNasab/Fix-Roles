namespace Divar.Controllers
{
    public class FtpController : Controller
    {
        private const string FtpUsername = "Ftp_User";
        private const string FtpPassword = "12345";
        private const string FtpPath = @"ftp://127.0.0.1/";

        public IActionResult Index()
        {
            List<string> photos = GetFtpFiles();
            return View(photos);
        }

        private List<string> GetFtpFiles()
        {
            List<string> fileNames = new List<string>();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FtpPath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(FtpUsername, FtpPassword);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                while (!reader.EndOfStream)
                {
                    string fileName = reader.ReadLine();
                    if (fileName.EndsWith(".jpg") || fileName.EndsWith(".png")) // فقط تصاویر
                    {
                        fileNames.Add(fileName);
                    }
                }
            }
            return fileNames;
        }

        public IActionResult ViewImage(string fileName)
        {
            string filePath = $"{FtpPath}{fileName}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filePath);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(FtpUsername, FtpPassword);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                MemoryStream memoryStream = new MemoryStream();
                responseStream.CopyTo(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                return File(imageBytes, "image/jpeg"); // یا "image/png" بسته به نوع تصویر
            }
        }

    }
}
