extends Node
# Isolated autoload: real Main handler + Enemy/Unit damage and deferred queue.
# Rendering flash and terminal die body are replaced only to avoid scene assets;
# the actual call_deferred("die") and pending guard run in unchanged Unit code.
class DispatchEnemy:
	extends Enemy
	var deaths = 0
	var dispatch_label = ""
	var dispatch_receiver
	func flash():
		pass
	func die(_args = Utils.default_die_args):
		deaths += 1
		if dispatch_receiver != null:
			dispatch_receiver.death_events.append(dispatch_label)

var main
var spawner
var victims = []
var rows = []
var death_events = []

func _ready():
	get_tree().create_timer(30, true).connect("timeout", self, "watchdog")
	call_deferred("run_probe")

func watchdog():
	push_error("DEATH_DISPATCH_REFERENCE watchdog")
	get_tree().quit(5)

func enemy(health):
	var victim = DispatchEnemy.new()
	victim.current_stats.health = health
	victim.max_stats.health = health
	victim.current_stats.armor = 0
	var effects = Node.new()
	victim.add_child(effects)
	victim.effect_behaviors = effects
	victims.append(victim)
	spawner.enemies = [victim]
	spawner.bosses = []
	spawner.charmed_enemies = []
	spawner._all_enemy_dirty = true
	return victim

func run_probe():
	DebugService.disable_saving = true
	RunData.reset()
	var lich
	for character in ItemService.characters:
		if character.my_id == "character_lich":
			lich = character
	if lich == null:
		get_tree().quit(4)
		return
	RunData.add_character(lich, 0)
	RunData.wave_in_progress = true
	RunData.reset_wave_caches()
	main = Main.new()
	spawner = EntitySpawner.new()
	main._entity_spawner = spawner
	var effects = RunData.get_player_effect(Keys.dmg_when_heal_hash, 0)
	var victim = enemy(10)
	var lich_result = main.handle_stat_damages(effects, 0)
	var health_after_healing = victim.current_stats.health
	var pending_after_healing = victim._pending_die
	var hit = Hitbox.new()
	hit.crit_chance = 0.0
	var args = TakeDamageArgs.new(0, hit)
	var late_result = victim.take_damage(50, args)
	rows.append({"case": "lethal_lich_preempts_later_weapon", "lich_result": lich_result, "health_after_healing": health_after_healing, "pending_after_healing": pending_after_healing, "later_weapon_result": late_result, "final_health": victim.current_stats.health, "deaths_before_deferred": victim.deaths})
	hit.free()
	RunData.add_item(load("res://items/all/giant_belt/giant_belt_data.tres"), 0)
	victim = enemy(100)
	lich_result = main.handle_stat_damages(effects, 0)
	health_after_healing = victim.current_stats.health
	hit = Hitbox.new()
	hit.crit_chance = 1.0
	hit.crit_damage = 2.0
	args = TakeDamageArgs.new(0, hit)
	late_result = victim.take_damage(10, args)
	rows.append({"case": "lich_before_giant_belt_crit", "lich_result": lich_result, "health_after_healing": health_after_healing, "later_weapon_result": late_result, "final_health": victim.current_stats.health})
	hit.free()
	# Both Player and Enemy inherit Unit's deferred die. These are narrow Unit
	# scheduling receipts, not complete Player.die/Main cleanup acceptance.
	for order in [["enemy", "player"], ["player", "enemy"]]:
		for label in order:
			var queued = enemy(1)
			queued.dispatch_label = str(rows.size()) + ":" + label
			queued.dispatch_receiver = self
			var plain_args = TakeDamageArgs.new(0)
			queued.take_damage(1, plain_args)
		rows.append({"case": "unit_fifo_" + order[0] + "_first", "queued_order": order, "scope": "Inherited Unit.take_damage/deferred call scheduling only; Player-specific die and Main cleanup not invoked"})
	call_deferred("finish_probe")

func finish_probe():
	rows[0]["deaths_after_deferred"] = victims[0].deaths
	rows[2]["dispatched_order"] = [death_events[0], death_events[1]]
	rows[3]["dispatched_order"] = [death_events[2], death_events[3]]
	var output = File.new()
	if output.open(OS.get_environment("SCRAPSHIFT_DEATH_DISPATCH_OUTPUT"), File.WRITE) != OK:
		get_tree().quit(2)
		return
	output.store_string(to_json({"scope": "Actual Main.handle_stat_damages and Enemy/Unit.take_damage with imported Lich/Giant Belt; Unit deferred die runs a counting sink, flash suppressed. No scene death signals, physical contacts, UI or rendering.", "rows": rows}))
	output.close()
	for victim in victims:
		victim.free()
	spawner.free()
	main.free()
	print("DEATH_DISPATCH_REFERENCE_DONE ", rows.size())
	get_tree().quit(0)
