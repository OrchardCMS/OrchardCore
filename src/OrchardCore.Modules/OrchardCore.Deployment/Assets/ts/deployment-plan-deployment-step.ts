import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { initReverseToggle } from "@orchardcore/bloom/components/checkbox-relations";

observeAndInit("[data-reversetoggle]", initReverseToggle);
