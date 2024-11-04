using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Game;
using OriModding.BF.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    public class RandomizerMessager : MonoBehaviour
    {
        // maximum number of messages before old messages are deleted so they don't flood the screen
        public const int MAX_MESSAGES = 20;
        // message duration constants
        public const float DEFAULT_DURATION = 6f;
        public const float MIN_DURATION = 2f;
        public const float MAX_DURATION = 10f;
        // amount of duration time left when fade should begin (should not be larger than min duration
        public const float FADE_THRESHOLD = 2f;

        // used as a basis to dynamically change the font size
        public Vector2 baseScreenSize = new Vector2(1920, 1080);

        public static RandomizerMessager instance;

        // controls whether messages are added to the queue
        public bool IsActive { get; private set; }

        // controls how long messages remain on the screen
        public float MessageDuration { get; private set; }

        // controls when to show message options
        public bool IsPaused { get; set; }

        // controls whether to show all or only some messages
        public bool IsVerbose { get; private set; }

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
        }

        private Queue<RandomizerMessage> messageQueue;

        private GUIStyle messageStyle, optionsStyle;

        private void Awake()
        {
            instance = this;
            IsActive = true;
            MessageDuration = DEFAULT_DURATION;
            IsPaused = false;
            IsVerbose = true;
            messageQueue = new Queue<RandomizerMessage>();

            messageStyle = new GUIStyle();
            messageStyle.wordWrap = true;
            messageStyle.fontStyle = FontStyle.Bold;

            optionsStyle = new GUIStyle();
            optionsStyle.wordWrap = false;
            optionsStyle.fontStyle = FontStyle.Bold;
            optionsStyle.normal.textColor = new Color(1f, 1f, 1f);
        }

        private void OnGUI()
        {
            int optionsOffset = 0;

            messageStyle.fontSize = (int)(25 * (Screen.width / baseScreenSize.x));
            optionsStyle.fontSize = (int)(25 * (Screen.width / baseScreenSize.x));

            // Show options
            if (IsPaused)
            {
                optionsOffset = 100;
                GUILayout.BeginArea(new Rect(5, 5, Screen.width / 3, 100));

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Message Duration: ", optionsStyle, GUILayout.ExpandWidth(false));
                MessageDuration = GUILayout.HorizontalSlider(MessageDuration, MIN_DURATION, MAX_DURATION);
                GUILayout.Label((int)MessageDuration + "", optionsStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Show all messages: ", optionsStyle, GUILayout.ExpandWidth(false));
                IsVerbose = GUILayout.Toggle(IsVerbose, "");
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }

            // Show messages
            GUILayout.BeginArea(new Rect(5, 5 + optionsOffset, Screen.width / 3, Screen.height));

            GUILayout.BeginVertical();

            foreach (RandomizerMessage message in messageQueue)
            {
                float opacity = 1;

                // only start to fade once the duration has passed the threshold
                if (message.currentDuration < FADE_THRESHOLD)
                {
                    opacity = message.currentDuration / FADE_THRESHOLD;
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

            bool showMessage = false;

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

                // always show the message if in verbose mode
                // or, if not verbose, check for a player message part with the active player
                if (IsVerbose ||
                    part is PlayerMessagePart && ((PlayerMessagePart)part).IsActivePlayer)
                {
                    showMessage = true;
                }
            }

            if (showMessage)
            {
                AddMessage(result);
            }
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
    }
}
