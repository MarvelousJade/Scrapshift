# Cursor parity

The cursor images are copied byte-for-byte from the user-supplied `E:/UnityProjects/Brotato/ui/custom_cursor.png` and `ui/manual_cursor.png`. Their original cursor manager uses hotspots `(3, 3)` for the 32×32 menu arrow and `(35, 35)` for the 70×70 manual-aim reticle.

Scrapshift loads these PNG bytes into readable RGBA32 textures at runtime. Menus, pause and intermission use the arrow. Normal combat hides the cursor. Mouse aiming shows the reticle; mouse-only movement and the mouse-press aiming option keep a visible pointer between aiming presses. The cursor resets to the system default when the game component is disabled.
