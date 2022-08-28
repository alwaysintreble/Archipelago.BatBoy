![Archipelago Batboy Image](/docs/BatBoy_AP_Logo_layered.png)
# What is this?
This is a mod for the currently unreleased [Bat Boy](https://store.steampowered.com/app/1709350/Bat_Boy/) demo that connects it to the
[Archipelago architecture](https://github.com/ArchipelagoMW/Archipelago). Currently, this only works as a fully remote mod, meaning that a server must host the data for the
randomized game, whether playing a seed by yourself or with other people.

# What's Archipelago?
Archipelago is a multi-game, multiworld randomizer. It allows games like this to be randomized and played in a multiworld with all other
[Archipelago supported games](https://archipelago.gg/games)!

# What gets randomized?
The seeds, cassettes, and abilities obtained within
the demo's 3 levels are randomized, as well as the seeds and first HP upgrade in Helsia's shop.

# Are there any "gotchas"?
* Bat spin is required to progress in the second and third levels, so your bat spin will always be either in the first level or the first purchase of your shop.
* If you select Quit multiple times from the pause menu while in a level, the game will crash. This is a vanilla bug,
so I won't be fixing this.

# How do I play?
This mod is split into three separate sections. All of these are available from the [releases page](https://github.com/alwaysintreble/Archipelago.BatBoy/releases).
* The apworld
* A config/yaml file
* The game mod

The mod is required to play by grabbing the zip folder from the releases page and unzipping all of its contents into your Bat Boy game folder.
In order to generate the randomized seed you will require the config/yaml file in order to supply your settings to the generator. The apworld
adds the game information to the archipelago generator, server, and clients.

# How do I generate my randomized game?
In order to generate a randomized game for Bat Boy, you must download the most recent `.apworld`, and place it in an Archipelago 0.3.5 or newer release in the `worlds`
folder. The default path for this is `C:\ProgramData\Archipelago\worlds`, so you should have `C:\ProgramData\Archipelago\worlds\batboy.apworld`. You can then use the
default YAML template, modify it in a text editor to select your options, place it in the `Archipelago/Players` folder and run `ArchipelagoGenerate.exe` to generate your randomized game.
This will output a zip folder in `Archipelago/output`. From there, you can either upload this resulting  zip to the [Archipelago website](https://archipelago.gg/),
or run `ArchipelagoMultiServer.exe` and select the resulting zip. Note that if the game is uploaded to the  website all of your items and locations will show up as "Unknown".

# How do I connect to the randomized game?
In order to connect your game to the randomized server, download the latest release from the [releases page](https://github.com/alwaysintreble/Archipelago.BatBoy/releases)
and place the contents of the zip folder in your Bat Boy Demo game folder (probably `C:\Program Files (x86)\Steam\steamapps\Common\Bat Boy Demo`, which you can also reach by right clicking the game in
your Steam library and going to `Manage>Browse local files`). Run the game, and in the top left connection box, enter the connection info to connect and play! `Host Name` is the server IP and port.
If you are running the `Multiserver` locally, this is `localhost`; if you're connecting to a friend, this will be `IP.Add.ress:38281`; and if it's on the website, it will be
`archipelago.gg:<port>`. `Name` is the name field from the earlier yaml file (default `YourName1`), and `password` is optional, only required if the host set one.

# Issues
* Disconnecting will crash the game.
   - If this occurs, your login information is stored along with the save slot, so you can simply reload the save file.
* No console exists.
   - To see items received and sent, or to hint, you must connect to your slot with an Archipelago Text Client.
