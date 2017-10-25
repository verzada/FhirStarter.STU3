using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FhirStarter.Bonfire.STU3.Parameter
{
    public class CommonParameters
    {
        #region Prefix
        public static readonly string PrefixEqual = "eq";
        public static readonly string PrefixNotEqual = "ne";
        public static readonly string PrefixLessThan = "lt";
        public static readonly string PrefixGreaterThan = "gt";
        public static readonly string PrefixSignGreaterThan = ">";
        public static readonly string PrefixGreaterOrEqual = "ge";
        public static readonly string PrefixSignGreaterOrEqual = "=>";
        public static readonly string PrefixSignLessThan = "<";
        public static readonly string PrefixLessOrEqual = "le";
        public static readonly string PrefixSignLessOrEqual = "<=";
        #endregion Prefix

        #region Parameter
        public static readonly string ParameterLastUpdated = "_lastupdated";
        public static readonly string ParameterName = "name";
        public static readonly string ParameterNameContains = "name:contains";
        public static readonly string ParameterNameExact = "name:exact";
        public static readonly string ParameterIdentifier = "identifier";
        public static readonly string ParameterPatientIdentifier = "Patient.identifier";
        public static readonly string ParameterContained = "_contained";
        public static readonly string ParameterInclude = "_include";
        public static readonly string ParameterRevInclude = "_revinclude";
        #endregion Parameter

        public CommonParameters(SearchParams parameters)
        {
            SetupSearchParameters(parameters);
        }

        #region Methods

        private void SetupSearchParameters(SearchParams searchParams)
        {
            var parameters = searchParams.Parameters;
            foreach (var parameter in parameters)
            {
                var item1 = parameter.Item1;

                if (item1.ToLower().Contains(ParameterLastUpdated.ToLower()) &&
                    HasLastupdatedDates == false)
                {
                    GetDates(parameters);
                }
                if (item1.ToLower().Contains(ParameterName.ToLower()))
                {
                    GetNameAndOperator(parameters);
                }
                if (item1.ToLower().Equals(ParameterIdentifier.ToLower()))
                {
                    GetIdentifier(parameters, ParameterIdentifier);
                }
                if (item1.ToLower().Equals(ParameterPatientIdentifier.ToLower()))
                {
                    GetIdentifier(parameters, ParameterPatientIdentifier);
                }
            }
            if (searchParams.Include.Count > 0)
            {
                GetInclude(searchParams.Include);
            }
            if (searchParams.RevInclude.Count > 0)
            {
                GetRevInclude(searchParams.RevInclude);
            }
        }

        private void GetIdentifier(IList<Tuple<string, string>> parameters, string parameterName)
        {
            var identifer =
                parameters
                    .FirstOrDefault(param => param.Item1.ToLower().Contains(parameterName.ToLower()));

            if (identifer != null)
            {
                if (parameterName == ParameterIdentifier)
                {
                    Identifier = identifer.Item2;
                    IdentifierOperator = FindOperator(identifer);
                }
                else if (parameterName == ParameterPatientIdentifier)
                {
                    PatientIdentifier = identifer.Item2;
                    PatientIdentifierOperator = FindOperator(identifer);
                }
            }
            else
            {
                throw new ArgumentException(parameterName + " is null or empty. " + parameterName +
                                            " must contain a valid value");
            }
        }

        private void GetNameAndOperator(IList<Tuple<string, string>> parameters)
        {
            var name = parameters.Where(param => param.Item1.ToLower().Contains(ParameterName.ToLower())).ToList();

            if (name.Any())
            {
                switch (name.Count)
                {
                    case 1:
                        var nameData = name[0];
                        NameOperator = FindOperator(nameData);
                        Name = nameData.Item2;
                        break;
                    default:
                        throw new ArgumentException("Can only contain one name argument, found " + name.Count);
                }
            }
        }

        private void GetDates(IEnumerable<Tuple<string, string>> parameters)
        {
            var dates =
                parameters.Where(param => param.Item1.ToLower().Contains(ParameterLastUpdated.ToLower())).ToList();

            var enumerable = dates;
            if (enumerable.Any())
            {
                switch (enumerable.Count)
                {
                    case 2:
                        string dateOneStr;
                        DateTimeOffset dateOne;
                        var dateOneOperator = ProcessLastUpdatedParameter(enumerable[0], out dateOneStr, out dateOne);


                        string dateTwoStr;
                        DateTimeOffset dateTwo;
                        var dateTwoOperator = ProcessLastUpdatedParameter(enumerable[1], out dateTwoStr, out dateTwo);


                        if (dateOne < dateTwo && IsDateOperatorsValid(dateOneOperator, dateTwoOperator))
                        {
                            SetDateFilter(dateOneStr, dateTwoStr, dateOneOperator, dateTwoOperator);
                        }
                        else if (dateTwo < dateOne && IsDateOperatorsValid(dateTwoOperator, dateOneOperator))
                        {
                            SetDateFilter(dateTwoStr, dateOneStr, dateTwoOperator, dateOneOperator);
                        }
                        else
                        {
                            throw new ArgumentException(
                                "The date operators are not valid compared to their date values. Check the operators set in the " +
                                ParameterLastUpdated + " parameters");
                        }
                        break;
                    case 1:
                        var dateOperator = GetOperatorForDateOrNumber(enumerable[0]);
                        switch (dateOperator)
                        {
                            case OperatorType.LessThan:
                            case OperatorType.LessOrEqual:
                                LastUpdatedToOperator = GetOperatorForDateOrNumber(enumerable[0]);
                                LastUpdatedToStr = RemovePrefix(enumerable[0].Item2, dateOperator);
                                break;
                            case OperatorType.Equals:
                            case OperatorType.NotEqual:
                            case OperatorType.GreaterOrEqual:
                            case OperatorType.GreaterThan:
                                LastUpdatedFromOperator = GetOperatorForDateOrNumber(enumerable[0]);
                                LastUpdatedFromStr = RemovePrefix(enumerable[0].Item2, dateOperator);
                                break;
                        }
                        break;
                    default:
                        throw new ArgumentException("Too many " + ParameterLastUpdated + ", can only maximum 2, got " + enumerable.Count);
                }
            }
        }

        private OperatorType ProcessLastUpdatedParameter(Tuple<string, string> enumerable, out string dateStr, out DateTimeOffset dateTimeOffset)
        {
            var dateOperator = GetOperatorForDateOrNumber(enumerable);
            dateStr = RemovePrefix(enumerable.Item2, dateOperator);
            var dateTime = new FhirDateTime(dateStr);
            dateTimeOffset = dateTime.ToDateTimeOffset();

            return dateOperator;
        }

        private void GetInclude(IList<string> include)
        {
            Include = include[0];
        }

        private void GetRevInclude(IList<string> revInclude)
        {
            RevInclude = revInclude[0];
        }

        private bool IsDateOperatorsValid(OperatorType from, OperatorType to)
        {
            if (from == OperatorType.LessThan || from == OperatorType.LessOrEqual)
            {
                return false;
            }
            if (to == OperatorType.GreaterOrEqual || to == OperatorType.GreaterThan)
            {
                return false;
            }
            return true;
        }

        private string RemovePrefix(string dateOrNumber, OperatorType operatorType)
        {
            if (operatorType == OperatorType.Equals)
                dateOrNumber = dateOrNumber.Replace(PrefixEqual, string.Empty);
            if (operatorType == OperatorType.NotEqual)
                dateOrNumber = dateOrNumber.Replace(PrefixNotEqual, string.Empty);
            if (operatorType == OperatorType.LessThan)
            {
                dateOrNumber = dateOrNumber.Replace(PrefixLessThan, string.Empty).Replace(PrefixSignLessThan, string.Empty);
            }

            if (operatorType == OperatorType.GreaterThan)
            {
                dateOrNumber = dateOrNumber.Replace(PrefixGreaterThan, string.Empty).Replace(PrefixSignGreaterThan, string.Empty);
            }

            if (operatorType == OperatorType.GreaterOrEqual)
            {
                dateOrNumber = dateOrNumber.Replace(PrefixGreaterOrEqual, string.Empty).Replace(PrefixSignGreaterOrEqual, string.Empty);
                var date = Convert.ToDateTime(dateOrNumber);
                var dateSub = date.Subtract(new TimeSpan(0, 0, 0, 1));
                dateOrNumber = dateSub.ToString("yyyy-MM-ddTHH:mm:ss");
                dateOrNumber = dateOrNumber.Replace('.', ':');
            }

            if (operatorType == OperatorType.LessOrEqual)
            {
                dateOrNumber = dateOrNumber.Replace(PrefixLessOrEqual, string.Empty).Replace(PrefixSignLessOrEqual, string.Empty);
                dateOrNumber = dateOrNumber.Replace(PrefixGreaterOrEqual, string.Empty).Replace(PrefixSignGreaterOrEqual, string.Empty);
                var date = Convert.ToDateTime(dateOrNumber);
                var dateSub = date.Add(new TimeSpan(0, 0, 0, 1));
                dateOrNumber = dateSub.ToString("yyyy-MM-ddTHH:mm:ss");
                dateOrNumber = dateOrNumber.Replace('.', ':');
            }
            return dateOrNumber.Trim();
        }

        private void SetDateFilter(string fromDate, string toDate, OperatorType fromOperator,
            OperatorType toOperator)
        {
            LastUpdatedFromStr = fromDate;
            LastUpdatedFromOperator = fromOperator;
            LastUpdatedToStr = toDate;
            LastUpdatedToOperator = toOperator;
        }

        /// <summary>
        /// Expects item2 to have 
        /// bracket or gt ge lt le etc
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        public OperatorType FindOperator(Tuple<string, string> tuple)
        {
            var split = tuple.Item1.Split(':');
            if (split.Length == 1)
                return OperatorType.Equals;
            var operatorType = split[1].ToLower();

            switch (operatorType)
            {
                case "gt":
                    return OperatorType.GreaterThan;
                case "ge":
                    return OperatorType.GreaterOrEqual;
                case "lt":
                    return OperatorType.LessThan;
                case "le":
                    return OperatorType.LessOrEqual;
                case "eq":
                    return OperatorType.Equals;
                case "contains":
                    return OperatorType.Contains;
                case "exact":
                    return OperatorType.Exact;
                default:
                    return OperatorType.Equals;
            }
        }

        public OperatorType GetOperatorForDateOrNumber(Tuple<string, string> tuple)
        {
            var dataItem = tuple.Item2;

            if (dataItem.Contains(PrefixEqual))
            {
                return OperatorType.Equals;
            }
            if (dataItem.Contains(PrefixNotEqual))
            {
                return OperatorType.NotEqual;
            }
            if (dataItem.Contains(PrefixLessThan) || dataItem.Contains(PrefixSignLessThan))
            {
                return OperatorType.LessThan;
            }
            if (dataItem.Contains(PrefixGreaterThan) || dataItem.Contains(PrefixSignGreaterThan))
            {
                return OperatorType.GreaterThan;
            }
            if (dataItem.Contains(PrefixGreaterOrEqual))
            {
                return OperatorType.GreaterOrEqual;
            }
            if (dataItem.StartsWith(PrefixLessOrEqual))
            {
                return OperatorType.LessOrEqual;
            }
            return OperatorType.Equals;
        }

        #endregion Methods

        #region Properties

        public bool HasLastUpdatedFrom => !string.IsNullOrEmpty(LastUpdatedFromStr);
        public DateTime LastUpdatedFrom => GetDateTimeFromString(LastUpdatedFromStr, true);

        public string LastUpdatedFromStr { private get; set; }
        public OperatorType LastUpdatedFromOperator { get; set; }

        public bool HasLastUpdatedTo => !string.IsNullOrEmpty(LastUpdatedToStr);

        public DateTime LastUpdatedTo => GetDateTimeFromString(LastUpdatedToStr, false);


        public string LastUpdatedToStr { private get; set; }
        public OperatorType LastUpdatedToOperator { get; set; }

        private static DateTime GetDateTimeFromString(string input, bool min)
        {
            DateTime datetime;
            if (input == null && !min)
            {
                datetime = DateTime.MaxValue;
            }
            else if (min && input == null)
            {


                datetime = DateTime.MinValue;
            }
            else
            {
                datetime = Convert.ToDateTime(input);
            }

            return datetime;
        }

        public bool HasLastupdatedDates => HasLastUpdatedTo || HasLastUpdatedFrom;

        public string Name { get; set; }
        public bool HasName => !string.IsNullOrEmpty(Name);
        public OperatorType NameOperator { get; set; }


        public string Identifier { get; set; }
        public bool HasIdentifier => !string.IsNullOrEmpty(Identifier);
        public OperatorType IdentifierOperator { get; set; }

        public string PatientIdentifier { get; set; }
        public bool HasPatientIdentifier => !string.IsNullOrEmpty(PatientIdentifier);
        public OperatorType PatientIdentifierOperator { get; set; }

        public string Include { get; set; }
        public bool HasInclude => !string.IsNullOrEmpty(Include);

        public string RevInclude { get; set; }
        public bool HasRevInclude => !string.IsNullOrEmpty(RevInclude);

        #endregion Properties
    }
}
