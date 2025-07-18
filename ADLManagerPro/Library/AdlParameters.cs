using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADLManager;

namespace ADLManagerPro
{
    public class AdlParameters
    {
        public List<(string paramName, ParameterType)> _adlUserParametersWithType = new List<(string, ParameterType)>();
        public  List<(string paramName, ParameterType)> _adlOrderProfileParametersWithType = new  List<(string, ParameterType)>();
        public  List<string> _adlUserParameters = new  List<string>();
        public  List<string> _adlOrderProfileParameters = new  List<string>();
        public AdlParameters( List<(string paramName, ParameterType)> adlUserParametersWithType,
                             List<(string paramName, ParameterType)> adlOrderProfileParametersWithType,
                             List<string> adlUserParameters,
                             List<string> adlOrderProfileParameters)
                            
        {
            _adlOrderProfileParameters = adlOrderProfileParameters;
            _adlOrderProfileParametersWithType = adlOrderProfileParametersWithType;
            _adlUserParameters = adlUserParameters;
            _adlUserParametersWithType = adlUserParametersWithType;

        }
    }
}
