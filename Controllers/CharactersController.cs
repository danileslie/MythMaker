using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MythMaker.Data;
using MythMaker.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MythMaker.Controllers
{
    // every action here needs a logged in user, no anonymous character stuff
    [Authorize]
    public class CharactersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CharactersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCharacterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // grabs the id of whoever's actually logged in right now
            var userId = _userManager.GetUserId(User);

            // mapping fields by hand instead of just casting the viewmodel -
            // keeps OwnerId/IsDraft out of the form entirely so nothing submitted
            // by the user can touch them
            var character = new Character
            {
                Name = model.Name,
                Race = model.Race,
                Class = model.Class,
                Level = model.Level,
                Strength = model.Strength,
                Dexterity = model.Dexterity,
                Constitution = model.Constitution,
                Intelligence = model.Intelligence,
                Wisdom = model.Wisdom,
                Charisma = model.Charisma,
                Backstory = model.Backstory,
                IsDraft = false,
                OwnerId = userId
            };

            _context.Characters.Add(character);
            await _context.SaveChangesAsync(); // nothing's actually saved until this line runs

            // Details doesn't exist yet - this'll 404 until that story's built
            return RedirectToAction("Details", new { id = character.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // only show my own finalized characters - drafts get their own separate view later
            var characters = await _context.Characters
                .Where(c => c.OwnerId == userId && !c.IsDraft)
                .ToListAsync();

            return View(characters);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            // checking OwnerId here too, not just Id - otherwise anyone logged in
            // could view any character just by guessing/incrementing the id in the url
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (character == null)
            {
                return NotFound();
            }

            return View(character);
        }
    }
}