using UnityEngine;
using System.Collections.Generic;

public class CharacterGenerator : MonoBehaviour
{
    [Header("������")]
    public CharaterObject profile;

    public GeneratedCharacter Generate()
    {
        GeneratedCharacter character = new GeneratedCharacter();

        if (Random.value < 0.5f)
            character.gender = GeneratedCharacter.Gender.Male;
        else
            character.gender = GeneratedCharacter.Gender.Female;

        if (profile.namePool != null)
        {
            string firstName;
            string lastName;

            if (character.gender == GeneratedCharacter.Gender.Male)
            {
                firstName = GetRandomFromList(profile.namePool.maleNames);
                lastName = GetRandomFromList(profile.namePool.maleLastNames);
            }
            else
            {
                firstName = GetRandomFromList(profile.namePool.femaleNames);
                lastName = GetRandomFromList(profile.namePool.femaleLastNames);
            }

            character.fullName = firstName + " " + lastName;
        }
        else
        {
            character.fullName = "����������";
        }

        character.age = Random.Range(profile.minAge, profile.maxAge + 1);

        if (profile.avObliks != null && profile.avObliks.Count > 0)
        {
            int randomIndex = Random.Range(0, profile.avObliks.Count);
            character.portrait = profile.avObliks[randomIndex];
        }
        else
        {
            character.portrait = null;
        }

        character.health = 100;
        character.morale = 100;
        character.daysWithoutFood = 0;
        character.daysWithoutWater = 0;

        if (profile.availableTraits != null && profile.availableTraits.Count > 0)
        {
            int index = Random.Range(0, profile.availableTraits.Count);
            character.traits.Add(profile.availableTraits[index]);
        }

        return character;
    }

    private string GetRandomFromList(List<string> list)
    {
        if (list == null || list.Count == 0)
            return "???";

        return list[Random.Range(0, list.Count)];
    }
}