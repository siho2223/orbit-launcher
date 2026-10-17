using System.Globalization;
using System.Windows.Data;
namespace OrbitLauncher;
public sealed class ScrollViewportConverter:IMultiValueConverter {
 public object Convert(object[] values,Type targetType,object parameter,CultureInfo culture){if(values.Length!=3||values.Any(v=>v is not double))return 0d;double viewport=(double)values[0],maximum=(double)values[1],height=(double)values[2];if(height<=34||maximum<=0)return viewport;return Math.Max(viewport,maximum*32/(height-32));}
 public object[] ConvertBack(object value,Type[] targetTypes,object parameter,CultureInfo culture)=>throw new NotSupportedException();
}
