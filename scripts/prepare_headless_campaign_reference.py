"""Prepare only the isolated reference clone for noninteractive campaign checks.

Does not launch Godot. Launch later with --no-window and OUTPUT=<fresh-folder>,
SURVIVAL=1 for ordinary damage or 0 for the explicit invulnerable comparison.
The original supplied project is read-only. Existing fixture behavior is retained
except headless captures, configurable mode/output, health observations and loss
acknowledgment handling. Preparation writes hashes/backups into --evidence.
"""
import argparse
import hashlib
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
DEFAULT_CLONE = REPO.parents[2] / "tmp/BrotatoParity20261020"
ORIGINAL = Path("E:/UnityProjects/brotato-(3.x)")

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def replace_once(text, old, new):
    if text.count(old) != 1:
        raise ValueError("Expected exactly one baseline fixture fragment: " + repr(old[:100]))
    return text.replace(old, new, 1)

def prepare(clone, evidence):
    clone = clone.resolve()
    expected = DEFAULT_CLONE.resolve()
    if clone != expected or clone == ORIGINAL.resolve():
        raise ValueError("Only the established isolated BrotatoParity20261020 clone may be changed.")
    evidence = evidence.resolve()
    evidence.relative_to(REPO.resolve())
    evidence.mkdir(parents=True, exist_ok=True)
    # The inherited isolated ModLoader redirection is initialized before the
    # fixture autoload boots. Prepare its parent directory before future launches.
    # Do not rewrite any running clone script to solve a log-directory problem.
    redirected_mod_log_dir = clone / "userdata-attempt-2-20261017/logs"
    redirected_mod_log_dir.mkdir(parents=True, exist_ok=True)
    fixture = clone / "parity_fixture.gd"
    baseline = evidence / "parity_fixture.baseline.gd"
    if not baseline.exists():
        if "HEADLESS_CAMPAIGN_ADAPTATION" in fixture.read_text(encoding="utf-8"):
            raise ValueError("Adapted clone requires its existing evidence baseline; do not overwrite provenance.")
        baseline.write_bytes(fixture.read_bytes())
    text = baseline.read_text(encoding="utf-8")
    before_original = {name: sha(ORIGINAL / name) for name in ["project.godot", "main.gd", "singletons/run_data.gd", "singletons/debug_service.gd"]}
    text = replace_once(text, "extends Node", """extends Node
# HEADLESS_CAMPAIGN_ADAPTATION: isolated fixture only, no desktop/window control.
var survival = OS.get_environment("SURVIVAL").to_lower() in ["1", "true", "yes"]
var completed_waves = {}
var observed_damage = 0.0
var observed_healing = 0.0
var lowest_health = -1.0
var last_health = -1.0
var last_max_health = -1.0
var death_observed_ms = -1
""")
    output_line = next(line for line in text.splitlines() if line.startswith("var output = "))
    text = replace_once(text, output_line, 'var output = OS.get_environment("OUTPUT")')
    text = replace_once(text, '\tprint("PARITY_BOOT_START")', '\tprint("PARITY_BOOT_START")\n\tif output.empty():\n\t\tpush_error("OUTPUT must name a fresh isolated evidence folder")\n\t\tget_tree().quit(2);return')
    text = replace_once(text, '\tDebugService.disable_saving = true', '\tDebugService.disable_saving = true\n\tDirectory.new().make_dir_recursive(output+"/userdata/")\n\tProgressData.init_save_paths(output+"/userdata/")\n\tProgressData.reset()\n\tProgressData.init_settings()')
    old_save_root = '"save_root":"E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity20261020/userdata-attempt-2-20261017/"'
    text = replace_once(text, old_save_root, '"save_root":output+"/userdata/"')
    text = replace_once(text, 'DebugService.invulnerable = true', 'DebugService.invulnerable = not survival')
    text = replace_once(text, 'RunData.invulnerable = true', 'RunData.invulnerable = not survival')
    text = replace_once(text, '\tProgressData.settings.mouse_only = false', '\tProgressData.settings.retry_wave = false\n\tProgressData.settings.mouse_only = false')
    text = replace_once(text, '\trow["fixture_time_scale"] = Engine.time_scale', """\trow["headless"] = true
\trow["captures_verified"] = false
\trow["survival"] = survival
\trow["invulnerability_injected"] = not survival
\trow["completed_waves"] = completed_waves.size()
\trow["health"] = last_health
\trow["max_health"] = last_max_health
\trow["lowest_observed_health"] = lowest_health
\trow["observed_net_damage"] = observed_damage
\trow["observed_net_healing"] = observed_healing
\trow["death_observation_age"] = (OS.get_ticks_msec()-death_observed_ms)/1000.0 if death_observed_ms >= 0 else 0.0
\trow["fixture_time_scale"] = Engine.time_scale""")
    text = replace_once(text, '\tvar im=get_viewport().get_texture().get_data();im.flip_y();im.save_png(output+"/"+name+".png")', '\t# --no-window evidence deliberately contains no screen captures.\n\tpass')
    text = replace_once(text, '\tif scene==null:return', """\tif scene==null:return
\tif survival and scene is Main and scene._is_wave_failed and scene._end_wave_timer_timedout and scene._retry_wave.visible:
\t\tfinish("lost");return""")
    text = replace_once(text, 'func _physics_process(delta):\n\tif active:wave_age+=delta', """func _physics_process(delta):
\tif not active:return
\twave_age+=delta
\tvar scene=get_tree().current_scene
\tif not scene is Main or scene._players.empty():return
\tvar player=scene._players[0]
\tif not is_instance_valid(player):return
\tvar health=float(player.current_stats.health)
\tlast_max_health=float(player.max_stats.health)
\tif last_health >= 0:
\t\tobserved_damage+=max(0.0,last_health-health)
\t\tobserved_healing+=max(0.0,health-last_health)
\tlowest_health=health if lowest_health < 0 else min(lowest_health,health)
\tvar changed=last_health >= 0 and health != last_health
\tlast_health=health
\tif health <= 0 and death_observed_ms < 0:death_observed_ms=OS.get_ticks_msec()
\tif changed:event("health_net_change")""")
    text = replace_once(text, 'func on_wave_end():\n\tevent("wave_timer_end")', 'func on_wave_end():\n\tcompleted_waves[RunData.current_wave]=true\n\tevent("wave_timer_end")')
    text = replace_once(text, '\tget_tree().quit(0 if status=="won" else 1)', """\tvar valid_loss=survival and status=="lost" and observed_damage > 0
\tvar passed=(status=="won" and completed_waves.size()==20) or valid_loss
\tvar summary={"status":"Passed" if passed else "Failed","outcome":status,"completed_waves":completed_waves.size(),"waves_started":wave_seen.size(),"headless":true,"captures_verified":false,"survival":survival,"invulnerability_injected":not survival,"observed_net_damage":observed_damage,"observed_net_healing":observed_healing,"lowest_observed_health":lowest_health,"scope":"Headless scripted reference fixture; six supplied tier-II weapons, neutral character effects, Danger0 and engine-specific RNG. Survival mode accepts natural defeat only after the source retry/OK acknowledgment; a loss is NOT twenty completed waves. Health values are net changes sampled per physics callback, not every hit/heal event. No captures, human play, ordinary input/focus or visual/audio validation."}
\tvar f=File.new();f.open(output+"/campaign-check.json",File.WRITE);f.store_string(JSON.print(summary,"  "));f.close()
\tget_tree().quit(0 if passed else 1)""")
    project_source = REPO / "artifacts/parity-remaining-2026-10-20/reference-project-campaign.godot"
    project = clone / "project.godot"
    if not (evidence / "project.before-headless.godot").exists():
        (evidence / "project.before-headless.godot").write_bytes(project.read_bytes())
    fixture.write_text(text, encoding="utf-8")
    project.write_bytes(project_source.read_bytes())
    (evidence / "parity_fixture.gd").write_bytes(fixture.read_bytes())
    (evidence / "project.godot").write_bytes(project.read_bytes())
    after_original = {name: sha(ORIGINAL / name) for name in before_original}
    if before_original != after_original:
        raise RuntimeError("Original reference changed during preparation; investigate concurrent writes.")
    report = {"clone": str(clone), "original_read_only": str(ORIGINAL), "original_hashes_before": before_original,
              "original_hashes_after": after_original, "fixture_baseline_sha256": sha(baseline),
              "fixture_sha256": sha(fixture), "project_sha256": sha(project), "project_template_sha256": sha(project_source),
              "changed_clone_files": ["parity_fixture.gd", "project.godot"], "prepared_mod_log_directory": str(redirected_mod_log_dir), "launch_performed": False,
              "launch_environment": {"OUTPUT": "required fresh output folder", "SURVIVAL": "1 ordinary damage; 0 invulnerable comparison"},
              "launch_engine_flag": "--no-window", "no_captures": True}
    (evidence / "preparation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))

if __name__ == "__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--clone",type=Path,default=DEFAULT_CLONE)
    parser.add_argument("--evidence",type=Path,default=REPO/"artifacts/parity-remaining-2026-10-20/headless-reference-preparation")
    args=parser.parse_args();prepare(args.clone,args.evidence)
