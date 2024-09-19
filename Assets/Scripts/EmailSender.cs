
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EmailSender : MonoBehaviour
{
    public void SendEmail()
    {
        StartCoroutine(SendEmailCoroutine());
    }

    IEnumerator SendEmailCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("service_id", "service_qbqixup");
        form.AddField("template_id", "template_h2d8gfe");
        form.AddField("user_id", "dq3pJiZPbQSZlH44C");
        form.AddField("template_params[from_name]", "Nombre del Player 1");
        form.AddField("template_params[message]", "Json del tablero");

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.emailjs.com/api/v1.0/email/send", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error al enviar el correo: " + www.error);
            }
            else
            {
                Debug.Log("Correo enviado exitosamente!");
            }
        }
    }
}
