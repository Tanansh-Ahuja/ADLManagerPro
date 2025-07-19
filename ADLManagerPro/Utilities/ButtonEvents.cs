using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace ADLManagerPro
{
    public class ButtonEvents
    {
        public ButtonEvents()
        {

        }
        public void AddRowInMainGrid(DataGridView mainGrid)
        {
            int serialNumber = mainGrid.Rows.Count + 1;
            mainGrid.Rows.Add(false, serialNumber);
        }

        public void OnStartbtnClick(DataGridView paramGrid, string AlgoName, List<string> _accounts,
            Dictionary<string, string> tabIndexWithSiteOrderKey, string currentTab, Dictionary<string, AdlParameters> algoNameWithParameters,
             Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription, Dictionary<string, Instrument> instrumentNameWithInstrument)
        {

            if (tabIndexWithSiteOrderKey.ContainsKey(currentTab))
            {
                MessageBox.Show("Order already placed!");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to place this order?",
                "Confirm sending order",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.OK)
            {
                // User clicked Cancel or closed the dialog — do nothing
                return;
            }
            Dictionary<string, object> algo_userparams = new Dictionary<string, object>();
            Dictionary<string, object> algo_orderprofileparams = new Dictionary<string, object>();
            string instrumentName = null;
            int accountNumber = -1;
            foreach (DataGridViewRow row in paramGrid.Rows)
            {
                if (row.IsNewRow) continue; // skip any placeholder row

                string paramName = row.Cells["ParamName"].Value?.ToString()?.Trim();
                var valueCell = row.Cells["Value"];

                if (valueCell != null)
                {

                    if (string.IsNullOrEmpty(paramName)) continue;

                    object value = null;

                    // Handle cell type: TextBox, ComboBox, CheckBox
                    if (valueCell is DataGridViewTextBoxCell || valueCell is DataGridViewComboBoxCell)
                    {
                        value = valueCell.Value;
                        if (value == null || (value is string && string.IsNullOrWhiteSpace((string)value)))
                        {
                            MessageBox.Show("Please enter all the parameters before starting the algo.");
                            return;
                        }
                    }
                    else if (valueCell is DataGridViewCheckBoxCell && valueCell.Value != null)
                    {
                        value = Convert.ToBoolean(valueCell.Value);
                    }


                    if (algoNameWithParameters.ContainsKey(AlgoName))
                    {
                        if (paramName == "Quoting Instrument Account")
                        {
                            accountNumber = _accounts.IndexOf(value.ToString());

                        }
                        if (paramName == "Quoting Instrument")
                        {
                            instrumentName = value.ToString();

                        }

                        if (algoNameWithParameters[AlgoName]._adlUserParameters.Contains(paramName))
                        {
                            algo_userparams[paramName] = value;
                        }
                        else if (algoNameWithParameters[AlgoName]._adlOrderProfileParameters.Contains(paramName))
                        {
                            algo_orderprofileparams[paramName] = value;
                        }
                        else
                        {
                            Console.WriteLine("ERROR: unknown param: " + paramName + " value: " + value);
                        }
                    }


                }
                else
                {
                    MessageBox.Show("Please enter all the parameters before starting the algo.");
                    return;
                }
            }

            if (accountNumber >= 0 &&
                algoNameWithTradeSubscription.ContainsKey(AlgoName) &&
                instrumentNameWithInstrument.ContainsKey(instrumentName))
            {
                string orderKey = algoNameWithTradeSubscription[AlgoName].StartAlgo(accountNumber,
                        instrumentNameWithInstrument[instrumentName],
                        algo_userparams,
                        algo_orderprofileparams);
                if (tabIndexWithSiteOrderKey.ContainsKey(currentTab))
                {
                    tabIndexWithSiteOrderKey.Add(currentTab, orderKey);
                }
                else
                {
                    tabIndexWithSiteOrderKey[currentTab] = orderKey;
                }


            }
            else
            {
                MessageBox.Show("Please check the parameters.");
                return;
            }

        }


        public void OnDeletebtnClick(DataGridView paramGrid, string AlgoName, Dictionary<string, string> tabIndexWithSiteOrderKey, string currentTab,
            Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription)
        {
            if (!tabIndexWithSiteOrderKey.ContainsKey(currentTab)
                || (tabIndexWithSiteOrderKey.ContainsKey(currentTab) && tabIndexWithSiteOrderKey[currentTab] == string.Empty))
            {
                MessageBox.Show("Order already placed!");
                return;
            }
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this order?",
                "Confirm Deletion",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.OK)
            {
                // User clicked Cancel or closed the dialog — do nothing
                return;
            }
            // TODO: Complete implementation

            //foreach (DataGridViewRow row in mainGrid.Rows)
            //{
            //    var cellValue = row.Cells[columnOneName].Value;

            //}
            if (tabIndexWithSiteOrderKey.ContainsKey(currentTab) && algoNameWithTradeSubscription.ContainsKey(AlgoName))
            {
                string orderKey = tabIndexWithSiteOrderKey[currentTab];

                algoNameWithTradeSubscription[AlgoName].DeleteAlgoOrder(orderKey);
            }
        }
    }
}
