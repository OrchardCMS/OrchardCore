import { LS_ID } from "@bloom/media/constants";
import { computed, ref, Ref, watch } from "vue";
import { useGlobals } from "./Globals";
import { ILocalStorageData } from "@bloom/media/interfaces";

const { selectedDirectory, rootDirectory, setSelectedDirectory } = useGlobals();

const smallThumbs: Ref<boolean> = ref(false);
const gridView: Ref<boolean> = ref(false);
const pageSize: Ref<number> = ref(10);
const largeThumbs: Ref<boolean> = ref(false);

export function useLocalStorage() {
  const localStorageData = computed<ILocalStorageData>({
    get() {
      return {
        smallThumbs: smallThumbs.value,
        selectedDirectory: selectedDirectory.value,
        gridView: gridView.value,
        pageSize: pageSize.value,
        largeThumbs: largeThumbs.value,
      };
    },
    set(localStorageData) {
      if (!localStorageData) {
        return;
      }

      smallThumbs.value = localStorageData.smallThumbs;
      setSelectedDirectory(localStorageData.selectedDirectory);
      gridView.value = localStorageData.gridView;
      pageSize.value = localStorageData.pageSize ?? 10;
      largeThumbs.value = localStorageData.largeThumbs ?? false;
    },
  });

  const setLocalStorage = () => {
    if (!localStorage.getItem(LS_ID)) {
      setSelectedDirectory(rootDirectory.value);
      return;
    }

    const fileApplicationPrefs = localStorage.getItem(LS_ID);

    if (fileApplicationPrefs != null) {
      localStorageData.value = JSON.parse(fileApplicationPrefs);
    }
  };

  watch(localStorageData, (data) => {
    localStorage.setItem(LS_ID, JSON.stringify(data));
  });

  return { setLocalStorage, localStorageData, smallThumbs, gridView, pageSize, largeThumbs };
}
