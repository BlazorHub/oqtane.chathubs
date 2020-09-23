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
            videoremoteid: '#chathubs-video-remote-',
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
            livestream: function (roomId, mediaStream) {

                __selflivestream = this;

                this.id = roomId;
                this.mediaStream = mediaStream;

                this.videolocalid = self.__obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(this.videolocalid);
                };

                this.videoremoteid = self.__obj.videoremoteid + roomId;
                this.getvideoremotedomelement = function () {
                    return document.querySelector(this.videoremoteid);
                };

                this.vElement = this.getvideolocaldomelement();
                this.vElement.srcObject = this.mediaStream;
                this.vElement.onloadedmetadata = function (e) {
                    __selflivestream.vElement.play();
                };
                this.vElement.autoplay = true;
                this.vElement.controls = true;
                this.vElement.muted = true;

                this.localmediasegments = []; this.remotemediasegments = [];

                this.mediaSource = new MediaSource();

                this.options = { mimeType: __obj.videoMimeTypeObject.mimeType, videoBitsPerSecond: 100000, audioBitsPerSecond: 100000, ignoreMutedMedia: true };
                this.recorder = new MediaRecorder(this.mediaStream, this.options);

                this.requestDataInterval = 200;
                this.recorder.start(this.requestDataInterval);
                console.log('buffering livestream: please wait: ' + this.requestDataInterval + 's');

                this.mediaSource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(MediaSource.isTypeSupported(__obj.videoMimeTypeObject.mimeType))) {

                        console.error('Unsupported MIME type or codec: ', self.videostreams.videoMimeTypeObject.mimeType);
                    }

                    __selflivestream.sourcebuffer = __selflivestream.mediaSource.addSourceBuffer(__obj.videoMimeTypeObject.mimeType);
                    __selflivestream.sourcebuffer.addEventListener('updateend', function (e) { });
                });
                this.mediaSource.addEventListener('error', function () { console.error('on media source error'); });
                this.mediaSource.addEventListener('sourceclose', function () { console.log("on media source close"); });
                this.mediaSource.addEventListener('sourceended', function () { console.log("on media source ended"); });

                this.video = this.getvideoremotedomelement();
                this.video.preload = 'auto';
                this.video.width = 320;
                this.video.height = 240;
                this.video.autoplay = true;
                this.video.controls = true;
                this.video.muted = true;
                this.video.src = URL.createObjectURL(this.mediaSource);
                //this.video.onloadedmetadata = function () { __selflivestream.video.play(); };

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

                    var blob = new Blob([arrayBuffer], { type: __obj.videoMimeTypeObject.mimeType });
                    return blob;
                };
                this.broadcastVideoData = function (chunk) {

                    var reader = new FileReader();
                    reader.onloadend = async function (event) {

                        var dataURI = event.target.result;
                        DotNet.invokeMethodAsync("Oqtane.ChatHubs.Client.Oqtane", 'OnDataAvailable', dataURI, roomId).then(obj => {
                            console.log(obj.msg);
                        });
                    }
                    reader.readAsDataURL(chunk);
                };
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
            startvideo: function (roomId) {

                navigator.mediaDevices.getUserMedia(this.constrains)
                    .then(function (mediaStream) {

                        var livestream = new self.__obj.livestream(roomId, mediaStream);
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
            appendbuffer: function (base64str, roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    livestream.item.appendBuffer(base64str);
                }
            },
            closelivestream: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    livestream.item.recyclebin();
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
