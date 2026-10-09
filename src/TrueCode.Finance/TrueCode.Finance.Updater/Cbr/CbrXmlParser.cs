using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using TrueCode.Finance.Application.Commands.ImportCurrencyRates;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Updater.Cbr;

internal static class CbrXmlParser
{
    private const string RootName = "ValCurs";
    private const string DateFormat = "dd.MM.yyyy";

    public static Result<IReadOnlyList<ImportCurrencyRateItem>> Parse(Stream stream)
    {
        var result = new Result<IReadOnlyList<ImportCurrencyRateItem>> { Data = [] };

        XDocument document;
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 2_000_000,
            };
            using var reader = XmlReader.Create(stream, settings);
            document = XDocument.Load(reader);
        }
        catch (XmlException ex)
        {
            return result.AddError($"Некорректный XML: {ex.Message}");
        }

        var root = document.Root;
        if (root == null || root.Name.LocalName != RootName)
            return result.AddError($"Корневой элемент должен быть {RootName}.");

        if (!DateOnly.TryParseExact((string?)root.Attribute("Date"), DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
            return result.AddError($"Атрибут Date отсутствует или не соответствует формату {DateFormat}.");

        var items = new List<ImportCurrencyRateItem>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var valute in root.Elements("Valute"))
        {
            var id = (string?)valute.Attribute("ID") ?? "?";
            var code = Text(valute, "CharCode");
            var name = Text(valute, "Name");

            if (code == null || name == null)
            {
                result.AddError($"Currency {id}: отсутствует CharCode или Name.");
                continue;
            }

            if (!int.TryParse(Text(valute, "Nominal"), NumberStyles.None, CultureInfo.InvariantCulture, out var nominal)
                || nominal <= 0)
            {
                result.AddError($"{code}: Nominal отсутствует или не является положительным целым числом.");
                continue;
            }

            if (!TryParseDecimal(Text(valute, "Value"), out var value) || value <= 0)
            {
                result.AddError($"{code}: Value отсутствует или не является положительным числом.");
                continue;
            }

            if (!seenCodes.Add(code))
            {
                result.AddError($"{code}: валюта встречается в ответе более одного раза.");
                continue;
            }

            items.Add(new ImportCurrencyRateItem(code, name, value / nominal));
        }

        if (items.Count == 0)
            result.AddError("В ответе нет ни одной корректной валюты.");

        result.Data = items;
        return result;
    }

    private static string? Text(XElement parent, string name)
    {
        var value = parent.Element(name)?.Value.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static bool TryParseDecimal(string? text, out decimal value) =>
        decimal.TryParse(text?.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
}
