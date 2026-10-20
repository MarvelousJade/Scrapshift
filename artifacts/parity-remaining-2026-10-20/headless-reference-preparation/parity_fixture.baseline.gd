extends Node
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
var output = "E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/parity-remaining-2026-10-20/original-full"
var last_action = 0.0
func _init():
	print("PARITY_INIT")
func _ready():
	print("PARITY_READY")
	pause_mode = Node.PAUSE_MODE_PROCESS
	call_deferred("boot")
func boot():
	print("PARITY_BOOT_START")
	Directory.new().make_dir_recursive(output)
	started = OS.get_ticks_msec()
	DebugService.disable_saving = true
	DebugService.starting_gold = 0
	DebugService.invulnerable = true
	ProgressData.settings.on_lost_focus = 0
	ProgressData.settings.pause_on_focus_lost = false
	# Scripted comparison does not consume human pause controls. InputMap
	# removal prevents polling; _input handling alone would not do that.
	InputMap.action_erase_events("ui_pause")
	Input.action_release("ui_pause")
	ProgressData.settings.mouse_only = false
	ProgressData.settings.manual_aim = false
	ProgressData.settings.manual_aim_on_mouse_press = false
	print("PARITY_RESET")
	RunData.reset()
	print("PARITY_RESET_DONE")
	RunData.players_data[0].current_character = ItemService.characters[0]
	RunData.current_difficulty = 0
	RunData.invulnerable = true
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
	event("boot", {"engine":Engine.get_version_info(),"loadout":weapon_list(),"userdata":OS.get_user_data_dir(),"save_root":"E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity20261020/userdata-attempt-2-20261017/", "character":"Well-Rounded identity only; effects not applied"})
	get_tree().change_scene(MenuData.game_scene)
func weapon_list():
	var a=[]
	for w in RunData.get_player_weapons(0):a.append(w.my_id)
	return a
func movement():
	return Vector2(cos(wave_age*.8),-sin(wave_age*.8))
func event(kind,extra={}):
	var row={"event":kind,"wave":RunData.current_wave,"wall_seconds":(OS.get_ticks_msec()-started)/1000.0,"simulated_seconds":total_age,"gold":RunData.get_player_gold(0),"level":RunData.players_data[0].current_level,"weapons":weapon_list(),"item_count":RunData.get_player_items(0).size()}
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
	var im=get_viewport().get_texture().get_data();im.flip_y();im.save_png(output+"/"+name+".png")
func _process(delta):
	if not active:return
	# Option initialization may reload global Input Map after boot. Reapply
	# only this fixture pause-control isolation before Main polling.
	InputMap.action_erase_events("ui_pause")
	ProgressData.settings.pause_on_focus_lost = false
	total_age += delta
	var scene=get_tree().current_scene
	if scene==null:return
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
					crates+=1;event("crate_take",{"id":item.my_id});ui._on_take_button_pressed(item,0)
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
			if container._is_delay_active:return
			purchases+=1;event("purchase",{"id":item.item_data.my_id,"price":item.value});container.on_shop_item_buy_button_pressed(item);last_action=total_age+.3;return
		event("shop_exit");scene._on_GoButton_pressed(0);last_action=total_age+1
func _input(_event):
	# Scripted fixture owns input; this does not change production input mappings.
	get_tree().set_input_as_handled()
func _physics_process(delta):
	if active:wave_age+=delta
func on_wave_end():
	event("wave_timer_end")
	capture("wave-%02d-end"%RunData.current_wave)
func finish(status):
	active=false
	event("finished",{"status":status,"waves_started":wave_seen.size(),"purchases":purchases,"upgrades":upgrades,"crates":crates,"run_won":RunData.run_won})
	get_tree().quit(0 if status=="won" else 1)


