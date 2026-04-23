# linguistic-research


# starting app

`python3.11 -m venv venv`
`source venv/bin/activate`
`pip install biopython==1.87 ete3==3.1.3 numpy==2.4.4 PyQt5==5.15.11 lxml==6.1.0 six==1.17.0`

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
- `--job phylogenetic-tree-ipa-singular-choice`, where in case of ambiguity in ipa rule selection, the first one is chosed
- `--job phylogenetic-tree-ipa-random-choice`, where random rule selection is done multiple times, and then the result is averaged.

required parameters:
- `--ipa-rules [path]`
- `--random-ipa-iterations [int]` required only for random choice job (second)

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

