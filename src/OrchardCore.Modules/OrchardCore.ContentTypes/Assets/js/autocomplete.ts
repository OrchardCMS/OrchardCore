import { getTechnicalName } from '@orchardcore/theadmin/js/TheAdmin/TheAdmin';

const nameElement = document.querySelector('[data-name]') as HTMLInputElement;
const displayNameElement = document.querySelector('[data-displayname]') as HTMLInputElement;

let nameAltered = false;

nameElement.addEventListener('keydown', () => {
    nameAltered = true;
});

const compute = () => {
    // stop processing automatically if altered by the user
    if (nameAltered) {
        return true;
    }

    nameElement.value = getTechnicalName(displayNameElement.value);
};

displayNameElement.addEventListener('keyup', compute);
displayNameElement.addEventListener('blur', compute);
