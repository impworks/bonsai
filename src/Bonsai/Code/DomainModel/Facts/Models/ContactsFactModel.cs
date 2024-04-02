using System;
using System.Text.RegularExpressions;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// Template for specifying known social profile links.
    /// </summary>
    public class ContactsFactModel: FactListModelBase<ContactFactItem>
    {
        public override void Validate()
        {
            for (var i = 1; i <= Values.Length; i++)
            {
                var item = Values[i-1];
                if (!Enum.IsDefined(item.Type))
                    throw new ValidationException(nameof(Page.Facts), $"Контакт #{i}: тип не указан");

                if(string.IsNullOrEmpty(item.Value))
                    throw new ValidationException(nameof(Page.Facts), $"Контакт #{i}: значение не указано");

                if (item.Type == ContactType.Email)
                {
                    if(!Regex.IsMatch(item.Value, ".+@.+\\..+"))
                        throw new ValidationException(nameof(Page.Facts), $"Контакт #{i}: адрес почты имеет некорректный формат");
                }
                else if (item.Type == ContactType.Phone)
                {
                    if (!Regex.IsMatch(item.Value, "\\+[0-9]+"))
                        throw new ValidationException(nameof(Page.Facts), $"Контакт #{i}: телефон должен начинаться с + и содержать только цифры");
                }
                else if (item.Type == ContactType.Telegram || item.Type == ContactType.Twitter)
                {
                    if(!item.Value.StartsWith("https://"))
                        if(!Regex.IsMatch(item.Value, "@[a-z0-9_-]+"))
                            throw new ValidationException(nameof(Page.Facts), $"Контакт #{i}: значение должно быть ссылкой, либо начинаться с @");
                }
                else if (!item.Value.StartsWith("https://"))
                    throw new ValidationException(nameof(Page.Facts), $"Контакт #{i}: ссылка должна начинаться с 'https://'");
            }
        }
    }

    /// <summary>
    /// Single social profile link.
    /// </summary>
    public class ContactFactItem
    {
        /// <summary>
        /// Type of the profile.
        /// </summary>
        public ContactType Type { get; set; }

        /// <summary>
        /// Link to the profile.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Returns the public hyperlink to this contact.
        /// </summary>
        public string GetLink()
        {
            return Type switch
            {
                ContactType.Phone => "tel:" + Value,
                ContactType.Email => "mailto:" + Value,
                ContactType.Telegram => Value.StartsWith('@') ? "https://t.me/" + Value.Substring(1) : Value,
                ContactType.Twitter => Value.StartsWith('@') ? "https://twitter.com/" + Value.Substring(1) : Value,
                _ => Value
            };
        }
    }

    /// <summary>
    /// Known social profile links (to display icons properly).
    /// </summary>
    public enum ContactType
    {
        Facebook,
        Twitter,
        Odnoklassniki,
        Vkontakte,
        Telegram,
        Youtube,
        Github,
        Email,
        Phone,
    }
}
