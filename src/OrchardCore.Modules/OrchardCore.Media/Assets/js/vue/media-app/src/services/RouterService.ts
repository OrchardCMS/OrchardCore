import { IFileLibraryItemDto } from "../interfaces/interfaces";
import router from "../router";
import { useEventBus } from "./UseEventBus";
import { useGlobals } from "./Globals";
import { useLocalStorage } from "./LocalStorage";

const { selectedDirectory, rootDirectory, assetsStore, setSelectedDirectory } = useGlobals();
const { localStorageData } = useLocalStorage();
const { emit } = useEventBus();

export const useRouterService = (): void => {
  if (router.currentRoute.value) {
    const routePath = router.currentRoute.value.params.path as string | undefined;

    if (!routePath) {
      // Home route - restore from localStorage or default to root
      if (localStorageData.value.selectedDirectory?.directoryPath) {
        const storedDir = localStorageData.value.selectedDirectory;
        const foundDir = assetsStore.value.find((x) => x.isDirectory && x.directoryPath == storedDir.directoryPath);

        if (foundDir) {
          setSelectedDirectory(foundDir);
        } else {
          setSelectedDirectory(rootDirectory.value);
        }
      } else {
        setSelectedDirectory(rootDirectory.value);
      }
    } else {
      // Folder route - find directory by directoryPath
      const routerDirectory = assetsStore.value.find((x) => x.isDirectory && x.directoryPath == routePath);

      if (routerDirectory) {
        setSelectedDirectory(routerDirectory);
      } else {
        setSelectedDirectory(rootDirectory.value);
      }
    }

    emit("DirSelected", selectedDirectory.value as IFileLibraryItemDto);
  } else if (localStorageData.value.selectedDirectory != null) {
    const storedDir = localStorageData.value.selectedDirectory;
    const foundDir = assetsStore.value.find((x) => x.isDirectory && x.directoryPath == storedDir.directoryPath);

    if (foundDir) {
      setSelectedDirectory(foundDir);
    } else {
      setSelectedDirectory(rootDirectory.value);
    }

    emit("DirSelected", selectedDirectory.value as IFileLibraryItemDto);
  }
};
