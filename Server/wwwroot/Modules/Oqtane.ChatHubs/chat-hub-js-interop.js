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

    window.showChatPage = function () {

        var $chatPage = $("#chat-page");
        $chatPage.fadeIn(200);
    };

    window.saveAsFile = function (filename, bytesBase64) {

        var link = document.createElement('a');
        link.download = filename;
        link.href = "data:application/octet-stream;base64," + bytesBase64;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    window.videojs = function () {

        videostreams = {

            videoId: '#chathubs-video-',
            canvasId: '#chathubs-canvas-',
            canvasDownloadId: '#chathubs-canvas-download-',

            video: function () {

                this.videoId = '';
                this.setVideoId = function (roomId) {
                    this.videoId = self.videostreams.videoId + roomId;
                };
                this.getVideoDomElement = function () {
                    return document.querySelector(this.videoId);
                };
            },
            canvas: function () {

                this.canvasId = '';
                this.setCanvasId = function (roomId) {
                    this.canvasId = self.videostreams.canvasId + roomId;
                };
                this.getCanvasDomElement = function () {
                    return document.querySelector(this.canvasId);
                };
            },
            canvasDownload: function () {

                this.canvasDownloadId = '';
                this.setCanvasDownloadId = function (roomId) {
                    this.canvasDownloadId = self.videostreams.canvasDownloadId + roomId;
                };
                this.getCanvasDownloadDomElement = function () {
                    return document.querySelector(this.canvasDownloadId);
                };
            },
            livestreams: [],
            addlivestream: function (obj) {

                var item = self.videostreams.getlivestream(obj.id);
                if (item !== null) {
                    self.videostreams.livestreams.push(obj);
                }
            },
            getlivestream: function (roomId) {

                return self.videostreams.livestreams.find(item => item.id === roomId);
            },
            constrains: {
                audio: true,
                video: {
                    width: { min: 640, ideal: 720, max: 1024 },
                    height: { min: 480, ideal: 576, max: 512 }
                }
            },
            startVideo: function (roomId) {

                navigator.mediaDevices.getUserMedia(this.constrains)
                    .then(function (mediaStream) {

                        var vInstance = new self.videostreams.video();
                        vInstance.setVideoId(roomId);
                        var vElement = vInstance.getVideoDomElement();
                        vElement.srcObject = mediaStream;
                        vElement.onloadedmetadata = function (e) {
                            vElement.play();
                        };

                        var cInstance = new self.videostreams.canvas();
                        cInstance.setCanvasId(roomId);

                        var cdInstance = new self.videostreams.canvasDownload();
                        cdInstance.setCanvasDownloadId(roomId);

                        var dicItem = {
                            id: roomId,
                            video: vInstance,
                            canvas: cInstance,
                            canvas_download: cdInstance
                        };

                        self.videostreams.addlivestream(dicItem);
                    })
                    .catch(function (ex) {
                        console.log(ex.message);
                    });
            },
            captureAudio: function (roomId) {

            },
            drawImage: function (roomId) {

                var livestream = self.videostreams.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.canvas.getCanvasDomElement();
                    if (canvas !== null) {
                        var context = canvas.getContext('2d');
                        var image = livestream.video.getVideoDomElement();
                        context.drawImage(image, 0, 0);
                    }
                }
            },
            getImageAsBase64String: function (roomId) {

                var livestream = self.videostreams.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.canvas.getCanvasDomElement();
                    if (canvas !== null) {
                        var dataUrl = canvas.toDataURL("image/jpeg", 0.5);
                        return dataUrl;
                    }
                }
            },
            stopVideo: function (roomId) {

                var livestream = self.videostreams.getlivestream(roomId);
                if (livestream !== undefined) {
                    var video = livestream.video.getVideoDomElement();
                    var stream = video.srcObject;
                    var tracks = stream.getTracks();
                    for (var i = 0; i < tracks.length; i++) {
                        var track = tracks[i];
                        track.stop();
                    }
                    video.srcObject = null;
                }
            },
            setImage: function (base64ImageString, roomId) {

                var livestream = self.videostreams.getlivestream(roomId);
                if (livestream !== undefined) {
                    var canvas = livestream.canvas_download.getCanvasDownloadDomElement();
                    if (canvas !== null) {
                        var ctx = canvas.getContext('2d');

                        var img = new Image();
                        img.onload = function () {
                            ctx.drawImage(img, 0, 0);
                        };

                        img.src = base64ImageString;
                    }
                }
            },
        };
    };

    window.initvideojs = function () {

        return storeObjectRef(new window.videojs());
    };

    var jsObjectRefs = {};
    var jsObjectRefId = 0;
    const jsRefKey = '_jsObjectRefId';
    function storeObjectRef(obj) {
        var id = jsObjectRefId++;
        jsObjectRefs[id] = obj;
        var jsRef = {};
        jsRef[jsRefKey] = id;
        return jsRef;
    };

});
