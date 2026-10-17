import type { ApiService } from "@bloom/services/api-service";
import type { DesignIssue } from "./types";

/**
 * The ProblemDetails a designer endpoint returns when it rejects a request. A conflict (409) tells who changed the
 * draft and when, and the revision it is at.
 */
export interface ProblemDetails {
    status?: number;
    title?: string;
    detail?: string;
    currentRevision?: number;
    modifiedBy?: string | null;
    modifiedUtc?: string | null;
    issues?: DesignIssue[];
    [extension: string]: unknown;
}

/**
 * An HTTP failure of a designer endpoint, with the ProblemDetails body when the server sent one.
 * `status` is 0 when the request never reached the server (offline, network error).
 */
export class DesignerApiError extends Error {
    readonly status: number;
    readonly problem: ProblemDetails;

    constructor(status: number, problem: ProblemDetails | undefined, message?: string) {
        super(problem?.detail ?? problem?.title ?? message ?? `Request failed with status ${status}`);
        this.name = "DesignerApiError";
        this.status = status;
        this.problem = problem ?? {};
    }

    get isConflict() {
        return this.status === 409;
    }

    get isNetworkError() {
        return this.status === 0;
    }
}

interface AxiosLikeError {
    message?: string;
    response?: { status: number; data?: unknown };
}

/**
 * Turns a failed request into a {@link DesignerApiError}.
 */
export const toDesignerApiError = (error: unknown): DesignerApiError => {
    if (error instanceof DesignerApiError) {
        return error;
    }

    const axiosError = error as AxiosLikeError;

    if (axiosError?.response) {
        const data = axiosError.response.data;

        return new DesignerApiError(axiosError.response.status, typeof data === "object" && data !== null ? (data as ProblemDetails) : undefined);
    }

    return new DesignerApiError(0, undefined, axiosError?.message);
};

/**
 * Adds query string values to a URL that may already have some.
 */
export const withQuery = (url: string, query: Record<string, string | number>) => {
    const separator = url.includes("?") ? "&" : "?";

    return url + separator + new URLSearchParams(Object.entries(query).map(([key, value]) => [key, String(value)])).toString();
};

/**
 * Returns the URL of an endpoint the current mode has. An endpoint a mode doesn't have (its URL is null) is a
 * programming error, not a server failure.
 */
export const required = (url: string | null | undefined, name: string) => {
    if (!url) {
        throw new Error(`The ${name} endpoint is not available in this mode.`);
    }

    return url;
};

/**
 * Forms are posted as multipart: the explicit header stops axios from converting the FormData to JSON (the service
 * defaults to application/json) and lets the browser add the boundary.
 */
export const multipart = { headers: { "Content-Type": "multipart/form-data" } };

/**
 * Runs a request of an {@link ApiService} and returns its data, or throws a {@link DesignerApiError}.
 */
export const call = async <T>(request: () => Promise<{ data: T }>): Promise<T> => {
    try {
        return (await request()).data;
    } catch (error) {
        throw toDesignerApiError(error);
    }
};

export type { ApiService };
