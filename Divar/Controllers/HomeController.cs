using Divar.Services;
using Microsoft.AspNetCore.Authorization;

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




        public async Task<IActionResult> Search(string searchTerm)
        {
            // Redirect to Index action with pageNumber set to 1
            return RedirectToAction("Index", new { pageNumber = 1, searchTerm = searchTerm });
        }


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
        [Authorize(Policy = "RequireHomeSelectCategory")]
        [HttpGet]
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

        [Authorize(Policy = "RequireHomeSelectCategory")]
        [HttpPost]
        public async Task<IActionResult> Create(Advertisement advertisement, IFormFile imageFile, IFormFile imageFile2, IFormFile imageFile3)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (ModelState.IsValid)
            {
                advertisement.CustomUserId = userId;
                advertisement.CreatedDate = DateTime.Now;
                await _adRepository.AddAdvertisementAsync(advertisement);

                // آپلود عکس‌ها به سرور FTP
                if (imageFile != null && imageFile.Length > 0)
                {
                    var result = _ftpService.UploadImageToFtp(imageFile, advertisement.Id);
                    if (result)
                    {
                        advertisement.ImageUrl = $"ftp://127.0.0.1/advertisement_{advertisement.Id}/" + imageFile.FileName;
                    }
                }

                if (imageFile2 != null && imageFile2.Length > 0)
                {
                    var result = _ftpService.UploadImageToFtp(imageFile2, advertisement.Id);
                    if (result)
                    {
                        advertisement.ImageUrl2 = $"ftp://127.0.0.1/advertisement_{advertisement.Id}/" + imageFile2.FileName;
                    }
                }

                if (imageFile3 != null && imageFile3.Length > 0)
                {
                    var result = _ftpService.UploadImageToFtp(imageFile3, advertisement.Id);
                    if (result)
                    {
                        advertisement.ImageUrl3 = $"ftp://127.0.0.1/advertisement_{advertisement.Id}/" + imageFile3.FileName;
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
        [Authorize(Policy = "RequireHomeSelectCategory")]
        [HttpGet]
        public IActionResult SelectCategory()
        {
            var categories = Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>();
            return View(categories);
        }


        [Authorize(Policy = "RequireHomeSelectCategory")]
        [HttpGet]
        public IActionResult SetCategory(CategoryType category)
        {
            HttpContext.Session.SetString("SelectedCategory", category.ToString());
            return RedirectToAction("Create");
        }



        // Edit Advertisement
        [Authorize(Policy = "RequireHomeEdit")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Advertisement updatedAdvertisement, IFormFile newImageFile1, IFormFile newImageFile2, IFormFile newImageFile3)
        {
            if (ModelState.IsValid)
            {
                var ad = await _adRepository.GetAdvertisementByIdAsync(id);
                if (ad == null)
                {
                    return NotFound();
                }

                var oldImageUrl1 = ad.ImageUrl;
                var oldImageUrl2 = ad.ImageUrl2;
                var oldImageUrl3 = ad.ImageUrl3;

                ad.Title = updatedAdvertisement.Title;
                ad.Content = updatedAdvertisement.Content;
                ad.Price = updatedAdvertisement.Price;
                ad.ImageUrl = updatedAdvertisement.ImageUrl;
                ad.ImageUrl2 = updatedAdvertisement.ImageUrl2;
                ad.ImageUrl3 = updatedAdvertisement.ImageUrl3;
                // Custom properties
                ad.SimCardsNumber = updatedAdvertisement.SimCardsNumber;
                ad.MobileBrand = updatedAdvertisement.MobileBrand;
                ad.BookAuthor = updatedAdvertisement.BookAuthor;
                ad.CarBrand = updatedAdvertisement.CarBrand;
                ad.GearboxType = updatedAdvertisement.GearboxType;
                ad.HomeAddress = updatedAdvertisement.HomeAddress;
                ad.HomeSize = updatedAdvertisement.HomeSize;

                // ویرایش تصاویر در سرور FTP
                _ftpService.EditImages(ad.Id, newImageFile1, newImageFile2, newImageFile3, oldImageUrl1, oldImageUrl2, oldImageUrl3);

                await _adRepository.UpdateAdvertisementAsync(ad);
                return RedirectToAction(nameof(Index));
            }
            return View(updatedAdvertisement);
        }

        [Authorize(Policy = "RequireHomeEdit")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ad = await _adRepository.GetAdvertisementByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }
            return View(ad);
        }






        // Delete advertisements
        [Authorize(Policy = "RequireHomeDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            var ad = await _adRepository.GetAdvertisementByIdAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            return View(ad);
        }

        // Delete Confirm
        [Authorize(Policy = "RequireHomeDelete")]
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



        // User dashboard 
        public async Task<IActionResult> Dashboard(int pageNumber = 1)
        {
            var userId = HttpContext.Session.GetString("UserId"); // Changed to string
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
