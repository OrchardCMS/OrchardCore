import { ref, onMounted, onUnmounted } from "vue";
import { IPermittedStorageResult, IFileStoreCapabilities } from "@bloom/media/interfaces";
import { FileDataService } from "@bloom/media/api/file-data-service";

export function useStoragePopover(basePath: string) {
  const storageInfo = ref<IPermittedStorageResult | null>(null);
  const storageCapabilities = ref<IFileStoreCapabilities | null>(null);
  const showStoragePopover = ref(false);
  const storageLoading = ref(false);

  const fetchStorageInfo = async () => {
    storageLoading.value = true;
    try {
      const service = new FileDataService(basePath);
      const [storage, caps] = await Promise.all([
        service.getPermittedStorage(),
        service.getCapabilities(),
      ]);
      storageInfo.value = storage;
      storageCapabilities.value = caps;
    } catch (e) {
      console.error("Failed to fetch storage info", e);
      storageInfo.value = null;
      storageCapabilities.value = null;
    } finally {
      storageLoading.value = false;
    }
  };

  const toggleStoragePopover = async () => {
    showStoragePopover.value = !showStoragePopover.value;
    if (showStoragePopover.value && !storageCapabilities.value) {
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

  return { storageInfo, storageCapabilities, showStoragePopover, storageLoading, toggleStoragePopover };
}
