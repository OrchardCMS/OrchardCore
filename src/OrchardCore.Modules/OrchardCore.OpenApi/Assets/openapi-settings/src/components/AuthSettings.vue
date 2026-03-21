<script setup lang="ts">
interface Settings {
    authenticationType: number
    authorizationUrl: string
    tokenUrl: string
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
const isClientCreds = () => props.settings.authenticationType === 2
const isOAuth = () => isPkce() || isClientCreds()
</script>

<template>
    <h5>API Documentation UIs</h5>

    <div class="mb-3">
        <div class="d-flex align-items-center mb-2">
            <span class="badge me-2" :class="featureStatus.swaggerUI ? 'bg-success' : 'bg-secondary'">
                {{ featureStatus.swaggerUI ? 'Enabled' : 'Disabled' }}
            </span>
            <strong>Swagger UI</strong>
            <span v-if="featureStatus.swaggerUI" class="hint dashed ms-2">
                Interactive API explorer at <a :href="`${pathBase}/swagger`" target="_blank">~/swagger</a>
            </span>
        </div>

        <div class="d-flex align-items-center mb-2">
            <span class="badge me-2" :class="featureStatus.reDocUI ? 'bg-success' : 'bg-secondary'">
                {{ featureStatus.reDocUI ? 'Enabled' : 'Disabled' }}
            </span>
            <strong>ReDoc UI</strong>
            <span v-if="featureStatus.reDocUI" class="hint dashed ms-2">
                Read-only API documentation at <a :href="`${pathBase}/redoc`" target="_blank">~/redoc</a>
            </span>
        </div>

        <div class="d-flex align-items-center mb-2">
            <span class="badge me-2" :class="featureStatus.scalarUI ? 'bg-success' : 'bg-secondary'">
                {{ featureStatus.scalarUI ? 'Enabled' : 'Disabled' }}
            </span>
            <strong>Scalar UI</strong>
            <span v-if="featureStatus.scalarUI" class="hint dashed ms-2">
                Modern API reference at <a :href="`${pathBase}/scalar/v1`" target="_blank">~/scalar/v1</a>
            </span>
        </div>

        <span class="hint">Enable or disable API documentation UIs via the <a href="/admin/features">Features</a> page.</span>
    </div>

    <h5 class="mt-4">API Authentication</h5>

    <div class="mb-3">
        <label class="form-label" for="vue-AuthenticationType">Authentication Type</label>
        <select
            class="form-select"
            id="vue-AuthenticationType"
            :value="settings.authenticationType"
            @change="update('authenticationType', Number(($event.target as HTMLSelectElement).value))"
        >
            <option :value="0">Cookie (default)</option>
            <option :value="1">OAuth2 Authorization Code + PKCE</option>
            <option :value="2">OAuth2 Client Credentials</option>
        </select>
        <span class="hint">Select the authentication method used by the "Try it out" buttons in the API documentation UIs.</span>
    </div>

    <!-- Cookie info -->
    <div v-if="settings.authenticationType === 0" class="alert alert-info mb-3">
        The API documentation UIs will use cookie authentication. If you are logged in, the "Try it out" buttons will
        automatically use your session cookie. No additional configuration is needed.
    </div>

    <!-- PKCE info -->
    <div v-if="isPkce()" class="alert alert-info mb-3">
        <p>For interactive authentication. The "Authorize" button will redirect users to the authorization server to sign in, then exchange the code for a token using PKCE.</p>
        <hr />
        <p>Ensure your OpenID Connect application has <strong>Allow Authorization Code Flow</strong> enabled and a redirect URI configured for the Swagger UI callback.</p>
        <hr />
        <p>The <strong>OpenID Token Validation</strong> feature must be enabled for Bearer token authentication to work on API endpoints.</p>
        <hr />
        <p class="mb-0">Hint: If you are logged in, the "Try it out" buttons will still work using your session cookie even without clicking "Authorize".</p>
    </div>

    <!-- Client Credentials info -->
    <div v-if="isClientCreds()" class="alert alert-info mb-3">
        <p>For machine-to-machine authentication. The "Authorize" dialog will prompt for client ID and client secret, then exchange them for a bearer token.</p>
        <hr />
        <p>Ensure your OpenID Connect application has <strong>Allow Client Credentials Flow</strong> enabled and is configured as a <strong>Confidential client</strong>. Assign the appropriate <strong>Client Credentials Roles</strong>.</p>
        <hr />
        <p>The <strong>OpenID Token Validation</strong> feature must be enabled for Bearer token authentication to work on API endpoints.</p>
        <hr />
        <p class="mb-0">Hint: If you are logged in, the "Try it out" buttons will still work using your session cookie even without clicking "Authorize".</p>
    </div>

    <!-- Authorization URL (PKCE only) -->
    <div v-if="isPkce()" class="mb-3">
        <label class="form-label" for="vue-AuthorizationUrl">Authorization URL</label>
        <input
            type="text"
            class="form-control"
            id="vue-AuthorizationUrl"
            placeholder="/connect/authorize"
            :value="settings.authorizationUrl"
            @input="update('authorizationUrl', ($event.target as HTMLInputElement).value)"
        />
        <span class="hint">The OAuth2 authorization endpoint. Relative URLs are resolved against the current tenant.</span>
    </div>

    <!-- Shared OAuth2 fields -->
    <template v-if="isOAuth()">
        <div class="mb-3">
            <label class="form-label" for="vue-TokenUrl">Token URL</label>
            <input
                type="text"
                class="form-control"
                id="vue-TokenUrl"
                placeholder="/connect/token"
                :value="settings.tokenUrl"
                @input="update('tokenUrl', ($event.target as HTMLInputElement).value)"
            />
            <span class="hint">The OAuth2 token endpoint. Relative URLs are resolved against the current tenant.</span>
        </div>

        <div class="mb-3">
            <label class="form-label" for="vue-OAuthClientId">Client ID</label>
            <input
                type="text"
                class="form-control"
                id="vue-OAuthClientId"
                placeholder="swagger-ui"
                :value="settings.oAuthClientId"
                @input="update('oAuthClientId', ($event.target as HTMLInputElement).value)"
            />
            <span class="hint">The OAuth2 client ID registered for the API documentation UIs.</span>
        </div>

        <div class="mb-3">
            <label class="form-label" for="vue-OAuthScopes">Scopes</label>
            <input
                type="text"
                class="form-control"
                id="vue-OAuthScopes"
                placeholder="openid profile email"
                :value="settings.oAuthScopes"
                @input="update('oAuthScopes', ($event.target as HTMLInputElement).value)"
            />
            <span class="hint">A space-separated list of OAuth2 scopes to request.</span>
        </div>
    </template>
</template>
