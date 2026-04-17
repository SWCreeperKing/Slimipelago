v0.3.0

- [ApWorld] Reworked the entirety of the logic (thank sterlia)
- [ApWorld] Changed how `treasure_cracker_checks` was checked
- [Client] Added 2 missing note locations
- [Client] You can no longer deposit quantum plorts for the Glass Desert teleporter if you don't have Glass Desert

v0.2.4

- [Client] Fixed a bug with DeathLink having delayed deaths
- [Client] Buffed Progressive Market Stonks from +5% to +20%
- [Client] Fixed a bug with 2 missing note locations
- [Client] Support for DeathLink Groups

---
v0.2.3

- [ApWorld] Minor fixes to logic and location names, there are 2 *possible* incompatible locations from worlds generated
  with v0.2.2 due to them previously having an extra space
    - Treasure Pod - Glass Desert Western Ruins
    - Treasure Pod - Glass Desert Leftside Isle
- [Client] (HOPEFULLY) Fixed save errors
- [Client] Extended timeout timer from 4s to 60s
- [Client] Extended the trap delay range from 30s-60s to 12s-60s
- [Client] Fixed a bug where reaching 7Zee lvl 28 would goal regardless of goal type
- [Client] Added version number
- [Client] Fixed Note Goal being susceptible to collect
- [Client] Custom Assets now loads on connect
- [Client] Traplink, added support for the following traps:
    - Banner, Chaser, Clear Image, Disarm, Explosion, Flip Horizontal, Flip Vertical, Frost, Gas, Help, Hey!, Inverted
      Mouse, Mana Drain, Ninja, Person, Reverse Controls, Sandstorm, Teleport, TNT, Tool Swap, Undo, Whirlpool, Zoom In,
      Zoom Out
- [Client] Forgot to implement the functionality of `Give random traps for unknown traps`
- [Client] Made the game tell you when trap linked, and if you have entered an area without an unlock
- [Client] Changed how the mod is structured slightly (in preparation for updating to melon 7.2)
    - IF YOU ARE UPDATING before running the game delete the `Slimipelago.dll` in the
      `Slime Rancher/Mods/SW_CreeperKing.Slimipelago` folder

---
v0.2.2

- [Client] Fixed a bug with options menu not being created correctly if you hadn't played v0.1 or any of the public
  betas
- [Client] Prevented 7zee goal from happening if all the locations were collected
    - this still happens with all notes goal, idk if i will fix/change it

---
v0.2.1

- [Client] Changed teleportation code slightly
- [Client] Changed popup code slightly
- [Client] Prevented traps from activating during time fast forward
- [Client] Deactivates traps when dying
- [Client] Can no longer go into house if a trap is active
- [Client] Changed Camera Flip trap active time from 10s to 20s
- [Client] Added more messages for text trap

---
v0.2

- [Client] Added DeathLink and TrapLink
- [Client] Added Trap effects
- [Client] Fixed a map tracker logic bug
- [Client] Making a new game now re-gives you all your items
- [Client] Intro cutscene no longer plays (had a polite conversation with it using a brick)
- [Client] Changed how AP info and settings are stored
- [Client] You no longer drop items on death
- [Client] Cleaned music rando code and added `Firestorm`
- [Client] Stopped AirNet from taking damage
- [ApWorld] Added credits goal
- [ApWorld] Added some missing locations
- [ApWorld] Fixed 7zee upgrades not respecting 7zee logic
- [ApWorld] Added Traps
- [ApWorld] Added Market Link item

---
v0.1

- Initial Release