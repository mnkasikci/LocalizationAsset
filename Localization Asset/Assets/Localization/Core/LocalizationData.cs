using System.Collections.Generic;

using System;

[Serializable]
public class LocalizationData
{
    public List<LocalizationItem> items;
    public IEnumerator<LocalizationItem> GetEnumerator() => items.GetEnumerator();
}
[Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}