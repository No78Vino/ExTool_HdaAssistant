import sys
sys.path.append(r'C:\Game\steam\steamapps\common\Houdini Indie\houdini\python3.9libs')
sys.path.append(r'C:\Game\steam\steamapps\common\Houdini Indie\python39\lib\site-packages')

import hrpyc

connection, hou = hrpyc.import_remote_module()

      
obj_node = hou.node("/obj")
geo_node = obj_node.createNode("geo","geo_my_hda_node")
boxNode = geo_node.createNode("box","my_box_node")

hou.hda.installFile('C:/Houdini_Project/GameDemoSceneBuilder/chatgpt_shop_slogan_board.hdalc')
hda_node_name = "gpt_gen_shop_slogan_board"  
hda_instance = geo_node.createNode(hda_node_name,"my_hda_node")