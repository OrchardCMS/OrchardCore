!function(e){"use strict";"function"==typeof define&&define.amd?define(["jquery"],e):"object"==typeof exports?e(require("jquery")):e(window.jQuery)}(function(e){"use strict";var t=0,r=e,n="parseJSON";"JSON"in window&&"parse"in JSON&&(r=JSON,n="parse"),e.ajaxTransport("iframe",function(r){if(r.async){var n,a,o,i=r.initialIframeSrc||"javascript:false;";return{send:function(p,f){(n=e('<form style="display:none;"></form>')).attr("accept-charset",r.formAcceptCharset),o=/\?/.test(r.url)?"&":"?","DELETE"===r.type?(r.url=r.url+o+"_method=DELETE",r.type="POST"):"PUT"===r.type?(r.url=r.url+o+"_method=PUT",r.type="POST"):"PATCH"===r.type&&(r.url=r.url+o+"_method=PATCH",r.type="POST"),t+=1,a=e('<iframe src="'+i+'" name="iframe-transport-'+t+'"></iframe>').on("load",function(){var t,o=e.isArray(r.paramName)?r.paramName:[r.paramName];a.off("load").on("load",function(){var t;try{if(!(t=a.contents()).length||!t[0].firstChild)throw Error()}catch(e){t=void 0}f(200,"success",{iframe:t}),e('<iframe src="'+i+'"></iframe>').appendTo(n),window.setTimeout(function(){n.remove()},0)}),n.prop("target",a.prop("name")).prop("action",r.url).prop("method",r.type),r.formData&&e.each(r.formData,function(t,r){e('<input type="hidden"/>').prop("name",r.name).val(r.value).appendTo(n)}),r.fileInput&&r.fileInput.length&&"POST"===r.type&&(t=r.fileInput.clone(),r.fileInput.after(function(e){return t[e]}),r.paramName&&r.fileInput.each(function(t){e(this).prop("name",o[t]||r.paramName)}),n.append(r.fileInput).prop("enctype","multipart/form-data").prop("encoding","multipart/form-data"),r.fileInput.removeAttr("form")),window.setTimeout(function(){n.submit(),t&&t.length&&r.fileInput.each(function(r,n){var a=e(t[r]);e(n).prop("name",a.prop("name")).attr("form",a.attr("form")),a.replaceWith(n)})},0)}),n.append(a).appendTo(document.body)},abort:function(){a&&a.off("load").prop("src",i),n&&n.remove()}}}}),e.ajaxSetup({converters:{"iframe text":function(t){return t&&e(t[0].body).text()},"iframe json":function(t){return t&&r[n](e(t[0].body).text())},"iframe html":function(t){return t&&e(t[0].body).html()},"iframe xml":function(t){var r=t&&t[0];return r&&e.isXMLDoc(r)?r:e.parseXML(r.XMLDocument&&r.XMLDocument.xml||e(r.body).html())},"iframe script":function(t){return t&&e.globalEval(e(t[0].body).text())}}})});
//# sourceMappingURL=jquery.iframe-transport.maprameSrc || 'javascript:false;',
        form,
        iframe,
        addParamChar;
      return {
        send: function send(_, completeCallback) {
          form = $('<form style="display:none;"></form>');
          form.attr('accept-charset', options.formAcceptCharset);
          addParamChar = /\?/.test(options.url) ? '&' : '?';
          // XDomainRequest only supports GET and POST:
          if (options.type === 'DELETE') {
            options.url = options.url + addParamChar + '_method=DELETE';
            options.type = 'POST';
          } else if (options.type === 'PUT') {
            options.url = options.url + addParamChar + '_method=PUT';
            options.type = 'POST';
          } else if (options.type === 'PATCH') {
            options.url = options.url + addParamChar + '_method=PATCH';
            options.type = 'POST';
          }
          // IE versions below IE8 cannot set the name property of
          // elements that have already been added to the DOM,
          // so we set the name along with the iframe HTML markup:
          counter += 1;
          iframe = $('<iframe src="' + initialIframeSrc + '" name="iframe-transport-' + counter + '"></iframe>').on('load', function () {
            var fileInputClones,
              paramNames = $.isArray(options.paramName) ? options.paramName : [options.paramName];
            iframe.off('load').on('load', function () {
              var response;
              // Wrap in a try/catch block to catch exceptions thrown
              // when trying to access cross-domain iframe contents:
              try {
                response = iframe.contents();
                // Google Chrome and Firefox do not throw an
                // exception when calling iframe.contents() on
                // cross-domain requests, so we unify the response:
                if (!response.length || !response[0].firstChild) {
                  throw new Error();
                }
              } catch (e) {
                response = undefined;
              }
              // The complete callback returns the
              // iframe content document as response object:
              completeCallback(200, 'success', {
                iframe: response
              });
              // Fix for IE endless progress bar activity bug
              // (happens on form submits to iframe targets):
              $('<iframe src="' + initialIframeSrc + '"></iframe>').appendTo(form);
              window.setTimeout(function () {
                // Removing the form in a setTimeout call
                // allows Chrome's developer tools to display
                // the response result
                form.remove();
              }, 0);
            });
            form.prop('target', iframe.prop('name')).prop('action', options.url).prop('method', options.type);
            if (options.formData) {
              $.each(options.formData, function (index, field) {
                $('<input type="hidden"/>').prop('name', field.name).val(field.value).appendTo(form);
              });
            }
            if (options.fileInput && options.fileInput.length && options.type === 'POST') {
              fileInputClones = options.fileInput.clone();
              // Insert a clone for each file input field:
              options.fileInput.after(function (index) {
                return fileInputClones[index];
              });
              if (options.paramName) {
                options.fileInput.each(function (index) {
                  $(this).prop('name', paramNames[index] || options.paramName);
                });
              }
              // Appending the file input fields to the hidden form
              // removes them from their original location:
              form.append(options.fileInput).prop('enctype', 'multipart/form-data')
              // enctype must be set as encoding for IE:
              .prop('encoding', 'multipart/form-data');
              // Remove the HTML5 form attribute from the input(s):
              options.fileInput.removeAttr('form');
            }
            window.setTimeout(function () {
              // Submitting the form in a setTimeout call fixes an issue with
              // Safari 13 not triggering the iframe load event after resetting
              // the load event handler, see also:
              // https://github.com/blueimp/jQuery-File-Upload/issues/3633
              form.submit();
              // Insert the file input fields at their original location
              // by replacing the clones with the originals:
              if (fileInputClones && fileInputClones.length) {
                options.fileInput.each(function (index, input) {
                  var clone = $(fileInputClones[index]);
                  // Restore the original name and form properties:
                  $(input).prop('name', clone.prop('name')).attr('form', clone.attr('form'));
                  clone.replaceWith(input);
                });
              }
            }, 0);
          });
          form.append(iframe).appendTo(document.body);
        },
        abort: function abort() {
          if (iframe) {
            // javascript:false as iframe src aborts the request
            // and prevents warning popups on HTTPS in IE6.
            iframe.off('load').prop('src', initialIframeSrc);
          }
          if (form) {
            form.remove();
          }
        }
      };
    }
  });

  // The iframe transport returns the iframe content document as response.
  // The following adds converters from iframe to text, json, html, xml
  // and script.
  // Please note that the Content-Type for JSON responses has to be text/plain
  // or text/html, if the browser doesn't include application/json in the
  // Accept header, else IE will show a download dialog.
  // The Content-Type for XML responses on the other hand has to be always
  // application/xml or text/xml, so IE properly parses the XML response.
  // See also
  // https://github.com/blueimp/jQuery-File-Upload/wiki/Setup#content-type-negotiation
  $.ajaxSetup({
    converters: {
      'iframe text': function iframeText(iframe) {
        return iframe && $(iframe[0].body).text();
      },
      'iframe json': function iframeJson(iframe) {
        return iframe && jsonAPI[jsonParse]($(iframe[0].body).text());
      },
      'iframe html': function iframeHtml(iframe) {
        return iframe && $(iframe[0].body).html();
      },
      'iframe xml': function iframeXml(iframe) {
        var xmlDoc = iframe && iframe[0];
        return xmlDoc && $.isXMLDoc(xmlDoc) ? xmlDoc : $.parseXML(xmlDoc.XMLDocument && xmlDoc.XMLDocument.xml || $(xmlDoc.body).html());
      },
      'iframe script': function iframeScript(iframe) {
        return iframe && $.globalEval($(iframe[0].body).text());
      }
    }
  });
});