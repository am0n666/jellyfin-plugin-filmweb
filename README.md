# Jellyfin Filmweb Metadata Provider Plugin

Wtyczka dla serwera **Jellyfin** integrująca polski serwis **Filmweb.pl** jako dostawcę metadanych dla filmów i seriali.

## Funkcje
- Wyszukiwanie filmów i seriali w bazie Filmweb
- Pobieranie polskich tytułów, opisów, gatunków, roku produkcji oraz ocen społeczności
- Pobieranie informacji o obsadzie (aktorzy, reżyserzy)
- Pobieranie oficjalnych plakatów (Primary Image)
- Rejestracja unikalnego `FilmwebId` (zewnętrzny identyfikator z linkiem do serwisu)

## Kompilacja w systemie Windows

Wymagania:
- Zainstalowane [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

Uruchomienie skryptu kompilacji:
```cmd
build.cmd
```
Skrypt automatycznie skompiluje projekt i przygotuje gotowy katalog wtyczki w folderze `dist\Filmweb`.

## Instalacja na serwerze Jellyfin

1. Skopiuj folder `dist\Filmweb` (zawierający `Jellyfin.Plugin.Filmweb.dll` oraz pliki zależności) do katalogu wtyczek serwera Jellyfin:
   - **Windows**: `%AppData%\Jellyfin\Server\plugins\Filmweb` lub `C:\ProgramData\Jellyfin\Server\plugins\Filmweb`
   - **Linux / Docker**: `/var/lib/jellyfin/plugins/Filmweb` lub katalog zamontowanego wolumenu `plugins/Filmweb`
2. Zrestartuj serwer Jellyfin.
3. Przejdź do **Pulpit** -> **Biblioteki** -> wybierz bibliotekę (np. Filmy lub Seriale) -> **Zarządzaj biblioteką**.
4. Zaznacz **Filmweb** na liście dostawców metadanych i pobierania grafik.
