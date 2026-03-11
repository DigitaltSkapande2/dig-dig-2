using UnityEngine.UIElements;

namespace DigDig2.UI.UxmlElements
{
	[UxmlElement] public sealed partial class SaveFile : VisualElement
	{
		private readonly Label saveInfoLabel;

		private readonly Label saveNameLabel;

		private string saveInfo = "Save Info";
		private string saveName = "Save Name";

		public SaveFile( ) : this( "" ) { }

		public SaveFile( string saveName )
		{
			this.saveName = saveName;

			focusable = true;
			AddToClassList( "saveFile" );

			Add( saveNameLabel = new( ) );
			saveNameLabel.name = "saveName";
			saveNameLabel.text = this.saveName;

			Add( saveInfoLabel = new( ) );
			saveInfoLabel.name = "saveInfo";
			saveInfoLabel.text = "Save Info";
		}

		[UxmlAttribute] public string SaveName
		{
			get => saveName;
			set
			{
				saveName = value;
				if ( saveNameLabel != null ) saveNameLabel.text = saveName;
			}
		}

		[UxmlAttribute] public string SaveInfo
		{
			get => saveInfo;
			set
			{
				saveInfo = value;
				if ( saveInfoLabel != null ) saveInfoLabel.text = saveInfo;
			}
		}
	}
}
