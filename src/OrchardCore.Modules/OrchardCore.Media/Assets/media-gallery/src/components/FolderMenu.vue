<template>
  <div>
    <a href="javascript:void(0)" class="tw:inline-flex tw:items-center tw:justify-center tw:gap-1.5 tw:text-[0.8125rem] tw:font-normal tw:leading-normal tw:cursor-pointer tw:select-none tw:bg-transparent tw:border-0 tw:text-[#6c757d] hover:tw:text-[#212529] tw:no-underline action-button tw:px-4 tw:py-3"
      :title="t.ActionFolderTitle" @click.stop="toggle">
      <fa-icon icon="fas fa-ellipsis-v" size="xl"></fa-icon>
    </a>
    <p-menu ref="menu" id="overlay_folder_menu" class="file-app" :model="items" :popup="true" />
  </div>
  <ModalsContainer v-if="showModal" />
</template>

<script setup lang="ts">
import { computed, PropType, ref } from "vue";
import { v4 as uuidv4 } from 'uuid';
import { FolderAction, IFileLibraryItemDto } from "@bloom/media/interfaces";
import { BASE_DIR } from "@bloom/media/constants";
import { ModalsContainer } from 'vue-final-modal';
import { getTranslations } from "@bloom/helpers/localizations";
import { useFolderActionModal } from "../services/FolderActionModalService";
import { useEventBus } from "../services/UseEventBus";
import { usePermissions } from "../services/Permissions";
import { notify, NotificationMessage } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";

const props = defineProps({
  folder: {
    type: Object as PropType<IFileLibraryItemDto>,
    required: true
  }
});

const t = getTranslations();
const { showFolderActionModal } = useFolderActionModal();
const { on, emit } = useEventBus();
const { canManageFolder } = usePermissions();

const menu = ref();
const showModal = ref(false);

on("CloseFileMenus", (sender: unknown) => {
  if (sender !== menu) {
    menu.value?.hide();
  }
});

const isRoot = computed(() => {
  return props.folder.directoryPath === "" || props.folder.directoryPath === BASE_DIR || props.folder.directoryPath === "/";
});

const openModal = (action: FolderAction, title: string) => {
  if (!canManageFolder(props.folder.directoryPath)) {
    notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFolder, severity: SeverityLevel.Warn }));
    return;
  }

  showFolderActionModal({
    folder: props.folder,
    action,
    uuid: uuidv4(),
    modalTitle: title,
  });
};

const items = computed(() => {
  const menuItems = [
    {
      label: t.CreateSubFolder,
      icon: 'fa-solid fa-folder-plus',
      command: () => openModal(FolderAction.Create, t.CreateFolderTitle ?? t.ActionFolderTitle)
    }
  ];

  if (!isRoot.value) {
    menuItems.push({
      label: t.Delete,
      icon: 'fa-solid fa-trash',
      command: () => openModal(FolderAction.Delete, t.DeleteFolderTitle ?? t.ActionFolderTitle)
    });
  }

  return [{ items: menuItems }];
});

const toggle = (event: any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
  emit("CloseFileMenus", menu);
  menu.value?.toggle(event);
};
</script>
