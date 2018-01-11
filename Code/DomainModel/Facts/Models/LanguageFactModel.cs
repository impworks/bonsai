﻿using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying known languages.
    /// </summary>
    public class LanguageFactModel: FactModelBase
    {
        /// <summary>
        /// List of known languages.
        /// </summary>
        public LanguageFactItem[] Values { get; set; }
    }

    /// <summary>
    /// Information about a single known language.
    /// </summary>
    public class LanguageFactItem : DurationFactItem
    {
        /// <summary>
        /// Name of the language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Language proficiency.
        /// </summary>
        public LanguageProficiency? Proficiency { get; set; }
    }

    public enum LanguageProficiency
    {
        [Description("Базовый")]
        Beginner,

        [Description("Средний")]
        Intermediate,

        [Description("Свободный")]
        Profound,

        [Description("Родной")]
        Native
    }
}