$(document).ready(function () {

    window.scroll = {

        scrollToBottom: function (elementId, animationTime) {

            var $messageWindow = $(elementId);
            var $lastChild = $messageWindow.children().last();
            var $lastChildHeight = Math.ceil($lastChild.height());
            var tolerance = 30;

            if (Math.ceil($messageWindow.scrollTop() + $messageWindow.innerHeight()) + $lastChildHeight + tolerance >= $messageWindow.prop("scrollHeight")) {

                //$messageWindow.stop(true, false);
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

            video: function () {

                this.videoElement = '';
                this.getVideoElement = function () {
                    return this.videoElement;
                };
                this.setVideoElement = function (elementId) {
                    this.videoElement = elementId;
                };
            },
            canvas: function () {

                this.canvasElement = '';
                this.getCanvasElement = function () {
                    return this.canvasElement;
                };
                this.setCanvasElement = function (elementId) {
                    this.canvasElement = elementId;
                };
            },
            canvasDownload: function () {

                this.canvasDownloadElement = '';
                this.getCanvasDownloadElement = function () {
                    return this.canvasDownloadElement;
                };
                this.setCanvasDownloadElement = function (elementId) {
                    this.canvasDownloadElement = elementId;
                };
            },
            livestreams: [],
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
            getUserMediaPermission: function (roomId) {

                console.log("get user media permission..");

                navigator.mediaDevices.getUserMedia(this.constrains)
                    .then(function (mediaStream) {

                        var vInstance = new self.videostreams.video();
                        vInstance.setVideoElement('#chathubs-video-' + roomId);
                        var vElement = document.querySelector(vInstance.getVideoElement());
                        vElement.srcObject = mediaStream;
                        vElement.onloadedmetadata = function (e) {
                            vElement.play();
                        };

                        var cInstance = new self.videostreams.canvas();
                        cInstance.setCanvasElement('#chathubs-canvas-' + roomId);

                        var cdInstance = new self.videostreams.canvasDownload();
                        cdInstance.setCanvasDownloadElement('#chathubs-canvas-download-' + roomId);

                        var dicItem = {
                            id: roomId,
                            video: vInstance,
                            canvas: cInstance,
                            canvas_download: cdInstance
                        };

                        self.videostreams.livestreams.push(dicItem);
                    })
                    .catch(function (ex) {
                        console.log(ex.message);
                    });
            },
            captureAudio: function (roomId) {

            },
            drawImage: function (roomId) {

                var livestream = self.videostreams.getlivestream(roomId);

                var canvas = document.querySelector(livestream.canvas.getCanvasElement());
                var context = canvas.getContext('2d');
                var image = document.querySelector(livestream.video.getVideoElement());

                context.drawImage(image, 0, 0);
            },
            getImageAsBase64String: function (roomId) {

                var livestream = self.videostreams.getlivestream(roomId);
                var canvas = document.querySelector(livestream.canvas.getCanvasElement());
                var dataUrl = canvas.toDataURL("image/jpeg", 0.5);
                return dataUrl;
            },
            stopVideo: function () {

                var stream = video.srcObject;
                var tracks = stream.getTracks();

                for (var i = 0; i < tracks.length; i++) {
                    var track = tracks[i];
                    track.stop();
                }
                video.srcObject = null;
            },
            setImage: function (base64ImageString, roomId) {

                var livestream = self.videostreams.getlivestream(roomId);

                var canvas = document.querySelector(livestream.canvas_download.getCanvasDownloadElement());
                var ctx = canvas.getContext('2d');

                var img = new Image();
                img.onload = function () {
                    ctx.drawImage(img, 0, 0);
                };

                img.src = base64ImageString;
            },
        };
    };

    window.initvideojs = function () {

        console.log("store object ref..");
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
    }  

});
