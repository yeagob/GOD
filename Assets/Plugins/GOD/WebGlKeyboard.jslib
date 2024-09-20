mergeInto(LibraryManager.library, {
    InputFocusHandleAction: function(namePtr, strPtr) {
        var name = UTF8ToString(namePtr);
        var str = UTF8ToString(strPtr);
        if (/Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent)) {
            var inputTextData = prompt("", str);

            if (inputTextData !== null && inputTextData !== "") {
                var bufferSize = lengthBytesUTF8(inputTextData) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(inputTextData, buffer, bufferSize);

                // Send the data back to Unity
                SendMessage(name, "ReceiveInputData", inputTextData);
                
                _free(buffer);
            }
        }
    }
});