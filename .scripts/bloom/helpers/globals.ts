import removeDiacritics from "./removeDiacritics";

const getTenantName = () => document.documentElement.getAttribute("data-tenant") || "default";

const getTechnicalName = (name: string) => {
    let result = "",
        c;

    if (!name || name.length == 0) {
        return "";
    }

    name = removeDiacritics(name);

    for (let i = 0; i < name.length; i++) {
        c = name[i];
        if (isLetter(c) || (isNumber(c) && i > 0)) {
            result += c;
        }
    }

    return result;
};

const isLetter = (str: string) => {
    return str.length === 1 && str.match(/[a-z]/i);
};

const isNumber = (str: string) => {
    return str.length === 1 && str.match(/[0-9]/i);
};

export { getTenantName, getTechnicalName, isLetter, isNumber };
