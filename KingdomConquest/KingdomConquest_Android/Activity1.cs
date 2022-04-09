using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;

namespace KingdomConquest_Android
{
   [Activity(Label = "KingdomConquest_Android"
       , MainLauncher = true
       , Icon = "@drawable/icon"
       , Theme = "@style/Theme.Splash"
       , AlwaysRetainTaskState = true
       , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
       , ScreenOrientation = ScreenOrientation.Landscape
       , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
   public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
   {
      private Microsoft.Xna.Framework.Game g;
      private View.KeyEventArgs prevKey;
      
      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);
         g = new Game1();
         SetContentView((View)g.Services.GetService(typeof(View)));
         g.Run();
      }
      /*
      private void OnKeyPress(object sender, View.KeyEventArgs e)
      {
         
      }

      private static void ShowKeyboard()
      {
         var pView = g.Services.GetService<View>();
         var inputMethodManager = g.Application.GetSystemService(InputMethodService) as InputMethodManager;
         inputMethodManager.ShowSoftInput(pView, ShowFlags.Forced);
         inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
      }

      private static void HideKeyboard()
      {
         InputMethodManager inputMethodManager = g.Application.GetSystemService(Context.InputMethodService) as InputMethodManager;
         inputMethodManager.HideSoftInputFromWindow(g.pView.WindowToken, HideSoftInputFlags.None);
      }*/
   }
}

