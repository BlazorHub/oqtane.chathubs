﻿$(document).ready(function () {

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

            videoMimeTypeObject: { mimeType: 'video/webm;codecs="opus,vp9"' },

            canvaslocalwidth: 100,
            canvaslocalheight: 100,

            livestream: function (roomId, mediaStream) {

                __selflivestream = this;

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
                this.vElement.srcObject = mediaStream;
                this.vElement.onloadedmetadata = function (e) {
                    __selflivestream.vElement.play();
                };

                this.localmediasegments = []; this.remotemediasegments = [];
                this.addMediaSegment = function (item) {
                    this.localmediasegments.push(item);
                };

                this.mediaSource = new MediaSource();

                this.options = { mimeType: 'video/webm;codecs="opus,vp9"', videoBitsPerSecond: 6000, audioBitsPerSecond: 100000 };
                this.recorder = new MediaRecorder(mediaStream, this.options);

                this.buffertime = 3000;
                this.recorder.start(this.buffertime);
                console.log('buffering livestream: please wait: ' + this.buffertime + 's');

                this.mediaSource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(MediaSource.isTypeSupported(___obj.videoMimeTypeObject.mimeType))) {

                        console.error('Unsupported MIME type or codec: ', self.videostreams.videoMimeTypeObject.mimeType);
                    }

                    __selflivestream.sourcebuffer = __selflivestream.mediaSource.addSourceBuffer(___obj.videoMimeTypeObject.mimeType);
                    __selflivestream.sourcebuffer.addEventListener('updateend', function (e) { });
                });
                this.mediaSource.addEventListener('error', function (e) { console.log(e) }, false);

                this.mediaSource.addEventListener('sourceclose', function () { console.log("on media source close"); });
                this.mediaSource.addEventListener('sourceended', function () { console.log("on media source ended"); });

                this.recorder.ondataavailable = (event) => {

                    try {

                        if (event.data.size >= 0) {

                            __selflivestream.localmediasegments.push(event.data);

                            if (!__selflivestream.sourcebuffer.updating) {

                                var item = __selflivestream.localmediasegments.shift();
                                __selflivestream.appendBuffer(item);
                            }
                        }
                    }
                    catch (error) {
                        console.log(error);
                    }
                };
                this.appendBuffer = function (chunk) {

                    var reader = new FileReader();
                    reader.onload = function (event) {
                        __selflivestream.sourcebuffer.appendBuffer(new Uint8Array(event.target.result));
                    };
                    reader.readAsArrayBuffer(chunk);
                };

                this.video = this.getvideoremotedomelement();
                this.video.preload = 'metadata';
                this.video.width = 640;
                this.video.height = 480;
                this.video.autoplay = true;
                this.video.controls = true;
                this.video.muted = false;

                this.video.src = URL.createObjectURL(this.mediaSource);
                this.video.onloadedmetadata = function () {

                    __selflivestream.video.play();
                };

                /*
                this.output = this.getcanvaslocaldomelement();
                this.ctx = this.output.getContext('2d');

                this.loop = function () {

                    __selflivestream.ctx.drawImage(__selflivestream.video, 0, 0);
                    if (!__selflivestream.video.paused) {
                        requestAnimationFrame(__selflivestream.loop);
                    }
                };
                */
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
                    width: { min: 640, ideal: 720, max: 1024 },
                    height: { min: 480, ideal: 576, max: 512 }
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
            captureaudio: function (roomId) {

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
            stopvideo: function (roomId) {

                var livestream = self.___obj.getlivestream(roomId);
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

                    self.___obj.removelivestream(roomId);
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
