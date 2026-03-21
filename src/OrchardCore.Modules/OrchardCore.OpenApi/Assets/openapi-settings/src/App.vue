<script setup lang="ts">
import { reactive, ref, watch, onMounted } from 'vue'
import AuthSettings from './components/AuthSettings.vue'
import ConnectionTester from './components/ConnectionTester.vue'

interface OpenApiSettingsData {
    authenticationType: number
    authorizationUrl: string
    tokenUrl: string
    oAuthClientId: string
    oAuthScopes: string
    pathBase?: string
    isSwaggerUIEnabled?: boolean
    isReDocUIEnabled?: boolean
    isScalarUIEnabled?: boolean
}

const settings = reactive<OpenApiSettingsData>({
    authenticationType: 0,
    authorizationUrl: '',
    tokenUrl: '',
    oAuthClientId: '',
    oAuthScopes: '',
})

const pathBase = ref('')

const featureStatus = reactive({
    swaggerUI: false,
    reDocUI: false,
    scalarUI: false,
})

// Load initial data from the JSON blob injected by Razor.
onMounted(() => {
    const dataEl = document.getElementById('openapi-settings-data')
    if (dataEl?.textContent) {
        try {
            const data = JSON.parse(dataEl.textContent) as OpenApiSettingsData
            pathBase.value = data.pathBase ?? ''
            featureStatus.swaggerUI = data.isSwaggerUIEnabled ?? false
            featureStatus.reDocUI = data.isReDocUIEnabled ?? false
            featureStatus.scalarUI = data.isScalarUIEnabled ?? false
            delete data.pathBase
            delete data.isSwaggerUIEnabled
            delete data.isReDocUIEnabled
            delete data.isScalarUIEnabled
            Object.assign(settings, data)
        }
        catch {
            // Use defaults if JSON is invalid.
        }
    }
})

// Sync reactive state to hidden form inputs so ASP.NET model binding works.
// All inputs are type="hidden", so we always set the value attribute.
// For booleans, ASP.NET model binding expects "true"/"false" strings.
function syncToHiddenInput(name: string, value: unknown) {
    const input = document.querySelector<HTMLInputElement>(`input[name$=".${name}"], input[name="${name}"]`)
    if (input) {
        if (typeof value === 'boolean') {
            input.value = value ? 'true' : 'false'
        }
        else {
            input.value = String(value ?? '')
        }
    }
}

watch(() => settings.authenticationType, (v) => syncToHiddenInput('AuthenticationType', v))
watch(() => settings.authorizationUrl, (v) => syncToHiddenInput('AuthorizationUrl', v))
watch(() => settings.tokenUrl, (v) => syncToHiddenInput('TokenUrl', v))
watch(() => settings.oAuthClientId, (v) => syncToHiddenInput('OAuthClientId', v))
watch(() => settings.oAuthScopes, (v) => syncToHiddenInput('OAuthScopes', v))

const isOAuth = () => settings.authenticationType === 1 || settings.authenticationType === 2
</script>

<template>
    <AuthSettings :settings="settings" :feature-status="featureStatus" :path-base="pathBase" @update:settings="Object.assign(settings, $event)" />
    <ConnectionTester
        v-if="isOAuth()"
        :authentication-type="settings.authenticationType"
        :token-url="settings.tokenUrl"
        :authorization-url="settings.authorizationUrl"
        :client-id="settings.oAuthClientId"
        :scopes="settings.oAuthScopes"
    />
</template>
