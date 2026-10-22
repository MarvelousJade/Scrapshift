extends SceneTree
func _init():
	var f = File.new()
	f.open("E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/reference-keyboard-review-2026-10-17/resolved-user-dir.txt", File.WRITE)
	f.store_string(OS.get_user_data_dir())
	f.close()
	quit()
