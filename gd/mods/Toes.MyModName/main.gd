extends Node

const MOD_NAME: String = "My Mod"

################################################
## This is where your GDScript mod should go! ##
################################################

func _ready() -> void:
	get_tree().connect("node_added", self, "_join_tree")
	get_tree().connect("node_added", self, "_add_menu_button")

func _join_tree(node: Node) -> void:
	var map: Node = get_tree().current_scene
	var in_game = map.name == "world"
	var in_menu = node.name == "main_menu"

	if in_menu:
		OS.alert("We're in the main menu!", MOD_NAME)

	if in_game:
		OS.alert("We've joined a lobby!", MOD_NAME)
