# Overview

Quick Localization is a Unity asset that eases the localization process for you with a *minimalistic* and *practical* approach. You can purchase the asset from  [Unity Asset Store](https://www).

See [Rest in Joy's YouTube channel](https://www.youtube.com/channel/UCDjGcerrcQBK0wMLlT3sovQ) for a video tutorial on using this asset as well as many other tutorials on game development and Unity.

## Features
- **Simple and Intuitive**: The asset implements the well-known key and value pairs system to store and retrieve localized texts.
- **Custom Localization Editor Window**: Language files can be created and edited through a custom editor window that comes with the asset. Localization Editor allows you to add, edit, or remove localization items (key / value pairs) via a graphical user interface.
- **System Language Detection**: The asset detects the system language on the first run and tries to retrieve the localization file for that language. If that fails, the game will load with the main language specified by the developer. 
- **Fallback Languages**: If the asset cannot find the translation for a specific key in the player's chosen language, it will return the translation in the system language. If that also fails, it will return the translation in the main language specified by the developer.
- **Remembering Chosen Langauge**: Any language chosen by the player in the game is automatically saved in the `PlayerPrefs` file and remembered when the game runs again.
- **Localization without Coding (via Inspector)**: The asset comes with a `LocalizeUIText` component that can be added to any gameobject that has a Text or Text Mesh Pro component. The key and any variables in the localized text can be entered in the inspector without any coding at all.
- **Supports Static and Dynamic Texts**: In addition to static texts like "Hello world", the asset can localize dynamic texts like "Hello {0}, you have {1} gold". 
- **Debugging Missing and Used Keys**: You never need to  worry about any missing keys as the asset logs these to a file in a "Missing Keys" folder. You can also create an empty language file with all the keys used in the game.

## Core Components
- **Localization Manager**: Handles all localization-related tasks in the game.
- **Localization Editor**: Assists with creating and editing the language files.
- **Localize UI Text**: A component that is added to any GameObject and localizes the Text or Text Mesh Pro component of that GameObject based on the given key and variables (if any).