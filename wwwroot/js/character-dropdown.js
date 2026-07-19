// shared dropdown-preset-with-custom-"other" logic, used by both
// Create and Edit. handles the case where there's no existing value yet
// (Create) and the case where there's a real value to restore (Edit)
function setupDropdown(selectId, inputId) {
    const select = document.getElementById(selectId);
    const input = document.getElementById(inputId);

    // check if the field already has a value (Edit) or is empty (Create)
    if (input.value) {
        const matchingOption = Array.from(select.options).find(o => o.value === input.value);
        if (matchingOption) {
            select.value = input.value;
            input.style.display = 'none';
        } else {
            select.value = 'Other';
            input.style.display = 'block';
        }
    } else {
        // brand new form, nothing to restore - start on the first preset, hidden
        input.style.display = 'none';
        input.value = select.value;
    }

    select.addEventListener('change', function () {
        if (select.value === 'Other') {
            input.style.display = 'block';
            input.value = '';
        } else {
            input.style.display = 'none';
            input.value = select.value;
        }
    });
}