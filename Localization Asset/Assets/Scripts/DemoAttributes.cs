using UnityEngine;

public class Attributes : MonoBehaviour
{


    private int prevgold;
    private bool mustCreateLo = false, checkTime = true;
    public LocalizationText lt, inspectorlt;
    public string ThiefName { get => "theworstthiefever"; }
    public int PrevGold
    {
        get => prevgold;
        set
        {
            prevgold = value;
            if (lt != null)
                lt.CodeCreatedUpdate(PrevGold, StolenGold, ThiefName);
            if(inspectorlt!=null)
                inspectorlt.InspectorCreatedUpdate();
        }
    }

    private void Update()
    {
        PrevGold++;
        if (mustCreateLo)
        {
            lt = LocalizationTextCreator.Add(g, "textStolen", PrevGold, StolenGold, ThiefName);
            mustCreateLo = false;
        }
        if (checkTime)
            if (Time.realtimeSinceStartup > 1)
            {
                mustCreateLo = true;
                checkTime = false;
            }

    }
    public GameObject g;


    public int StolenGold { get => 200; }


}
