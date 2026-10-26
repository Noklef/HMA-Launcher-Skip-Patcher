# Hitman: Absolution launcher skip patcher

This is a .exe patcher for Hitman Absolution that allows you to skip the launcher... Why this wasn't a feature already is beyond me.

## What it does

There's an unused property in the .exe called `EnableSplashScreen`, which is set to 1. This can be set to 0 to skip the launcher entirely. The app will also detect if the game's already patched.

## How to use it
0. Close the game if it's running
1. Download the latest .exe version from the release
2. Run it, and follow the prompts (it will try to auto-detect the .exe on a steam copy)
4. Launch the game when finished

## "But i don't trust a random .exe / random person on the internet"
That is fair. Here's how you do it manually:

1. Close the game and launcher, then download [HxD](https://mh-nexus.de/en/hxd/)
2. Make a backup of `HMA.exe`
3. Open `HMA.exe` in `HxD`
4. Press `Ctrl + F`, select **Text-string**, and search for `EnableSplashScreen=1`
5. Confirm there is only one matching setting and that it is followed by a line break and `BOOT_MOVIE=`. If the setting is already `EnableSplashScreen=0`, it is already patched
6. In **Overwrite** mode, change only the final `1` to `0` in the decoded text pane. In hex, this changes the value byte from `31` to `30`; do not insert or delete bytes
7. Save and run the game. That should skip the splash screen

To undo the change, set `EnableSplashScreen=0` back to `EnableSplashScreen=1`, or restore your backup. Search for the setting rather than using a fixed offset, which can differ between executables.

## Build info

It's a simple C# console app, it should be easy enough to build.
1. Clone the repo
2. Navigate to the cloned folder
3. Run `dotnet build -c Release`
