using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "GameData/StageData")]
public class StageDataSO : ScriptableObject
{
    [Header("Stage Info")]
    public string stageName;
    public int stageID;

    [Header("Visuals")]
    public Sprite backgroundSprite;
}
