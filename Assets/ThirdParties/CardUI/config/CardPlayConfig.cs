using System;
using UnityEngine;

namespace config 
{
    [Serializable]
    public class CardPlayConfig
    {
        [SerializeField] private Transform prepareArea;
        [SerializeField] private RectTransform playArea;
        [SerializeField] private Transform drawArea;
        [SerializeField] private Transform discardArea;
        public Transform PrepareArea => prepareArea;
        public RectTransform PlayArea => playArea;
        public Transform DrawArea => drawArea;
        public Transform DiscardArea => discardArea;

        //public bool DestroyOnPlay;
    }
}
