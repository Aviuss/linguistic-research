# linguistic-research


# starting app

Make sure you have following files:
- /data and results/book database/SadownikDB.sqlite
- /data and results/json settings/ipa_rules.json
- /data and results/json settings/map_idb_to_name.json
- /data and results/json settings/ipa_letter_distance.csv

also:
- dotnet about 8.0 (maybe would work on older)
- required python libraries (if not provided, the script would generate matrix but crash during tree generation. It is okey, as long as you only need matrix)

launching:
- cd phylogenetic-project
- dotnet build
- dotnet run
