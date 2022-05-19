using System;
using SecondScreen.Utilities.Extensions;

namespace SecondScreen
{
    namespace Utilities
    {
        namespace Extensions
        {
            public static class ActionExtensions
            {
                public static void RunAfter(this Action action, int span)
                {
                    var myTimer = new System.Windows.Forms.Timer { Interval = span };
                    myTimer.Tick += (sender, args) =>
                    {
                        var timer = (System.Windows.Forms.Timer)sender;
                        if (timer != null)
                        {
                            timer.Stop();
                        }

                        action();
                    };
                    myTimer.Start();
                }
            }
        }

        //<Namespace>.Utilities
        public static class CommonUtil
        {
            public static void Run(Action action, int afterSpan)
            {
                action.RunAfter(afterSpan);
            }
        }
    }
}