namespace Slimipelago.Archipelago;

public class ApData
{
    public string AddressPort = "archipelago.gg:12345";
    public string Password = "";
    public string SlotName = "Rancher1";
    public bool DeathLink = false;
    public bool DeathLinkTrap = false;
    public bool TrapLink = false;
    public bool TrapLinkRandom = false;
    public bool MusicRando = false;
    public bool MusicRandoRandomizeOnce = false;
    public bool UseCustomAssets = true;

    public void Init()
    {
        var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
        AddressPort = fileText[0];
        Password = fileText[1];
        SlotName = fileText[2];

        if (fileText.Length <= 3) return;
        var boolArr = fileText[3].ToCharArray();
        DeathLink = StrBool(boolArr[0]);
        DeathLinkTrap = StrBool(boolArr[1]);
        TrapLink = StrBool(boolArr[2]);
        TrapLinkRandom = StrBool(boolArr[3]);
        MusicRando = StrBool(boolArr[4]);
        MusicRandoRandomizeOnce = StrBool(boolArr[5]);
        if (boolArr.Length > 6) UseCustomAssets = StrBool(boolArr[6]);
    }
    
    private static bool StrBool(char c) => c == '1';
}