export interface TenantInfo {
    name: string;
    prefix: string;
    setupRecipe: string;
    description?: string;
}

export interface OrchardConfig {
    username: string;
    email: string;
    password: string;
}

export const defaultOrchardConfig: OrchardConfig = {
    username: 'admin',
    email: 'admin@orchard.com',
    password: 'Orchard1!',
};

export function generateTenantInfo(setupRecipeName: string, description?: string): TenantInfo {
    const date = new Date();
    const today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const uniqueName = 't' + (date.getTime() - today.getTime()).toString(32);
    return {
        name: uniqueName,
        prefix: uniqueName,
        setupRecipe: setupRecipeName,
        description,
    };
}
