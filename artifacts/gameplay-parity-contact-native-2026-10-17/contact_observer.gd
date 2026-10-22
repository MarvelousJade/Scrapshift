extends Node
var fixture
func _physics_process(_delta):
 fixture.record_phase("post_children_before_deferred")
 fixture.call_deferred("record_phase","after_physics_deferred")
