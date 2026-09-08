// Thin entry that lives inside the Vite root (standalone/) so the dev server can serve it directly;
// the real logic is in src/. A `<script src="../src/…">` in the HTML would be URL-normalized by the
// browser to `/src/…` and 404 against this root, but a JS import escaping the root is rewritten by
// Vite (via /@fs), so it resolves in both dev and build.
import "../src/standalone";
