using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Generates names from a list of first and last names
public class NameGenerator : MonoBehaviour
{
    public List<string> firstNames = new List<string>();
    public List<string> lastNames = new List<string>();

    public string GenerateName()
    {
        if (firstNames.Count == 0 || lastNames.Count == 0)
        {
            Debug.Log("You forgot to enter some names in the Name Generator");
            return null;
        }

        string result = firstNames[Random.Range(0,firstNames.Count)];
        result += ' ';
        

        result += lastNames[Random.Range(0, lastNames.Count)];

        return result;
    }
}
