import { computed } from "vue";

/**
 * Simple permission check — OrchardCore handles authorization server-side,
 * so the frontend always allows management (API will return 403 if not authorized).
 */
export function usePermissions() {
  const canManage = computed(() => {
    return true;
  });

  const canManageFolder = (_directory: string): boolean => {
    return true;
  };

  return { canManage, canManageFolder };
}

export const supportsUpload = (): boolean => {
  return true;
};

export const supportsDelete = (): boolean => {
  return true;
};
