# Reversible Web audio size experiment

`WebBuild.Build` supports a baseline `--web-original-audio` option and a separate
`--web-output` folder. The default experiment temporarily sets included SFX import
quality to0.65, retaining load type, sample rate, channels, preload and source
files. The four canonical preview assets are excluded. Music retains the earlier
0.45/compressed-in-memory policy. This does not promise decoded runtime RAM savings.

Preparation and restoration are inside try/finally, including preparation failures.
Every importer has full before/during/after settings and source SHA256 receipts;
restoration failure fails the operation. Detailed packed asset sizes and explicit
failed-build receipts prevent a stale successful output from being reported.

Unity6000.6 compilation passed after correcting summary count fields to `int`.
The combined1309-test suite passed. Actual A/B builds, restoration receipts and
browser validation are pending; this experiment is not yet a release acceptance.
