import { ref } from 'vue'
import axios from 'axios'
import { ApiService } from '@bloom/services/api-service'
import { Client, TestConnectionRequest, TestConnectionResult, OpenApiAuthenticationType } from '@bloom/services/OpenApiClient'
import { notify } from '@bloom/services/notifications/notifier'
import { SeverityLevel } from '@bloom/services/notifications/interfaces'
import { getTenantPathBase } from '@bloom/helpers/globals'

const apiService = new ApiService()
const client = new Client(getTenantPathBase(), apiService.getAxiosInstance())

export function useOpenApi() {
    const testing = ref(false)
    const testSuccess = ref(false)
    const testResult = ref<TestConnectionResult | null>(null)

    async function testConnection(config: {
        authenticationType: number
        tokenUrl: string
        authorizationUrl?: string
        clientId: string
        clientSecret?: string
        scopes?: string
    }): Promise<void> {
        testing.value = true
        testSuccess.value = false
        testResult.value = null

        try {
            const request = new TestConnectionRequest({
                authenticationType: config.authenticationType as OpenApiAuthenticationType,
                tokenUrl: config.tokenUrl,
                authorizationUrl: config.authorizationUrl,
                clientId: config.clientId,
                clientSecret: config.clientSecret,
                scopes: config.scopes,
            })

            const result = await client.apiTestOpenApiConnection(request)
            testSuccess.value = true
            testResult.value = result

            notify(
                {
                    summary: 'Success',
                    detail: result.message || 'Connection test passed.',
                    severity: SeverityLevel.Success,
                } as any, // eslint-disable-line @typescript-eslint/no-explicit-any
            )
        }
        catch (err) {
            const problemDetails = axios.isAxiosError(err) ? err.response?.data : err

            testResult.value = new TestConnectionResult({
                message: problemDetails?.detail ?? (err instanceof Error ? err.message : 'An unknown error occurred.'),
            })

            notify(problemDetails, SeverityLevel.Error)
        }
        finally {
            testing.value = false
        }
    }

    function clearResult() {
        testSuccess.value = false
        testResult.value = null
    }

    return { testing, testSuccess, testResult, testConnection, clearResult }
}
