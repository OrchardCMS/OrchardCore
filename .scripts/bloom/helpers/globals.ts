import removeDiacritics from "./removeDiacritics";

const getTenantName = () => document.documentElement.getAttribute("data-tenant") || "default";

const getAdminPrefix = (): string => {
    // Get the admin prefix from the current pathname.
    // In a multi-tenant setup, the path could be like: /tenant1/admin or /admin
    // We need to extract just the admin prefix part.
    const pathname = window.location.pathname;
    const adminMatch = pathname.match(/^(\/[^/]+)?\/admin/i);

    if (adminMatch && adminMatch[1]) {
        // Multi-tenant: return the full prefix including tenant, e.g., "/tenant1/admin"
        return adminMatch[1] + '/admin';
    } else if (adminMatch) {
        // Single tenant: return just "/admin"
        return '/admin';
    }

    // Fallback: try to extract from pathname if it contains /admin
    const adminIndex = pathname.toLowerCase().indexOf('/admin');
    if (adminIndex !== -1) {
        return pathname.substring(0, adminIndex + 6); // 6 is the length of "/admin"
    }

    return '/admin'; // Default fallback
};

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

export { getTenantName, getAdminPrefix, getTechnicalName, isLetter, isNumber };
