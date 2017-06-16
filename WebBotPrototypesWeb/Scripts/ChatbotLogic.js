
// SDK USAGE 
//<script>

// On doument load resolve the SDK dependecy
function Initialize(onComplete) {
    require(["Speech.Browser.Sdk"], function (SDK) {
        onComplete(SDK);
    });
}

// Setup the recongizer
function RecognizerSetup(SDK, recognitionMode, language, format, subscriptionKey) {
    var recognizerConfig = new SDK.RecognizerConfig(
        new SDK.SpeechConfig(
            new SDK.Context(
                new SDK.OS(navigator.userAgent, "Browser", null),
                new SDK.Device("SpeechSample", "SpeechSample", "1.0.00000"))),
        recognitionMode, // SDK.RecognitionMode.Interactive  (Options - Interactive/Conversation/Dictation>)
        language, // Supported laguages are specific to each recognition mode. Refer to docs.
        format); // SDK.SpeechResultFormat.Simple (Options - Simple/Detailed)

    // Alternatively use SDK.CognitiveTokenAuthentication(fetchCallback, fetchOnExpiryCallback) for token auth
    var authentication = new SDK.CognitiveSubscriptionKeyAuthentication(subscriptionKey);

    return SDK.CreateRecognizer(recognizerConfig, authentication);
}

// Start the recognition
function RecognizerStart(SDK, recognizer) {
    recognizer.Recognize((event) => {
        /*
         Alternative syntax for typescript devs.
         if (event instanceof SDK.RecognitionTriggeredEvent)
        */
        switch (event.Name) {
            case "RecognitionTriggeredEvent":
                UpdateStatus("Initializing");
                break;

            case "ListeningStartedEvent":
                UpdateStatus("Listening");
                break;

            case "RecognitionStartedEvent":
                UpdateStatus("Listening_Recognizing");
                break;

            case "SpeechStartDetectedEvent":
                UpdateStatus("Listening_DetectedSpeech_Recognizing");
                console.log(JSON.stringify(event.Result)); // check console for other information in result
                break;

            case "SpeechHypothesisEvent":
                UpdateRecognizedHypothesis(event.Result.Text);
                console.log(JSON.stringify(event.Result)); // check console for other information in result
                break;

            case "SpeechEndDetectedEvent":
                OnSpeechEndDetected();
                UpdateStatus("Processing_Adding_Final_Touches");
                console.log(JSON.stringify(event.Result)); // check console for other information in result
                break;

            case "SpeechSimplePhraseEvent":
                UpdateRecognizedPhrase(JSON.stringify(event.Result, null, 3));
                //SendChat(event.Result);
                break;

            case "SpeechDetailedPhraseEvent":
                UpdateRecognizedPhrase(JSON.stringify(event.Result, null, 3));
                break;

            case "RecognitionEndedEvent":
                OnComplete();
                UpdateStatus("Idle");
                SendChat();
                console.log(JSON.stringify(event)); // Debug information
                break;
        }
    })
        .On(() => {
            // The request succeeded. Nothing to do here.
        },
        (error) => {
            console.error(error);
        });
}

// Stop the Recognition.
function RecognizerStop(SDK, recognizer) {
    // recognizer.AudioSource.Detach(audioNodeId) can be also used here. (audioNodeId is part of ListeningStartedEvent)
    recognizer.AudioSource.TurnOff();
}

// </script>
// <!--Browser Hooks -- >
// <script>

var startBtn, stopBtn, hypothesisDiv, phraseDiv, statusDiv, key, languageOptions, formatOptions;
var SDK;
var recognizer;
var previousSubscriptionKey;

document.addEventListener("DOMContentLoaded", function () {
    createBtn = document.getElementById("createBtn");
    startBtn = document.getElementById("startBtn");
    stopBtn = document.getElementById("stopBtn");
    phraseDiv = document.getElementById("phraseDiv");
    hypothesisDiv = document.getElementById("hypothesisDiv");
    statusDiv = document.getElementById("statusDiv");
    key = document.getElementById("key");
    languageOptions = document.getElementById("languageOptions");
    formatOptions = document.getElementById("formatOptions");

    languageOptions.addEventListener("change", function () {
        Setup();
    });

    formatOptions.addEventListener("change", function () {
        Setup();
    });

    startBtn.addEventListener("click", function () {
        if (!recognizer || previousSubscriptionKey != key.value) {
            previousSubscriptionKey = key.value;
            Setup();
        }

        hypothesisDiv.innerHTML = "";
        phraseDiv.innerHTML = "";
        RecognizerStart(SDK, recognizer);
        startBtn.disabled = true;
        stopBtn.disabled = false;
    });

    stopBtn.addEventListener("click", function () {
        RecognizerStop(SDK);
        startBtn.disabled = false;
        stopBtn.disabled = true;
    });

    Initialize(function (speechSdk) {
        SDK = speechSdk;
        startBtn.disabled = false;
    });
});

function Setup() {
    recognizer = RecognizerSetup(SDK, SDK.RecognitionMode.Interactive, languageOptions.value, SDK.SpeechResultFormat[formatOptions.value], key.value);
}

function UpdateStatus(status) {
    statusDiv.innerHTML = status;
}

function UpdateRecognizedHypothesis(text) {
    hypothesisDiv.innerHTML = text;
}

function OnSpeechEndDetected() {
    stopBtn.disabled = true;
}

function UpdateRecognizedPhrase(json) {
    phraseDiv.innerHTML = json;
}

function SendChat() {

    var data = {
        text: hypothesisDiv.innerHTML
    };

    if (data.text == "" || data.text.length == 0) {
        UpdateMyStatus("No text detected.");
        return;
    }

    $.ajax(
        {
            url: "/VoiceChat/SendChat",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(data),
            success: function (mydata) {
                UpdateMyStatus("SendChat successful");

                HandleChatResult(mydata);
            },
            error: function (data, error) {
                ShowError(data.statusText);
            }
        });
    UpdateMyStatus("SendChat started.");
}

var textparts = new Array();
var maxParts = 0;
var currentTextPart = 0;

function HandleChatResult(chatResult) {
    chatResponse.innerHTML = chatResult.ChatResponse;

    var parts = chatResult.ChatResponseParts;

    maxParts = parts.length;
    currentTextPart = 0;
    currentUploadedTextPart = 0;

    for (var i = parts.length - 1; i >= 0; i--)
    {
        var t = parts[i];
        //alert(t);
        textparts.push(t);
    }

    //while (textparts.length > 0) {
    //    var t = textparts.pop();
    //    alert(t);
    //}
    HandleNextTextPart();
}

var currentUploadedTextPart = 0;

function HandleNextTextPart() {

    if (textparts.length == 0)
    {
        return;
    }

    var myText = textparts.pop();

    var data = {
        text: myText,
        no: currentUploadedTextPart
    };

    currentUploadedTextPart++;

    if (data.text == null || data.text.length == 0) {
        UpdateMyStatus("HandleChatResult kein text.");
        return;
    }

    $.ajax(
        {
            url: "/VoiceChat/GetTextToSpeech",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(data),
            success: function (mydata) {
                UpdateMyStatus("GetTextToSpeech successful");
                if (mydata.ErrorMessage) {
                    ShowError(mydata.ErrorMessage);
                }
                addSpeechToQueue(mydata);
                //playWave(mydata.SpeechEventData);
                HandleNextTextPart();
            },
            error: function (data, error) {
                ShowError(data.statusText);
            }
        });
}

var speechQueue = new Array();

function addSpeechToQueue(mydata) {

    if (mydata == null)
    {
        return;
    }

    speechQueue.push(mydata);
    playTextPart();

    console.log("addSpeechToQueue: " + speechQueue.length + " Queuesize " + "/" + mydata.TextPartNo + " TextPartNo");
}

function playTextPart() {

    if (currentTextPart >= maxParts) {
        console.log("playTextPart: finished");
        console.log("playTextPart --> clear queue");
        speechQueue = new Array();
        maxParts = 0;
        currentTextPart = 0;
        return;
    } else {
        console.log("playTextPart: play " + currentTextPart + " /  " + maxParts + " Parts");
    }

    // try to find next part if already downloaded. 
    for (var i = 0; i < maxParts; i++) {
        if (maxParts > speechQueue.length)
            break;

        var p = speechQueue[i];

        if (p.TextPartNo == currentTextPart) {
            console.log("TextPartNo " + p.TextPartNo + " found");
            soundplayByteArray(p.SpeechEventData);
            break;
        }
    }
}

var myAudioContext = null;

function soundinit() {
    if (myAudioContext != null) {
        return;
    }

    if (!window.AudioContext) {
        if (!window.webkitAudioContext) {
            alert("Your browser does not support any AudioContext and cannot play back this audio.");
            return;
        }
        window.AudioContext = window.webkitAudioContext;
    }
    myAudioContext = new AudioContext();
    //myAudioContext.onstatechange = function () {
    //    console.log("myAudioContext.state: " + myAudioContext.state);
    //};
}



function soundplayByteArray(byteArray) {
    soundinit();

    var arrayBuffer = new ArrayBuffer(byteArray.length);
    var bufferView = new Uint8Array(arrayBuffer);
    for (i = 0; i < byteArray.length; i++) {
        bufferView[i] = byteArray[i];
    }

    myAudioContext.decodeAudioData(arrayBuffer, function (buffer) {
        buf = buffer;
        soundplay();
    });
}

// Play the loaded file
function soundplay() {
    // Create a source node from the buffer
    var source = myAudioContext.createBufferSource();
    source.buffer = buf;

    // Connect to the final output node (the speakers)
    source.connect(myAudioContext.destination);

    source.onended = function () {
        console.log("buffersource.onended");

        currentTextPart++;
        playTextPart();
    };

    // Play immediately
    source.start(0);
}

function OnComplete() {
    startBtn.disabled = false;
    stopBtn.disabled = true;
}

function ShowError(errorMessageText) {
    //alert("New error: " + errorMessage);
    errorMessageSpan.innerHTML = errorMessageText;
    errorDiv.style.visibility = "visible";
}

function UpdateMyStatus(statusMessage) {
    //alert("Status: " + statusMessage);
    myStatusDiv.innerHTML = statusMessage;
}
