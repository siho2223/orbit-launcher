using System.Windows;
namespace OrbitLauncher;
public partial class XmclTheme:ResourceDictionary {
 public XmclTheme(){InitializeComponent();}
 void VersionCard_Click(object sender,RoutedEventArgs e){if(Application.Current.MainWindow is MainWindow window)window.VersionCard_Click(sender,e);}
 void InstanceFolder_Click(object sender,RoutedEventArgs e){if(Application.Current.MainWindow is MainWindow window)window.InstanceFolder_Click(sender,e);}
 void ExportPack_Click(object sender,RoutedEventArgs e){if(Application.Current.MainWindow is MainWindow window)window.ExportPack_Click(sender,e);}
 void XmclInstance_Click(object sender,RoutedEventArgs e){if(Application.Current.MainWindow is MainWindow window)window.XmclInstance_Click(sender,e);}
}
