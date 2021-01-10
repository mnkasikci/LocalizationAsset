# Scene Load
Before you load any scene in the game, you need to make sure that the language files are loaded and `Localization Manager` is ready to give out localized values . Otherwise, if the localization files are not loaded properly, the objects in the scene will not be able to retrieve the localized values.

What you need to do is to check if `Localization Manager` is ready before you load the scene in another script. This is typically done by a `Game Manager` script that continuously checks if the `Localization Manager` is ready and loads the scene only when it is. For this purpose, `Localization Manager` has an `IsReady` property that will return true once it is ready.

You can use an if statement like below to check if the `Localization Manager` is ready:
> if(LocalizationManager.Instance.IsReady == true) <br />
> { <br />
> // Load the scene here <br />
> }

See the demo for a sample implementation of this system.