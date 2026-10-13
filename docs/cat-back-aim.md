# Catling back mounts and aim

Reference inspected: E:/UnityProjects/brotato-(3.x)/entities/units/pet/catling_gun/catling_gun.gd and catling_gun.tscn.

The reference scene puts Gun_pivot_l and Gun_pivot_r under the animated sp_body. Each has a gun sprite and muzzle. _orient_gun runs for both guns every physics tick, compensates for mirrored body facing, flips the gun vertically when aiming behind, and changes its drawing order relative to the body.

Scrapshift previously kept the mounts beside the body rather than under it, used a centered gun sprite with a large positional offset, and always hid the rear gun behind the body. The corrected rig puts both pivots over the back as children of the animated body, rotates around the sprite's rear joint, and swaps front/back drawing order with aim direction. Muzzles follow the visible gun transform, including recoil.

Validation: all 21 SupportParityTests passed, including a new check covering both guns, all four cardinal directions, both body facings, nonuniform body scale, body sway, mount stability during recoil, and drawing order.

Native review: artifacts/cat-back-aim/review.html.
Build: Builds/CatBackAim/Scrapshift.exe.
