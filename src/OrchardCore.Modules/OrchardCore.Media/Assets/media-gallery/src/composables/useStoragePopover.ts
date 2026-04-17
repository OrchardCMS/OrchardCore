import { ref, onMounted, onUnmounted } from "vue";
import { IPermittedStorageResult } from "@bloom/media/interfaces";
import { FileDataService } from "@bloom/media/api/file-data-service";

export function useStoragePopover(basePath: string) {
  const storageInfo = ref<IPermittedStorageResult | null>(null);
  const showStoragePopover = ref(false);
  const storageLoading = ref(false);

  const fetchStorageInfo = async () => {
    storageLoading.value = true;
    try {
      const service = new FileDataService(basePath);
      storageInfo.value = await service.getPermittedStorage();
    } catch (e) {
      console.error("Failed to fetch storage info", e);
      storageInfo.value = null;
    } finally {
      storageLoading.value = false;
    }
  };

  const toggleStoragePopover = async () => {
    showStoragePopover.value = !showStoragePopover.value;
    if (showStoragePopover.value && !storageInfo.value) {
      await fetchStorageInfo();
    }
  };

  const handleClickOutside = (e: MouseEvent) => {
    if (showStoragePopover.value && !(e.target as HTMLElement)?.closest('.storage-info-btn, .storage-popover')) {
      showStoragePopover.value = false;
    }
  };

  onMounted(() => document.addEventListener('click', handleClickOutside));
  onUnmounted(() => document.removeEventListener('click', handleClickOutside));

  return { storageInfo, showStoragePopover, storageLoading, toggleStoragePopover };
}
