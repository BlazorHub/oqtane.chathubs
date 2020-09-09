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

    window.videoElement = '';
    window.getVideoElement = function () {
        return videoElement;
    };
    window.setVideoElement = function (elementId) {
        videoElement = document.querySelector(elementId);
    };

    window.video = {

        constrains: {
            audio: true,
            video: {
                width: { min: 256, ideal: 1024, max: 1280 },
                height: { min: 144, ideal: 512, max: 720 }
            }
        },
        getUserMediaPermission: function (roomId) {

            navigator.mediaDevices.getUserMedia(this.constrains)
                .then(function (mediaStream) {

                    window.setVideoElement('#chathubs-video-' + roomId);
                    window.videoElement.srcObject = mediaStream;
                    window.videoElement.onloadedmetadata = function (e) {
                        window.videoElement.play();
                    };
                })
                .catch(function (ex) {
                    console.log(ex.message);
                });
        },
        captureAudio: function (roomId) {

        },
        drawImage: function (roomId) {

            const canvas = document.querySelector('#chathubs-canvas-' + roomId);
            const context = canvas.getContext('2d');
            const image = document.querySelector('#chathubs-video-' + roomId);

            context.drawImage(image, 0, 0);
        },
        getImageAsBase64String: async function (roomId , format = "image/jpeg") {

            var canvas = document.querySelector('#chathubs-canvas-' + roomId);
            var dataUrl = canvas.toDataURL(format);
            return dataUrl.split(',')[1];
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
            
            var canvas = document.querySelector('#chathubs-canvas-download-' + roomId);
            var context = canvas.getContext("2d");

            var image = new Image();
            image.onload = function () {
                context.drawImage(image, 0, 0);
            };
            var format = "data:image/png;base64";
            image.src = format + "," + base64ImageString;

            console.log(source);
        },
    };

});
