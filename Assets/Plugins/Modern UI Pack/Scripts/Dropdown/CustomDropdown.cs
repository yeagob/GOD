﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;


    public class CustomDropdown : MonoBehaviour, IPointerExitHandler
    {
        // Resources
        public GameObject triggerObject;
        public TextMeshProUGUI selectedText;
        public Image selectedImage;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        private VerticalLayoutGroup itemList;
        private Transform currentListParent;
        public Transform listParent;

        // Settings
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool setHighPriorty = true;
        public bool outOnPointerExit = false;
        public bool isListItem = false;
        public bool invokeAtStart = false;
        public AnimationType animationType;
        public int selectedItemIndex = 0;

        // Saving
        public bool saveSelected = false;
        public string dropdownTag = "Dropdown";

        // Item list
        [SerializeField]
        public List<Item> dropdownItems = new List<Item>();
        [System.Serializable]
        public class DropdownEvent : UnityEvent<int> { }
        [Space(8)] public DropdownEvent dropdownEvent;

        // Hidden variables
        [HideInInspector] public Animator dropdownAnimator;
        [HideInInspector] public bool isOn;
        [HideInInspector] public int index = 0;
        [HideInInspector] public int siblingIndex = 0;
        TextMeshProUGUI setItemText;
        Image setItemImage;
        Sprite imageHelper;
        string textHelper;
        string newItemTitle;
        Sprite newItemIcon;

        public enum AnimationType
        {
            FADING,
            SLIDING,
            STYLISH
        }

        [System.Serializable]
        public class Item
        {
            public string itemName = "Dropdown Item";
            public Sprite itemIcon;
            public UnityEvent OnItemSelection;
        }

        void Start()
        {
            try
            {
                dropdownAnimator = gameObject.GetComponent<Animator>();
                itemList = itemParent.GetComponent<VerticalLayoutGroup>();
                SetupDropdown();
                currentListParent = transform.parent;
            }

            catch
            {
                Debug.LogError("Dropdown - Cannot initalize the object due to missing resources.", this);
            }

            if (enableScrollbar == true)
                itemList.padding.right = 25;

            else
                itemList.padding.right = 8;

            if (setHighPriorty == true)
                transform.SetAsLastSibling();

            if (saveSelected == true)
            {
                if (invokeAtStart == true)
                    dropdownItems[PlayerPrefs.GetInt(dropdownTag + "Dropdown")].OnItemSelection.Invoke();
                else
                    ChangeDropdownInfo(PlayerPrefs.GetInt(dropdownTag + "Dropdown"));
            }
        }

        public void SetupDropdown()
        {
            foreach (Transform child in itemParent)
                GameObject.Destroy(child.gameObject);

            index = 0;
            for (int i = 0; i < dropdownItems.Count; ++i)
            {
                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = dropdownItems[i].itemName;
                setItemText.text = textHelper;

                Transform goImage;
                goImage = go.gameObject.transform.Find("Icon");
                setItemImage = goImage.GetComponent<Image>();
                imageHelper = dropdownItems[i].itemIcon;
                setItemImage.sprite = imageHelper;

                Button itemButton;
                itemButton = go.GetComponent<Button>();

                itemButton.onClick.AddListener(Animate);
                itemButton.onClick.AddListener(delegate
                {
                    ChangeDropdownInfo(index = go.transform.GetSiblingIndex());
                    dropdownEvent.Invoke(index = go.transform.GetSiblingIndex());

                    if (saveSelected == true)
                        PlayerPrefs.SetInt(dropdownTag + "Dropdown", go.transform.GetSiblingIndex());
                });

                if (dropdownItems[i].OnItemSelection != null)
                    itemButton.onClick.AddListener(dropdownItems[i].OnItemSelection.Invoke);

                if (invokeAtStart == true)
                    dropdownItems[i].OnItemSelection.Invoke();
            }

            selectedText.text = dropdownItems[selectedItemIndex].itemName;
            selectedImage.sprite = dropdownItems[selectedItemIndex].itemIcon;
            currentListParent = transform.parent;
        }

        public void ChangeDropdownInfo(int itemIndex)
        {
            if (selectedImage != null)
                selectedImage.sprite = dropdownItems[itemIndex].itemIcon;

            if (selectedText != null)
                selectedText.text = dropdownItems[itemIndex].itemName;

            selectedItemIndex = itemIndex;
        }

        public void Animate()
        {
            if (isOn == false && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            else if (isOn == false && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;

                if (isListItem == true)
                {
                    siblingIndex = transform.GetSiblingIndex();
                    gameObject.transform.SetParent(listParent, true);
                }
            }

            else if (isOn == true && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;

                if (isListItem == true)
                {
                    gameObject.transform.SetParent(currentListParent, true);
                    gameObject.transform.SetSiblingIndex(siblingIndex);
                }
            }

            if (enableTrigger == true && isOn == false)
                triggerObject.SetActive(false);

            else if (enableTrigger == true && isOn == true)
                triggerObject.SetActive(true);

            if (outOnPointerExit == true)
                triggerObject.SetActive(false);

            if (setHighPriorty == true)
                transform.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (outOnPointerExit == true)
            {
                if (isOn == true)
                {
                    Animate();
                    isOn = false;
                }

                if (isListItem == true)
                    gameObject.transform.SetParent(currentListParent, true);
            }
        }

        public void UpdateValues()
        {
            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                scrollbar.SetActive(false);
            }

            if (enableIcon == false)
                selectedImage.gameObject.SetActive(false);
            else
                selectedImage.gameObject.SetActive(true);
        }

        public void CreateNewItem()
        {
            Item item = new Item();
            item.itemName = newItemTitle;
            item.itemIcon = newItemIcon;
            dropdownItems.Add(item);
            SetupDropdown();
        }

        public void SetItemTitle(string title)
        {
            newItemTitle = title;
        }

        public void SetItemIcon(Sprite icon)
        {
            newItemIcon = icon;
        }

        public void AddNewItem()
        {
            Item item = new Item();
            dropdownItems.Add(item);
        }
    }
