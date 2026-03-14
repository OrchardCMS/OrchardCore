import { computed } from "vue";
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useGlobals } from "../services/Globals";

export function useBreadcrumbs() {
  const { directoryIndex, selectedDirectory, rootDirectory } = useGlobals();

  const breadcrumbs = computed((): IFileLibraryItemDto[] => {
    const result: IFileLibraryItemDto[] = [];
    const dirMap = directoryIndex.value;

    if (dirMap.size > 0 && selectedDirectory.value) {
      result.push(rootDirectory.value);

      if (selectedDirectory.value.directoryPath) {
        const segments = selectedDirectory.value.directoryPath.split("/");
        let path = "";

        segments.forEach((segment, index) => {
          path = index > 0 ? path + "/" + segment : segment;
          const dir = dirMap.get(path);

          if (dir) {
            result.push(dir);
          }
        });
      }
    }

    return result;
  });

  return { breadcrumbs };
}
