using MelonLoader;
using UnityEngine;

namespace Slimipelago;

[RegisterTypeInIl2Cpp]
public class Invisinator : MonoBehaviour
{
    /*
     ahhh~ perry the platypus, you are just in time to witness my latest invention- 
     THE INVISINATOR
     it will make whatever it touches, invisible
     those annoying buttons on the main menu that bug you? just zap them with the Invisinator and bam! invisible
     */

    private void Update() => gameObject.SetActive(false);
}