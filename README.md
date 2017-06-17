# web-bots-with-voice

A framework to build webbased bots with voice recognition.

## Description

This project was developed together with a customer, who wanted to have a Question and Answer-Bot with Voice Recognition and Speech Output.

## Requirements

* Azure Subscription needed.

### Bing Speech API Key needed.

[Free Version](https://www.microsoft.com/cognitive-services/en-us/subscriptions?productId=/products/Bing.Speech.Preview)

[Paid Version](https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/Bing.Speech/pricingtier/S0)

## Setup

To use this project, you need to do the following steps.

1. Clone the project
1. Open Solution WebBotPrototypesWeb
1. Open web.config in root directory
1. Optional: Change SQL Azure DB Connection string if you don't want to use local database
1. Add your Bing Speech API Key- You can create one here: [Bing Speech API Key](http://wwww.asdasd.asd)
1. Create your own QnA-Maker on [https://qnamaker.ai](https://qnamaker.ai)
1. Create a Bot Service in the Azure Portal and select the QnABot-Template, and the connect it to the QnA-Maker Project. Add the name of the Bot the AppSettings-Value *BotId*
1. In the Azure Bot Service, edit the WebChat-Channel and copy the Key and paste it to the AppSetting-Value *BotKey*.
1. Add a new channel of type 'DirectLine' and copy the generated key to the AppSettings-Value *DirectLineKey*
1. Run the Application and navigate to the VoiceChat

## Content in this project

This project contains several implementations of chats. 

### WebChat

The WebChat-Control demonstrates the use of the standard WebChat-Control. You can easily integreate this in you existing Website using the Name of the Bot and the Secret.

```HTML
@{
    ViewBag.Title = "Web Chat ";
    var botId = System.Configuration.ConfigurationManager.AppSettings["BotId"];
    var botKey = System.Configuration.ConfigurationManager.AppSettings["BotKey"];
    var url = $"https://webchat.botframework.com/embed/{botId}?s={botKey}";
}

<h2>Web Chat</h2>

<iframe style="position: relative; height:400px; width:100%" src='@url'></iframe>

```

### Directline

Directline enables you easily to access the bot logic without any standard-Controls. It is the best way to integrate a bot into your own application without Skype or other chat clients

### Voice Recognition

This page is just a demonstration and re-use of an official Azure Sample to communicate with the Bing Speech Api. The link to the samples is listed in the Credits sections.

## Application-Flow for Voice Chat

1.A webpage (VoiceChat) handles the complete process of Recognizing speech from the microphone in a browser (HTML5 only, Firefox, Chrome, Edge) using the Bing Speech API
1. Send the recognized text to the Azure bot service
1. Bot service is connected to the QnAMaker - Project for the BASF Knowledge Base, which is maintained by BASF and me.
1. Handle the answer of the QnA-Service friendly (if there is no answer, be friendly and say "sorry, I didn't find what you mean. Please try again"). 
1. Take the answer, split it into a set of phrases, because the answers can be to long as one phrase for the API to be convert text to speech with the Bing Speech API again.This task is done parallel, so that the results of each phrase can be return in a different sequence. 
1. Receiving the set of phrases in the browser, and playing the "soundfiles" in the correct order. 

## Credits

I used the following websites and scripts to implement the bot:

* [Azure Speech To Text Sample Code](https://github.com/Azure-Samples/SpeechToText-WebSockets-Javascript/blob/master/samples/browser/Sample.html)
* [Azure Speech To Text Sample Code Live Sample](https://htmlpreview.github.io/?https://github.com/Azure-Samples/SpeechToText-WebSockets-Javascript/blob/master/samples/browser/Sample.html)

## Open To Do's

1. Updating Documentation
1. Visualization of personalized communication, which is stored in database
1. Creating a better view of the VoiceChat 


