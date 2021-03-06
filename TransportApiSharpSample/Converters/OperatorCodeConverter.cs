﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TransportApiSharpSample.Converters
{
    public class OperatorCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string returnVal = string.Empty;

            var valueString = value as string;

            if (App.OperatorCodes != null)
            {
                if (valueString != null)
                {
                    // Operator code 'TFL' isn't in the operator code database...
                    if (valueString == "TFL")
                        returnVal = "London Buses";
                    else
                    {
                        var operatorRecord = App.OperatorCodes
                                    .Where(operators => operators.Code == valueString)
                                    .ToList();

                        if (operatorRecord != null && operatorRecord.Count > 0)
                            returnVal = operatorRecord[0].ShortName;
                    }
                }
            }

            return (string.IsNullOrEmpty(returnVal)) ? valueString : returnVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}