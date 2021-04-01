var tenantPrefix = document.currentScript.dataset.tenantPrefix;
require.config({ paths: { 'vs': tenantPrefix + '/OrchardCore.Resources/Scripts/monaco/dev/vs' } });