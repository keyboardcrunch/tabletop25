using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace VoidChaos
{
    public static class psychosis
    {
        public static void chaos() {
            Random random = new Random();
            int funcIndex = random.Next(0, 4);
            switch (funcIndex) {
                case 0:
                    yoToast();
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }

        public static void yoToast()
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Please update your AntiVirus!!!")
                .AddText("Your system is unsecure and your files are at risk!")
                .AddInlineImage(new Uri("https://beaverpro.sketchybins.com/infected.jpg"))
                .AddButton(new ToastButton()
                    .SetContent("Scan Now")
                    .AddArgument("action", "scannow")
                    .SetBackgroundActivation())
                .AddButton(new ToastButton()
                    .SetContent("Blow shit up")
                    .AddArgument("action", "kaboom")
                    .SetBackgroundActivation())
                .Show();
        }

        
    }
}
