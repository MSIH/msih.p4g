// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Text.RegularExpressions;

namespace MSIH.Core.Services.Messages.Utilities
{
    /// <summary>
    /// Helper class for processing message templates and replacing placeholders
    /// </summary>
    public static class TemplateProcessor
    {
        // Regular expression to match placeholders in the format {{PlaceholderName}}
        private static readonly Regex _placeholderRegex = new Regex(@"\{\{([^{}]+)\}\}", RegexOptions.Compiled);

        /// <summary>
        /// Processes a template by replacing placeholders with values
        /// </summary>
        /// <param name="templateContent">The template content with placeholders</param>
        /// <param name="placeholderValues">Dictionary of placeholder values</param>
        /// <returns>The processed template with placeholders replaced</returns>
        public static string ProcessTemplate(string templateContent, Dictionary<string, string> placeholderValues)
        {
            if (string.IsNullOrEmpty(templateContent))
            {
                return string.Empty;
            }

            if (placeholderValues == null)
            {
                return templateContent;
            }

            return _placeholderRegex.Replace(templateContent, match =>
            {
                string placeholderName = match.Groups[1].Value.Trim();
                return placeholderValues.TryGetValue(placeholderName, out string value)
                    ? value
                    : match.Value; // Keep the placeholder if no value is provided
            });
        }

        /// <summary>
        /// Extracts all placeholder names from a template
        /// </summary>
        /// <param name="templateContent">The template content</param>
        /// <returns>List of placeholder names found in the template</returns>
        public static List<string> ExtractPlaceholders(string templateContent)
        {
            if (string.IsNullOrEmpty(templateContent))
            {
                return new List<string>();
            }

            var placeholders = new List<string>();
            var matches = _placeholderRegex.Matches(templateContent);

            foreach (Match match in matches)
            {
                string placeholderName = match.Groups[1].Value.Trim();
                if (!placeholders.Contains(placeholderName))
                {
                    placeholders.Add(placeholderName);
                }
            }

            return placeholders;
        }

        /// <summary>
        /// Validates that all required placeholders have values
        /// </summary>
        /// <param name="templateContent">The template content</param>
        /// <param name="placeholderValues">Dictionary of placeholder values</param>
        /// <param name="missingPlaceholders">Output list of missing placeholder names</param>
        /// <returns>True if all required placeholders have values, false otherwise</returns>
        public static bool ValidatePlaceholders(
            string templateContent,
            Dictionary<string, string> placeholderValues,
            out List<string> missingPlaceholders)
        {
            missingPlaceholders = new List<string>();

            if (string.IsNullOrEmpty(templateContent) || placeholderValues == null)
            {
                return true;
            }

            var requiredPlaceholders = ExtractPlaceholders(templateContent);

            foreach (var placeholder in requiredPlaceholders)
            {
                if (!placeholderValues.ContainsKey(placeholder) || string.IsNullOrEmpty(placeholderValues[placeholder]))
                {
                    missingPlaceholders.Add(placeholder);
                }
            }

            return missingPlaceholders.Count == 0;
        }
    }
}
