using System.Linq;
using System.Text.RegularExpressions;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    internal class DomainName
    {
        private readonly string[] _subdomains;

        public DomainName(string domainName)
        {
            var validDomainName = ReplaceInvalidCharacters(domainName);
            _subdomains = validDomainName.Split('.');
        }

        public DomainName(string productName = "product-name",
            string companyName = "company-name",
            string topLevel = "com")
        {
            var validTopLevel = ReplaceInvalidCharacters(topLevel);
            var validCompanyName = ReplaceInvalidCharacters(companyName);
            var validProductName = ReplaceInvalidCharacters(productName);
            _subdomains = new[] {validProductName, validCompanyName, validTopLevel};
        }

        private static string ReplaceInvalidCharacters(string input)
        {
            return Regex.Replace(ToLower(input), "[^a-z0-9-_.]", "-");
        }

        private static string ToLower(string input)
        {
            return Regex.Replace(input, "((?<=[a-z])[A-Z]|(?<=[A-Z])[A-Z](?=[a-z]))", ToLower)
                .ToLowerInvariant();
        }

        private static string ToLower(Match match)
        {
            return $"-{match.Value.ToLowerInvariant()}";
        }

        public string Reverse()
        {
            return Join(_subdomains.Reverse().ToArray());
        }

        public override string ToString()
        {
            return Join(_subdomains);
        }

        private static string Join(string[] value)
        {
            return string.Join(".", value);
        }
    }
}