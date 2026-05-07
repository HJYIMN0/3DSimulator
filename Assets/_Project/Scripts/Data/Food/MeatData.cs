using UnityEngine;

[CreateAssetMenu(fileName = "MeatData", menuName = "Meat/MeatData")]
public class MeatData : ScriptableObject
{
    public enum ProcessedState
    {
        Raw,
        Cut,
        Cooked,
        Burnt
    }

    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Visuals")]
    [SerializeField] private Sprite icon;
    [SerializeField] private CarryableItem prefab;

    [Header("State")]
    [SerializeField] private ProcessedState processedState;

    public string DisplayName => displayName;
    public string Id => id;
    public Sprite Icon => icon;
    public CarryableItem Prefab => prefab;
    public ProcessedState ProcessingState => processedState;

}
