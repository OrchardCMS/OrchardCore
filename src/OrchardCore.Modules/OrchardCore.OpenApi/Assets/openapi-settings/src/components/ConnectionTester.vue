<script setup lang="ts">
import { ref, computed } from 'vue'
import { useOpenApi } from '../composables/useOpenApi'
import { getTranslations } from '@bloom/helpers/localizations'

const t = (key: string) => getTranslations()[key] ?? key

const props = defineProps<{
    authenticationType: number
    tokenUrl: string
    authorizationUrl: string
    clientId: string
    scopes: string
}>()

const clientSecret = ref('')
const { testing, testSuccess, testResult, testConnection, clearResult } = useOpenApi()

const canTest = computed(() => {
    if (!props.tokenUrl || !props.clientId) return false
    if (props.authenticationType === 1 && !props.authorizationUrl) return false
    if (props.authenticationType === 2 && !clientSecret.value) return false
    return true
})

async function onTestClick() {
    clearResult()

    const request = {
        authenticationType: props.authenticationType,
        tokenUrl: props.tokenUrl,
        authorizationUrl: props.authorizationUrl || undefined,
        clientId: props.clientId,
        clientSecret: clientSecret.value || undefined,
        scopes: props.scopes || undefined,
    }

    await testConnection(request)

    // Clear the secret from the DOM after the test.
    clientSecret.value = ''
}
</script>

<template>
    <div class="card mt-3 mb-3">
        <div class="card-header">
            <h6 class="mb-0">{{ t('testConnection') }}</h6>
        </div>
        <div class="card-body">
            <!-- Client Secret field (Client Credentials only) -->
            <div v-if="authenticationType === 2" class="mb-3">
                <label class="form-label" for="vue-ClientSecret">{{ t('clientSecret') }}</label>
                <input
                    type="password"
                    class="form-control"
                    id="vue-ClientSecret"
                    v-model="clientSecret"
                    :placeholder="t('enterClientSecret')"
                    autocomplete="off"
                />
                <span class="hint">{{ t('clientSecretHint') }}</span>
            </div>

            <button
                type="button"
                class="btn btn-secondary"
                :disabled="!canTest || testing"
                @click="onTestClick"
            >
                <span v-if="testing" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                {{ testing ? t('testing') : t('testConnection') }}
            </button>

            <!-- Result feedback -->
            <div v-if="testResult" class="mt-3">
                <div
                    class="alert mb-0"
                    :class="testSuccess ? 'alert-success' : 'alert-danger'"
                    role="alert"
                >
                    {{ testResult.message }}
                </div>
            </div>
        </div>
    </div>
</template>
