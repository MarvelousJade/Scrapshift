extends Node
# Isolated autoload only. Player methods are invoked without a rendered scene;
# no input, audio, screen capture, real-time timing or ordinary hit path is tested.
const STAT_KEYS = ["max_hp", "hp_regeneration", "lifesteal", "percent_damage", "melee_damage", "ranged_damage", "elemental_damage", "attack_speed", "crit_chance", "engineering", "range", "armor", "dodge", "speed", "luck", "harvesting"]

func _ready():
	get_tree().create_timer(60, true).connect("timeout", self, "watchdog")
	call_deferred("run_cases")

func watchdog():
	push_error("HEADLESS_DYNAMIC_ITEMS watchdog: callback failed or exceeded 60 seconds")
	get_tree().quit(5)

func source_item_id(id):
	return "retromations_hoodie" if id == "hoodie" else id

func run_cases():
	DebugService.disable_saving = true
	var input = File.new()
	var output = File.new()
	if input.open(OS.get_environment("SCRAPSHIFT_ORACLE_CASES"), File.READ) != OK or output.open(OS.get_environment("SCRAPSHIFT_ORACLE_OUTPUT"), File.WRITE) != OK:
		get_tree().quit(2)
		return
	var items = {}
	var characters = {}
	var weapons = {}
	for item in ItemService.items:
		items[item.my_id] = item
	for character in ItemService.characters:
		characters[character.my_id] = character
	for weapon in ItemService.weapons:
		weapons[weapon.my_id] = weapon
	var count = 0
	while not input.eof_reached():
		var line = input.get_line()
		if line.empty():
			continue
		var parsed = JSON.parse(line)
		if parsed.error != OK:
			push_error("HEADLESS_DYNAMIC_ITEMS invalid JSON")
			get_tree().quit(4)
			return
		var spec = parsed.result
		var weapon_key = "weapon_" + spec.starter + "_1"
		if not weapons.has(weapon_key):
			push_error("HEADLESS_DYNAMIC_ITEMS missing starter: " + weapon_key)
			get_tree().quit(4)
			return
		if not characters.has("character_" + spec.character):
			push_error("HEADLESS_DYNAMIC_ITEMS missing character: " + spec.character)
			get_tree().quit(4)
			return
		for id in spec.items:
			if not items.has("item_" + source_item_id(id)):
				push_error("HEADLESS_DYNAMIC_ITEMS missing item: " + id)
				get_tree().quit(4)
				return
		RunData.reset()
		RunData.add_character(characters["character_" + spec.character], 0)
		RunData.add_weapon(weapons[weapon_key], 0)
		RunData.add_starting_items_and_weapons()
		for id in spec.items:
			RunData.add_item(items["item_" + source_item_id(id)], 0)
		RunData.wave_in_progress = true
		RunData.reset_wave_caches()
		# No _ready()/scene tree for Player: only the invoked callback dependencies
		# are supplied. Timer nodes belong to this autoload, not the Player scene.
		var player = Player.new()
		var main = Main.new()
		main._players = [player]
		var moving_timer = Timer.new()
		var standing_timer = Timer.new()
		add_child(moving_timer)
		add_child(standing_timer)
		player._moving_timer = moving_timer
		player._not_moving_timer = standing_timer
		player.check_not_moving_stats(Vector2.ZERO)
		var snapshots = []
		for operation in spec.operations:
			match operation.kind:
				"tick":
					for _i in range(int(operation.value)):
						player._on_OneSecondTimer_timeout()
				"move":
					var movement = Vector2.RIGHT if operation.value > 0 else Vector2.ZERO
					player.check_not_moving_stats(movement)
					player.check_moving_stats(movement)
				"fruit":
					player.max_stats.health = int(Utils.get_capped_stat(Keys.stat_max_hp_hash, 0))
					player.current_stats.health = player.max_stats.health - int(operation.value)
					# Callback samples pre-healing HP. Consumable healing is external
					# to this state-stat oracle and is covered by survival fixtures.
					player.on_consumable_picked_up(load("res://items/consumables/fruit/fruit_data.tres"))
				"health":
					player.max_stats.health = int(Utils.get_capped_stat(Keys.stat_max_hp_hash, 0))
					var maximum = player.max_stats.health
					match int(operation.value):
						0: player.current_stats.health = maximum
						1: player.current_stats.health = int(floor(maximum / 2.0))
						2: player.current_stats.health = int(ceil(maximum / 2.0))
						3: player.current_stats.health = int(floor(maximum / 2.0)) - 1
					main.check_half_health_stats(0)
				"intermission":
					# Actual cleanup stat callbacks. Doctor grants 5 Harvesting;
					# its growth must be included in these lifecycle cases.
					TempStats.reset()
					main._on_HarvestingTimer_timeout()
					RunData.wave_in_progress = false
				"remove", "add":
					var item_key = "item_" + source_item_id(operation.key)
					if not items.has(item_key):
						push_error("HEADLESS_DYNAMIC_ITEMS missing operation item: " + item_key)
						get_tree().quit(4)
						return
					if operation.kind == "remove":
						RunData.remove_item(items[item_key], 0)
					else:
						RunData.add_item(items[item_key], 0)
				"next_wave":
					RunData.current_wave += 1
					RunData.wave_in_progress = true
					TempStats.reset()
					RunData.reset_wave_caches()
					player._one_second_timeouts = 0
					player.not_moving_bonuses_applied = false
					player.moving_bonuses_applied = false
					player.check_not_moving_stats(Vector2.ZERO)
				"end_lost":
					# Exact source reset from Player.die, rather than executing
					# scene/graphics cleanup on an unready Player dependency.
					player.current_stats.health = 0
					TempStats.reset_player(0)
				"end_won":
					# Staged final wave. Actual normal-end cleanup/growth and
					# end_run._ready stat-reset operations, not a full world run.
					RunData.current_wave = 20
					TempStats.reset()
					main._on_HarvestingTimer_timeout()
					RunData.wave_in_progress = false
					TempStats.reset()
			LinkedStats.reset_player(0)
			var values = []
			for key in STAT_KEYS:
				values.append(Utils.get_stat(Keys.generate_hash("stat_" + key), 0))
			snapshots.append(values)
		output.store_line(to_json({"id": spec.id, "stats": snapshots, "error": ""}))
		player.free()
		main.free()
		moving_timer.queue_free()
		standing_timer.queue_free()
		count += 1
	input.close()
	output.close()
	print("HEADLESS_DYNAMIC_ITEMS_REFERENCE_DONE ", count)
	get_tree().quit(0)
