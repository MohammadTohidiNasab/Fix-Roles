using Divar.DAL;

namespace Divar.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DivarDbContext _context;

        public CategoriesViewComponent(DivarDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var categories = Enum.GetValues(typeof(CategoryType))
                .Cast<CategoryType>()
                .ToList();

            return View(categories);
        }

    }
}
