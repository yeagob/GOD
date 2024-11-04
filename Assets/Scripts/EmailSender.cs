
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EmailSender : MonoBehaviour
{
    [System.Serializable]
    public class EmailPayload
    {
        public string service_id;
        public string template_id;
        public string user_id;
        public TemplateParams template_params;

        public EmailPayload(string serviceId, string templateId, string userId, string player, string message)
        {
            service_id = serviceId;
            template_id = templateId;
            user_id = userId;
            template_params = new TemplateParams(player, message);
        }
    }

    [System.Serializable]
    public class TemplateParams
    {
        public string message;

        public TemplateParams(string fromName, string messageContent)
        {
           // username = fromName;
            message = messageContent;
        }
    }

    public void SendEmail(string data)
    {
        StartCoroutine(SendEmailCoroutine(data));
    }

    IEnumerator SendEmailCoroutine(string json)
    {
        string url = "https://api.emailjs.com/api/v1.0/email/send";

        // Crear el objeto JSON que requiere EmailJS
        EmailPayload payload = new EmailPayload(
            "service_qbqixup",
            "template_h2d8gfe",
            "dq3pJiZPbQSZlH44C",
            "Duck",
            json
        );

        // Convertir el objeto en string JSON
        string jsonData = JsonUtility.ToJson(payload);
       // Debug.Log("Maildata = " + jsonData);
        // Crear la solicitud y configurar los headers
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // Enviar la solicitud
        yield return www.SendWebRequest();

        // Verificar el resultado
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
