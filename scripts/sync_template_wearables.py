"""Copy validated Unity import metadata and only the four appearance bindings."""
from pathlib import Path
import re
import shutil
import json
import sys

root = Path(__file__).resolve().parents[1]
clone = root.parents[2] / 'tmp' / 'ScrapshiftParity'
ids = {'beanie', 'leather_vest', 'glasses', 'coffee', 'blindfold'}
all_wearables = '--all-wearables' in sys.argv
if all_wearables:
    manifest=json.loads((root/'Assets/Scrapshift/Content/OriginalArt/FittedWearables/manifest.json').read_text())
    ids.update(e['id'] for sheet in manifest['sheets'] for e in sheet['entries'])
relative = Path('Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset')
source = (clone / relative).read_text()
target = (root / relative).read_text()
field_names = ('wearSlot', 'wearOffset', 'wearScale', 'wearOrder', 'legacyAppearance', 'appearances')

def item_blocks(text):
    section = text.split('    items:\n', 1)[1].split('    characters:\n', 1)[0]
    result = {}
    for block in re.split(r'(?=^    - )', section, flags=re.M):
        match = re.search(r'^      id: (.+)$', block, re.M)
        if match and match[1] in ids:
            result[match[1]] = block
    return result

validated = item_blocks(source)
old = item_blocks(target)
assert validated.keys() == old.keys() == ids
for item_id in ids:
    updated = old[item_id]
    for field in field_names:
        pattern = rf'^      {field}:.*?(?=^      [A-Za-z]|\Z)'
        value = re.search(pattern, validated[item_id], re.M | re.S)
        assert value, (item_id, field)
        updated, count = re.subn(pattern, lambda _: value[0], updated, flags=re.M | re.S)
        assert count == 1, (item_id, field, count)
    target = target.replace(old[item_id], updated, 1)
(root / relative).write_text(target)

folder = Path('Assets/Scrapshift/Content/OriginalArt/TemplateWearables')
for meta in (clone / folder).glob('*.meta'):
    shutil.copy2(meta, root / folder / meta.name)
for relative in (Path(str(folder)+'.meta'), Path('Assets/Scrapshift/Editor/TemplateWearableBuilder.cs.meta')):
    shutil.copy2(clone / relative, root / relative)
if all_wearables:
    folder=Path('Assets/Scrapshift/Content/OriginalArt/FittedWearables')
    for meta in (clone/folder).glob('*.meta'):
        shutil.copy2(meta,root/folder/meta.name)
print(f'Synchronized {len(ids)} appearance bindings and Unity metadata; other item fields untouched.')
