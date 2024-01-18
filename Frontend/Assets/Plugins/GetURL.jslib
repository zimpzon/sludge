var GetURL = {
    
    GetURLFromPage: function () {
		var returnStr = (window.location != window.parent.location)
            ? document.referrer
            : document.location.href;
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        stringToUTF8(returnStr, buffer, lengthBytesUTF8(returnStr));
        return buffer;
    }
};

mergeInto(LibraryManager.library, GetURL);