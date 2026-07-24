using UnityEngine;

[System.Serializable]
public class CellData
{
    public int dayNumber;
    public CellModifier modifier = CellModifier.None;
    public GameObject pinPrefabOverride; // null ise level'ýn varsayýlan pin'i kullanýlýr
}