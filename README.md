# Hitman: Absolution launcher bypass

This is a .exe patcher for Hitman Absolution that allows you to skip the launcher... Why this wasn't a feature already is beyond me.

## What it does

There's an unused property in the .exe called `EnableSplashScreen`, which is set to 1. This can be set to 0 and bypass it entirely. The app will also detect if the game's already patched.

## How to use it
0. Close the game if it's running
1. Download the latest version from the release, and extract the .zip
2. Run `HMALauncherBypass-v1.0.exe`
3. Follow the prompts (it will try to auto-detect the .exe on a steam copy)
4. Launch the game when finished

## "But i don't trust a random .exe / random person on the internet"
That is fair. Here's how you do it manually:

1. Download a hex editor, for this we'll use [HxD]()
2. Make a backup of the .exe
3. Open `HMA.exe` in `HxD`
4. Press `Ctrl + G` and enter `A7AE3A` in the offset field
5. It should take you to a value of `31`. Change ONLY that value to `30`
6. Save and run the game. That should skip the splash screen

## Build info

It's a simple C# console app, it should be easy enough to build.
1. Clone the repo
2. Navigate to the cloned folder
3. Run `dotnet build -c Release`
