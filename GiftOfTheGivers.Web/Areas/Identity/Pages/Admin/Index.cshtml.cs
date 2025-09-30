using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiftOfTheGivers.Web.Areas.Identity.Pages.Admin
{
    [Authorize(Roles = "Administrator")] // <--- ADD THIS LINE
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            // This code will only execute if the user is logged in AND has the Administrator role.
        }
    }
}
