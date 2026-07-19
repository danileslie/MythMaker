// tracks which draft we're actually updating - starts empty since a brand
// new character has no id yet. gets set once the first autosave succeeds,
// and every save after that sends this id so the server updates instead
// of creating a new draft row every time
let currentCharacterId = null;

// holds the pending "save in 500ms" timer so it can be cancelled/restarted
let debounceTimer = null;

// grabs whatever's currently in every field and shapes it to match
// AutosaveViewModel on the server
function collectFormData() {
    return {
        id: currentCharacterId,
        name: document.getElementById('Name').value,
        race: document.getElementById('raceInput').value,
        class: document.getElementById('classInput').value,
        level: parseInt(document.getElementById('Level').value) || 0,
        strength: parseInt(document.getElementById('Strength').value) || 0,
        dexterity: parseInt(document.getElementById('Dexterity').value) || 0,
        constitution: parseInt(document.getElementById('Constitution').value) || 0,
        intelligence: parseInt(document.getElementById('Intelligence').value) || 0,
        wisdom: parseInt(document.getElementById('Wisdom').value) || 0,
        charisma: parseInt(document.getElementById('Charisma').value) || 0,
        backstory: document.getElementById('Backstory').value
    };
}

// the actual save call - posts to the Autosave endpoint as json
async function performAutosave() {
    const errorBox = document.getElementById('autosaveError');
    errorBox.style.display = 'none'; // hide any old error before trying again

    try {
        const response = await fetch('/Characters/Autosave', {

            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                // fetch doesn't send this automatically like a normal form post
                // does - has to be pulled from the hidden field and sent by hand
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(collectFormData())
        });

        // fetch doesn't treat a bad status code as an error on its own -
        // have to check response.ok and throw manually to land in catch
        if (!response.ok) {
            throw new Error('Autosave failed');
        }

        const result = await response.json();
        currentCharacterId = result.id; // now future saves update this same row
        document.getElementById('characterId').value = currentCharacterId; // keep the form's hidden field in sync too
    } catch (err) {
        // just show the error - no auto-retry, user has to click the button
        errorBox.style.display = 'block';
    }
}

// this is the actual debounce - every blur cancels whatever was pending
// and restarts a fresh 500ms countdown. only the last blur in a quick
// tab-through actually ends up firing a save
function scheduleAutosave() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(performAutosave, 500);
}

// wire every relevant field's blur event to trigger the debounced save
document.querySelectorAll('#Name, #raceInput, #classInput, #Level, #Strength, #Dexterity, #Constitution, #Intelligence, #Wisdom, #Charisma, #Backstory')
    .forEach(field => field.addEventListener('blur', scheduleAutosave));

// manual retry button - just calls the save again, no automatic loop
document.getElementById('retryAutosave').addEventListener('click', performAutosave);