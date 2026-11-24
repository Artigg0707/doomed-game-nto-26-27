using UnityEngine;
using System.Collections.Generic;

public class CharacterGenerator : MonoBehaviour
{
    [Header("Профиль персонажей")]
    public CharaterObject profile;

    private List<Sprite> availableMaleObliks;
    private List<Sprite> availableFemaleObliks;
    private HashSet<string> generatedDescriptions = new HashSet<string>();

    private readonly string[] backgroundTemplates =
    {
        "{0} до катастрофы жил{1} обычной жизнью и с трудом привык{1} к замкнутому бункеру.",
        "{0} оказался{1} в убежище в первые часы апокалипсиса и до сих пор помнит тревожные сирены.",
        "{0} был{1} среди тех, кто пробился к гермодвери перед ледяной бурей.",
        "{0} называет бункер временным пристанищем, но уже начал{1} привыкать к новой реальности.",
        "{0} до конца помнит день, когда снаружи исчезло солнце и город погрузился в зимний мрак.",
        "{0} сумел{1} сохранить самообладание во время эвакуации и теперь помогает держать бункер вместе."
    };

    private readonly string[] characterTemplates =
    {
        "Он{0} выглядит сдержанно и редко говорит лишнее, но внимательно следит за окружающими.",
        "Она{0} выглядит сдержанно и редко говорит лишнее, но внимательно следит за окружающими.",
        "В сложных ситуациях он{0} принимает решения спокойно и не любит тянуть с выбором.",
        "В сложных ситуациях она{0} принимает решения спокойно и не любит тянуть с выбором.",
        "Ему{0} тяжелее всего переносить неопределенность, поэтому он всё пытается держать под контролем.",
        "Ей{0} тяжелее всего переносить неопределенность, поэтому она всё пытается держать под контролем.",
        "Он{0} предпочитает действовать молча и замечает детали, которые проходят мимо других.",
        "Она{0} предпочитает действовать молча и замечает детали, которые проходят мимо других."
    };

    public void ResetAvailablePortraits()
    {
        availableMaleObliks = profile != null && profile.maleObliks != null
            ? new List<Sprite>(profile.maleObliks)
            : new List<Sprite>();
        availableFemaleObliks = profile != null && profile.femaleObliks != null
            ? new List<Sprite>(profile.femaleObliks)
            : new List<Sprite>();

        generatedDescriptions.Clear();
    }

    public GeneratedCharacter Generate()
    {
        GeneratedCharacter character = new GeneratedCharacter();

        if (profile == null)
        {
            Debug.LogError("В CharacterGenerator не назначен профиль персонажей.");
            return null;
        }

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
            character.fullName = "Без имени";
        }

        character.age = Random.Range(profile.minAge, profile.maxAge + 1);

       
        if (character.gender == GeneratedCharacter.Gender.Male)
        {
            if (availableMaleObliks != null && availableMaleObliks.Count > 0)
            {
                int randomIndex = Random.Range(0, availableMaleObliks.Count);
                character.portrait = availableMaleObliks[randomIndex];
                availableMaleObliks.RemoveAt(randomIndex); 
            }
            else
            {
                character.portrait = null;
            }
        }
        else 
        {
            if (availableFemaleObliks != null && availableFemaleObliks.Count > 0)
            {
                int randomIndex = Random.Range(0, availableFemaleObliks.Count);
                character.portrait = availableFemaleObliks[randomIndex];
                availableFemaleObliks.RemoveAt(randomIndex); 
            }
            else
            {
                character.portrait = null;
            }
        }

        character.health = 100;
        character.morale = 100;
        character.daysWithoutFood = 0;
        character.daysWithoutWater = 0;
        character.receivesFoodToday = false;
        character.receivesWaterToday = false;

        if (profile.availableTraits != null && profile.availableTraits.Count > 0)
        {
            int index = Random.Range(0, profile.availableTraits.Count);
            character.traits.Add(profile.availableTraits[index]);
        }

        character.description = GenerateUniqueDescription(character);

        return character;
    }

    private string GenerateUniqueDescription(GeneratedCharacter character)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            bool isMale = character.gender == GeneratedCharacter.Gender.Male;

            string genderEnding = isMale ? "" : "а";
            string background = string.Format(
                backgroundTemplates[Random.Range(0, backgroundTemplates.Length)],
                character.fullName,
                genderEnding
            );

            int genderTemplateOffset = isMale ? 0 : 1;
            int characterTemplateIndex = genderTemplateOffset
                + Random.Range(0, characterTemplates.Length / 2) * 2;

            string behavior = string.Format(
                characterTemplates[characterTemplateIndex],
                ""
            );

            string traitSentence = "";
            if (character.traits != null && character.traits.Count > 0)
            {
                Trait trait = character.traits[0];
                if (trait != null && !string.IsNullOrWhiteSpace(trait.traitDesc))
                {
                    traitSentence = $" Сейчас в карточке известна черта «{trait.traitName}».";
                }
            }

            string description =
                $"{background} {behavior}{traitSentence}";

            if (generatedDescriptions.Add(description))
                return description;
        }

        string fallback = $" {character.fullName} находится в бункере уже несколько дней.";
        int duplicateNumber = 1;
        while (!generatedDescriptions.Add(fallback))
        {
            fallback = $" {character.fullName} находится в бункере уже несколько дней. Повторная запись №{duplicateNumber}.";
            duplicateNumber++;
        }

        return fallback.TrimStart();
    }

    private string GetRandomFromList(List<string> list)
    {
        if (list == null || list.Count == 0)
            return "???";

        return list[Random.Range(0, list.Count)];
    }
}