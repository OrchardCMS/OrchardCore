import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string): void };
};

interface CultureStats {
    culture: string;
    displayName: string;
    percentage: number;
    translated: number;
    total: number;
}

interface CategoryStats {
    category: string;
    percentage: number;
    translated: number;
    total: number;
}

interface TranslationStats {
    overall: { percentage: number; translated: number; total: number };
    byCulture: CultureStats[];
    byCategory: Record<string, CategoryStats[]>;
}

interface StatisticsInstance {
    stats: TranslationStats;
    selectedCulture: string;
    editorBaseUrl: string;
}

const statsEl = document.getElementById("translation-statistics");

if (statsEl) {
    const stats = getDatasetJson<TranslationStats>(statsEl, "stats") ?? ({} as TranslationStats);
    const editorBaseUrl = statsEl.dataset.editorBaseUrl ?? "";

    const { createApp } = Vue;

    const app = createApp({
        data() {
            return {
                stats,
                selectedCulture: "",
                editorBaseUrl,
            };
        },
        computed: {
            selectedCultureCategories(this: StatisticsInstance) {
                if (!this.selectedCulture || !this.stats.byCategory) {
                    return [];
                }
                return this.stats.byCategory[this.selectedCulture] || [];
            },
        },
        methods: {
            getProgressClass(percentage: number) {
                if (percentage >= 75) return "bg-success";
                if (percentage >= 25) return "bg-warning";
                return "bg-danger";
            },
            getBadgeClass(percentage: number) {
                if (percentage >= 75) return "text-bg-success";
                if (percentage >= 25) return "text-bg-warning";
                return "text-bg-danger";
            },
            getEditorUrl(this: StatisticsInstance, culture: string) {
                return `${this.editorBaseUrl}?culture=${encodeURIComponent(culture)}`;
            },
        },
        mounted(this: StatisticsInstance) {
            if (this.stats.byCulture && this.stats.byCulture.length > 0) {
                this.selectedCulture = this.stats.byCulture[0].culture;
            }
        },
    });

    app.mount("#translation-statistics");
}
