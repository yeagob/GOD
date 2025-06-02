using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIPopups
{
    public class PlayerPanel : MonoBehaviour
    {
        public Image ColorImage;
        public TMP_InputField NameInputField;

        public void SetupAsLocalPlayer(string playerName, Color playerColor)
        {
            if (NameInputField != null)
            {
                NameInputField.text = playerName;
                NameInputField.interactable = true;
            }

            if (ColorImage != null)
            {
                ColorImage.color = playerColor;
            }
        }

        public void SetupAsRemotePlayer(string playerName, Color playerColor)
        {
            if (NameInputField != null)
            {
                NameInputField.text = playerName;
                NameInputField.interactable = false;
            }

            if (ColorImage != null)
            {
                ColorImage.color = playerColor;
            }
        }

        public void UpdatePlayerInfo(string playerName, Color playerColor)
        {
            if (NameInputField != null)
            {
                NameInputField.text = playerName;
            }

            if (ColorImage != null)
            {
                ColorImage.color = playerColor;
            }
        }
    }
}