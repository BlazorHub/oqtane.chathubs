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

    window.showchathubscontainer = function () {

        var $chathubscontainer = $(".chathubs-container");
        $chathubscontainer.fadeIn(200);
    };

    window.__jsstreams = function () {

        __obj = {

            videolocalid: '#chathubs-video-local-',
            videoremoteid: '#chathubs-video-remote-',
            canvaslocalid: '#chathubs-canvas-local-',
            canvasremoteid: '#chathubs-canvas-remote-',

            videoMimeTypeObject: { mimeType: 'video/webm;codecs=opus,vp9' },
            constrains: {
                audio: true,
                video: {
                    width: { min: 320, ideal: 320, max: 320 },
                    height: { min: 240, ideal: 240, max: 240 },
                    frameRate: 60,
                    facingMode: "user"
                }
            },
            locallivestream: function (roomId, mediaStream) {

                __selflocallivestream = this;
                this.mediaStream = mediaStream;
                this.id = roomId;

                this.videolocalid = self.__obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(this.videolocalid);
                };

                this.canvaslocalid = self.__obj.canvaslocalid + roomId;
                this.getcanvaslocaldomelement = function () {
                    return document.querySelector(this.canvaslocalid);
                };

                this.vElement = this.getvideolocaldomelement();
                this.vElement.srcObject = this.mediaStream;
                this.vElement.onloadedmetadata = function (e) {

                    __selflocallivestream.vElement.play();
                };
                this.vElement.autoplay = true;
                this.vElement.controls = true;
                this.vElement.muted = true;

                this.recordsequence = function () {

                    try {

                        if (__selflocallivestream.recorder.state === 'recording' || __selflocallivestream.recorder.state === 'paused') {

                            __selflocallivestream.recorder.stop();
                        }

                        __selflocallivestream.recorder.start(__selflocallivestream.requestDataInterval);
                        console.log('buffering livestream: please wait: ' + __selflocallivestream.requestDataInterval + 's');
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };

                this.drawimage = function () {

                    try {

                        var videoElement = __selflocallivestream.getvideolocaldomelement();
                        var canvasElement = __selflocallivestream.getcanvaslocaldomelement();
                        var ctx = this.getcanvaslocaldomelement().getContext('2d');

                        ctx.drawImage(videoElement, 0, 0, 320, 240, 0, 0, 320, 240);
                        var dataURI = canvasElement.toDataURL('image/jpeg', 0.42);
                        __selflocallivestream.broadcastsnapshot(dataURI, 'image');
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };

                this.broadcastsnapshot = function (dataURI, dataType) {

                    DotNet.invokeMethodAsync("Oqtane.ChatHubs.Client.Oqtane", 'OnDataAvailable', dataURI, roomId, dataType).then(obj => {
                        console.log(obj.msg);
                    });
                };

                this.options = { mimeType: __obj.videoMimeTypeObject.mimeType, videoBitsPerSecond: 100000, audioBitsPerSecond: 100000, ignoreMutedMedia: true };
                this.recorder = new MediaRecorder(this.mediaStream, this.options);

                this.requestDataInterval = 10000;

                this.recorder.ondataavailable = (event) => {

                    if (event.data.size > 0) {

                        __selflocallivestream.broadcastvideodata(event.data);
                    }
                };

                this.broadcastvideodata = function (chunk) {

                    var reader = new FileReader();
                    reader.onloadend = async function (event) {

                        var dataURI = event.target.result;
                        DotNet.invokeMethodAsync("Oqtane.ChatHubs.Client.Oqtane", 'OnDataAvailable', dataURI, roomId, 'video').then(obj => {
                            console.log(obj.msg);
                        });
                    };
                    reader.readAsDataURL(chunk);
                };

                this.recyclebin = function () {

                    var localElement = __selflocallivestream.getvideolocaldomelement();
                    if (localElement !== null) {

                        __selflocallivestream.mediaStream.getTracks().forEach(track => track.stop());
                    }

                    self.__obj.removelivestream(roomId);
                };
            },
            remotelivestream: function (roomId) {

                __selfremotelivestream = this;
                this.id = roomId;

                this.videoremoteid = self.__obj.videoremoteid + roomId;
                this.getvideoremotedomelement = function () {
                    return document.querySelector(this.videoremoteid);
                };

                this.canvasremoteid = self.__obj.canvasremoteid + roomId;
                this.getcanvasremotedomelement = function () {
                    return document.querySelector(this.canvasremoteid);
                };

                this.remotemediasegments = [];

                this.mediaSource = new MediaSource();
                this.mediaSource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(MediaSource.isTypeSupported(__obj.videoMimeTypeObject.mimeType))) {

                        console.error('Unsupported MIME type or codec: ', self.__obj.videoMimeTypeObject.mimeType);
                    }

                    __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediaSource.addSourceBuffer(__obj.videoMimeTypeObject.mimeType);
                    __selfremotelivestream.sourcebuffer.mode = 'sequence';

                    __selfremotelivestream.sourcebuffer.addEventListener('updatestart', function (e) {});
                    __selfremotelivestream.sourcebuffer.addEventListener('updateend', function (e) {});
                });
                this.mediaSource.addEventListener('sourceended', function (event) { console.log("on media source ended"); });
                this.mediaSource.addEventListener('sourceclose', function (event) { console.log("on media source close"); });

                this.video = this.getvideoremotedomelement();
                this.video.preload = 'auto';
                this.video.width = 320;
                this.video.height = 240;
                this.video.autoplay = true;
                this.video.controls = true;
                this.video.muted = true;
                this.video.src = URL.createObjectURL(this.mediaSource);

                this.rendervideo = function (base64str) {

                    try {
                        console.warn(base64str);
                        __selfremotelivestream.appendBuffer(base64str);
                    }
                    catch (ex) {
                        console.error(ex);
                    }
                };

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

                this.appendBuffer = async function (base64str) {

                    try {

                        var blob = self.__obj.base64ToBlob(base64str);

                        var reader = new FileReader();
                        reader.onloadend = function (event) {

                            __selfremotelivestream.remotemediasegments.push(reader.result);

                            if (__selfremotelivestream.remotemediasegments.length > 20 && !__selfremotelivestream.sourcebuffer.updating && __selfremotelivestream.mediaSource.readyState === 'open') {

                                var item = __selfremotelivestream.remotemediasegments.shift();
                                __selfremotelivestream.sourcebuffer.appendBuffer(new Uint8Array(item));
                            }
                        }
                        reader.readAsArrayBuffer(blob);
                    }
                    catch (ex) {
                        console.error(ex);
                    }
                };

                this.recyclebin = function () {

                    self.__obj.removelivestream(roomId);
                };
            },
            livestreams: [],
            getlivestream: function (roomId) {

                return self.__obj.livestreams.find(item => item.id === roomId);
            },
            addlivestream: function (obj) {

                var item = self.__obj.getlivestream(obj.id);
                if (item !== null) {
                    self.__obj.livestreams.push(obj);
                }
            },
            removelivestream: function (roomId) {

                self.__obj.livestreams = self.__obj.livestreams.filter(item => item.id !== roomId);
            },
            startbroadcasting: function (roomId) {

                navigator.mediaDevices.getUserMedia(this.constrains)
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
            recordsequence: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item instanceof self.__obj.locallivestream) {

                        livestream.item.recordsequence();
                    }
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

                            livestream.item.rendervideo(dataURI);
                        }
                        else if (dataType === 'image') {

                            livestream.item.drawimage(dataURI);
                        }
                        else if (dataType === 'audio') {

                            livestream.item.playaudio(dataURI);
                        }
                    }
                }
            },
            closelivestream: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item instanceof self.__obj.locallivestream) {

                        livestream.item.recyclebin();
                    }
                    else if (livestream.item instanceof self.__obj.remotelivestream) {

                        livestream.item.recyclebin();
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
            readAsTextAsync: function (blob) {

                return new Promise((resolve, reject) => {

                    let fileReader = new window.FileReader();
                    fileReader.onload = () => {

                        resolve(fileReader.result);
                    };
                    fileReader.readAsText(blob);
                })
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
