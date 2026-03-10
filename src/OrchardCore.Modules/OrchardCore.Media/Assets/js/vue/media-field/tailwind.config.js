import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

/** @type {import('tailwindcss').Config} */
export default {
    prefix: "tw-",
    content: [
        path.resolve(__dirname, "./index.html"),
        path.resolve(__dirname, "./src/**/*.{vue,ts,tsx}"),
    ],
    darkMode: ["selector", '[data-bs-theme="dark"]'],
    theme: {
        extend: {},
    },
    plugins: [],
};
