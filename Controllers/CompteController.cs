using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Linq;

[Authorize(Roles = "Admin")]
public class CompteController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CompteController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Compte
    public IActionResult Index()
    {
        var users = _userManager.Users.ToList();
        var userRoles = users.Select(u => new
        {
            u.Id,
            u.Email,
            Roles = _userManager.GetRolesAsync(u).Result
        }).ToList();

        ViewBag.Users = userRoles;
        return View();
    }

    // GET: Compte/Create
    // GET: Compte/Create
    public IActionResult Create()
    {
        var roles = _roleManager.Roles
    .Where(r => r.Name == "Étudiant" || r.Name == "Admin")
    .ToList();
        ViewBag.Roles = new SelectList(roles, "Name", "Name");


        var users = _userManager.Users.ToList();
        var userRoles = users.Select(u => new
        {
            u.Email,
            Roles = _userManager.GetRolesAsync(u).Result
        }).ToList();

        ViewBag.Users = userRoles;

        return View();
    }


    // POST: Compte/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string email, string password, string role)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction("Success");

            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        // 🔁 Recharger la liste des rôles sans "Professeur"
        var roles = _roleManager.Roles
            .Where(r => r.Name != "Professeur")
            .Select(r => r.Name)
            .ToList();

        ViewBag.Roles = new SelectList(roles);
        return View();
    }

    public IActionResult List(string searchEmail)
    {
        var users = _userManager.Users.ToList();

        if (!string.IsNullOrEmpty(searchEmail))
        {
            users = users.Where(u => u.Email.Contains(searchEmail, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var userRoles = users.Select(u => new
        {
            u.Email,
            Roles = _userManager.GetRolesAsync(u).Result
        }).ToList();

        ViewBag.Users = userRoles;
        ViewBag.Search = searchEmail;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToAction("List");
    }

    // GET: Compte/Success
    public IActionResult Success()
    {
        return View();
    }

}
