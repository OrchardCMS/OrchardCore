import { createRouter, createWebHashHistory } from "vue-router";
import FileItemsComponent from "../components/FileItemsComponent.vue";

const router = createRouter({
  history: createWebHashHistory("/"),
  routes: [
    {
      path: "/",
      name: "home",
      component: FileItemsComponent,
      props: true,
    },
    { name: "folder", path: "/folder/:path", component: FileItemsComponent, props: true },
  ],
});

export default router;
