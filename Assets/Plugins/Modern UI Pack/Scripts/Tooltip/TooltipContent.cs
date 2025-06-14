﻿using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


    public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("CONTENT")]
        [TextArea] public string description;

        [Header("RESOURCES")]
        public GameObject tooltipRect;
        public TextMeshProUGUI descriptionText;

        TooltipManager tpManager;
        [HideInInspector] public Animator tooltipAnimator;

        void Start()
        {
            if (tooltipRect == null || descriptionText == null)
            {
                try
                {
                    tooltipRect = GameObject.Find("Tooltip Rect");
                    descriptionText = tooltipRect.transform.GetComponentInChildren<TextMeshProUGUI>();
                }

                catch
                {
                    Debug.LogError("No Tooltip object assigned.", this);
                }
            }

            if (tooltipRect != null)
            {
                tpManager = tooltipRect.GetComponentInParent<TooltipManager>();
                tooltipAnimator = tooltipRect.GetComponentInParent<Animator>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipRect != null)
            {
                descriptionText.text = description;
                tpManager.allowUpdating = true;
                tooltipAnimator.Play("In");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipRect != null)
            {
                tooltipAnimator.Play("Out");
                tpManager.allowUpdating = false;
            }
        }
    }
