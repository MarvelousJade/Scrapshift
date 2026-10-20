# Death dispatch and current-health item interactions — October 20

## Confirmed source difference

R34 reserves the selected target and vulnerability when Lich, Baby Elephant or Cyberball requests damage, but applies the hit at the end of the combat step. The original applies `Unit.take_damage` synchronously inside `Main.handle_stat_damages`. Capturing defense context alone does not preserve current health, pending-death state or kill ownership.

Two reachable imported interactions distinguish the implementations:

1. A Lich healing event targets an enemy with 10 HP. The original immediately reduces it to zero and sets `_pending_die`; a later weapon hit returns zero damage. Delaying Lich lets that weapon claim the kill first, and then discards the reserved Lich hit.
2. Lich damages a 100 HP enemy before a critical weapon hit with Giant Belt. The original computes the Belt bonus from 90 HP: `100 - 10 - (20 + floor(90 × 10%)) = 61`. Delayed Lich computes the bonus from 100 HP and ends at 60. Neither case needs arbitrary synthetic effect definitions.

The exact event order is staged. These cases establish behavior when those callbacks occur in that order; they do not establish how often the order occurs in ordinary physics.

## Source basis

- Original `main.gd:1556`: `handle_stat_damages` selects a target, aggregates effects and calls its `take_damage` before returning.
- Original `entities/units/unit/unit.gd:262`: `take_damage` rejects `_pending_die` targets before applying damage.
- Original `entities/units/unit/unit.gd:309`: health changes during that call.
- Original `entities/units/unit/unit.gd:352`: death is deferred, and `_pending_die` is set synchronously. Rendering and death signals happen later.
- Original `entities/units/enemies/enemy.gd:285`: Giant Belt uses current health when resolving a critical hit.
- Original imported `items/characters/lich/lich_effect_0c.tres` supplies the 100% Max HP healing-triggered hit; `items/all/giant_belt/giant_belt_effect_1.tres` supplies 10% current HP on critical hits.

## Focused acceptance

`scripts/death_dispatch_reference.gd` calls actual Main/Enemy/Unit methods in an isolated reference clone. It suppresses the rendered flash and replaces the terminal `die` body with a counting sink, while retaining the actual deferred call and pending-death guard. It therefore checks hit timing and deferred invocation, not complete scene death signals, actor rendering or physics.

`DeathDispatchFollowupTests` checks immediate HP, Giant Belt's later current-health calculation, rejection of a later hit after lethal Lich, and stable list membership before deferred finalization. Stable membership is required because the production weapon/enemy loops hold integer indices; applying procs synchronously while removing unrelated targets immediately can invalidate those indices.

An additional imported mixture covers one material pickup with Cute Monkey, Lich and Baby Elephant. Successful Monkey healing makes Lich kill the only target before Elephant selects it; Elephant then aborts before consuming its probability roll. The explicit finalization check drains the death boundary twice and requires exactly one kill/corpse, avoiding duplicate payouts.

## Implementation under validation

`CaptureImplantDamageTarget` now submits the selected target's hit synchronously. `DamageWithCapturedVulnerability` rejects zero-HP pending victims, clamps HP at zero like the original, and queues death finalization before later critical/healing/on-hit callbacks. `ResolveEnemyDeaths` drains that queue at an explicit safe boundary; the active enemy list remains stable during indexed traversal. Kill counts, corpse transfer, item death callbacks, drops and spawned children remain in the deferred finalizer. The private reflected damage wrappers remain available.

The queue is cleared with combat state. There is no automatic test-only flush based on component activation: focused callers must explicitly invoke the same boundary used by the runtime.

`ResolveCombatCallbacks` drains pending deaths and newly requested implant attacks until stable. The zero-HP guard prevents duplicate deaths; spawned enemy children remain queued births rather than expanding the active target set inside the drain. `UpdateEnemies` uses a reverse identity snapshot so a player-lethal boundary can flush older deaths without invalidating integer indices.

The death finalizer separates Cyberball damage, Beard/explosion creation, burning-item stat gains, loot, and progression counts in Main's source order. The existing public `KillEffects` API remains available for model callers. Corpse membership is being validated directly through Godot signal dispatch; pool reuse is not assumed to determine that order.

### Separate player-death ordering risk

Player also inherits Unit's deferred `die`. Enemy death queued before player death should dispatch first, while a player-death cleanup dispatched first suppresses later pending enemy death rewards. Immediate `RunRules.Hurt` terminal transition and a later enemy-only drain cannot by themselves establish that shared ordering. The source probe includes two narrow Unit FIFO receipts for both enqueue orders, but its counting sink does not execute complete Player/Main cleanup. This case remains open until a source-backed combined boundary check covers it.

The implementation now installs `BeforeDeathCleanup` in ArenaGame, draining earlier enemy requests while the phase is still Playing. Two added Editor cases bind this production hook explicitly: enemy-lethal first must award exactly one enemy kill before Lost; player-lethal first must reject a subsequent enemy hit without awarding a kill. These are source-derived cleanup-order checks, not full-scene Godot Player/Main execution. Their execution result remains pending until the combined run is inspected.

The Will-o'-the-Wisp achievement counter is distinct from the item's burn-kill stat gain. Original `unit.gd:698–700` advances the achievement synchronously after a lethal BurningTimer hit sets `_pending_die`; the item's burning-death stat gain runs later in Main's death callback. Moving both to deferred finalization would erase that distinction.

Execution receipts and integration results are pending. No passing result is claimed here yet.

## Attribution and remaining limits

The user requested remaining death-timing and item-interaction checks without desktop control. Codex performed this source investigation and authored the probe/tests. The supplied original is read-only; probes run in isolated copies. This is a useful interview example only after the user traces the ordering, explains why the queue was insufficient, and reproduces the focused check. Broader arbitrary combinations, ordinary physics scheduling, visual death animations and human play remain separate coverage.
