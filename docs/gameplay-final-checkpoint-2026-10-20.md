# Final headless gameplay checkpoint - October 20, 2026

## Current result

**R29**, branch `codex/gameplay-parity`, production source checkpoint `1ff6bb1`. Launch later: `Builds/GameplayParity20261020R29/LaunchGame.cmd`; keep its entire unpacked folder together. No visible game or fresh physical input is claimed.

| Check | Actual result |
| --- | --- |
| Complete editor suite | **1,200/1,200 Passed**, zero failures/skips, exit0;69.7817621 seconds (`suite-attempt-4.xml`) |
| Character/single-item oracle | **10,500/10,500** static16-stat vectors match the original; zero export errors/mismatches |
| Neutral item-pair oracle | **22,155/22,155** baseline/single/distinct-pair/double vectors matched in the recorded full-health intermission scope |
| Windows build | Succeeded, zero build errors/warnings;401761114 bytes |
| Input/package hashes |596 source/settings/import inputs and159 player files verified before and after campaigns; no mismatches; catalog matches isolated clone after line-ending normalization |
| Latest full campaign | R29 **Passed/Won**,20 ordered waves,19 shops, exit0; normal60Hz combat/pickup timing |
| Reference comparison | Fresh supplied-original run also Won/20 waves/19 shops/exit0; **18/18** unchanged comparator checks pass against R29 |
| Ordinary-damage R29 campaign | Passed/exit0; natural defeat during wave8 after7 completed waves, no injected invulnerability; cleanup acknowledgment age0.500002 seconds |

## Meaningful corrections

- Character stat gains apply to linked bonuses. Pet stat links count registered effects rather than live actors.
- Beast Master chooses one starting pet and separately receives Lootworm, rather than all four alternatives. UI preview/startup retain the selected identity.
- Wrench, Screwdriver and Pruner register structure effects. Corrected character bonuses, two structure unlock conditions, turret deployment unlock and grouped spawn rules. Source tracing disproved an older test assumption; historical documents now link the correction.
- Lethal periodic damage uses common defeat cleanup. Loss acknowledgment precedes pending unlock rewards; retry retains the reward queue.
- Optional optimized cleanup follows source per-pile material boosting. Shop weapon modals preserve inherited disabled controls.

Implementation and focused evidence are in [items](items-parity-remaining-2026-10-20.md), [starting choices](starting-item-choices-2026-10-20.md), [support registration](support-registration-parity-2026-10-20.md), [survival](survival-parity-remaining-2026-10-20.md), [cleanup](optimized-cleanup-parity-2026-10-20.md) and [shop](shop-parity-remaining-2026-10-20.md). Every logical correction was committed separately with related checks/docs; this checkpoint records final integrated evidence.

## Full campaign receipts

Evidence root: `artifacts/parity-remaining-2026-10-20`. R29 output is `unity-full-headless-r29`; original output is `original-full-headless`. Comparator JSON/Markdown are `headless-economy-comparison-r29.*`; exact input hashes are included. The unchanged generator retains an October17 title in its generated Markdown; these input/process records establish the actual October20 attempt.

All31 reference purchase debits and40 R29 purchase debits reconcile. R29 ends with45 item copies across37 IDs; reference32 copies across28 IDs. Snapshot stack sums/caps, known IDs and fresh-run unlock eligibility pass. These quantities need not equal across engine-specific random runs and do not prove each acquired effect.

The explicit R29 final receipt credits **96 materials**,1940 to2036, using Harvesting96 before it grows to101. No materials remain on the ground in the final trace. Active combat totals1050.199951 seconds; reported wall time1093.774536 seconds, with43.342981 seconds of cleanup/choices and summed combat-window excess-0.884296 seconds. Small negative excess reflects callback sampling. This is not a graphics/FPS benchmark.

Both comparison fixtures deliberately use neutral character effects, six supplied tier-II weapons, Danger0, scripted movement, invulnerability, mute and isolated fresh progression. The separate ordinary-damage run uses the same staged multiweapon basis and accepts a genuine natural defeat after cleanup; it is not a completed human20-wave survival run. See [survival comparison](headless-survival-comparison-2026-10-20.md).

R29 player/stderr logs have no error/exception/assertion/warning matches in the final scan. The original is not diagnostic-free:35 missing-log-path assertions, other preserved shutdown/resource/Attach warnings and one scripted startup pause intervention remain disclosed in [reference evidence](headless-full-comparison-2026-10-20.md). Its selected-source provenance is not a complete tree-equality claim. The separate stat oracle audited3,816 source/resource files and recorded12 isolated harness/path differences.

## Remaining evidence

The user paused desktop control. Current native selection/support presentation, physical Xbox/ordinary focus and browser checks wait for explicit authorization. Static matrices do not establish timed/proc combinations, all acquisition orders, every item pair under every character or human balance.109 unlock definitions still lack individual native threshold acceptance in the recorded inventory; all115 have model producer/persistence cases. The reference's first-wave aggregate snapshot leaves one material unexplained; the trace does not establish a causal payout defect.

Use the [consolidated status](gameplay-verification-status.md) and [finite remaining acceptance checklist](remaining-native-acceptance-2026-10-20.md). No additional confirmed implementation mismatch remains unresolved from this pass; those coverage gaps are not represented as verified exact parity.

Codex agents performed source investigation, implementation, tests and scripted runs. The user supplied direction/reference and earlier actual controller feedback. Interview notes distinguish that contribution from work the user has personally reproduced.
