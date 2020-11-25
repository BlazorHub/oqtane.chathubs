$(document).ready(function () {

    window.scroll = {

        scrollToBottom: function (elementId, animationTime) {

            var $messageWindow = $(elementId);
            var $lastChild = $messageWindow.children().last();
            var lastChildHeight = Math.ceil($lastChild.height());
            var tolerance = 30;

            if (Math.ceil($messageWindow.scrollTop() + $messageWindow.innerHeight()) + lastChildHeight + tolerance >= $messageWindow.prop("scrollHeight")) {

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

    window.showchathubscontainer = function () {

        var $chathubscontainer = $(".chathubs-container");
        $chathubscontainer.fadeIn(240);
    };

    window.cookies = {

        setCookie: function (cookiename, cookievalue, expirationdays) {
            var d = new Date();
            d.setTime(d.getTime() + (expirationdays * 24 * 60 * 60 * 1000));
            var expires = "expires=" + d.toUTCString();
            document.cookie = cookiename + "=" + cookievalue + ";" + expires + ";path=/";
        },
        getCookie: function (cookiename) {
            var name = cookiename + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return "";
        },
    };

    window.__jsstreams = function (videoserviceObjectReference) {

        __obj = {

            videoservice: videoserviceObjectReference,
            videolocalid: '#chathubs-video-local-',
            videoremoteid: '#chathubs-video-remote-',
            canvaslocalid: '#chathubs-canvas-local-',
            canvasremoteid: '#chathubs-canvas-remote-',
            videoMimeTypeObject: { mimeType: 'video/webm;codecs=opus,vp9' },
            constrains: {
                audio: true,
                video: {
                    width: { min: 150, ideal: 150, max: 150 },
                    height: { min: 100, ideal: 100, max: 100 },
                    frameRate: 10,
                    facingMode: "user"
                }
            },
            livestreams: [],
            locallivestream: function (roomId, mediaStream) {

                __selflocallivestream = this;

                this.videolocalid = self.__obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.videolocalid);
                };

                this.canvaslocalid = self.__obj.canvaslocalid + roomId;
                this.getcanvaslocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.canvaslocalid);
                };

                this.vElement = this.getvideolocaldomelement();
                this.vElement.srcObject = mediaStream;
                this.vElement.onloadedmetadata = function (e) {

                    __selflocallivestream.vElement.play();
                };
                this.vElement.autoplay = true;
                this.vElement.controls = true;
                this.vElement.muted = true;

                this.options = { mimeType: __obj.videoMimeTypeObject.mimeType, videoBitsPerSecond: 1000, audioBitsPerSecond: 1000, ignoreMutedMedia: true };
                this.recorder = new MediaRecorder(mediaStream, this.options);
                this.recorder.start();

                this.startsequence = function () {

                    try {

                        if (__selflocallivestream.recorder.state === 'inactive' || __selflocallivestream.recorder.state === 'paused') {

                            __selflocallivestream.recorder.start();
                        }
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                },
                this.stopsequence = function () {

                    try {

                        if (__selflocallivestream.recorder.state === 'recording' || __selflocallivestream.recorder.state === 'paused') {

                            __selflocallivestream.recorder.stop();
                        }
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };
                this.recorder.ondataavailable = (event) => {

                    if (event.data.size > 0) {

                        console.log(event.data);
                        __selflocallivestream.broadcastvideodata(event.data);
                    }
                };
                this.broadcastvideodata = function (sequence) {

                    var reader = new FileReader();
                    reader.onloadend = async function (event) {

                        var dataURI = event.target.result;
                        
                        var totalBytes = Math.ceil(event.total * 8 / 6);
                        var totalKiloBytes = Math.ceil(totalBytes / 1024);
                        if (totalKiloBytes >= 32) {

                            console.warn('data uri too large to broadcast >= 32kb');
                            return;
                        }

                        self.__obj.videoservice.invokeMethodAsync('OnDataAvailable', dataURI, roomId, 'video').then(obj => {
                            console.log(obj.msg);
                        });
                    };
                    reader.readAsDataURL(sequence);
                };

                this.drawimage = function () {

                    try {

                        var videoElement = __selflocallivestream.getvideolocaldomelement();
                        var canvasElement = __selflocallivestream.getcanvaslocaldomelement();
                        var ctx = this.getcanvaslocaldomelement().getContext('2d');

                        ctx.drawImage(videoElement, 0, 0, 300, 150, 0, 0, 300, 150);
                        var dataURI = canvasElement.toDataURL('image/jpeg', 0.42);
                        __selflocallivestream.broadcastsnapshot(dataURI, 'image');
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };
                this.broadcastsnapshot = function (dataURI, dataType) {

                    self.__obj.videoservice.invokeMethodAsync('OnDataAvailable', dataURI, roomId, dataType).then(obj => {
                        console.log(obj.msg);
                    });
                };
                this.cancel = function () {

                    try {
                        if (__selflocallivestream.recorder.state === 'recording' || __selflocallivestream.recorder.state === 'paused') {

                            __selflocallivestream.recorder.stop();
                        }
                        mediaStream.getTracks().forEach(track => track.stop());
                    }
                    catch (err) {
                        console.error(err);
                    }
                };
            },
            remotelivestream: function (roomId) {

                __selfremotelivestream = this;

                this.videoremoteid = self.__obj.videoremoteid + roomId;
                this.getvideoremotedomelement = function () {
                    return document.querySelector(__selfremotelivestream.videoremoteid);
                };

                this.canvasremoteid = self.__obj.canvasremoteid + roomId;
                this.getcanvasremotedomelement = function () {
                    return document.querySelector(__selfremotelivestream.canvasremoteid);
                };

                this.remotemediasequences = [];

                this.mediaSource = new MediaSource();
                this.mediaSource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(MediaSource.isTypeSupported(__obj.videoMimeTypeObject.mimeType))) {

                        console.error('Unsupported MIME type or codec: ', self.__obj.videoMimeTypeObject.mimeType);
                    }

                    __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediaSource.addSourceBuffer(__obj.videoMimeTypeObject.mimeType);
                    __selfremotelivestream.sourcebuffer.mode = 'sequence';

                    __selfremotelivestream.sourcebuffer.addEventListener('updatestart', function (e) {});
                    __selfremotelivestream.sourcebuffer.addEventListener('update', function (e) {});
                    __selfremotelivestream.sourcebuffer.addEventListener('updateend', function (e) {

                        if (e.currentTarget.buffered.length === 1) {

                            var timestampOffset = __selfremotelivestream.sourcebuffer.timestampOffset;
                            var end = e.currentTarget.buffered.end(0);
                        }
                    });
                });
                this.mediaSource.addEventListener('sourceended', function (event) { console.log("on media source ended"); });
                this.mediaSource.addEventListener('sourceclose', function (event) { console.log("on media source close"); });

                this.video = this.getvideoremotedomelement();
                this.video.controls = true;
                this.video.autoplay = false;
                this.video.preload = 'auto';
                this.video.muted = true;

                try {
                    this.video.srcObject = this.mediaSource;
                } catch (ex) {
                    console.warn(ex);
                    this.video.src = URL.createObjectURL(this.mediaSource);
                }

                this.drawimage = function (base64str) {

                    try {
                        var canvas = __selfremotelivestream.getcanvasremotedomelement();
                        var ctx = canvas.getContext("2d");

                        var image = new Image();
                        image.onload = function () {
                            ctx.drawImage(image, 0, 0);
                        };
                        image.src = base64str;
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };

                this.appendsequencecounter = 0;
                this.appendBuffer = async function (base64str) {

                    try {

                        console.log(base64str);
                        var blob = self.__obj.base64ToBlob(base64str);

                        var reader = new FileReader();
                        reader.onloadend = function (event) {

                            __selfremotelivestream.remotemediasequences.push(reader.result);
                            var item = __selfremotelivestream.remotemediasequences.shift();

                            if (!__selfremotelivestream.sourcebuffer.updating && __selfremotelivestream.mediaSource.readyState === 'open') {

                                __selfremotelivestream.sourcebuffer.appendBuffer(new Uint8Array(item));
                                __selfremotelivestream.appendsequencecounter++;
                            }
                            if (__selfremotelivestream.appendsequencecounter === 10) {

                                __selfremotelivestream.video.play();
                            }
                        }
                        reader.readAsArrayBuffer(blob);
                    }
                    catch (ex) {
                        console.error(ex);
                    }
                };

                this.cancel = function () {

                };
            },
            getlivestream: function (roomId) {

                return self.__obj.livestreams.find(item => item.id === roomId);
            },
            addlivestream: function (obj) {

                var item = self.__obj.getlivestream(obj.id);
                if (item === undefined) {

                    self.__obj.livestreams.push(obj);
                }
            },
            removelivestream: function (roomId) {

                //self.__obj.livestreams = self.__obj.livestreams.filter(item => item.id !== roomId);
                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    self.__obj.livestreams.splice(self.__obj.livestreams.indexOf(livestream), 1);
                }
            },
            startbroadcasting: function (roomId) {

                window.navigator.mediaDevices.getUserMedia(self.__obj.constrains)
                    .then(function (mediaStream) {

                        var livestream = new self.__obj.locallivestream(roomId, mediaStream);
                        var livestreamdicitem = {
                            id: roomId,
                            item: livestream,
                        };

                        self.__obj.addlivestream(livestreamdicitem);
                    })
                    .catch(function (ex) {
                        console.log(ex.message);
                    });
            },
            startstreaming: function (roomId) {

                    var livestream = new self.__obj.remotelivestream(roomId);
                    var livestreamdicitem = {
                        id: roomId,
                        item: livestream,
                    };

                self.__obj.addlivestream(livestreamdicitem);
            },
            startsequence: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined && livestream.item instanceof self.__obj.locallivestream) {

                    livestream.item.startsequence();
                }
            },
            stopsequence: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined && livestream.item instanceof self.__obj.locallivestream) {

                    livestream.item.stopsequence();
                }
            },
            drawimage: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item instanceof self.__obj.locallivestream) {

                        livestream.item.drawimage();
                    }
                }
            },
            appendbuffer: function (dataURI, roomId, dataType) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item instanceof self.__obj.remotelivestream) {

                        if (dataType === 'video') {

                            livestream.item.appendBuffer(dataURI);
                        }
                        else if (dataType === 'image') {

                            livestream.item.drawimage(dataURI);
                        }
                    }
                }
            },
            closelivestream: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    livestream.item.cancel();
                    self.__obj.removelivestream(roomId);
                }
            },
            base64ToBlob: function (base64str) {

                var byteString = atob(base64str.split('base64,')[1]);
                var arrayBuffer = new ArrayBuffer(byteString.length);

                var bytes = new Uint8Array(arrayBuffer);
                for (var i = 0; i < byteString.length; i++) {
                    bytes[i] = byteString.charCodeAt(i);
                }

                var blob = new Blob([arrayBuffer], { type: self.__obj.videoMimeTypeObject.mimeType });
                return blob;
            },
        };
    };

    window.__initjsstreams = function (videoserviceObjectReference) {

        return storeObjectRef(new window.__jsstreams(videoserviceObjectReference));
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
