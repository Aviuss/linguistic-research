import sys
import json
from Bio.Phylo.TreeConstruction import DistanceTreeConstructor
from Bio.Phylo.TreeConstruction import DistanceMatrix


def main():
    if len(sys.argv) != 2:
        print("Error: Unsupported number of arguments")
        return
    arguments = json.loads(sys.argv[1])
    
    if "save_path_newick" in arguments and "inputmatrix" in arguments and "names" in arguments:
        constructor = DistanceTreeConstructor()
        matrix = DistanceMatrix(names=arguments["names"], matrix=arguments["inputmatrix"])
        tree = constructor.nj(matrix)
        try:
            tree.root_with_outgroup(outgroup_targets=["polski A", "polski B"])
        except:
            pass
        tree.ladderize()
        newick = tree.format('newick')
        f = open(arguments["save_path_newick"], "w+", encoding="utf-8")
        f.write(newick)
        f.close()
        print("Newick saved.")
    else:
        print("Error: missing arguments")
    

if __name__ == "__main__":
    main()
    