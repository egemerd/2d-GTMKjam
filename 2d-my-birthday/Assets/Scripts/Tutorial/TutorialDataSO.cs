using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Calendar/Tutorial Data")]
public class TutorialDataSO : ScriptableObject
{
    [TextArea(3, 6)]
    public List<string> pages = new List<string>();
}