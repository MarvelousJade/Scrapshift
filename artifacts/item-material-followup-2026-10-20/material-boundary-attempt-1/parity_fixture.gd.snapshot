extends Node
# Isolated, deliberately injected registration/cleanup race. Not natural play.
var output = OS.get_environment("OUTPUT")
var case_name = OS.get_environment("MATERIAL_CASE")
var started = 0
var active = false
var running = false
var trace = []
var main = null
var optimized = false
var pooled = false

func movement():
	return Vector2.ZERO

func _ready():
	pause_mode = Node.PAUSE_MODE_PROCESS
	started = OS.get_ticks_msec()
	call_deferred("boot")

func boot():
	if output.empty() or not case_name in ["fresh_normal", "pooled_normal", "fresh_optimized", "pooled_optimized"]:
		get_tree().quit(2)
		return
	Directory.new().make_dir_recursive(output + "/userdata/")
	optimized = case_name.ends_with("optimized")
	pooled = case_name.begins_with("pooled")
	DebugService.disable_saving = true
	ProgressData.init_save_paths(output + "/userdata/")
	ProgressData.reset()
	ProgressData.init_settings()
	ProgressData.settings.pause_on_focus_lost = false
	ProgressData.settings.on_lost_focus = 0
	ProgressData.settings.optimize_end_waves = optimized
	InputMap.action_erase_events("ui_pause")
	Input.action_release("ui_pause")
	DebugService.starting_gold = 0
	DebugService.invulnerable = true
	RunData.reset()
	RunData.players_data[0].current_character = ItemService.characters[0]
	RunData.current_difficulty = 0
	RunData.invulnerable = true
	RunData.current_zone = 0
	ZoneService.set_current_zone(ZoneService.get_zone_data(0))
	RunData.add_weapon(load("res://weapons/ranged/pistol/2/pistol_2_data.tres"), 0)
	seed(771)
	AudioServer.set_bus_mute(0, true)
	OS.vsync_enabled = false
	active = true
	get_tree().change_scene(MenuData.game_scene)

func _process(_delta):
	if OS.get_ticks_msec() - started > 45000:
		finish("timeout", 5)
		return
	if not active:
		return
	InputMap.action_erase_events("ui_pause")
	ProgressData.settings.pause_on_focus_lost = false
	if get_tree().paused:
		get_tree().paused = false
	var scene = get_tree().current_scene
	if not running and scene is Main and not scene._players.empty():
		running = true
		main = scene
		call_deferred("run_case")

func snapshot(kind, extra = {}):
	var row = {"event":kind, "case":case_name, "wall_seconds":(OS.get_ticks_msec()-started)/1000.0}
	if is_instance_valid(main):
		row["bank"] = RunData.bonus_gold
		row["player_gold"] = RunData.get_player_gold(0)
		row["registered_count"] = main._active_golds.size()
		row["registered_value"] = 0
		row["nodes"] = []
		row["cleaning_up"] = main._cleaning_up
		for gold in main._active_golds:
			row["registered_value"] += gold.value
			row["nodes"].append({"id":gold.get_instance_id(), "value":gold.value, "visible":gold.visible,
				"attracted":is_instance_valid(gold.attracted_by), "picked":gold.already_picked_up})
	for key in extra:
		row[key] = extra[key]
	trace.append(row)
	var file = File.new()
	file.open(output + "/trace.json", File.WRITE)
	file.store_string(JSON.print(trace, "  "))
	file.close()
	print("BOUNDARY ", JSON.print(row))

func material_event(kind, details = {}):
	if active:
		snapshot("material_" + kind, details)

func run_case():
	# No combat is awaited: suppress scheduled spawning before the first group.
	main._wave_timer.stop()
	main._wave_manager.set_process(false)
	main._wave_manager.set_physics_process(false)
	main._entity_spawner.set_process(false)
	main._entity_spawner.set_physics_process(false)
	for player in main._players:
		player.disable_gold_pickup()
	if main._active_golds.size() != 0 or RunData.bonus_gold != 0 or main._entity_spawner.enemies.size() != 0:
		finish("nonempty_initial_world", 3)
		return
	ProgressData.settings.optimize_end_waves = optimized
	var pos = main._players[0].global_position + Vector2(180, 0)
	if pooled:
		main.spawn_gold(1, pos, 0)
		yield(get_tree(), "idle_frame")
		yield(get_tree(), "idle_frame")
		if main._active_golds.size() != 1:
			finish("pool_warmup_failed", 3)
			return
		# Genuine pickup without cleanup/player credit returns this node to pool.
		main._active_golds[0].pickup(-1)
	snapshot("before_request")
	var state = main.spawn_gold(1, pos, 0)
	snapshot("before_cleanup", {"spawn_suspended":state is GDScriptFunctionState})
	main.clean_up_room()
	# Hold the scene here to inspect accounting before any scene replacement.
	main._end_wave_timer.stop()
	snapshot("after_cleanup_before_idle")
	yield(get_tree(), "idle_frame")
	yield(get_tree(), "idle_frame")
	snapshot("after_deferred_registration")
	yield(get_tree().create_timer(3.0), "timeout")
	snapshot("after_three_seconds")
	finish("completed", 0)

func finish(reason, code):
	if not active:
		get_tree().quit(code)
		return
	snapshot("finished", {"reason":reason})
	active = false
	var result = {"status":"Completed" if code == 0 else "Failed", "case":case_name, "reason":reason,
		"scope":"Injected fresh/pooled material request immediately before genuine Main.clean_up_room; scene transition held. Not historical reproduction, normal gameplay, UI or input validation."}
	var file = File.new()
	file.open(output + "/result.json", File.WRITE)
	file.store_string(JSON.print(result, "  "))
	file.close()
	get_tree().quit(code)
