using System.Globalization;
using System.Text;

namespace SocialcastBot
{
    public class ApiResponse
    {
        public ApiResponse(bool isValid, ApiResponseFormat? apiResponseFormat, string content)
        {
            IsValid = isValid;
            ApiResponseFormat = apiResponseFormat;
            Content = content;
        }

        public bool IsValid { get; }
        public ApiResponseFormat? ApiResponseFormat { get; }
        public string Content { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", nameof(IsValid), IsValid));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", nameof(ApiResponseFormat), ApiResponseFormat));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", nameof(Content), Content));

            return stringBuilder.ToString();
        }
    }
}
