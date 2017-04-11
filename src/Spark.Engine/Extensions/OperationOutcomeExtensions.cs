/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Spark.Engine.Core;

namespace Spark.Engine.Extensions
{
    public static class OperationOutcomeExtensions
    {
        private static void Init(this OperationOutcome outcome)
        {
            if (outcome.Issue == null)
            {
                outcome.Issue = new List<OperationOutcome.IssueComponent>();
            }
        }

        private static void AddError(this OperationOutcome outcome, Exception exception)
        {
            string message;

            if(exception is SparkException)
                message = exception.Message;
            else
                message = $"{exception.GetType().Name}: {exception.Message}";
            
           outcome.AddError(message);

            // Don't add a stacktrace if this is an acceptable logical-level error
            if (!(exception is SparkException))
            {
                var stackTrace = new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Information,
                    Diagnostics = exception.StackTrace
                };
                outcome.Issue.Add(stackTrace);
            }
        }

        public static OperationOutcome AddAllInnerErrors(this OperationOutcome outcome, Exception exception)
        {
            AddError(outcome, exception);
            while (exception.InnerException != null)
            {
                AddError(outcome, exception);
                exception = exception.InnerException;
            }

            return outcome;
        }

        public static OperationOutcome AddError(this OperationOutcome outcome, string message)
        {
            return outcome.AddIssue(OperationOutcome.IssueSeverity.Error, message);
        }

        private static OperationOutcome AddIssue(this OperationOutcome outcome, OperationOutcome.IssueSeverity severity, string message)
        {
            if (outcome.Issue == null) outcome.Init();

            var item = new OperationOutcome.IssueComponent
            {
                Severity = severity,
                Diagnostics = message
            };
            outcome.Issue.Add(item);
            return outcome;
        }
    }
}