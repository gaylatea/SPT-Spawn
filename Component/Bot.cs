using EFT;
using UnityEngine;

using System;
using System.Collections;

namespace Framesaver.Component
{
    public class Bot : MonoBehaviour
    {
        public BotOwner bot;

        public void Awake()
        {
            bot = GetComponent<BotOwner>();
            StartCoroutine(Run());
        }

        public IEnumerator Run()
        {
            while (true)
            {
                bot?.Brain?.Agent?.Update();
                bot?.UpdateManual();

                yield return new WaitForEndOfFrame();
            }
        }
    }
}