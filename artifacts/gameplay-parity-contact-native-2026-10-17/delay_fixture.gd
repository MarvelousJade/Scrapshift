extends Node2D
var frame=0
var step=0
var index=-1
var projectile
var target
var rows=[]
var events=[]
var cases=[
 {"id":"ordinary_matching_offset","scene":"res://projectiles/player_projectile.tscn","speed":0.0,"target":Vector2(3,99),"target_speed":0.0,"delay":-1.0},
 {"id":"delayed_zero","scene":"res://projectiles/bullet_lightning/delayed_lightning_projectile.tscn","speed":0.0,"target":Vector2(3,99),"target_speed":0.0,"delay":0.0},
 {"id":"delayed_one","scene":"res://projectiles/bullet_lightning/delayed_lightning_projectile.tscn","speed":0.0,"target":Vector2(3,99),"target_speed":0.0,"delay":1.0},
 {"id":"delayed_two","scene":"res://projectiles/bullet_lightning/delayed_lightning_projectile.tscn","speed":0.0,"target":Vector2(3,99),"target_speed":0.0,"delay":2.0}
]
func _ready():
 DebugService.disable_saving=true
 AudioServer.set_bus_mute(0,true)
 Engine.time_scale=1
 Engine.iterations_per_second=60
 Engine.target_fps=60
 OS.vsync_enabled=false
 call_deferred("next_case")
 var observer=Node.new()
 observer.name="ContactObserver"
 observer.set_script(load("res://contact_observer.gd"))
 observer.fixture=self
 add_child(observer)
func next_case():
 if is_instance_valid(projectile): projectile.queue_free()
 if is_instance_valid(target): target.queue_free()
 index+=1
 if index>=cases.size():
  var file=File.new()
  file.open("E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/gameplay-parity-contact-native-2026-10-17/delay-results.json",File.WRITE)
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
 if "delay" in projectile: projectile.delay=c.delay
 if c.id=="ordinary_matching_offset": projectile.get_node("Hitbox").position=Vector2(-20,-1)
 projectile.shoot()
 var enemy=load("res://entities/units/enemies/baby_alien/baby_alien.tscn").instance()
 target=enemy.get_node("Hurtbox")
 enemy.remove_child(target)
 enemy.free()
 target.collision_mask=8
 target.position=c.target-Vector2(0,7)
 target.connect("area_entered",self,"on_contact")
 add_child(target)
 move_child(get_node("ContactObserver"),get_child_count()-1)
 record_phase("after_shoot_before_deferred")
 print("CONTACT_CASE ",c.id)
func record_phase(phase):
 if index<0 or index>=cases.size(): return
 rows.append({"case":cases[index].id,"phase":phase,"step":step,"delta":1.0/60,"projectile_x":projectile.position.x,"target_x":target.position.x,"disabled":projectile._hitbox.is_disabled(),"delay":projectile.delay if "delay" in projectile else -1})
func on_contact(area):
 events.append({"case":cases[index].id,"step":step,"frame":frame,"disabled":area.is_disabled(),"projectile_x":projectile.position.x,"target_x":target.position.x})
func _physics_process(delta):
 frame+=1
 if index<0 or index>=cases.size(): return
 step+=1
 record_phase("pre_children")
 target.position.x+=cases[index].target_speed*delta
 if step>=18: call_deferred("next_case")
