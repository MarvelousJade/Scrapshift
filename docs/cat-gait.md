# Catling gait and fixed mounting

October 13, 2026. Reference inspected: catling_gun.tscn, move animation (lines 626 onward), in E:/UnityProjects/brotato-(3.x). Its tracks animate the body, head, ears, tail and gun offsets, not separate alternating legs.

The requested alternating steps are a Scrapshift addition: a subdivided rendering mesh deforms the lower paw regions with opposite near/far phases and fixed upper attachment points. UVs remain fixed so the existing drawn legs bend continuously, without duplicated limbs or detached sprites. Idle, downed and reduced-motion poses disable the stepping.

Gun length increased from 0.29 to 0.36 world units (24%). Fixed outlined mounts connect the body to each rotating gun joint. Gun pivots remain attached to the body and inherit sway. Their aiming and direction-dependent layering remain intact.

22 support tests passed. Native captures include both step extremes and four gun directions. Use Builds/CatGait/LaunchSupportPreview.cmd for the updated playable scene.
