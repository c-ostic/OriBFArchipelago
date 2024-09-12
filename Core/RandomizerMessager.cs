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
        public static RandomizerMessager instance;

        public struct RandomizerMessage
        {
            public readonly string message;
            public readonly float duration;

            public RandomizerMessage(string message, float duration)
            {
                this.message = message;
                this.duration = duration;
            }
        }

        public bool IsActive { get; private set; }

        private readonly Queue<RandomizerMessage> messageQueue = new Queue<RandomizerMessage>();

        private BasicMessageProvider messageProvider;
        private float remainingDuration;

        private void Awake()
        {
            instance = this;
            IsActive = true;
        }

        private void Update()
        {
            if (remainingDuration > 0)
            {
                remainingDuration -= Time.deltaTime;
                if (remainingDuration <= 0)
                    ShowNext();
            }
        }

        private void ShowNext()
        {
            if (messageQueue.Count == 0)
                return;

            if (messageProvider == null)
                messageProvider = ScriptableObject.CreateInstance<BasicMessageProvider>();

            var nextMessage = messageQueue.Dequeue();

            messageProvider.SetMessage(nextMessage.message);
            UI.Hints.Show(messageProvider, HintLayer.Gameplay, nextMessage.duration);
            remainingDuration = nextMessage.duration;
        }

        public void AddMessage(string message, float duration = 3f)
        {
            if (!IsActive) return;

            messageQueue.Enqueue(new RandomizerMessage(message, duration));
            if (remainingDuration <= 0)
                ShowNext();
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
