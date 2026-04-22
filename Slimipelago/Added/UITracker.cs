using System.Text;
using Slimipelago.Archipelago;
using Slimipelago.Patches.PlayerPatches;
using TMPro;
using UnityEngine;
using static Slimipelago.Archipelago.ApSlimeClient;

namespace Slimipelago.Added;

public class UITracker : MonoBehaviour
{
    private TextMeshProUGUI UiText;
    private StringBuilder Sb = new();

    public void Start()
    {
        var timePos = gameObject.GetChild(4).transform.position;
        var mailPos = gameObject.GetChild(5).transform.position;
        
        var uiObj = Instantiate(gameObject.GetChild(2), transform, true);
        UiText = uiObj.GetComponent<TextMeshProUGUI>();

        var pos = uiObj.transform.position;
        pos.y = mailPos.y + (mailPos.y - timePos.y);
        uiObj.transform.position = pos;

        UiText.enableWordWrapping = false;
    }

    private void Update()
    {
        Sb.Clear();

        if (Core.DebugLevel > 0) Sb.Append(PlayerModelPatch.Model.position.HashPos()).Append('\n');
        if (Client.IsGoalType(GoalType.Notes)) Sb.Append("Notes: ").Append(CurrentNotes).Append('/').Append(NoteCount);

        UiText.text = Sb.ToString();
    }
}