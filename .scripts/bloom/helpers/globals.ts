const getTenantName = () => document.documentElement.getAttribute("data-tenant") || "default";

export { getTenantName };
