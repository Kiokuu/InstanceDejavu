# InstanceDejavu
A VRChat [MelonLoader](https://github.com/LavaGang/MelonLoader) mod which adds an icon to instances you have visited before.

## Features
* Display a icon on instances you have visited before (in the current session).

## Compatability
* Currently working on build 1160
* This mod was made for Melonloader 0.5.3

## Installation
* Download the [latest release](https://github.com/Kiokuu/InstanceDejavu/releases/latest) of the compiled DLL and place into the "VRChat/Mods" folder.

## Building
To build this mod, reference the following libraries from MelonLoader/Managed after assembly generation;
* Assembly-CSharp.dll
* Il2Cppmscorlib.dll
* UnhollowerBaseLib.dll
* UnityEngine.CoreModule.dll
* UnityEngine.ImageConversionModule.dll
* UnityEngine.UI.dll
* VRCCore-Standalone.dll

Additionally, reference the following library;
* MelonLoader.dll (from MelonLoader base directory)

Ensure that the Resources folder is referenced, and "visited.png" is marked as an embedded resource.

Finally, build in your favourite IDE.
