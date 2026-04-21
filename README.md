# linguistic-research


# starting app

Make sure you have following files:
- /data and results/book database/SadownikDB.sqlite
- /data and results/json settings/ipa_rules.json
- /data and results/json settings/map_idb_to_name.json
- /data and results/json settings/ipa_letter_distance.csv

also:
- dotnet about 10.0
- required python libraries (if not provided, the script would generate matrix but crash during tree generation. It is okey, as long as you only need matrix)

launching:
- cd phylogenetic-project
- dotnet build
- dotnet run

# docs

## phylogenetic tree generation

### 1. Generating trees using text based levensthein

param: `--job phylogenetic-tree-standard-text`

### 2. Generating trees using ipa based levensthein

While generating trees using ipa we can choose the options for it.
- `--job phylogenetic-tree-ipa-singular-choice`, where in case of ambiguity in ipa rule selection, the first one is chosed
- `--job phylogenetic-tree-ipa-random-choice`, where random rule selection is done multiple times, and then the result is averaged.

optional parameters:
- `--custom-ipa-distance [path]`

### 3. Required params for phylogenetic tree generation

#### 3.1. `--input-type`

param: `--input-type sql`

param: `--input-type json`

Both input types must have specific interface (TODO). 

#### 3.2. `--input-type-path [path]`

#### 3.3. `--input-type-id [id]`

Identifier must be provided to distinguish data source from each other. It is needed for caching and logging. It exists because different system path may exist for the same data resource, so it is unfeasible to identify resource by path.

#### 3.4. `--book-idbs [...]`

param e.g.: `--book-idbs 28,29,36,38,46,37,44,39,43,33,42`

#### 3.5. `--chapters [...]`

param e.g.: `--chapters 1,2,3`

#### 3.6. `--output-folder-path [path]`

### 4. Optional params for phylogenetic tree generation

#### 4.1 `--no-python`

#### 4.2 `--map-idb-to-name [path]`

Maps `idb`s to more human readable names.

C# code executes some python scripts to generate graphs and some python implemented algorithms. If not set up properly, may cause issues.

# code docs

## phylogenetic tree generation

