/*
 * jQuery Iframe Transport Plugin
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2011, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * https://opensource.org/licenses/MIT
 */

/* global define, require */

(function (factory) {
  'use strict';
  if (typeof define === 'function' && define.amd) {
    // Register as an anonymous AMD module:
    define(['jquery'], factory);
  } else if (typeof exports === 'object') {
    // Node/CommonJS:
    factory(require('jquery'));
  } else {
    // Browser globals:
    factory(window.jQuery);
  }
})(function ($) {
  'use strict';

  // Helper variable to create unique names for the transport iframes:
  var counter = 0,
    jsonAPI = $,
    jsonParse = 'parseJSON';

  if ('JSON' in window && 'parse' in JSON) {
    jsonAPI = JSON;
    jsonParse = 'parse';
  }

  // The iframe transport accepts four additional options:
  // options.fileInput: a jQuery collection of file input fields
  // options.paramName: the parameter name for the file form data,
  //  overrides the name property of the file input field(s),
  //  can be a string or an array of strings.
  // options.formData: an array of objects with name and value properties,
  //  equivalent to the return data of .serializeArray(), e.g.:
  //  [{name: 'a', value: 1}, {name: 'b', value: 2}]
  // options.initialIframeSrc: the URL of the initial iframe src,
  //  by default set to "javascript:false;"
  $.ajaxTransport('iframe', function (options) {
    if (options.async) {
      // javascript:false as initial iframe src
      // prevents warning popups on HTTPS in IE6:
      // eslint-disable-next-line no-script-url
      var initialIframeSrc = options.initialIframeSrc || 'javascript:false;',
        form,
        iframe,
        addParamChar;
      return {
        send: function (_, completeCallback) {
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
          iframe = $(
            '<iframe src="' +
              initialIframeSrc +
              '" name="iframe-transport-' +
              counter +
              '"></iframe>'
          ).on('load', function () {
            var fileInputClones,
              paramNames = $.isArray(options.paramName)
                ? options.paramName
                : [options.paramName];
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
              completeCallback(200, 'success', { iframe: response });
              // Fix for IE endless progress bar activity bug
              // (happens on form submits to iframe targets):
              $('<iframe src="' + initialIframeSrc + '"></iframe>').appendTo(
                form
              );
              window.setTimeout(function () {
                // Removing the form in a setTimeout call
                // allows Chrome's developer tools to display
                // the response result
                form.remove();
              }, 0);
            });
            form
              .prop('target', iframe.prop('name'))
              .prop('action', options.url)
              .prop('method', options.type);
            if (options.formData) {
              $.each(options.formData, function (index, field) {
                $('<input type="hidden"/>')
                  .prop('name', field.name)
                  .val(field.value)
                  .appendTo(form);
              });
            }
            if (
              options.fileInput &&
              options.fileInput.length &&
              options.type === 'POST'
            ) {
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
              form
                .append(options.fileInput)
                .prop('enctype', 'multipart/form-data')
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
                  $(input)
                    .prop('name', clone.prop('name'))
                    .attr('form', clone.attr('form'));
                  clone.replaceWith(input);
                });
              }
            }, 0);
          });
          form.append(iframe).appendTo(document.body);
        },
        abort: function () {
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
      'iframe text': function (iframe) {
        return iframe && $(iframe[0].body).text();
      },
      'iframe json': function (iframe) {
        return iframe && jsonAPI[jsonParse]($(iframe[0].body).text());
      },
      'iframe html': function (iframe) {
        return iframe && $(iframe[0].body).html();
      },
      'iframe xml': function (iframe) {
        var xmlDoc = iframe && iframe[0];
        return xmlDoc && $.isXMLDoc(xmlDoc)
          ? xmlDoc
          : $.parseXML(
              (xmlDoc.XMLDocument && xmlDoc.XMLDocument.xml) ||
                $(xmlDoc.body).html()
            );
      },
      'iframe script': function (iframe) {
        return iframe && $.globalEval($(iframe[0].body).text());
      }
    }
  });
});


/*
 * jQuery File Upload Plugin
 * https://github.com/blueimp/jQuery-File-Upload
 *
 * Copyright 2010, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * https://opensource.org/licenses/MIT
 */

/* global define, require */
/* eslint-disable new-cap */

(function (factory) {
  'use strict';
  if (typeof define === 'function' && define.amd) {
    // Register as an anonymous AMD module:
    define(['jquery', 'jquery-ui/ui/widget'], factory);
  } else if (typeof exports === 'object') {
    // Node/CommonJS:
    factory(require('jquery'), require('./vendor/jquery.ui.widget'));
  } else {
    // Browser globals:
    factory(window.jQuery);
  }
})(function ($) {
  'use strict';

  // Detect file input support, based on
  // https://viljamis.com/2012/file-upload-support-on-mobile/
  $.support.fileInput = !(
    new RegExp(
      // Handle devices which give false positives for the feature detection:
      '(Android (1\\.[0156]|2\\.[01]))' +
        '|(Windows Phone (OS 7|8\\.0))|(XBLWP)|(ZuneWP)|(WPDesktop)' +
        '|(w(eb)?OSBrowser)|(webOS)' +
        '|(Kindle/(1\\.0|2\\.[05]|3\\.0))'
    ).test(window.navigator.userAgent) ||
    // Feature detection for all other devices:
    $('<input type="file"/>').prop('disabled')
  );

  // The FileReader API is not actually used, but works as feature detection,
  // as some Safari versions (5?) support XHR file uploads via the FormData API,
  // but not non-multipart XHR file uploads.
  // window.XMLHttpRequestUpload is not available on IE10, so we check for
  // window.ProgressEvent instead to detect XHR2 file upload capability:
  $.support.xhrFileUpload = !!(window.ProgressEvent && window.FileReader);
  $.support.xhrFormDataFileUpload = !!window.FormData;

  // Detect support for Blob slicing (required for chunked uploads):
  $.support.blobSlice =
    window.Blob &&
    (Blob.prototype.slice ||
      Blob.prototype.webkitSlice ||
      Blob.prototype.mozSlice);

  /**
   * Helper function to create drag handlers for dragover/dragenter/dragleave
   *
   * @param {string} type Event type
   * @returns {Function} Drag handler
   */
  function getDragHandler(type) {
    var isDragOver = type === 'dragover';
    return function (e) {
      e.dataTransfer = e.originalEvent && e.originalEvent.dataTransfer;
      var dataTransfer = e.dataTransfer;
      if (
        dataTransfer &&
        $.inArray('Files', dataTransfer.types) !== -1 &&
        this._trigger(type, $.Event(type, { delegatedEvent: e })) !== false
      ) {
        e.preventDefault();
        if (isDragOver) {
          dataTransfer.dropEffect = 'copy';
        }
      }
    };
  }

  // The fileupload widget listens for change events on file input fields defined
  // via fileInput setting and paste or drop events of the given dropZone.
  // In addition to the default jQuery Widget methods, the fileupload widget
  // exposes the "add" and "send" methods, to add or directly send files using
  // the fileupload API.
  // By default, files added via file input selection, paste, drag & drop or
  // "add" method are uploaded immediately, but it is possible to override
  // the "add" callback option to queue file uploads.
  $.widget('blueimp.fileupload', {
    options: {
      // The drop target element(s), by the default the complete document.
      // Set to null to disable drag & drop support:
      dropZone: $(document),
      // The paste target element(s), by the default undefined.
      // Set to a DOM node or jQuery object to enable file pasting:
      pasteZone: undefined,
      // The file input field(s), that are listened to for change events.
      // If undefined, it is set to the file input fields inside
      // of the widget element on plugin initialization.
      // Set to null to disable the change listener.
      fileInput: undefined,
      // By default, the file input field is replaced with a clone after
      // each input field change event. This is required for iframe transport
      // queues and allows change events to be fired for the same file
      // selection, but can be disabled by setting the following option to false:
      replaceFileInput: true,
      // The parameter name for the file form data (the request argument name).
      // If undefined or empty, the name property of the file input field is
      // used, or "files[]" if the file input name property is also empty,
      // can be a string or an array of strings:
      paramName: undefined,
      // By default, each file of a selection is uploaded using an individual
      // request for XHR type uploads. Set to false to upload file
      // selections in one request each:
      singleFileUploads: true,
      // To limit the number of files uploaded with one XHR request,
      // set the following option to an integer greater than 0:
      limitMultiFileUploads: undefined,
      // The following option limits the number of files uploaded with one
      // XHR request to keep the request size under or equal to the defined
      // limit in bytes:
      limitMultiFileUploadSize: undefined,
      // Multipart file uploads add a number of bytes to each uploaded file,
      // therefore the following option adds an overhead for each file used
      // in the limitMultiFileUploadSize configuration:
      limitMultiFileUploadSizeOverhead: 512,
      // Set the following option to true to issue all file upload requests
      // in a sequential order:
      sequentialUploads: false,
      // To limit the number of concurrent uploads,
      // set the following option to an integer greater than 0:
      limitConcurrentUploads: undefined,
      // Set the following option to true to force iframe transport uploads:
      forceIframeTransport: false,
      // Set the following option to the location of a redirect url on the
      // origin server, for cross-domain iframe transport uploads:
      redirect: undefined,
      // The parameter name for the redirect url, sent as part of the form
      // data and set to 'redirect' if this option is empty:
      redirectParamName: undefined,
      // Set the following option to the location of a postMessage window,
      // to enable postMessage transport uploads:
      postMessage: undefined,
      // By default, XHR file uploads are sent as multipart/form-data.
      // The iframe transport is always using multipart/form-data.
      // Set to false to enable non-multipart XHR uploads:
      multipart: true,
      // To upload large files in smaller chunks, set the following option
      // to a preferred maximum chunk size. If set to 0, null or undefined,
      // or the browser does not support the required Blob API, files will
      // be uploaded as a whole.
      maxChunkSize: undefined,
      // When a non-multipart upload or a chunked multipart upload has been
      // aborted, this option can be used to resume the upload by setting
      // it to the size of the already uploaded bytes. This option is most
      // useful when modifying the options object inside of the "add" or
      // "send" callbacks, as the options are cloned for each file upload.
      uploadedBytes: undefined,
      // By default, failed (abort or error) file uploads are removed from the
      // global progress calculation. Set the following option to false to
      // prevent recalculating the global progress data:
      recalculateProgress: true,
      // Interval in milliseconds to calculate and trigger progress events:
      progressInterval: 100,
      // Interval in milliseconds to calculate progress bitrate:
      bitrateInterval: 500,
      // By default, uploads are started automatically when adding files:
      autoUpload: true,
      // By default, duplicate file names are expected to be handled on
      // the server-side. If this is not possible (e.g. when uploading
      // files directly to Amazon S3), the following option can be set to
      // an empty object or an object mapping existing filenames, e.g.:
      // { "image.jpg": true, "image (1).jpg": true }
      // If it is set, all files will be uploaded with unique filenames,
      // adding increasing number suffixes if necessary, e.g.:
      // "image (2).jpg"
      uniqueFilenames: undefined,

      // Error and info messages:
      messages: {
        uploadedBytes: 'Uploaded bytes exceed file size'
      },

      // Translation function, gets the message key to be translated
      // and an object with context specific data as arguments:
      i18n: function (message, context) {
        // eslint-disable-next-line no-param-reassign
        message = this.messages[message] || message.toString();
        if (context) {
          $.each(context, function (key, value) {
            // eslint-disable-next-line no-param-reassign
            message = message.replace('{' + key + '}', value);
          });
        }
        return message;
      },

      // Additional form data to be sent along with the file uploads can be set
      // using this option, which accepts an array of objects with name and
      // value properties, a function returning such an array, a FormData
      // object (for XHR file uploads), or a simple object.
      // The form of the first fileInput is given as parameter to the function:
      formData: function (form) {
        return form.serializeArray();
      },

      // The add callback is invoked as soon as files are added to the fileupload
      // widget (via file input selection, drag & drop, paste or add API call).
      // If the singleFileUploads option is enabled, this callback will be
      // called once for each file in the selection for XHR file uploads, else
      // once for each file selection.
      //
      // The upload starts when the submit method is invoked on the data parameter.
      // The data object contains a files property holding the added files
      // and allows you to override plugin options as well as define ajax settings.
      //
      // Listeners for this callback can also be bound the following way:
      // .on('fileuploadadd', func);
      //
      // data.submit() returns a Promise object and allows to attach additional
      // handlers using jQuery's Deferred callbacks:
      // data.submit().done(func).fail(func).always(func);
      add: function (e, data) {
        if (e.isDefaultPrevented()) {
          return false;
        }
        if (
          data.autoUpload ||
          (data.autoUpload !== false &&
            $(this).fileupload('option', 'autoUpload'))
        ) {
          data.process().done(function () {
            data.submit();
          });
        }
      },

      // Other callbacks:

      // Callback for the submit event of each file upload:
      // submit: function (e, data) {}, // .on('fileuploadsubmit', func);

      // Callback for the start of each file upload request:
      // send: function (e, data) {}, // .on('fileuploadsend', func);

      // Callback for successful uploads:
      // done: function (e, data) {}, // .on('fileuploaddone', func);

      // Callback for failed (abort or error) uploads:
      // fail: function (e, data) {}, // .on('fileuploadfail', func);

      // Callback for completed (success, abort or error) requests:
      // always: function (e, data) {}, // .on('fileuploadalways', func);

      // Callback for upload progress events:
      // progress: function (e, data) {}, // .on('fileuploadprogress', func);

      // Callback for global upload progress events:
      // progressall: function (e, data) {}, // .on('fileuploadprogressall', func);

      // Callback for uploads start, equivalent to the global ajaxStart event:
      // start: function (e) {}, // .on('fileuploadstart', func);

      // Callback for uploads stop, equivalent to the global ajaxStop event:
      // stop: function (e) {}, // .on('fileuploadstop', func);

      // Callback for change events of the fileInput(s):
      // change: function (e, data) {}, // .on('fileuploadchange', func);

      // Callback for paste events to the pasteZone(s):
      // paste: function (e, data) {}, // .on('fileuploadpaste', func);

      // Callback for drop events of the dropZone(s):
      // drop: function (e, data) {}, // .on('fileuploaddrop', func);

      // Callback for dragover events of the dropZone(s):
      // dragover: function (e) {}, // .on('fileuploaddragover', func);

      // Callback before the start of each chunk upload request (before form data initialization):
      // chunkbeforesend: function (e, data) {}, // .on('fileuploadchunkbeforesend', func);

      // Callback for the start of each chunk upload request:
      // chunksend: function (e, data) {}, // .on('fileuploadchunksend', func);

      // Callback for successful chunk uploads:
      // chunkdone: function (e, data) {}, // .on('fileuploadchunkdone', func);

      // Callback for failed (abort or error) chunk uploads:
      // chunkfail: function (e, data) {}, // .on('fileuploadchunkfail', func);

      // Callback for completed (success, abort or error) chunk upload requests:
      // chunkalways: function (e, data) {}, // .on('fileuploadchunkalways', func);

      // The plugin options are used as settings object for the ajax calls.
      // The following are jQuery ajax settings required for the file uploads:
      processData: false,
      contentType: false,
      cache: false,
      timeout: 0
    },

    // jQuery versions before 1.8 require promise.pipe if the return value is
    // used, as promise.then in older versions has a different behavior, see:
    // https://blog.jquery.com/2012/08/09/jquery-1-8-released/
    // https://bugs.jquery.com/ticket/11010
    // https://github.com/blueimp/jQuery-File-Upload/pull/3435
    _promisePipe: (function () {
      var parts = $.fn.jquery.split('.');
      return Number(parts[0]) > 1 || Number(parts[1]) > 7 ? 'then' : 'pipe';
    })(),

    // A list of options that require reinitializing event listeners and/or
    // special initialization code:
    _specialOptions: [
      'fileInput',
      'dropZone',
      'pasteZone',
      'multipart',
      'forceIframeTransport'
    ],

    _blobSlice:
      $.support.blobSlice &&
      function () {
        var slice = this.slice || this.webkitSlice || this.mozSlice;
        return slice.apply(this, arguments);
      },

    _BitrateTimer: function () {
      this.timestamp = Date.now ? Date.now() : new Date().getTime();
      this.loaded = 0;
      this.bitrate = 0;
      this.getBitrate = function (now, loaded, interval) {
        var timeDiff = now - this.timestamp;
        if (!this.bitrate || !interval || timeDiff > interval) {
          this.bitrate = (loaded - this.loaded) * (1000 / timeDiff) * 8;
          this.loaded = loaded;
          this.timestamp = now;
        }
        return this.bitrate;
      };
    },

    _isXHRUpload: function (options) {
      return (
        !options.forceIframeTransport &&
        ((!options.multipart && $.support.xhrFileUpload) ||
          $.support.xhrFormDataFileUpload)
      );
    },

    _getFormData: function (options) {
      var formData;
      if ($.type(options.formData) === 'function') {
        return options.formData(options.form);
      }
      if ($.isArray(options.formData)) {
        return options.formData;
      }
      if ($.type(options.formData) === 'object') {
        formData = [];
        $.each(options.formData, function (name, value) {
          formData.push({ name: name, value: value });
        });
        return formData;
      }
      return [];
    },

    _getTotal: function (files) {
      var total = 0;
      $.each(files, function (index, file) {
        total += file.size || 1;
      });
      return total;
    },

    _initProgressObject: function (obj) {
      var progress = {
        loaded: 0,
        total: 0,
        bitrate: 0
      };
      if (obj._progress) {
        $.extend(obj._progress, progress);
      } else {
        obj._progress = progress;
      }
    },

    _initResponseObject: function (obj) {
      var prop;
      if (obj._response) {
        for (prop in obj._response) {
          if (Object.prototype.hasOwnProperty.call(obj._response, prop)) {
            delete obj._response[prop];
          }
        }
      } else {
        obj._response = {};
      }
    },

    _onProgress: function (e, data) {
      if (e.lengthComputable) {
        var now = Date.now ? Date.now() : new Date().getTime(),
          loaded;
        if (
          data._time &&
          data.progressInterval &&
          now - data._time < data.progressInterval &&
          e.loaded !== e.total
        ) {
          return;
        }
        data._time = now;
        loaded =
          Math.floor(
            (e.loaded / e.total) * (data.chunkSize || data._progress.total)
          ) + (data.uploadedBytes || 0);
        // Add the difference from the previously loaded state
        // to the global loaded counter:
        this._progress.loaded += loaded - data._progress.loaded;
        this._progress.bitrate = this._bitrateTimer.getBitrate(
          now,
          this._progress.loaded,
          data.bitrateInterval
        );
        data._progress.loaded = data.loaded = loaded;
        data._progress.bitrate = data.bitrate = data._bitrateTimer.getBitrate(
          now,
          loaded,
          data.bitrateInterval
        );
        // Trigger a custom progress event with a total data property set
        // to the file size(s) of the current upload and a loaded data
        // property calculated accordingly:
        this._trigger(
          'progress',
          $.Event('progress', { delegatedEvent: e }),
          data
        );
        // Trigger a global progress event for all current file uploads,
        // including ajax calls queued for sequential file uploads:
        this._trigger(
          'progressall',
          $.Event('progressall', { delegatedEvent: e }),
          this._progress
        );
      }
    },

    _initProgressListener: function (options) {
      var that = this,
        xhr = options.xhr ? options.xhr() : $.ajaxSettings.xhr();
      // Access to the native XHR object is required to add event listeners
      // for the upload progress event:
      if (xhr.upload) {
        $(xhr.upload).on('progress', function (e) {
          var oe = e.originalEvent;
          // Make sure the progress event properties get copied over:
          e.lengthComputable = oe.lengthComputable;
          e.loaded = oe.loaded;
          e.total = oe.total;
          that._onProgress(e, options);
        });
        options.xhr = function () {
          return xhr;
        };
      }
    },

    _deinitProgressListener: function (options) {
      var xhr = options.xhr ? options.xhr() : $.ajaxSettings.xhr();
      if (xhr.upload) {
        $(xhr.upload).off('progress');
      }
    },

    _isInstanceOf: function (type, obj) {
      // Cross-frame instanceof check
      return Object.prototype.toString.call(obj) === '[object ' + type + ']';
    },

    _getUniqueFilename: function (name, map) {
      // eslint-disable-next-line no-param-reassign
      name = String(name);
      if (map[name]) {
        // eslint-disable-next-line no-param-reassign
        name = name.replace(
          /(?: \(([\d]+)\))?(\.[^.]+)?$/,
          function (_, p1, p2) {
            var index = p1 ? Number(p1) + 1 : 1;
            var ext = p2 || '';
            return ' (' + index + ')' + ext;
          }
        );
        return this._getUniqueFilename(name, map);
      }
      map[name] = true;
      return name;
    },

    _initXHRData: function (options) {
      var that = this,
        formData,
        file = options.files[0],
        // Ignore non-multipart setting if not supported:
        multipart = options.multipart || !$.support.xhrFileUpload,
        paramName =
          $.type(options.paramName) === 'array'
            ? options.paramName[0]
            : options.paramName;
      options.headers = $.extend({}, options.headers);
      if (options.contentRange) {
        options.headers['Content-Range'] = options.contentRange;
      }
      if (!multipart || options.blob || !this._isInstanceOf('File', file)) {
        options.headers['Content-Disposition'] =
          'attachment; filename="' +
          encodeURI(file.uploadName || file.name) +
          '"';
      }
      if (!multipart) {
        options.contentType = file.type || 'application/octet-stream';
        options.data = options.blob || file;
      } else if ($.support.xhrFormDataFileUpload) {
        if (options.postMessage) {
          // window.postMessage does not allow sending FormData
          // objects, so we just add the File/Blob objects to
          // the formData array and let the postMessage window
          // create the FormData object out of this array:
          formData = this._getFormData(options);
          if (options.blob) {
            formData.push({
              name: paramName,
              value: options.blob
            });
          } else {
            $.each(options.files, function (index, file) {
              formData.push({
                name:
                  ($.type(options.paramName) === 'array' &&
                    options.paramName[index]) ||
                  paramName,
                value: file
              });
            });
          }
        } else {
          if (that._isInstanceOf('FormData', options.formData)) {
            formData = options.formData;
          } else {
            formData = new FormData();
            $.each(this._getFormData(options), function (index, field) {
              formData.append(field.name, field.value);
            });
          }
          if (options.blob) {
            formData.append(
              paramName,
              options.blob,
              file.uploadName || file.name
            );
          } else {
            $.each(options.files, function (index, file) {
              // This check allows the tests to run with
              // dummy objects:
              if (
                that._isInstanceOf('File', file) ||
                that._isInstanceOf('Blob', file)
              ) {
                var fileName = file.uploadName || file.name;
                if (options.uniqueFilenames) {
                  fileName = that._getUniqueFilename(
                    fileName,
                    options.uniqueFilenames
                  );
                }
                formData.append(
                  ($.type(options.paramName) === 'array' &&
                    options.paramName[index]) ||
                    paramName,
                  file,
                  fileName
                );
              }
            });
          }
        }
        options.data = formData;
      }
      // Blob reference is not needed anymore, free memory:
      options.blob = null;
    },

    _initIframeSettings: function (options) {
      var targetHost = $('<a></a>').prop('href', options.url).prop('host');
      // Setting the dataType to iframe enables the iframe transport:
      options.dataType = 'iframe ' + (options.dataType || '');
      // The iframe transport accepts a serialized array as form data:
      options.formData = this._getFormData(options);
      // Add redirect url to form data on cross-domain uploads:
      if (options.redirect && targetHost && targetHost !== location.host) {
        options.formData.push({
          name: options.redirectParamName || 'redirect',
          value: options.redirect
        });
      }
    },

    _initDataSettings: function (options) {
      if (this._isXHRUpload(options)) {
        if (!this._chunkedUpload(options, true)) {
          if (!options.data) {
            this._initXHRData(options);
          }
          this._initProgressListener(options);
        }
        if (options.postMessage) {
          // Setting the dataType to postmessage enables the
          // postMessage transport:
          options.dataType = 'postmessage ' + (options.dataType || '');
        }
      } else {
        this._initIframeSettings(options);
      }
    },

    _getParamName: function (options) {
      var fileInput = $(options.fileInput),
        paramName = options.paramName;
      if (!paramName) {
        paramName = [];
        fileInput.each(function () {
          var input = $(this),
            name = input.prop('name') || 'files[]',
            i = (input.prop('files') || [1]).length;
          while (i) {
            paramName.push(name);
            i -= 1;
          }
        });
        if (!paramName.length) {
          paramName = [fileInput.prop('name') || 'files[]'];
        }
      } else if (!$.isArray(paramName)) {
        paramName = [paramName];
      }
      return paramName;
    },

    _initFormSettings: function (options) {
      // Retrieve missing options from the input field and the
      // associated form, if available:
      if (!options.form || !options.form.length) {
        options.form = $(options.fileInput.prop('form'));
        // If the given file input doesn't have an associated form,
        // use the default widget file input's form:
        if (!options.form.length) {
          options.form = $(this.options.fileInput.prop('form'));
        }
      }
      options.paramName = this._getParamName(options);
      if (!options.url) {
        options.url = options.form.prop('action') || location.href;
      }
      // The HTTP request method must be "POST" or "PUT":
      options.type = (
        options.type ||
        ($.type(options.form.prop('method')) === 'string' &&
          options.form.prop('method')) ||
        ''
      ).toUpperCase();
      if (
        options.type !== 'POST' &&
        options.type !== 'PUT' &&
        options.type !== 'PATCH'
      ) {
        options.type = 'POST';
      }
      if (!options.formAcceptCharset) {
        options.formAcceptCharset = options.form.attr('accept-charset');
      }
    },

    _getAJAXSettings: function (data) {
      var options = $.extend({}, this.options, data);
      this._initFormSettings(options);
      this._initDataSettings(options);
      return options;
    },

    // jQuery 1.6 doesn't provide .state(),
    // while jQuery 1.8+ removed .isRejected() and .isResolved():
    _getDeferredState: function (deferred) {
      if (deferred.state) {
        return deferred.state();
      }
      if (deferred.isResolved()) {
        return 'resolved';
      }
      if (deferred.isRejected()) {
        return 'rejected';
      }
      return 'pending';
    },

    // Maps jqXHR callbacks to the equivalent
    // methods of the given Promise object:
    _enhancePromise: function (promise) {
      promise.success = promise.done;
      promise.error = promise.fail;
      promise.complete = promise.always;
      return promise;
    },

    // Creates and returns a Promise object enhanced with
    // the jqXHR methods abort, success, error and complete:
    _getXHRPromise: function (resolveOrReject, context, args) {
      var dfd = $.Deferred(),
        promise = dfd.promise();
      // eslint-disable-next-line no-param-reassign
      context = context || this.options.context || promise;
      if (resolveOrReject === true) {
        dfd.resolveWith(context, args);
      } else if (resolveOrReject === false) {
        dfd.rejectWith(context, args);
      }
      promise.abort = dfd.promise;
      return this._enhancePromise(promise);
    },

    // Adds convenience methods to the data callback argument:
    _addConvenienceMethods: function (e, data) {
      var that = this,
        getPromise = function (args) {
          return $.Deferred().resolveWith(that, args).promise();
        };
      data.process = function (resolveFunc, rejectFunc) {
        if (resolveFunc || rejectFunc) {
          data._processQueue = this._processQueue = (this._processQueue ||
            getPromise([this]))
            [that._promisePipe](function () {
              if (data.errorThrown) {
                return $.Deferred().rejectWith(that, [data]).promise();
              }
              return getPromise(arguments);
            })
            [that._promisePipe](resolveFunc, rejectFunc);
        }
        return this._processQueue || getPromise([this]);
      };
      data.submit = function () {
        if (this.state() !== 'pending') {
          data.jqXHR = this.jqXHR =
            that._trigger(
              'submit',
              $.Event('submit', { delegatedEvent: e }),
              this
            ) !== false && that._onSend(e, this);
        }
        return this.jqXHR || that._getXHRPromise();
      };
      data.abort = function () {
        if (this.jqXHR) {
          return this.jqXHR.abort();
        }
        this.errorThrown = 'abort';
        that._trigger('fail', null, this);
        return that._getXHRPromise(false);
      };
      data.state = function () {
        if (this.jqXHR) {
          return that._getDeferredState(this.jqXHR);
        }
        if (this._processQueue) {
          return that._getDeferredState(this._processQueue);
        }
      };
      data.processing = function () {
        return (
          !this.jqXHR &&
          this._processQueue &&
          that._getDeferredState(this._processQueue) === 'pending'
        );
      };
      data.progress = function () {
        return this._progress;
      };
      data.response = function () {
        return this._response;
      };
    },

    // Parses the Range header from the server response
    // and returns the uploaded bytes:
    _getUploadedBytes: function (jqXHR) {
      var range = jqXHR.getResponseHeader('Range'),
        parts = range && range.split('-'),
        upperBytesPos = parts && parts.length > 1 && parseInt(parts[1], 10);
      return upperBytesPos && upperBytesPos + 1;
    },

    // Uploads a file in multiple, sequential requests
    // by splitting the file up in multiple blob chunks.
    // If the second parameter is true, only tests if the file
    // should be uploaded in chunks, but does not invoke any
    // upload requests:
    _chunkedUpload: function (options, testOnly) {
      options.uploadedBytes = options.uploadedBytes || 0;
      var that = this,
        file = options.files[0],
        fs = file.size,
        ub = options.uploadedBytes,
        mcs = options.maxChunkSize || fs,
        slice = this._blobSlice,
        dfd = $.Deferred(),
        promise = dfd.promise(),
        jqXHR,
        upload;
      if (
        !(
          this._isXHRUpload(options) &&
          slice &&
          (ub || ($.type(mcs) === 'function' ? mcs(options) : mcs) < fs)
        ) ||
        options.data
      ) {
        return false;
      }
      if (testOnly) {
        return true;
      }
      if (ub >= fs) {
        file.error = options.i18n('uploadedBytes');
        return this._getXHRPromise(false, options.context, [
          null,
          'error',
          file.error
        ]);
      }
      // The chunk upload method:
      upload = function () {
        // Clone the options object for each chunk upload:
        var o = $.extend({}, options),
          currentLoaded = o._progress.loaded;
        o.blob = slice.call(
          file,
          ub,
          ub + ($.type(mcs) === 'function' ? mcs(o) : mcs),
          file.type
        );
        // Store the current chunk size, as the blob itself
        // will be dereferenced after data processing:
        o.chunkSize = o.blob.size;
        // Expose the chunk bytes position range:
        o.contentRange =
          'bytes ' + ub + '-' + (ub + o.chunkSize - 1) + '/' + fs;
        // Trigger chunkbeforesend to allow form data to be updated for this chunk
        that._trigger('chunkbeforesend', null, o);
        // Process the upload data (the blob and potential form data):
        that._initXHRData(o);
        // Add progress listeners for this chunk upload:
        that._initProgressListener(o);
        jqXHR = (
          (that._trigger('chunksend', null, o) !== false && $.ajax(o)) ||
          that._getXHRPromise(false, o.context)
        )
          .done(function (result, textStatus, jqXHR) {
            ub = that._getUploadedBytes(jqXHR) || ub + o.chunkSize;
            // Create a progress event if no final progress event
            // with loaded equaling total has been triggered
            // for this chunk:
            if (currentLoaded + o.chunkSize - o._progress.loaded) {
              that._onProgress(
                $.Event('progress', {
                  lengthComputable: true,
                  loaded: ub - o.uploadedBytes,
                  total: ub - o.uploadedBytes
                }),
                o
              );
            }
            options.uploadedBytes = o.uploadedBytes = ub;
            o.result = result;
            o.textStatus = textStatus;
            o.jqXHR = jqXHR;
            that._trigger('chunkdone', null, o);
            that._trigger('chunkalways', null, o);
            if (ub < fs) {
              // File upload not yet complete,
              // continue with the next chunk:
              upload();
            } else {
              dfd.resolveWith(o.context, [result, textStatus, jqXHR]);
            }
          })
          .fail(function (jqXHR, textStatus, errorThrown) {
            o.jqXHR = jqXHR;
            o.textStatus = textStatus;
            o.errorThrown = errorThrown;
            that._trigger('chunkfail', null, o);
            that._trigger('chunkalways', null, o);
            dfd.rejectWith(o.context, [jqXHR, textStatus, errorThrown]);
          })
          .always(function () {
            that._deinitProgressListener(o);
          });
      };
      this._enhancePromise(promise);
      promise.abort = function () {
        return jqXHR.abort();
      };
      upload();
      return promise;
    },

    _beforeSend: function (e, data) {
      if (this._active === 0) {
        // the start callback is triggered when an upload starts
        // and no other uploads are currently running,
        // equivalent to the global ajaxStart event:
        this._trigger('start');
        // Set timer for global bitrate progress calculation:
        this._bitrateTimer = new this._BitrateTimer();
        // Reset the global progress values:
        this._progress.loaded = this._progress.total = 0;
        this._progress.bitrate = 0;
      }
      // Make sure the container objects for the .response() and
      // .progress() methods on the data object are available
      // and reset to their initial state:
      this._initResponseObject(data);
      this._initProgressObject(data);
      data._progress.loaded = data.loaded = data.uploadedBytes || 0;
      data._progress.total = data.total = this._getTotal(data.files) || 1;
      data._progress.bitrate = data.bitrate = 0;
      this._active += 1;
      // Initialize the global progress values:
      this._progress.loaded += data.loaded;
      this._progress.total += data.total;
    },

    _onDone: function (result, textStatus, jqXHR, options) {
      var total = options._progress.total,
        response = options._response;
      if (options._progress.loaded < total) {
        // Create a progress event if no final progress event
        // with loaded equaling total has been triggered:
        this._onProgress(
          $.Event('progress', {
            lengthComputable: true,
            loaded: total,
            total: total
          }),
          options
        );
      }
      response.result = options.result = result;
      response.textStatus = options.textStatus = textStatus;
      response.jqXHR = options.jqXHR = jqXHR;
      this._trigger('done', null, options);
    },

    _onFail: function (jqXHR, textStatus, errorThrown, options) {
      var response = options._response;
      if (options.recalculateProgress) {
        // Remove the failed (error or abort) file upload from
        // the global progress calculation:
        this._progress.loaded -= options._progress.loaded;
        this._progress.total -= options._progress.total;
      }
      response.jqXHR = options.jqXHR = jqXHR;
      response.textStatus = options.textStatus = textStatus;
      response.errorThrown = options.errorThrown = errorThrown;
      this._trigger('fail', null, options);
    },

    _onAlways: function (jqXHRorResult, textStatus, jqXHRorError, options) {
      // jqXHRorResult, textStatus and jqXHRorError are added to the
      // options object via done and fail callbacks
      this._trigger('always', null, options);
    },

    _onSend: function (e, data) {
      if (!data.submit) {
        this._addConvenienceMethods(e, data);
      }
      var that = this,
        jqXHR,
        aborted,
        slot,
        pipe,
        options = that._getAJAXSettings(data),
        send = function () {
          that._sending += 1;
          // Set timer for bitrate progress calculation:
          options._bitrateTimer = new that._BitrateTimer();
          jqXHR =
            jqXHR ||
            (
              ((aborted ||
                that._trigger(
                  'send',
                  $.Event('send', { delegatedEvent: e }),
                  options
                ) === false) &&
                that._getXHRPromise(false, options.context, aborted)) ||
              that._chunkedUpload(options) ||
              $.ajax(options)
            )
              .done(function (result, textStatus, jqXHR) {
                that._onDone(result, textStatus, jqXHR, options);
              })
              .fail(function (jqXHR, textStatus, errorThrown) {
                that._onFail(jqXHR, textStatus, errorThrown, options);
              })
              .always(function (jqXHRorResult, textStatus, jqXHRorError) {
                that._deinitProgressListener(options);
                that._onAlways(
                  jqXHRorResult,
                  textStatus,
                  jqXHRorError,
                  options
                );
                that._sending -= 1;
                that._active -= 1;
                if (
                  options.limitConcurrentUploads &&
                  options.limitConcurrentUploads > that._sending
                ) {
                  // Start the next queued upload,
                  // that has not been aborted:
                  var nextSlot = that._slots.shift();
                  while (nextSlot) {
                    if (that._getDeferredState(nextSlot) === 'pending') {
                      nextSlot.resolve();
                      break;
                    }
                    nextSlot = that._slots.shift();
                  }
                }
                if (that._active === 0) {
                  // The stop callback is triggered when all uploads have
                  // been completed, equivalent to the global ajaxStop event:
                  that._trigger('stop');
                }
              });
          return jqXHR;
        };
      this._beforeSend(e, options);
      if (
        this.options.sequentialUploads ||
        (this.options.limitConcurrentUploads &&
          this.options.limitConcurrentUploads <= this._sending)
      ) {
        if (this.options.limitConcurrentUploads > 1) {
          slot = $.Deferred();
          this._slots.push(slot);
          pipe = slot[that._promisePipe](send);
        } else {
          this._sequence = this._sequence[that._promisePipe](send, send);
          pipe = this._sequence;
        }
        // Return the piped Promise object, enhanced with an abort method,
        // which is delegated to the jqXHR object of the current upload,
        // and jqXHR callbacks mapped to the equivalent Promise methods:
        pipe.abort = function () {
          aborted = [undefined, 'abort', 'abort'];
          if (!jqXHR) {
            if (slot) {
              slot.rejectWith(options.context, aborted);
            }
            return send();
          }
          return jqXHR.abort();
        };
        return this._enhancePromise(pipe);
      }
      return send();
    },

    _onAdd: function (e, data) {
      var that = this,
        result = true,
        options = $.extend({}, this.options, data),
        files = data.files,
        filesLength = files.length,
        limit = options.limitMultiFileUploads,
        limitSize = options.limitMultiFileUploadSize,
        overhead = options.limitMultiFileUploadSizeOverhead,
        batchSize = 0,
        paramName = this._getParamName(options),
        paramNameSet,
        paramNameSlice,
        fileSet,
        i,
        j = 0;
      if (!filesLength) {
        return false;
      }
      if (limitSize && files[0].size === undefined) {
        limitSize = undefined;
      }
      if (
        !(options.singleFileUploads || limit || limitSize) ||
        !this._isXHRUpload(options)
      ) {
        fileSet = [files];
        paramNameSet = [paramName];
      } else if (!(options.singleFileUploads || limitSize) && limit) {
        fileSet = [];
        paramNameSet = [];
        for (i = 0; i < filesLength; i += limit) {
          fileSet.push(files.slice(i, i + limit));
          paramNameSlice = paramName.slice(i, i + limit);
          if (!paramNameSlice.length) {
            paramNameSlice = paramName;
          }
          paramNameSet.push(paramNameSlice);
        }
      } else if (!options.singleFileUploads && limitSize) {
        fileSet = [];
        paramNameSet = [];
        for (i = 0; i < filesLength; i = i + 1) {
          batchSize += files[i].size + overhead;
          if (
            i + 1 === filesLength ||
            batchSize + files[i + 1].size + overhead > limitSize ||
            (limit && i + 1 - j >= limit)
          ) {
            fileSet.push(files.slice(j, i + 1));
            paramNameSlice = paramName.slice(j, i + 1);
            if (!paramNameSlice.length) {
              paramNameSlice = paramName;
            }
            paramNameSet.push(paramNameSlice);
            j = i + 1;
            batchSize = 0;
          }
        }
      } else {
        paramNameSet = paramName;
      }
      data.originalFiles = files;
      $.each(fileSet || files, function (index, element) {
        var newData = $.extend({}, data);
        newData.files = fileSet ? element : [element];
        newData.paramName = paramNameSet[index];
        that._initResponseObject(newData);
        that._initProgressObject(newData);
        that._addConvenienceMethods(e, newData);
        result = that._trigger(
          'add',
          $.Event('add', { delegatedEvent: e }),
          newData
        );
        return result;
      });
      return result;
    },

    _replaceFileInput: function (data) {
      var input = data.fileInput,
        inputClone = input.clone(true),
        restoreFocus = input.is(document.activeElement);
      // Add a reference for the new cloned file input to the data argument:
      data.fileInputClone = inputClone;
      $('<form></form>').append(inputClone)[0].reset();
      // Detaching allows to insert the fileInput on another form
      // without losing the file input value:
      input.after(inputClone).detach();
      // If the fileInput had focus before it was detached,
      // restore focus to the inputClone.
      if (restoreFocus) {
        inputClone.trigger('focus');
      }
      // Avoid memory leaks with the detached file input:
      $.cleanData(input.off('remove'));
      // Replace the original file input element in the fileInput
      // elements set with the clone, which has been copied including
      // event handlers:
      this.options.fileInput = this.options.fileInput.map(function (i, el) {
        if (el === input[0]) {
          return inputClone[0];
        }
        return el;
      });
      // If the widget has been initialized on the file input itself,
      // override this.element with the file input clone:
      if (input[0] === this.element[0]) {
        this.element = inputClone;
      }
    },

    _handleFileTreeEntry: function (entry, path) {
      var that = this,
        dfd = $.Deferred(),
        entries = [],
        dirReader,
        errorHandler = function (e) {
          if (e && !e.entry) {
            e.entry = entry;
          }
          // Since $.when returns immediately if one
          // Deferred is rejected, we use resolve instead.
          // This allows valid files and invalid items
          // to be returned together in one set:
          dfd.resolve([e]);
        },
        successHandler = function (entries) {
          that
            ._handleFileTreeEntries(entries, path + entry.name + '/')
            .done(function (files) {
              dfd.resolve(files);
            })
            .fail(errorHandler);
        },
        readEntries = function () {
          dirReader.readEntries(function (results) {
            if (!results.length) {
              successHandler(entries);
            } else {
              entries = entries.concat(results);
              readEntries();
            }
          }, errorHandler);
        };
      // eslint-disable-next-line no-param-reassign
      path = path || '';
      if (entry.isFile) {
        if (entry._file) {
          // Workaround for Chrome bug #149735
          entry._file.relativePath = path;
          dfd.resolve(entry._file);
        } else {
          entry.file(function (file) {
            file.relativePath = path;
            dfd.resolve(file);
          }, errorHandler);
        }
      } else if (entry.isDirectory) {
        dirReader = entry.createReader();
        readEntries();
      } else {
        // Return an empty list for file system items
        // other than files or directories:
        dfd.resolve([]);
      }
      return dfd.promise();
    },

    _handleFileTreeEntries: function (entries, path) {
      var that = this;
      return $.when
        .apply(
          $,
          $.map(entries, function (entry) {
            return that._handleFileTreeEntry(entry, path);
          })
        )
        [this._promisePipe](function () {
          return Array.prototype.concat.apply([], arguments);
        });
    },

    _getDroppedFiles: function (dataTransfer) {
      // eslint-disable-next-line no-param-reassign
      dataTransfer = dataTransfer || {};
      var items = dataTransfer.items;
      if (
        items &&
        items.length &&
        (items[0].webkitGetAsEntry || items[0].getAsEntry)
      ) {
        return this._handleFileTreeEntries(
          $.map(items, function (item) {
            var entry;
            if (item.webkitGetAsEntry) {
              entry = item.webkitGetAsEntry();
              if (entry) {
                // Workaround for Chrome bug #149735:
                entry._file = item.getAsFile();
              }
              return entry;
            }
            return item.getAsEntry();
          })
        );
      }
      return $.Deferred().resolve($.makeArray(dataTransfer.files)).promise();
    },

    _getSingleFileInputFiles: function (fileInput) {
      // eslint-disable-next-line no-param-reassign
      fileInput = $(fileInput);
      var entries = fileInput.prop('entries'),
        files,
        value;
      if (entries && entries.length) {
        return this._handleFileTreeEntries(entries);
      }
      files = $.makeArray(fileInput.prop('files'));
      if (!files.length) {
        value = fileInput.prop('value');
        if (!value) {
          return $.Deferred().resolve([]).promise();
        }
        // If the files property is not available, the browser does not
        // support the File API and we add a pseudo File object with
        // the input value as name with path information removed:
        files = [{ name: value.replace(/^.*\\/, '') }];
      } else if (files[0].name === undefined && files[0].fileName) {
        // File normalization for Safari 4 and Firefox 3:
        $.each(files, function (index, file) {
          file.name = file.fileName;
          file.size = file.fileSize;
        });
      }
      return $.Deferred().resolve(files).promise();
    },

    _getFileInputFiles: function (fileInput) {
      if (!(fileInput instanceof $) || fileInput.length === 1) {
        return this._getSingleFileInputFiles(fileInput);
      }
      return $.when
        .apply($, $.map(fileInput, this._getSingleFileInputFiles))
        [this._promisePipe](function () {
          return Array.prototype.concat.apply([], arguments);
        });
    },

    _onChange: function (e) {
      var that = this,
        data = {
          fileInput: $(e.target),
          form: $(e.target.form)
        };
      this._getFileInputFiles(data.fileInput).always(function (files) {
        data.files = files;
        if (that.options.replaceFileInput) {
          that._replaceFileInput(data);
        }
        if (
          that._trigger(
            'change',
            $.Event('change', { delegatedEvent: e }),
            data
          ) !== false
        ) {
          that._onAdd(e, data);
        }
      });
    },

    _onPaste: function (e) {
      var items =
          e.originalEvent &&
          e.originalEvent.clipboardData &&
          e.originalEvent.clipboardData.items,
        data = { files: [] };
      if (items && items.length) {
        $.each(items, function (index, item) {
          var file = item.getAsFile && item.getAsFile();
          if (file) {
            data.files.push(file);
          }
        });
        if (
          this._trigger(
            'paste',
            $.Event('paste', { delegatedEvent: e }),
            data
          ) !== false
        ) {
          this._onAdd(e, data);
        }
      }
    },

    _onDrop: function (e) {
      e.dataTransfer = e.originalEvent && e.originalEvent.dataTransfer;
      var that = this,
        dataTransfer = e.dataTransfer,
        data = {};
      if (dataTransfer && dataTransfer.files && dataTransfer.files.length) {
        e.preventDefault();
        this._getDroppedFiles(dataTransfer).always(function (files) {
          data.files = files;
          if (
            that._trigger(
              'drop',
              $.Event('drop', { delegatedEvent: e }),
              data
            ) !== false
          ) {
            that._onAdd(e, data);
          }
        });
      }
    },

    _onDragOver: getDragHandler('dragover'),

    _onDragEnter: getDragHandler('dragenter'),

    _onDragLeave: getDragHandler('dragleave'),

    _initEventHandlers: function () {
      if (this._isXHRUpload(this.options)) {
        this._on(this.options.dropZone, {
          dragover: this._onDragOver,
          drop: this._onDrop,
          // event.preventDefault() on dragenter is required for IE10+:
          dragenter: this._onDragEnter,
          // dragleave is not required, but added for completeness:
          dragleave: this._onDragLeave
        });
        this._on(this.options.pasteZone, {
          paste: this._onPaste
        });
      }
      if ($.support.fileInput) {
        this._on(this.options.fileInput, {
          change: this._onChange
        });
      }
    },

    _destroyEventHandlers: function () {
      this._off(this.options.dropZone, 'dragenter dragleave dragover drop');
      this._off(this.options.pasteZone, 'paste');
      this._off(this.options.fileInput, 'change');
    },

    _destroy: function () {
      this._destroyEventHandlers();
    },

    _setOption: function (key, value) {
      var reinit = $.inArray(key, this._specialOptions) !== -1;
      if (reinit) {
        this._destroyEventHandlers();
      }
      this._super(key, value);
      if (reinit) {
        this._initSpecialOptions();
        this._initEventHandlers();
      }
    },

    _initSpecialOptions: function () {
      var options = this.options;
      if (options.fileInput === undefined) {
        options.fileInput = this.element.is('input[type="file"]')
          ? this.element
          : this.element.find('input[type="file"]');
      } else if (!(options.fileInput instanceof $)) {
        options.fileInput = $(options.fileInput);
      }
      if (!(options.dropZone instanceof $)) {
        options.dropZone = $(options.dropZone);
      }
      if (!(options.pasteZone instanceof $)) {
        options.pasteZone = $(options.pasteZone);
      }
    },

    _getRegExp: function (str) {
      var parts = str.split('/'),
        modifiers = parts.pop();
      parts.shift();
      return new RegExp(parts.join('/'), modifiers);
    },

    _isRegExpOption: function (key, value) {
      return (
        key !== 'url' &&
        $.type(value) === 'string' &&
        /^\/.*\/[igm]{0,3}$/.test(value)
      );
    },

    _initDataAttributes: function () {
      var that = this,
        options = this.options,
        data = this.element.data();
      // Initialize options set via HTML5 data-attributes:
      $.each(this.element[0].attributes, function (index, attr) {
        var key = attr.name.toLowerCase(),
          value;
        if (/^data-/.test(key)) {
          // Convert hyphen-ated key to camelCase:
          key = key.slice(5).replace(/-[a-z]/g, function (str) {
            return str.charAt(1).toUpperCase();
          });
          value = data[key];
          if (that._isRegExpOption(key, value)) {
            value = that._getRegExp(value);
          }
          options[key] = value;
        }
      });
    },

    _create: function () {
      this._initDataAttributes();
      this._initSpecialOptions();
      this._slots = [];
      this._sequence = this._getXHRPromise(true);
      this._sending = this._active = 0;
      this._initProgressObject(this);
      this._initEventHandlers();
    },

    // This method is exposed to the widget API and allows to query
    // the number of active uploads:
    active: function () {
      return this._active;
    },

    // This method is exposed to the widget API and allows to query
    // the widget upload progress.
    // It returns an object with loaded, total and bitrate properties
    // for the running uploads:
    progress: function () {
      return this._progress;
    },

    // This method is exposed to the widget API and allows adding files
    // using the fileupload API. The data parameter accepts an object which
    // must have a files property and can contain additional options:
    // .fileupload('add', {files: filesList});
    add: function (data) {
      var that = this;
      if (!data || this.options.disabled) {
        return;
      }
      if (data.fileInput && !data.files) {
        this._getFileInputFiles(data.fileInput).always(function (files) {
          data.files = files;
          that._onAdd(null, data);
        });
      } else {
        data.files = $.makeArray(data.files);
        this._onAdd(null, data);
      }
    },

    // This method is exposed to the widget API and allows sending files
    // using the fileupload API. The data parameter accepts an object which
    // must have a files or fileInput property and can contain additional options:
    // .fileupload('send', {files: filesList});
    // The method returns a Promise object for the file upload call.
    send: function (data) {
      if (data && !this.options.disabled) {
        if (data.fileInput && !data.files) {
          var that = this,
            dfd = $.Deferred(),
            promise = dfd.promise(),
            jqXHR,
            aborted;
          promise.abort = function () {
            aborted = true;
            if (jqXHR) {
              return jqXHR.abort();
            }
            dfd.reject(null, 'abort', 'abort');
            return promise;
          };
          this._getFileInputFiles(data.fileInput).always(function (files) {
            if (aborted) {
              return;
            }
            if (!files.length) {
              dfd.reject();
              return;
            }
            data.files = files;
            jqXHR = that._onSend(null, data);
            jqXHR.then(
              function (result, textStatus, jqXHR) {
                dfd.resolve(result, textStatus, jqXHR);
              },
              function (jqXHR, textStatus, errorThrown) {
                dfd.reject(jqXHR, textStatus, errorThrown);
              }
            );
          });
          return this._enhancePromise(promise);
        }
        data.files = $.makeArray(data.files);
        if (data.files.length) {
          return this._onSend(null, data);
        }
      }
      return this._getXHRPromise(false, data && data.context);
    }
  });
});


// <upload> component
Vue.component('upload', {
    template: '\
        <div :class="{ \'upload-warning\' : model.errorMessage }" class="upload m-2 p-2 pt-0"> \
            <span v-if="model.errorMessage" v-on:click="dismissWarning()" class="close-warning"><i class="fa-solid fa-xmark" aria-hidden="true"></i> </span>\
            <p class="upload-name" :title="model.errorMessage">{{ model.name }}</p> \
            <div> \
               <span v-show="!model.errorMessage" :style="{ width: model.percentage + \'%\'}" class="progress-bar"> </span> \
               <span v-if="model.errorMessage" class="error-message" :title="model.errorMessage"> Error: {{ model.errorMessage }} </span> \
            </div> \
        </div> \
        ',
    props: {
        model: Object,
        uploadInputId: String
    },
    mounted: function () {
        var self = this;
        var uploadInput = document.getElementById(self.uploadInputId ?? 'fileupload');
        $(uploadInput).bind('fileuploadprogress', function (e, data) {
            if (data.files[0].name !== self.model.name) {
                return;
            }            
            self.model.percentage = parseInt(data.loaded / data.total * 100, 10);
        });

        $(uploadInput).bind('fileuploaddone', function (e, data) {
            if (data.files[0].name !== self.model.name) {
                return;
            }
            if (data.result.files[0].error) {
                self.handleFailure(data.files[0].name, data.result.files[0].error);
            } else {  
                bus.$emit('removalRequest', self.model);
            }
        });

        $(uploadInput).bind('fileuploadfail', function (e, data) {
            if (data.files[0].name !== self.model.name) {
                return;
            }
            self.handleFailure(data.files[0].name, $('#t-error').val());
        });
    },
    methods: {
        handleFailure: function (fileName, message) {
            if (fileName !== this.model.name) {
                return;
            }
            this.model.errorMessage = message;
            bus.$emit('ErrorOnUpload', this.model);
        },
        dismissWarning: function () {
            bus.$emit('removalRequest', this.model);
        }
    }
});


// <upload-list> component
Vue.component('uploadList', {
    template: '\
        <div class="upload-list" v-show="files.length > 0"> \
            <div class="header" @click="expanded = !expanded"> \
                <span> {{ T.uploads }} </span> \
                <span v-show="pendingCount"> (Pending: {{ pendingCount }}) </span> \
                <span v-show="errorCount" :class="{ \'text-danger\' : errorCount }"> ( {{ T.errors }}: {{ errorCount }} / <a href="javascript:;" v-on:click.stop="clearErrors" > {{ T.clearErrors }} </a>)</span> \
                    <div class="toggle-button"> \
                    <div v-show="expanded"> \
                        <i class="fa-solid fa-chevron-down" aria-hidden="true"></i> \
                    </div> \
                    <div v-show="!expanded"> \
                        <i class="fa-solid fa-chevron-up" aria-hidden="true"></i> \
                    </div> \
                </div> \
            </div> \
            <div class="card-body" v-show="expanded"> \
                <div class="d-flex flex-wrap"> \
                    <upload :upload-input-id="uploadInputId" v-for="f in files" :key="f.name"  :model="f"></upload> \
                </div > \
            </div> \
        </div> \
        ',
    data: function () {
        return {
            files: [],
            T: {},
            expanded: false,
            pendingCount: 0,
            errorCount: 0
        }
    },
    props: {
        uploadInputId: String
    },
    created: function () {
        var self = this;
        // retrieving localized strings from view
        self.T.uploads = $('#t-uploads').val();
        self.T.errors = $('#t-errors').val();
        self.T.clearErrors = $('#t-clear-errors').val();
    },
    computed: {
        fileCount: function () {
            return this.files.length;
        }
    },
    mounted: function () {
        var self = this;
        var uploadInput = document.getElementById(self.uploadInputId ?? 'fileupload');
        $(uploadInput).bind('fileuploadadd', function (e, data) {
            if (!data.files) {
                return;
            }
            data.files.forEach(function (newFile) {
                var alreadyInList = self.files.some(function (f) {
                    return f.name == newFile.name;
                });

                if (!alreadyInList) {
                    self.files.push({ name: newFile.name, percentage: 0, errorMessage: '' });
                } else {
                    console.error('A file with the same name is already on the queue:' + newFile.name);
                }
            });
        });

        bus.$on('removalRequest', function (fileUpload) {
            self.files.forEach(function (item, index, array) {
                if (item.name == fileUpload.name) {
                    array.splice(index, 1);
                }
            });
        });

        bus.$on('ErrorOnUpload', function (fileUpload) {
            self.updateCount();
        });
    },
    methods: {
        updateCount: function () {
            this.errorCount = this.files.filter(function (item) {
                return item.errorMessage != '';
            }).length;
            this.pendingCount = this.files.length - this.errorCount;
            if (this.files.length < 1) {
                this.expanded = false;
            }
        },
        clearErrors: function () {
            this.files = this.files.filter(function (item) {
                return item.errorMessage == '';
            });
        }
    },
    watch: {
        files: function () {
            this.updateCount();
        }
    }
});


﻿// https://github.com/spatie/font-awesome-filetypes

const faIcons = {
    image: 'fa-regular fa-image',
    pdf: 'fa-regular fa-file-pdf',
    word: 'fa-regular fa-file-word',
    powerpoint: 'fa-regular fa-file-powerpoint',
    excel: 'fa-regular fa-file-excel',
    csv: 'fa-regular fa-file',
    audio: 'fa-regular fa-file-audio',
    video: 'fa-regular fa-file-video',
    archive: 'fa-regular fa-file-zipper',
    code: 'fa-regular fa-file-code',
    text: 'fa-regular fa-file-lines',
    file: 'fa-regular fa-file'
};

const faThumbnails = {
    gif: faIcons.image,
    jpeg: faIcons.image,
    jpg: faIcons.image,
    png: faIcons.image,
    pdf: faIcons.pdf,
    doc: faIcons.word,
    docx: faIcons.word,
    ppt: faIcons.powerpoint,
    pptx: faIcons.powerpoint,
    xls: faIcons.excel,
    xlsx: faIcons.excel,
    csv: faIcons.csv,
    aac: faIcons.audio,
    mp3: faIcons.audio,
    ogg: faIcons.audio,
    avi: faIcons.video,
    flv: faIcons.video,
    mkv: faIcons.video,
    mp4: faIcons.video,
    webm: faIcons.video,
    gz: faIcons.archive,
    zip: faIcons.archive,
    css: faIcons.code,
    html: faIcons.code,
    js: faIcons.code,
    txt: faIcons.text
};

function getClassNameForExtension(extension) {
    return faThumbnails[extension.toLowerCase()] || faIcons.file
}

function getExtensionForFilename(filename) {
    return filename.slice((filename.lastIndexOf('.') - 1 >>> 0) + 2)
}

function getClassNameForFilename(filename) {
    return getClassNameForExtension(getExtensionForFilename(filename))
}


// This component receives a list of all the items, unpaged.
// As the user interacts with the pager, it raises events with the items in the current page.
// It's the parent's responsibility to listen for these events and display the received items
// <pager> component
Vue.component('pager', {
    template: `
    <div>
        <nav id="media-pager" class="d-flex justify-content-center" aria-label="Pagination Navigation" role="navigation" :data-computed-trigger="itemsInCurrentPage.length">
            <ul class="pagination pagination-sm m-0">
                <li class="page-item media-first-button" :class="{disabled : !canDoFirst}">
                    <a class="page-link" href="#" :tabindex="canDoFirst ? 0 : -1" v-on:click="goFirst">{{ T.pagerFirstButton }}</a>
                </li>
                <li class="page-item" :class="{disabled : !canDoPrev}">
                    <a class="page-link" href="#" :tabindex="canDoPrev ? 0 : -1" v-on:click="previous">{{ T.pagerPreviousButton }}</a>
                </li>
                <li v-if="link !== -1" class="page-item page-number"  :class="{active : current == link - 1}" v-for="link in pageLinks">
                    <a class="page-link" href="#" v-on:click="goTo(link - 1)" :aria-label="'Goto Page' + link">
                        {{link}}
                        <span v-if="current == link -1" class="visually-hidden">(current)</span>
                    </a>
                </li>
                <li class="page-item" :class="{disabled : !canDoNext}">
                    <a class="page-link" href="#" :tabindex="canDoNext ? 0 : -1" v-on:click="next">{{ T.pagerNextButton }}</a>
                </li>
                <li class="page-item media-last-button" :class="{disabled : !canDoLast}">
                    <a class="page-link" href="#" :tabindex="canDoLast ? 0 : -1" v-on:click="goLast">{{ T.pagerLastButton }}</a>
                </li>
                <li class="page-item ms-4 page-size-info">
                    <div style="display: flex;">
                        <span class="page-link disabled text-muted page-size-label">{{ T.pagerPageSizeLabel }}</span>
                        <select id="pageSizeSelect" class="page-link" v-model="pageSize">
                            <option v-for="option in pageSizeOptions" v-bind:value="option">
                                {{option}}
                            </option>
                        </select>
                    </div>
                </li>
            </ul>
        </nav>
        <nav class="d-flex justify-content-center">
            <ul class="pagination pagination-sm m-0 mt-2">
                <li class="page-item ms-4 page-info">
                    <span class="page-link disabled text-muted ">{{ T.pagerPageLabel }} {{current + 1}}/{{totalPages}}</span>
                </li>
                <li class="page-item ms-4 total-info">
                    <span class="page-link disabled text-muted "> {{ T.pagerTotalLabel }} {{total}}</span>
                </li>
            </ul>
        </nav>
        </div>
        `,
    props: {
        sourceItems: Array
    },
    data: function () {
        return {
            pageSize: 10,
            pageSizeOptions: [10, 30, 50, 100],
            current: 0,
            T: {}
        };
    },
    created: function () {
        var self = this;

        // retrieving localized strings from view
        self.T.pagerFirstButton = $('#t-pager-first-button').val();
        self.T.pagerPreviousButton = $('#t-pager-previous-button').val();
        self.T.pagerNextButton = $('#t-pager-next-button').val();
        self.T.pagerLastButton = $('#t-pager-last-button').val();
        self.T.pagerPageSizeLabel = $('#t-pager-page-size-label').val();
        self.T.pagerPageLabel = $('#t-pager-page-label').val();
        self.T.pagerTotalLabel = $('#t-pager-total-label').val();        
    },
    methods: {
        next: function () {
            this.current = this.current + 1;
        },
        previous: function () {
            this.current = this.current - 1;
        },
        goFirst: function () {
            this.current = 0;
        },
        goLast: function () {
            this.current = this.totalPages - 1;
        },
        goTo: function (targetPage) {
            this.current = targetPage;
        }
    },
    computed: {
        total: function () {
            return this.sourceItems ? this.sourceItems.length : 0;
        },
        totalPages: function () {
            var pages = Math.ceil(this.total / this.pageSize);
            return pages > 0 ? pages : 1;
        },
        isLastPage: function () {
            return this.current + 1 >= this.totalPages;
        },
        isFirstPage: function () {
            return this.current === 0;
        },
        canDoNext: function () {
            return !this.isLastPage;
        },
        canDoPrev: function () {
            return !this.isFirstPage;
        },
        canDoFirst: function () {
            return !this.isFirstPage;
        },
        canDoLast: function () {
            return !this.isLastPage;
        },
        // this computed is only to have a central place where we detect changes and leverage Vue JS reactivity to raise our event.
        // That event will be handled by the parent media app to display the items in the page.
        // this logic will not run if the computed property is not used in the template. We use a dummy "data-computed-trigger" attribute for that.
        itemsInCurrentPage: function () {
            var start = this.pageSize * this.current;
            var end = start + this.pageSize;
            var result = this.sourceItems.slice(start, end);
            bus.$emit('pagerEvent', result);
            return result;
        },
        pageLinks: function () {

            var links = [];

            links.push(this.current + 1);

            // Add 2 items before current
            var beforeCurrent = this.current > 0 ? this.current : -1;
            links.unshift(beforeCurrent);

            var beforeBeforeCurrent = this.current > 1 ? this.current - 1 : -1;
            links.unshift(beforeBeforeCurrent);


            // Add 2 items after current
            var afterCurrent = this.totalPages - this.current > 1 ? this.current + 2 : -1;
            links.push(afterCurrent);

            var afterAfterCurrent = this.totalPages - this.current > 2 ? this.current + 3 : -1;
            links.push(afterAfterCurrent);

            return links;
        }
    },
    watch: {
        sourceItems: function () {
            this.current = 0; // resetting current page after receiving a new list of unpaged items
        },
        pageSize: function () {
            this.current = 0;
        }
    }
});


// <sort-indicator> component
Vue.component('sortIndicator', {
    template: `
        <div v-show="isActive" class="sort-indicator">
            <span v-show="asc"><i class="small fa fa-chevron-up"></i></span>
            <span v-show="!asc"><i class="small fa fa-chevron-down"></i></span>
        </div>
        `,
    props: {
        colname: String,
        selectedcolname: String,
        asc: Boolean
    },
    computed: {
        isActive: function () {
            return this.colname.toLowerCase() == this.selectedcolname.toLowerCase();
        }
    }
});


// <folder> component
Vue.component('folder', {
    template: `
        <li :class="{selected: isSelected}" 
                v-on:dragleave.prevent = "handleDragLeave($event);" 
                v-on:dragover.prevent.stop="handleDragOver($event);" 
                v-on:drop.prevent.stop = "moveMediaToFolder(model, $event)" >
            <div :class="{folderhovered: isHovered , treeroot: level == 1}" >
                <a href="javascript:;" :style="{ padding${document.dir == "ltr" ? "Left" : "Right"}:padding + 'px' }" v-on:click="select"  draggable="false" class="folder-menu-item">
                  <span v-on:click.stop="toggle" class="expand" :class="{opened: open, closed: !open, empty: empty}"><i v-if="open" class="fa-solid fa-chevron-${document.dir == "ltr" ? "right" : "left"}"></i></span> 
                  <div class="folder-name ms-2">{{model.name}}</div>
                    <div class="btn-group folder-actions" >
                            <a v-cloak href="javascript:;" class="btn btn-sm" v-on:click="createFolder" v-if="canCreateFolder && (isSelected || isRoot)"><i class="fa-solid fa-plus" aria-hidden="true"></i></a>
                            <a v-cloak href="javascript:;" class="btn btn-sm" v-on:click="deleteFolder" v-if="canDeleteFolder && isSelected && !isRoot"><i class="fa-solid fa-trash" aria-hidden="true"></i></a>
                    </div>
                </a>
            </div>
            <ol v-show="open">
                <folder v-for="folder in children"
                        :key="folder.path"
                        :model="folder"
                        :selected-in-media-app="selectedInMediaApp"
                        :level="level + 1">
                </folder>
            </ol>
        </li>
        `,
    props: {
        model: Object,
        selectedInMediaApp: Object,
        level: Number
    },
    data: function () {
        return {
            open: false,
            children: null, // not initialized state (for lazy-loading)
            parent: null,
            isHovered: false,
            padding: 0
        }
    },
    computed: {
        empty: function () {
            return !this.children || this.children.length == 0;
        },
        isSelected: function () {
            return (this.selectedInMediaApp.name == this.model.name) && (this.selectedInMediaApp.path == this.model.path);
        },
        isRoot: function () {
            return this.model.path === '';
        },
        canCreateFolder: function () {
            return this.model.canCreateFolder !== undefined ? this.model.canCreateFolder : true;
        },
        canDeleteFolder: function () {
            return this.model.canDeleteFolder !== undefined ? this.model.canDeleteFolder : true;
        }
    },
    mounted: function () {
        if ((this.isRoot == false) && (this.isAncestorOfSelectedFolder())){
            this.toggle();
        }

        this.padding = this.level < 3 ?  16 : 16 + (this.level * 8);
    },
    created: function () {
        var self = this;
        bus.$on('deleteFolder', function (folder) {
            if (self.children) {
                var index = self.children && self.children.indexOf(folder)
                if (index > -1) {
                    self.children.splice(index, 1)
                    bus.$emit('folderDeleted');
                }
            }
        });

        bus.$on('addFolder', function (target, folder) {
            if (self.model == target) {
                if (self.children !== null) {
                    self.children.push(folder);
                }                
                folder.parent = self.model;
                bus.$emit('folderAdded', folder);
            }
        });
    },
    methods: {
        isAncestorOfSelectedFolder: function () {
            parentFolder = mediaApp.selectedFolder;
            while (parentFolder) {
                if (parentFolder.path == this.model.path) {
                    return true;
                }
            parentFolder = parentFolder.parent;
            }

            return false;
        },
        toggle: function () {
            this.open = !this.open;
            if (this.open && !this.children) {
                this.loadChildren();
            }
        },
        select: function () {
            bus.$emit('folderSelected', this.model);
            this.loadChildren();
        },
        createFolder: function () {           
            bus.$emit('createFolderRequested');
        },
        deleteFolder: function () {
            bus.$emit('deleteFolderRequested');
        },
        loadChildren: function () {            
            var self = this;
            if (this.open == false) {
                this.open = true;
            }
            $.ajax({
                url: $('#getFoldersUrl').val() + "?path=" + encodeURIComponent(self.model.path),
                method: 'GET',
                success: function (data) {
                    self.children = data;
                    self.children.forEach(function (c) {
                        c.parent = self.model;
                    });
                },
                error: function (error) {
                    emtpy = false;
                    console.error(error.responseText);
                }
            });
        },
        handleDragOver: function (e) {
            this.isHovered = true;
        },
        handleDragLeave: function (e) {
            this.isHovered = false;            
        },
        moveMediaToFolder: function (folder, e) {

            var self = this;
            self.isHovered = false;

            var mediaNames = JSON.parse(e.dataTransfer.getData('mediaNames')); 

            if (mediaNames.length < 1) {
                return;
            }

            var sourceFolder = e.dataTransfer.getData('sourceFolder');
            var targetFolder = folder.path;

            if (sourceFolder === '') {
                sourceFolder = 'root';
            }

            if (targetFolder === '') {
                targetFolder = 'root';
            }

            if (sourceFolder === targetFolder) {
                alert($('#sameFolderMessage').val());
                return;
            }

            confirmDialog({...$("#moveMedia").data(), callback: function (resp) {
                if (resp) {
                    $.ajax({
                        url: $('#moveMediaListUrl').val(),
                        method: 'POST',
                        data: {
                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
                            mediaNames: mediaNames,
                            sourceFolder: sourceFolder,
                            targetFolder: targetFolder
                        },
                        success: function () {
                            bus.$emit('mediaListMoved'); // MediaApp will listen to this, and then it will reload page so the moved medias won't be there anymore
                        },
                        error: function (error) {
                            console.error(error.responseText);
                            bus.$emit('mediaListMoved', error.responseText);
                        }
                    });
                }
            }});
        }

    }
});


// <media-items-table> component
Vue.component('media-items-table', {
    template: `
        <table class="table media-items-table m-0">
            <thead>
                <tr class="header-row">
                    <th scope="col" class="thumbnail-column">{{ T.imageHeader }}</th>
                    <th scope="col" v-on:click="changeSort('name')">
                       {{ T.nameHeader }}
                         <sort-indicator colname="name" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                    </th>
                    <th scope="col" v-on:click="changeSort('lastModify')"> 
                       {{ T.lastModifyHeader }} 
                         <sort-indicator colname="lastModify" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator> 
                    </th> 
                    <th scope="col" v-on:click="changeSort('size')">
                        <span class="optional-col">
                            {{ T.sizeHeader }}
                         <sort-indicator colname="size" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                        </span>
                    </th>
                    <th scope="col" v-on:click="changeSort('mime')">
                        <span class="optional-col">
                           {{ T.typeHeader }}
                         <sort-indicator colname="mime" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                        </span>
                    </th>
                </tr>
            </thead>
            <tbody>
                    <tr v-for="media in filteredMediaItems"
                          class="media-item"
                          :class="{selected: isMediaSelected(media)}"
                          v-on:click.stop="toggleSelectionOfMedia(media)"
                          draggable="true" v-on:dragstart="dragStart(media, $event)"
                          :key="media.name">
                             <td class="thumbnail-column">
                                <div class="img-wrapper">
                                    <img v-if="media.mime.startsWith('image')" draggable="false" :src="buildMediaUrl(media.url, thumbSize)" />
                                    <i v-else :class="getfontAwesomeClassNameForFileName(media.name, \'fa-4x\')" :data-mime="media.mime"></i>
                                </div>
                            </td>
                            <td>
                                <div class="media-name-cell">
                                   <span class="break-word"> {{ media.name }} </span>
                                    <div class="buttons-container">
                                        <a href="javascript:;" class="btn btn-link btn-sm me-1 edit-button" v-on:click.stop="renameMedia(media)"> {{ T.editButton }} </a >
                                        <a href="javascript:;" class="btn btn-link btn-sm delete-button" v-on:click.stop="deleteMedia(media)"> {{ T.deleteButton }} </a>
                                        <a :href="media.url" target="_blank" class="btn btn-link btn-sm view-button"> {{ T.viewButton }} </a>
                                    </div>
                                </div>
                            </td>
                            <td>
                                <div class="text-col"> {{ printDateTime(media.lastModify) }} </div>
                            </td>
                            <td>
                                <div class="text-col optional-col"> {{ isNaN(media.size)? 0 : Math.round(media.size / 1024) }} KB</div>
                            </td>
                            <td>
                                <div class="text-col optional-col">{{ media.mime }}</div>
                            </td>
                   </tr>
            </tbody>
        </table>
        `,
    data: function () {
        return {
            T: {}
        }
    },
    props: {
        sortBy: String,
        sortAsc: Boolean,
        filteredMediaItems: Array,
        selectedMedias: Array,
        thumbSize: Number
    },
    created: function () {
        var self = this;
        self.T.imageHeader = $('#t-image-header').val();
        self.T.nameHeader = $('#t-name-header').val();
        self.T.lastModifyHeader = $('#t-lastModify-header').val();
        self.T.sizeHeader = $('#t-size-header').val();
        self.T.typeHeader = $('#t-type-header').val();
        self.T.editButton = $('#t-edit-button').val();
        self.T.deleteButton = $('#t-delete-button').val();
        self.T.viewButton = $('#t-view-button').val();
    },
    methods: {
        isMediaSelected: function (media) {
            var result = this.selectedMedias.some(function (element, index, array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url, thumbSize) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        changeSort: function (newSort) {
            bus.$emit('sortChangeRequested', newSort);
        },
        toggleSelectionOfMedia: function (media) {
            bus.$emit('mediaToggleRequested', media);
        },
        renameMedia: function (media) {
            bus.$emit('renameMediaRequested', media);            
        },
        deleteMedia: function (media) {
            bus.$emit('deleteMediaRequested', media);
        },
        dragStart: function (media, e) {
            bus.$emit('mediaDragStartRequested', media, e);
        },
        printDateTime: function (datemillis){
            var d = new Date(datemillis);
            return d.toLocaleString();            
        },
        getfontAwesomeClassNameForFileName:function getfontAwesomeClassNameForFilename(filename, thumbsize){
             return   getClassNameForFilename(filename) + ' ' + thumbsize;
        }
    }
});


// <media-items-grid> component
Vue.component('media-items-grid', {
    template: `
        <ol class="row media-items-grid">
                <li v-for="media in filteredMediaItems"
                    :key="media.name" 
                    class="media-item media-container-main-list-item card p-0"
                    :style="{width: thumbSize + 2 + 'px'}"
                    :class="{selected: isMediaSelected(media)}"
                    v-on:click.stop="toggleSelectionOfMedia(media)"
                    draggable="true" v-on:dragstart="dragStart(media, $event)">
                    <div class="thumb-container" :style="{height: thumbSize +'px'}">
                        <img v-if="media.mime.startsWith('image')"
                                :src="buildMediaUrl(media.url, thumbSize)"
                                :data-mime="media.mime"
                                :style="{maxHeight: thumbSize +'px', maxWidth: thumbSize +'px'}" />
                        <i v-else :class="getfontAwesomeClassNameForFileName(media.name, \'fa-5x\')" :data-mime="media.mime"></i>
                    </div>
                <div class="media-container-main-item-title card-body">
                        <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button edit-button" v-on:click.stop="renameMedia(media)"><i class="fa-solid fa-pen-to-square" aria-hidden="true"></i></a>
                        <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button" v-on:click.stop="deleteMedia(media)"><i class="fa-solid fa-trash" aria-hidden="true"></i></a>
                        <a :href="media.url" target="_blank" class="btn btn-light btn-sm float-end inline-media-button view-button""><i class="fa-solid fa-download" aria-hidden="true"></i></a>
                        <span class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
                    </div>
                 </li>
        </ol>
        `,
    data: function () {
        return {
            T: {}
        }
    },
    props: {
        filteredMediaItems: Array,
        selectedMedias: Array,
        thumbSize: Number
    },
    created: function () {
        var self = this;
        // retrieving localized strings from view
        self.T.editButton = $('#t-edit-button').val();
        self.T.deleteButton = $('#t-delete-button').val();
    },
    methods: {
        isMediaSelected: function (media) {
            var result = this.selectedMedias.some(function (element, index, array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url, thumbSize) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        toggleSelectionOfMedia: function (media) {
            bus.$emit('mediaToggleRequested', media);
        },
        renameMedia: function (media) {
            bus.$emit('renameMediaRequested', media);
        },
        deleteMedia: function (media) {
            bus.$emit('deleteMediaRequested', media);
        },
        dragStart: function (media, e) {
            bus.$emit('mediaDragStartRequested', media, e);
        },
        getfontAwesomeClassNameForFileName:function getfontAwesomeClassNameForFilename(filename, thumbsize){
            return getClassNameForFilename(filename) + ' ' + thumbsize;
        }
    }
});


$(document).on('mediaApp:ready', function () {
    var chunkedFileUploadId = randomUUID();

    $('#fileupload')
        .fileupload({
            dropZone: $('#mediaApp'),
            limitConcurrentUploads: 20,
            dataType: 'json',
            url: $('#uploadFiles').val(),
            maxChunkSize: Number($('#maxUploadChunkSize').val() || 0),
            formData: function () {
                var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                return [
                    { name: 'path', value: mediaApp.selectedFolder.path },
                    { name: '__RequestVerificationToken', value: antiForgeryToken },
                    { name: '__chunkedFileUploadId', value: chunkedFileUploadId },
                ]
            },
            done: function (e, data) {
                $.each(data.result.files, function (index, file) {
                    if (!file.error) {
                        mediaApp.mediaItems.push(file)
                    }
                });
            }
        })
        .on('fileuploadchunkbeforesend', (e, options) => {
            let file = options.files[0];
            // Here we replace the blob with a File object to ensure the file name and others are preserved for the backend.
            options.blob = new File(
                [options.blob],
                file.name,
                {
                    type: file.type,
                    lastModified: file.lastModified,
                });
        });
});


$(document).bind('dragover', function (e) {
    var dt = e.originalEvent.dataTransfer;
    if (dt.types && (dt.types.indexOf ? dt.types.indexOf('Files') != -1 : dt.types.contains('Files'))) {
        var dropZone = $('#customdropzone'),
            timeout = window.dropZoneTimeout;
        if (timeout) {
            clearTimeout(timeout);
        } else {
            dropZone.addClass('in');
        }
        var hoveredDropZone = $(e.target).closest(dropZone);
        window.dropZoneTimeout = setTimeout(function () {
            window.dropZoneTimeout = null;
            dropZone.removeClass('in');
        }, 100);
    }
});


var initialized;
var mediaApp;

var bus = new Vue();

function initializeMediaApplication(displayMediaApplication, mediaApplicationUrl, pathBase) {

    if (initialized) {
        return;
    }

    initialized = true;

    if (!mediaApplicationUrl) {
        console.error('mediaApplicationUrl variable is not defined');
    }

    $.ajax({
        url: mediaApplicationUrl,
        method: 'GET',
        success: function (content) {
            $('.ta-content').append(content);

            $(document).trigger('mediaapplication:ready');

            var root = {
                name: $('#t-mediaLibrary').text(),
                path: '',
                folder: '',
                isDirectory: true,
                canCreateFolder: $('#allowNewRootFolders').val() === 'true'
            };

            mediaApp = new Vue({
                el: '#mediaApp',
                data: {
                    selectedFolder: {},
                    mediaItems: [],
                    selectedMedias: [],
                    errors: [],
                    dragDropThumbnail: new Image(),
                    smallThumbs: false,
                    gridView: false,
                    mediaFilter: '',
                    sortBy: '',
                    sortAsc: true,
                    itemsInPage: []
                },
                created: function () {
                    var self = this;

                    self.dragDropThumbnail.src = (pathBase || '') + '/OrchardCore.Media/Images/drag-thumbnail.png';

                    bus.$on('folderSelected', function (folder) {
                        self.selectedFolder = folder;
                    });

                    bus.$on('folderDeleted', function () {
                        self.selectRoot();
                    });

                    bus.$on('folderAdded', function (folder) {
                        self.selectedFolder = folder;
                        folder.selected = true;
                    });

                    bus.$on('mediaListMoved', function (errorInfo) {
                        self.loadFolder(self.selectedFolder);
                        if (errorInfo) {
                            self.errors.push(errorInfo);
                        }
                    });

                    bus.$on('mediaRenamed', function (newName, newPath, oldPath, newUrl) {
                        var media = self.mediaItems.filter(function (item) {
                            return item.mediaPath === oldPath;
                        })[0];

                        media.mediaPath = newPath;
                        media.name = newName;
                        media.url = newUrl;
                    });

                    bus.$on('createFolderRequested', function (media) {
                        self.createFolder();
                    });

                    bus.$on('deleteFolderRequested', function (media) {
                        self.deleteFolder();
                    });

                    // common handlers for actions in both grid and table view.
                    bus.$on('sortChangeRequested', function (newSort) {
                        self.changeSort(newSort);
                    });

                    bus.$on('mediaToggleRequested', function (media) {
                        self.toggleSelectionOfMedia(media);
                    });

                    bus.$on('renameMediaRequested', function (media) {
                        self.renameMedia(media);
                    });

                    bus.$on('deleteMediaRequested', function (media) {
                        self.deleteMediaItem(media);
                    });

                    bus.$on('mediaDragStartRequested', function (media, e) {
                        self.handleDragStart(media, e);
                    });


                    // handler for pager events
                    bus.$on('pagerEvent', function (itemsInPage) {
                        self.itemsInPage = itemsInPage;
                        self.selectedMedias = [];
                    });

                    if (!localStorage.getItem('mediaApplicationPrefs')) {
                        self.selectedFolder = root;
                        return;
                    }

                    self.currentPrefs = JSON.parse(localStorage.getItem('mediaApplicationPrefs'));
                },
                computed: {
                    isHome: function () {
                        return this.selectedFolder == root;
                    },
                    parents: function () {
                        var p = [];
                        parentFolder = this.selectedFolder;
                        while (parentFolder && parentFolder.path != '') {
                            p.unshift(parentFolder);
                            parentFolder = parentFolder.parent;
                        }
                        return p;
                    },
                    root: function () {
                        return root;
                    },
                    filteredMediaItems: function () {
                        var self = this;

                        self.selectedMedias = [];

                        var filtered = self.mediaItems.filter(function (item) {
                            return item.name.toLowerCase().indexOf(self.mediaFilter.toLowerCase()) > - 1;
                        });

                        switch (self.sortBy) {
                            case 'size':
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.size - b.size : b.size - a.size;
                                });
                                break;
                            case 'mime':
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.mime.toLowerCase().localeCompare(b.mime.toLowerCase()) : b.mime.toLowerCase().localeCompare(a.mime.toLowerCase());
                                });
                                break;
                            case 'lastModify':
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.lastModify - b.lastModify : b.lastModify - a.lastModify;
                                });
                                break;
                            default:
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.name.toLowerCase().localeCompare(b.name.toLowerCase()) : b.name.toLowerCase().localeCompare(a.name.toLowerCase());
                                });
                        }

                        return filtered;
                    },
                    hiddenCount: function () {
                        var result = 0;
                        result = this.mediaItems.length - this.filteredMediaItems.length;
                        return result;
                    },
                    thumbSize: function () {
                        return this.smallThumbs ? 100 : 240;
                    },
                    currentPrefs: {
                        get: function () {
                            return {
                                smallThumbs: this.smallThumbs,
                                selectedFolder: this.selectedFolder,
                                gridView: this.gridView
                            };
                        },
                        set: function (newPrefs) {
                            if (!newPrefs) {
                                return;
                            }

                            this.smallThumbs = newPrefs.smallThumbs;
                            this.selectedFolder = newPrefs.selectedFolder;
                            this.gridView = newPrefs.gridView;
                        }
                    }
                },
                watch: {
                    currentPrefs: function (newPrefs) {
                        localStorage.setItem('mediaApplicationPrefs', JSON.stringify(newPrefs));
                    },
                    selectedFolder: function (newFolder) {
                        this.mediaFilter = '';
                        this.selectedFolder = newFolder;
                        this.loadFolder(newFolder);
                    }

                },
                mounted: function () {
                    this.$refs.rootFolder.toggle();
                },
                methods: {
                    uploadUrl: function () {

                        if (!this.selectedFolder) {
                            return null;
                        }

                        var urlValue = $('#uploadFiles').val();
                        var allowedExtensions = $('#allowedExtensions').val();
                        if (allowedExtensions && allowedExtensions !== "") {
                            urlValue = urlValue + (urlValue.indexOf('?') == -1 ? '?' : '&') + "extensions=" + encodeURIComponent(allowedExtensions);
                        }

                        return urlValue + (urlValue.indexOf('?') == -1 ? '?' : '&') + "path=" + encodeURIComponent(this.selectedFolder.path);
                    },
                    selectRoot: function () {
                        this.selectedFolder = this.root;
                    },
                    loadFolder: function (folder) {
                        this.errors = [];
                        this.selectedMedias = [];
                        var self = this;
                        var mediaUrl = $('#getMediaItemsUrl').val();
                        var allowedExtensions = $('#allowedExtensions').val();
                        if (allowedExtensions && allowedExtensions !== "") {
                            mediaUrl = mediaUrl + (mediaUrl.indexOf('?') == -1 ? '?' : '&') + "extensions=" + encodeURIComponent(allowedExtensions);
                        }
                        console.log(folder.path);
                        $.ajax({
                            url: mediaUrl + (mediaUrl.indexOf('?') == -1 ? '?' : '&') + "path=" + encodeURIComponent(folder.path),
                            method: 'GET',
                            success: function (data) {
                                data.forEach(function (item) {
                                    item.open = false;
                                });
                                self.mediaItems = data;
                                self.selectedMedias = [];
                                self.sortBy = '';
                                self.sortAsc = true;
                            },
                            error: function (error) {
                                console.log('error loading folder:' + folder.path);
                                self.selectRoot();
                            }
                        });
                    },
                    refresh: function () {
                        var self = this;
                        if (self.selectedFolder) {
                            self.loadFolder(self.selectedFolder);
                        }
                    },
                    selectAll: function () {
                        this.selectedMedias = [];
                        for (var i = 0; i < this.filteredMediaItems.length; i++) {
                            this.selectedMedias.push(this.filteredMediaItems[i]);
                        }
                    },
                    unSelectAll: function () {
                        this.selectedMedias = [];
                    },
                    invertSelection: function () {
                        var temp = [];
                        for (var i = 0; i < this.filteredMediaItems.length; i++) {
                            if (this.isMediaSelected(this.filteredMediaItems[i]) == false) {
                                temp.push(this.filteredMediaItems[i]);
                            }
                        }
                        this.selectedMedias = temp;
                    },
                    toggleSelectionOfMedia: function (media) {
                        if (this.isMediaSelected(media) == true) {
                            this.selectedMedias.splice(this.selectedMedias.indexOf(media), 1);
                        } else {
                            this.selectedMedias.push(media);
                        }
                    },
                    isMediaSelected: function (media) {
                        var result = this.selectedMedias.some(function (element, index, array) {
                            return element.url.toLowerCase() === media.url.toLowerCase();
                        });
                        return result;
                    },
                    deleteFolder: function () {
                        var folder = this.selectedFolder;
                        var self = this;
                        // The root folder can't be deleted
                        if (folder == this.root.model) {
                            return;
                        }

                        confirmDialog({
                            ...$("#deleteFolder").data(), callback: function (resp) {
                                if (resp) {
                                    $.ajax({
                                        url: $('#deleteFolderUrl').val() + "?path=" + encodeURIComponent(folder.path),
                                        method: 'POST',
                                        data: {
                                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                                        },
                                        success: function (data) {
                                            bus.$emit('deleteFolder', folder);
                                        },
                                        error: function (error) {
                                            console.error(error.responseText);
                                        }
                                    });
                                }
                            }
                        });
                    },
                    createFolder: function () {
                        $('#createFolderModal-errors').empty();
                        var modal = bootstrap.Modal.getOrCreateInstance($('#createFolderModal'));
                        modal.show();
                        $('#createFolderModal .modal-body input').val('').focus();
                    },
                    renameMedia: function (media) {
                        $('#renameMediaModal-errors').empty();
                        var modal = bootstrap.Modal.getOrCreateInstance($('#renameMediaModal'));
                        modal.show();
                        $('#old-item-name').val(media.name);
                        $('#renameMediaModal .modal-body input').val(media.name).focus();
                    },
                    selectAndDeleteMedia: function (media) {
                        this.deleteMedia();
                    },
                    deleteMediaList: function () {
                        var mediaList = this.selectedMedias;
                        var self = this;

                        if (mediaList.length < 1) {
                            return;
                        }

                        confirmDialog({
                            ...$("#deleteMedia").data(), callback: function (resp) {
                                if (resp) {
                                    var paths = [];
                                    for (var i = 0; i < mediaList.length; i++) {
                                        paths.push(mediaList[i].mediaPath);
                                    }

                                    $.ajax({
                                        url: $('#deleteMediaListUrl').val(),
                                        method: 'POST',
                                        data: {
                                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
                                            paths: paths
                                        },
                                        success: function (data) {
                                            for (var i = 0; i < self.selectedMedias.length; i++) {
                                                var index = self.mediaItems && self.mediaItems.indexOf(self.selectedMedias[i]);
                                                if (index > -1) {
                                                    self.mediaItems.splice(index, 1);
                                                    bus.$emit('mediaDeleted', self.selectedMedias[i]);
                                                }
                                            }
                                            self.selectedMedias = [];
                                        },
                                        error: function (error) {
                                            console.error(error.responseText);
                                        }
                                    });
                                }
                            }
                        });
                    },
                    deleteMediaItem: function (media) {
                        var self = this;
                        if (!media) {
                            return;
                        }

                        confirmDialog({
                            ...$("#deleteMedia").data(), callback: function (resp) {
                                if (resp) {
                                    $.ajax({
                                        url: $('#deleteMediaUrl').val() + "?path=" + encodeURIComponent(media.mediaPath),
                                        method: 'POST',
                                        data: {
                                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                                        },
                                        success: function (data) {
                                            var index = self.mediaItems && self.mediaItems.indexOf(media)
                                            if (index > -1) {
                                                self.mediaItems.splice(index, 1);
                                                bus.$emit('mediaDeleted', media);
                                            }
                                            //self.selectedMedia = null;
                                        },
                                        error: function (error) {
                                            console.error(error.responseText);
                                        }
                                    });
                                }
                            }
                        });
                    },
                    handleDragStart: function (media, e) {
                        // first part of move media to folder:
                        // prepare the data that will be handled by the folder component on drop event
                        var mediaNames = [];
                        this.selectedMedias.forEach(function (item) {
                            mediaNames.push(item.name);
                        });

                        // in case the user drags an unselected item, we select it first
                        if (this.isMediaSelected(media) == false) {
                            mediaNames.push(media.name);
                            this.selectedMedias.push(media);
                        }

                        e.dataTransfer.setData('mediaNames', JSON.stringify(mediaNames));
                        e.dataTransfer.setData('sourceFolder', this.selectedFolder.path);
                        e.dataTransfer.setDragImage(this.dragDropThumbnail, 10, 10);
                        e.dataTransfer.effectAllowed = 'move';
                    },
                    handleScrollWhileDrag: function (e) {
                        if (e.clientY < 150) {
                            window.scrollBy(0, -10);
                        }

                        if (e.clientY > window.innerHeight - 100) {
                            window.scrollBy(0, 10);
                        }
                    },
                    changeSort: function (newSort) {
                        if (this.sortBy == newSort) {
                            this.sortAsc = !this.sortAsc;
                        } else {
                            this.sortAsc = true;
                            this.sortBy = newSort;
                        }
                    }
                }
            });

            $('#create-folder-name').keydown(function (e) {
                if (e.key == 'Enter') {
                    $('#modalFooterOk').click();
                    return false;
                }
            });

            $('#modalFooterOk').on('click', function (e) {
                var name = $('#create-folder-name').val();

                if (name === "") {
                    return;
                }

                $.ajax({
                    url: $('#createFolderUrl').val() + "?path=" + encodeURIComponent(mediaApp.selectedFolder.path) + "&name=" + encodeURIComponent(name),
                    method: 'POST',
                    data: {
                        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (data) {
                        bus.$emit('addFolder', mediaApp.selectedFolder, data);
                        var modal = bootstrap.Modal.getOrCreateInstance($('#createFolderModal'));
                        modal.hide();
                    },
                    error: function (error) {
                        $('#createFolderModal-errors').empty();
                        var errorMessage = JSON.parse(error.responseText).value;
                        $('<div class="alert alert-danger" role="alert"></div>').text(errorMessage).appendTo($('#createFolderModal-errors'));
                    }
                });
            });

            $('#renameMediaModalFooterOk').on('click', function (e) {
                var newName = $('#new-item-name').val();
                var oldName = $('#old-item-name').val();

                if (newName === "") {
                    return;
                }

                var currentFolder = mediaApp.selectedFolder.path + "/";
                if (currentFolder === "/") {
                    currentFolder = "";
                }

                var newPath = currentFolder + newName;
                var oldPath = currentFolder + oldName;

                if (newPath.toLowerCase() === oldPath.toLowerCase()) {
                    var modal = bootstrap.Modal.getOrCreateInstance($('#renameMediaModal'));
                    modal.hide();
                    return;
                }

                $.ajax({
                    url: $('#renameMediaUrl').val() + "?oldPath=" + encodeURIComponent(oldPath) + "&newPath=" + encodeURIComponent(newPath),
                    method: 'POST',
                    data: {
                        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (data) {
                        var modal = bootstrap.Modal.getOrCreateInstance($('#renameMediaModal'));
                        modal.hide();
                        bus.$emit('mediaRenamed', newName, newPath, oldPath, data.newUrl);
                    },
                    error: function (error) {
                        $('#renameMediaModal-errors').empty();
                        var errorMessage = JSON.parse(error.responseText).value;
                        $('<div class="alert alert-danger" role="alert"></div>').text(errorMessage).appendTo($('#renameMediaModal-errors'));
                    }
                });
            });

            $(document).trigger('mediaApp:ready');

            if (displayMediaApplication) {
                setTimeout(function () {
                    document.getElementById("mediaApp").classList.remove("d-none");
                }, 100)              
            }
        },
        error: function (error) {
            console.error(error.responseText);
        }
    });
}


// different media field editors will add themselves to this array
var mediaFieldApps = [];


// <media-field-thumbs-container> component
// different media field editors share this component to present the thumbs.
Vue.component('mediaFieldThumbsContainer', {
    template:
    /* html */
    `
    <div :id="idPrefix + '_mediaContainerMain'">
         <div v-if="mediaItems.length < 1" class="card text-center">
            <div class= "card-body">
                <span class="hint">{{T.noImages}}</span>
            </div>
         </div>
         <ol ref="multiDragContainer" class="media-items-grid d-flex flex-row align-items-start flex-wrap" >
            <li v-for="media in mediaItems"
                :key="media.vuekey"
                class="media-thumb-item media-container-main-list-item card overflow"
                :class="{'media-thumb-item-active': !allowMultiple && selectedMedia === media}"
                :style="{width: thumbSize + 2 + 'px'}"
                v-on:click="selectMedia(media)"
                v-if="!media.isRemoved">
                    <div v-if="media.mediaPath!== 'not-found'">
                        <div class="thumb-container" :style="{height: thumbSize + 'px'}" >
                            <img v-if="media.mime.startsWith('image')"
                                :src="buildMediaUrl(media.url, thumbSize)"
                                :data-mime="media.mime"
                                width="100%"
                            />
                            <i v-else :class="getfontAwesomeClassNameForFileName(media.name, 'fa-4x')" :data-mime="media.mime"></i>
                        </div>
                        <div class="media-container-main-item-title card-body">
                            <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button"
                                v-on:click.stop="selectAndDeleteMedia(media)"><i class="fa-solid fa-trash" aria-hidden="true"></i></a>
                            <a :href="media.url" target="_blank" class="btn btn-light btn-sm float-end inline-media-button view-button""><i class="fa-solid fa-download" aria-hidden="true"></i></a>
                            <span class="media-filename card-text small" :title="media.mediaPath">{{ media.isNew ? media.name.substr(36) : media.name }}</span>
                        </div>
                    </div>
                    <div v-else>
                        <div class="thumb-container flex-column" :style="{height: thumbSize + 'px'}">
                            <i class="fa-solid fa-ban text-danger d-block" aria-hidden="true"></i>
                            <span class="text-danger small d-block">{{ T.mediaNotFound }}</span>
                            <span class="text-danger small d-block text-center">{{ T.discardWarning }}</span>
                        </div>
                        <div class="media-container-main-item-title card-body">
                            <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button"
                                v-on:click.stop="selectAndDeleteMedia(media)"><i class="fa-solid fa-trash" aria-hidden="true"></i></a>
                            <span class="media-filename card-text small text-danger" :title="media.name">{{ media.name }}</span>
                        </div>
                  </div>
            </li>
         </ol>
    </div>
    `,
    data: function () {
        return {
            T: {},
            sortableInstance: null,
            multiSelectedItems: [],
        };
    },
    model: {
        prop: 'mediaItems',
        event: 'changed',
    },
    props: {
        mediaItems: Array,
        selectedMedia: Object,
        thumbSize: Number,
        idPrefix: String,
        allowMultiple: Boolean,
    },
    created: function () {
        var self = this;

        // retrieving localized strings from view
        self.T.mediaNotFound = $('#t-media-not-found').val();
        self.T.discardWarning = $('#t-discard-warning').val();
        self.T.noImages = $('#t-no-images').val();
    },
    mounted: function () {
        if (this.allowMultiple) this.initSortable();
    },
    methods: {
        initSortable: function () {
            if (this.$refs.multiDragContainer) {
                var self = this;

                this.sortableInstance = Sortable.create(this.$refs.multiDragContainer, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                multiDrag: true,
                selectedClass: 'sortable-selected',
                preventOnFilter: true,
                forceHelperSize: true,
                onUpdate: function (evt) {
                    let newOrder = [...self.mediaItems];

                    if (evt.oldIndicies && evt.oldIndicies.length > 0) {
                        let oldIndices = evt.oldIndicies.sort((a, b) => a.index - b.index);
                        let newIndices = evt.newIndicies.sort((a, b) => a.index - b.index);

                        const itemsToMove = oldIndices.map(oldIdx => self.mediaItems[oldIdx.index]);

                        // old out
                        for (let i = oldIndices.length - 1; i >= 0; i--) {
                            newOrder.splice(oldIndices[i].index, 1);
                        }

                        // new in
                        for (let i = 0; i < newIndices.length; i++) {
                            newOrder.splice(newIndices[i].index, 0, itemsToMove[i]);
                        }

                    } else {
                        // Single item move
                        const [movedItem] = newOrder.splice(evt.oldIndex, 1);
                        newOrder.splice(evt.newIndex, 0, movedItem);
                    }

                    self.$emit('changed', newOrder);

                    const selectedElements = self.$refs.multiDragContainer.querySelectorAll('.sortable-selected');
                    selectedElements.forEach(el => {
                        Sortable.utils.deselect(el);
                    });
                    self.multiSelectedItems = [];
                }
                });
            }
        },
        selectAndDeleteMedia: function (media) {
            this.$parent.$emit('selectAndDeleteMediaRequested', media);
        },
        selectMedia: function (media) {
            this.$parent.$emit('selectMediaRequested', media);
        },
        buildMediaUrl: function (url, thumbSize) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        getfontAwesomeClassNameForFileName:function getfontAwesomeClassNameForFilename(filename, thumbsize){
            return getClassNameForFilename(filename) + ' ' + thumbsize;
        }
    }
});


function initializeAttachedMediaField(el, idOfUploadButton, uploadAction, mediaItemUrl, allowMultiple, allowMediaText, allowAnchors, tempUploadFolder, maxUploadChunkSize) {

    var target = $(document.getElementById($(el).data('for')));
    var initialPaths = target.data("init");

    var mediaFieldEditor = $(el);
    var idprefix = mediaFieldEditor.attr("id");
    var mediaFieldApp;

    mediaFieldApps.push(mediaFieldApp = new Vue({
        el: mediaFieldEditor.get(0),
        data: {
            mediaItems: [],
            selectedMedia: null,
            smallThumbs: false,
            idPrefix: idprefix,
            initialized: false,
            allowMediaText: allowMediaText,
            backupMediaText: '',
            allowAnchors: allowAnchors,
            backupAnchor: null,
            mediaTextmodal: null,
            anchoringModal: null
        },
        created: function () {
            var self = this;

            self.currentPrefs = JSON.parse(localStorage.getItem('mediaFieldPrefs'));
        },
        computed: {
            paths: {
                get: function () {
                    var mediaPaths = [];
                    if (!this.initialized) {
                        return JSON.stringify(initialPaths);
                    }
                    this.mediaItems.forEach(function (x) {
                        if (x.mediaPath === 'not-found') {
                            return;
                        }
                        mediaPaths.push({ path: x.mediaPath, isRemoved: x.isRemoved, isNew: x.isNew, mediaText: x.mediaText, anchor: x.anchor, attachedFileName: x.attachedFileName });
                    });
                    return JSON.stringify(mediaPaths);
                },
                set: function (values) {
                    var self = this;
                    var mediaPaths = values || [];
                    var signal = $.Deferred();
                    var items = [];
                    var length = 0;
                    mediaPaths.forEach(function (x, i) {
                        items.push({ name: ' ' + x.path, mime: '', mediaPath: '', anchor: x.anchor, attachedFileName: x.attachedFileName }); // don't remove the space. Something different is needed or it wont react when the real name arrives.
                        promise = $.when(signal).done(function () {
                            $.ajax({
                                url: mediaItemUrl + "?path=" + encodeURIComponent(x.path),
                                method: 'GET',
                                success: function (data) {
                                    data.vuekey = data.name + i.toString(); // Because a unique key is required by Vue on v-for 
                                    data.mediaText = x.mediaText; // This value is not returned from the ajax call.
                                    data.anchor = x.anchor; // This value is not returned from the ajax call.
                                    data.attachedFileName = x.attachedFileName;// This value is not returned from the ajax call.
                                    items.splice(i, 1, data);
                                    if (items.length === ++length) {
                                        items.forEach(function (x) {
                                            self.mediaItems.push(x);
                                        });
                                        self.initialized = true;
                                    }
                                },
                                error: function (error) {
                                    console.log(JSON.stringify(error));
                                    items.splice(i, 1, { name: x.path, mime: '', mediaPath: 'not-found', mediaText: '', anchor: { x: 0.5, y: 0.5 }, attachedFileName: x.attachedFileName });
                                    if (items.length === ++length) {
                                        items.forEach(function (x) {
                                            self.mediaItems.push(x);
                                        });
                                        self.initialized = true;
                                    }
                                }
                            });
                        });
                    });

                    signal.resolve();
                }
            },
            fileSize: function () {
                return Math.round(this.selectedMedia.size / 1024);
            },
            canAddMedia: function () {
                var nonRemovedMediaItems = [];
                for (var i = 0; i < this.mediaItems.length; i++) {
                    if (!this.mediaItems[i].isRemoved) {
                        nonRemovedMediaItems.push(this.mediaItems[i]);
                    }
                }

                return nonRemovedMediaItems.length === 0 || nonRemovedMediaItems.length > 0 && allowMultiple;
            },
            thumbSize: function () {
                return this.smallThumbs ? 120 : 240;
            },
            currentPrefs: {
                get: function () {
                    return {
                        smallThumbs: this.smallThumbs
                    };
                },
                set: function (newPrefs) {
                    if (!newPrefs) {
                        return;
                    }
                    this.smallThumbs = newPrefs.smallThumbs;
                }
            }
        },
        mounted: function () {
            var self = this;

            self.paths = initialPaths;

            self.$on('selectAndDeleteMediaRequested', function (media) {
                self.selectAndDeleteMedia(media);
            });

            self.$on('selectMediaRequested', function (media) {
                self.selectMedia(media);
            });

            var selector = '#' + idOfUploadButton;
            var editorId = mediaFieldEditor.attr('id');
            var chunkedFileUploadId = randomUUID();

            $(selector)
                .fileupload({
                    limitConcurrentUploads: 20,
                    dropZone: $('#' + editorId),
                    dataType: 'json',
                    url: uploadAction,
                    maxChunkSize: maxUploadChunkSize,
                    add: function (e, data) {
                        var count = data.files.length;
                        var i;
                        for (i = 0; i < count; i++) {
                            data.files[i].uploadName =
                                self.getUniqueId() + data.files[i].name;
                            data.files[i].attachedFileName = data.files[i].name;
                        }
                        data.submit();
                    },
                    formData: function () {
                        var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                        return [
                            { name: 'path', value: tempUploadFolder },
                            { name: '__RequestVerificationToken', value: antiForgeryToken },
                            { name: '__chunkedFileUploadId', value: chunkedFileUploadId },
                        ];
                    },
                    done: function (e, data) {
                        var newMediaItems = [];
                        var errormsg = "";

                        if (data.result.files.length > 0) {
                            for (var i = 0; i < data.result.files.length; i++) {
                                data.result.files[i].isNew = true;
                                //if error is defined probably the file type is not allowed
                                if (data.result.files[i].error === undefined || data.result.files[i].error === null) {
                                    data.result.files[i].attachedFileName = data.files[i].attachedFileName;
                                    newMediaItems.push(data.result.files[i]);
                                }
                                else
                                    errormsg += data.result.files[i].error + "\n";
                            }
                        }

                        if (errormsg !== "") {
                            alert(errormsg);
                            return;
                        }
                        console.log(newMediaItems);
                        if (newMediaItems.length > 1 && allowMultiple === false) {
                            alert($('#onlyOneItemMessage').val());
                            mediaFieldApp.mediaItems.push(newMediaItems[0]);
                            mediaFieldApp.initialized = true;
                        } else {
                            mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(newMediaItems);
                            mediaFieldApp.initialized = true;
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log('Error on upload.');
                        console.log(jqXHR);
                        console.log(textStatus);
                        console.log(errorThrown);
                    }
                })
                .on('fileuploadchunkbeforesend', (e, options) => {
                    let file = options.files[0];
                    // Here we replace the blob with a File object to ensure the file name and others are preserved for the backend.
                    options.blob = new File(
                        [options.blob],
                        file.name,
                        {
                            type: file.type,
                            lastModified: file.lastModified,
                        });
                });
        },
        methods: {
            selectMedia: function (media) {
                this.selectedMedia = media;
            },
            getUniqueId: function () {
                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
            },
            removeSelected: function (event) {
                var removed = {};
                if (this.selectedMedia) {
                    var index = this.mediaItems && this.mediaItems.indexOf(this.selectedMedia);
                    if (index > -1) {
                        removed = this.mediaItems[index];
                        removed.isRemoved = true;
                        //this.mediaItems.splice([index], 1, removed);
                        this.mediaItems.splice(index, 1);
                    }
                }
                else {
                    // The remove button can also remove a unique media item
                    if (this.mediaItems.length === 1) {
                        removed = this.mediaItems[index];
                        removed.isRemoved = true;
                        //this.mediaItems.splice(0, 1, removed);                        
                        this.mediaItems.splice(0, 1);
                    }
                }
                this.selectedMedia = null;
            },
            showMediaTextModal: function (event) {
                this.mediaTextModal = new bootstrap.Modal(this.$refs.mediaTextModal);
                this.mediaTextModal.show();
                this.backupMediaText = this.selectedMedia.mediaText;
            },
            cancelMediaTextModal: function (event) {
                this.mediaTextModal.hide();
                this.selectedMedia.mediaText = this.backupMediaText;
            },
            showAnchorModal: function (event) {
                this.anchoringModal = new bootstrap.Modal(this.$refs.anchoringModal);
                this.anchoringModal.show();
                // Cause a refresh to recalc heights.
                this.selectedMedia.anchor = {
                    x: this.selectedMedia.anchor.x,
                    y: this.selectedMedia.anchor.y
                }
                this.backupAnchor = this.selectedMedia.anchor;
            },
            cancelAnchoringModal: function (event) {
                this.anchoringModal.hide();
                this.selectedMedia.anchor = this.backupAnchor;
            },
            resetAnchor: function (event) {
                this.selectedMedia.anchor = { x: 0.5, y: 0.5 };
            },
            onAnchorDrop: function (event) {
                var image = this.$refs.anchorImage;
                this.selectedMedia.anchor = {
                    x: event.offsetX / image.clientWidth,
                    y: event.offsetY / image.clientHeight
                }
            },
            anchorLeft: function () {
                if (this.$refs.anchorImage && this.$refs.modalBody && this.selectedMedia) {
                    // When image is shrunk compare against the modal body.
                    var offset = (this.$refs.modalBody.clientWidth - this.$refs.anchorImage.clientWidth) / 2;
                    var position = (this.selectedMedia.anchor.x * this.$refs.anchorImage.clientWidth) + offset;
                    if (position < 17) { // Adjust so the target doesn't show outside image.
                        position = 17;
                    } else {
                        position = position - 8; // Adjust to hit the mouse pointer.
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },
            anchorTop: function () {
                if (this.$refs.anchorImage && this.selectedMedia) {
                    var position = this.selectedMedia.anchor.y * this.$refs.anchorImage.clientHeight;
                    if (position < 15) { // Adjustment so the target doesn't show outside image.
                        position = 15;
                    } else {
                        position = position + 5; // Adjust to hit the mouse pointer.
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },
            setAnchor: function (event) {
                var image = this.$refs.anchorImage;
                this.selectedMedia.anchor = {
                    x: event.offsetX / image.clientWidth,
                    y: event.offsetY / image.clientHeight
                }
            },
            addMediaFiles: function (files) {
                if ((files.length > 1) && (allowMultiple === false)) {
                    alert($('#onlyOneItemMessage').val());
                    mediaFieldApp.mediaItems.push(files[0]);
                    mediaFieldApp.initialized = true;
                } else {
                    mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(files);
                    mediaFieldApp.initialized = true;
                }
            },
            selectAndDeleteMedia: function (media) {
                var self = this;
                self.selectedMedia = media;
                // setTimeout because sometimes 
                // removeSelected was called even before the media was set.
                setTimeout(function () {
                    self.removeSelected();
                }, 100);
            }
        },
        watch: {
            mediaItems: {
                deep: true,
                handler() {
                    // Trigger preview rendering
                    setTimeout(function () { $(document).trigger('contentpreview:render'); }, 100);
                }
            },
            currentPrefs: function (newPrefs) {
                localStorage.setItem('mediaFieldPrefs', JSON.stringify(newPrefs));
            }
        }
    }));
}


const MEDIA_FIELD_GALLERY = "mediaFieldGallery_";

Vue.component("mediaFieldGalleryListItem", {
    template:
    /*html*/
    `
        <li class="list-group-item d-flex p-0 overflow-hidden align-items-center" v-if="!media.isRemoved" :class="media.mediaPath=='not-found' ? 'text-danger' : ''">
            <div class="media-preview flex-shrink-0">
                <img
                    v-if="media.mime.startsWith('image')"
                    :src="buildMediaUrl(media.url, media.anchor)"
                    :data-mime="media.mime"
                    class="w-100 object-fit-scale"
                />
                <i v-else-if="media.mediaPath=='not-found'" :title="media.name" class="fa-solid fa-triangle-exclamation"></i>
                <i v-else :class="$parent.getfontAwesomeClassNameForFileName(media.name, 'fa-4x card-text')" :data-mime="media.mime"></i>
            </div>
            <div class="me-auto flex-shrink-1">
                <span v-if="media.mediaPath=='not-found'" class="media-filename card-text small">{{ $parent.T.mediaNotFound }} - {{ $parent.T.discardWarning }}</span>
                <span v-else class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
            </div>
            <div class="media-field-gallery-list-actions flex-shrink-0">
                <a
                    v-show="allowMediaText && media.mediaPath!=='not-found'"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    v-on:click.prevent.stop="$parent.showMediaTextModal(media)"
                    href="javascript:;"
                    title="Edit media text"
                >
                    <span v-show="!media.mediaText">
                        <i class="far fa-comment"></i>
                    </span>
                    <span v-show="media.mediaText">
                        <i class="fa-solid fa-comment"></i>
                    </span>
                </a>
                <a
                    href="javascript:;"
                    v-show="allowAnchors && media.mime.startsWith('image') && media.mediaPath!=='not-found'"
                    v-on:click="$parent.showAnchorModal(media)"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    title="Set anchor"
                >
                    <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
                </a>
                <a
                    :href="media.url"
                    target="_blank"
                    v-show="media.mediaPath!=='not-found'"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    title="View media"
                >
                    <i class="fa-solid fa-download" aria-hidden="true"></i>
                </a>
                <a
                    href="javascript:;"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    v-on:click.stop="$parent.selectAndDeleteMedia(media)"
                    title="Remove media"
                >
                    <i class="fa-solid fa-trash" aria-hidden="true"></i>
                </a>
            </div>
        </li>
    `,
    props: {
        media: Object,
        canAddMedia: Boolean,
        allowMediaText: Boolean,
        allowAnchors: Boolean,
        cols: Number,
    },
    methods: {
        fileSize: function fileSize(rawSize) {
            return Math.round(rawSize / 1024);
        },
        buildMediaUrl: function buildMediaUrl(url) {
            return url + (url.indexOf("?") == -1 ? "?" : "&") + "width=32&height=32";
        },
    },
});

Vue.component("mediaFieldGalleryCardItem", {
    template:
    /*html*/
    `
        <li class="media-field-gallery-item" v-if="!media.isRemoved">
            <div class="card ratio ratio-1x1 overflow-hidden" :class="media.mediaPath=='not-found' ? 'text-danger border-danger' : ''">
                <div class="d-flex flex-column h-100">
                    <div class="flex-grow-1 media-preview d-flex justify-content-center align-items-center">
                        <div class="update-media" v-if="!$parent.allowMultiple" v-on:click="$parent.showMediaModal">
                            + Media Library
                        </div>
                        <div class="image-wrapper" v-if="media.mime.startsWith('image')">
                            <img
                                :src="buildMediaUrl(media.url)"
                                :data-mime="media.mime"
                                class="w-100 h-100 object-fit-scale"
                            />
                        </div>
                        <div v-else-if="media.mediaPath=='not-found'" class="d-flex flex-column justify-content-center align-items-center h-100 bg-body file-icon not-found" :title="media.name">
                            <i class="fa-solid fa-triangle-exclamation fa-2x card-text'"></i>
                            <span class="card-text small pt-2" :title="media.name">{{ $parent.T.mediaNotFound }}</span>
                            <span class="card-text small pt-2 px-2 text-center" :title="media.name">{{ $parent.T.discardWarning }}</span>
                        </div>
                        <div v-else class="d-flex flex-column justify-content-center align-items-center h-100 bg-body file-icon">
                            <i :class="$parent.getfontAwesomeClassNameForFileName(media.name, 'fa-4x card-text')" :data-mime="media.mime"></i>
                            <span class="media-filename card-text small pt-2" :title="media.name">{{ media.name }}</span>
                        </div>
                    </div>

                    <div class="media-field-gallery-card-actions flex-shrink-0">
                        <a
                            v-show="allowMediaText && media.mediaPath!=='not-found'"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            v-on:click.prevent.stop="$parent.showMediaTextModal(media)"
                            href="javascript:;"
                            title="Edit media text"
                        >
                            <span v-show="!media.mediaText">
                                <i class="far fa-comment"></i>
                            </span>
                            <span v-show="media.mediaText">
                                <i class="fa-solid fa-comment"></i>
                            </span>
                        </a>
                        <a
                            href="javascript:;"
                            v-show="allowAnchors && media.mime.startsWith('image') && media.mediaPath!=='not-found'"
                            v-on:click="$parent.showAnchorModal(media)"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            title="Set anchor"
                        >
                            <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
                        </a>
                        <a
                            :href="media.url"
                            target="_blank"
                            v-show="media.mediaPath!=='not-found'"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            title="View media"
                        >
                            <i class="fa-solid fa-download" aria-hidden="true"></i>
                        </a>
                        <a
                            href="javascript:;"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            v-on:click.stop="$parent.selectAndDeleteMedia(media)"
                            title="Remove media"
                        >
                            <i class="fa-solid fa-trash" aria-hidden="true"></i>
                        </a>
                    </div>
                </div>
            </div>
        </li>
    `,
    props: {
        media: Object,
        canAddMedia: Boolean,
        allowAnchors: Boolean,
        allowMediaText: Boolean,
    },
    methods: {
        fileSize: function fileSize(rawSize) {
            return Math.round(rawSize / 1024);
        },
        buildMediaUrl: function buildMediaUrl(url) {
            return (url + (url.indexOf("?") == -1 ? "?" : "&") + "width=240&height=240");
        },
    },
});

Vue.component("mediaFieldGalleryContainer", {
    template:
    /*html*/
    `
        <div class="media-field-gallery" :id="idPrefix + '_mediaContainerMain'" v-cloak>
            <div v-if="allowMultiple" class="pb-2" v-cloak>
                <div class="me-auto">
                    <a
                        type="button"
                        class="btn btn-sm btn-primary"
                        href="javascript:;"
                        v-show="$parent.canAddMedia"
                        v-on:click="$parent.showModal"
                    >
                        <i class="fa-solid fa-plus" aria-hidden="true"></i>
                        Media Library
                    </a>
                    <a
                        type="button"
                        class="btn btn-sm btn-secondary draft me-2"
                        :class="selectedItemCount == 0 ? 'disabled' : ''"
                        :disabled="selectedItemCount == 0"
                        href="javascript:;"
                        v-on:click="deselectAll"
                    >
                        Deselect All
                    </a>

                    <div class="btn-group">
                        <button type="button" class="btn btn-sm" :class="!gridView ? 'text-primary': ''" v-on:click="gridView = false;">
                            <span title="Gridview"></span>
                            <i class="fa-solid fa-th-list"></i>
                        </button>
                        <button type="button" class="btn btn-sm" :class="gridView ? 'text-primary': ''" v-on:click="gridView = true;">
                            <span title="List"></span>
                            <i class="fa-solid fa-th-large"></i>
                        </button>
                        <button type="button" class="btn btn-sm" :class="size == 'sm' ? 'text-primary': ''" v-on:click="size = 'sm';" v-show="gridView">
                            <span title="Small Thumbs"></span>
                            <i class="fa-solid fa-compress"></i>
                        </button>
                        <button type="button" class="btn btn-sm" :class="size == 'lg' ? 'text-primary': ''" v-on:click="size = 'lg';" v-show="gridView">
                            <span title="Large Thumbs"></span>
                            <i class="fa-solid fa-expand"></i>
                        </button>
                    </div>
                </div>
            </div>

            <ol ref="mediaContainer" v-if="!gridView" :class="'media-field-gallery-list list-group list-unstyled size-' + size">
                <media-field-gallery-list-item
                    v-for= "media in mediaItemsNoDuplicates"
                    :key="media.vuekey ?? media.name"
                    :media="media"
                    :canAddMedia="$parent.canAddMedia"
                    :allowMediaText="$parent.allowMediaText"
                    :allowAnchors="$parent.allowAnchors"
                    :size="size"
                />

                <li v-if="!mediaItems || mediaItems.length < 1" class="list-group-item text-center hint media-field-gallery-choose-btn" v-on:click="showMediaModal">
                    + Media Library
                </li>
            </ol>

            <ol ref="mediaContainer" v-if="gridView" :class="'media-field-gallery-cards list-unstyled size-' + size">
                <media-field-gallery-card-item
                    v-for= "media in mediaItemsNoDuplicates"
                    :key="media.vuekey ?? media.name"
                    :media="media"
                    :canAddMedia="$parent.canAddMedia"
                    :allowMediaText="$parent.allowMediaText"
                    :allowAnchors="$parent.allowAnchors"
                    :size="size"
                />

                <li class="add-media-card" v-if="allowMultiple || mediaItems.length < 1" v-on:click="showMediaModal">
                    <div class="card media-field-gallery-choose-btn text-center overflow-hidden">
                        <div class="ratio ratio-1x1 text-center">
                            <div class="hint d-flex align-items-center justify-content-center w-100">+ Media Library</div>
                        </div>
                    </div>
                </li>
            </ol>
        </div>
    `,
    data: function data() {
        return {
            T: {},
            sortableInstance: null,
            multiSelectedItems: [],
            selectedItemCount: 0,
            gridView: true,
            size: "lg",
        };
    },
    props: {
        mediaItems: Array,
        selectedMedia: Object,
        idPrefix: String,
        modalId: String,
        allowMultiple: Boolean
    },
    created: function created() {
        var self = this;

        // retrieving localized strings from view
        self.T.mediaNotFound = $("#t-media-not-found").val();
        self.T.discardWarning = $("#t-discard-warning").val();
        self.T.noImages = $("#t-no-images").val();
    },
    mounted: function mounted() {
        this.getLocalStorageState();
        this.initSortable();
    },
    beforeDestroy: function beforeDestroy() {
        if (this.sortableInstance) {
            this.sortableInstance.destroy();
        }
    },
    watch: {
        mediaItems: {
            handler() {
                if (this.sortableInstance) {
                    this.sortableInstance.destroy();
                }
                this.$nextTick(() => {
                    this.initSortable();
                    // as this component is now responsible for inforcing one item only when allowMultiple is false, we emit the last item in the list
                    if (!this.allowMultiple && this.mediaItems.length > 1) {
                        this.$emit("updated", [this.mediaItems[this.mediaItems.length - 1]]);
                    }
                });
            },
            deep: true,
        },
        size: {
            handler() {
                this.setLocalStorageState();
            },
        },
        gridView: {
            handler() {
                this.setLocalStorageState();
            },
        },
    },
    computed: {
        mediaItemsNoDuplicates: function mediaItemsNoDuplicates() {
          return this.removeDuplicates(this.mediaItems);
        }
    },
    methods: {
        getLocalStorageState: function getLocalStorageState() {
            if (localStorage.getItem(MEDIA_FIELD_GALLERY + this.idPrefix)) {
                try {
                    const state = JSON.parse(localStorage.getItem(MEDIA_FIELD_GALLERY + this.idPrefix));
                    this.size = state.size || "lg";
                    this.gridView = !this.allowMultiple ? true : state.gridView ?? false;
                } catch (e) {
                    localStorage.removeItem(MEDIA_FIELD_GALLERY + this.idPrefix);
                }
            }
        },
        setLocalStorageState: function setLocalStorageState() {
            const parsed = JSON.stringify({
                size: this.size,
                gridView: this.gridView,
            });
            localStorage.setItem(MEDIA_FIELD_GALLERY + this.idPrefix, parsed);
        },
        initSortable: function initSortable() {
            if (this.allowMultiple && this.$refs.mediaContainer && this.mediaItemsNoDuplicates.length > 0) {
                var self = this;
                this.sortableInstance = Sortable.create(this.$refs.mediaContainer, {
                    animation: 150,
                    ghostClass: "sortable-ghost",
                    multiDrag: true,
                    selectedClass: "sortable-selected",
                    filter: "a, button, .add-media-card",
                    preventOnFilter: false,
                    onSelect: () => this.updateSelectedCount(),
                    onDeselect: () => this.updateSelectedCount(),
                    onEnd: () => this.updateSelectedCount(),
                    onMove: function (evt) {
                        // Prevent moving the .add-media-card or placing anything after it
                        const dragged = evt.dragged;
                        const related = evt.related;

                        // If trying to move the "add" card or drop something after it
                        if (
                            dragged.classList.contains("add-media-card") ||
                            related.classList.contains("add-media-card")
                        ) {
                            return false; // Cancel the move
                        }

                        return true;
                    },
                    onUpdate: function (evt) {
                        let newOrder = [...self.mediaItemsNoDuplicates];

                        if (evt.oldIndicies && evt.oldIndicies.length > 0) {
                        let oldIndices = evt.oldIndicies.sort(
                            (a, b) => a.index - b.index
                        );
                        let newIndices = evt.newIndicies.sort(
                            (a, b) => a.index - b.index
                        );

                        const itemsToMove = oldIndices.map(
                            (oldIdx) => self.mediaItems[oldIdx.index]
                        );

                        // old out
                        for (let i = oldIndices.length - 1; i >= 0; i--) {
                            newOrder.splice(oldIndices[i].index, 1);
                        }

                        // new in
                        for (let i = 0; i < newIndices.length; i++) {
                            newOrder.splice(newIndices[i].index, 0, itemsToMove[i]);
                        }
                        } else {
                            // Single item move
                            const [movedItem] = newOrder.splice(evt.oldIndex, 1);
                            newOrder.splice(evt.newIndex, 0, movedItem);
                        }

                        self.$emit("updated", newOrder);

                        const selectedElements = self.$refs.mediaContainer.querySelectorAll(".sortable-selected");
                        selectedElements.forEach((el) => {
                            Sortable.utils.deselect(el);
                        });
                        self.multiSelectedItems = [];
                    },
                });
            }
        },
        selectAndDeleteMedia: function selectAndDeleteMedia(media) {
            this.$parent.$emit("selectAndDeleteMediaRequested", media);
        },
        showMediaTextModal: function showMediaTextModal(media) {
            this.$parent.$emit("selectMediaRequested", media);
            this.$nextTick(() => this.$emit("updatemediatext"));
        },
        showAnchorModal: function showAnchorModal(media) {
            this.$parent.$emit("selectMediaRequested", media);
            this.$nextTick(() => this.$emit("updateanchor"));
        },
        updateSelectedCount() {
            this.selectedItemCount = this.$refs.mediaContainer.querySelectorAll(".sortable-selected").length;
        },
        getfontAwesomeClassNameForFileName: function getfontAwesomeClassNameForFilename(filename, thumbsize) {
            return getClassNameForFilename(filename) + " " + thumbsize;
        },
        deselectAll: function deselectAll() {
            this.$refs.mediaContainer
                .querySelectorAll(".sortable-selected")
                .forEach((el) => {
                    Sortable.utils.deselect(el);
                });
            this.multiSelectedItems = [];
        },
        showMediaModal: function () {
            this.$parent.showModal();
        },
        removeDuplicates: function (array) {
            return array.filter((obj, index, self) =>
                index === self.findIndex((o) => o.mediaPath === obj.mediaPath)
            );
        },
    }
});


function initializeMediaField(el, modalBodyElement, mediaItemUrl, allowMultiple, allowMediaText, allowAnchors, allowedExtensions) {
    //BagPart create a script section without other DOM elements
    if (el === null) return;

    var target = $(document.getElementById($(el).data('for')));
    var initialPaths = target.data("init");

    var mediaFieldEditor = $(el);
    var idprefix = mediaFieldEditor.attr("id");
    var mediaFieldApp;

    //when hide modal detach media app to avoid issue on BagPart
    modalBodyElement.addEventListener('hidden.bs.modal', function (event) {
        $("#mediaApp").appendTo('body');
        document.getElementById("mediaApp").classList.add("d-none");
    });

    mediaFieldApps.push(mediaFieldApp = new Vue({
        el: mediaFieldEditor.get(0),
        data: {
            mediaItems: [],
            selectedMedia: null,
            smallThumbs: false,
            idPrefix: idprefix,
            initialized: false,
            allowMediaText: allowMediaText,
            backupMediaText: '',
            allowAnchors: allowAnchors,
            allowedExtensions: allowedExtensions,
            backupAnchor: null,
            mediaTextModal: null,
            anchoringModal: null
        },
        created: function () {
            var self = this;

            self.currentPrefs = JSON.parse(localStorage.getItem('mediaFieldPrefs'));
        },
        computed: {
            paths: {
                get: function () {
                    var mediaPaths = [];
                    if (!this.initialized) {
                        return JSON.stringify(initialPaths);
                    }
                    this.mediaItems.forEach(function (x) {
                        if (x.mediaPath === 'not-found') {
                            return;
                        }
                        mediaPaths.push({ path: x.mediaPath, mediaText: x.mediaText, anchor: x.anchor });
                    });
                    return JSON.stringify(mediaPaths);
                },
                set: function (values) {
                    var self = this;
                    var mediaPaths = values || [];
                    var signal = $.Deferred();
                    var items = [];
                    var length = 0;
                    mediaPaths.forEach(function (x, i) {
                        items.push({ name: ' ' + x.path, mime: '', mediaPath: '' }); // don't remove the space. Something different is needed or it wont react when the real name arrives.
                        promise = $.when(signal).done(function () {
                            $.ajax({
                                url: mediaItemUrl + "?path=" + encodeURIComponent(x.path),
                                method: 'GET',
                                success: function (data) {
                                    data.vuekey = data.name + i.toString();
                                    data.mediaText = x.mediaText; // This value is not returned from the ajax call.
                                    data.anchor = x.anchor; // This value is not returned from the ajax call.
                                    items.splice(i, 1, data);
                                    if (items.length === ++length) {
                                        items.forEach(function (y) {
                                            self.mediaItems.push(y);
                                        });
                                        self.initialized = true;

                                        // Preselect first thumbnail if only single image is allowed
                                        if (!allowMultiple && self.mediaItems.length === 1) {
                                            self.$nextTick(function () {
                                                self.selectedMedia = self.mediaItems[0];
                                            });
                                        }
                                    }
                                },
                                error: function (error) {
                                    console.log(error);
                                    items.splice(i, 1, { name: x.path, mime: '', mediaPath: 'not-found', mediaText: '', anchor: { x: 0, y: 0 } });
                                    if (items.length === ++length) {
                                        items.forEach(function (x) {
                                            self.mediaItems.push(x);
                                        });
                                        self.initialized = true;
                                    }
                                }
                            });
                        });
                    });


                    signal.resolve();
                }
            },
            fileSize: function () {
                return Math.round(this.selectedMedia.size / 1024);
            },
            canAddMedia: function () {
                return this.mediaItems.length === 0 || this.mediaItems.length > 0 && allowMultiple;
            },
            thumbSize: function () {
                return this.smallThumbs ? 120 : 240;
            },
            currentPrefs: {
                get: function () {
                    return {
                        smallThumbs: this.smallThumbs
                    };
                },
                set: function (newPrefs) {
                    if (!newPrefs) {
                        return;
                    }
                    this.smallThumbs = newPrefs.smallThumbs;
                }
            }
        },
        mounted: function () {
            var self = this;

            self.paths = initialPaths;

            self.$on('selectAndDeleteMediaRequested', function (media) {
                self.selectAndDeleteMedia(media);
            });

            self.$on('selectMediaRequested', function (media) {
                self.selectMedia(media);
            });

            self.$on('filesUploaded', function (files) {
                self.addMediaFiles(files);
            });
        },
        methods: {
            selectMedia: function (media) {
                this.selectedMedia = media;
            },
            showModal: function (event) {
                var self = this;
                if (self.canAddMedia) {
                    $('#allowedExtensions').val(this.allowedExtensions);
                    $('#fileupload').attr('accept', this.allowedExtensions);
                    $("#mediaApp").appendTo($(modalBodyElement).find('.modal-body'));

                    // Reload current folder in case the allowed extensions have changed.
                    mediaApp.refresh();

                    var modal = new bootstrap.Modal(modalBodyElement);
                    modal.show();

                    setTimeout(function () {
                        document.getElementById("mediaApp").classList.remove("d-none");
                    }, 100)

                    $(modalBodyElement).find('.mediaFieldSelectButton').off('click').on('click', function (v) {
                        self.addMediaFiles(mediaApp.selectedMedias);

                        // we don't want the included medias to be still selected the next time we open the modal.
                        mediaApp.selectedMedias = [];

                        modal.hide();
                        return true;
                    });
                }
            },
            showMediaTextModal: function (event) {
                this.mediaTextModal = new bootstrap.Modal(this.$refs.mediaTextModal);
                this.mediaTextModal.show();
                this.backupMediaText = this.selectedMedia.mediaText;
            },
            cancelMediaTextModal: function (event) {
                this.mediaTextModal.hide();
                this.selectedMedia.mediaText = this.backupMediaText;
            },
            showAnchorModal: function (event) {
                this.anchoringModal = new bootstrap.Modal(this.$refs.anchoringModal);
                this.anchoringModal.show();
                // Cause a refresh to recalc heights.
                this.selectedMedia.anchor = {
                  x: this.selectedMedia.anchor.x,
                  y: this.selectedMedia.anchor.y
                }
                this.backupAnchor = this.selectedMedia.anchor;
            },            
            cancelAnchoringModal: function (event) {
                this.anchoringModal.hide();
                this.selectedMedia.anchor = this.backupAnchor;
            },            
            resetAnchor: function (event) {
                this.selectedMedia.anchor = { x: 0.5, y: 0.5 };
            },  
            onAnchorDrop: function(event) {
                var image = this.$refs.anchorImage;
                this.selectedMedia.anchor = {
                   x: event.offsetX / image.clientWidth,
                   y: event.offsetY / image.clientHeight
                }
            },
            anchorLeft: function () {
                if (this.$refs.anchorImage && this.$refs.modalBody && this.selectedMedia) {
                    // When image is shrunk compare against the modal body.
                    var offset = (this.$refs.modalBody.clientWidth - this.$refs.anchorImage.clientWidth) / 2;
                    var position = (this.selectedMedia.anchor.x * this.$refs.anchorImage.clientWidth) + offset;
                    var anchorIcon = Math.round(this.$refs.modalBody.querySelector('.icon-media-anchor').clientWidth);
                    if(Number.isInteger(anchorIcon))
                    {
                        position = position - anchorIcon/2;
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },            
            anchorTop: function () {
                if (this.$refs.anchorImage && this.selectedMedia) {
                    var position = this.selectedMedia.anchor.y * this.$refs.anchorImage.clientHeight;
                    return position + 'px';
                } else {
                    return '0';
                }
            },
            setAnchor: function (event) {
                var image = this.$refs.anchorImage;
                this.selectedMedia.anchor = {
                    x: event.offsetX / image.clientWidth,
                    y: event.offsetY / image.clientHeight
                }
            },         
            addMediaFiles: function (files) {
                if ((files.length > 1) && (allowMultiple === false)) {
                    alert($('#onlyOneItemMessage').val());
                    mediaFieldApp.mediaItems.push(files[0]);
                    mediaFieldApp.initialized = true;
                    // Preselect the newly added media item
                    mediaFieldApp.$nextTick(function() {
                        mediaFieldApp.selectedMedia = mediaFieldApp.mediaItems[0];
                    });
                } else {
                    mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(files);
                    mediaFieldApp.initialized = true;
                    // Preselect first thumbnail if only single image is allowed
                    if (!allowMultiple && mediaFieldApp.mediaItems.length === 1) {
                        mediaFieldApp.$nextTick(function() {
                            mediaFieldApp.selectedMedia = mediaFieldApp.mediaItems[0];
                        });
                    }
                }
            },
            removeSelected: function (event) {
                if (this.selectedMedia) {
                    var index = this.mediaItems && this.mediaItems.indexOf(this.selectedMedia);
                    if (index > -1) {
                        this.mediaItems.splice(index, 1);
                    }
                }
                else {
                    // The remove button can also remove a unique media item
                    if (this.mediaItems.length === 1) {
                        this.mediaItems.splice(0, 1);
                    }
                }
                this.selectedMedia = null;
            },
            selectAndDeleteMedia: function (media) {
                var self = this;
                self.selectedMedia = media;
                // setTimeout because sometimes removeSelected was called even before the media was set.
                setTimeout(function () {
                    self.removeSelected();
                }, 100);
            }
        },
        watch: {
            mediaItems: {
                deep: true,
                handler () {
                    // Trigger preview rendering
                    setTimeout(function () { $(document).trigger('contentpreview:render'); }, 100); 
                }
            },            
            currentPrefs: function (newPrefs) {
                localStorage.setItem('mediaFieldPrefs', JSON.stringify(newPrefs));
            }
        }
    }));
}


