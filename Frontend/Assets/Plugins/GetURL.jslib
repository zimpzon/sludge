var GetURL = {
    
    GetURLFromPage: function () {
		var returnStr = (window.location != window.parent.location)
            ? document.referrer
            : document.location.href;
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    }
};

mergeInto(LibraryManager.library, GetURL);