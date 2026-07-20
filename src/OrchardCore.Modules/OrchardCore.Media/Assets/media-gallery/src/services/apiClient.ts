import type { AxiosInstance } from "axios";
import { ApiService } from "@bloom/services/api-service";
import { getAccessToken, renewAccessToken } from "./auth";

/**
 * Shared bearer-mode ApiService for the media gallery. Its axios instance is passed to the
 * NSwag-generated MediaApiClient (via FileDataService) so every Media API call carries the
 * silently-acquired OIDC access token and retries once on a 401 after a silent renewal.
 *
 * The instance has no baseURL: the generated client builds the full "/api/media/..." URL
 * itself, so a baseURL here would be prepended twice.
 */
let sharedApiService: ApiService | null = null;

function getApiService(): ApiService {
  if (!sharedApiService) {
    sharedApiService = new ApiService({
      authType: "bearer",
      getToken: getAccessToken,
      onRenew: renewAccessToken,
    });
  }
  return sharedApiService;
}

export function getSharedAxios(): AxiosInstance {
  return getApiService().getAxiosInstance();
}
