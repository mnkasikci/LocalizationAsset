using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class LocalizationManager : MonoBehaviour
{
    #region Singleton
    public static LocalizationManager Instance { get; private set; }

    void Awake()
    {
        transform.parent = null;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    #endregion

    #region Fields
    [SerializeField] private bool debugging = default;

    [Header("Language Selection")]
    [Tooltip("Enter the language tag from ISO 639-1 classification. See here for " +
        "the tags: http://docwiki.embarcadero.com/RADStudio/Sydney/en/Language_Culture_Names,_Codes,_and_ISO_Values. " +
        "For instance, use 'en-US' for American English and 'en-GB' for British English. " +
        "Remember that a language file with the exact same name should exist in the languages folder.")]
    [SerializeField] private string mainLanguageTag;


    private string chosenCultureName;
    [Header("File Paths")]
    [Tooltip("Language file will be retrieved from this folder. The path is relative to your Assets " +
        "folder. So 'Languages' means 'Assets/Languages'. Do not start the path with '/' because" +
        "then the path will not be relative to your Assets folder.")]
    [SerializeField] private string LanguageFolder;
    private string fullLanguageFolder;
    
    [Tooltip("When another script requests the localized text for a key and Localization Manager cannot find that value," +
        "it will log these keys to this folder so that you can see which keys were missing / not translated for that language.")]
    [SerializeField] private string MissingKeysFolder;
    private string fullMissingKeysFolder;
    

    private LocalizationHandler localizationHandler = new LocalizationHandler();
    private List<CultureData> allCultures;

    public HashSet<string> allKeys = new HashSet<string>();
    private int currentLanguageIndex = -1, systemLanguageIndex = -1, mainLanguageIndex = -1;

    [HideInInspector]
    public UnityEvent OnLanguageChanged = default;
    
    public bool IsReady { get; private set; }
    #endregion;

    #region Create New Language File With Used Keys
    [MenuItem("Tools/Localization/Create New Language File with Used Keys (Play Mode Only)")]
    public static void CreateNewLanguageFileWithUsedKeys()
    {
        string filePath = Path.Combine(Instance.fullMissingKeysFolder, "EmptyDictionary.json");
        LocalizationData localizationData = new LocalizationData { items = new List<LocalizationItem>() };
        foreach (var key in Instance.allKeys) localizationData.items.Add(new LocalizationItem() { key = key, value = "" });
        string jsonString = JsonUtility.ToJson(localizationData);
        File.WriteAllText(filePath, jsonString);
    }

    [MenuItem("Tools/Localization/Create New Language File with Used Keys", true)]
    private static bool ValidateCreateNewLanguageFileWithUsedKeys()  => Application.isPlaying;
    #endregion

    #region Methods
    private bool AnyLanguageIsUnavailable() =>
        (   allCultures[currentLanguageIndex].IsBroken ||
            allCultures[systemLanguageIndex].IsBroken ||
            allCultures[mainLanguageIndex].IsBroken     );

    public async void Start()
    {
        
        await StartupLanguageActionsAsync();
        IsReady = true;
    }

    public async Task StartupLanguageActionsAsync()
    {
        fullLanguageFolder = Path.Combine(Application.dataPath, LanguageFolder);
        fullMissingKeysFolder= Path.Combine(Application.dataPath, MissingKeysFolder);

    GetAvailableLanguageFiles();
        
        try
        {
            await ChooseLanguagesAsync();
        }
        catch
        {
            while (AnyLanguageIsUnavailable()) {  await ReConstructLanguages();  }
        }
        localizationHandler.ConstructPriorityList();

        if (Instance.debugging)
        {
            Debug.Log("Localization Completed");
            Debug.Log("cl = " + allCultures[currentLanguageIndex].culture.Name);
            Debug.Log("dl = " + allCultures[systemLanguageIndex].culture.Name);
            Debug.Log("ml = " + allCultures[mainLanguageIndex].culture.Name);
        }
    }

    /// <summary>
    /// If this method is being called, it means one of the language files is broken
    /// </summary>
    private async Task ReConstructLanguages()
    {
        if(allCultures[mainLanguageIndex].IsBroken)
            for(int i =0;i<allCultures.Count;i++)
                if(!allCultures[i].IsBroken)
                {
                    mainLanguageIndex = i;
                    if (Instance.debugging) Debug.LogWarning("Because the set main language is broken," +
                        " now attempting to set " +allCultures[i].culture.Name + " as the main language");
                    await MakeLanguageAvailable(i);
                    return;
                }
        if (allCultures[mainLanguageIndex].IsBroken)
                throw new LocalizationException("All of the language files were corrupted." +
                    "Please check the languages and restart the application", fullLanguageFolder);
        if (allCultures[systemLanguageIndex].IsBroken)
        {
            if (Instance.debugging) Debug.LogWarning("Because the set system language (" + allCultures[systemLanguageIndex].culture.Name + 
                ") is broken, now setting main language (" +allCultures[mainLanguageIndex].culture.Name+ ") as the system language.");
            systemLanguageIndex = mainLanguageIndex;
            return;
            
        }
        if (allCultures[currentLanguageIndex].IsBroken)
        {
            
            if (Instance.debugging) Debug.LogWarning("Because the set chosen language (" + allCultures[currentLanguageIndex].culture.Name +
                ") is broken, now setting system language (" + allCultures[systemLanguageIndex].culture.Name + ") as the chosen language" +
                "and refreshing the language choices.");
            currentLanguageIndex = systemLanguageIndex;
            Instance.chosenCultureName = "";
        }
    }

    private async Task<bool> SaveAllMissingKeys()
    {
        if (allCultures == null) return false;
        Directory.CreateDirectory(fullMissingKeysFolder);

        List<Task> filesToWrite = new List<Task>();

        foreach (var item in allCultures)
        {
            HashSet<string> missingKeys = item.MissingKeys;
            if (missingKeys == null) continue;

            LocalizationData localizationData = new LocalizationData();
            localizationData.items = new List<LocalizationItem>();
            foreach (var key in missingKeys)
                localizationData.items.Add(new LocalizationItem() { key = key, value = "" });

            string jsonString = JsonUtility.ToJson(localizationData);
            string missingKeysPath = Path.Combine(fullMissingKeysFolder, item.culture.Name + ".json");
            File.WriteAllText(missingKeysPath, jsonString);

            StreamWriter sw = new StreamWriter(missingKeysPath);
            filesToWrite.Add(sw.WriteAsync(jsonString));
        }
        await Task.WhenAll(filesToWrite);

        return true;
    }

    private async void OnApplicationQuit()
    {
        await SaveAllMissingKeys();
    }
    /// <summary>
    /// Checks whether main, system and current langauge files are broken and reconstructs the choices.
    /// <para>If it is impossible to reconstruct, throws an error.</para>
    /// </summary>
    public async Task ChooseLanguagesAsync()
    {
        try
        {
            if (!allCultures.Exists(c => c.culture.Name == mainLanguageTag))
                throw new LocalizationException("Couldn't find main language (" + mainLanguageTag + ".json)\n" + "First available language will be set as the main language", fullLanguageFolder);
            mainLanguageIndex = allCultures.FindIndex(c => c.culture.Name == mainLanguageTag);

            if (Instance.debugging) Debug.Log("Language " + allCultures[mainLanguageIndex].culture.Name + " was successfully found");
        }
        catch
        {
            mainLanguageIndex = 0;
            if (Instance.debugging) Debug.Log("Language " + allCultures[mainLanguageIndex].culture.Name + " was set as main language ");
        }

        systemLanguageIndex = GetClosestToSystemLanguage();
        if (systemLanguageIndex == -1) systemLanguageIndex = mainLanguageIndex;
        if (PlayerPrefs.HasKey("chosenLanguage"))
        {
            chosenCultureName = PlayerPrefs.GetString("chosenLanguage");
        }
        string temp = chosenCultureName;
        currentLanguageIndex = temp == "" ? systemLanguageIndex : allCultures.FindIndex(c => c.culture.Name == temp);

        if (currentLanguageIndex == -1)
        {
            if (Instance.debugging)
                Debug.LogWarning("Since the preferred language couldn't be found, the preferred language is refreshed");
            chosenCultureName = "";
            currentLanguageIndex = systemLanguageIndex;
        }

        await MakeLanguageAvailable(mainLanguageIndex, systemLanguageIndex, currentLanguageIndex);
    }


    private async Task ChooseLanguage(int newLanguageIndex)
    {
        Task MLA = MakeLanguageAvailable(newLanguageIndex);
       
        try
        {
            await MLA;
            currentLanguageIndex = newLanguageIndex;
            localizationHandler.ConstructPriorityList();
            chosenCultureName = allCultures[newLanguageIndex].culture.Name;
            PlayerPrefs.SetString("chosenLanguage", chosenCultureName);


            OnLanguageChanged.Invoke();
            
            
        }
        catch
        {
            Debug.LogError("The langauge file is broken and couldn't be loaded. Language was not changed ");
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> GetAvailableLanguages()
    {
        Dictionary<string, string> names = new Dictionary<string, string>();
        foreach (var item in allCultures)
            names.Add(item.culture.NativeName, item.culture.Name);

        return names;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="languageTag"></param>
    /// <returns></returns>
    public async Task ChooseLanguage(string languageTag)
    {
        int index = allCultures.IndexOf(allCultures.Find(f => f.culture.Name == languageTag));
        if (index == -1) Debug.LogError("The given culture name could not be found in the available languages.");

        await ChooseLanguage(index);
    }

    private async Task MakeLanguageAvailable(params int[] Indexes)
    {
        List<Task> DictionaryLoad = new List<Task>();

        foreach (var index in Indexes.Distinct())
        {
            if (allCultures[index].IsBroken)
            {
                if (Instance.debugging)
                {
                    Debug.LogWarning("Can't load language " +
                    allCultures[index].culture.Name +
                    " because it is marked as broken. Will attempt to load other languages\n" +
                    " To use this language please check the language file and restart the application");
                }
                continue;
            }
            if (allCultures[index].IsLoadingOrLoaded == false)
            {
                allCultures[index].IsLoadingOrLoaded = true;
                DictionaryLoad.Add(allCultures[index].LoadDictionary());
            }
        }
        await Task.WhenAll(DictionaryLoad);
    }

    public async void Callbackmethod()
    {
        await Task.Run(() => { });
    }

    /// <summary>
    /// Returns the closest available language to system language. 
    /// <para>i.e If the system language is en-US, tries to find en-US , en, en-others respectively</para> 
    /// <para>i.e If the system language is en, tries to find en, en-others respectively</para> 
    /// <para>if can't find any language close to system language, returns -1 </para> 
    /// </summary>
    /// <returns></returns>
    private int GetClosestToSystemLanguage()
    {
        CultureInfo cur = CultureInfo.CurrentCulture;


        for (int i = 0; i < allCultures.Count; i++)
            if (allCultures[i].culture == cur)
                return i;
        for (int i = 0; i < allCultures.Count; i++)
            if (allCultures[i].culture == cur.Parent ||
                Application.systemLanguage.ToString() == allCultures[i].culture.EnglishName
                )
                return i;
        for (int i = 0; i < allCultures.Count; i++)
            if (allCultures[i].culture.Parent == cur.Parent ||
                Application.systemLanguage.ToString() == allCultures[i].culture.Parent.EnglishName)
                return i;

        return -1;
    }

    public void GetAvailableLanguageFiles()
    {
        DirectoryInfo directoryinfo = new DirectoryInfo(fullLanguageFolder);
        List<FileInfo> languageFiles = directoryinfo.GetFiles()
            .Where(f => f.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            .Select(f => f)
            .ToList();

        allCultures = new List<CultureData>();
        foreach (var lf in languageFiles)
        {
            
            CultureInfo tempCultureInfo = CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(lf.FullName));
            allCultures.Add(new CultureData() { culture = tempCultureInfo, filePath = lf.FullName });
        }

        if (allCultures.Count == 0)
            throw new LocalizationException("Couldn't find any languages", fullLanguageFolder);
    }

    List<object> GetDynamicParts(DynamicVariables dynamicParts)
    {
        List<object> DynamicValues = new List<object>();
        foreach (var dynamicPart in dynamicParts.list)
        {
            object temp;
            try
            {
                temp = DataFinder.GetData(dynamicPart.variableSource, dynamicPart.variableName);
            }
            catch
            {
                temp = "";
            }
            DynamicValues.Add(temp);
        }
        return DynamicValues;
    }

    public string GetLocalizedValue(string key, params object[] variables)
    {
        allKeys.Add(key);
        string rawTranslation = localizationHandler.GetRawTranslation(key);
        if (variables.Length == 0) return rawTranslation;
        List<object> DynamicValues = variables.ToList();

        return TurnDynamicsIntoText(rawTranslation, DynamicValues);
    }

    /// <summary>
    /// Do not use this method manually. This is intended for the localization via LocalizeUIText component.
    /// </summary>
    public string LocalizeThroughComponent(string key, DynamicVariables dp)
    {
        allKeys.Add(key);
        string rawTranslation = localizationHandler.GetRawTranslation(key);
        if (dp.list.Count == 0) return rawTranslation;
        List<object> DynamicValues = GetDynamicParts(dp);

        return TurnDynamicsIntoText(rawTranslation, DynamicValues);
    }

    /// <summary>
    /// Do not use this method manually. This is intended for the localization via LocalizeUIText component.
    /// </summary>
    public string LocalizeThroughComponent(string key, DynamicVarsForCode dp)
    {
        allKeys.Add(key);
        string rawTranslation = localizationHandler.GetRawTranslation(key);
        if (dp==null || dp.dynamicVariables.Count == 0) return rawTranslation;

        return TurnDynamicsIntoText(rawTranslation, dp.dynamicVariables);
    }

    private string TurnDynamicsIntoText(string textwithNumbers, List<object> DynamicValues)
    {
        if (DynamicValues.Count == 0) return textwithNumbers;
        int PlaceHolders = 0;
        int startIndex = 0, checkforDynamicEndIndex = 0;
        StringBuilder s = new StringBuilder();

        bool[] IsReplaced = new bool[100]; // REVIEW: Should confirm it is filled with false

        while (checkforDynamicEndIndex < textwithNumbers.Length)
        {
            checkforDynamicEndIndex = textwithNumbers.IndexOf('{', startIndex);
            if (checkforDynamicEndIndex == -1)
            {
                if(PlaceHolders<DynamicValues.Count)
                    if(Instance.debugging)
                        Debug.LogError("There were "+PlaceHolders+" placeholders in the value but you provided "
                        +DynamicValues.Count +  " parameters. The rest of the parameters will be disregarded.");
                break; 
            }

            var result = NumberInsideListParanthesis(textwithNumbers, checkforDynamicEndIndex);

            if (result.Item1 == true)
            {
                PlaceHolders++;
                if (result.Item2 == -1)
                {
                    startIndex = checkforDynamicEndIndex = result.Item3; continue;
                }
                if (result.Item2 >= DynamicValues.Count)
                {
                    if (Instance.debugging)
                            Debug.LogError("The number inside the placeholder is " + result.Item2 + " but you provided only " +
                            DynamicValues.Count + " arguments (placeholder numbers start with 0 and must be smaller than " +
                            "number of arguments). This placeholder will be left empty");
                    startIndex = checkforDynamicEndIndex = result.Item3;
                    continue;
                }

                s.Append(textwithNumbers.Substring(startIndex, checkforDynamicEndIndex - startIndex));
                string toAdd = GetDynamicLocal(DynamicValues[result.Item2]);
                s.Append(toAdd);
                IsReplaced[result.Item2] = true;
                startIndex = checkforDynamicEndIndex = result.Item3;
            }
            else
                break; // Already logged in NumberInsideListParanthesis method

        }
        s.Append(textwithNumbers.Substring(startIndex, textwithNumbers.Length - startIndex));

        return s.ToString();
    }

    public static string GetDynamicLocal(object v)
    {
        if (v is int) return ((int)v).ToString(Instance.allCultures[Instance.currentLanguageIndex].culture.NumberFormat);
        if (v is double) return ((double)v).ToString(Instance.allCultures[Instance.currentLanguageIndex].culture.NumberFormat);
        if (v is DateTime) return ((DateTime)v).ToString(Instance.allCultures[Instance.currentLanguageIndex].culture.DateTimeFormat);
        return v.ToString();
    }

    /// <summary>
    /// Result's <para> Item1 corresponds to whether successfully located right paranthesis</para>
    /// <para> Item2 corresponds to the number inside of the paranthesis (-1 if invalid number) </para>
    /// <para> Item3 corresonds to the string index after '}' </para>
    /// </summary>
    private Tuple<bool, int, int> NumberInsideListParanthesis(string s, int index)
    {
        int endIndex = s.IndexOf('}', index);
        if (endIndex == -1) { if (Instance.debugging) Debug.LogError("Couldn't find a '}' after the number."); return Tuple.Create(false, 0, 0); }
        if (endIndex > index + 2) { if (Instance.debugging) Debug.LogError("Unsupported number inside the placeholder."); return Tuple.Create(true, -1, endIndex + 1); } 

        int numberInside = 0;

        for (int i = index + 1; i < endIndex; i++)
        {
            char c = s[i];
            if (c > '9' || c < '0') { if (Instance.debugging) Debug.LogError("Unsupported number inside the placeholder."); return Tuple.Create(true, -1, endIndex + 1); }
            numberInside += c - '0';
            numberInside *= 10;
        }
        numberInside /= 10;

        return Tuple.Create(true, numberInside, endIndex + 1);

    }
    #endregion
    
    public class LocalizationHandler
    {
        List<int> PriorityList;

        /// <summary>
        /// Constructs or re-constructs language priority list.<para>Language Indexes should be checked before this method is called here</para>
        /// </summary>
        public void ConstructPriorityList()
        {
            PriorityList = new List<int>();
            AttemptAdd(Instance.currentLanguageIndex);
            AttemptAdd(Instance.systemLanguageIndex);
            AttemptAdd(Instance.mainLanguageIndex);
        }

        private void AttemptAdd(int index)
        {
            if (!Instance.allCultures[index].IsBroken && !PriorityList.Contains(index))
                PriorityList.Add(index);
        }

        internal string GetRawTranslation(string key)
        {
            foreach (var langIndex in PriorityList)
            {
                try { return Instance.allCultures[langIndex].GetValue(key); }
                catch { }
                
            }
            return "";
        }
    }

    [Serializable]
    public class CultureData
    {
        public CultureInfo culture;
        private Dictionary<string, string> dictionary;
        private HashSet<string> missingkeys;
        public string filePath;
        public bool IsLoadingOrLoaded = false;

        private bool isBroken = false;
        public bool IsBroken { get => isBroken; }

        public override string ToString() => culture.NativeName;
        private void AddMissingKey(string key)
        {
            if (this.missingkeys == null) this.missingkeys = new HashSet<string>();
            this.missingkeys.Add(key);
        }
        internal HashSet<string> MissingKeys { get => missingkeys; }

        public async Task LoadDictionary()
        {
            if (dictionary != null) return;

            if (Instance.debugging) Debug.Log("Loading " + this.culture.Name);
            this.dictionary = new Dictionary<string, string>();

            if (File.Exists(this.filePath))
            {
                try
                {
                    StreamReader sr = new StreamReader(this.filePath);
                    string dataasJson = await sr.ReadToEndAsync();

                    LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(dataasJson);
                    foreach (var item in localizationData)
                    {
                        try { this.dictionary.Add(item.key, item.value); }
                        catch (ArgumentException e) { if(Instance.debugging) Debug.LogWarning(e.Message + " this entry will be disregarded"); }
                    }
                }

                catch (Exception e)
                {
                    this.isBroken = true;
                    throw new LocalizationException("Problem loading localization file", this.filePath, e);
                }
                if (Instance.debugging) Debug.Log("Data loaded, dictionary "+ this.culture.Name+" has " + this.dictionary.Count + " entries.");
            }
            else
                throw new LocalizationException("Localization file not found", this.filePath);
        }

        internal string GetValue(string key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            if (Instance.debugging)
            {
                Debug.LogWarning("The key \"" + key + "\" couldn't be found  in the language dictionary \"" +
                    this.culture.Name + "\". It will be added to the missing keys");
            }
            throw new KeyNotFoundException();
        }
    }

    public static LocalizeUIText AddLocalizeUIText(GameObject g, string key, params object[] DynamicParts)
    {
        
        if (g == null || key == null)
        {
            Debug.LogError("You must provide a game object and string which are not null");
            return null;
        }
        if (g.GetComponent<LocalizeUIText>() != null) return null;

        g.SetActive(false);

        LocalizeUIText lt = g.AddComponent<LocalizeUIText>();

        lt.SetKey(key);
        lt.SetCodeCreated();
        if (DynamicParts.Length != 0) lt.SetDynamicVariables(DynamicParts.ToList());
        
        g.SetActive(true);

        return lt;
    }
}