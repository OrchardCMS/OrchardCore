<script setup lang="ts">
import { ref } from 'vue'
import { getTranslations } from '@bloom/helpers/localizations'

const t = (key: string) => getTranslations()[key] ?? key

interface Settings {
    allowAnonymousSchemaAccess: boolean
    authenticationType: number
    authorizationUrl: string
    tokenUrl: string
    serverMetadataUrl: string
    oAuthClientId: string
    oAuthScopes: string
}

interface FeatureStatus {
    swaggerUI: boolean
    reDocUI: boolean
    scalarUI: boolean
}

const props = defineProps<{
    settings: Settings
    featureStatus: FeatureStatus
    pathBase: string
}>()

const emit = defineEmits<{
    'update:settings': [value: Settings]
}>()

function update<K extends keyof Settings>(key: K, value: Settings[K]) {
    emit('update:settings', { ...props.settings, [key]: value })
}

const isPkce = () => props.settings.authenticationType === 1

const fetchingMetadata = ref(false)
const metadataFetchSucceeded = ref(false)
const metadataFetchError = ref('')
const suggestedScopes = ref<string[]>([])
const manualEndpointsEdit = ref(false)

function scopeList(): string[] {
    return (props.settings.oAuthScopes || '').split(/\s+/).filter(Boolean)
}

const hasScope = (scope: string) => scopeList().includes(scope)

function toggleScope(scope: string) {
    const scopes = scopeList()
    const index = scopes.indexOf(scope)

    if (index >= 0) {
        scopes.splice(index, 1)
    }
    else {
        scopes.push(scope)
    }

    update('oAuthScopes', scopes.join(' '))
}

function resolveMetadataUrl(url: string): string | null {
    const trimmed = url?.trim()
    if (!trimmed) return null

    try {
        return new URL(trimmed).href
    }
    catch {
        // Relative URLs are resolved against the current tenant, matching the
        // server-side resolution applied on save.
        return `${window.location.origin}${props.pathBase}${trimmed.startsWith('/') ? '' : '/'}${trimmed}`
    }
}

async function fetchEndpointsFromMetadata() {
    metadataFetchSucceeded.value = false
    metadataFetchError.value = ''

    const url = resolveMetadataUrl(props.settings.serverMetadataUrl)
    if (!url) return

    fetchingMetadata.value = true

    try {
        const response = await fetch(url, { credentials: 'omit' })
        if (!response.ok) throw new Error(`HTTP ${response.status}`)

        const metadata = await response.json()
        const updated = { ...props.settings }

        if (metadata.authorization_endpoint) updated.authorizationUrl = metadata.authorization_endpoint
        if (metadata.token_endpoint) updated.tokenUrl = metadata.token_endpoint

        // Only suggest the supported scopes — never auto-select them: which scopes this
        // client may request is per-application policy the server metadata cannot know.
        suggestedScopes.value = Array.isArray(metadata.scopes_supported)
            ? metadata.scopes_supported.filter((scope: unknown) => typeof scope === 'string')
            : []

        emit('update:settings', updated)
        metadataFetchSucceeded.value = true
    }
    catch {
        metadataFetchError.value = t('metadataFetchError')
    }
    finally {
        fetchingMetadata.value = false
    }
}
</script>

<template>
    <h5>{{ t('apiDocumentationUIs') }}</h5>

    <div class="mb-3">
        <div class="d-flex align-items-center mb-2">
            <span class="badge me-2" :class="featureStatus.swaggerUI ? 'bg-success' : 'bg-secondary'">
                {{ featureStatus.swaggerUI ? t('enabled') : t('disabled') }}
            </span>
            <strong>{{ t('swaggerUI') }}</strong>
            <span v-if="featureStatus.swaggerUI" class="hint dashed ms-2">
                {{ t('interactiveApiExplorerAt') }} <a :href="`${pathBase}/swagger`" target="_blank">~/swagger</a>
            </span>
        </div>

        <div class="d-flex align-items-center mb-2">
            <span class="badge me-2" :class="featureStatus.reDocUI ? 'bg-success' : 'bg-secondary'">
                {{ featureStatus.reDocUI ? t('enabled') : t('disabled') }}
            </span>
            <strong>{{ t('reDocUI') }}</strong>
            <span v-if="featureStatus.reDocUI" class="hint dashed ms-2">
                {{ t('readOnlyApiDocumentationAt') }} <a :href="`${pathBase}/redoc`" target="_blank">~/redoc</a>
            </span>
        </div>

        <div class="d-flex align-items-center mb-2">
            <span class="badge me-2" :class="featureStatus.scalarUI ? 'bg-success' : 'bg-secondary'">
                {{ featureStatus.scalarUI ? t('enabled') : t('disabled') }}
            </span>
            <strong>{{ t('scalarUI') }}</strong>
            <span v-if="featureStatus.scalarUI" class="hint dashed ms-2">
                {{ t('modernApiReferenceAt') }} <a :href="`${pathBase}/scalar/v1`" target="_blank">~/scalar/v1</a>
            </span>
        </div>

        <span class="hint" v-html="t('enableDisableUIsHint')"></span>
    </div>

    <h5 class="mt-4">{{ t('apiSchemaAccess') }}</h5>

    <div class="mb-3">
        <div class="form-check">
            <input
                type="checkbox"
                class="form-check-input"
                id="vue-AllowAnonymousSchemaAccess"
                :checked="settings.allowAnonymousSchemaAccess"
                @change="update('allowAnonymousSchemaAccess', ($event.target as HTMLInputElement).checked)"
            />
            <label class="form-check-label" for="vue-AllowAnonymousSchemaAccess">{{ t('allowAnonymousSchemaAccess') }}</label>
            <span class="hint dashed" v-html="t('allowAnonymousSchemaAccessHint')"></span>
        </div>
    </div>

    <h5 class="mt-4">{{ t('apiAuthentication') }}</h5>

    <div class="mb-3">
        <label class="form-label" for="vue-AuthenticationType">{{ t('authenticationType') }}</label>
        <select
            class="form-select"
            id="vue-AuthenticationType"
            :value="settings.authenticationType"
            @change="update('authenticationType', Number(($event.target as HTMLSelectElement).value))"
        >
            <option :value="0">{{ t('cookieDefault') }}</option>
            <option :value="1">{{ t('oauth2Pkce') }}</option>
        </select>
        <span class="hint">{{ t('authenticationTypeHint') }}</span>
    </div>

    <!-- Cookie info -->
    <div v-if="settings.authenticationType === 0" class="alert alert-info mb-3">
        {{ t('cookieInfo') }}
    </div>

    <!-- PKCE info -->
    <div v-if="isPkce()" class="alert alert-info mb-3">
        <p>{{ t('pkceInfo') }}</p>
        <hr />
        <p v-html="t('pkceEnsure')"></p>
        <hr />
        <p class="mb-0" v-html="t('openIdTokenValidation')"></p>
    </div>

    <!-- OAuth2 fields (PKCE only). The card holds the metadata URL and its fetch button;
         the endpoint URLs below can be filled from the metadata document, either explicitly
         via the button (fetched by the browser) or on save when left empty. -->
    <template v-if="isPkce()">
        <div class="card mb-3">
            <div class="card-header">
                <h6 class="mb-0">{{ t('serverConfiguration') }}</h6>
            </div>
            <div class="card-body">
                <div class="mb-3">
                    <label class="form-label" for="vue-ServerMetadataUrl">{{ t('serverMetadataUrl') }}</label>
                    <input
                        type="text"
                        class="form-control"
                        id="vue-ServerMetadataUrl"
                        placeholder="/.well-known/openid-configuration"
                        :value="settings.serverMetadataUrl"
                        @input="update('serverMetadataUrl', ($event.target as HTMLInputElement).value)"
                    />
                    <span class="hint">{{ t('serverMetadataUrlHint') }}</span>
                </div>

                <button
                    type="button"
                    class="btn btn-secondary"
                    :disabled="!settings.serverMetadataUrl?.trim() || fetchingMetadata"
                    @click="fetchEndpointsFromMetadata"
                >
                    <span v-if="fetchingMetadata" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                    {{ fetchingMetadata ? t('fetchingMetadata') : t('autoFillEndpoints') }}
                </button>

                <div v-if="metadataFetchSucceeded" class="alert alert-success mt-3 mb-0" role="alert">
                    {{ t('metadataFetchSuccess') }}
                </div>
                <div v-if="metadataFetchError" class="alert alert-danger mt-3 mb-0" role="alert">
                    {{ metadataFetchError }}
                </div>
            </div>
        </div>

        <div class="card mb-3">
            <div class="card-header">
                <h6 class="mb-0">{{ t('clientConfiguration') }}</h6>
            </div>
            <div class="card-body">
                <div class="mb-3">
                    <label class="form-label" for="vue-OAuthClientId">{{ t('clientId') }}</label>
                    <input
                        type="text"
                        class="form-control"
                        id="vue-OAuthClientId"
                        placeholder="swagger-ui"
                        :value="settings.oAuthClientId"
                        @input="update('oAuthClientId', ($event.target as HTMLInputElement).value)"
                    />
                    <span class="hint">{{ t('clientIdHint') }}</span>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="vue-OAuthScopes">{{ t('scopes') }}</label>
                    <input
                        type="text"
                        class="form-control"
                        id="vue-OAuthScopes"
                        placeholder="openid profile email"
                        :value="settings.oAuthScopes"
                        @input="update('oAuthScopes', ($event.target as HTMLInputElement).value)"
                    />
                    <span class="hint">{{ t('scopesHint') }}</span>

                    <div v-if="suggestedScopes.length" class="mt-2">
                        <span class="hint d-block mb-1">{{ t('availableScopes') }}</span>
                        <button
                            v-for="scope in suggestedScopes"
                            :key="scope"
                            type="button"
                            class="btn btn-sm me-1 mb-1"
                            :class="hasScope(scope) ? 'btn-primary' : 'btn-outline-secondary'"
                            @click="toggleScope(scope)"
                        >
                            {{ scope }}
                        </button>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="vue-AuthorizationUrl">{{ t('authorizationUrl') }}</label>
                    <input
                        type="text"
                        class="form-control"
                        id="vue-AuthorizationUrl"
                        placeholder="/connect/authorize"
                        :readonly="!manualEndpointsEdit"
                        :value="settings.authorizationUrl"
                        @input="update('authorizationUrl', ($event.target as HTMLInputElement).value)"
                    />
                    <span class="hint">{{ t('authorizationUrlHint') }}</span>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="vue-TokenUrl">{{ t('tokenUrl') }}</label>
                    <input
                        type="text"
                        class="form-control"
                        id="vue-TokenUrl"
                        placeholder="/connect/token"
                        :readonly="!manualEndpointsEdit"
                        :value="settings.tokenUrl"
                        @input="update('tokenUrl', ($event.target as HTMLInputElement).value)"
                    />
                    <span class="hint">{{ t('tokenUrlHint') }}</span>
                </div>

                <!-- The endpoint URLs derive from the server metadata, so they are read-only by
                     default. The toggle covers servers without a usable metadata document and
                     split-horizon setups where the browser must use different URLs than the
                     metadata advertises. -->
                <div class="form-check form-switch">
                    <input
                        type="checkbox"
                        class="form-check-input"
                        id="vue-EditEndpointsManually"
                        v-model="manualEndpointsEdit"
                    />
                    <label class="form-check-label" for="vue-EditEndpointsManually">{{ t('editEndpointsManually') }}</label>
                </div>
            </div>
        </div>
    </template>
</template>
