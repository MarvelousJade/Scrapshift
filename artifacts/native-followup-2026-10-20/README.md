# R36-R40 evidence

Final checkpoint: R40 source ede4094; use docs/gameplay-verification-status.md and docs/full-run-r40-comparison-2026-10-21.md.

The five largest raw JSON/JSONL traces remain unchanged in the working folder. Git stores deterministic gzip copies with SHA-256 records in compressed-evidence.json. Run restore_large_evidence.py with Python to restore them in another checkout; existing files are only verified, never overwritten. Capture/report hashes refer to the original decompressed bytes.

Generated engine userdata/profile caches are not committed. The fresh source summary preserves its actual final debug inventory in original-safe-full/reference-run-data.log; use that file with summarize_original_campaign.py --debug-log when reproducing the summary outside this machine. Engine diagnostics, failed attempts and the metadata-only first comparator failure are preserved.

Builds are separate, unpacked local Windows folders. R40 replaces the older reviews; R39 was not verified/shared because its input gate caught a late fixture-counter edit. These archives are test evidence, not a zipped game distribution.
