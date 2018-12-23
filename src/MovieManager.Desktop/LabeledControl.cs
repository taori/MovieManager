using System.Windows;
using System.Windows.Controls;

namespace MovieManager.Desktop
{
	/// <summary>
	/// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
	///
	/// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
	/// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
	/// an der Stelle hinzu, an der es verwendet werden soll:
	///
	///     xmlns:MyNamespace="clr-namespace:KinoxToExporter.Desktop"
	///
	///
	/// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
	/// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
	/// an der Stelle hinzu, an der es verwendet werden soll:
	///
	///     xmlns:MyNamespace="clr-namespace:KinoxToExporter.Desktop;assembly=KinoxToExporter.Desktop"
	///
	/// Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
	/// zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
	///
	///     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
	///     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
	///
	///
	/// Schritt 2)
	/// Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
	///
	///     <MyNamespace:LabeledControl/>
	///
	/// </summary>
	public class LabeledControl : Control
	{
		static LabeledControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledControl), new FrameworkPropertyMetadata(typeof(LabeledControl)));
		}

		public static readonly DependencyProperty ValueControlProperty = DependencyProperty.Register(
			nameof(ValueControl), typeof(object), typeof(LabeledControl), new PropertyMetadata(default(object)));

		public object ValueControl
		{
			get { return (object) GetValue(ValueControlProperty); }
			set { SetValue(ValueControlProperty, value); }
		}
	}
}
