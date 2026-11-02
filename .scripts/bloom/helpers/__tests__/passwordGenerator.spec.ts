import { afterEach, describe, expect, it, vi } from "vitest";
import generatePassword, { type PasswordRequirements } from "../passwordGenerator";

const allRequirements: PasswordRequirements = {
    length: 12,
    requireUppercase: true,
    requireLowercase: true,
    requireDigit: true,
    requireNonAlphanumeric: true,
    requiredUniqueChars: 6,
};

const noRequirements: PasswordRequirements = {
    length: 1,
    requireUppercase: false,
    requireLowercase: false,
    requireDigit: false,
    requireNonAlphanumeric: false,
    requiredUniqueChars: 1,
};

describe("generatePassword", () => {
    afterEach(() => {
        vi.restoreAllMocks();
    });

    it("generatePassword_AllRequirements_MeetsEachOfThem", () => {
        for (let i = 0; i < 500; i++) {
            const password = generatePassword(allRequirements);

            expect(password).toHaveLength(12);
            expect(password).toMatch(/[A-Z]/);
            expect(password).toMatch(/[a-z]/);
            expect(password).toMatch(/[0-9]/);
            expect(password).toMatch(/[^A-Za-z0-9]/);
            expect(new Set(password).size).toBeGreaterThanOrEqual(6);
        }
    });

    it("generatePassword_UniqueCharsAboveLength_ExtendsLength", () => {
        const password = generatePassword({ ...noRequirements, length: 4, requiredUniqueChars: 10 });

        expect(password).toHaveLength(10);
        expect(new Set(password).size).toBe(10);
    });

    it("generatePassword_Always_UsesCryptoRandomValues", () => {
        const getRandomValues = vi.spyOn(crypto, "getRandomValues");
        const mathRandom = vi.spyOn(Math, "random");

        generatePassword(allRequirements);

        expect(getRandomValues).toHaveBeenCalled();
        expect(mathRandom).not.toHaveBeenCalled();
    });

    it("generatePassword_SingleDigit_IsUniformlyDistributed", () => {
        const counts = new Map<string, number>();
        const draws = 20000;

        for (let i = 0; i < draws; i++) {
            const digit = generatePassword({ ...noRequirements, length: 0, requireDigit: true });
            counts.set(digit, (counts.get(digit) ?? 0) + 1);
        }

        expect(counts.size).toBe(10);

        for (const count of counts.values()) {
            // Expected 2000 per digit; this bound is more than 8 standard deviations wide.
            expect(count).toBeGreaterThan(1650);
            expect(count).toBeLessThan(2350);
        }
    });
});
