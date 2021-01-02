﻿//TODO: culture name not recognized error
//TODO: what if this is broken, maybe choose another language from folder?

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
    [SerializeField] private string mainLanguageName;
    [SerializeField] private string chosenCultureName;
    [SerializeField] private string LanguageFolder;
    [SerializeField] private string MissingKeysFolder;
    
    private LocalizationHandler localizationHandler = new LocalizationHandler();
    private List<CultureData> AllCult;

    public HashSet<string> Allkeys = new HashSet<string>();
    private int currentLanguageIndex = -1, systemLanguageIndex = -1, mainLanguageIndex = -1;


    

    [HideInInspector]
    public UnityEvent OnLanguageChange = default;
    public bool LocalizationCompleted = false;

    #endregion;



    [MenuItem("Tools/Localization/Create Empty Dictionary")]
    public static void CreateEmptyDictionary()
    {

        string filePath = Path.Combine(Instance.MissingKeysFolder, "EmptyDictionary.json");
        LocalizationData localizationData = new LocalizationData { items = new List<LocalizationItem>() };
        foreach (var key in Instance.Allkeys) localizationData.items.Add(new LocalizationItem() { key = key, value = "" });
        string jsonString = JsonUtility.ToJson(localizationData);
        File.WriteAllText(filePath, jsonString);
    }
   
    #region Methods
    private bool AnyLanguageIsUnavailable() =>
        (   AllCult[currentLanguageIndex].IsBroken ||
            AllCult[systemLanguageIndex].IsBroken ||
            AllCult[mainLanguageIndex].IsBroken     );

    public async void Start()
    {

        await StartupLanguageActionsAsync();
        LocalizationCompleted = true;
    }
    public async Task StartupLanguageActionsAsync()
    {

        GetAvailableLanguages();
        
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
            Debug.Log("cl = " + AllCult[currentLanguageIndex].culture.Name);
            Debug.Log("dl = " + AllCult[systemLanguageIndex].culture.Name);
            Debug.Log("ml = " + AllCult[mainLanguageIndex].culture.Name);

        }

    }
    /// <summary>
    /// If this method is being called, it means one of the language index is -1
    /// </summary>
    private async Task ReConstructLanguages()
    {
        if(AllCult[mainLanguageIndex].IsBroken)
            for(int i =0;i<AllCult.Count;i++)
                if(!AllCult[i].IsBroken)
                {
                    mainLanguageIndex = i;
                    if (Instance.debugging) Debug.LogWarning("Because the set main language is broken," +
                        " now attempting to set " +AllCult[i].culture.Name + " as the main language");
                    await MakeLanguageAvailable(i);
                    return;
                }
        if (AllCult[mainLanguageIndex].IsBroken)
                throw new LocalizationException("All of the language files were corrupted." +
                    "Please check the languages and restart the application", LanguageFolder);
        if (AllCult[systemLanguageIndex].IsBroken)
        {
            if (Instance.debugging) Debug.LogWarning("Because the set system language (" + AllCult[systemLanguageIndex].culture.Name + 
                ") is broken, now setting main language (" +AllCult[mainLanguageIndex].culture.Name+ ") as the system language.");
            systemLanguageIndex = mainLanguageIndex;
            return;
            
        }
        if (AllCult[currentLanguageIndex].IsBroken)
        {
            
            if (Instance.debugging) Debug.LogWarning("Because the set chosen language (" + AllCult[currentLanguageIndex].culture.Name +
                ") is broken, now setting system language (" + AllCult[systemLanguageIndex].culture.Name + ") as the chosen language" +
                "and refreshing the language choices.");
            currentLanguageIndex = systemLanguageIndex;
            Instance.chosenCultureName = "";
        }
            


    }

    private async Task<bool> SaveAllMissingKeys()
    {
        if (AllCult == null) return false;
        Directory.CreateDirectory(MissingKeysFolder);

        List<Task> filesToWrite = new List<Task>();

        foreach (var item in AllCult)
        {
            HashSet<string> missingKeys = item.MissingKeys;
            if (missingKeys == null) continue;

            LocalizationData localizationData = new LocalizationData();
            localizationData.items = new List<LocalizationItem>();
            foreach (var key in missingKeys)
                localizationData.items.Add(new LocalizationItem() { key = key, value = "" });

            string jsonString = JsonUtility.ToJson(localizationData);
            string missingKeysPath = Path.Combine(MissingKeysFolder, item.culture.Name + ".json");
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


    public async Task ChooseLanguagesAsync()
    {

        try
        {
            if (!AllCult.Exists(c => c.culture.Name == mainLanguageName))
                throw new LocalizationException("Couldn't find main language (" + mainLanguageName + ".json)\n" + "First available language will be set as the main language", LanguageFolder);
            mainLanguageIndex = AllCult.FindIndex(c => c.culture.Name == mainLanguageName);

            if (Instance.debugging) Debug.Log("Language " + AllCult[mainLanguageIndex].culture.Name + " was successfully found");

        }
        catch
        {
            mainLanguageIndex = 0;
            if (Instance.debugging) Debug.Log("Language " + AllCult[mainLanguageIndex].culture.Name + " was set as main language ");
        }

        systemLanguageIndex = GetClosestToSystemLanguage();
        if (systemLanguageIndex == -1) systemLanguageIndex = mainLanguageIndex;

        string temp = chosenCultureName;
        currentLanguageIndex = temp == "" ? systemLanguageIndex : AllCult.FindIndex(c => c.culture.Name == temp);

        if (currentLanguageIndex == -1)
        {
            if (Instance.debugging)
                Debug.LogWarning("Since the preferred language couldn't be found, the preferred language is refreshed");
            chosenCultureName = "";
            currentLanguageIndex = systemLanguageIndex;
        }

        await MakeLanguageAvailable(mainLanguageIndex, systemLanguageIndex, currentLanguageIndex);

    }
    /// <summary>
    /// Checks whether main, system and current langauge files are broken and reconstructs the choices.
    /// <para>If it is impossible to reconstruct, throws an error</para>
    /// </summary>



    public async Task ChooseLanguage(int newLanguageIndex)
    {
        Task MLA = MakeLanguageAvailable(newLanguageIndex);
        chosenCultureName = AllCult[newLanguageIndex].culture.Name;
        await MLA;
        currentLanguageIndex = newLanguageIndex;
        OnLanguageChange.Invoke() ;
        localizationHandler.ConstructPriorityList();
    }
    private async Task MakeLanguageAvailable(params int[] Indexes)
    {
        List<Task> DictionaryLoad = new List<Task>();

        foreach (var index in Indexes.Distinct())
        {
            if (AllCult[index].IsBroken)
            {
                if (Instance.debugging)
                {
                    Debug.LogWarning("Can't load language " +
                    AllCult[index].culture.Name +
                    " because it is marked as broken. Will attempt to load other languages\n" +
                    " To use this language please check the language file and restart the application");
                }
                continue;
            }
            if (AllCult[index].IsLoadingOrLoaded == false)
            {
                AllCult[index].IsLoadingOrLoaded = true;
                DictionaryLoad.Add(AllCult[index].LoadDictionary());
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


        for (int i = 0; i < AllCult.Count; i++)
            if (AllCult[i].culture == cur)
                return i;
        for (int i = 0; i < AllCult.Count; i++)
            if (AllCult[i].culture == cur.Parent ||
                Application.systemLanguage.ToString() == AllCult[i].culture.EnglishName
                )
                return i;
        for (int i = 0; i < AllCult.Count; i++)
            if (AllCult[i].culture.Parent == cur.Parent ||
                Application.systemLanguage.ToString() == AllCult[i].culture.Parent.EnglishName)
                return i;

        return -1;
    }

    public void GetAvailableLanguages()
    {
        DirectoryInfo directoryinfo = new DirectoryInfo(LanguageFolder);
        List<FileInfo> languageFiles = directoryinfo.GetFiles()
            .Where(f => f.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            .Select(f => f)
            .ToList();

        AllCult = new List<CultureData>();
        foreach (var lf in languageFiles)
        {

            CultureInfo tempCultureInfo = CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(lf.FullName));
            AllCult.Add(new CultureData() { culture = tempCultureInfo, filePath = lf.FullName });
        }

        if (AllCult.Count == 0)
            throw new LocalizationException("Couldn't find any languages", LanguageFolder);


    }



    List<object> GetDynamicParts(DynamicParts dynamicParts)
    {
        List<object> DynamicValues = new List<object>();
        foreach (var dynamicPart in dynamicParts.dp)
        {
            object temp;
            try
            {
                temp = DataFinder.GetData(dynamicPart.script, dynamicPart.VariableName);
            }
            catch
            {
                temp = "";
            }
            DynamicValues.Add(temp);
        }
        return DynamicValues;
    }
    public string GetLocalizedValue(string key, DynamicParts dp)
    {
        Allkeys.Add(key);
        string rawTranslation = localizationHandler.GetRawTranslation(key);
        if (dp.dp.Count == 0) return rawTranslation;
        List<object> DynamicValues = GetDynamicParts(dp);

        return TurnDynamicsIntoText(rawTranslation, DynamicValues);

    }
    public string GetLocalizedValue(string key, DynamicVarsForCode dp)
    {
        Allkeys.Add(key);
        string rawTranslation = localizationHandler.GetRawTranslation(key);
        if (dp.DynamicVariables.Count == 0) return rawTranslation;


        return TurnDynamicsIntoText(rawTranslation, dp.DynamicVariables);

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
                break;//Already logged in NumberInsideListParanthesis method

        }
        s.Append(textwithNumbers.Substring(startIndex, textwithNumbers.Length - startIndex));

        return s.ToString();
    }

    public static string GetDynamicLocal(object v)
    {
        if (v is int) return ((int)v).ToString(Instance.AllCult[Instance.currentLanguageIndex].culture.NumberFormat);
        if (v is double) return ((double)v).ToString(Instance.AllCult[Instance.currentLanguageIndex].culture.NumberFormat);
        if (v is DateTime) return ((DateTime)v).ToString(Instance.AllCult[Instance.currentLanguageIndex].culture.DateTimeFormat);
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
            if (!Instance.AllCult[index].IsBroken && !PriorityList.Contains(index))
                PriorityList.Add(index);
        }

        internal string GetRawTranslation(string key)
        {
            foreach (var langIndex in PriorityList)
            {
                try { return Instance.AllCult[langIndex].GetValue(key); }
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
    

}


