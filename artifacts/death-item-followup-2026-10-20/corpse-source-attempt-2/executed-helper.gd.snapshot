extends Node
# Windowless autoload for an isolated reference clone. The signal connections
# follow the inspected fresh / pooled lifecycle. Main's large death handler is
# narrowed to its actual handle_stat_damages call; spawning/physics is not run.

class ProbeEnemy:
	extends Enemy
	var received = 0
	var pool_returned = false
	func free_entity() -> void:
		pool_returned = true # Replace only the final Main pool insertion sink.
	func take_damage(value: int, _args: TakeDamageArgs) -> Array:
		var amount = min(current_stats.health, value)
		current_stats.health = max(0, current_stats.health - value)
		received += amount
		return [value, amount, false]

class RecordingMain:
	extends Main
	var recorded_rows = []
	var probe_alive
	var probe_effects
	func _on_enemy_died(enemy: Enemy, _args: Entity.DieArgs) -> void:
		var candidates = _entity_spawner.get_all_enemies(false)
		var members = candidates.size()
		var corpse_present = candidates.has(enemy)
		var result = handle_stat_damages(probe_effects, 0)
		recorded_rows.append({"members_at_main": members, "corpse_at_main": corpse_present, "alive_damage": probe_alive.received, "returned": result})

var main
var spawner
var pending
var alive
var effects

func _ready():
	get_tree().create_timer(30, true).connect("timeout", self, "watchdog")
	call_deferred("run_probe")

func watchdog():
	push_error("CORPSE_MEMBERSHIP_REFERENCE watchdog")
	get_tree().quit(5)

func run_probe():
	DebugService.disable_saving = true
	RunData.reset()
	var character = null
	for entry in ItemService.characters:
		if entry.my_id == "character_well_rounded": character = entry
	if character == null:
		push_error("Missing character_well_rounded")
		get_tree().quit(4)
		return
	RunData.add_character(character, 0)
	RunData.add_item(load("res://items/all/cyberball/cyberball_data.tres"), 0)
	RunData.wave_in_progress = true
	RunData.reset_wave_caches()
	effects = RunData.get_player_effect(Keys.dmg_when_death_hash, 0)
	if effects.empty():
		push_error("Imported Cyberball missing death effect")
		get_tree().quit(4)
		return
	var rows = []
	var connections = {}
	for creation in ["main_before_spawner", "spawner_before_main"]:
		if creation == "main_before_spawner":
			main = RecordingMain.new()
			spawner = EntitySpawner.new()
		else:
			spawner = EntitySpawner.new()
			main = RecordingMain.new()
		main._entity_spawner = spawner
		pending = ProbeEnemy.new()
		alive = ProbeEnemy.new()
		main.probe_alive = alive
		main.probe_effects = effects
		run_lifecycle(creation, rows, connections)
		pending.free()
		alive.free()
		main.free()
		spawner.free()
	var output = File.new()
	if output.open(OS.get_environment("SCRAPSHIFT_CORPSE_OUTPUT"), File.WRITE) != OK:
		get_tree().quit(2)
		return
	output.store_string(to_json({"scope": "Actual Godot signal dispatch with Main-derived listener, Entity.death_animation_finished disconnection, EntitySpawner removal/get_all_enemies, Main.handle_stat_damages, and imported Cyberball. Two target construction orders; source-derived spawn connection staging; final damage/pool insertion sinks replaced. No actual spawn, animation scheduling, Unit.take_damage or full Main death handler.", "connections": connections, "rows": rows}))
	output.close()
	print("CORPSE_MEMBERSHIP_REFERENCE_DONE ", rows.size())
	get_tree().quit(0)

func run_lifecycle(creation, rows, connections):
	var animation = AnimationPlayer.new()
	animation.add_animation("RESET", Animation.new())
	pending._animation_player = animation
	pending.entity_signal_node = spawner
	# Source spawn_entity connects spawner, then emits enemy_spawned to Main.
	pending.connect("died", spawner, "_on_enemy_died")
	pending.connect("died", main, "_on_enemy_died")
	pending.connect("wanted_to_spawn_an_enemy", spawner, "on_enemy_wanted_to_spawn_an_enemy")
	pending.connect("charmed", spawner, "on_enemy_charmed")
	for lifecycle in ["fresh", "pooled"]:
		if lifecycle == "pooled":
			# Entity.death_animation_finished disconnects spawner only. The
			# pooled spawn branch reconnects it, retaining Main's connection.
			pending.death_animation_finished()
			pending.connect("died", spawner, "_on_enemy_died")
		var order = []
		for connection in pending.get_signal_connection_list("died"):
			order.append({"method": connection.method, "target_id": connection.target.get_instance_id()})
		connections[creation + ":" + lifecycle] = {"order": order, "main_id": main.get_instance_id(), "spawner_id": spawner.get_instance_id()}
		for trial in range(256):
			pending.current_stats.health = 0
			pending.received = 0
			alive.current_stats.health = 1000
			alive.received = 0
			spawner.enemies = [pending, alive]
			spawner.bosses = []
			spawner.charmed_enemies = []
			spawner._all_enemy_dirty = true
			seed(20201020 + trial)
			pending.emit_signal("died", pending, Entity.DieArgs.new())
			var row = main.recorded_rows.pop_back()
			row.creation = creation
			row.lifecycle = lifecycle
			row.seed = 20201020 + trial
			row.members_after_signal = spawner.enemies.size()
			row.source_pool_return_path = pending.pool_returned
			rows.append(row)
	animation.free()
