﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RotA.Mono
{
    public class PrecursorIllumControl : MonoBehaviour
    {
        private Color colorBefore;
        /// <summary>
        /// The color that the material will be set to each frame.
        /// </summary>
        private Color colorNow;
        private Color targetColor;
        private float timeColorShifted;
        private float shiftLength;
        private Renderer renderer;

        public Color TargetColor { get { return targetColor; } }

        private void Start()
        {
            colorNow = Color.green;
            SetTargetColor(Color.green, 1f);
            renderer = GetComponentInChildren<Renderer>();
        }

        public void SetTargetColor(Color color, float shiftLength)
        {
            colorBefore = colorNow;
            targetColor = color;
            timeColorShifted = Time.time;
            this.shiftLength = shiftLength;
            this.StopAllCoroutines();
        }

        public void SetTargetColor(PrecursorColor color, float shiftLength)
        {
            colorBefore = colorNow;
            targetColor = GetColorForEnum(color);
            timeColorShifted = Time.time;
            this.shiftLength = shiftLength;
            this.StopAllCoroutines();
        }

        private void SetTargetColorWithoutStoppingCoroutine(Color color, float shiftLength)
        {
            colorBefore = colorNow;
            targetColor = color;
            timeColorShifted = Time.time;
            this.shiftLength = shiftLength;
        }

        public void Pulse(Color intoColor, Color resetColor, float pulseLength, float transitionInLength, float transitionOutLength)
        {
            StartCoroutine(PulseCoroutine(intoColor, resetColor, pulseLength, transitionInLength, transitionOutLength));
        }

        public void Pulse(PrecursorColor intoColor, PrecursorColor resetColor, float pulseLength, float transitionInLength, float transitionOutLength)
        {
            StartCoroutine(PulseCoroutine(GetColorForEnum(intoColor), GetColorForEnum(resetColor), pulseLength, transitionInLength, transitionOutLength));
        }

        private IEnumerator PulseCoroutine(Color target, Color resetColor, float pulseLength, float transitionInLength, float transitionOutLength)
        {
            SetTargetColorWithoutStoppingCoroutine(target, transitionInLength);
            yield return new WaitForSeconds(transitionInLength + pulseLength);
            SetTargetColorWithoutStoppingCoroutine(resetColor, transitionOutLength);
        }

        void Update()
        {
            colorNow = Color.Lerp(colorBefore, targetColor, (Time.time - timeColorShifted) / shiftLength);
            renderer.material.SetColor("_Color", colorNow);
        }

        public enum PrecursorColor
        {
            Black,
            Green,
            Purple,
            Pink
        }

        static Color GetColorForEnum(PrecursorColor col)
        {
            switch (col)
            {
                default:
                    return Color.black;
                case PrecursorColor.Green:
                    return green;
                case PrecursorColor.Purple:
                    return purple;
                case PrecursorColor.Pink:
                    return pink;
            }
        }

        static readonly Color green = new Color(0.54f, 1f, 0.54f);
        static readonly Color purple = new Color(0.75f, 0f, 1f);
        static readonly Color pink = new Color(1f, 0.5f, 0.8f);
    }
}
