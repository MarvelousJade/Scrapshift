extends Node2D
var frame=0
var step=0
var index=-1
var projectile
var target
var rows=[]
var events=[]
var cases=[
 {"id":"flame_cap_center","scene":"res://projectiles/flames/flame_projectile.tscn","speed":6000.0,"target":Vector2(174,104),"target_speed":0.0},
 {"id":"flame_cap_graze","scene":"res://projectiles/flames/flame_projectile.tscn","speed":6000.0,"target":Vector2(174,174),"target_speed":0.0},
 {"id":"ordinary_catalog_speed","scene":"res://projectiles/player_projectile.tscn","speed":3000.0,"target":Vector2(150,100),"target_speed":0.0},
 {"id":"delayed_catalog_speed","scene":"res://projectiles/bullet_lightning/delayed_lightning_projectile.tscn","speed":3000.0,"target":Vector2(150,100),"target_speed":0.0},
 {"id":"ordinary_static","scene":"res://projectiles/player_projectile.tscn","speed":600.0,"target":Vector2(150,100),"target_speed":0.0},
 {"id":"delayed_static","scene":"res://projectiles/bullet_lightning/delayed_lightning_projectile.tscn","speed":600.0,"target":Vector2(150,100),"target_speed":0.0},
 {"id":"ordinary_fast_crossing","scene":"res://projectiles/player_projectile.tscn","speed":36000.0,"target":Vector2(300,100),"target_speed":0.0},
 {"id":"delayed_target_crosses_before_enable","scene":"res://projectiles/bullet_lightning/delayed_lightning_projectile.tscn","speed":0.0,"target":Vector2(20,100),"target_speed":6000.0},
 {"id":"ordinary_target_crosses","scene":"res://projectiles/player_projectile.tscn","speed":0.0,"target":Vector2(20,100),"target_speed":6000.0}
]
func _ready():
 DebugService.disable_saving=true
 AudioServer.set_bus_mute(0,true)
 Engine.time_scale=1
 Engine.iterations_per_second=60
 Engine.target_fps=60
 OS.vsync_enabled=false
 call_deferred("next_case")
func next_case():
 if is_instance_valid(projectile): projectile.queue_free()
 if is_instance_valid(target): target.queue_free()
 index+=1
 if index>=cases.size():
  var file=File.new()
  file.open("E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/gameplay-parity-contact-native-2026-10-17/results.json",File.WRITE)
  file.store_string(JSON.print({"engine":Engine.get_version_info(),"physics_hz":60,"time_scale":1,"cases":cases,"frames":rows,"events":events},"  "))
  file.close()
  print("CONTACT_COMPLETE ",events)
  get_tree().quit(0)
  return
 step=0
 var c=cases[index]
 projectile=load(c.scene).instance()
 add_child(projectile)
 projectile.destroy_on_leaving_screen=false
 var stats=load("res://weapons/ranged/pistol/1/pistol_stats.tres").duplicate()
 stats.max_range=100000
 stats.projectile_speed=max(c.speed,1.0)
 projectile.set_weapon_stats(stats)
 projectile.spawn_position=Vector2(0,100)
 projectile.velocity=Vector2(c.speed,0)
 if "delay" in projectile: projectile.delay=2.0
 projectile.shoot()
 var enemy=load("res://entities/units/enemies/baby_alien/baby_alien.tscn").instance()
 target=enemy.get_node("Hurtbox")
 enemy.remove_child(target)
 enemy.free()
 target.collision_mask=8
 target.position=c.target-Vector2(0,7)
 target.connect("area_entered",self,"on_contact")
 add_child(target)
 print("CONTACT_CASE ",c.id)
func on_contact(area):
 events.append({"case":cases[index].id,"step":step,"frame":frame,"disabled":area.is_disabled(),"projectile_x":projectile.position.x,"target_x":target.position.x})
func _physics_process(delta):
 frame+=1
 if index<0 or index>=cases.size(): return
 step+=1
 rows.append({"case":cases[index].id,"step":step,"delta":delta,"projectile_x":projectile.position.x,"target_x":target.position.x,"disabled":projectile._hitbox.is_disabled()})
 target.position.x+=cases[index].target_speed*delta
 if step>=18: call_deferred("next_case")
