using UnityEngine;
using Slimipelago.Archipelago.CustGui;

namespace Slimipelago.Added;

public class GuiUpdaterinator : MonoBehaviour
{
    public GuiBuilder GuiToRender;
    public bool WindowOpen;
    
    /*
     ahhh~ perry the platypus, you are just in time to witness my latest invention-
     THE GuiUpdaterinator
     it will make Unity ImGuis render!
     those annoying Unity UI causing pain and misfortune will no longer when they are completely forsaken!
     */

    private void OnGUI()
    {
        if (!WindowOpen) return;
        GuiToRender.Render();
    }
}