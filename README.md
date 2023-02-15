# PuxDesign.FileWatcher
Vzorový program pro PuxDesign

# Popis 
Program po zadání uživatelem cesty k adresáři analyzuje obsah adresáře a vytvoří JSON soubor (pokud neexistuje), který bude zapisovat změny adresáře. Vždy když adresář je analyzován po první tak zapíše obsah do JSON souboru aby program mohl při dalším spuštění porovnávat změny. 

JSON jsem použil abych si každý soubor v adresáři serializoval do vlastního listu objektu a z něho porovnám jestli soubor je změněn, odstraněn a nebo přidán. Na konci analýzy se smaže pro daný adresář celá jeho historie a znovu se naplní JSON s aktualním stavem v adresáři.

Pro detekci přidaného a odebraného souboru jen porovnávam aktualní stav s minulým stavem souborů v adresáři.
Při detekci změny se porovná hash souboru který se při zápisu do JSON vypočite pomocí SHA256 algoritmu. 







 
