using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Types text into a UI Toolkit Label character by character.
/// </summary>
public static class TypewriterEffect
{
    public static IEnumerator Run(Label label, string fullText, float charDelay = 0.02f, bool playSound = true)
    {
        label.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            label.text = fullText.Substring(0, i + 1);

            if (playSound && fullText[i] != ' ' && fullText[i] != '\n' && i % 2 == 0)
            {
                if (ProceduralAudio.Instance != null)
                    ProceduralAudio.Instance.PlayType();
            }

            // Handle dramatic pauses
            if (i + 3 < fullText.Length && fullText.Substring(i, 3) == "...")
            {
                yield return new WaitForSeconds(0.4f);
            }
            else if (fullText[i] == '.' || fullText[i] == '!' || fullText[i] == '?')
            {
                yield return new WaitForSeconds(0.08f);
            }
            else if (fullText[i] == ',')
            {
                yield return new WaitForSeconds(0.04f);
            }

            // Skip ahead if player clicks
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                label.text = fullText;
                yield break;
            }

            yield return new WaitForSeconds(charDelay);
        }
    }
}
