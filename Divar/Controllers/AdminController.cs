using Divar.Services;
using Microsoft.AspNetCore.Authorization;

public class AdminController : Controller
{
    private readonly IAdminRepository _adminRepository;
    private readonly FtpService _ftpService;

    public AdminController(IAdminRepository adminRepository, FtpService ftpService)
    {
        _adminRepository = adminRepository;
        _ftpService = ftpService;
    }


    // صفحه اصلی
    //[Authorize(Policy = "RequireAdminAccess")]
    public async Task<IActionResult> Index()
    {
        var users = await _adminRepository.GetUsersAsync();
        var advertisements = await _adminRepository.GetAdvertisementsAsync();
        var comments = await _adminRepository.GetCommentsAsync();

        var viewModel = new AdminPanel
        {
            Users = users,
            Advertisements = advertisements,
            Comments = comments
        };

        return View(viewModel);
    }




    //حذف کاربر
    [Authorize(Policy = "RequireDeleteUser")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _adminRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }
    //تایید حذف کاربر
    [Authorize(Policy = "RequireDeleteUser")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _adminRepository.DeleteUserAsync(id, HttpContext);
        return RedirectToAction(nameof(Index));
    }



    // نمایش صفحه تأیید حذف کامنت
    [Authorize(Policy = "RequireDeleteComment")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _adminRepository.GetCommentsAsync(); 
        var commentToDelete = comment.FirstOrDefault(c => c.Id == id);

        if (commentToDelete == null)
        {
            return NotFound();
        }

        return View(commentToDelete);
    }

    // تأیید حذف کامنت
    [Authorize(Policy = "RequireDeleteComment")]
    [HttpPost, ActionName("DeleteConfirmedComment")]
    public async Task<IActionResult> DeleteConfirmedComment(int id)
    {
        await _adminRepository.DeleteComment(id, HttpContext);
        return RedirectToAction(nameof(Index));
    }



    //جزییات اگهی
    [Authorize(Policy = "RequireAdminAdvertisementDetail")]
    public async Task<IActionResult> AdvertisementDetail(int id)
    {
        var advertisement = await _adminRepository.GetAdvertisementsAsync();
        var adDetail = advertisement.FirstOrDefault(ad => ad.Id == id);

        if (adDetail == null)
        {
            return NotFound();
        }

        var (imageUrl1, imageUrl2, imageUrl3) = _ftpService.DownloadImages(id);

        ViewBag.ImageUrl1 = imageUrl1;
        ViewBag.ImageUrl2 = imageUrl2;
        ViewBag.ImageUrl3 = imageUrl3;

        return View(adDetail);
    }

}
