using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Cozyffee
{
    public class TypingEffect : MonoBehaviour
    {
        public TextMeshPro textMeshPro;
        private Coroutine typingCoroutine;

        public void StartTyping(string text, float duration)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(CR_TypeText(text, duration));
        }

        private IEnumerator CR_TypeText(string text, float duration)
        {
            if (textMeshPro == null)
            {
                Debug.LogError("TextMeshPro reference is missing!");
                yield break;
            }

            textMeshPro.text = "";

            float timePerCharacter = duration / text.Length;

            for (int i = 0; i <= text.Length; i++)
            {
                textMeshPro.text = text.Substring(0, i);
                yield return new WaitForSeconds(timePerCharacter);
            }
        }
    }
}