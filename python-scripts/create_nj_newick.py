import sys
import json
from Bio.Phylo.TreeConstruction import DistanceTreeConstructor
from Bio.Phylo.TreeConstruction import DistanceMatrix


def main():
    if len(sys.argv) != 2:
        print("Error: Unsupported number of arguments")
        return
    arguments = json.loads(sys.argv[1])
    
    if "save_path" in arguments and "inputmatrix" in arguments:
        matrix = DistanceMatrix(names=names, matrix=inputmatrix)
        tree = constructor.nj(matrix)
        try:
            tree.root_with_outgroup(outgroup_targets=["polski A", "polski B"])
        except:
            pass
        tree.ladderize()
        newick = tree.format('newick')
        print(newick)
    else:
        print("Error: missing arguments")

    constructor = DistanceTreeConstructor()
    

if __name__ == "__main__":
    main()
    