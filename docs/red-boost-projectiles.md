# Red-eyed boost and turret projectile presentation

Catling uses CatBodyBoost during proximity boost, including reduced-motion mode, and restores CatBodyV2 afterward. Body transform and gun mounts are unchanged. The 0.2-second entry and body pulse remain. This is a custom red-eyed expression, not the original separate head/ear animation.

SupportProjectileArt uses unmodified reference PNGs from local `E:/UnityProjects/brotato-(3.x)/projectiles`: bullet/frame0000, rocket/rocket, blue_flames/sprites/bullet_flame_0000..0007, bullet_laser/bullet_laser_0000..0001, bullet_medical/bullet_medical_0000..0001. Flame frames advance every 0.05s; laser/medical switch at 0.02s. Simulation update advances visuals, so pausing freezes them. Damage, range and firing intervals are unchanged by this presentation update.

## Image generation
Mode: edit, transparent background. Final asset: Assets/Scrapshift/Resources/CatBodyBoost.png.
First edit of CatBodyV2: angry narrowed golden eyes and small open hiss, preserving pose and body. Second edit prompt: “Edit this exact transparent cat game sprite. Change ONLY both golden irises to vivid glowing red irises with a small soft red glow immediately around the eyes. Keep black pupils, angry eyelids and clear facial readability. Preserve absolutely everything else: identical body, silhouette, pose, paws, ears, tail, colors, placement, canvas dimensions and transparency. No added background or objects. This is the boosted rapid-fire state.”
Generated source: exec-2746b948-f981-4c4f-bb71-daa8f5828b08.png. The boost rectangle includes extra room for the expression while preserving the original pivot and pixel scale.

## Validation
30 focused EditMode tests passed, including face restore, reduced-motion visibility, unchanged body transform, and distinct projectile frame sequences. Native player capture/check results: ../artifacts/red-boost/pets.json. Review: ../artifacts/red-boost/review.html. Latest playable level: Builds/RedBoost/LaunchSupportPreview.cmd.
