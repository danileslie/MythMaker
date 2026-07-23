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
        public async Task<IActionResult> Create(int? id)
        {
            // no id in the url - normal "start fresh" case, same as before
            if (!id.HasValue)
            {
                return View();
            }

            var userId = _userManager.GetUserId(User);

            // resuming a draft - only ever loads drafts, not finalized characters,
            // so this link can't be repurposed to reload something already finished
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id.Value && c.OwnerId == userId && c.IsDraft);

            if (character == null)
            {
                return NotFound();
            }

            // copy the draft's saved data into the viewmodel so the form loads pre-filled -
            // including Id, so autosave/submit both know this is an existing row, not new
            var model = new CreateCharacterViewModel
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
        public async Task<IActionResult> Create(CreateCharacterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            Character character;

            if (model.Id.HasValue)
            {
                // a draft already exists from autosave - finalize that same row,
                // don't create a second one
                character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == model.Id.Value && c.OwnerId == userId);

                if (character == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // no autosave ever fired (e.g. submitted immediately) - genuinely new row
                character = new Character { OwnerId = userId };
                _context.Characters.Add(character);
            }

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
            character.IsDraft = false;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = character.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = new CharacterIndexViewModel
            {
                FinalizedCharacters = await _context.Characters
                    .Where(c => c.OwnerId == userId && !c.IsDraft)
                    .ToListAsync(),
                Drafts = await _context.Characters
                    .Where(c => c.OwnerId == userId && c.IsDraft)
                    .ToListAsync()
            };

            return View(viewModel);
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
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // same ownership check as everywhere else - don't even show someone
            // else's character on a delete confirmation screen
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (character == null)
            {
                return NotFound();
            }

            return View(character);
        }

        // can't have two methods both named Delete with the same (int id) signature -
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (character == null)
            {
                return NotFound();
            }

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // not a form post, expects json in the body
        // and skips full validation on purpose since drafts can be incomplete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Autosave([FromBody] AutosaveViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            Character character;

            if (model.Id.HasValue)
            {
                // already has an id -> this is an update to an existing draft,
                // same ownership check as everywhere else
                character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == model.Id.Value && c.OwnerId == userId);

                if (character == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // no id yet -> first autosave on a brand new character, create the draft row
                character = new Character { OwnerId = userId, IsDraft = true };
                _context.Characters.Add(character);
            }

            // no ModelState check here on purpose -> more relaxed validation
            // a draft can have blank/incomplete fields
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
            character.IsDraft = true;

            await _context.SaveChangesAsync();

            // send the id back so the frontend knows which draft to keep updating
            // on every autosave after this first one
            return Json(new { id = character.Id });
        }
    }
}