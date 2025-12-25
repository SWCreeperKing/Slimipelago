Help support me, this and future projects on [Patreon](https://www.patreon.com/c/SW_CreeperKing)

---
### DISCLAIMER: THIS MOD WILL >>NOT<< GIVE YOU THE PAID DLC FOR FREE

---
<details>
<summary><h1 style="display: inline"> How to install</h1></summary>

(tutorial totally not copy and pasted from Tunic AP mod and BTD6 Mod helper)

1. Make sure to have [.Net6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed
2. Download and Install [Melon Loader](https://melonwiki.xyz/#/?id=automated-installation).
    - The default Slime Rancher install directory (for steam): C:\Program Files (x86)\Steam\steamapps\common\Slime
      Rancher
    - Recommended melon version: 7.1
3. Launch the game and close it. This will finalize the Melon installation.
4. Download and extract the `Slimipelago.zip` from
  the [latest release page](https://github.com/SWCreeperKing/Slimipelago/releases).
    - Copy the `Mods` and `UserLibs` folders from the zip into the game's directory.
5. Launch the game again and you should see no new or load game options!
    - There should be the archipelago connection menu in the options
6. To uninstall the mod, either remove/delete the `Mods/SW_CreeperKing.Slimipelago` folder

> [!Note]
> This mod will most likely not work with the SRModLoader
> Double/triple/quadruple check the mod folder, melon might rename it to `~SW_CreeperKing.Slimipelago`
> which will prevent the mod from loading correctly

</details>

---
<details>
<summary><h1 style="display: inline"> Randomizer Information</h1></summary>

<h3>[Logic Spread Sheet](https://docs.google.com/spreadsheets/d/15PdrnGmkYdocX9RU-D5U_9OgihRNN9axX71mm-jOPUQ)</h3>

---
<details>
<summary><h3 style="display: inline">What is Randomized</h3></summary>

- Personal Upgrades
- 7Zee Rewards
  - Yaml Option (default off)
  - The 3 upgrades' locations will be checked when buying the reward
- Map Fragments
  - Map regions still unveil when interacting
- Treasure Pods
  - Treasure pod rewards are still given
  - DLC Style Treasure pods (if you own the dlc and the yaml setting is on)
- Hobson's Notes

</details>

---
<details>
<summary><h3 style="display: inline">Features</h3></summary>

- Saves are handled differently 
  - You cannot make or load a save until connecting
  - Only saves that are made with the same seed appear
- Disables Tutorials
- Disables Vanilla Popups
- Custom Map Markers for most locations
  - Darkened: Out of Logic
  - Not Darkened: In Logic
  - Yellow: Hinted
  - the marker `|>|>` is a fast travel marker
    - will only appear if you open their respective slime gate, except for reef
- Entering an area you aren't supposed to be teleports you back to the ranch
- All buildings *should* refund all their costs when demolishing
- Drones do not need water
- Extractors stay for inf cycles
- AirNets take no damage
- Traps & TrapLink see [Traps.md](https://github.com/SWCreeperKing/Slimipelago/blob/master/Traps.md)
- DeathLink

</details>

---
<details>
<summary><h3 style="display: inline">Upgrade Requirements</h3></summary>

| Upgrade            | Prev. Upgrade Req | In-Game Hours Passed |
|:-------------------|:-----------------:|---------------------:|
| Run Efficency      |       None        |                   48 |
| Air Burst          |       None        |                   72 |
| Jetpack Efficiency |      Jetpack      |                  120 |
| Health lv.2        |    Health lv.1    |                   48 |
| Health lv.3        |    Health lv.2    |                   72 |
| Energy lv.2        |    Energy lv.1    |                   48 |
| Energy lv.3        |    Energy lv.2    |                   72 |
| Ammo lv.2          |     Ammo lv.1     |                   48 |
| Ammo lv.3          |     Ammo lv.2     |                   72 |

Treasure Cracker Upgrades require the `Region Unlock: The Lab`

- You will also need fabricated gadgets if you haven't already
    - lv.1 requires 1 gadget crafted
    - lv.2 requires 20 gadgets crafted
    - lv.3 requires 50 gadgets crafted

</details>

---
<details>
<summary><h3 style="display:inline">Music Rando</h3></summary>

Inorder to refresh the music options the game must be restarted
the music rando folders only appear after the first launch with the mod

the folders for music rando is first made into areas then time of day (`day`/`night`/`both`)

music in the `Any` region folder can appear in any region

Tarr music is special as it doesn't take from the `Any` region, nor does any region take from the Tarr folder

</details>

</details>

---
<details>
<summary><h1 style="display: inline">FaQ</h1></summary>

<h3>WIP</h3>

</details>

---
<details>
<summary><h1 style="display: inline">Known Issues</h1></summary>

- <details>
  <summary><h3 style="display: inline">Audio starts to die after some time</h3></summary>
  
    > Description: The longer you play the more some sounds start to not play or get shortened
  
    > Cause: Unknown  
  
    > Solution: Quit to Main Menu and re-enter save 
  </details>

</details>

---
### Special Thanks

- Sterlia - Logic Slave
- nv_ious - Initial Location names and logic

- Redriel - APSR.png
- Nônimo - APSR_Got_Trap.png

- Slime Rancher Wiki
    - APSR_Progressive.png
    - APSR_Trap.png
    - APSR_Useful.png

### Tools:

- Melon Loader (obv)
- Rider
- UnityExplorer