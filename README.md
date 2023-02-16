# FileWatcher
Vzorový program pro PuxDesign
REST API , Blazor

# Popis 
Program po zadání uživatelem cesty k adresáři analyzuje obsah adresáře a vytvoří JSON soubor (pokud neexistuje), který bude zapisovat změny adresáře. Když je adresář analyzován po první tak zapíše jeho obsah do JSONu aby při dalším spuštění mohl porovnávat změny. 

JSON jsem použil abych si každý soubor v adresáři jednoduše serializoval do vlastního listu objektu a z něho porovnám jestli soubor je změněn, odstraněn a nebo přidán. Na konci analýzy se smaže pro daný adresář celá jeho historie a znovu se naplní JSON s aktualním stavem souborů.

Po spuštění je možné buď využit UI od swaggeru a nebo využít jednoduché UI napsané v Blazoru, kde výsledek je zobrazen v tabulce.




 
