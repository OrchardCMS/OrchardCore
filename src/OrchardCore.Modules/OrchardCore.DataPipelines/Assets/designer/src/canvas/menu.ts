export interface MenuItem {
    label: string;
    icon?: string;
    danger?: boolean;
    dataCy?: string;
    run(): void;
}
