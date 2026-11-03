export interface PasswordRequirements {
    /** The minimum number of characters. */
    length: number;
    requireUppercase: boolean;
    requireLowercase: boolean;
    requireDigit: boolean;
    requireNonAlphanumeric: boolean;
    /** The minimum number of distinct characters. */
    requiredUniqueChars: number;
}

const uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
const lowers = "abcdefghijklmnopqrstuvwxyz";
const digits = "0123456789";
const symbols = "!#$%&()*+,-./:;<=>'?@[]^_`{|}~";
const allChars = uppers + lowers + digits + symbols;

// Returns a uniformly distributed integer in [0, max) from a cryptographically secure source.
const getRandomInt = (max: number) => {
    // Rejecting values above the largest multiple of max avoids a modulo bias.
    const limit = Math.floor(0x100000000 / max) * max;
    const buffer = new Uint32Array(1);

    do {
        crypto.getRandomValues(buffer);
    } while (buffer[0] >= limit);

    return buffer[0] % max;
};

const getRandomChar = (chars: string) => chars.charAt(getRandomInt(chars.length));

// Fisher-Yates shuffle.
const shuffle = <T>(items: T[]) => {
    for (let i = items.length - 1; i > 0; i--) {
        const j = getRandomInt(i + 1);
        [items[i], items[j]] = [items[j], items[i]];
    }

    return items;
};

const countUniqueChars = (chars: string[]) => new Set(chars).size;

/**
 * Generates a random password that meets the given requirements, using a cryptographically secure
 * random number generator.
 */
export default function generatePassword(requirements: PasswordRequirements): string {
    const requiredUniqueChars = Math.max(requirements.requiredUniqueChars || 1, 1);

    // A password can't have more distinct characters than its length.
    const length = Math.max(requirements.length, requiredUniqueChars);

    let password: string[];

    do {
        password = [];

        if (requirements.requireUppercase) {
            password.push(getRandomChar(uppers));
        }

        if (requirements.requireLowercase) {
            password.push(getRandomChar(lowers));
        }

        if (requirements.requireDigit) {
            password.push(getRandomChar(digits));
        }

        if (requirements.requireNonAlphanumeric) {
            password.push(getRandomChar(symbols));
        }

        while (password.length < length) {
            password.push(getRandomChar(allChars));
        }
    } while (countUniqueChars(password) < requiredUniqueChars);

    return shuffle(password).join("");
}
