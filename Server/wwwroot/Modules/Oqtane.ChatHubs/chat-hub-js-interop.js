$(document).ready(function () {

    window.scroll = {

        scrollToBottom: function (elementId, animationTime) {

            var $messageWindow = $(elementId);
            var $lastChild = $messageWindow.children().last();
            var $lastChildHeight = Math.ceil($lastChild.height());
            var tolerance = 30;

            if (Math.ceil($messageWindow.scrollTop() + $messageWindow.innerHeight()) + $lastChildHeight + tolerance >= $messageWindow.prop("scrollHeight")) {

                $messageWindow.animate({ scrollTop: $messageWindow[0].scrollHeight }, { queue: false, duration: animationTime });
            }
        }
    };

    window.browserResize = {

        getInnerHeight: function () {
            return window.innerHeight;
        },
        getInnerWidth: function () {
            return window.innerWidth;
        },
        registerResizeCallback: function () {

            var resizeTimer;
            $(window).on('resize', function (e) {

                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(() => {

                    window.browserResize.resized();
                }, 200);
            });
        },
        resized: function () {

            DotNet.invokeMethodAsync("Oqtane.ChatHubs.Client.Oqtane", 'OnBrowserResize');
        }
    };

    window.saveAsFile = function (filename, bytesBase64) {

        var link = document.createElement('a');
        link.download = filename;
        link.href = "data:application/octet-stream;base64," + bytesBase64;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    window.__jsstreams = function () {

        __obj = {

            videolocalid: '#chathubs-video-local-',
            canvaslocalid: '#chathubs-canvas-local-',
            canvasremoteid: '#chathubs-canvas-remote-',

            canvaslocalwidth: 100,
            canvaslocalheight: 100,

            livestream: function (roomId) {

                this.videolocalid = self.__obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(this.videolocalid);
                };

                this.canvaslocalid = self.__obj.canvaslocalid + roomId;
                this.getcanvaslocaldomelement = function () {
                    return document.querySelector(this.canvaslocalid);
                };

                this.canvasremoteid = self.__obj.canvasremoteid + roomId;
                this.getcanvasremotedomelement = function () {
                    return document.querySelector(this.canvasremoteid);
                };
            },
            livestreams: [],
            addlivestream: function (obj) {

                var item = self.__obj.getlivestream(obj.id);
                if (item !== null) {
                    self.__obj.livestreams.push(obj);
                }
            },
            removelivestream: function (roomId) {

                self.__obj.livestreams = self.__obj.livestreams.filter(item => item.id !== roomId);
            },
            getlivestream: function (roomId) {

                return self.__obj.livestreams.find(item => item.id === roomId);
            },
            constrains: {
                audio: true,
                video: {
                    width: { min: 640, ideal: 720, max: 1024 },
                    height: { min: 480, ideal: 576, max: 512 }
                }
            },
            startvideo: function (roomId) {

                navigator.mediaDevices.getUserMedia(this.constrains)
                    .then(function (mediaStream) {

                        var livestream = new self.__obj.livestream(roomId);

                        var vElement = livestream.getvideolocaldomelement();
                        vElement.srcObject = mediaStream;
                        vElement.onloadedmetadata = function (e) {
                            vElement.play();
                        };

                        var livestreamdicitem = {
                            id: roomId,
                            item: livestream,
                        };

                        self.__obj.addlivestream(livestreamdicitem);
                        self.__obj.resizecanvas();
                    })
                    .catch(function (ex) {
                        console.log(ex.message);
                    });
            },
            captureaudio: function (roomId) {

            },            
            drawimage: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.item.getcanvaslocaldomelement();
                    if (canvas !== null) {
                        var ctx = canvas.getContext('2d');
                        var img = livestream.item.getvideolocaldomelement();

                        ctx.drawImage(img, 0, 0);
                    }
                }
            },
            getimageasbase64string: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.item.getcanvaslocaldomelement();
                    if (canvas !== null) {
                        var dataUrl = canvas.toDataURL("image/jpeg", 0.5);
                        return dataUrl;
                    }
                }
            },
            stopvideo: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var video = livestream.item.getvideolocaldomelement();
                    if (video !== null) {
                        var stream = video.srcObject;
                        var tracks = stream.getTracks();
                        for (var i = 0; i < tracks.length; i++) {
                            var track = tracks[i];+
                            track.stop();
                        }
                        video.srcObject = null;
                    }

                    self.__obj.removelivestream(roomId);
                }
            },
            setimage: function (base64ImageString, roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.item.getcanvasremotedomelement();
                    if (canvas !== null) {
                        var ctx = canvas.getContext('2d');

                        var img = new Image();
                        img.onload = function () {

                            self.__obj.drawimageextension(ctx, img, 0, 0, 640, 480, 0.5, 0.5);
                        };

                        img.src = base64ImageString;
                    }
                }
            },
            resizecanvas: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.item.getcanvasremotedomelement();
                    if (canvas !== null) {

                        var context = canvas.getContext('2d');

                        window.addEventListener('resize', resizeCanvas, false);

                        var resizeCanvas = function() {

                            //canvas.width = window.innerWidth;
                            //canvas.height = window.innerHeight;
                        };
                        resizeCanvas();
                    }
                }
            },
            drawimageextension: function (ctx, img, x, y, w, h, offsetX, offsetY) {

                if (arguments.length === 2) {
                    x = y = 0;
                    w = ctx.canvas.width;
                    h = ctx.canvas.height;
                }

                offsetX = typeof offsetX === "number" ? offsetX : 0.5;
                offsetY = typeof offsetY === "number" ? offsetY : 0.5;

                if (offsetX < 0) offsetX = 0;
                if (offsetY < 0) offsetY = 0;
                if (offsetX > 1) offsetX = 1;
                if (offsetY > 1) offsetY = 1;

                var iw = img.width,
                    ih = img.height,
                    r = Math.min(w / iw, h / ih),
                    nw = iw * r,
                    nh = ih * r,
                    cx, cy, cw, ch, ar = 1;

                if (nw < w) ar = w / nw;
                if (Math.abs(ar - 1) < 1e-14 && nh < h) ar = h / nh;  // updated
                nw *= ar;
                nh *= ar;

                cw = iw / (nw / w);
                ch = ih / (nh / h);

                cx = (iw - cw) * offsetX;
                cy = (ih - ch) * offsetY;

                if (cx < 0) cx = 0;
                if (cy < 0) cy = 0;
                if (cw > iw) cw = iw;
                if (ch > ih) ch = ih;

                ctx.drawImage(img, cx, cy, cw, ch, x, y, w, h);
            },
        };
    };

    window.__initjsstreams = function () {

        return storeObjectRef(new window.__jsstreams());
    };

    var jsObjectRefs = {};
    var jsObjectRefId = 0;
    const jsRefKey = '__jsObjectRefId';
    function storeObjectRef(obj) {
        var id = jsObjectRefId++;
        jsObjectRefs[id] = obj;
        var jsRef = {};
        jsRef[jsRefKey] = id;
        return jsRef;
    };

});
