using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;


    public class ModalWindowManager : MonoBehaviour
    {
        // Resources
        public Image windowIcon;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;
        public Button confirmButton;
        public Button cancelButton;

        // Content
        public Sprite icon;
        public string titleText = "Title";
        [TextArea] public string descriptionText = "Description here";

        // Events
        public UnityEvent onConfirm;
        public UnityEvent onCancel;

        // Settings
        public bool sharpAnimations = false;
        public bool useCustomValues = false;

        Animator mwAnimator;
        public bool isOn = false;

		//private PlayFabAuth playFabAuth;

        void Start()
        {
			//playFabAuth = FindObjectOfType<PlayFabAuth>();


			try
            {
                mwAnimator = gameObject.GetComponent<Animator>();
            }

            catch
            {
                Debug.LogError("Modal Window - Cannot initalize the window due to missing 'Animator' variable.", this);
            }

            if (confirmButton != null)
                confirmButton.onClick.AddListener(onConfirm.Invoke);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(onCancel.Invoke);

            if (useCustomValues == false)
                UpdateUI();
        }

        public void UpdateUI()
        {
            try
            {
                windowIcon.sprite = icon;
                windowTitle.text = titleText;
                windowDescription.text = descriptionText;
            }

            catch
            {
                Debug.LogWarning("Modal Window - Cannot update the content due to missing variables.", this);
            }
        }

        public void OpenWindow()
        {
            if (isOn == false)
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-in", 0.1f);
                else
                    mwAnimator.Play("Fade-in");

                isOn = true;
            }
        }


		public void CloseWindowGoGuest()
		{
			if (isOn == true)
			{
				if (sharpAnimations == false)
				{
					mwAnimator.CrossFade("Fade-out", 0.1f);
					//playFabAuth.PlayHasGuest();
				}
				else
				{
					mwAnimator.Play("Fade-out");
					
				}

				isOn = false;
			}

		}
		public void CloseWindow()
		{
			if (isOn == true)
			{
				if (sharpAnimations == false)
					mwAnimator.CrossFade("Fade-out", 0f);
				else
					mwAnimator.Play("Fade-out");



				isOn = false;
			}

			
		}

		public void AnimateWindow()
        {
            if (isOn == false)
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-in", 0.1f);
                else
                    mwAnimator.Play("Fade-in");

                isOn = true;
            }

            else
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-out", 0.1f);
                else
                    mwAnimator.Play("Fade-out");

                isOn = false;
            }
        }
    }
