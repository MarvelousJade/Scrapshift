# Menu proportions, preview motion, music and HUD spacing

The title PNG is 1672 x 941. Its texture importer previously used ToNearest power-of-two resizing, which could distort this almost-16:9 illustration to 2048 x 1024 before aspect-preserving display. Disabled NPOT resizing. The title continues to fill the screen with a proportional center crop on other aspect ratios; GUI coordinates already use uniform scaling.

Character-selection preview legs were explicitly fixed while the body squashed. They now spread and flex subtly with the existing idle spring; Reduced Motion keeps the resting pose. Gameplay locomotion is unchanged.

Music already called Play at initialization, so the reported silence was not an intentional first-wave gate. Removed the quieter menu gain and silent startup volume, and added recovery when a loaded looping clip stops. Music still respects Music/Master volume, focus muting and fixture muting. Its first second contains audible signal (RMS 0.1551), not a silent intro. This does not replace the composition.

Moved money and harvesting down 18 canvas units together, preserving their separation and icon/text alignment. Updated the money counter pulse, gain text and end-wave collection target to the same new location.

Validation: Unity build succeeded with zero errors/warnings; all 288 EditMode tests passed. A fresh 79-stage rendered review covers the title, resting/compressed preview and new HUD spacing. Screenshot fixtures mute audio and do not establish audible output on the user device.

Character selection now uses the reference three-panel composition (character details, Records, Run Options), a 225:250 portrait card, and 17 columns with up to three rows for the base roster. The mascot occupies less of the preview to match the reference. Records reads the existing per-character win achievement; older saves do not contain best-wave or per-character danger records, so those are not invented. Run Options shows the supported single-player 20-wave mode; no nonfunctional endless/co-op switches were added. Map selection remains in Video settings only.
