using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public CharacterGenerator genCgaracter;
    
    public List<GeneratedCharacter> characters = new List<GeneratedCharacter>();

    
    
    public void genList()
    {
        for (int i = 0; i < 5; i++)
        {
            CreateCharacter();
        }
    }
    
    public GeneratedCharacter CreateCharacter()
    {
        GeneratedCharacter newCharacter = genCgaracter.Generate();
        characters.Add(newCharacter);

        Debug.Log("Создан персонаж: " + newCharacter.fullName +
                  ", возраст: " + newCharacter.age);

        return newCharacter;
    }
}
