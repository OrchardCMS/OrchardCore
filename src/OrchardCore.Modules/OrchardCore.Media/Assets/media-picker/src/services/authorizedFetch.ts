import { getAccessToken, isAuthConfigured } from "@media-gallery-auth";

/**
 * fetch() that attaches the silently-acquired bearer token when the Media API is in bearer mode.
 * In cookie mode it behaves like a normal same-origin fetch (the ambient admin cookie authenticates)
 * and calls fetch exactly as it would have been called directly.
 */
export async function authorizedFetch(input: string, init?: RequestInit): Promise<Response> {
  if (isAuthConfigured()) {
    const token = await getAccessToken();
    if (token) {
      return fetch(input, {
        ...init,
        headers: {
          ...(init?.headers ?? {}),
          Authorization: `Bearer ${token}`,
        },
      });
    }
  }

  return init ? fetch(input, init) : fetch(input);
}
