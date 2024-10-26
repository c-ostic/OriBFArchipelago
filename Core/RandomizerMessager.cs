using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Game;
using OriModding.BF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    public class RandomizerMessager : MonoBehaviour
    {
        // maximum number of messages before old messages are deleted so they don't flood the screen
        public const int MAX_MESSAGES = 20;
        // default total duration that messages last
        public const float DEFAULT_DURATION = 8f;
        // percentage of total duration when fade begins
        public const float FADE_PERCENT_THRESHOLD = 0.4f;

        public static RandomizerMessager instance;

        // controls whether messages are added to the queue
        public bool IsActive { get; private set; }
        // controls how long messages remain on the screen
        public float MessageDuration { get; private set; }

        public class RandomizerMessage
        {
            public string message;
            public float initDuration;
            public float currentDuration;

            public RandomizerMessage(string message, float duration)
            {
                this.message = message;
                this.initDuration = duration;
                this.currentDuration = duration;
            }

            // Represents the percentage of duration left
            public float GetDurationPercent()
            {
                return currentDuration / initDuration;
            }
        }

        private Queue<RandomizerMessage> messageQueue;

        private GUIStyle messageStyle;

        private void Awake()
        {
            instance = this;
            IsActive = true;
            MessageDuration = DEFAULT_DURATION;
            messageQueue = new Queue<RandomizerMessage>();

            messageStyle = new GUIStyle();
            messageStyle.fontSize = 20;
            messageStyle.wordWrap = true;
            messageStyle.fontStyle = FontStyle.Bold;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5, 5, Screen.width / 3, Screen.height));

            GUILayout.BeginVertical();

            foreach (RandomizerMessage message in messageQueue)
            {
                float opacity = 1;

                // only start to fade once the duration percent has passed the threshold
                if (message.GetDurationPercent() < FADE_PERCENT_THRESHOLD)
                {
                    opacity = message.GetDurationPercent() / FADE_PERCENT_THRESHOLD;
                }

                string opacityHex = ((int)(opacity * 255)).ToString("X2");

                // set default text opacity
                messageStyle.normal.textColor = new Color(1f, 1f, 1f, opacity);

                // replace any instance of ## with opacity for rich colored text
                string formattedMessage = message.message.Replace("##", opacityHex);

                GUILayout.Label(formattedMessage, messageStyle);
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        private void Update()
        {
            // dont want to clog up screen with messages, so remove oldest until under max
            while (messageQueue.Count > MAX_MESSAGES)
            {
                messageQueue.Dequeue();
            }

            // update the time on each message
            foreach (var message in messageQueue)
            {
                message.currentDuration -= Time.deltaTime;
            }

            // remove the first in the queue if it is out of time
            // since this is a queue, the first will always have the least time
            if (messageQueue.Count() > 0 && messageQueue.Peek().currentDuration < 0.0f)
            {
                messageQueue.Dequeue();
            }
        }

        public void AddMessage(string message)
        {
            if (IsActive)
            {
                messageQueue.Enqueue(new RandomizerMessage(message, MessageDuration));
            }
        }

        public void AddMessage(LogMessage message)
        {
            string result = "";

            foreach(MessagePart part in message.Parts)
            {
                // i'm unsure what messages has background color, but i'll check for it just to be safe
                if (!part.IsBackgroundColor)
                {
                    // add a color tag to the text
                    // the ## at the end is a placeholder for the opacity to be filled in at runtime
                    result += "<color=#" + ConvertColor(part.Color) + "##>";
                    result += part.Text;
                    result += "</color>";
                }
                else
                {
                    result += part.Text;
                }
            }

            AddMessage(result);
        }

        private string ConvertColor(Archipelago.MultiClient.Net.Models.Color color)
        {
            byte[] colorData = {color.R, color.G, color.B};
            return BitConverter.ToString(colorData).Replace("-", string.Empty);
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        public void SetMessageDuration(float messageDuration)
        {
            MessageDuration = messageDuration;
        }
    }
}
