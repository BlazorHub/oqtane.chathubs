window.addEventListener('DOMContentLoaded', () => {

    window.scroll = {

        scrollToBottom: function (elementId) {

            var messagewindow = document.querySelector(elementId);
            if (messagewindow !== null) {

                var lastchild = messagewindow.lastChild;
                var lastchildheight = lastchild.offsetHeight;
                var tolerance = 100;

                if (messagewindow.scrollTop + messagewindow.offsetHeight + lastchildheight + tolerance >= messagewindow.scrollHeight) {

                    messagewindow.scrollTo({
                        top: messagewindow.scrollHeight,
                        left: 0,
                        behavior: 'smooth'
                    });
                }
            }
        }
    };

    window.showchathubscontainer = function () {

        var chathubscontainer = document.querySelector('.chathubs-container');

        chathubscontainer.style.transition = "opacity 0.24s";
        chathubscontainer.style.opacity = "1";
    };

    window.cookies = {

        getCookie: function (cname) {
            var name = cname + "=";
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
        setCookie: function (cname, cvalue, expirationdays) {
            var d = new Date();
            d.setTime(d.getTime() + (expirationdays * 24 * 60 * 60 * 1000));
            var expires = "expires=" + d.toUTCString();
            document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
        },
    };

    window.__initjs = function (videoserviceobjectreference, draggablejsdotnetobj, resizejsdotnetobj) {

        __obj = {

            videoservice: videoserviceobjectreference,
            videolocalid: '#chathubs-video-local-',
            videoremoteid: '#chathubs-video-remote-',
            canvaslocalid: '#chathubs-canvas-local-',
            canvasremoteid: '#chathubs-canvas-remote-',
            videomimetypeobject: {

                get mimetype() {

                    var userAgent = navigator.userAgent.toLowerCase();

                    if (userAgent.indexOf('chrome') > -1 ||
                        userAgent.indexOf('firefox') > -1 ||
                        userAgent.indexOf('opera') > -1 ||
                        userAgent.indexOf('safari') > -1 ||
                        userAgent.indexOf('msie') > -1 ||
                        userAgent.indexOf('edge') > -1) {

                        return 'video/webm;codecs=opus,vp8';
                    }
                    else {

                        window.alert('Your current browser support is not implemented.');
                        return 'video/webm;codecs=opus,vp9';
                    }
                }
            },
            constrains: {
                audio: {
                    volume: { exact: 0.5 },
                },
                video: {
                    width: { min: 320, ideal: 320, max: 320 },
                    height: { min: 240, ideal: 240, max: 240 },
                    frameRate: { ideal: 12 },
                    facingMode: { ideal: "user" },
                }
            },
            livestreams: [],
            locallivestream: function (roomId, mediastream) {

                var __selflocallivestream = this;

                this.videolocalid = self.__obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.videolocalid);
                };

                this.canvaslocalid = self.__obj.canvaslocalid + roomId;
                this.getcanvaslocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.canvaslocalid);
                };

                this.vElement = this.getvideolocaldomelement();
                this.vElement.srcObject = mediastream;
                this.vElement.onloadedmetadata = function (e) {

                    __selflocallivestream.vElement.play();
                };
                this.vElement.autoplay = true;
                this.vElement.controls = true;
                this.vElement.muted = true;
                this.vElement.addEventListener("pause", function () {

                    __selflocallivestream.pauselivestreamtask();
                });

                this.options = { mimeType: __obj.videomimetypeobject.mimetype, videoBitsPerSecond: 1000, audioBitsPerSecond: 1000, ignoreMutedMedia: true };
                this.recorder = new MediaRecorder(mediastream, this.options);
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

                this.pauselivestreamtask = function () {

                    self.__obj.videoservice.invokeMethodAsync('PauseLivestreamTask', roomId);
                },
                this.continuelivestreamtask = function () {

                    self.__obj.videoservice.invokeMethodAsync('ContinueLivestreamTask', roomId);
                },
                this.cancel = function () {

                    try {
                        if (__selflocallivestream.recorder.state === 'recording' || __selflocallivestream.recorder.state === 'paused') {

                            __selflocallivestream.recorder.stop();
                        }
                        mediastream.getTracks().forEach(track => track.stop());
                    }
                    catch (err) {
                        console.error(err);
                    }
                };
            },
            remotelivestream: function (roomId) {

                var __selfremotelivestream = this;

                this.videoremoteid = self.__obj.videoremoteid + roomId;
                this.getvideoremotedomelement = function () {
                    return document.querySelector(__selfremotelivestream.videoremoteid);
                };

                this.canvasremoteid = self.__obj.canvasremoteid + roomId;
                this.getcanvasremotedomelement = function () {
                    return document.querySelector(__selfremotelivestream.canvasremoteid);
                };

                this.remotemediasequences = [];

                this.mediasource = new MediaSource();
                this.sourcebuffer = undefined;

                this.mediasource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(window.MediaSource.isTypeSupported(self.__obj.videomimetypeobject.mimetype))) {

                        console.error('Unsupported MIME type or codec: ', self.__obj.videomimetypeobject.mimetype);
                    }

                    __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediasource.addSourceBuffer(__obj.videomimetypeobject.mimetype);
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
                this.mediasource.addEventListener('sourceended', function (event) { console.log("on media source ended"); });
                this.mediasource.addEventListener('sourceclose', function (event) { console.log("on media source close"); });

                this.video = this.getvideoremotedomelement();
                this.video.controls = true;
                this.video.autoplay = true;
                this.video.preload = 'auto';
                this.video.muted = true;

                try {
                    this.video.srcObject = this.mediasource;
                } catch (ex) {
                    console.warn(ex);
                    this.video.src = URL.createObjectURL(this.mediasource);
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

                this.appendbuffer = async function (base64str) {

                    try {

                        console.log(base64str);
                        var blob = self.__obj.base64toblob(base64str);

                        var reader = new FileReader();
                        reader.onloadend = function (event) {

                            __selfremotelivestream.remotemediasequences.push(reader.result);

                            if (!__selfremotelivestream.sourcebuffer.updating && __selfremotelivestream.mediasource.readyState === 'open') {

                                var item = __selfremotelivestream.remotemediasequences.shift();
                                __selfremotelivestream.sourcebuffer.appendBuffer(new Uint8Array(item));
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

                navigator.mediaDevices.getUserMedia(self.__obj.constrains)
                    .then(function (mediastream) {

                        var livestream = new self.__obj.locallivestream(roomId, mediastream);
                        var livestreamdicitem = {
                            id: roomId,
                            item: livestream,
                        };

                        self.__obj.addlivestream(livestreamdicitem);
                    })
                    .catch(function (ex) {
                        console.warn(ex.message);
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

                            livestream.item.appendbuffer(dataURI);
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
            base64toblob: function (base64str) {

                var bytestring = atob(base64str.split('base64,')[1]);
                var arraybuffer = new ArrayBuffer(bytestring.length);

                var bytes = new Uint8Array(arraybuffer);
                for (var i = 0; i < bytestring.length; i++) {
                    bytes[i] = bytestring.charCodeAt(i);
                }

                var blob = new Blob([arraybuffer], { type: self.__obj.videomimetypeobject.mimetype });
                return blob;
            },

            draggableservice: draggablejsdotnetobj,
            initeventlisteners: function () {

                document.addEventListener('dragstart', function (event) {

                    event.dataTransfer.effectAllowed = "move";

                    var id = event.target.id;
                    var arr = id.split('-');
                    var dragstartindex = arr[arr.length - 1];

                    var exceptDropzone = '.dropzone-' + dragstartindex;
                    var dropzones = document.querySelectorAll('.dropzone:not(' + exceptDropzone + ')');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "block";
                    });

                    event.dataTransfer.setData("index", dragstartindex);
                });
                document.addEventListener('dragenter', function (event) {

                    event.target.classList.add('active-dropzone');
                });
                document.addEventListener('dragleave', function (event) {

                    event.target.classList.remove('active-dropzone');
                });
                document.addEventListener('dragover', function (event) {

                    event.preventDefault();
                    event.dataTransfer.dropEffect = 'move';
                });
                document.addEventListener('drop', function (event) {

                    event.preventDefault();

                    var dragindex = event.dataTransfer.getData('index');

                    var id = event.target.id;
                    var arr = id.split('-');
                    var dropindex = arr[arr.length - 1];

                    self.__obj.draggableservice.invokeMethodAsync('OnDrop', parseInt(dragindex), parseInt(dropindex));
                });
                document.addEventListener('dragend', function (event) {

                    var dropzones = document.getElementsByClassName('dropzone');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "none";
                        item.classList.remove('active-dropzone');
                    });
                });
            },

            resizeservice: resizejsdotnetobj,
            browserResize: {

                getInnerHeight: function () {

                    return window.innerHeight;
                },
                getInnerWidth: function () {

                    return window.innerWidth;
                },
                registerResizeCallback: function () {

                    var resizeTimer;
                    window.addEventListener('resize', function (e) {

                        clearTimeout(resizeTimer);
                        resizeTimer = setTimeout(() => {

                            self.__obj.browserResize.resized();
                        }, 200);
                    });
                },
                resized: function () {

                    self.__obj.resizeservice.invokeMethodAsync('OnBrowserResize');
                },
            },
        };
    };

    window.__init = function (videojsdotnetobj, draggablejsdotnetobj, resizejsdotnetobj) {

        return storeObjectRef(new window.__initjs(videojsdotnetobj, draggablejsdotnetobj, resizejsdotnetobj));
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
