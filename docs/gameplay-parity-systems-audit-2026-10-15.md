# Gameplay systems continuation — October 15, 2026

Branch: `codex/gameplay-parity`. Reference inspected read-only: `E:/UnityProjects/brotato-(3.x)`.

The user explicitly requested continuing parallel fixes before the full tests, with bug documentation and interview notes kept current. The preceding follow-up passed a **filtered 213-case selection**. Those results predate the changes below. No whole suite, new player build or campaign has been started for this continuation.

## Source audit tracks

| Track | Findings / current work | Status |
| --- | --- | --- |
| Shop and unlocks | Weapon-adjusted prices retain fractional base values through inflation/recycling; lifetime counted achievements check at wave end; Hourglass uses fewer than five remaining seconds during an active wave; Fruit Basket excludes cleanup fruit. | Agent fixes and 13 focused cases authored; root integrating real projectile contact counting. |
| Support actors | Pet range must include applicable range stats; live target retention follows source entry/death behavior; melee hit shapes use actual enemy areas; Lootworm attack area and cached movement target serve separate purposes. | Source comparison and bounded fixes in progress. |
| Weapons / item interactions | Ghost counters belong to individual weapon slots and reset each wave; critical-kill materials go directly to currency; explosive hits replace direct damage with an area hit and melee has a separately connected explosion callback; Taser must not apply its slow twice. | Source comparison, callback integration and regressions in progress. |
| Enemies / incoming projectiles | Additive charge/Pursuer speed is separate from finalized enemy speed and uses integer source pixels; Evil Mob overrides stat-speed modifiers; projectile contacts count before dodge/protection/healing, and disabled hurtboxes do not consume new contacts. | Root implementing and cross-checking with the source. |

## Important distinctions

- This pass includes registered base mechanics. Generic source hooks with no supplied resource registration are labeled separately; they do not establish a missing playable feature.
- Source Hurtbox disabling is deferred. The incoming-projectile adapter captures eligibility for a contact batch, preserves queued contacts, and excludes new contacts while the hurtbox is disabled.
- Source healing-turret projectiles are Projectile subclasses and reach the player's hurtbox, so they also enter the projectile-contact achievement check before healing. The medical gun's enemy-targeting projectile is distinct.
- The previously approved movement audio, artwork, item identities and controller changes are preserved. No listening, new physical-controller test or human full-run acceptance is claimed.
- Exact source-coordinate adapters must use the specific scene's collision/body/root relationships. Similar-looking art or a generic radius does not prove equal targeting or overlap.
- A proposed slow-recovery change from 3 to 5 world units per second was rejected by independent review: the source adds `60 * delta * 5` pixels, or 300 pixels/second = 3 world units/second. The original recovery rate is retained.

## Evidence and next validation

Domain reports: `shop-unlock-next-audit-2026-10-15.md`, `support-next-audit-2026-10-15.md`, `weapon-next-audit-2026-10-15.md`, and the root enemy findings recorded here. Agent-authored tests are pending until actual results are read. Focused checks may resolve integration risks; the full suite and another full-run comparison remain held until the remaining audits and confirmed fixes are finished.

Codex performs source inspection, implementation and automated checks. The user supplies direction, reference material and physical controller/audio observations. Do not attribute the implementation to the user or equate scripted/native fixtures with ordinary human gameplay.
