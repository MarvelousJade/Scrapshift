"""Author body-origin registrations without editing any source PNG pixels.

The four approved full-canvas garments remain separate. These explicit corrections
are based on the six native item gallery pages, not the original game's art bounds.
Output is a checked-in art registration table, consumed by the Unity importer.
"""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
folder = root / 'Assets/Scrapshift/Content/OriginalArt/FittedWearables'
source = json.loads((folder / 'manifest.json').read_text())
approved = {'beanie', 'leather_vest', 'glasses', 'coffee', 'blindfold'}
# key: width, height, centerX, centerY. Deliberately retain good silhouettes.
corrections = {
    'adrenaline_0': (.24,.38,-.38,.40),
    'baby_elephant_0': (.24,.18,.20,-.12),
    'bandana_0': (1.02,.52,0,.38),
    'banner_0': (1.12,1.25,0,.06),
    'boxing_glove_0': (.44,.40,.42,.37),
    'broken_hourglass_0': (.42,.50,0,.78),
    'campfire_0': (.54,.44,0,.76),
    'candle_0': (.28,.49,0,.77),
    'clockwork_wasp_0': (.56,.42,0,.76),
    'coil_0': (.55,.35,0,.70),
    'community_support_0': (1.14,.55,-.38,.12),
    'crown_0': (.88,.52,0,.44),
    'cyclops_worm_0': (.72,.36,0,.11),
    'defective_steroids_0': (.42,.49,.33,.61),
    'duct_tape_0': (.28,.17,.29,-.28),
    'fin_0': (.50,.48,0,.73),
    'focus_0': (.28,.24,.19,.27),
    'gentle_alien_0': (.55,.80,-.25,.63),
    'glass_cannon_0': (.46,.41,.36,.49),
    'gnome_0': (1.03,.69,0,.47),
    'goat_skull_0': (.36,.55,-.34,.44),
    'goat_skull_1': (.36,.55,.34,.44),
    'hourglass_0': (.47,.43,0,.72),
    'hunting_trophy_0': (.50,.79,-.48,.47),
    'hunting_trophy_1': (.50,.79,.48,.47),
    'improved_tools_0': (.73,.33,0,-.42),
    'jelly_0': (.55,.44,0,.75),
    'little_frog_0': (.62,.38,0,.69),
    'little_muscley_dude_0': (.43,.40,-.18,.48),
    'metal_detector_0': (.44,.64,.36,.54),
    'mutation_0': (.27,.47,.37,.40),
    'octopus_0': (.37,.44,.47,-.15),
    'pencil_0': (.22,.36,-.33,.35),
    'piggy_bank_0': (.27,.17,.12,-.20),
    'plant_0': (.55,.52,0,.76),
    'power_generator_0': (.71,.50,.19,.45),
    'recycling_machine_0': (.36,.29,-.32,.44),
    'robot_arm_0': (.62,.58,.56,-.16),
    'scope_0': (.59,.35,0,.70),
    'sifds_relic_0': (.78,.37,0,.17),
    'snake_0': (.30,.40,.40,.33),
    'spicy_sauce_0': (.18,.32,-.31,.30),
    'strange_book_0': (.70,.31,0,.11),
    'tentacle_0': (.40,.53,.48,-.16),
    'torture_0': (.30,.46,-.43,-.13),
    'torture_1': (.30,.46,.43,-.13),
    'toxic_sludge_0': (.31,.48,-.25,.41),
    'triangle_of_power_0': (.27,.29,0,.29),
    'white_flag_0': (.48,.53,0,.76),
    'dynamite_0': (.45,.36,0,.70),
    'book_0': (.99,.56,0,.43),
    'evil_hat_0': (1.07,.57,0,.44),
    'pile_of_books_0': (1.02,.57,0,.45),
    'snowball_0': (.82,.44,0,.40),
    'tree_0': (1.04,.62,0,.44),
    'warrior_helmet_0': (1.07,.62,0,.40),
    'wolf_helmet_0': (1.07,.62,0,.40),
    'hedgehog_0': (1.07,.57,0,.43),
    'propeller_hat_0': (1.07,.62,0,.44),
    'bowler_hat_0': (.90,.56,0,.43),
    'big_arms_0': (.52,.65,.46,-.12),
    'claw_tree_0': (.28,.36,.43,-.35),
    'cute_monkey_0': (.34,.30,-.45,-.13),
    'cute_monkey_1': (.34,.30,.45,-.13),
}
entries=[]
for sheet in source['sheets']:
    for e in sheet['entries']:
        if e['id'] in approved:
            continue
        values=corrections.get(e['key'],(e['width'],e['height'],e['offsetX'],e['offsetY']))
        entries.append(dict(key=e['key'],width=values[0],height=values[1],centerX=values[2],centerY=values[3],
                            reviewed=True,changed=e['key'] in corrections))
assert len(entries)==180
assert set(corrections)<=set(e['key'] for e in entries)
(folder/'registrations.json').write_text(json.dumps(dict(entries=entries),indent=2)+'\n')
print(f'{len(entries)} reviewed registrations; {len(corrections)} individual fit corrections; five full-canvas wearables preserved.')
