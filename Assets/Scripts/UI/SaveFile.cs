using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2.UIElements
{
    [UxmlElement] public partial class SaveFile : VisualElement
    {
        private string _saveName = "Save Name";
        [UxmlAttribute] public string saveName
        {
            get => _saveName;
            set
            {
                _saveName = value;
                if (saveNameLabel != null) saveNameLabel.text = _saveName;
            }
        }

        private string _saveInfo = "Save Info";
        [UxmlAttribute] public string saveInfo
        {
            get => _saveInfo;
            set
            {
                _saveInfo = value;
                if (saveInfoLabel != null) saveInfoLabel.text = _saveInfo;
            }
        }

        private Label saveNameLabel;
        private Label saveInfoLabel;

        public event Action selected;



        public SaveFile() : this("") { }
        public SaveFile(string saveName)
        {
            _saveName = saveName;

            focusable = true;
            AddToClassList("saveFile");

            Add(saveNameLabel = new Label());
            saveNameLabel.name = "saveName";
            saveNameLabel.text = _saveName;

            Add(saveInfoLabel = new Label());
            saveInfoLabel.name = "saveInfo";
            saveInfoLabel.text = "Save Info";
        }
    }
}
