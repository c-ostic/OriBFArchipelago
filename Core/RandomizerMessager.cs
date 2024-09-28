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
        public static int MAX_MESSAGES = 20;

        public static RandomizerMessager instance;

        public bool IsActive { get; private set; }

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

            foreach (var message in messageQueue)
            {
                messageStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, message.GetDurationPercent());
                GUILayout.Label(message.message, messageStyle);
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

        public void AddMessage(string message, float duration = 5f)
        {
            if (IsActive)
            {
                messageQueue.Enqueue(new RandomizerMessage(message, duration));
            }
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
