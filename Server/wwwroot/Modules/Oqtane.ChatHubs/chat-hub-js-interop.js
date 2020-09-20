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

        ___obj = {

            videolocalid: '#chathubs-video-local-',
            videoremoteid: '#chathubs-video-remote-',
            canvaslocalid: '#chathubs-canvas-local-',
            canvasremoteid: '#chathubs-canvas-remote-',

            videoMimeTypeObject: { mimeType: 'video/webm;codecs=opus,vp9' },

            canvaslocalwidth: 100,
            canvaslocalheight: 100,

            livestream: function (roomId, mediaStream) {

                __selflivestream = this;

                this.id = roomId;
                this.mediaStream = mediaStream;

                this.videolocalid = self.___obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(this.videolocalid);
                };

                this.videoremoteid = self.___obj.videoremoteid + roomId;
                this.getvideoremotedomelement = function () {
                    return document.querySelector(this.videoremoteid);
                };

                this.canvaslocalid = self.___obj.canvaslocalid + roomId;
                this.getcanvaslocaldomelement = function () {
                    return document.querySelector(this.canvaslocalid);
                };

                this.canvasremoteid = self.___obj.canvasremoteid + roomId;
                this.getcanvasremotedomelement = function () {
                    return document.querySelector(this.canvasremoteid);
                };

                this.vElement = this.getvideolocaldomelement();
                this.vElement.srcObject = this.mediaStream;
                this.vElement.onloadedmetadata = function (e) {
                    __selflivestream.vElement.play();
                };

                this.localmediasegments = []; this.remotemediasegments = [];

                this.mediaSource = new MediaSource();

                this.options = { mimeType: ___obj.videoMimeTypeObject.mimeType, videoBitsPerSecond: 100000, audioBitsPerSecond: 100000, ignoreMutedMedia: true };
                this.recorder = new MediaRecorder(this.mediaStream, this.options);

                this.requestDataInterval = 250;
                this.recorder.start(this.requestDataInterval);
                console.log('buffering livestream: please wait: ' + this.requestDataInterval + 's');

                this.mediaSource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(MediaSource.isTypeSupported(___obj.videoMimeTypeObject.mimeType))) {

                        console.error('Unsupported MIME type or codec: ', self.videostreams.videoMimeTypeObject.mimeType);
                    }

                    __selflivestream.sourcebuffer = __selflivestream.mediaSource.addSourceBuffer(___obj.videoMimeTypeObject.mimeType);
                    __selflivestream.sourcebuffer.addEventListener('updateend', function (e) { });
                });
                this.mediaSource.addEventListener('error', function () { console.error('on media source error'); });
                this.mediaSource.addEventListener('sourceclose', function () { console.log("on media source close"); });
                this.mediaSource.addEventListener('sourceended', function () { console.log("on media source ended"); });

                this.recorder.ondataavailable = (event) => {

                    if (event.data.size >= 0) {

                        __selflivestream.broadcastVideoData(event.data);
                    }
                };
                this.appendBufferLocalChunkDeprecated = function (chunk) {

                    var reader = new FileReader();
                    reader.onload = function (event) {
                        __selflivestream.sourcebuffer.appendBuffer(new Uint8Array(event.target.result));
                    };
                    reader.readAsArrayBuffer(chunk);
                };
                this.appendBuffer = function (base64str) {

                    try {

                        var blob = __selflivestream.base64ToBlob(base64str);
                        var reader = new FileReader();
                        reader.onloadend = function (event) {

                            __selflivestream.remotemediasegments.push(reader.result);

                            if (!__selflivestream.sourcebuffer.updating && __selflivestream.mediaSource.readyState === 'open') {

                                var item = __selflivestream.remotemediasegments.shift();
                                __selflivestream.sourcebuffer.appendBuffer(new Uint8Array(item));
                            }
                        }
                        reader.readAsArrayBuffer(blob);
                    }
                    catch (ex) {
                        console.error(ex);
                    }
                };
                this.base64ToBlob = function (base64str) {

                    var byteString = atob(base64str.split('base64,')[1]);
                    var arrayBuffer = new ArrayBuffer(byteString.length);

                    var bytes = new Uint8Array(arrayBuffer);
                    for (var i = 0; i < byteString.length; i++) {
                        bytes[i] = byteString.charCodeAt(i);
                    }

                    var blob = new Blob([arrayBuffer], { type: ___obj.videoMimeTypeObject.mimeType });
                    return blob;
                };
                this.broadcastVideoData = function (chunk) {

                    var reader = new FileReader();
                    reader.onloadend = async function (event) {

                        var dataURI = event.target.result;
                        DotNet.invokeMethodAsync("Oqtane.ChatHubs.Client.Oqtane", 'OnDataAvailable', dataURI, roomId);
                    }
                    reader.readAsDataURL(chunk);
                };

                this.video = this.getvideoremotedomelement();
                this.video.preload = 'auto';
                this.video.width = 320;
                this.video.height = 240;
                this.video.autoplay = true;
                this.video.controls = true;
                this.video.muted = true;
                this.video.src = URL.createObjectURL(this.mediaSource);
                //this.video.onloadedmetadata = function () { __selflivestream.video.play(); };

                this.recyclebin = function () {

                    var localElement = __selflivestream.getvideolocaldomelement();
                    if (localElement !== null) {

                        __selflivestream.recorder.stop();
                        __selflivestream.mediaStream.getTracks().forEach(track => track.stop());
                    }

                    var remoteElement = __selflivestream.getvideoremotedomelement();
                    if (remoteElement !== null) {

                        if (__selflivestream.mediaSource.readyState === 'open') {

                            __selflivestream.mediaSource.endOfStream();
                        }
                    }

                    self.___obj.removelivestream(roomId);
                }

            },
            livestreams: [],
            addlivestream: function (obj) {

                var item = self.___obj.getlivestream(obj.id);
                if (item !== null) {
                    self.___obj.livestreams.push(obj);
                }
            },
            removelivestream: function (roomId) {

                self.___obj.livestreams = self.___obj.livestreams.filter(item => item.id !== roomId);
            },
            getlivestream: function (roomId) {

                return self.___obj.livestreams.find(item => item.id === roomId);
            },
            constrains: {
                audio: true,
                video: {
                    width: { min: 320, ideal: 320, max: 320 },
                    height: { min: 240, ideal: 240, max: 240 },
                    frameRate: 3,
                    facingMode: "user"
                }
            },
            startvideo: function (roomId) {

                navigator.mediaDevices.getUserMedia(this.constrains)
                    .then(function (mediaStream) {

                        var livestream = new self.___obj.livestream(roomId, mediaStream);
                        var livestreamdicitem = {
                            id: roomId,
                            item: livestream,
                        };

                        self.___obj.addlivestream(livestreamdicitem);
                        self.___obj.resizecanvas();
                    })
                    .catch(function (ex) {
                        console.log(ex.message);
                    });
            },
            setitem: function (base64str, roomId) {

                var livestream = self.___obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    livestream.item.appendBuffer(base64str);
                }
            },
            closelivestream: function (roomId) {

                var livestream = self.___obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    livestream.item.recyclebin();
                }
            },
            drawimage: function (roomId) {

                var livestream = self.___obj.getlivestream(roomId);
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

                var livestream = self.___obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.item.getcanvaslocaldomelement();
                    if (canvas !== null) {
                        var dataUrl = canvas.toDataURL("image/jpeg", 0.5);
                        return dataUrl;
                    }
                }
            },
            setimage: function (base64ImageString, roomId) {

                var livestream = self.___obj.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.item.getcanvasremotedomelement();
                    if (canvas !== null) {
                        var ctx = canvas.getContext('2d');

                        var img = new Image();
                        img.onload = function () {

                            //self.___obj.drawimageextension(ctx, img, 0, 0, 640, 480, 0.5, 0.5);
                        };

                        img.src = base64ImageString;
                    }
                }
            },
            readAsDataUrlAsync: function (blob) {

                return new Promise((resolve, reject) => {

                    let fileReader = new window.FileReader();
                    fileReader.onload = () => {

                        resolve(fileReader.result);
                    };
                    fileReader.readAsDataURL(blob);
                })
            },
            readAsArrayBufferAsync: function (blob) {

                return new Promise((resolve, reject) => {

                    let fileReader = new window.FileReader();
                    fileReader.onload = () => {

                        resolve(fileReader.result);
                    };
                    fileReader.readAsArrayBuffer(blob);
                })
            },
            resizecanvas: function (roomId) {

                var livestream = self.___obj.getlivestream(roomId);
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
                if (Math.abs(ar - 1) < 1e-14 && nh < h) ar = h / nh;
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
