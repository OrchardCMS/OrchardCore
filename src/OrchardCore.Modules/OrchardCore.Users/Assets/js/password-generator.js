window.passwordManager = function () {
    const allowed = {
        uppers: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
        lowers: 'abcdefghijklmnopqrstuvwxyz',
        digits: '0123456789',
        symbols: '!#$%&()*+,-./:;<=>\'?@[]^_`{|}~'
    };

    // Returns a uniformly distributed integer in [0, max) from a cryptographically secure source.
    const getRandomInt = (max) => {
        const limit = Math.floor(0x100000000 / max) * max;
        const buffer = new Uint32Array(1);
        do {
            crypto.getRandomValues(buffer);
        } while (buffer[0] >= limit);

        return buffer[0] % max;
    };

    const getRandomChar = (str) => str.charAt(getRandomInt(str.length));

    // Fisher-Yates shuffle.
    const shuffle = (arr) => {
        for (let i = arr.length - 1; i > 0; i--) {
            const j = getRandomInt(i + 1);
            [arr[i], arr[j]] = [arr[j], arr[i]];
        }

        return arr;
    };

    const meetUniqueRule = (str, requiredUniqueChars) => {
        let uniqueCharacters = str.filter((item, i, ar) => ar.indexOf(item) === i);

        return uniqueCharacters.length >= requiredUniqueChars;
    };

    const copyPassword = (str) => navigator.clipboard.writeText(str);

    const generatePassword = (requiredPasswordLength, requireUppercase, requireLowercase, requireDigit, requireNonAlphanumeric, requiredUniqueChars) => {
        let password = [];
        requiredUniqueChars = requiredUniqueChars || 1;

        if (requireUppercase) {
            // At least one uppercase
            password.push(getRandomChar(allowed.uppers));
        }
        if (requireLowercase) {
            // At least one lowercase
            password.push(getRandomChar(allowed.lowers));
        }
        if (requireDigit) {
            // At least one digit
            password.push(getRandomChar(allowed.digits));
        }

        if (requireNonAlphanumeric) {
            // At least one special character
            password.push(getRandomChar(allowed.symbols));
        }

        let passwordLength = password.length;
        if (passwordLength < requiredPasswordLength) {
            // At this point we need lengthier password.Fill the rest of the password with random characters
            let combineAllowed = shuffle(Object.values(allowed)).join('');
            for (let i = passwordLength; i < requiredPasswordLength; i++) {
                password.push(getRandomChar(combineAllowed));
            }
        }

        if (requiredUniqueChars > 1 && !meetUniqueRule(password, requiredUniqueChars)) {
            // The generated password does not meet the required-unique-chars requirment, create another one
            return generatePassword(requiredPasswordLength, requireUppercase, requireLowercase, requireDigit, requireNonAlphanumeric, requiredUniqueChars);
        }

        return shuffle(password).join('');
    }

    return {
        generatePassword: generatePassword,
        copyPassword: copyPassword
    };
}();
