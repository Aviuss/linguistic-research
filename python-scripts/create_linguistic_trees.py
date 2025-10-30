import sys
import json
from io import StringIO 
from Bio import Phylo


def main():
    if len(sys.argv) != 2:
        print("Error: Unsupported number of arguments")
        return
    arguments = json.loads(sys.argv[1])
    
    if "save_path" in arguments and "newickFormat" in arguments:
        treeGeneration(newickFormat, save_path)
    else:
        print("Error: missing arguments")

from ete3 import Tree, TreeStyle, NodeStyle, TextFace

def _setColor(root, leafsAmount):
    nstyle = NodeStyle(hz_line_width=3, vt_line_width=3)
    nstyle["shape"] = "square"
    nstyle["size"] = 3
    
    nstyle["vt_line_width"] = 3
    nstyle["hz_line_width"] = 3

    colorHEX = "#000000"
    nstyle["fgcolor"] = colorHEX
    nstyle["vt_line_color"] = colorHEX
    nstyle["hz_line_color"] = colorHEX

    if root.is_leaf():
        nstyle["size"] = 0
    
    root.set_style(nstyle)
    
    children = root.get_children()
    for child in children:
        _setColor(child, leafsAmount)



def treeGeneration(newickFormat, save_path):
    handle = StringIO(newickFormat.replace("polski A", "polski_A").replace("polski B", "polski_B").replace("kaszubski A", "kaszubski_A").replace("kaszubski B", "kaszubski_B"))
    tree = Phylo.read(handle, "newick")
    try:
        tree.root_with_outgroup(outgroup_targets=["polski_A", "polski_B"])
    except:
        pass
    tree.ladderize()
    newick = tree.format('newick').replace("polski_A", "polski A").replace("polski_B", "polski B").replace("kaszubski_A", "kaszubski A").replace("kaszubski_B", "kaszubski B")
    t = Tree(newick, format=1)
    t.sort_descendants()
    t.ladderize()
    
    ts = TreeStyle()
    ts.margin_top = 10
    ts.margin_left = 10
    ts.margin_right = 10
    ts.margin_bottom = 10
    ts.show_leaf_name = True
    ts.branch_vertical_margin = 3
    ts.scale =  600 # X pixels per branch length unit

    _setColor(t, len(t))
   

    t.render(save_path, w=183, units="mm", tree_style=ts, dpi=400)

    
if __name__ == "__main__":
    main()
    