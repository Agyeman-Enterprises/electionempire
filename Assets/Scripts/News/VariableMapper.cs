using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Maps news entities to template variables
    /// </summary>
    public class VariableMapper
    {
        /// <summary>
        /// Fill template with values from processed news
        /// </summary>
        public Dictionary<string, string> MapVariables(ProcessedNews news, EventTemplate template)
        {
            var variables = new Dictionary<string, string>();
            
            if (template.VariableSlots == null)
                return variables;
            
            foreach (var slot in template.VariableSlots)
            {
                string value = ExtractVariableValue(news, slot.Key, slot.Value);
                variables[slot.Key] = value;
            }
            
            return variables;
        }
        
        private string ExtractVariableValue(ProcessedNews news, string variableName, VariableType type)
        {
            switch (type)
            {
                case VariableType.Person:
                    return ExtractPerson(news);
                
                case VariableType.Organization:
                    return ExtractOrganization(news);
                
                case VariableType.Location:
                    return ExtractLocation(news);
                
                case VariableType.Topic:
                    return ExtractTopic(news);
                
                case VariableType.Issue:
                    return ExtractIssue(news);
                
                case VariableType.Number:
                    return ExtractNumber(news, variableName);
                
                case VariableType.Date:
                    return news.OriginalArticle.PublishedDate.ToString("MMMM dd, yyyy");
            }
            
            return "Unknown";
        }
        
        private string ExtractPerson(ProcessedNews news)
        {
            // Find first person entity
            var person = news.Entities?.FirstOrDefault(e => e.Type == EntityType.Person);
            
            if (person != null)
                return person.Name;
            
            // Fallback: extract from title/description
            return ExtractNameFromText(news.OriginalArticle.Title + " " + news.OriginalArticle.Description);
        }
        
        private string ExtractOrganization(ProcessedNews news)
        {
            var org = news.Entities?.FirstOrDefault(e => e.Type == EntityType.Organization);
            
            if (org != null)
                return org.Name;
            
            // Common organizations
            string[] commonOrgs = { "Congress", "Senate", "House", "White House", "Supreme Court" };
            string text = (news.OriginalArticle.Title + " " + news.OriginalArticle.Description).ToLower();
            
            foreach (var orgName in commonOrgs)
            {
                if (text.Contains(orgName.ToLower()))
                    return orgName;
            }
            
            return "Government";
        }
        
        private string ExtractLocation(ProcessedNews news)
        {
            var location = news.Entities?.FirstOrDefault(e => e.Type == EntityType.Location);
            
            if (location != null)
                return location.Name;
            
            // Common locations
            string[] commonLocations = { "Washington", "DC", "Capitol", "White House", "United States" };
            string text = (news.OriginalArticle.Title + " " + news.OriginalArticle.Description);
            
            foreach (var loc in commonLocations)
            {
                if (text.Contains(loc))
                    return loc;
            }
            
            return "Nationwide";
        }
        
        private string ExtractTopic(ProcessedNews news)
        {
            // Use first topic from processed news
            if (news.Topics != null && news.Topics.Count > 0)
            {
                return CapitalizeFirst(news.Topics[0]);
            }
            
            // Use first issue category
            if (news.IssueCategories != null && news.IssueCategories.Count > 0)
            {
                return news.IssueCategories[0].ToString();
            }
            
            return "Current Events";
        }
        
        private string ExtractIssue(ProcessedNews news)
        {
            if (news.IssueCategories != null && news.IssueCategories.Count > 0)
            {
                return news.IssueCategories[0].ToString();
            }
            
            return "Policy";
        }
        
        private string ExtractNumber(ProcessedNews news, string variableName)
        {
            // Extract numbers from text based on variable name
            string text = news.OriginalArticle.Title + " " + news.OriginalArticle.Description;
            
            // Look for percentages, dollar amounts, counts
            if (variableName.Contains("PERCENT") || variableName.Contains("RATE"))
            {
                // Find percentage patterns
                var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d+)%");
                if (match.Success)
                    return match.Groups[1].Value;
            }
            
            if (variableName.Contains("AMOUNT") || variableName.Contains("COST"))
            {
                // Find dollar amounts
                var match = System.Text.RegularExpressions.Regex.Match(text, @"\$(\d+(?:,\d+)*(?:\.\d+)?)");
                if (match.Success)
                    return match.Groups[1].Value;
            }
            
            // Generic number
            var numMatch = System.Text.RegularExpressions.Regex.Match(text, @"\b(\d+)\b");
            if (numMatch.Success)
                return numMatch.Groups[1].Value;
            
            return "0";
        }
        
        private string ExtractNameFromText(string text)
        {
            // Simple name extraction (first capitalized word sequence)
            var words = text.Split(' ');
            
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (char.IsUpper(words[i][0]) && char.IsUpper(words[i + 1][0]))
                {
                    return words[i] + " " + words[i + 1];
                }
            }
            
            // Fallback: first capitalized word
            foreach (var word in words)
            {
                if (word.Length > 0 && char.IsUpper(word[0]) && !IsCommonWord(word))
                {
                    return word.Trim(new char[] { ',', '.', ':', ';' });
                }
            }
            
            return "Official";
        }
        
        private bool IsCommonWord(string word)
        {
            string[] common = { "The", "A", "An", "In", "On", "At", "To", "For", "With", "By" };
            return common.Contains(word);
        }
        
        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            return char.ToUpper(text[0]) + text.Substring(1);
        }
        
        /// <summary>
        /// Fill template string with variable values
        /// </summary>
        public string FillTemplate(string template, Dictionary<string, string> variables)
        {
            string result = template;
            
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }
            
            return result;
        }
    }
}

