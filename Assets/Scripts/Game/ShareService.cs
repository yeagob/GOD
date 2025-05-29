using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ShareService
{
    private readonly PopupsController _popupsController;
    private readonly EmailSender _emailSender;
    private readonly URLParameterHandler _urlParameterHandler;

    public ShareService(PopupsController popupsController, EmailSender emailSender, URLParameterHandler urlParameterHandler)
    {
        _popupsController = popupsController;
        _emailSender = emailSender;
        _urlParameterHandler = urlParameterHandler;
    }

    public async Task PublishBoard(BoardData boardData)
    {
        string mail = await _popupsController.ShowPublishBoardPopup();

        if (mail != "")
        {
            boardData.autor = mail;
            _emailSender.SendEmail(JsonUtility.ToJson(boardData));
            await _popupsController.ShowGenericMessage("Solicitud de publicacion enviada!!");
        }
    }

    public void ShareBoard(List<string> boardNames, int selectedIndex)
    {
        if (_urlParameterHandler.ShouldLoadFromURL)
        {
            _popupsController.ShowShareBoardPopup(Application.absoluteURL).WrapErrors();
        }
        else if (selectedIndex != -1)
        {
            _popupsController.ShowShareBoardPopup("https://xr-dreams.com/GOD?board=" + boardNames[selectedIndex]).WrapErrors();
        }
    }
}
