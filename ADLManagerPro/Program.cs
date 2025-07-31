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
        
        [STAThread]
        static void Main()
        {
            try
            {

                using (ApiKeyForm keyForm = new ApiKeyForm())
                {
                    if (keyForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    string appSecretKey = keyForm.SecretKey;
                    tt_net_sdk.ServiceEnvironment environment;
                    switch (keyForm.SelectedEnvironment)
                    {
                        case "ProdSim":
                            environment = tt_net_sdk.ServiceEnvironment.ProdSim;
                            break;
                        case "ProdLive":
                            environment = tt_net_sdk.ServiceEnvironment.ProdLive;
                            break;
                        case "UatCert":
                        default:
                            environment = tt_net_sdk.ServiceEnvironment.UatCert;
                            break;
                    }
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
            catch
            {
                MessageBox.Show("Error occured while starting the application. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
            }
        }
    }
}
