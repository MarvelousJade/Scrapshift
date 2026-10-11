# First hands-on playtest

This is a checklist for the user to complete; it is not marked passed in advance.

## Before play

Launch the Windows build, read the start screen and try WASD/arrow keys. Verify the objective is understandable without looking at this document.

## One run

- Collect green materials instead of only waiting for enemies; red/ivory repair capsules restore health.
- Acquire ranged and melee weapons from the shop; compare their range, attack rate and knockback.
- Try one fire-rate/speed upgrade and one repair upgrade.
- Pause during a busy moment. Toggle sound and reduced motion.
- Observe the wave-clear reset, choose queued stat upgrades, then use the separate shop.
- Play to a win or loss; restart and check that the previous upgrades are gone.

## Record useful feedback

| Question | Your observation |
| --- | --- |
| Which part was immediately clear? | Pending |
| What was confusing? | Pending |
| Which enemy was hardest to read? | Pending |
| Did an upgrade feel obviously better than every alternative? | Pending |
| Did the sound or movement feel distracting? | Pending |
| Did you encounter a bug? Exact steps and expected/actual result? | Pending |
| One change you want to make and why? | Pending |

Keep the first tuning change small. Record its original value, intended effect, changed value, playtest result and commit. This will create a stronger personal interview story than memorizing an implementation explanation.

## Current reference-rebuild playtest

Earlier five-wave checklist entries are historical. For this version:

1. Play the 20-second first wave; verify XP increases without interrupting combat, red X warnings precede enemies, and corpse animation does not deal damage.
2. Choose any queued stat rewards, then inspect the four shop offers. Buy a weapon or item when affordable; note its cost and the resulting inventory/stats.
3. Lock an offer, reroll with R and verify it remains. Select a weapon slot to combine a matching duplicate or recycle it. Check the six-slot rule.
4. Start the next wave with Enter. Listen for distinct pistol/SMG/shotgun/melee sounds after acquiring them. Pause to adjust volume or reduced motion.
5. Record the first unfair spawn, unreadable enemy, bad sound, confusing shop action or dominant build, including wave, weapon tiers and HP. Full-run difficulty and subjective sound remain user-feedback tasks, not established results.


## Enemy and UI follow-up

Compare the small chasing drones, charge warning/dash/recovery, and ranged warning/projectile colors. Sidestep after a direction cue appears; record any enemy that seems to turn after committing. Check that clustered red Xs stay coherent and never activate on the player without a fresh warning. Compare normal/reduced-motion cues. In the right-stats shop, buy one of the eight available item types and verify both displayed tradeoffs and the wardrobe preview. Record wave/loadout/position for confusing encounters. The automated twenty-wave fixture used invulnerability, so a normal human run remains useful evidence.


## After the October 11 source-parity changes

Personal checks still to do (not recorded as completed):

- Start with 10 HP, level 0. Collect 16 XP: level 1, +1 Max HP, and a queued reward at wave end.
- Observe a charger: direction commits during preparation; normal movement resumes immediately after its dash. Strafe during a spitter's preparation: its shot aims at your release-time position.
- Compare looter/bruiser wandering, spawner movement around the player, and the pursuer's one-second speed steps.
- Reroll an upgrade: four different stats should replace the previous four. Check level 5's guaranteed uncommon tier.
- Lock a shop offer, advance a wave, and verify its displayed purchase price is unchanged. First two shops should contain two weapons, including locked weapons.
- Play without fixture overrides through later waves and Predator's half-health/45-second mutation. Record deaths, loadout, purchases and fairness/readability issues before claiming the 20-wave balance is good.

## Weapon-set manual checks still to perform

- Hover each set row and a weapon's membership label; verify all five thresholds and the current highlight at different window sizes.
- Equip duplicates and different tiers; each slot should count once. Two Guns give +10 Range, two Knives +3% Critical Chance, two Spears +3 Max HP.
- Combine/recycle and verify counts and stats change immediately. Buying a matching tier at six slots should upgrade that slot without reducing its set count.
- Buy a permanent stat item while its matching set is active, then remove the set; only the temporary bonus should disappear.

Automated rules and staged rendering passed. These hands-on checks remain a user playtest checklist, not recorded manual results.

## Loot and healing — manual acceptance still needed

- Pursue a loot alien and break trees. Identify the brown strapped crate separately from green materials and the existing repair pickup.
- At full HP, pass near a repair/crate without touching it; then take damage and verify nearby consumables attract. Direct contact can collect at full HP.
- Leave a crate on the ground until wave end. Verify its item choice appears before levels, with the proper stats/tradeoffs.
- Test Enter/mouse Take and X/mouse Recycle, including two queued crates. One key press must not skip the shop or start the next wave. Confirm worn item appearance after taking.
- Choose regeneration and observe timed whole-HP ticks. Pause must stop the timer.
- Play the first wave normally and assess difficulty. The latest scripted movement smoke died before clearing it; the invulnerable twenty-wave fixture does not settle balance.
