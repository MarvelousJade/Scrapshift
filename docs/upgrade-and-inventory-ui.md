# Upgrade, reward and inventory UI follow-up

Compared the user's October 12 Brotato and Scrapshift screenshots. Kept the existing cream mascot and cybernetic artwork.

- Common upgrade cards have black outlines rather than white tier borders. Colored tiers retain their colored outlines. Reroll has an opaque black background, an F key indicator and a currency icon in both shop and level-up. F is now bound; R remains an alias.
- Removed the owned-item/weapon row from the level-up screen. Pause and shop inspection remain available.
- Pending upgrades show the current mascot's face next to a green arrow. Pending crates show wooden planks, a lid, frame and fasteners; ground crates also use planks and a wooden brace.
- Owned shop item and weapon tiles turn white on hover. The reference's separate `ui/sounds/inventory_element_focus.wav` is used, with entry detection to prevent replaying it every repaint. Normal button audio remains separate. Audio provenance is recorded in `artifacts/reference-media-provenance.json`.
- Shop currency text is vertically centered in the same-height row as its icon. Removed the item-count suffix from the Items heading; weapon capacity still appears. Added a muted green separator between Items and the sorting control.
- The active character appears after acquired items, without becoming a gameplay item or affecting sorting, shop rolls or stats. Its hover panel uses the same effect formatter as character selection, avoiding a second divergent description implementation.
- Leading stat icons are 1.55 times the description font size; embedded scaling icons are 1.1 times. Bullet dots use an explicit visible round mark. Layout measurement and rendering share the new sizes so wrapping and scrolling include the enlarged symbols.

Verification uses the existing regression suite and fresh built-player inspection captures, including mixed upgrade tiers, item/weapon hover, rewards and a new character-inspection case. The staged hover fixture calls the same presentation helper as pointer hover but does not certify a native-input playthrough or subjective audio mix. Final counts and package details are recorded in verification.md.

Debugging note: after intentionally removing level-up inventory, the old fixture still targeted a weapon on that page. Updated that obsolete expectation rather than reintroducing the removed UI. Character selection and inventory now share the effect-label formatter; the character portrait is never inserted into RunRules.Items.

## Pause, HUD and surface cleanup

Pause now uses a wide left action column, a separate inventory area with sorting and character inspection, and a full-width bottom timeline. Completed waves are yellow dots, future waves white dots, the current wave carries our mascot in a yellow ring, and wave 20 carries our boss art. Milestones are numbered 5/10/15/20. The caption states actual danger and wave; it does not claim an endless mode. Added a browsable item/weapon Codex and an explicit paused-run abandonment action that enters the loss flow.

Health and experience share a 262-by-38 canvas-unit rectangle with four-unit dark borders. Removed the shared five-by-six-unit offset shadow from Frame: cards and inventory tiles now use a clean dark edge without the displaced grey slab. Original sprite artwork and its own drawn outlines remain intact.

The first Codex build caught an incorrect catalog field name; corrected it to the existing equipment collection. Added a regression that ending a paused run cannot resume or count as victory, and that restarting restores the playing state.
