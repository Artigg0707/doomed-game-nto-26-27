using UnityEngine;

[CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Trait")]
public class Trait : ScriptableObject
{
    public string traitName;
    public string traitDesc;
    public Sprite traitSprite;
    
    public enum SpecialAbility
    {
        None,
        
    }
}
