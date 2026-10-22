extends Node
# Isolated autoload only. Execute the original handle_explode_effect body after
# replacing its two deferred emission calls with a payload recorder. Calculation,
# imported effect resources, burning initialization and argument assignment stay
# original. No explosion scene, overlap, damage sink or ordinary play is tested.

func _ready():
	get_tree().create_timer(30, true).connect("timeout", self, "watchdog")
	call_deferred("run_probe")

func watchdog():
	push_error("ITEM_CHAIN_REFERENCE watchdog")
	get_tree().quit(5)

func run_probe():
	DebugService.disable_saving = true
	var source_file = File.new()
	if source_file.open("res://singletons/run_data.gd", File.READ) != OK:
		get_tree().quit(2)
		return
	var source = source_file.get_as_text()
	source_file.close()
	var first = source.find("func handle_explode_effect(")
	var last = source.find("\nfunc update_recycling_tracking_value(", first)
	if first < 0 or last < 0:
		push_error("Cannot locate original explosion function")
		get_tree().quit(4)
		return
	var body = source.substr(first, last-first)
	var first_call = 'WeaponService.call_deferred("explode", first_effect, _explode_args_node)'
	var second_call = 'WeaponService.call_deferred("explode", first_effect, _second_explode_args_node)'
	if body.count(first_call) != 1 or body.count(second_call) != 1:
		push_error("Explosion emission boundary changed")
		get_tree().quit(4)
		return
	body = body.replace(first_call, "capture(first_effect, _explode_args_node)").replace(second_call, "capture(first_effect, _second_explode_args_node)")
	var code = "extends Reference\nvar _explode_args_node = WeaponServiceExplodeArgs.new()\nvar _second_explode_args_node = WeaponServiceExplodeArgs.new()\nvar rows = []\nfunc get_player_effects(index):\n\treturn RunData.get_player_effects(index)\nfunc capture(effect, args):\n\trows.append({\"effect_script\": effect.get_script().resource_path, \"damage\": args.damage, \"chance\": args.burning_data.chance, \"ticks\": args.burning_data.duration, \"burn_damage\": args.burning_data.damage})\n" + body
	var script = GDScript.new()
	script.source_code = code
	if script.reload() != OK:
		get_tree().quit(4)
		return
	var calculator = script.new()
	var rows = []
	for item in ["rip_and_tear", "spicy_sauce"]:
		var cap = 5 if item == "rip_and_tear" else 2
		for copies in range(1, cap+1):
			for sausages in range(0, 5):
				for greek in range(0, 2):
					RunData.reset()
					RunData.add_character(load("res://items/characters/well_rounded/well_rounded_data.tres"), 0)
					for _i in copies:
						RunData.add_item(load("res://items/all/"+item+"/"+item+"_data.tres"), 0)
					for _i in sausages:
						RunData.add_item(load("res://items/all/scared_sausage/scared_sausage_data.tres"), 0)
					if greek:
						RunData.add_item(load("res://items/all/greek_fire/greek_fire_data.tres"), 0)
					var key = Keys.explode_on_death_hash if item == "rip_and_tear" else Keys.explode_on_consumable_hash
					calculator.rows.clear()
					for attempt in range(256):
						seed(20201020+attempt)
						calculator.handle_explode_effect(key, Vector2.ZERO, 0)
						if not calculator.rows.empty():
							break
					if calculator.rows.size() != 1:
						push_error("Source explosion did not emit")
						get_tree().quit(4)
						return
					var payload = calculator.rows[0].duplicate()
					payload.item = item
					payload.copies = copies
					payload.sausages = sausages
					payload.greek_fire = greek
					rows.append(payload)
	var passed = true
	for row in rows:
		passed = passed and row.chance == 0 and row.ticks == 0 and row.burn_damage == 0
	var output = File.new()
	if output.open(OS.get_environment("SCRAPSHIFT_ITEM_CHAIN_OUTPUT"), File.WRITE) != OK:
		get_tree().quit(2)
		return
	output.store_string(to_json({"status": "Passed" if passed else "Failed", "source_sha256": source.sha256_text(), "scope": "original handle_explode_effect body, deferred emission intercepted; imported resources, original damage calculations and payload assignment; no scene or physics", "rows": rows}))
	output.close()
	print("ITEM_CHAIN_REFERENCE_DONE ", rows.size())
	get_tree().quit(0 if passed else 3)
