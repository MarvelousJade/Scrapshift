"""Import the supplied game's crate, wave-letter, and run-ending sounds."""
import hashlib
import json
import shutil
import uuid
from pathlib import Path

import analyze_reference as source

project = Path(__file__).resolve().parents[1]
out = project / "Assets/Scrapshift/Content/Reference"
template = (out / "Install_01.wav.meta").read_text()
entries = [
    ("items/consumables/item_box/item_box_pickup.wav", "CratePickup_01.wav"),
    ("ui/sounds/CGM3_Bubble_Button_01_3.wav", "WaveLetter_01.wav"),
    ("ui/sounds/CGM3_Bubble_Button_01_4.wav", "WaveLetter_02.wav"),
    ("resources/sounds/Fear 10.mp3", "Win_01.mp3"),
]
entries += [
    (f"resources/sounds/Madness {number}.mp3", f"Lose_{index:02}.mp3")
    for index, number in enumerate((1, 2, 3, 4, 6, 7, 8, 9, 10), 1)
]

manifest = []
for relative, name in entries:
    original = source.ROOT / relative
    destination = out / name
    shutil.copyfile(original, destination)
    metadata = Path(str(destination) + ".meta")
    if not metadata.exists():
        metadata.write_text(
            template.replace("guid: 56b696bf534e60347b1b6b16d8feec5d", f"guid: {uuid.uuid4().hex}")
            .replace("forceToMono: 1", "forceToMono: 0")
            .replace("normalize: 1", "normalize: 0")
            .replace("userData: ", "userData:")
            .replace("assetBundleName: ", "assetBundleName:")
            .replace("assetBundleVariant: ", "assetBundleVariant:")
        )
    manifest.append({"source": relative, "asset": str(destination.relative_to(project)).replace("\\", "/"),
                     "sha256": hashlib.sha256(original.read_bytes()).hexdigest()})

# The reference wave cue was already imported, but the older Unity importer
# collapsed its original stereo file to mono.
wave_metadata = out / "WaveClear_01.wav.meta"
wave_metadata.write_text(wave_metadata.read_text().replace("forceToMono: 1", "forceToMono: 0")
                         .replace("normalize: 1", "normalize: 0"))
wave = out / "WaveClear_01.wav"
original_wave = source.ROOT / "ui/sounds/end_wave.wav"
if wave.read_bytes() != original_wave.read_bytes():
    raise ValueError("Existing end-wave cue differs from the supplied original")
manifest.append({"source": "ui/sounds/end_wave.wav", "asset": str(wave.relative_to(project)).replace("\\", "/"),
                 "sha256": hashlib.sha256(wave.read_bytes()).hexdigest()})
report = project / "artifacts/audio-parity-provenance.json"
report.write_text(json.dumps(manifest, indent=2) + "\n")
print(f"Imported {len(entries)} sounds; verified original wave cue")
