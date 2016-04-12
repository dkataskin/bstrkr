using Android.App;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;

namespace bstrkr.core.android.views
{
    public class BusTrackerActionBarDrawerToggle : Android.Support.V7.App.ActionBarDrawerToggle
    {
        public BusTrackerActionBarDrawerToggle(
                        Activity activity,
                        DrawerLayout drawerLayout,
                        int openDrawerContentDescRes,
                        int closeDrawerContentDescRes)
            : base(activity, drawerLayout, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
        }

        public BusTrackerActionBarDrawerToggle(
                                    Activity activity,
                                    DrawerLayout drawerLayout,
                                    Toolbar toolbar,
                                    int openDrawerContentDescRes,
                                    int closeDrawerContentDescRes)
            : base(activity, drawerLayout, toolbar, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
        }

        public event ActionBarDrawerChangedEventHandler DrawerClosed;
        public event ActionBarDrawerChangedEventHandler DrawerOpened;
        public event ActionBarDrawerChangedEventHandler DrawerSlide;
        public event ActionBarDrawerChangedEventHandler DrawerStateChanged;

        public override void OnDrawerClosed(View drawerView)
        {
            this.DrawerClosed?.Invoke(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });

            base.OnDrawerClosed(drawerView);
        }

        public override void OnDrawerOpened(View drawerView)
        {
            this.DrawerOpened?.Invoke(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });

            base.OnDrawerOpened(drawerView);
        }

        public override void OnDrawerSlide(View drawerView, float slideOffset)
        {
            this.DrawerSlide?.Invoke(
                this,
                new ActionBarDrawerEventArgs
                {
                    DrawerView = drawerView,
                    SlideOffset = slideOffset
                });

            base.OnDrawerSlide(drawerView, slideOffset);
        }

        public override void OnDrawerStateChanged(int newState)
        {
            this.DrawerStateChanged?.Invoke(
                this,
                new ActionBarDrawerEventArgs
                {
                    NewState = newState
                });

            base.OnDrawerStateChanged(newState);
        }
    }
}