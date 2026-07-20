import axios from "axios";
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from "axios";
import dbg from "debug";
import { getAntiForgeryToken } from "../helpers/globals";

const debug = dbg("orchardcore:bloom:api-service");

export type AuthType = "cookie" | "bearer";

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
     * Dynamic token provider for "bearer" auth. When set, it is awaited on every request
     * and takes precedence over the static token, letting a caller acquire/renew silently.
     */
    getToken?: () => string | null | Promise<string | null>;
    /**
     * Called once per request when a "bearer" call returns 401, to obtain a fresh token.
     * If it resolves to a token, the request is retried a single time with it.
     */
    onRenew?: () => Promise<string | null>;
}

/**
 * A generic, reusable HTTP service for calling OrchardCore API endpoints.
 * Wraps Axios with sensible defaults and supports cookie or Bearer token auth.
 */
export class ApiService {
    private readonly instance: AxiosInstance;
    private readonly authType: AuthType;
    private token: string | null;
    private readonly getToken?: () => string | null | Promise<string | null>;
    private readonly onRenew?: () => Promise<string | null>;

    constructor(options?: ApiServiceOptions) {
        const baseUrl = options?.baseUrl ?? "";
        this.authType = options?.authType ?? "cookie";
        this.token = options?.token ?? null;
        this.getToken = options?.getToken;
        this.onRenew = options?.onRenew;

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
                const token = this.getToken ? await this.getToken() : this.token;
                if (token) {
                    config.headers["Authorization"] = `Bearer ${token}`;
                }
            }
            return config;
        });

        // On a 401 for a bearer call, renew the token once and retry the request.
        this.instance.interceptors.response.use(
            (response) => response,
            async (error) => {
                const config = error?.config;
                if (
                    this.authType === "bearer" &&
                    this.onRenew &&
                    error?.response?.status === 401 &&
                    config &&
                    !config.__isRetry
                ) {
                    config.__isRetry = true;
                    const token = await this.onRenew();
                    if (token) {
                        config.headers = config.headers ?? {};
                        config.headers["Authorization"] = `Bearer ${token}`;
                        return this.instance.request(config);
                    }
                }
                return Promise.reject(error);
            },
        );
    }

    /**
     * Update the Bearer token for subsequent requests.
     */
    setToken(token: string): void {
        this.token = token;
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

}

/**
 * Create a default ApiService instance.
 */
export function createApiService(options?: ApiServiceOptions): ApiService {
    return new ApiService(options);
}
