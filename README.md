# ADOFAI Modding Helper
Unity package made for ADOFAI mod developers working alongside Thunderkit. This package streamlines some process in mod development. **Make sure to read the instructions to properly use this package!**

The package "ADOFAI Runner" has also been merged with this package, so make sure to remove the "ADOFAI Runner" package before adding this.
***
## Features
* **Mod Info** - A Unity asset that is used to create the mod Info
    - Currently supported mod managers: BepInEx & UnityModManager.
* **Some extra stuff regarding ThunderKit** - This package comes with an option to create a preset Pipeline + Manifest of ThunderKit, that I normally use for my mods.
***
## Setup
1. Have a Unity Editor modding environment with ThunderKit
    - Make sure you have a Unity Editor modding environment with ThunderKit first before proceeding
    - You can follow this guide to set it up: TBA
2. Install ADOFAI Runner
    - ## Package Manager
        - Head over to the package manager in your Unity Project. Hit the plus top left of the panel, and click "Add package from git URL". 
         ![PMGit](./Editor/ADOFAIRunner/Images/AddPackageGit.png)
        - Then paste `https://github.com/JofoDuh/ADOFAIModdingHelper.git` into the field and click add afterwards.
         ![GitLink2](./Images/GitLink2.png)
***
## Usage
* **Create Mod Template (Ctrl + Shift + M)** - Create an ADOFAI Mod Template. ![CreateMod](./Images/CreateMod.png)
* **Mod Info** - A Unity asset used to generate information of the mod. ![ModInfo](./Images/ModInfo.png)
    - Mod Info UMM (Unity Mod Manager)
        - **Assembly Name** - The name of the assembly, usually is just named after the assembly definition + dll.
        - **Entry Method** - The method that Unity Mod Manager will execute upon loading the mod.
    - Mod Info BepInEx
        - **GUID** - A unique identifer for the mod
        - **BIP Mod Info CS Path** - The mod BepInEx template will contain a .cs file that contains information regarding the mod. Since BepInEx doesn't want data-driven mod info unlike UMM with its Info.json practice, I had to resort to manually editing the .cs file before compilation, don't worry, this is including inside the process chain of ADOFAI Runner. This field is also auto assigned when creating the mod template. ![BIPModInfo](./Images/BepInExModInfo.png)
# ADOFAI Runner
Unity Editor extension for ADOFAI mod developers working alongside Thunderkit.
**Make sure to read the instructions to properly use this package!**
***
## Features
* **Symbol Definer** - Help define build symbol for different compiled build. Use to quickly switch between build to compile for different mod managers.
    - Currently supported mod managers: BepInEx & UnityModManager.
* **Run** - Choose the corresponding mod added in the settings and click run -> 
    - Compile everything by executing ThunderKit's pipeline.
    - Move all the needed file to a mod folder named based off of the pipeline's manifest identity name.
    - Execute the game automatically.
* **FRun** - Quickly launch the executable based off of the chosen build.
***
## Configurations
- Once the package finish importing. Set up ADOFAI Runner's settings by clicking on the setting icon on the toolbar, that should appear top left after the package finish importing: ![ToolBar](./Editor/ADOFAIRunner/Images/ToolBar.png)
***
## Setting up ADOFAI Runner's settings
  - ## General
      - ThunderKit Export Path
          - The folder that ThunderKit compiles by default to. The folder should have some folders already inside depending on what you've done. "Libraries" folder should exist if you've compiled your mod assembly before and the "AssetBundleStaging" should exist if you've compiled assets bundle with ThunderKit before. Once you've pinpointed the folder, input the path to this field.
      - Include PDB File 
          - Just includes the PDB file when moving for debugging purposes.
      - Mod List 
          - Here you can add all the ThunderKit pipelines and your mod info for all the mods you are currently developing in the Unity Project modding environment.
      ### Example: ![General](./Editor/ADOFAIRunner/Images/GeneralSetting.png)
  - ## UMM & BepInEx
      - Mod Path: Just the folder within your game directory of the corresponding mod loader, that holds all the different mod folders.
      - Game Executable: The ADOFAI executable path of the corresponding mod loader.