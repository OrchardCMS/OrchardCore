function getTenantName() {
    return document.documentElement.getAttribute('data-tenant') || 'default';
}

function getTenantStorageKey(key) {
    return getTenantName() + '-' + key;
}