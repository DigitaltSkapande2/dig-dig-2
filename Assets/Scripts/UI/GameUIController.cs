using System.Collections.Generic;
using System.Linq;
using DigDig2.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2.UINavigation
{
    [RequireComponent(typeof(UIDocument))]
    public class GameUIController : MonoBehaviour
    {
        UIDocument uiDocument;
        VisualElement characterIndicatorContainer;

        Image maxCharacterIndicatorImage;
        Image minisCharacterIndicatorImage;

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        void Start()
        {

            characterIndicatorContainer = uiDocument.rootVisualElement.Query<VisualElement>("mainNavigation");

            maxCharacterIndicatorImage = characterIndicatorContainer.Query<Image>("maxCharacterIndicatorImage");
            minisCharacterIndicatorImage = characterIndicatorContainer.Query<Image>("minisCharacterIndicatorImage");
        }

        



    }
}