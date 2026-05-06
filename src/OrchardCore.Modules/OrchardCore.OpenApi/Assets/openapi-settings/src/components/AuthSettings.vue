<script setup lang="ts">
import { getTranslations } from '@bloom/helpers/localizations'

const t = (key: string) => getTranslations()[key] ?? key

interface Settings {
    allowAnonymousSchemaAccess: boolean
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
            <option :value="2">{{ t('oauth2ClientCredentials') }}</option>
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
        <p v-html="t('openIdTokenValidation')"></p>
        <hr />
        <p class="mb-0">{{ t('sessionCookieHint') }}</p>
    </div>

    <!-- Client Credentials info -->
    <div v-if="isClientCreds()" class="alert alert-info mb-3">
        <p>{{ t('clientCredsInfo') }}</p>
        <hr />
        <p v-html="t('clientCredsEnsure')"></p>
        <hr />
        <p v-html="t('openIdTokenValidation')"></p>
        <hr />
        <p class="mb-0">{{ t('sessionCookieHint') }}</p>
    </div>

    <!-- Authorization URL (PKCE only) -->
    <div v-if="isPkce()" class="mb-3">
        <label class="form-label" for="vue-AuthorizationUrl">{{ t('authorizationUrl') }}</label>
        <input
            type="text"
            class="form-control"
            id="vue-AuthorizationUrl"
            placeholder="/connect/authorize"
            :value="settings.authorizationUrl"
            @input="update('authorizationUrl', ($event.target as HTMLInputElement).value)"
        />
        <span class="hint">{{ t('authorizationUrlHint') }}</span>
    </div>

    <!-- Shared OAuth2 fields -->
    <template v-if="isOAuth()">
        <div class="mb-3">
            <label class="form-label" for="vue-TokenUrl">{{ t('tokenUrl') }}</label>
            <input
                type="text"
                class="form-control"
                id="vue-TokenUrl"
                placeholder="/connect/token"
                :value="settings.tokenUrl"
                @input="update('tokenUrl', ($event.target as HTMLInputElement).value)"
            />
            <span class="hint">{{ t('tokenUrlHint') }}</span>
        </div>

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
        </div>
    </template>
</template>
