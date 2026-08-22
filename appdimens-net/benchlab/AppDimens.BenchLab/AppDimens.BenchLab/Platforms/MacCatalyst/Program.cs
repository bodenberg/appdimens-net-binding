using ObjCRuntime;
using UIKit;

namespace AppDimens.BenchLab.Platforms.MacCatalyst;

public class Program
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
