# linguistic-research


# starting app

## docker

In the main directory
- `docker compose build` (rebuild if any changes to the code are made)
- ensure you have all files (probably in input_data directory)
- `docker compose up`

Program runs.

In [docker-compose.yml](./docker-compose.yml) you can edit command's to the script.

## no docker

`python3.11 -m venv venv`
`source venv/bin/activate`
`pip install --no-cache-dir -r requirements.txt`

- dotnet 10.0

launching:
- cd phylogenetic-project
- dotnet build
- dotnet run

ensure you have all files (probably in input_data directory) to run below mock cli command:

test:
```
dotnet run --job phylogenetic-tree-standard-text --input-type sql --input-type-path ../input_data/SadownikDB.sqlite --input-type-id sadownikdb --output-folder-path ../output_data/ --book-idbs 28,29,36 --chapters 1,2 --no-python
```

# docs

## phylogenetic tree generation

### 1. Generating trees using text based levensthein

param: `--job phylogenetic-tree-standard-text`

### 2. Generating trees using ipa based levensthein

While generating trees using ipa we can choose the options for it.
- `--job phylogenetic-tree-ipa-singular-choice`, where in case of ambiguity in ipa rule selection, the first one is chosen
- `--job phylogenetic-tree-ipa-random-choice`, where random rule selection is done multiple times, and then the result is averaged.
- `--job analyze-missing-letters-from-ipa-rules`, doesn't generate trees. Checks the letters used in the text, and checks ipa rules coverage. Additionally when, `--custom-ipa-distance` is provided, it, checks coverage of ipa rules in defined custom distance. Should be ran with all chapters.

required parameters:
- `--ipa-rules [path]`
- `--ipa-rules-id [string]` identifier for the resource (used mainly in valid caching)
- `--random-ipa-iterations [int]` required only for random choice job (second)

optional parameters:
- `--custom-ipa-distance [path]`
- `--custom-ipa-distance-id [string]` identifier for the resource (used mainly in valid caching)
- `--parallel-workers [int]` works only for random choice job (second). It parallelizes the work across X workers. Default is 1.

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

C# code executes some python scripts to generate graphs and some python implemented algorithms. If not set up properly, may cause issues.

#### 4.2 `--map-idb-to-name [path]`

Maps `idb`s to more human readable names.

#### 4.3 `--cache-path [path]`

If not present, it will be created.

#### 4.4 `--normalization-rules [path]` and `--normalization-rules-id [string]`

Normalizes chapter text (e.g. removes punctuation, collapses spaces) before any job uses it. Works with every job and every `--input-type`. Both params must be provided together.

- `--normalization-rules [path]` path to the rules json
- `--normalization-rules-id [string]` identifier for the resource (used mainly in valid caching).

# code docs

## phylogenetic tree generation

