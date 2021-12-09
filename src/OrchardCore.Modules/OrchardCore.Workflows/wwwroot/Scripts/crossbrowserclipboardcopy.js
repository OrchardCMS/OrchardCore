/* @ts-check
 *  Javascript Copy to Clipboard Crossbrowser
   
    Paste the following javascript call in your HTML web page:
    
    <div onclick="select_all_and_copy(this)">This div will copy when clicked on</div>
*/


function select_all_and_copy(el) {
    // Copy textarea, pre, div, etc.
    if (document.body.createTextRange) {
        // IE
        var textRange = document.body.createTextRange();
        textRange.moveToElementText(el);
        textRange.select();
        textRange.execCommand("Copy");
        
    }
    else if (window.getSelection && document.createRange) {
        // non-IE
        var editable = el.contentEditable; // Record contentEditable status of element
        var readOnly = el.readOnly; // Record readOnly status of element
        el.contentEditable = true; // iOS will only select text on non-form elements if contentEditable = true;
        el.readOnly = false; // iOS will not select in a read only form element
        var range = document.createRange();
        range.selectNodeContents(el);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range); // Does not work for Firefox if a textarea or input
        if (el.nodeName === "TEXTAREA" || el.nodeName === "INPUT")
            el.select(); // Firefox will only select a form element with select()
        if (el.setSelectionRange && navigator.userAgent.match(/ipad|ipod|iphone/i))
            el.setSelectionRange(0, 999999); // iOS only selects "form" elements with SelectionRange
        el.contentEditable = editable; // Restore previous contentEditable status
        el.readOnly = readOnly; // Restore previous readOnly status
        if (document.queryCommandSupported("copy")) {
            var successful = document.execCommand('copy');
            //if (successful) tooltip(el, "Copied to clipboard.");
            //else tooltip(el, "Press CTRL+C to copy");
        }
        //else {
        //    if (!navigator.userAgent.match(/ipad|ipod|iphone|android|silk/i))
        //        tooltip(el, "Press CTRL+C to copy");
        //}
    }
} // end function select_all_and_copy(el)


    /* Note: document.queryCommandSupported("copy") should return "true" on browsers that support copy
	    but there was a bug in Chrome versions 42 to 47 that makes it return "false".  So in those
	    versions of Chrome feature detection does not work!
	    See https://code.google.com/p/chromium/issues/detail?id=476508
    */