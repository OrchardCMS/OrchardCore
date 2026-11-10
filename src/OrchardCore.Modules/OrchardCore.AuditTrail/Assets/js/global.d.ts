declare module "*.scss";

// Prism is loaded as a global <script> tag rather than bundled; only the members this module uses are typed.
declare const Prism: {
    highlight: (text: string, grammar: unknown, language?: string) => string;
    languages: Record<string, unknown>;
};
