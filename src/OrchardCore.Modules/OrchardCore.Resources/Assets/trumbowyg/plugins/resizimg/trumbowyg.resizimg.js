;(function ($) {
    'use strict';

    var defaultOptions = {
        minSize: 32,
        step: 4
    };

    function preventDefault(e) {
        e.stopPropagation();
        e.preventDefault();
    }

    var ResizeWithCanvas = function () {
        // variable to create canvas and save img in resize mode
        this.resizeCanvas = document.createElement('canvas');
        // to allow canvas to get focus
        this.resizeCanvas.setAttribute('tabindex', '0');
        this.resizeCanvas.id = 'trumbowyg-resizimg-' + (+new Date());
        this.ctx = null;
        this.resizeImg = null;

        this.pressEscape = function (obj) {
            obj.reset();
        };
        this.pressBackspaceOrDelete = function (obj) {
            $(obj.resizeCanvas).replaceWith('');
            obj.resizeImg = null;
        };

        // PRIVATE FUNCTION
        var focusedNow = false;
        var isCursorSeResize = false;

        // calculate offset to change mouse over square in the canvas
        var offsetX, offsetY;
        var reOffset = function (canvas) {
            var BB = canvas.getBoundingClientRect();
            offsetX = BB.left;
            offsetY = BB.top;
        };

        var drawRect = function (shapeData, ctx) {
            // Inner
            ctx.beginPath();
            ctx.fillStyle = 'rgb(255, 255, 255)';
            ctx.rect(shapeData.points.x, shapeData.points.y, shapeData.points.width, shapeData.points.height);
            ctx.fill();
            ctx.stroke();
        };

        var updateCanvas = function (canvas, ctx, img, canvasWidth, canvasHeight) {
            ctx.translate(0.5, 0.5);
            ctx.lineWidth = 1;

            // image
            ctx.drawImage(img, 5, 5, canvasWidth - 10, canvasHeight - 10);

            // border
            ctx.beginPath();
            ctx.rect(5, 5, canvasWidth - 10, canvasHeight - 10);
            ctx.stroke();

            // square in the angle
            ctx.beginPath();
            ctx.fillStyle = 'rgb(255, 255, 255)';
            ctx.rect(canvasWidth - 10, canvasHeight - 10, 9, 9);
            ctx.fill();
            ctx.stroke();

            // get the offset to change the mouse cursor
            reOffset(canvas);

            return ctx;
        };

        // PUBLIC FUNCTION
        // necessary to correctly print cursor over square. Called once for instance. Useless with trumbowyg.
        this.init = function () {
            var _this = this;
            $(window).on('scroll resize', function () {
                _this.reCalcOffset();
            });
        };

        this.reCalcOffset = function () {
            reOffset(this.resizeCanvas);
        };

        this.canvasId = function () {
            return this.resizeCanvas.id;
        };

        this.isActive = function () {
            return this.resizeImg !== null;
        };

        this.isFocusedNow = function () {
            return focusedNow;
        };

        this.blurNow = function () {
            focusedNow = false;
        };

        // restore image in the HTML of the editor
        this.reset = function () {
            if (this.resizeImg === null) {
                return;
            }

            this.resizeImg.width = this.resizeCanvas.clientWidth - 10;
            this.resizeImg.height = this.resizeCanvas.clientHeight - 10;
            // clear style of image to avoid issue on resize because this attribute have priority over width and height attribute
            this.resizeImg.removeAttribute('style');

            $(this.resizeCanvas).replaceWith($(this.resizeImg));

            // reset canvas style
            this.resizeCanvas.removeAttribute('style');
            this.resizeImg = null;
        };

        // setup canvas with points and border to allow the resizing operation
        this.setup = function (img, resizableOptions) {
            this.resizeImg = img;

            if (!this.resizeCanvas.getContext) {
                return false;
            }

            focusedNow = true;

            // draw canvas
            this.resizeCanvas.width = $(this.resizeImg).width() + 10;
            this.resizeCanvas.height = $(this.resizeImg).height() + 10;
            this.resizeCanvas.style.margin = '-5px';
            this.ctx = this.resizeCanvas.getContext('2d');

            // replace image with canvas
            $(this.resizeImg).replaceWith($(this.resizeCanvas));

            updateCanvas(this.resizeCanvas, this.ctx, this.resizeImg, this.resizeCanvas.width, this.resizeCanvas.height);

            // enable resize
            $(this.resizeCanvas).resizable(resizableOptions)
                .on('mousedown', preventDefault);

            var _this = this;
            $(this.resizeCanvas)
                .on('mousemove', function (e) {
                    var mouseX = Math.round(e.clientX - offsetX);
                    var mouseY = Math.round(e.clientY - offsetY);

                    var wasCursorSeResize = isCursorSeResize;

                    _this.ctx.rect(_this.resizeCanvas.width - 10, _this.resizeCanvas.height - 10, 9, 9);
                    isCursorSeResize = _this.ctx.isPointInPath(mouseX, mouseY);
                    if (wasCursorSeResize !== isCursorSeResize) {
                        this.style.cursor = isCursorSeResize ? 'se-resize' : 'default';
                    }
                })
                .on('keydown', function (e) {
                    if (!_this.isActive()) {
                        return;
                    }

                    var x = e.keyCode;
                    if (x === 27) { // ESC
                        _this.pressEscape(_this);
                    } else if (x === 8 || x === 46) { // BACKSPACE or DELETE
                        _this.pressBackspaceOrDelete(_this);
                    }
                })
                .on('focus', preventDefault);

            this.resizeCanvas.focus();

            return true;
        };

        // update the canvas after the resizing
        this.refresh = function () {
            if (!this.resizeCanvas.getContext) {
                return;
            }

            this.resizeCanvas.width = this.resizeCanvas.clientWidth;
            this.resizeCanvas.height = this.resizeCanvas.clientHeight;
            updateCanvas(this.resizeCanvas, this.ctx, this.resizeImg, this.resizeCanvas.width, this.resizeCanvas.height);
        };
    };

    // object to interact with canvas
    var resizeWithCanvas = new ResizeWithCanvas();

    function destroyResizable(trumbowyg) {
        // clean html code
        trumbowyg.$ed.find('canvas.resizable')
            .resizable('destroy')
            .off('mousedown', preventDefault)
            .removeClass('resizable');

        resizeWithCanvas.reset();

        trumbowyg.syncCode();
    }

    $.extend(true, $.trumbowyg, {
        plugins: {
            resizimg: {
                init: function (trumbowyg) {
                    trumbowyg.o.plugins.resizimg = $.extend(true, {},
                        defaultOptions,
                        trumbowyg.o.plugins.resizimg || {},
                        {
                            resizable: {
                                resizeWidth: false,
                                onDragStart: function (ev, $el) {
                                    var opt = trumbowyg.o.plugins.resizimg;
                                    var x = ev.pageX - $el.offset().left;
                                    var y = ev.pageY - $el.offset().top;
                                    if (x < $el.width() - opt.minSize || y < $el.height() - opt.minSize) {
                                        return false;
                                    }
                                },
                                onDrag: function (ev, $el, newWidth, newHeight) {
                                    var opt = trumbowyg.o.plugins.resizimg;
                                    if (newHeight < opt.minSize) {
                                        newHeight = opt.minSize;
                                    }
                                    newHeight -= newHeight % opt.step;
                                    $el.height(newHeight);
                                    return false;
                                },
                                onDragEnd: function () {
                                    // resize update canvas information
                                    resizeWithCanvas.refresh();
                                    trumbowyg.syncCode();
                                }
                            }
                        }
                    );

                    function initResizable() {
                        trumbowyg.$ed.find('img')
                            .off('click')
                            .on('click', function (e) {
                                // if I'm already do a resize, reset it
                                if (resizeWithCanvas.isActive()) {
                                    resizeWithCanvas.reset();
                                }
                                // initialize resize of image
                                resizeWithCanvas.setup(this, trumbowyg.o.plugins.resizimg.resizable);

                                preventDefault(e);
                            });
                    }

                    trumbowyg.$c.on('tbwinit', function () {
                        initResizable();

                        // disable resize when click on other items
                        trumbowyg.$ed.on('click', function (e) {
                            // check if I've clicked out of canvas or image to reset it
                            if ($(e.target).is('img') || e.target.id === resizeWithCanvas.canvasId()) {
                                return;
                            }

                            preventDefault(e);
                            resizeWithCanvas.reset();

                            // save changes
                            trumbowyg.$c.trigger('tbwchange');
                        });

                        trumbowyg.$ed.on('scroll', function () {
                            resizeWithCanvas.reCalcOffset();
                        });
                    });

                    trumbowyg.$c.on('tbwfocus tbwchange', initResizable);
                    trumbowyg.$c.on('tbwresize', function () {
                        resizeWithCanvas.reCalcOffset();
                    });

                    // Destroy
                    trumbowyg.$c.on('tbwblur', function () {
                        // if I have already focused the canvas avoid destroy
                        if (resizeWithCanvas.isFocusedNow()) {
                            resizeWithCanvas.blurNow();
                        } else {
                            destroyResizable(trumbowyg);
                        }
                    });
                },
                destroy: function (trumbowyg) {
                    destroyResizable(trumbowyg);
                }
            }
        }
    });
})(jQuery);
