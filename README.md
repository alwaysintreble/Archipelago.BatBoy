![Archipelago Batboy Image](/docs/BatBoy_AP_Logo_layered.png)
# What is this?
This is a mod for the currently unreleased [Bat Boy](https://store.steampowered.com/app/1709350/Bat_Boy/) demo to connect it to the
[Archipelago architecture](https://github.com/ArchipelagoMW/Archipelago). Currently this only works as a fully remote mod, meaning a server must host the data for the
randomized game, whether playing a seed by yourself or with other people.

# What's Archipelago?
Archipelago is a multi-game multiworld randomizer and as this works with it allows this game to be randomized and played in a multiworld with all other
[Archipelago supported games!](https://archipelago.gg/games).

# What gets randomized?
Currently, as the demo only contains 3 levels, this randomizes the seeds and abilities obtained within
the demo's 3 levels, as well as the seeds and first hp upgrade in Helsia's shop.

# How do I generate my randomized game?
In order to generate a randomized game for Bat Boy you must download the most recent [`.apworld` release here](), and place it in an Archipelago 0.3.5 or newer
release in the `worlds` folder. The default path for this is `C:\ProgramData\Archipelago\worlds` so you should have `C:\ProgramData\Archipelago\worlds\BatBoy.apworld`. 
You can then download the [default YAML template](), modify it in a text editor to select your options, place it in the `Archipelago/Players` folder and run 
`ArchipelagoGenerate.exe` to generate your randomized game. This will output a zip folder in `Archipelago/output`. From there you can either upload this resulting 
zip to the [Archipelago website](), or run `ArchipelagoMultiServer.exe` and select the resulting zip. 

# How do I connect to the randomized game?
In order to connect your game to the randomized server download 
the latest release from the [releases page](/releases) and place the `BepInEx` folder in your Bat Boy Demo game folder (probably 
`C:\Program Files (x86)\Steam\steamapps\Common\Bat Boy Demo` which you can also reach by right clicking the game in  your steam library and going to 
`Manage>browse local files`). Run the game and in the top left connection box enter the connection info to connect and play! `Host Name` is the server IP and port. 
If you are running the `Multiserver` locally, this is `localhost`, if you're connecting to a friend this will be `IP.Add.ress:38281` and if it's on the website, 
`archipelago.gg:<port>`. `Name` is the name field from the earlier yaml file (default `YourName1`), and `password` is optional, only required if the host set one.

# Issues
* Seeds picked up in levels only get sent when the level is cleared. If you exit a level without clearing it  
* Ability tutorials that trigger at the end of levels will still occur but may not be possible with your toolset.
The locations send before the tutorial starts so you can simply quit the tutorial to continue playing.
