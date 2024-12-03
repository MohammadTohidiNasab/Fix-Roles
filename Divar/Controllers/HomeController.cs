namespace Divar.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAdvertisementRepository _adRepository;
        private readonly int pageSize = 8;
        private readonly FtpService _ftpService;

        public HomeController(IAdvertisementRepository adRepository, FtpService ftpService)
        {
            _adRepository = adRepository;
            _ftpService = ftpService;
        }



        // Show all advertisements
        public async Task<IActionResult> Index(int pageNumber = 1, CategoryType? category = null, string searchTerm = "")
        {
            var totalAds = await _adRepository.GetTotalAdvertisementsCountAsync(category, searchTerm);
            var totalPages = (int)Math.Ceiling((double)totalAds / pageSize);
            var ads = await _adRepository.GetAllAdvertisementsAsync(pageNumber, pageSize, category, searchTerm);

            foreach (var ad in ads)
            {
                var (imageUrl1, imageUrl2, imageUrl3) = _ftpService.DownloadImages(ad.Id);
                ad.ImageUrl = imageUrl1 ?? ad.ImageUrl; // تنظیم اولین عکس به‌عنوان ImageUrl
            }

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearchTerm = searchTerm;

            return View(ads);
        }



        //جست و جو میان تیتر اگهی ها 
        public async Task<IActionResult> Search(string searchTerm)
        {
            // Redirect to Index action with pageNumber set to 1
            return RedirectToAction("Index", new { pageNumber = 1, searchTerm = searchTerm });
        }


        //جزییات اگهی
        public async Task<IActionResult> Detail(int id)
        {
            var ad = await _adRepository.GetAdvertisementByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            var (imageUrl1, imageUrl2, imageUrl3) = _ftpService.DownloadImages(id);

            ViewBag.ImageUrl1 = imageUrl1;
            ViewBag.ImageUrl2 = imageUrl2;
            ViewBag.ImageUrl3 = imageUrl3;

            return View(ad);
        }




        //ایجاد اگهی
        [HttpGet]
        [Authorize(Policy = "RequireHomeCreateAdvertisement")]
        public IActionResult Create()
        {
            var selectedCategory = HttpContext.Session.GetString("SelectedCategory");
            if (string.IsNullOrEmpty(selectedCategory))
            {
                return RedirectToAction("SelectCategory");
            }

            var category = Enum.Parse<CategoryType>(selectedCategory);
            var model = new Advertisement { Category = category };
            return View(model);
        }



        [HttpPost]
        [Authorize(Policy = "RequireHomeCreateAdvertisement")]
        public async Task<IActionResult> Create(Advertisement advertisement, IFormFile? imageFile, IFormFile? imageFile2, IFormFile? imageFile3)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (ModelState.IsValid)
            {
                advertisement.CustomUserId = userId;
                advertisement.CreatedDate = DateTime.Now;
                await _adRepository.AddAdvertisementAsync(advertisement);

                List<IFormFile> imageFiles = new List<IFormFile> { imageFile, imageFile2, imageFile3 };
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    var image = imageFiles[i];
                    if (image != null && image.Length > 0)
                    {
                        var result = _ftpService.UploadImageToFtp(image, advertisement.Id);
                        if (result)
                        {
                            if (i == 0)
                            {
                                advertisement.ImageUrl = $"ftp://127.0.0.1/advertisement_{advertisement.Id}/" + image.FileName;
                            }
                            else if (i == 1)
                            {
                                advertisement.ImageUrl2 = $"ftp://127.0.0.1/advertisement_{advertisement.Id}/" + image.FileName;
                            }
                            else if (i == 2)
                            {
                                advertisement.ImageUrl3 = $"ftp://127.0.0.1/advertisement_{advertisement.Id}/" + image.FileName;
                            }
                        }
                    }
                }

                await _adRepository.UpdateAdvertisementAsync(advertisement);

                // پاک کردن session بعد از ذخیره آگهی
                HttpContext.Session.Remove("SelectedCategory");

                return RedirectToAction("Index");
            }
            return View(advertisement);
        }





        //انتخاب دسته بندی
        [HttpGet]
        [Authorize(Policy = "RequireHomeCreateAdvertisement")]
        public IActionResult SelectCategory()
        {
            var categories = Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>();
            return View(categories);
        }

        //اعمال دسته بندی انتخاب شده
        [HttpGet]
        [Authorize(Policy = "RequireHomeCreateAdvertisement")]
        public IActionResult SetCategory(CategoryType category)
        {
            HttpContext.Session.SetString("SelectedCategory", category.ToString());
            return RedirectToAction("Create");
        }






        // ویرایش اگهی ها
        [HttpGet]
        [Authorize(Policy = "RequireHomeEditAdvertisement")]
        public async Task<IActionResult> Edit(int id)
        {
            var ad = await _adRepository.GetAdvertisementByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            // ارسال نام فایل‌ها به ویو
            ViewBag.ImageFileName1 = Path.GetFileName(ad.ImageUrl);
            ViewBag.ImageFileName2 = Path.GetFileName(ad.ImageUrl2);
            ViewBag.ImageFileName3 = Path.GetFileName(ad.ImageUrl3);

            return View(ad);
        }



        [HttpPost]
        [Authorize(Policy = "RequireHomeEditAdvertisement")]
        public async Task<IActionResult> Edit(int id, Advertisement updatedAdvertisement, IFormFile? newImageFile1, IFormFile? newImageFile2, IFormFile? newImageFile3)
        {
            if (ModelState.IsValid)
            {
                var ad = await _adRepository.GetAdvertisementByIdAsync(id);
                if (ad == null)
                {
                    return NotFound();
                }

                // به‌روزرسانی فیلدهای آگهی
                ad.Title = updatedAdvertisement.Title;
                ad.Content = updatedAdvertisement.Content;
                ad.Price = updatedAdvertisement.Price;
                ad.SimCardsNumber = updatedAdvertisement.SimCardsNumber;
                ad.MobileBrand = updatedAdvertisement.MobileBrand;
                ad.BookAuthor = updatedAdvertisement.BookAuthor;
                ad.CarBrand = updatedAdvertisement.CarBrand;
                ad.GearboxType = updatedAdvertisement.GearboxType;
                ad.HomeAddress = updatedAdvertisement.HomeAddress;
                ad.HomeSize = updatedAdvertisement.HomeSize;

                // حذف تمام تصاویر موجود
                await _ftpService.DeleteAllImagesAsync(ad.Id);

                // آپلود تصاویر جدید یا حفظ تصاویر موجود
                ad.ImageUrl = await UpdateImageAsync(newImageFile1, ad.Id, 1, ad.ImageUrl);
                ad.ImageUrl2 = await UpdateImageAsync(newImageFile2, ad.Id, 2, ad.ImageUrl2);
                ad.ImageUrl3 = await UpdateImageAsync(newImageFile3, ad.Id, 3, ad.ImageUrl3);

                await _adRepository.UpdateAdvertisementAsync(ad);

                // پاک کردن کش محلی برای این آگهی
                await _ftpService.ClearLocalCacheAsync(ad.Id);

                return RedirectToAction(nameof(Index));
            }
            return View(updatedAdvertisement);
        }

        private async Task<string> UpdateImageAsync(IFormFile newImageFile, int advertisementId, int imageNumber, string existingImageUrl)
        {
            if (newImageFile != null && newImageFile.Length > 0)
            {
                return await _ftpService.UploadImageToFtpAsync(newImageFile, advertisementId, imageNumber);
            }
            else if (!string.IsNullOrEmpty(existingImageUrl))
            {
                // بارگذاری مجدد تصویر موجود
                var fileName = Path.GetFileName(existingImageUrl);
                var localPath = Path.Combine("wwwroot", "uploads", $"advertisement_{advertisementId}", fileName);
                if (System.IO.File.Exists(localPath))
                {
                    using var fileStream = new FileStream(localPath, FileMode.Open);
                    var formFile = new FormFile(fileStream, 0, fileStream.Length, null, fileName);
                    return await _ftpService.UploadImageToFtpAsync(formFile, advertisementId, imageNumber);
                }
            }
            return null;
        }





        // حذف اگهی ها
        [Authorize(Policy = "RequireHomeDeleteAdvertisement")]
        public async Task<IActionResult> Delete(int id)
        {
            var ad = await _adRepository.GetAdvertisementByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }


        // تایید حذف اگهی
        [Authorize(Policy = "RequireHomeDeleteAdvertisement")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ad = await _adRepository.GetAdvertisementByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            // حذف پوشه تصاویر مرتبط با آگهی از سرور FTP
            _ftpService.DeleteFtpDirectory(ad.Id);

            // حذف آگهی از پایگاه داده
            await _adRepository.DeleteAdvertisementAsync(id);

            return RedirectToAction(nameof(Index));
        }




        // داشبورد کاربر 
        public async Task<IActionResult> Dashboard(int pageNumber = 1)
        {
            var userId = HttpContext.Session.GetString("UserId"); 
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var totalAds = await _adRepository.GetTotalAdvertisementsCountByUserIdAsync(userId);
            var totalPages = (int)Math.Ceiling((double)totalAds / pageSize);

            var ads = await _adRepository.GetAdvertisementsByUserIdAsync(userId, pageNumber, pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(ads);
        }
    }
}
