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
                this.id = roomId;

                this.mediaStream = mediaStream;

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

                this.ctx = this.getcanvaslocaldomelement().getContext('2d');
                this.drawimage = function () {

                    __selflocallivestream.ctx.drawImage(__selflocallivestream.getvideolocaldomelement(), 0, 0, 120, 90);
                    var snapshot = __selflocallivestream.getcanvaslocaldomelement().toDataURL(self.__obj.videoMimeTypeObject.mimeType);
                    __selflocallivestream.broadcastsnapshot(snapshot, 'image');
                };

                this.broadcastsnapshot = function (dataURI, type) {

                    DotNet.invokeMethodAsync("Oqtane.ChatHubs.Client.Oqtane", 'OnDataAvailable', dataURI, roomId, type).then(obj => {
                        console.log(obj.msg.substring(0, 100));
                    });
                };

                this.audiocontext = new AudioContext();
                this.mediasource = this.audiocontext.createMediaStreamSource(__selflocallivestream.mediaStream);
                this.scriptprocessor = this.audiocontext.createScriptProcessor(256, 1, 1);
                this.mediasource.connect(this.scriptprocessor);
                this.scriptprocessor.connect(this.audiocontext.destination);

                this.counter = 0;
                this.scriptprocessor.onaudioprocess = function (audioProcessingEvent) {

                    __selflocallivestream.counter++;

                    if (__selflocallivestream.counter % 100 === 0) {

                        var inputBuffer = audioProcessingEvent.inputBuffer;
                        var outputBuffer = audioProcessingEvent.outputBuffer;

                        var inputData;
                        var outputData;
                        for (var channel = 0; channel < outputBuffer.numberOfChannels; channel++) {

                            inputData = inputBuffer.getChannelData(channel);
                            outputData = outputBuffer.getChannelData(channel);
                        }

                        var blob = new Blob([inputData]);
                        var reader = new FileReader();
                        reader.onload = function (event) {
                            var base64str = event.target.result;
                            __selflocallivestream.broadcastsnapshot(base64str, 'audio');
                        };
                        reader.readAsDataURL(blob);
                    }
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

                this.drawImage = function (base64str) {

                    var canvas = __selfremotelivestream.getcanvasremotedomelement();
                    var ctx = canvas.getContext("2d");

                    var image = new Image();
                    image.onload = function () {
                        ctx.drawImage(image, 0, 0);
                    };
                    image.src = base64str;
                };

                this.playaudio = async function (base64str) {

                    var audio_context = new (window.AudioContext || window.webkitAudioContext)();
                    var gainNode = audio_context.createGain();
                    gainNode.gain.value = 0.5;

                    var buffer_source = audio_context.createBufferSource();
                    var arraybuffer = audio_context.createBuffer(1, audio_context.sampleRate * 2.0, audio_context.sampleRate)
                    var floatArray = __selfremotelivestream.base64strtofloat32array(base64str);
                    arraybuffer.copyFromChannel(floatArray, 0, 0);
                    buffer_source.buffer = arraybuffer;
                    buffer_source.connect(audio_context.destination);

                    buffer_source.connect(gainNode);
                    gainNode.connect(audio_context.destination);

                    setTimeout(function () {

                        buffer_source.start();
                        console.log('buffer source started');
                    }, 1000);
                };

                this.base64strtofloat32array = function (base64str) {

                    var blob = window.atob(base64str.split('base64,')[1]);
                    var floatArrayLength = blob.length / Float32Array.BYTES_PER_ELEMENT;
                    var dataView = new DataView(new ArrayBuffer(Float32Array.BYTES_PER_ELEMENT));
                    var floatArray = new Float32Array(floatArrayLength);
                    var j = 0;

                    for (var i = 0; i < floatArrayLength; i++) {
                        j = i * 4;
                        dataView.setUint8(0, blob.charCodeAt(j));
                        dataView.setUint8(1, blob.charCodeAt(j + 1));
                        dataView.setUint8(2, blob.charCodeAt(j + 2));
                        dataView.setUint8(3, blob.charCodeAt(j + 3));
                        floatArray[i] = dataView.getFloat32(0, true);
                    }

                    return floatArray;
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
            drawimage: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item.mediaStream !== undefined) {

                        livestream.item.drawimage();
                    }
                }
            },
            appendbuffer: function (base64str, roomId, type) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item.mediaStream === undefined) {

                        if (type === 'image') {

                            livestream.item.drawImage(base64str);
                        }
                        else if (type === 'audio') {

                            livestream.item.playaudio(base64str);
                        }
                    }
                }
            },
            closelivestream: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item.mediaStream !== undefined) {

                        livestream.item.recyclebin();
                        return true;
                    }
                    else {

                        livestream.item.recyclebin();
                        return false;
                    }
                }

                return false;
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
