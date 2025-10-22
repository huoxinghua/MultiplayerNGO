using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Code.UI
{
    public class ButtonHoverTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TMP_Text textToChange;
        public Color normalColor = Color.white;
        public Color hoverColor = Color.yellow;

        public void OnPointerEnter(PointerEventData eventData)
        {
            textToChange.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            textToChange.color = normalColor;
        }
    }
}
