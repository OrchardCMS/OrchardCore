import { computed } from "vue";
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useGlobals } from "../services/Globals";

export function useFileListFiltering() {
  const { fileFilter, fileItems, sortBy, sortAsc } = useGlobals();

  const filteredFileItems = computed(() => {
    const filter = fileFilter.value.toLowerCase();
    let filtered = fileItems.value.filter((item: IFileLibraryItemDto) => {
      return item.name.toLowerCase().indexOf(filter) > -1;
    });

    const asc = sortAsc.value;

    switch (sortBy.value) {
      case "size":
        filtered.sort((a: IFileLibraryItemDto, b: IFileLibraryItemDto) => {
          return asc ? (a.size ?? 0) - (b.size ?? 0) : (b.size ?? 0) - (a.size ?? 0);
        });
        break;
      case "mime":
        filtered.sort((a: IFileLibraryItemDto, b: IFileLibraryItemDto) => {
          return asc ? (a.mime ?? "").toLowerCase().localeCompare((b.mime ?? "").toLowerCase()) : (b.mime ?? "").toLowerCase().localeCompare((a.mime ?? "").toLowerCase());
        });
        break;
      case "lastModify":
        filtered.sort((a: IFileLibraryItemDto, b: IFileLibraryItemDto) => {
          return asc ? new Date(a.lastModifiedUtc ?? 0).getTime() - new Date(b.lastModifiedUtc ?? 0).getTime() : new Date(b.lastModifiedUtc ?? 0).getTime() - new Date(a.lastModifiedUtc ?? 0).getTime();
        });
        break;
      default:
        filtered.sort((a: IFileLibraryItemDto, b: IFileLibraryItemDto) => {
          return asc ? a.name.toLowerCase().localeCompare(b.name.toLowerCase()) : b.name.toLowerCase().localeCompare(a.name.toLowerCase());
        });
    }

    return filtered;
  });

  return { filteredFileItems };
}
