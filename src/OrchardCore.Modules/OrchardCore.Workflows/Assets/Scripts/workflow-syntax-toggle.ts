import initSyntaxToggle from "@orchardcore/bloom/components/syntax-toggle";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

observeAndInit("select[data-syntax-toggle]", initSyntaxToggle);
