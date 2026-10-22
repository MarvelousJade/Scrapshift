extends Node
# Isolated autoload only. Mode "minimal" runs the ORIGINAL Player.take_damage
# with a counting die sink. Mode "full" loads the actual Main/Player scene and
# leaves all damage/death/cleanup implementations and signal connections intact.

class DamagePlayer:
	extends Player
	var death_calls = 0
	func flash():
		pass
	func _attract_nearby_consumables():
		pass
	func die(_args = Utils.default_die_args):
		death_calls += 1
		dead = true

var output = OS.get_environment("SCRAPSHIFT_PLAYER_CLEANUP_OUTPUT")
var mode = OS.get_environment("SCRAPSHIFT_PLAYER_CLEANUP_MODE")
var case_name = OS.get_environment("SCRAPSHIFT_PLAYER_CLEANUP_CASE")
var rows = []
var main
var player
var victim
var initial_kills = 0
var waiting = false
var enemy_signals = []
var validation_errors = []

func _ready():
	pause_mode = Node.PAUSE_MODE_PROCESS
	get_tree().create_timer(45, true).connect("timeout", self, "watchdog")
	call_deferred("boot")

func watchdog():
	push_error("PLAYER_CLEANUP_REFERENCE watchdog")
	get_tree().quit(5)

func boot():
	if output.empty():
		get_tree().quit(2)
		return
	DebugService.disable_saving = true
	var save_root = output.get_base_dir() + "/profile/"
	Directory.new().make_dir_recursive(save_root)
	ProgressData.init_save_paths(save_root)
	ProgressData.reset()
	ProgressData.init_settings()
	ProgressData.settings.on_lost_focus = 0
	ProgressData.settings.pause_on_focus_lost = false
	ProgressData.settings.retry_wave = false
	InputMap.action_erase_events("ui_pause")
	RunData.reset()
	RunData.players_data[0].current_character = ItemService.characters[0]
	RunData.current_difficulty = 0
	RunData.current_zone = 0
	RunData.invulnerable = false
	DebugService.invulnerable = false
	DebugService.starting_gold = 0
	AudioServer.set_bus_mute(0, true)
	if mode != "full":
		minimal_probe()
		return
	ZoneService.set_current_zone(ZoneService.get_zone_data(0))
	# Pause before changing scenes: no ordinary movement/physics callback may run
	# between Main._ready and the fixture's recursive freeze. Autoload still runs.
	get_tree().paused = true
	waiting = true
	get_tree().change_scene(MenuData.game_scene)

func minimal_probe():
	var unit = DamagePlayer.new()
	unit.player_index = 0
	unit.current_stats.health = 1
	unit.max_stats.health = 10
	unit.current_stats.armor = 0
	var invincibility = Timer.new()
	add_child(invincibility)
	unit._invincibility_timer = invincibility
	# _health_regen_timer is a FixedTimer Reference, not a Node Timer. Preserve
	# Player's authored instance; assigning a Timer silently converted it to nil.
	unit._health_regen_timer = FixedTimer.new()
	var args = TakeDamageArgs.new(-1)
	args.armor_applied = false
	args.dodgeable = false
	args.bypass_invincibility = true
	var result = unit.take_damage(1, args)
	rows.append({"case": "actual_player_take_damage", "result": result, "health": unit.current_stats.health, "dead_before_return": unit.dead, "die_calls_before_return": unit.death_calls, "scope": "Actual Player.take_damage and defense math; die/flash/consumable-attraction sinks replaced, timers supplied, no Main cleanup"})
	if not result is Array or unit.current_stats.health != 0 or not unit.dead or unit.death_calls != 1:
		validation_errors.append("Actual Player.take_damage must return a damage array and synchronously call die exactly once before return")
	unit.free()
	finish()

func _process(_delta):
	if not waiting:
		return
	var current = get_tree().current_scene
	if not current is Main or current._players.empty():
		return
	waiting = false
	main = current
	call_deferred("full_probe")

func freeze(node):
	node.set_process(false)
	node.set_physics_process(false)
	if node is Timer:
		node.stop()
	for child in node.get_children():
		freeze(child)

func snapshot(label):
	var ground = 0
	for gold in main._active_golds:
		ground += gold.value
	rows.append({"event": label, "player_health": player.current_stats.health, "player_dead": player.dead, "main_cleaning": main._cleaning_up, "spawner_cleaning": main._entity_spawner._cleaning_up, "enemy_hp": victim.current_stats.health, "enemy_pending": victim._pending_die, "enemy_dead": victim.dead, "enemy_kills": ProgressData.data.get("enemies_killed", 0) - initial_kills, "ground_material_value": ground, "banked_materials": RunData.bonus_gold, "enemy_death_signals": enemy_signals.duplicate(true)})

func enemy_died(_enemy, args):
	enemy_signals.append({"cleaning_up": args.cleaning_up, "enemy_killed_by_player": args.enemy_killed_by_player})

func full_probe():
	player = main._players[0]
	freeze(main)
	player.current_stats.health = 1
	player.current_stats.armor = 0
	player._hit_protection = 0
	var spawn_args = EntitySpawner.SpawnEntityArgs.new(Vector2(1000, 700), EntityType.ENEMY)
	victim = main._entity_spawner.spawn_entity(load("res://entities/units/enemies/baby_alien/baby_alien.tscn"), spawn_args)
	freeze(victim)
	victim.current_stats.health = 1
	victim.max_stats.health = 1
	victim.current_stats.armor = 0
	victim.connect("died", self, "enemy_died")
	initial_kills = ProgressData.data.get("enemies_killed", 0)
	var enemy_args = TakeDamageArgs.new(0)
	var player_args = TakeDamageArgs.new(-1)
	player_args.dodgeable = false
	player_args.armor_applied = false
	player_args.bypass_invincibility = true
	snapshot("before")
	if case_name == "player_first":
		player.take_damage(1, player_args)
		snapshot("after_player_before_enemy_hit")
		victim.take_damage(1, enemy_args)
	else:
		victim.take_damage(1, enemy_args)
		snapshot("after_enemy_before_player_hit")
		if case_name == "enemy_flushed":
			yield(get_tree(), "idle_frame")
			yield(get_tree(), "idle_frame")
			snapshot("after_enemy_deferred_flush")
		player.take_damage(1, player_args)
	snapshot("after_calls_before_deferred_flush")
	yield(get_tree(), "idle_frame")
	yield(get_tree(), "idle_frame")
	snapshot("after_deferred_flush")
	finish()

func finish():
	if mode == "full":
		var final = rows[rows.size() - 1]
		var expected_kills = 1 if case_name == "enemy_flushed" else 0
		if final.event != "after_deferred_flush" or not final.player_dead or not final.main_cleaning or not final.spawner_cleaning or not final.enemy_dead or final.enemy_kills != expected_kills:
			validation_errors.append("Full cleanup terminal flags or kill count did not match source acceptance")
		if case_name == "enemy_flushed":
			var found_flush = false
			for row in rows:
				if row.event == "after_enemy_deferred_flush":
					found_flush = row.enemy_kills == 1 and row.ground_material_value >= 1 and not row.player_dead and not row.main_cleaning
			if not found_flush:
				validation_errors.append("Control must credit the dispatched enemy death and create material loot before player death")
		else:
			if final.ground_material_value != 0 or final.banked_materials != 0:
				validation_errors.append("Preempted enemy must not create or bank material loot")
		if final.enemy_death_signals.size() != 1:
			validation_errors.append("Expected exactly one enemy died signal despite deferred request and cleanup overlap")
	var passed = validation_errors.empty()
	var result = {"status": "Passed" if passed else "Failed", "validation_errors": validation_errors, "mode": mode, "case": case_name, "rows": rows, "scope": "Full mode uses actual Main/Player/Enemy scenes and production damage, died signals and cleanup; natural Update/physics/timers frozen, HP and event order staged. Minimal mode substitutes terminal/rendering sinks. No human controls, balance or audio/visual acceptance."}
	var file = File.new()
	if file.open(output, File.WRITE) != OK:
		get_tree().quit(2)
		return
	file.store_string(JSON.print(result, "  "))
	file.close()
	print("PLAYER_CLEANUP_REFERENCE_DONE ", mode, " ", case_name)
	get_tree().quit(0 if passed else 1)
