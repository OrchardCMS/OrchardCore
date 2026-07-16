import axios from "axios";
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from "axios";
import dbg from "debug";
import { getAntiForgeryToken } from "../helpers/globals";

const debug = dbg("orchardcore:bloom:api-service");

export type AuthType = "cookie" | "bearer";

/**
 * Client Credentials configuration for automatic token acquisition.
 */
export interface ClientCredentialsConfig {
    /** The token endpoint URL (e.g., "/connect/token"). */
    tokenUrl: string;
    /** The OAuth2 client ID. */
    clientId: string;
    /** The OAuth2 client secret. */
    clientSecret: string;
    /** Space-separated scopes (e.g., "api"). */
    scopes?: string;
}

/**
 * Options for creating an ApiService instance.
 */
export interface ApiServiceOptions {
    /** Base URL for all requests. Defaults to current origin. */
    baseUrl?: string;
    /** Authentication type. Defaults to "cookie". */
    authType?: AuthType;
    /** Bearer token for "bearer" auth. Can be updated later via setToken(). */
    token?: string;
    /**
     * Client Credentials configuration. When provided, the service automatically
     * acquires a Bearer token on the first request. Sets authType to "bearer".
     */
    clientCredentials?: ClientCredentialsConfig;
}

/**
 * A generic, reusable HTTP service for calling OrchardCore API endpoints.
 * Wraps Axios with sensible defaults and supports cookie or Bearer token auth.
 */
export class ApiService {
    private readonly instance: AxiosInstance;
    private readonly authType: AuthType;
    private readonly clientCredentials: ClientCredentialsConfig | null;
    private token: string | null;
    private tokenPromise: Promise<string> | null = null;

    constructor(options?: ApiServiceOptions) {
        const baseUrl = options?.baseUrl ?? "";
        this.clientCredentials = options?.clientCredentials ?? null;
        this.authType = this.clientCredentials ? "bearer" : (options?.authType ?? "cookie");
        this.token = options?.token ?? null;

        this.instance = axios.create({
            baseURL: baseUrl,
            withCredentials: this.authType === "cookie",
            headers: {
                "Content-Type": "application/json",
            },
        });

        this.instance.interceptors.request.use(async (config) => {
            if (this.authType === "cookie") {
                const antiForgeryToken = getAntiForgeryToken();
                if (antiForgeryToken) {
                    config.headers["RequestVerificationToken"] = antiForgeryToken;
                }
            } else if (this.authType === "bearer") {
                if (!this.token && this.clientCredentials) {
                    await this.acquireToken();
                }
                if (this.token) {
                    config.headers["Authorization"] = `Bearer ${this.token}`;
                }
            }
            return config;
        });
    }

    /**
     * Update the Bearer token for subsequent requests.
     */
    setToken(token: string): void {
        this.token = token;
        this.tokenPromise = null;
    }

    /**
     * Returns the underlying Axios instance for use with generated clients
     * (e.g., NSwag OpenApiClient) that accept an AxiosInstance in their constructor.
     */
    getAxiosInstance(): AxiosInstance {
        return this.instance;
    }

    /**
     * Perform a GET request.
     */
    async get<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        debug("GET %s", url);
        return this.instance.get<T>(url, config);
    }

    /**
     * Perform a POST request.
     */
    async post<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        debug("POST %s", url);
        return this.instance.post<T>(url, data, config);
    }

    /**
     * Perform a PUT request.
     */
    async put<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        debug("PUT %s", url);
        return this.instance.put<T>(url, data, config);
    }

    /**
     * Perform a PATCH request.
     */
    async patch<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        debug("PATCH %s", url);
        return this.instance.patch<T>(url, data, config);
    }

    /**
     * Perform a DELETE request.
     */
    async delete<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        debug("DELETE %s", url);
        return this.instance.delete<T>(url, config);
    }

    /**
     * Acquires a Bearer token using the Client Credentials flow.
     * Deduplicates concurrent requests so the token endpoint is called only once.
     */
    private async acquireToken(): Promise<void> {
        if (!this.clientCredentials) {
            return;
        }

        if (!this.tokenPromise) {
            this.tokenPromise = this.exchangeClientCredentials();
        }

        this.token = await this.tokenPromise;
    }

    private async exchangeClientCredentials(): Promise<string> {
        const { tokenUrl, clientId, clientSecret, scopes } = this.clientCredentials!;

        debug("Acquiring token from %s", tokenUrl);

        const params = new URLSearchParams({
            grant_type: "client_credentials",
            client_id: clientId,
            client_secret: clientSecret,
        });

        if (scopes) {
            params.set("scope", scopes);
        }

        const response = await axios.post(tokenUrl, params, {
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
        });

        const accessToken = response.data?.access_token;

        if (!accessToken) {
            throw new Error("Token response did not contain an access_token.");
        }

        debug("Token acquired successfully");

        return accessToken;
    }
}

/**
 * Create a default ApiService instance.
 */
export function createApiService(options?: ApiServiceOptions): ApiService {
    return new ApiService(options);
}
