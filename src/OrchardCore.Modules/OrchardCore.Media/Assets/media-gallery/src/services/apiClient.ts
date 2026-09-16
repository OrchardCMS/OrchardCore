import type { AxiosInstance } from "axios";
import { ApiService } from "@bloom/services/api-service";
import { getAccessToken, renewAccessToken, isAuthConfigured } from "./media-gallery-auth";

/**
 * Shared ApiService for the media gallery, whose axios instance is passed to the NSwag-generated
 * MediaApiClient (via FileDataService). It matches the site's Media API authentication mode: when
 * OIDC has been configured (bearer mode) every call carries the silently-acquired access token and
 * retries once on a 401 after a silent renewal; otherwise (cookie mode) it uses the ambient admin
 * cookie and sends the antiforgery token. Exactly one mode is active — never both.
 *
 * The instance has no baseURL: the generated client builds the full "/api/media/..." URL itself,
 * so a baseURL here would be prepended twice.
 */
let sharedApiService: ApiService | null = null;

function getApiService(): ApiService {
  if (!sharedApiService) {
    sharedApiService = isAuthConfigured()
      ? new ApiService({ authType: "bearer", getToken: getAccessToken, onRenew: renewAccessToken })
      : new ApiService({ authType: "cookie" });
  }
  return sharedApiService;
}

export function getSharedAxios(): AxiosInstance {
  return getApiService().getAxiosInstance();
}
