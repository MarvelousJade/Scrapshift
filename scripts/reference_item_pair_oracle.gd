extends Node
# Copy into an isolated reference project and register as an autoload.
# Set SCRAPSHIFT_ORACLE_CASES and SCRAPSHIFT_ORACLE_OUTPUT to absolute paths.
# Never add this fixture to the supplied read-only reference checkout.

const STAT_KEYS = ["max_hp", "hp_regeneration", "lifesteal", "percent_damage", "melee_damage", "ranged_damage", "elemental_damage", "attack_speed", "crit_chance", "engineering", "range", "armor", "dodge", "speed", "luck", "harvesting"]

func _ready():
	call_deferred("run_cases")

func run_cases():
	var input_path = OS.get_environment("SCRAPSHIFT_ORACLE_CASES")
	var output_path = OS.get_environment("SCRAPSHIFT_ORACLE_OUTPUT")
	var input = File.new()
	var output = File.new()
	if input_path.empty() or output_path.empty() or input.open(input_path, File.READ) != OK or output.open(output_path, File.WRITE) != OK:
		push_error("Item oracle requires readable SCRAPSHIFT_ORACLE_CASES and writable SCRAPSHIFT_ORACLE_OUTPUT")
		get_tree().quit(2)
		return
	DebugService.disable_saving = true
	var character = ItemService.get_element_safe(ItemService.characters, "character_well_rounded")
	var pistol = load("res://weapons/ranged/pistol/1/pistol_data.tres")
	if character == null or pistol == null:
		push_error("Missing original Well-Rounded character or tier-I pistol")
		get_tree().quit(3)
		return
	var index = {}
	for item in ItemService.items:
		index[item.my_id] = item
	var count = 0
	while not input.eof_reached() and count < 23000:
		var line = input.get_line()
		if line.empty():
			continue
		var parsed = JSON.parse(line)
		if parsed.error != OK:
			push_error("Bad case JSON line " + str(count + 1))
			get_tree().quit(4)
			return
		var spec = parsed.result
		var row = {"id": spec.id, "stats": [], "error": "", "reference_item_count": 0}
		RunData.reset()
		RunData.add_character(character, 0)
		RunData.add_weapon(pistol, 0)
		for id in spec.items:
			var key = "item_" + id
			if not index.has(key):
				row.error = "missing reference item " + key
				break
			RunData.add_item(index[key], 0)
		if row.error.empty():
			for key in STAT_KEYS:
				row.stats.append(Utils.get_stat(Keys.generate_hash("stat_" + key), 0))
			row.reference_item_count = RunData.get_player_items(0).size()
		output.store_line(to_json(row))
		count += 1
		if count % 1000 == 0:
			print("ITEM_ORACLE_REFERENCE ", count)
	input.close()
	output.close()
	print("ITEM_ORACLE_REFERENCE_DONE ", count)
	get_tree().quit(0)
