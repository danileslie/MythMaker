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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // check ownership on the GET too - otherwise someone could load another
            // user's character into the edit form even if they couldn't save it after
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (character == null)
            {
                return NotFound();
            }

            // copy the real entity into a viewmodel so the form only ever sees
            // what it's supposed to edit - no OwnerId/IsDraft exposed here either
            var model = new EditCharacterViewModel
            {
                Id = character.Id,
                Name = character.Name,
                Race = character.Race,
                Class = character.Class,
                Level = character.Level,
                Strength = character.Strength,
                Dexterity = character.Dexterity,
                Constitution = character.Constitution,
                Intelligence = character.Intelligence,
                Wisdom = character.Wisdom,
                Charisma = character.Charisma,
                Backstory = character.Backstory
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCharacterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            // re-checking ownership here too, separately from the GET - this is the
            // actual security boundary, GET's check is just so the form doesn't even load
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == model.Id && c.OwnerId == userId);

            if (character == null)
            {
                return NotFound();
            }

            // mutating the tracked entity directly instead of building a new Character -
            // this is what makes EF generate an UPDATE instead of a second INSERT
            character.Name = model.Name;
            character.Race = model.Race;
            character.Class = model.Class;
            character.Level = model.Level;
            character.Strength = model.Strength;
            character.Dexterity = model.Dexterity;
            character.Constitution = model.Constitution;
            character.Intelligence = model.Intelligence;
            character.Wisdom = model.Wisdom;
            character.Charisma = model.Charisma;
            character.Backstory = model.Backstory;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = character.Id });
        }
    }
}