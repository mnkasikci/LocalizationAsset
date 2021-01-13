# How Localization Works

This will be a primer on how a localization system works in general so you can get the most out of Quick Localization.

## Language Files
For each language you will need a file that contains the text in that language. For instance, you might have a file for English and another file for German. The important thing is that you keep translations separate and organize them by language. You do not mix English and German translations in a single file. 

##  Localization Dictionaries
Dictionaries hold key and value pairs. For instance, the key can be "Warm Hello" and the value can be "Hi friend, how are you doing?" This allows you to give the key to the dictionary and request the corresponding value.

Dictionaries are widely used in localization. Each language file acts essentially like a dictionary and the keys are the same throughout all language files. This data structure allows you to use the same key with different language files to retrieve the localized version in the intended language.

See the below example:

|Language File|Key|Value|
|--|--|--|
|English|Warm Hello|Hi friend, how are you doing?|
|German|Warm Hello|Hallo Freund, wie geht es dir?|

## Loading Language Files
There is something special with loading language files. You need to make sure that the language files are loaded and ready to give out values before you load any scene in your game. Otherwise, if the localization files are not loaded properly, the objects in the scene will fail to obtain the localized texts and the result will be localization chaos.

This is why any localization asset should alert you when it loaded the language files and that you can proceed with loading your scenes.