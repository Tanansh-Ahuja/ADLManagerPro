using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace ADLManagerPro
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (ApiKeyForm keyForm = new ApiKeyForm())
            {
                if (keyForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string appSecretKey = keyForm.SecretKey;
                tt_net_sdk.ServiceEnvironment environment = tt_net_sdk.ServiceEnvironment.UatCert;
                tt_net_sdk.TTAPIOptions apiConfig = new tt_net_sdk.TTAPIOptions(environment, appSecretKey, 5000);


                using (Dispatcher disp = Dispatcher.AttachUIDispatcher())
                {
                    Application.EnableVisualStyles();

                    // Create an instance of the API
                    Form1 frm = new Form1(appSecretKey);
                    //apiConfig.EnableAccountFiltering = true;
                    ApiInitializeHandler handler = new ApiInitializeHandler(frm.ttNetApiInitHandler);
                    TTAPI.CreateTTAPI(disp, apiConfig, handler);

                    Application.Run(frm);
                }

            }
        }
    }
}
