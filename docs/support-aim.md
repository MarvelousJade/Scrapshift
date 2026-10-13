# Fixed Catling platform and visible turret aiming

Catling now uses a fixed platform and two mounting posts attached to the animated body. Only the upper Gatling assembly rotates around its rear circular joint. Recoil compresses the barrel direction slightly without sliding the joint away from its mount. Existing alternating attached paws remain.

Standalone turret heads now visibly turn toward their target before shooting; the tripods stay fixed. This is an intentional visual change requested by the user, superseding the upright-head presentation documented in the earlier support audit. Simulation targeting and attack timing remain unchanged.

## Artwork

Built-in imagegen edit mode. Saved asset: `Assets/Scrapshift/Resources/PetGatlingUpper.png`.

Prompt: Remove ONLY the bottom pedestal: remove the horizontal oval foot and the short vertical stalk underneath the circular rear side joint. Keep the large circular rear side joint itself, the cream receiver, amber light, and all barrels exactly as they are. The result is only the rotating upper gun assembly, no stand, no foot, no support underneath. Preserve the chunky black outlines, colors, horizontal right-pointing orientation and transparent alpha. No new objects, shadows, labels or background. This is a game rig part that will rotate around the center of the rear circular side joint.

## Validation

26 Unity support tests passed. Native renders cover four aiming directions for Catling and turret, plus both Catling step extremes. Native behavior and motion results are linked from the review page.
