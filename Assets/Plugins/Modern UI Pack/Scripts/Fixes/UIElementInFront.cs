﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


    public class UIElementInFront : MonoBehaviour
    {
        void Start()
        {
            transform.SetAsLastSibling();
        }
    }
