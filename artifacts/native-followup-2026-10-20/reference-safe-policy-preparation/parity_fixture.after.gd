extends Node
# HEADLESS_CAMPAIGN_ADAPTATION: isolated fixture only, no desktop/window control.
var survival = OS.get_environment("SURVIVAL").to_lower() in ["1", "true", "yes"]
# MATCHED_SAFE_LOADOUT_POLICY: fixture acquisition choices only; no damage edits.
var safe_loadout = OS.get_environment("CAMPAIGN_SAFE_LOADOUT").to_lower() in ["1", "true", "yes"] and not survival
var safe_skipped_offers = {}
var safe_skipped_offer_count = 0
var safe_recycled_crates = 0

var completed_waves = {}
var observed_damage = 0.0
var observed_healing = 0.0
var lowest_health = -1.0
var last_health = -1.0
var last_max_health = -1.0
var death_observed_ms = -1

var active = false
var pause_interventions = 0
var started = 0
var scene_id = 0
var wave_age = 0.0
var total_age = 0.0
var purchases = 0
var upgrades = 0
var crates = 0
var wave_seen = {}
var trace = []
var output = OS.get_environment("OUTPUT")
var last_action = 0.0
func _init():
	print("PARITY_INIT")
func _ready():
	print("PARITY_READY")
	pause_mode = Node.PAUSE_MODE_PROCESS
	call_deferred("boot")
func boot():
	print("PARITY_BOOT_START")
	if output.empty():
		push_error("OUTPUT must name a fresh isolated evidence folder")
		get_tree().quit(2);return
	Directory.new().make_dir_recursive(output)
	started = OS.get_ticks_msec()
	DebugService.disable_saving = true
	Directory.new().make_dir_recursive(output+"/userdata/")
	ProgressData.init_save_paths(output+"/userdata/")
	ProgressData.reset()
	ProgressData.init_settings()
	DebugService.starting_gold = 0
	DebugService.invulnerable = not survival
	ProgressData.settings.on_lost_focus = 0
	ProgressData.settings.pause_on_focus_lost = false
	# Scripted comparison does not consume human pause controls. InputMap
	# removal prevents polling; _input handling alone would not do that.
	InputMap.action_erase_events("ui_pause")
	Input.action_release("ui_pause")
	ProgressData.settings.retry_wave = false
	ProgressData.settings.mouse_only = false
	ProgressData.settings.manual_aim = false
	ProgressData.settings.manual_aim_on_mouse_press = false
	print("PARITY_RESET")
	RunData.reset()
	print("PARITY_RESET_DONE")
	RunData.players_data[0].current_character = ItemService.characters[0]
	RunData.current_difficulty = 0
	RunData.invulnerable = not survival
	RunData.current_zone = 0
	ZoneService.set_current_zone(ZoneService.get_zone_data(0))
	for spec in [["ranged","pistol"],["ranged","smg"],["ranged","double_barrel_shotgun"],["ranged","laser_gun"],["melee","knife"],["melee","spear"]]:
		print("PARITY_ADD_WEAPON ",spec)
		RunData.add_weapon(load("res://weapons/%s/%s/2/%s_2_data.tres" % [spec[0],spec[1],spec[1]]),0)
	seed(771)
	Engine.time_scale = 1
	Engine.iterations_per_second = 60
	Engine.target_fps = 60
	OS.vsync_enabled = false
	AudioServer.set_bus_mute(0,true)
	active = true
	event("boot", {"engine":Engine.get_version_info(),"loadout":weapon_list(),"userdata":OS.get_user_data_dir(),"save_root":output+"/userdata/", "character":"Well-Rounded identity only; effects not applied"})
	if safe_loadout:
		event("safe_loadout_policy", {"detail": "Opt-in matched acquisition policy skips positive lose_hp_per_second offers and recycles such crates; contact immunity only, periodic damage unchanged. Historical original traces did not filter choices."})
	get_tree().change_scene(MenuData.game_scene)
func safe_loadout_excludes(item):
	if not safe_loadout or item == null or not item is ItemParentData or item.effects == null:
		return false
	for effect in item.effects:
		if effect is Effect and effect.value > 0 and effect.key in ["lose_hp_per_second", "stat_lose_hp_per_second"]:
			return true
	return false

func safe_skip_offer(item):
	var token = str(RunData.current_wave) + ":" + str(item.get_instance_id()) + ":" + item.item_data.my_id
	if not safe_skipped_offers.has(token):
		safe_skipped_offers[token] = true
		safe_skipped_offer_count += 1
		event("purchase_skipped_policy", {"id": item.item_data.my_id, "price": item.value})

func weapon_list():
	var a=[]
	for w in RunData.get_player_weapons(0):a.append(w.my_id)
	return a
func movement():
	return Vector2(cos(wave_age*.8),-sin(wave_age*.8))
func event(kind,extra={}):
	var row={"event":kind,"wave":RunData.current_wave,"wall_seconds":(OS.get_ticks_msec()-started)/1000.0,"simulated_seconds":total_age,"gold":RunData.get_player_gold(0),"level":RunData.players_data[0].current_level,"weapons":weapon_list(),"item_count":RunData.get_player_items(0).size()}
	row["safe_loadout_policy"] = safe_loadout
	row["damage_immunity_scope"] = "none" if survival else "contact_hurtbox_only"
	row["skipped_self_damage_offers"] = safe_skipped_offer_count
	row["recycled_self_damage_crates"] = safe_recycled_crates
	row["headless"] = true
	row["captures_verified"] = false
	row["survival"] = survival
	row["invulnerability_injected"] = not survival
	row["completed_waves"] = completed_waves.size()
	row["health"] = last_health
	row["max_health"] = last_max_health
	row["lowest_observed_health"] = lowest_health
	row["observed_net_damage"] = observed_damage
	row["observed_net_healing"] = observed_healing
	row["death_observation_age"] = (OS.get_ticks_msec()-death_observed_ms)/1000.0 if death_observed_ms >= 0 else 0.0
	row["fixture_time_scale"] = Engine.time_scale
	row["fixture_physics_hz"] = Engine.iterations_per_second
	row["fixture_render_target"] = Engine.target_fps
	row["fixture_pause_interventions"] = pause_interventions
	row["harvesting"] = Utils.get_stat(Keys.stat_harvesting_hash,0)
	row["luck"] = Utils.get_stat(Keys.stat_luck_hash,0)
	row["banked_materials"] = RunData.bonus_gold
	row["enemy_kills"] = ProgressData.data.get("enemies_killed",0)
	row["tree_kills"] = ProgressData.data.get("trees_killed",0)
	row["material_pickups"] = ProgressData.data.get("materials_collected",0)
	row["ground_material_objects"] = 0
	row["ground_material_value"] = 0
	var main_scene = get_tree().current_scene
	if main_scene is Main:
		row["ground_material_objects"] = main_scene._active_golds.size()
		for gold in main_scene._active_golds:
			row["ground_material_value"] += gold.value
	for k in extra:row[k]=extra[k]
	trace.append(row)
	var f=File.new();f.open(output+"/trace.json",File.WRITE);f.store_string(JSON.print(trace,"  "));f.close()
	print("PARITY ",JSON.print(row))
func capture(name):
	# --no-window evidence deliberately contains no screen captures.
	pass
func _process(delta):
	if not active:return
	# Option initialization may reload global Input Map after boot. Reapply
	# only this fixture pause-control isolation before Main polling.
	InputMap.action_erase_events("ui_pause")
	ProgressData.settings.pause_on_focus_lost = false
	total_age += delta
	var scene=get_tree().current_scene
	if scene==null:return
	if survival and scene is Main and scene._is_wave_failed and scene._end_wave_timer_timedout and scene._retry_wave.visible:
		finish("lost");return
	if get_tree().paused:
		pause_interventions += 1
		event("fixture_pause_intervention", {"reason":"unexpected pause during scripted comparison; explicitly resumed", "was_paused":true})
		if scene is Main:
			scene._pause_menu.unpause()
		else:
			get_tree().paused = false
	if OS.get_ticks_msec()-started>1800000:
		finish("timeout");return
	if scene.get_instance_id()!=scene_id:
		scene_id=scene.get_instance_id();last_action=total_age+.8
		event("scene",{"filename":scene.filename})
		if scene is Main:
			wave_age=0;wave_seen[RunData.current_wave]=true
			scene._wave_timer.connect("timeout",self,"on_wave_end")
			capture("wave-%02d-start"%RunData.current_wave)
		elif not scene is BaseShop and not scene.filename.ends_with("parity_bootstrap.tscn"):
			capture("end");finish("won" if RunData.run_won else "unexpected_end");return
	if total_age<last_action:return
	if scene is Main:
		var ui=scene._upgrades_ui
		if ui.visible and ui._player_is_choosing[0]:
			var panel=ui._get_player_container(0)
			if panel._items_container.visible:
				var item=panel._item_data
				if item!=null:
					crates += 1
					if safe_loadout_excludes(item):
						safe_recycled_crates += 1
						event("crate_recycle_policy", {"id": item.my_id})
						ui._on_discard_button_pressed(item, 0)
					else:
						event("crate_take", {"id": item.my_id})
						ui._on_take_button_pressed(item, 0)
			elif panel._upgrades_container.visible:
				var upgrade=panel._upgrade_ui_1.upgrade_data
				if upgrade!=null:
					upgrades+=1;event("upgrade",{"id":upgrade.my_id});ui._on_choose_button_pressed(upgrade,0)
			last_action=total_age+.3
	elif scene is BaseShop:
		var container=scene._get_shop_items_container(0)
		for item in container._shop_items:
			if not item.active or item.item_data==null:continue
			if item.value>RunData.get_player_currency(0):continue
			if item.item_data is WeaponData and not container._can_weapon_be_bought(item):continue
			if safe_loadout_excludes(item.item_data):
				safe_skip_offer(item)
				continue
			if container._is_delay_active:return
			purchases+=1;event("purchase",{"id":item.item_data.my_id,"price":item.value});container.on_shop_item_buy_button_pressed(item);last_action=total_age+.3;return
		event("shop_exit");scene._on_GoButton_pressed(0);last_action=total_age+1
func _input(_event):
	# Scripted fixture owns input; this does not change production input mappings.
	get_tree().set_input_as_handled()
func _physics_process(delta):
	if not active:return
	wave_age+=delta
	var scene=get_tree().current_scene
	if not scene is Main or scene._players.empty():return
	var player=scene._players[0]
	if not is_instance_valid(player):return
	var health=float(player.current_stats.health)
	last_max_health=float(player.max_stats.health)
	if last_health >= 0:
		observed_damage+=max(0.0,last_health-health)
		observed_healing+=max(0.0,health-last_health)
	lowest_health=health if lowest_health < 0 else min(lowest_health,health)
	var changed=last_health >= 0 and health != last_health
	last_health=health
	if health <= 0 and death_observed_ms < 0:death_observed_ms=OS.get_ticks_msec()
	if changed:event("health_net_change")
func on_wave_end():
	completed_waves[RunData.current_wave]=true
	event("wave_timer_end")
	capture("wave-%02d-end"%RunData.current_wave)
func finish(status):
	active=false
	event("finished",{"status":status,"waves_started":wave_seen.size(),"purchases":purchases,"upgrades":upgrades,"crates":crates,"run_won":RunData.run_won})
	var valid_loss=survival and status=="lost" and observed_damage > 0
	var passed=(status=="won" and completed_waves.size()==20) or valid_loss
	var summary={"status":"Passed" if passed else "Failed","outcome":status,"completed_waves":completed_waves.size(),"waves_started":wave_seen.size(),"headless":true,"captures_verified":false,"survival":survival,"invulnerability_injected":not survival,"observed_net_damage":observed_damage,"observed_net_healing":observed_healing,"lowest_observed_health":lowest_health,"scope":"Headless scripted reference fixture; six supplied tier-II weapons, neutral character effects, Danger0 and engine-specific RNG. Survival mode accepts natural defeat only after the source retry/OK acknowledgment; a loss is NOT twenty completed waves. Health values are net changes sampled per physics callback, not every hit/heal event. No captures, human play, ordinary input/focus or visual/audio validation."}
	summary["safe_loadout_policy"] = safe_loadout
	summary["damage_immunity_scope"] = "none" if survival else "contact_hurtbox_only"
	summary["skipped_self_damage_offers"] = safe_skipped_offer_count
	summary["recycled_self_damage_crates"] = safe_recycled_crates
	if safe_loadout:
		summary["scope"] += " Explicit matched safe-loadout acquisition policy: positive periodic self-damage offers skipped and such crates recycled; no production damage changes. Historical original campaign used unfiltered choices."
	var f=File.new();f.open(output+"/campaign-check.json",File.WRITE);f.store_string(JSON.print(summary,"  "));f.close()
	get_tree().quit(0 if passed else 1)


