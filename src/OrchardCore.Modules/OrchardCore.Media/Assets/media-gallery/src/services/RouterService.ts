import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import router from "../router";
import { useEventBus } from "./UseEventBus";
import { useGlobals } from "./Globals";
import { useLocalStorage } from "./LocalStorage";

const { selectedDirectory, rootDirectory, directoryIndex, setSelectedDirectory } = useGlobals();
const { localStorageData } = useLocalStorage();
const { emit } = useEventBus();

export const useRouterService = (): void => {
  if (router.currentRoute.value) {
    const routePath = router.currentRoute.value.params.path as string | undefined;

    if (!routePath) {
      // Home route - restore from localStorage or default to root
      if (localStorageData.value.selectedDirectory?.directoryPath) {
        const storedDir = localStorageData.value.selectedDirectory;
        const foundDir = directoryIndex.value.get(storedDir.directoryPath);

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
      const routerDirectory = directoryIndex.value.get(routePath);

      if (routerDirectory) {
        setSelectedDirectory(routerDirectory);
      } else {
        setSelectedDirectory(rootDirectory.value);
      }
    }

    emit("DirSelected", selectedDirectory.value as IFileLibraryItemDto);
  } else if (localStorageData.value.selectedDirectory != null) {
    const storedDir = localStorageData.value.selectedDirectory;
    const foundDir = directoryIndex.value.get(storedDir.directoryPath);

    if (foundDir) {
      setSelectedDirectory(foundDir);
    } else {
      setSelectedDirectory(rootDirectory.value);
    }

    emit("DirSelected", selectedDirectory.value as IFileLibraryItemDto);
  }
};
