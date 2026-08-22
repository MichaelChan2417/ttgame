// Regenerate src/legal.ts from ../../legal/{terms,privacy}.html so the Worker serves the
// latest Terms & Privacy after a single `npx wrangler deploy`.
const fs = require("fs");
const path = require("path");
const legalDir = path.join(__dirname, "..", "..", "legal");
const esc = s => s.replace(/\\/g, "\\\\").replace(/`/g, "\\`").replace(/\$\{/g, "\\${");
const terms = fs.readFileSync(path.join(legalDir, "terms.html"), "utf8");
const privacy = fs.readFileSync(path.join(legalDir, "privacy.html"), "utf8");
const out =
`// Auto-generated from /legal/*.html by sync-legal.js — do not edit by hand.
export const TERMS_HTML = \`
${esc(terms)}\`;

export const PRIVACY_HTML = \`
${esc(privacy)}\`;
`;
fs.writeFileSync(path.join(__dirname, "src", "legal.ts"), out);
console.log(`wrote src/legal.ts  (terms=${terms.length}B, privacy=${privacy.length}B)`);
