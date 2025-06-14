﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class NotificationStacking : MonoBehaviour
    {
        [HideInInspector] public List<NotificationManager> notifications = new List<NotificationManager>();
        [HideInInspector] public bool enableUpdating = false;

        [Header("SETTINGS")]
        public float delay = 1;
        int currentNotification = 0;

        void Update()
        {
            if (enableUpdating == true)
            {
                try
                {
                    notifications[currentNotification].gameObject.SetActive(true);

                    if (notifications[currentNotification].notificationAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait"))
                    {
                        notifications[currentNotification].OpenNotification();
                        StartCoroutine("StartNotification");
                    }

                    if (currentNotification >= notifications.Count)
                    {
                        enableUpdating = false;
                        currentNotification = 0;
                    }
                }

                catch
                {
                    enableUpdating = false;
                    currentNotification = 0;
                }
            }
        }

        IEnumerator StartNotification()
        {
            yield return new WaitForSeconds(notifications[currentNotification].timer + delay);
            Destroy(notifications[currentNotification].gameObject);
            currentNotification += 1;
            StopCoroutine("StartNotification");
        }
    }
