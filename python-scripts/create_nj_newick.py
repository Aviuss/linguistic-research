import sys
import json
from Bio.Phylo.TreeConstruction import DistanceTreeConstructor
from Bio.Phylo.TreeConstruction import DistanceMatrix


def main():
    if len(sys.argv) != 2:
        print("Error: Unsupported number of arguments")
        return
    print(sys.argv[1])
    arguments = json.loads(sys.argv[1])
    print(arguments)
    
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
        print(newick)
        f = open(arguments["save_path_newick"], "w+")
        f.write(newick)
        f.close()
    else:
        print("Error: missing arguments")

    constructor = DistanceTreeConstructor()
    

if __name__ == "__main__":
    main()
    